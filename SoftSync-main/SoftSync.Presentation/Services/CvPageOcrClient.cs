using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SoftSync.Presentation.Services;

public sealed class CvPageOcrClient(IHttpClientFactory clients, IConfiguration configuration) : ICvPageOcr
{
    public async Task<IReadOnlyDictionary<int, CvOcrPage>> ReadAsync(byte[] pdf, int[] pages, CancellationToken cancellationToken)
    {
        var binding = configuration["CV_OCR_SERVICE_URL"];
        var endpoint = string.IsNullOrWhiteSpace(binding) ? configuration["CvOcr:BaseUrl"] : binding;
        // Base64 plus JSON must stay below the Vercel Function request limit.
        if (!string.IsNullOrWhiteSpace(binding) && pdf.Length > 3 * 1024 * 1024)
            throw new CvReviewException("ocr_file_too_large", "PDF scan trên Vercel tối đa 3 MB. Hãy giảm dung lượng hoặc dùng PDF có chữ/TXT.", "Scanned PDFs on Vercel must be at most 3 MB. Reduce the file size or use a text PDF/TXT.");
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var origin) || origin.Scheme is not ("http" or "https"))
            throw Failure("ocr_unavailable");
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(configuration.GetValue("CvOcr:TimeoutSeconds", 90), 1, 120)));
        try
        {
            using var message = new HttpRequestMessage(HttpMethod.Post, new Uri(origin.ToString().TrimEnd('/') + "/ocr"));
            var key = configuration["CvOcr:ApiKey"];
            if (!string.IsNullOrWhiteSpace(key)) message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
            message.Content = JsonContent.Create(new { pdf = Convert.ToBase64String(pdf), pages });
            using var response = await clients.CreateClient("CvOcr").SendAsync(message, HttpCompletionOption.ResponseHeadersRead, deadline.Token);
            if (!response.IsSuccessStatusCode) throw Failure("ocr_unavailable");
            await response.Content.LoadIntoBufferAsync(512_000, deadline.Token);
            var payload = await response.Content.ReadFromJsonAsync<OcrResponse>(deadline.Token);
            if (payload?.Pages is null || payload.Pages.Count != pages.Length
                || payload.Pages.Any(p => p is null || !pages.Contains(p.Page) || p.Text is null || p.Text.Length > 40_000
                    || !double.IsFinite(p.Confidence) || p.Confidence is < 0 or > 1)
                || payload.Pages.Select(p => p.Page).Distinct().Count() != pages.Length)
                throw Failure("ocr_invalid_response");
            return payload.Pages.ToDictionary(p => p.Page);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) { throw Failure("ocr_timeout"); }
        catch (Exception ex) when (ex is HttpRequestException or System.Text.Json.JsonException) { throw Failure("ocr_unavailable"); }
    }
    private static CvReviewException Failure(string code) => new(code,
        "Chưa thể đọc CV bằng OCR. Vui lòng thử lại hoặc dùng PDF có chữ/TXT.",
        "OCR could not read this CV. Retry or use a text-selectable PDF/TXT.");
    private sealed record OcrResponse(List<CvOcrPage> Pages);
}
