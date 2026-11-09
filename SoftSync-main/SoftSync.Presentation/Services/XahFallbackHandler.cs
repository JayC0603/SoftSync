using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;

namespace SoftSync.Presentation.Services;

// Used by the deployment's XAH clients only. Each request still tries its primary key first.
public sealed class XahFallbackHandler(IConfiguration configuration, ILogger<XahFallbackHandler> logger) : DelegatingHandler
{
    public static readonly HttpRequestOptionsKey<bool> SkipFallback = new("SoftSync.SkipXahFallback");
    public const string ModelHeader = "X-SoftSync-Ai-Model";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var key = configuration["XahFallback:ApiKey"]?.Trim();
        var model = configuration["XahFallback:Model"]?.Trim();
        var primaryKey = request.Headers.Authorization?.Parameter;
        var eligible = request.Method == HttpMethod.Post && request.RequestUri is { Scheme: "https", Host: "api.xah.io", Port: 443, AbsolutePath: "/v1/chat/completions" }
            && request.Headers.Authorization?.Scheme == "Bearer"
            && !string.IsNullOrWhiteSpace(primaryKey)
            && (primaryKey == configuration["AiApi:ApiKey"]?.Trim() || primaryKey == configuration["CvLlm:ApiKey"]?.Trim())
            && !string.IsNullOrWhiteSpace(key) && key != primaryKey
            && !string.IsNullOrWhiteSpace(model) && model.Length <= 200 && !model.Any(char.IsWhiteSpace) && !model.Any(char.IsControl)
            && !(request.Options.TryGetValue(SkipFallback, out var skip) && skip)
            && request.Content is not null;
        if (!eligible) return await base.SendAsync(request, cancellationToken);

        // Buffer before sending so both attempts use the same documents and generation settings.
        var bytes = await request.Content!.ReadAsByteArrayAsync(cancellationToken);
        JsonObject? body;
        try { body = JsonNode.Parse(bytes) as JsonObject; }
        catch (System.Text.Json.JsonException) { return await base.SendAsync(request, cancellationToken); }
        if (body is null) return await base.SendAsync(request, cancellationToken);

        HttpResponseMessage? response;
        try
        {
            response = await base.SendAsync(request, cancellationToken);
            if (!ShouldFallback(response.StatusCode)) return response;
            logger.LogWarning("Primary XAH request returned HTTP {Status}; trying the configured backup once", (int)response.StatusCode);
            response.Dispose();
        }
        catch (HttpRequestException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Primary XAH connection failed; trying the configured backup once");
        }

        cancellationToken.ThrowIfCancellationRequested();
        body["model"] = model;
        using var backup = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version, VersionPolicy = request.VersionPolicy,
            Content = new StringContent(body.ToJsonString(), System.Text.Encoding.UTF8, "application/json")
        };
        foreach (var header in request.Headers)
            if (!header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                backup.Headers.TryAddWithoutValidation(header.Key, header.Value);
        backup.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
        response = await base.SendAsync(backup, cancellationToken);
        response.Headers.Remove(ModelHeader);
        response.Headers.TryAddWithoutValidation(ModelHeader, model);
        return response;
    }

    private static bool ShouldFallback(HttpStatusCode status) => status is HttpStatusCode.Unauthorized
        or HttpStatusCode.Forbidden or HttpStatusCode.PaymentRequired or HttpStatusCode.NotFound
        or HttpStatusCode.TooManyRequests || (int)status is >= 500 and <= 599;
}
