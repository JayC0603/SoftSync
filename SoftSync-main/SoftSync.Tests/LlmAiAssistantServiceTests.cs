using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using SoftSync.BLL.AI;
using SoftSync.BLL.Interfaces;
using SoftSync.Common.Dtos;
using SoftSync.Presentation.Services;
using Xunit;

namespace SoftSync.Tests;

public sealed class AssistantApiSmokeFact : FactAttribute
{
    public AssistantApiSmokeFact()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SOFTSYNC_ASSISTANT_SMOKE_KEY_FILE")))
            Skip = "Requires explicit opt-in to a small synthetic live assistant API check.";
    }
}

public sealed class LlmAiAssistantServiceTests
{
    private const string Question = "Mình muốn luyện giao tiếp";
    private const string Model = "giabaopro1908/gemini-3.8-flash-high";

    [Fact]
    public async Task One_provider_request_returns_both_languages_with_learner_context()
    {
        using var fixture = new Fixture();
        Assert.Equal("Phản hồi API", await fixture.Assistant.GetReplyAsync("[vi]" + Question, 7));
        Assert.Equal("API reply", await fixture.Assistant.GetReplyAsync("[en]" + Question, 7));
        var call = Assert.Single(fixture.Transport.Calls);
        Assert.Equal("https://api.xah.io/v1/chat/completions", call.Uri);
        Assert.True(call.Authenticated);
        using var body = JsonDocument.Parse(call.Body);
        Assert.Equal(Model, body.RootElement.GetProperty("model").GetString());
        var messages = body.RootElement.GetProperty("messages");
        Assert.Contains("SYNCY", messages[0].GetProperty("content").GetString());
        using var context = JsonDocument.Parse(messages[1].GetProperty("content").GetString()!);
        Assert.Equal(Question, context.RootElement.GetProperty("question").GetString());
        Assert.Equal(7, context.RootElement.GetProperty("userId").GetInt32());
        Assert.Equal(25, context.RootElement.GetProperty("progress")[0].GetProperty("PercentComplete").GetInt32());
        Assert.NotEmpty(context.RootElement.GetProperty("knowledge").EnumerateArray());
        Assert.DoesNotContain("synthetic-key", call.Body);
    }

    [Fact]
    public async Task Knowledge_fallback_is_not_cached_and_provider_can_recover()
    {
        using var fixture = new Fixture(firstStatus: HttpStatusCode.ServiceUnavailable);
        Assert.Equal("Nội dung cơ sở SoftSync.", await fixture.Assistant.GetReplyAsync("[vi]" + Question, 7));
        Assert.Equal("Phản hồi API", await fixture.Assistant.GetReplyAsync("[vi]" + Question, 7));
        Assert.Equal("API reply", await fixture.Assistant.GetReplyAsync("[en]" + Question, 7));
        Assert.Equal(2, fixture.Transport.Calls.Count);
    }

    [Fact]
    public async Task Identical_questions_do_not_share_cached_answers_between_users()
    {
        using var fixture = new Fixture();
        await fixture.Assistant.GetReplyAsync("[vi]" + Question, 7);
        await fixture.Assistant.GetReplyAsync("[vi]" + Question, 8);
        Assert.Equal(2, fixture.Transport.Calls.Count);
    }

    [AssistantApiSmokeFact]
    public async Task Live_assistant_uses_configured_model_and_returns_SYNCY_in_both_languages()
    {
        var keyPath = Environment.GetEnvironmentVariable("SOFTSYNC_ASSISTANT_SMOKE_KEY_FILE")!;
        using var fixture = new Fixture(key: File.ReadAllText(keyPath).Trim(), live: true);
        var vi = await fixture.Assistant.GetReplyAsync("[vi]Bạn là ai?", 0);
        var en = await fixture.Assistant.GetReplyAsync("[en]Bạn là ai?", 0);
        Assert.Contains("SYNCY", vi, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("SYNCY", en, StringComparison.OrdinalIgnoreCase);
        Assert.Single(fixture.Transport.Calls);
    }

    private sealed class Fixture : IDisposable
    {
        private readonly string directory = Path.Combine(Path.GetTempPath(), "softsync-assistant-api-" + Guid.NewGuid().ToString("N"));
        private readonly HttpClient client;
        public RecordingHandler Transport { get; }
        public LlmAiAssistantService Assistant { get; }
        public Fixture(HttpStatusCode firstStatus = HttpStatusCode.OK, string key = "synthetic-key", bool live = false)
        {
            Directory.CreateDirectory(directory);
            var knowledgePath = Path.Combine(directory, "knowledge.json");
            File.WriteAllText(knowledgePath, """[{"id":"softsync-overview","titleVi":"SoftSync","titleEn":"SoftSync","answerVi":"Nội dung cơ sở SoftSync.","answerEn":"SoftSync knowledge fallback.","route":"/","keywords":["SoftSync"],"tags":[]}]""");
            var knowledge = AssistantKnowledgeBase.Load(knowledgePath);
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AiApi:Enabled"] = "true", ["AiApi:Provider"] = "Xah", ["AiApi:UseDeploymentConfiguration"] = "true",
                ["AiApi:BaseUrl"] = "https://api.xah.io/v1/",
                ["AiApi:Model"] = live ? Environment.GetEnvironmentVariable("SOFTSYNC_ASSISTANT_SMOKE_MODEL") ?? XahCvReviewClient.DefaultModel : Model,
                ["AiApi:ApiKey"] = key,
                ["AiApi:TimeoutSeconds"] = "75"
            }).Build();
            Transport = new RecordingHandler(firstStatus, live);
            client = new HttpClient(Transport);
            var factory = new Factory(client);
            var progress = new Progress();
            var pdf = new PdfDocumentKnowledge(config, new HostEnvironment(directory), NullLogger<PdfDocumentKnowledge>.Instance);
            Assistant = new LlmAiAssistantService(factory, config, knowledge, progress,
                new KnowledgeBasedAiAssistantService(knowledge, progress), pdf, NullLogger<LlmAiAssistantService>.Instance);
        }
        public void Dispose() { client.Dispose(); Directory.Delete(directory, recursive: true); }
    }

    private sealed class Factory(HttpClient client) : IHttpClientFactory
    { public HttpClient CreateClient(string name) => client; }
    private sealed class Progress : IProgressService
    {
        public Task<IEnumerable<ProgressDto>> GetUserProgressAsync(int userId) => Task.FromResult<IEnumerable<ProgressDto>>(
            [new() { UserId = userId, SkillName = "Communication", PercentComplete = 25 }]);
    }
    private sealed class HostEnvironment(string directory) : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = directory;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ApplicationName { get; set; } = "SoftSync.Presentation";
        public string EnvironmentName { get; set; } = "Testing";
        public string ContentRootPath { get; set; } = directory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
    private sealed class RecordingHandler(HttpStatusCode firstStatus, bool live) : DelegatingHandler(new HttpClientHandler { AllowAutoRedirect = false })
    {
        public List<(string Uri, string Body, bool Authenticated)> Calls { get; } = [];
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls.Add((request.RequestUri!.AbsoluteUri, await request.Content!.ReadAsStringAsync(cancellationToken),
                !string.IsNullOrWhiteSpace(request.Headers.Authorization?.Parameter)));
            if (live) return await base.SendAsync(request, cancellationToken);
            var reply = JsonSerializer.Serialize(new { answerVi = "Phản hồi API", answerEn = "API reply", route = "/progress" });
            return new HttpResponseMessage(Calls.Count == 1 ? firstStatus : HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new { choices = new[] { new { message = new { content = reply } } } }), Encoding.UTF8, "application/json")
            };
        }
    }
}
