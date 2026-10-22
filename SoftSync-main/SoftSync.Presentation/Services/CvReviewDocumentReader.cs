using System.Text;
using UglyToad.PdfPig;

namespace SoftSync.Presentation.Services;

public static class CvReviewDocumentReader
{
    public const int MaxFileBytes = 10 * 1024 * 1024;
    public static async Task<string> ReadAsync(Stream stream, string filename, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(filename).ToLowerInvariant();
        if (extension is not (".pdf" or ".txt"))
            throw new CvReviewException("unsupported_file", "Chỉ hỗ trợ tệp PDF hoặc TXT.", "Only PDF and TXT files are supported.");
        using var buffer = new MemoryStream();
        var chunk = new byte[8192];
        int read;
        while ((read = await stream.ReadAsync(chunk, cancellationToken)) > 0)
        {
            if (buffer.Length + read > MaxFileBytes)
                throw new CvReviewException("file_too_large", "Mỗi tệp không được vượt quá 10 MB.", "Each file must be no larger than 10 MB.");
            buffer.Write(chunk, 0, read);
        }
        var bytes = buffer.ToArray();
        if (extension == ".txt")
        {
            try
            {
                var text = new UTF8Encoding(false, true).GetString(bytes).TrimStart('\uFEFF');
                if (text.Contains('\0')) throw new DecoderFallbackException();
                return text;
            }
            catch (DecoderFallbackException)
            { throw new CvReviewException("invalid_text", "Tệp TXT cần được lưu bằng mã hóa UTF-8.", "The TXT file must use UTF-8 encoding."); }
        }
        try
        {
            using var pdf = PdfDocument.Open(bytes);
            if (pdf.NumberOfPages > 40)
                throw new CvReviewException("too_many_pages", "Tệp PDF không được vượt quá 40 trang.", "The PDF must contain no more than 40 pages.");
            var text = new StringBuilder();
            foreach (var page in pdf.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();
                text.AppendLine(page.Text);
                if (text.Length > CvReviewService.MaxCvCharacters)
                    throw new CvReviewException("document_too_long", "Nội dung tệp quá dài để đánh giá trong một lần.", "The document is too long to review in one request.");
            }
            if (string.IsNullOrWhiteSpace(text.ToString()))
                throw new CvReviewException("pdf_no_text", "Không đọc được văn bản từ PDF. Hãy dùng PDF có thể chọn chữ hoặc tệp TXT.", "No text could be read from the PDF. Use a text-selectable PDF or a TXT file.");
            return text.ToString();
        }
        catch (Exception ex) when (ex is not (CvReviewException or OperationCanceledException))
        { throw new CvReviewException("invalid_pdf", "Không đọc được PDF. Hãy kiểm tra tệp có bị lỗi hoặc đặt mật khẩu không.", "The PDF could not be read. Check whether it is damaged or password-protected."); }
    }
}
