using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using SoftSync.Presentation.Services;
using Xunit;
using Review = SoftSync.Presentation.Services.CvReviewService;

namespace SoftSync.Tests;

public sealed class CvReviewReliabilityTests
{
    [Fact]
    public async Task Recognized_enum_casing_and_PDF_typography_do_not_trigger_an_AI_retry()
    {
        const string cv = XahCvReviewTests.Cv + "\nBuilt Office ﬁltering for the API’s product catalog.\nTiếng Việt: Nguyê\u0303n và kỹ năng giao tiếp.";
        var model = new StubModel((_, _) =>
        {
            var response = XahCvReviewTests.Response();
            response.Criteria[0].Id = " CONTENT "; response.Actions[0].Priority = "high";
            response.Actions[0].Evidence = "Office filtering for the API's product catalog.";
            response.JobMatch!.Requirements[0].Status = "met";
            response.JobMatch.Requirements[0].Importance = "required";
            return Task.FromResult(response);
        });
        using var service = new Review(model);
        var review = await service.ReviewAsync(new(cv, XahCvReviewTests.Jd, "vi"));
        Assert.Equal(1, model.Calls);
        Assert.Equal("content", review.Criteria[0].Id);
        Assert.Equal("High", review.Actions[0].Priority);
        Assert.Equal("Met", review.JobMatch!.Requirements[0].Status);
    }

    [Fact]
    public async Task JD_correction_receives_exact_field_paths_and_original_rows_without_overwriting_valid_sections()
    {
        var model = new StubModel((call, _) =>
        {
            var response = XahCvReviewTests.Response();
            if (call == 1) { response.JobMatch!.Requirements[0].Evidence = "Invented private phrase"; return Task.FromResult(response); }
            return Task.FromResult(new CvReviewAiResponse { JobMatch = response.JobMatch });
        });
        using var service = new Review(model);
        var result = await service.ReviewAsync(new(XahCvReviewTests.Cv, XahCvReviewTests.Jd, "vi"));
        Assert.Equal(2, model.Calls);
        Assert.NotEmpty(result.Criteria);
        var input = JsonSerializer.SerializeToElement(model.LastInput);
        Assert.Equal("jobMatch", Assert.Single(input.GetProperty("repairSections").EnumerateArray()).GetString());
        Assert.StartsWith("jobMatch.requirements[0].evidence:", Assert.Single(input.GetProperty("validationIssuesToCorrect").EnumerateArray()).GetString());
        var previous = Assert.Single(input.GetProperty("previousSections").EnumerateObject());
        Assert.Equal("jobMatch", previous.Name);
        Assert.Equal("React", previous.Value.GetProperty("requirements")[0].GetProperty("requirement").GetString());
        Assert.Contains("Invented private phrase", previous.Value.GetRawText());
        Assert.DoesNotContain("Invented private phrase", input.GetProperty("validationIssuesToCorrect").GetRawText());
    }

    [Fact]
    public async Task Generation_and_correction_share_one_total_deadline_and_release_the_gate_on_timeout()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["CvLlm:TotalTimeoutSeconds"] = "1" }).Build();
        var model = new StubModel(async (call, token) =>
        {
            await Task.Delay(call == 1 ? 600 : Timeout.Infinite, token);
            var response = XahCvReviewTests.Response(); response.Criteria.Clear(); return response;
        });
        using var service = new Review(model, configuration: config);
        var timer = Stopwatch.StartNew();
        var failure = await Assert.ThrowsAsync<CvReviewException>(() => service.ReviewAsync(new(XahCvReviewTests.Cv, XahCvReviewTests.Jd)));
        Assert.Equal("ai_timeout", failure.Code);
        Assert.Equal(2, model.Calls);
        Assert.True(timer.Elapsed < TimeSpan.FromSeconds(4));
        model.Generate = (_, _) => Task.FromResult(XahCvReviewTests.Response());
        Assert.NotEmpty((await service.ReviewAsync(new(XahCvReviewTests.Cv, XahCvReviewTests.Jd))).Criteria);
        Assert.Equal(3, model.Calls); // A timeout is not cached and does not retain the semaphore.
    }

    [Fact]
    public async Task Numeric_strings_are_read_as_numbers_without_accepting_invented_evidence()
    {
        var handler = new JsonHandler();
        using var service = new Review(XahCvReviewTests.Client(handler));
        var result = await service.ReviewAsync(new(XahCvReviewTests.Cv, XahCvReviewTests.Jd));
        Assert.Equal(1, handler.Calls);
        Assert.Equal(70, result.QualityScore);
        Assert.Equal(50, result.JobMatch!.Score);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("dungcsnd113/gemini-flash-3.8")]
    [InlineData("my/custom-model")]
    public void Retired_default_migrates_to_Gemini_38_and_explicit_custom_model_is_preserved(string? configured)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["CvLlm:Model"] = configured }).Build();
        var client = new XahCvReviewClient(new Factory(new JsonHandler()), config, Microsoft.Extensions.Logging.Abstractions.NullLogger<XahCvReviewClient>.Instance);
        Assert.Equal(configured == "my/custom-model" ? configured : "cuzem3/gemini-3.8-flash-low", client.Model);
    }

    private sealed class StubModel(Func<int, CancellationToken, Task<CvReviewAiResponse>> generate) : ICvReviewLanguageModel
    {
        public string Model => "synthetic-model";
        public int Calls;
        public object? LastInput;
        public Func<int, CancellationToken, Task<CvReviewAiResponse>> Generate = generate;
        public Task<CvReviewAiResponse> AnalyzeAsync(string prompt, object input, CancellationToken token)
        { LastInput = input; return Generate(++Calls, token); }
    }

    private sealed class JsonHandler : HttpMessageHandler
    {
        public int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Calls++;
            var response = JsonSerializer.SerializeToNode(XahCvReviewTests.Response(), new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
            foreach (var criterion in response["criteria"]!.AsArray()) criterion!["score"] = "70";
            response["jobMatch"]!["score"] = "0";
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new { choices = new[] { new { finish_reason = "stop", message = new { content = response.ToJsonString() } } } }))
            });
        }
    }
    private sealed class Factory(HttpMessageHandler handler) : IHttpClientFactory
    { public HttpClient CreateClient(string name) => new(handler, false); }
}
