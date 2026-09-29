using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using SoftSync.Presentation.Services;
using Xunit;

namespace SoftSync.Tests;

public sealed class GeminiConfigurationTests
{
    [Fact]
    public async Task Default_transport_uses_Gemini_and_keeps_key_out_of_payload_and_URL()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["AiApi:Enabled"] = "true", ["AiApi:ApiKey"] = "synthetic-key" }).Build();
        var handler = new RecordingHandler();
        using var http = new HttpClient(handler);
        AiApiConfiguration.Configure(http, configuration);
        var client = new HuggingFaceJsonClient(new Factory(http), configuration, NullLogger<HuggingFaceJsonClient>.Instance);
        var result = await client.AskAsync<Reply>("test", "Return JSON only.", new { question = "Example" });
        Assert.Equal("ok", result?.Text);
        Assert.Equal(AiApiConfiguration.DefaultBaseUrl + "chat/completions", handler.Url);
        Assert.Equal("Bearer synthetic-key", handler.Authorization);
        Assert.DoesNotContain("synthetic-key", handler.Body);
        Assert.DoesNotContain("synthetic-key", handler.Url);
        using var json = JsonDocument.Parse(handler.Body);
        Assert.Equal(AiApiConfiguration.DefaultModel, json.RootElement.GetProperty("model").GetString());
        Assert.Equal(TimeSpan.FromSeconds(45), http.Timeout);
    }

    [Fact]
    public void Explicit_configuration_is_preserved_and_base_URL_normalized()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["AiApi:BaseUrl"] = "https://example.invalid/openai", ["AiApi:Model"] = "custom-model", ["AiApi:TimeoutSeconds"] = "12" }).Build();
        using var http = new HttpClient();
        AiApiConfiguration.Configure(http, configuration);
        Assert.Equal("https://example.invalid/openai/chat/completions", new Uri(http.BaseAddress!, AiApiConfiguration.ChatEndpoint).AbsoluteUri);
        Assert.Equal("custom-model", AiApiConfiguration.Model(configuration));
        Assert.Equal(TimeSpan.FromSeconds(12), http.Timeout);
    }

    public sealed class Reply { public string Text { get; set; } = ""; }
    [Theory]
    [InlineData(null, "gemini-3.1-flash-lite")]
    [InlineData("", "gemini-3.1-flash-lite")]
    [InlineData("  custom-lite  ", "custom-lite")]
    public async Task CV_uses_its_own_model_without_changing_other_AI(string? cvModel, string expected)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["AiApi:Enabled"] = "true", ["AiApi:ApiKey"] = "synthetic-key", ["AiApi:Model"] = "assistant-model", ["AiApi:CvModel"] = cvModel }).Build();
        var handler = new RecordingHandler();
        using var http = new HttpClient(handler);
        AiApiConfiguration.Configure(http, configuration);
        var client = new HuggingFaceJsonClient(new Factory(http), configuration, NullLogger<HuggingFaceJsonClient>.Instance);
        await client.AskAsync<Reply>("cv-review", "Return JSON only.", new { cvText = "Example" });
        using var json = JsonDocument.Parse(handler.Body);
        Assert.Equal(expected, json.RootElement.GetProperty("model").GetString());
        Assert.Equal(expected, AiApiConfiguration.Model(configuration, "cv-review"));
        Assert.Equal("assistant-model", AiApiConfiguration.Model(configuration));
    }
    private sealed class Factory(HttpClient client) : IHttpClientFactory
    { public HttpClient CreateClient(string name) => client; }
    private sealed class RecordingHandler : HttpMessageHandler
    {
        public string Url = "", Authorization = "", Body = "";
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Url = request.RequestUri!.AbsoluteUri;
            Authorization = request.Headers.Authorization!.ToString();
            Body = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new(HttpStatusCode.OK) { Content = new StringContent("{\"choices\":[{\"message\":{\"content\":\"{\\\"text\\\":\\\"ok\\\"}\"}}]}") };
        }
    }
}
