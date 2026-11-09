using System.Diagnostics;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

namespace SoftSync.Presentation.Services;

public sealed record CvExtractionPage(int Page, string Method, double? Confidence, bool NeedsReview);
public sealed record CvExtractionResult(string Text, IReadOnlyList<CvExtractionPage> Pages, long ElapsedMilliseconds, int Version = 1);
public interface ICvPageOcr
{
    Task<IReadOnlyDictionary<int, CvOcrPage>> ReadAsync(byte[] pdf, int[] pages, CancellationToken cancellationToken);
}
public sealed record CvOcrPage(int Page, string Text, double Confidence);

public sealed class CvDocumentExtractionService(ICvPageOcr? ocr = null, ILogger<CvDocumentExtractionService>? logger = null)
{
    public async Task<CvExtractionResult> ExtractAsync(Stream stream, string filename,
        Action<string>? progress = null, CancellationToken cancellationToken = default)
    {
        var timer = Stopwatch.StartNew();
        var extension = Path.GetExtension(filename).ToLowerInvariant();
        if (extension is not (".pdf" or ".txt"))
            throw Error("unsupported_file", "Chỉ hỗ trợ tệp PDF hoặc TXT.", "Only PDF and TXT files are supported.");
        using var buffer = new MemoryStream();
        var chunk = new byte[8192];
        int read;
        while ((read = await stream.ReadAsync(chunk, cancellationToken)) > 0)
        {
            if (buffer.Length + read > CvReviewDocumentReader.MaxFileBytes)
                throw Error("file_too_large", "Mỗi tệp không được vượt quá 10 MB.", "Each file must be no larger than 10 MB.");
            buffer.Write(chunk, 0, read);
        }
        var bytes = buffer.ToArray();
        if (extension == ".txt")
        {
            try
            {
                var text = new UTF8Encoding(false, true).GetString(bytes).TrimStart('\uFEFF');
                if (text.Contains('\0')) throw new DecoderFallbackException();
                ValidateLength(text.Length);
                if (string.IsNullOrWhiteSpace(text)) throw Error("empty_text", "Tệp không có văn bản.", "The file has no text.");
                logger?.LogInformation("CV stage text-extraction completed in {ElapsedMilliseconds} ms; pages 1; OCR pages 0", timer.ElapsedMilliseconds);
                return new(text, [new(1, "text", null, false)], timer.ElapsedMilliseconds);
            }
            catch (DecoderFallbackException)
            { throw Error("invalid_text", "Tệp TXT cần được lưu bằng mã hóa UTF-8.", "The TXT file must use UTF-8 encoding."); }
        }
        var texts = new List<string>();
        var metadata = new List<CvExtractionPage>();
        var scanPages = new List<int>();
        try
        {
            using var pdf = PdfDocument.Open(bytes);
            if (pdf.NumberOfPages > 40)
                throw Error("too_many_pages", "Tệp PDF không được vượt quá 40 trang.", "The PDF must contain no more than 40 pages.");
            foreach (var page in pdf.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();
                progress?.Invoke($"read:{page.Number}:{pdf.NumberOfPages}");
                var words = page.GetWords(NearestNeighbourWordExtractor.Instance);
                var blocks = DocstrumBoundingBoxes.Instance.GetBlocks(words);
                var text = string.Join("\n\n", UnsupervisedReadingOrderDetector.Instance.Get(blocks).Select(b => b.Text));
                texts.Add(text);
                ValidateLength(texts.Sum(t => t.Length));
                // A short footer on a scanned page is not a usable text layer.
                var needsOcr = text.Count(char.IsLetterOrDigit) < 30
                    || text.Count(c => c == '\uFFFD') > Math.Max(2, text.Length / 100);
                metadata.Add(new(page.Number, "text", null, false));
                if (needsOcr) scanPages.Add(page.Number);
            }
        }
        catch (Exception ex) when (ex is not (CvReviewException or OperationCanceledException))
        { throw Error("invalid_pdf", "Không đọc được PDF. Hãy kiểm tra tệp có bị lỗi hoặc đặt mật khẩu không.", "The PDF could not be read. Check whether it is damaged or password-protected."); }
        logger?.LogInformation("CV stage text-extraction completed in {ElapsedMilliseconds} ms; pages {PageCount}; OCR pages {OcrPageCount}", timer.ElapsedMilliseconds, texts.Count, scanPages.Count);
        if (scanPages.Count > 0)
        {
            if (ocr is null) throw Error("ocr_unavailable", "CV có trang cần OCR. Dịch vụ OCR chưa được cấu hình; hãy dùng PDF có chữ hoặc TXT.", "Some pages need OCR. Configure OCR or use a text-selectable PDF/TXT.");
            if (scanPages.Count > 10) throw Error("ocr_page_limit", "Mỗi lần hỗ trợ tối đa 10 trang cần OCR. Hãy chia nhỏ tài liệu.", "Split the document into at most 10 OCR pages per upload.");
            progress?.Invoke($"ocr:{scanPages.Count}");
            var ocrTimer = Stopwatch.StartNew();
            var pages = await ocr.ReadAsync(bytes, scanPages.ToArray(), cancellationToken);
            logger?.LogInformation("CV stage OCR completed in {ElapsedMilliseconds} ms; pages {PageCount}", ocrTimer.ElapsedMilliseconds, scanPages.Count);
            foreach (var number in scanPages)
            {
                if (!pages.TryGetValue(number, out var page) || string.IsNullOrWhiteSpace(page.Text))
                    throw Error("ocr_empty", "OCR chưa đọc được đầy đủ các trang. Hãy dùng bản scan rõ hơn hoặc TXT.", "OCR could not read every page. Use a clearer scan or TXT.");
                texts[number - 1] = page.Text;
                metadata[number - 1] = new(number, "ocr", page.Confidence, page.Confidence < .85);
            }
        }
        var combined = string.Join("\n\n", texts.Select((text, i) => $"[Page {i + 1}]\n{text.Trim()}"));
        ValidateLength(combined.Length);
        if (texts.All(string.IsNullOrWhiteSpace)) throw Error("pdf_no_text", "Không đọc được văn bản từ PDF.", "No text could be read from the PDF.");
        return new(combined, metadata, timer.ElapsedMilliseconds);
    }
    private static void ValidateLength(int length)
    {
        if (length > CvReviewService.MaxCvCharacters)
            throw Error("document_too_long", "Nội dung tệp vượt quá 40.000 ký tự.", "Document text exceeds 40,000 characters.");
    }
    private static CvReviewException Error(string code, string vi, string en) => new(code, vi, en);
}
