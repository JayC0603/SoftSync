using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using SoftSync.Presentation.Services;
using Xunit;

namespace SoftSync.Tests;

public sealed class XahFallbackTests
{
    private const string Primary = "synthetic-primary", Backup = "synthetic-backup", Model = "backup/model";
    private static IConfiguration Config() => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>
    {
        ["AiApi:ApiKey"] = Primary, ["CvLlm:ApiKey"] = Primary,
        ["XahFallback:ApiKey"] = Backup, ["XahFallback:Model"] = Model
    }).Build();
    private static HttpRequestMessage Request(string url = "https://api.xah.io/v1/chat/completions", string key = Primary)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
        request.Content = new StringContent("""{"model":"primary/model","max_tokens":7500,"response_format":{"type":"json_object"},"messages":[{"role":"user","content":"Synthetic document"}]}""",Encoding.UTF8,"application/json");
        return request;
    }
    private static HttpClient Client(Transport transport) => new(new XahFallbackHandler(Config(), NullLogger<XahFallbackHandler>.Instance) { InnerHandler = transport });

    [Fact]
    public async Task Healthy_primary_never_calls_backup_and_next_request_still_tries_primary()
    {
        var transport = new Transport { FirstStatus = 401 };
        using var client = Client(transport);
        using var first = await client.SendAsync(Request());
        using var second = await client.SendAsync(Request());
        Assert.Equal(new[]{Primary,Backup,Primary},transport.Keys);
        Assert.Equal(200,(int)second.StatusCode);
    }

    [Theory]
    [InlineData(401)][InlineData(403)][InlineData(402)][InlineData(404)][InlineData(429)]
    [InlineData(500)][InlineData(502)][InlineData(503)][InlineData(504)]
    public async Task Provider_failure_tries_one_backup_with_the_correct_model_and_unchanged_prompt(int status)
    {
        var transport = new Transport { FirstStatus = status };
        using var client = Client(transport);
        using var response = await client.SendAsync(Request());
        Assert.Equal(new[]{Primary,Backup},transport.Keys);
        using var original = JsonDocument.Parse(transport.Bodies[0]);
        using var backup = JsonDocument.Parse(transport.Bodies[1]);
        Assert.Equal("primary/model",original.RootElement.GetProperty("model").GetString());
        Assert.Equal(Model,backup.RootElement.GetProperty("model").GetString());
        foreach(var field in new[]{"messages","max_tokens","response_format"})
            Assert.Equal(original.RootElement.GetProperty(field).GetRawText(),backup.RootElement.GetProperty(field).GetRawText());
        Assert.DoesNotContain(Backup,transport.Bodies[1]);
        Assert.Equal(Model,Assert.Single(response.Headers.GetValues(XahFallbackHandler.ModelHeader)));
    }

    [Theory]
    [InlineData(200)][InlineData(400)][InlineData(413)][InlineData(422)]
    public async Task Successful_or_invalid_input_responses_do_not_rotate_credentials(int status)
    {
        var transport = new Transport { FirstStatus = status };
        using var client = Client(transport);
        using var response = await client.SendAsync(Request());
        Assert.Single(transport.Keys); Assert.Equal(status,(int)response.StatusCode);
    }

    [Theory]
    [InlineData("https://other.example/v1/chat/completions",Primary)]
    [InlineData("https://api.xah.io/v1/other",Primary)]
    [InlineData("https://api.xah.io/v1/chat/completions","unrelated-admin-key")]
    public async Task Other_hosts_routes_and_admin_credentials_never_receive_the_backup(string url,string key)
    {
        var transport = new Transport { FirstStatus = 401 };
        using var client = Client(transport);
        using var response = await client.SendAsync(Request(url,key));
        Assert.Equal(401,(int)response.StatusCode); Assert.Equal(new[]{key},transport.Keys);
    }

    [Fact]
    public async Task Explicit_connection_test_and_missing_configuration_do_not_use_backup()
    {
        var transport = new Transport { FirstStatus = 401 };
        using var client = Client(transport);
        using var request = Request(); request.Options.Set(XahFallbackHandler.SkipFallback,true);
        using var response = await client.SendAsync(request);
        Assert.Single(transport.Keys); Assert.Equal(401,(int)response.StatusCode);
        var empty = new ConfigurationBuilder().Build();
        var other = new Transport { FirstStatus = 401 };
        using var unconfigured = new HttpClient(new XahFallbackHandler(empty,NullLogger<XahFallbackHandler>.Instance){ InnerHandler=other });
        using var result = await unconfigured.SendAsync(Request());
        Assert.Single(other.Keys);
    }

    [Fact]
    public async Task Backup_failure_is_returned_without_a_retry_loop()
    {
        var transport = new Transport { FirstStatus = 401, BackupStatus = 429 };
        using var client = Client(transport);
        using var response = await client.SendAsync(Request());
        Assert.Equal(429,(int)response.StatusCode); Assert.Equal(2,transport.Keys.Count);
    }

    [Fact]
    public async Task Network_failure_can_use_backup_but_cancellation_cannot()
    {
        var transport = new Transport { ConnectionFailure = true };
        using var client = Client(transport);
        using var response = await client.SendAsync(Request());
        Assert.Equal(2,transport.Keys.Count); Assert.True(response.IsSuccessStatusCode);
        using var cancelled = new CancellationTokenSource();
        var stopping = new Transport { Cancel = cancelled };
        using var other = Client(stopping);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>other.SendAsync(Request(),cancelled.Token));
        Assert.Single(stopping.Keys);
    }

    [Fact]
    public async Task Cv_client_reports_the_model_that_actually_answered()
    {
        var transport = new Transport { FirstStatus = 401 };
        using var http = Client(transport);
        var client = new XahCvReviewClient(new Factory(http),Config(),NullLogger<XahCvReviewClient>.Instance);
        await client.AnalyzeAsync("Return JSON",new{cvText="Synthetic document"},CancellationToken.None);
        Assert.Equal(Model,client.ResponseModel);
    }

    private sealed class Factory(HttpClient http) : IHttpClientFactory { public HttpClient CreateClient(string name)=>http; }
    private sealed class Transport : HttpMessageHandler
    {
        public int FirstStatus = 200, BackupStatus = 200;
        public bool ConnectionFailure;
        public CancellationTokenSource? Cancel;
        public List<string> Keys = [], Bodies = [];
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Keys.Add(request.Headers.Authorization!.Parameter!);
            Bodies.Add(await request.Content!.ReadAsStringAsync(token));
            if(Keys.Count==1)
            {
                if(Cancel is not null) { Cancel.Cancel(); token.ThrowIfCancellationRequested(); }
                if(ConnectionFailure) throw new HttpRequestException("Synthetic network error");
            }
            var status=Keys.Count==1?FirstStatus:BackupStatus;
            return new((HttpStatusCode)status){Content=new StringContent("""{"choices":[{"finish_reason":"stop","message":{"content":"{}"}}]}""",Encoding.UTF8,"application/json")};
        }
    }
}
