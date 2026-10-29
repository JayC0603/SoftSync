using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SoftSync.BLL.Services;
using SoftSync.DAL.Data;
using SoftSync.Presentation.Services;
using Xunit;
using Review = SoftSync.Presentation.Services.CvReviewService;
using ReviewException = SoftSync.Presentation.Services.CvReviewException;

namespace SoftSync.Tests;

public sealed class CvExtractionPipelineTests
{
    private const string Native = "Developer with React experience building accessible websites and REST API integrations.";

    [Fact]
    public async Task Native_PDF_never_calls_OCR_and_mixed_PDF_only_requests_scanned_pages()
    {
        var fake = new FakeOcr();
        var reader = new CvDocumentExtractionService(fake);
        var native = await reader.ExtractAsync(new MemoryStream(Pdf(Native)), "cv.pdf");
        Assert.Empty(fake.Requests);
        Assert.Contains(Native, native.Text);
        var mixed = await reader.ExtractAsync(new MemoryStream(Pdf(Native, "")), "cv.pdf");
        Assert.Equal(new[] { 2 }, Assert.Single(fake.Requests));
        Assert.Contains(Native, mixed.Text);
        Assert.Contains("OCR Vietnamese experience", mixed.Text);
        Assert.Equal("text", mixed.Pages[0].Method);
        Assert.Equal("ocr", mixed.Pages[1].Method);
        Assert.True(mixed.Pages[1].NeedsReview);
        Assert.True(mixed.Text.IndexOf(Native) < mixed.Text.IndexOf("OCR Vietnamese"));
    }

    [Fact]
    public async Task Missing_OCR_empty_pages_cancellation_and_limits_do_not_return_partial_success()
    {
        var missing = await Assert.ThrowsAsync<ReviewException>(() => new CvDocumentExtractionService()
            .ExtractAsync(new MemoryStream(Pdf("")), "cv.pdf"));
        Assert.Equal("ocr_unavailable", missing.Code);
        await Assert.ThrowsAsync<ReviewException>(() => new CvDocumentExtractionService(new FakeOcr { Empty = true })
            .ExtractAsync(new MemoryStream(Pdf(Native, "")), "cv.pdf"));
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new CvDocumentExtractionService(new FakeOcr())
            .ExtractAsync(new MemoryStream(Pdf("")), "cv.pdf", cancellationToken: cancelled.Token));
        await Assert.ThrowsAsync<ReviewException>(() => new CvDocumentExtractionService()
            .ExtractAsync(new MemoryStream(Encoding.UTF8.GetBytes(new string('a', 40_001))), "cv.txt"));
        await Assert.ThrowsAsync<ReviewException>(() => new CvDocumentExtractionService(new FakeOcr())
            .ExtractAsync(new MemoryStream(Pdf(Enumerable.Repeat("", 11).ToArray())), "cv.pdf"));
    }

    [Fact]
    public async Task Extracted_text_and_metadata_are_reused_after_reopening_without_OCR()
    {
        var fake = new FakeOcr();
        var extracted = await new CvDocumentExtractionService(fake).ExtractAsync(new MemoryStream(Pdf("")), "cv.pdf");
        var options = new DbContextOptionsBuilder<SoftSyncDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var metadata = JsonSerializer.Serialize(new { extracted.Pages, extracted.Version, extracted.ElapsedMilliseconds });
        await using (var db = new SoftSyncDbContext(options))
            await new CvWorkspaceService(db).AddDocumentAsync(1, "cv.pdf", "pdf", extracted.Text, Pdf(""), metadata);
        await using (var db = new SoftSyncDbContext(options))
        {
            var stored = Assert.Single(await new CvWorkspaceService(db).ListDocumentsAsync(1));
            Assert.Equal(extracted.Text, stored.Text);
            Assert.Equal(metadata, stored.ExtractionJson);
            Assert.Empty(await new CvWorkspaceService(db).ListDocumentsAsync(2));
        }
        Assert.Single(fake.Requests);
    }

    [Theory]
    [InlineData("{\"pages\":[{\"page\":2,\"text\":\"read\",\"confidence\":0.9}]}")]
    [InlineData("{\"pages\":[{\"page\":1,\"text\":\"read\",\"confidence\":9}]}")]
    [InlineData("{\"pages\":null}")]
    public async Task OCR_HTTP_adapter_rejects_invalid_page_mapping_and_confidence(string response)
    {
        var handler = new OcrHandler(response);
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["CvOcr:BaseUrl"] = "http://ocr.local/", ["CvOcr:ApiKey"] = "synthetic-key" }).Build();
        var client = new CvPageOcrClient(new ClientFactory(handler), config);
        await Assert.ThrowsAsync<ReviewException>(() => client.ReadAsync(Pdf(""), [1], CancellationToken.None));
        Assert.Equal("Bearer synthetic-key", handler.Authorization);
        Assert.Equal("http://ocr.local/ocr", handler.Url);
    }

    [Fact]
    public void Feedback_rejects_invented_quotes_requirements_and_empty_JD_comparison()
    {
        var valid = ValidResponse();
        Assert.True(Review.IsValid(valid, Native, "React and TypeScript required"));
        valid.Actions[0].Evidence = "Invented work at a famous company";
        Assert.False(Review.IsValid(valid, Native, "React and TypeScript required"));
        valid = ValidResponse(); valid.JobMatch!.Requirements.Clear();
        Assert.False(Review.IsValid(valid, Native, "React and TypeScript required"));
        valid = ValidResponse(); valid.JobMatch!.Requirements[0].Requirement = "Kubernetes";
        Assert.False(Review.IsValid(valid, Native, "React and TypeScript required"));
        valid = ValidResponse(); valid.JobMatch!.Requirements[0].Evidence = "Invented skill";
        Assert.False(Review.IsValid(valid, Native, "React and TypeScript required"));
        valid = ValidResponse(); valid.JobMatch = null;
        Assert.True(Review.IsValid(valid, Native, null));
    }

    [Fact]
    public async Task Vercel_binding_takes_precedence_and_oversized_scan_is_rejected_before_upload()
    {
        var handler = new OcrHandler("{\"pages\":[{\"page\":1,\"text\":\"OCR text\",\"confidence\":0.9}]}");
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["CV_OCR_SERVICE_URL"] = "http://private-ocr.local/", ["CvOcr:BaseUrl"] = "http://old-ocr.local/" }).Build();
        var client = new CvPageOcrClient(new ClientFactory(handler), config);
        var response = await client.ReadAsync(Pdf(""), [1], CancellationToken.None);
        Assert.Equal("http://private-ocr.local/ocr", handler.Url);
        Assert.Equal("OCR text", response[1].Text);
        var error = await Assert.ThrowsAsync<ReviewException>(() => client.ReadAsync(new byte[3 * 1024 * 1024 + 1], [1], CancellationToken.None));
        Assert.Equal("ocr_file_too_large", error.Code);
    }

    private static CvReviewText Text() => new() { Vi = "Nhận xét cụ thể", En = "Specific feedback" };
    private static CvReviewAiResponse ValidResponse() => new()
    {
        Summary = Text(), Strengths = [Text()], Improvements = [Text()],
        Criteria = new[] { "content", "clarity", "achievements", "structure" }.Select(id => new CvReviewCriterion { Id = id, Score = 70, Feedback = Text() }).ToList(),
        Actions = [new() { Priority = "High", Section = Text(), Evidence = "React experience", Issue = Text(), Action = Text() }],
        JobMatch = new() { Score = 70, Summary = Text(), Requirements = [new() { Requirement = "React", Status = "Met", Evidence = "React experience", Recommendation = Text() }] }
    };

    private sealed class FakeOcr : ICvPageOcr
    {
        public bool Empty { get; init; }
        public List<int[]> Requests { get; } = [];
        public Task<IReadOnlyDictionary<int, CvOcrPage>> ReadAsync(byte[] pdf, int[] pages, CancellationToken token)
        {
            token.ThrowIfCancellationRequested(); Requests.Add(pages);
            return Task.FromResult<IReadOnlyDictionary<int, CvOcrPage>>(pages.ToDictionary(p => p,
                p => new CvOcrPage(p, Empty ? "" : "OCR Vietnamese experience and project evidence", .8)));
        }
    }
    private sealed class OcrHandler(string json) : HttpMessageHandler
    {
        public string? Authorization, Url;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Authorization = request.Headers.Authorization?.ToString(); Url = request.RequestUri?.ToString();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") });
        }
    }
    private sealed class ClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    { public HttpClient CreateClient(string name) => new(handler, false); }

    private static byte[] Pdf(params string[] pages)
    {
        var objects = new List<string> { "<< /Type /Catalog /Pages 2 0 R >>", "", "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>" };
        var ids = new List<int>();
        foreach (var text in pages)
        {
            int pageId = objects.Count + 1; ids.Add(pageId);
            objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 3 0 R >> >> /Contents {pageId + 1} 0 R >>");
            var content = $"BT /F1 12 Tf 50 750 Td ({text}) Tj ET";
            objects.Add($"<< /Length {content.Length} >>\nstream\n{content}\nendstream");
        }
        objects[1] = $"<< /Type /Pages /Kids [{string.Join(" ", ids.Select(id => $"{id} 0 R"))}] /Count {pages.Length} >>";
        var source = new StringBuilder("%PDF-1.4\n"); var offsets = new List<int>();
        for (int i = 0; i < objects.Count; i++) { offsets.Add(source.Length); source.Append($"{i + 1} 0 obj\n{objects[i]}\nendobj\n"); }
        var xref = source.Length; source.Append($"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n");
        foreach (var offset in offsets) source.Append($"{offset:D10} 00000 n \n");
        source.Append($"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
        return Encoding.ASCII.GetBytes(source.ToString());
    }
}
