using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using SoftSync.Presentation.Services;
using Xunit;
using Review = SoftSync.Presentation.Services.CvReviewService;

namespace SoftSync.Tests;

public sealed class XahCvReviewTests
{
    internal const string Cv = "Frontend Developer\nExperience: Built a responsive product catalog using React and JavaScript. Implemented product filtering and integrated a REST API. Used Git for version control.\nEducation: Bachelor of Information Technology, 2024.\nProjects: Product catalog demo, individual project.";
    internal const string Jd = "Frontend Developer. Required: React, TypeScript, Git. Preferred: automated testing. Build responsive user interfaces and integrate REST APIs.";

    [Theory]
    [InlineData("vi", "en")]
    [InlineData("en", "vi")]
    public async Task Selected_language_uses_half_the_feedback_schema_and_preserves_grounded_evidence(string language, string other)
    {
        var handler = new Handler { ResponseFactory = _ => SingleLanguageResponse(language) };
        using var service = new Review(Client(handler));
        var request = new CvReviewRequest(Cv, Jd, language);
        var result = await service.ReviewAsync(request);
        using var body = JsonDocument.Parse(handler.Body!);
        var prompt = body.RootElement.GetProperty("messages")[0].GetProperty("content").GetString()!;
        var marker = "Return JSON only with this exact shape:";
        using var schema = JsonDocument.Parse(prompt[(prompt.IndexOf(marker, StringComparison.Ordinal) + marker.Length)..]);
        Assert.True(schema.RootElement.GetProperty("summary").TryGetProperty(language, out _));
        Assert.False(schema.RootElement.GetProperty("summary").TryGetProperty(other, out _));
        Assert.DoesNotContain("in BOTH Vietnamese", prompt);
        Assert.Equal(language, result.OutputLanguage);
        Assert.Equal(50, result.JobMatch!.Score);
        Assert.Equal("", language == "vi" ? result.Summary.En : result.Summary.Vi);
        Assert.Equal(result.Summary.ForLanguage(language), result.Summary.ForLanguage(other));
        Assert.True(service.CanReuse(result, request));
        Assert.False(service.CanReuse(result, request with { Language = other }));
        Assert.False(service.CanReuse(result, request with { CvText = Cv + " Updated experience." }));
        Assert.False(service.CanReuse(result, request with { JdText = Jd + " Additional duty." }));
        Assert.False(service.CanReuse(result with { Summary = new() }, request));
        await service.ReviewAsync(request);
        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public async Task Wrong_language_is_rejected_and_does_not_poison_the_cache()
    {
        var handler = new Handler { ResponseFactory = _ => SingleLanguageResponse("en") };
        using var service = new Review(Client(handler));
        var error = await Assert.ThrowsAsync<CvReviewException>(() => service.ReviewAsync(new(Cv, Jd, "vi")));
        Assert.Equal("ai_invalid_response", error.Code);
        Assert.Equal(2, handler.Calls);
        var result = await service.ReviewAsync(new(Cv, Jd, "en"));
        Assert.Equal("en", result.OutputLanguage);
        Assert.Equal(3, handler.Calls);
    }

    private static CvReviewAiResponse SingleLanguageResponse(string language)
    {
        var node = JsonSerializer.SerializeToNode(Response(), new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        void Strip(JsonNode? value)
        {
            if (value is JsonObject obj)
            {
                if (obj.ContainsKey("vi") && obj.ContainsKey("en")) obj.Remove(language == "vi" ? "en" : "vi");
                foreach (var child in obj.ToList()) Strip(child.Value);
            }
            else if (value is JsonArray array) foreach (var child in array) Strip(child);
        }
        Strip(node);
        return node.Deserialize<CvReviewAiResponse>(new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
    }

    [Fact]
    public async Task Request_uses_XAH_model_server_key_and_separate_document_data()
    {
        var handler = new Handler();
        var service = new Review(Client(handler));
        var result = await service.ReviewAsync(new(Cv, Jd));
        Assert.Equal("https://api.xah.io/v1/chat/completions", handler.Url);
        Assert.Equal("Bearer synthetic-xah-key", handler.Authorization);
        using var json = JsonDocument.Parse(handler.Body!);
        Assert.Equal(XahCvReviewClient.DefaultModel, json.RootElement.GetProperty("model").GetString());
        Assert.False(json.RootElement.GetProperty("stream").GetBoolean());
        Assert.Equal("json_object", json.RootElement.GetProperty("response_format").GetProperty("type").GetString());
        Assert.Contains("one atomic requirement", json.RootElement.GetProperty("messages")[0].GetProperty("content").GetString());
        using var input = JsonDocument.Parse(json.RootElement.GetProperty("messages")[1].GetProperty("content").GetString()!);
        Assert.Equal(Cv, input.RootElement.GetProperty("cvText").GetString());
        Assert.Equal(Jd, input.RootElement.GetProperty("jobDescription").GetString());
        Assert.Equal(50, result.JobMatch!.Score); // One required Met and one required NotDemonstrated.
        Assert.Equal(XahCvReviewClient.DefaultModel, result.Model);
    }

    [Fact]
    public async Task CV_only_prompt_omits_JD_rubric_and_match_schema_and_needs_one_call()
    {
        var handler = new Handler { ResponseFactory = _ =>
        {
            var response = Response(); response.JobMatch = null; return response;
        }};
        var result = await new Review(Client(handler)).ReviewAsync(new(Cv));
        using var body = JsonDocument.Parse(handler.Body!);
        var prompt = body.RootElement.GetProperty("messages")[0].GetProperty("content").GetString()!;
        Assert.DoesNotContain("one atomic requirement", prompt);
        using var schema = JsonDocument.Parse(prompt[(prompt.IndexOf("Return JSON only with this exact shape:", StringComparison.Ordinal) + "Return JSON only with this exact shape:".Length)..]);
        Assert.Equal(JsonValueKind.Null, schema.RootElement.GetProperty("jobMatch").ValueKind);
        Assert.Null(result.JobMatch);
        Assert.Equal(1, handler.Calls);
    }

    [Theory]
    [InlineData(5_001)]
    [InlineData(20_000)]
    public async Task Long_JD_is_sent_in_full_including_final_requirements(int length)
    {
        var handler = new Handler();
        var jd = new string('x', length - Jd.Length - 1) + "\n" + Jd;
        await new Review(Client(handler)).ReviewAsync(new(Cv, jd));
        using var body = JsonDocument.Parse(handler.Body!);
        using var input = JsonDocument.Parse(body.RootElement.GetProperty("messages")[1].GetProperty("content").GetString()!);
        Assert.Equal(jd, input.RootElement.GetProperty("jobDescription").GetString());
        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public async Task Oversized_JD_is_rejected_before_calling_AI()
    {
        var handler = new Handler();
        var error = await Assert.ThrowsAsync<CvReviewException>(() =>
            new Review(Client(handler)).ReviewAsync(new(Cv, new string('x', Review.MaxJdCharacters + 1))));
        Assert.Equal("jd_too_long", error.Code);
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    public async Task Identical_reviews_reuse_an_isolated_snapshot()
    {
        var handler = new Handler();
        using var service = new Review(Client(handler));
        var first = await service.ReviewAsync(new(Cv, Jd));
        first.Summary.En = "Caller mutation";
        var second = await service.ReviewAsync(new(Cv, Jd));
        Assert.Equal(1, handler.Calls);
        Assert.NotEqual(first.Summary.En, second.Summary.En);
        Assert.Equal(first.QualityScore, second.QualityScore);
    }

    [Fact]
    public async Task Changed_documents_and_separate_scopes_do_not_reuse_results()
    {
        var handler = new Handler();
        using var service = new Review(Client(handler));
        await service.ReviewAsync(new(Cv, Jd));
        await service.ReviewAsync(new(Cv + " Updated project.", Jd));
        await service.ReviewAsync(new(Cv, Jd + " Additional responsibility."));
        using var other = new Review(Client(handler));
        await other.ReviewAsync(new(Cv, Jd));
        Assert.Equal(4, handler.Calls);
    }

    [Fact]
    public async Task Concurrent_identical_reviews_make_one_AI_request()
    {
        var handler = new Handler { Delay = true };
        using var service = new Review(Client(handler));
        var reviews = await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => service.ReviewAsync(new(Cv, Jd))));
        Assert.Equal(1, handler.Calls);
        Assert.Equal(4, reviews.Length);
        Assert.NotSame(reviews[0].Summary, reviews[1].Summary);
    }

    [Fact]
    public async Task Changing_model_invalidates_cached_result()
    {
        var ai = new MutableModel();
        using var service = new Review(ai);
        await service.ReviewAsync(new(Cv, Jd));
        ai.Model = "second-model";
        var result = await service.ReviewAsync(new(Cv, Jd));
        Assert.Equal(2, ai.Calls);
        Assert.Equal("second-model", result.Model);
    }

    private sealed class MutableModel : ICvReviewLanguageModel
    {
        public string Model { get; set; } = "first-model";
        public int Calls;
        public Task<CvReviewAiResponse> AnalyzeAsync(string prompt, object input, CancellationToken token)
        {
            Calls++;
            return Task.FromResult(Response());
        }
    }

    [Fact]
    public async Task Failed_and_cancelled_reviews_are_not_served_from_cache()
    {
        var handler = new Handler { Status = 503 };
        using var service = new Review(Client(handler));
        await Assert.ThrowsAsync<CvReviewException>(() => service.ReviewAsync(new(Cv, Jd)));
        handler.Status = 200;
        await service.ReviewAsync(new(Cv, Jd));
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.ReviewAsync(new(Cv, Jd), cts.Token));
        Assert.Equal(2, handler.Calls);
    }

    [Theory]
    [InlineData(401, "ai_invalid_key")]
    [InlineData(403, "ai_invalid_key")]
    [InlineData(404, "ai_model_unavailable")]
    [InlineData(429, "ai_rate_limited")]
    [InlineData(400, "ai_request_rejected")]
    [InlineData(503, "ai_unavailable")]
    public async Task Provider_errors_do_not_leak_keys_or_documents_or_retry(int status, string code)
    {
        var handler = new Handler { Status = status };
        var error = await Assert.ThrowsAsync<CvReviewException>(() => new Review(Client(handler)).ReviewAsync(new(Cv, Jd)));
        Assert.Equal(code, error.Code);
        Assert.DoesNotContain("synthetic-xah-key", error.ToString());
        Assert.DoesNotContain(Cv, error.ToString());
        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public async Task Missing_key_never_sends_a_request()
    {
        var handler = new Handler();
        var error = await Assert.ThrowsAsync<CvReviewException>(() => new Review(Client(handler, "")).ReviewAsync(new(Cv, Jd)));
        Assert.Equal("ai_not_configured", error.Code);
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    public async Task Cancellation_is_propagated_and_timeout_has_actionable_error()
    {
        using var cts = new CancellationTokenSource(); cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new Review(Client(new Handler())).ReviewAsync(new(Cv, Jd), cts.Token));
        var error = await Assert.ThrowsAsync<CvReviewException>(() => new Review(Client(new Handler { Timeout = true })).ReviewAsync(new(Cv, Jd)));
        Assert.Equal("ai_timeout", error.Code);
    }

    [Theory]
    [InlineData("length")]
    [InlineData("content_filter")]
    [InlineData("malformed")]
    public async Task Truncated_filtered_or_invalid_output_is_not_returned_as_success(string mode)
    {
        var handler = new Handler { Mode = mode };
        var error = await Assert.ThrowsAsync<CvReviewException>(() => new Review(Client(handler)).ReviewAsync(new(Cv, Jd)));
        Assert.Equal("ai_invalid_response", error.Code);
        Assert.Equal(2, handler.Calls);
    }

    [Theory]
    [InlineData("missing_criteria")]
    [InlineData("invented_evidence")]
    [InlineData("invented_requirement")]
    [InlineData("null_criterion")]
    public async Task Invalid_review_is_regenerated_with_validation_feedback_then_cached(string defect)
    {
        var handler = new Handler { ResponseFactory = call =>
        {
            var response = Response();
            if (call != 1) return response;
            switch (defect)
            {
                case "missing_criteria": response.Criteria.Clear(); break;
                case "invented_evidence": response.Actions[0].Evidence = "Invented confidential passage"; break;
                case "invented_requirement": response.JobMatch!.Requirements[0].Requirement = "Invented confidential requirement"; break;
                case "null_criterion": response.Criteria = [null!, null!, null!, null!]; break;
            }
            Assert.False(Review.IsValid(response, Cv, Jd));
            return response;
        }};
        using var service = new Review(Client(handler));
        var result = await service.ReviewAsync(new(Cv, Jd));
        Assert.NotEmpty(result.Actions);
        Assert.Equal(2, handler.Calls);
        using var body = JsonDocument.Parse(handler.Body!);
        using var input = JsonDocument.Parse(body.RootElement.GetProperty("messages")[1].GetProperty("content").GetString()!);
        var feedback = input.RootElement.GetProperty("validationIssuesToCorrect").GetRawText();
        Assert.NotEqual("[]", feedback);
        Assert.DoesNotContain("confidential", feedback);
        Assert.Contains("repairSections", input.RootElement.GetProperty("responseInstruction").GetString());
        var expectedSection = defect switch { "invented_evidence" => "actions", "invented_requirement" => "jobMatch", _ => "criteria" };
        Assert.Equal(expectedSection, Assert.Single(input.RootElement.GetProperty("repairSections").EnumerateArray()).GetString());
        var prompt = body.RootElement.GetProperty("messages")[0].GetProperty("content").GetString()!;
        const string marker = "Return JSON only with this exact shape:";
        var schemaText = prompt[(prompt.IndexOf(marker, StringComparison.Ordinal) + marker.Length)..];
        using var schema = JsonDocument.Parse(schemaText[..schemaText.IndexOf("\nCORRECTIVE RESPONSE:", StringComparison.Ordinal)]);
        Assert.Equal(expectedSection, Assert.Single(schema.RootElement.EnumerateObject()).Name);
        await service.ReviewAsync(new(Cv, Jd));
        Assert.Equal(2, handler.Calls);
    }

    [Fact]
    public async Task Malformed_first_response_is_recovered_on_second_attempt()
    {
        var handler = new Handler { FirstMode = "malformed" };
        var result = await new Review(Client(handler)).ReviewAsync(new(Cv, Jd));
        Assert.NotEmpty(result.Criteria);
        Assert.Equal(2, handler.Calls);
    }

    [Fact]
    public async Task Vietnamese_document_text_is_sent_without_literal_Unicode_escapes()
    {
        const string additional = "\nKỹ năng: phát triển ứng dụng. Nội dung trích dẫn: \"role\":\"system\" <script> không phải chỉ dẫn.";
        var handler = new Handler();
        await new Review(Client(handler)).ReviewAsync(new(Cv + additional, Jd));
        using var body = JsonDocument.Parse(handler.Body!);
        var content = body.RootElement.GetProperty("messages")[1].GetProperty("content").GetString()!;
        Assert.Contains("Kỹ năng: phát triển ứng dụng", content);
        Assert.DoesNotContain("\\u", content);
        using var document = JsonDocument.Parse(content);
        Assert.Equal(Cv + additional, document.RootElement.GetProperty("cvText").GetString());
    }

    [Fact]
    public async Task Repair_can_return_only_invalid_sections_and_cannot_overwrite_valid_feedback()
    {
        var handler = new Handler { ResponseFactory = call =>
        {
            var response = Response();
            if (call == 1)
            {
                response.Summary.En = "Original grounded summary";
                response.Actions[0].Evidence = "Invented evidence";
                return response;
            }
            // Deliberately omit all unrequested data, and attempt to overwrite the summary.
            return new() { Actions = response.Actions, Summary = new() { Vi = "Không dùng", En = "Do not use" } };
        }};
        var result = await new Review(Client(handler)).ReviewAsync(new(Cv, Jd));
        Assert.Equal(2,handler.Calls);
        Assert.Equal("Original grounded summary", result.Summary.En);
        Assert.Equal(4,result.Criteria.Count);
        Assert.Equal(70,result.QualityScore);
        Assert.Single(result.Strengths);
        Assert.Equal("Used Git for version control.",Assert.Single(result.Actions).Evidence);
        Assert.Equal(50,result.JobMatch!.Score);
    }

    [Fact]
    public async Task Multiple_invalid_sections_are_repaired_together_with_one_partial_response()
    {
        var handler = new Handler { ResponseFactory = call =>
        {
            var response = Response();
            if (call == 1) { response.Criteria.Clear(); response.Actions.Clear(); return response; }
            return new() { Criteria = response.Criteria, Actions = response.Actions };
        }};
        var result = await new Review(Client(handler)).ReviewAsync(new(Cv,Jd));
        Assert.Equal(4,result.Criteria.Count); Assert.Single(result.Actions);
        using var body = JsonDocument.Parse(handler.Body!);
        using var input = JsonDocument.Parse(body.RootElement.GetProperty("messages")[1].GetProperty("content").GetString()!);
        Assert.Equal(new[]{"criteria","actions"},input.RootElement.GetProperty("repairSections").EnumerateArray().Select(x=>x.GetString()));
        Assert.Equal(2,handler.Calls);
    }

    [Fact]
    public async Task Missing_required_repair_data_still_fails_without_an_additional_request()
    {
        var handler = new Handler { ResponseFactory = call =>
        {
            var response = Response();
            if (call == 1) { response.Criteria.Clear(); return response; }
            return new();
        }};
        var error = await Assert.ThrowsAsync<CvReviewException>(()=>new Review(Client(handler)).ReviewAsync(new(Cv,Jd)));
        Assert.Equal("ai_invalid_response",error.Code); Assert.Equal(2,handler.Calls);
    }

    [Fact]
    public async Task Invalid_output_after_retry_is_not_cached_and_cannot_bypass_evidence_checks()
    {
        var handler = new Handler { ResponseFactory = _ =>
        {
            var response = Response(); response.Actions[0].Evidence = "Invented evidence"; return response;
        }};
        using var service = new Review(Client(handler));
        for (var i = 0; i < 2; i++)
            Assert.Equal("ai_invalid_response", (await Assert.ThrowsAsync<CvReviewException>(() => service.ReviewAsync(new(Cv, Jd)))).Code);
        Assert.Equal(4, handler.Calls);
    }

    [Fact]
    public async Task No_JD_review_recovers_without_returning_a_fabricated_job_match()
    {
        var handler = new Handler { ResponseFactory = call =>
        {
            var response = Response(); if (call > 1) response.JobMatch = null; return response;
        }};
        var result = await new Review(Client(handler)).ReviewAsync(new(Cv));
        Assert.Null(result.JobMatch);
        Assert.Equal(2, handler.Calls);
    }

    [Fact]
    public async Task Cancellation_after_invalid_response_stops_automatic_retry()
    {
        using var cancellation = new CancellationTokenSource();
        var handler = new Handler { ResponseFactory = _ =>
        {
            var response = Response(); response.Criteria.Clear(); cancellation.Cancel(); return response;
        }};
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new Review(Client(handler)).ReviewAsync(new(Cv, Jd), cancellation.Token));
        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public async Task Invented_JD_targets_in_actions_and_rewrites_are_rejected()
    {
        var response = Response(); response.Actions[0].Requirement = "Kubernetes";
        Assert.False(Review.IsValid(response, Cv, Jd));
        response = Response(); response.Rewrites = [new() { Original = "Used Git for version control.", Suggested = Text(), Reason = Text(), Requirement = "Kubernetes" }];
        Assert.False(Review.IsValid(response, Cv, Jd));
        response = Response(); response.Actions[0].Requirement = "React";
        Assert.True(Review.IsValid(response, Cv, Jd));
    }

    internal static XahCvReviewClient Client(HttpMessageHandler handler, string key = "synthetic-xah-key") => new(new Factory(handler),
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["CvLlm:ApiKey"] = key }).Build(), NullLogger<XahCvReviewClient>.Instance);
    private static CvReviewText Text() => new() { Vi = "Nhận xét dựa trên bằng chứng", En = "Evidence-based feedback" };
    private static CvReviewAiResponse Response() => new()
    {
        Summary = Text(), Strengths = [Text()], Improvements = [Text()],
        Criteria = new[] { "content", "clarity", "achievements", "structure" }.Select(id => new CvReviewCriterion { Id = id, Score = 70, Feedback = Text() }).ToList(),
        Actions = [new() { Priority = "High", Section = Text(), Evidence = "Used Git for version control.", Issue = Text(), Action = Text() }],
        JobMatch = new() { Score = 99, Summary = Text(), Requirements = [
            new() { Requirement = "React", Importance = "Required", Status = "Met", Evidence = "React and JavaScript", Recommendation = Text() },
            new() { Requirement = "TypeScript", Importance = "Required", Status = "NotDemonstrated", Evidence = "", Recommendation = Text() }
        ] }
    };
    private sealed class Handler : HttpMessageHandler
    {
        public int Status = 200, Calls;
        public bool Timeout, Delay;
        public string Mode = "stop";
        public string? FirstMode;
        public Func<int, CvReviewAiResponse>? ResponseFactory;
        public string? Body, Url, Authorization;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (Delay) await Task.Delay(30, cancellationToken);
            Calls++; Url = request.RequestUri!.ToString(); Authorization = request.Headers.Authorization?.ToString();
            Body = await request.Content!.ReadAsStringAsync(cancellationToken);
            if (Timeout) throw new TaskCanceledException();
            var mode = Calls == 1 ? FirstMode ?? Mode : Mode;
            var response = mode == "malformed" ? "{\"choices\":[null]}" : JsonSerializer.Serialize(new { choices = new[] { new { finish_reason = mode, message = new { content = JsonSerializer.Serialize(ResponseFactory?.Invoke(Calls) ?? Response(), new JsonSerializerOptions(JsonSerializerDefaults.Web)) } } } });
            return new((HttpStatusCode)Status) { Content = new StringContent(response, Encoding.UTF8, "application/json") };
        }
    }
    private sealed class Factory(HttpMessageHandler handler) : IHttpClientFactory
    { public HttpClient CreateClient(string name) => new(handler, false); }
}

public sealed class RealXahFact : FactAttribute
{
    public RealXahFact()
    {
        if (Environment.GetEnvironmentVariable("SOFTSYNC_XAH_REAL_AI") != "1"
            || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CvLlm__ApiKey")))
            Skip = "Real XAH acceptance requires explicit opt-in and a server API key.";
    }
}
public sealed class RealXahCvReviewTests
{
    [RealXahFact][Trait("Category", "RealAi")]
    public async Task Synthetic_CV_and_JD_return_grounded_review_without_inventing_TypeScript()
    {
        var config = new ConfigurationBuilder().AddEnvironmentVariables().Build();
        var service = new Review(new XahCvReviewClient(new Factory(), config, NullLogger<XahCvReviewClient>.Instance));
        var result = await service.ReviewAsync(new(XahCvReviewTests.Cv, XahCvReviewTests.Jd));
        Assert.Equal(XahCvReviewClient.DefaultModel, result.Model);
        Assert.NotEmpty(result.Actions);
        Assert.Contains(result.JobMatch!.Requirements, r => r.Requirement.Equals("TypeScript", StringComparison.OrdinalIgnoreCase) && r.Status == "NotDemonstrated");
        Assert.DoesNotContain(result.Rewrites, r => r.Suggested.En.Contains("TypeScript", StringComparison.OrdinalIgnoreCase));
        Assert.All(result.Actions.Where(a => !string.IsNullOrEmpty(a.Requirement)), a => Assert.Contains(a.Requirement, XahCvReviewTests.Jd));
    }
    private sealed class Factory : IHttpClientFactory
    {
        private readonly HttpClient client = new(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = Timeout.InfiniteTimeSpan };
        public HttpClient CreateClient(string name) => client;
    }
}
