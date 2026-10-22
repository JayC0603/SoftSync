using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using SoftSync.BLL.Auth;
using SoftSync.BLL.Interfaces;
using SoftSync.Common.Dtos;
using SoftSync.DAL.Data;

namespace SoftSync.Presentation.Services;

public sealed class AiSecretProtector(IDataProtectionProvider protection) : IAiSecretProtector
{
    private readonly IDataProtector protector = protection.CreateProtector("SoftSync.AiProvider.ApiKey.v1");
    public string Protect(string value) => protector.Protect(value);
    public string Unprotect(string value) => protector.Unprotect(value);
}

public sealed class IdentityAiAdminAccess(AuthenticationStateProvider auth,
    IDbContextFactory<SoftSyncDbContext> contexts) : IAiAdminAccess
{
    public async Task<int> RequireAdminAsync(CancellationToken cancellationToken = default)
    {
        var principal = (await auth.GetAuthenticationStateAsync()).User;
        var id = principal.GetUserId();
        if (principal.Identity?.IsAuthenticated != true || id <= 0 || !principal.IsInRole(CourseAuthorization.AdminRole))
            throw new UnauthorizedAccessException("Admin access required.");
        await using var db = await contexts.CreateDbContextAsync(cancellationToken);
        // Recheck persisted role; a stale circuit claim does not retain access after demotion.
        var allowed = await (from membership in db.UserRoles join role in db.Roles on membership.RoleId equals role.Id
            where membership.UserId == id && role.Name == CourseAuthorization.AdminRole select membership.UserId).AnyAsync(cancellationToken);
        if (!allowed) throw new UnauthorizedAccessException("Admin access required.");
        return id;
    }
}

public sealed class AiProviderConfigurationResolver(IDbContextFactory<SoftSyncDbContext> contexts,
    IAiSecretProtector protector, IConfiguration configuration, ILogger<AiProviderConfigurationResolver> logger)
    : IAiProviderConfigurationResolver
{
    public async Task<ResolvedAiProviderConfiguration?> ResolveAsync(string? task = null, CancellationToken cancellationToken = default)
    {
        await using var db = await contexts.CreateDbContextAsync(cancellationToken);
        var row = await db.AiProviderConfigurations.AsNoTracking().SingleOrDefaultAsync(x => x.IsEnabled && x.IsDefault, cancellationToken);
        if (row is null) return EnvironmentAiConfiguration.Resolve(configuration, task);
        try
        {
            // Do not fall back to another provider/key on decryption or request failures.
            if (row.Endpoint != SoftSync.BLL.Services.AiProviderRules.Endpoint(row.Provider)) return null;
            var key = protector.Unprotect(row.EncryptedApiKey);
            return new() { Provider = row.Provider, Endpoint = row.Endpoint,
                Model = task == "cv-review" && !string.IsNullOrWhiteSpace(row.CvModel) ? row.CvModel : row.Model,
                ApiKey = key, TimeoutSeconds = Math.Clamp(configuration.GetValue("AiApi:TimeoutSeconds", 45), 1, 120) };
        }
        catch (CryptographicException)
        { logger.LogWarning("Active AI credential could not be decrypted. Check Data Protection key persistence."); return null; }
    }
}

public static class EnvironmentAiConfiguration
{
    public static ResolvedAiProviderConfiguration? Resolve(IConfiguration configuration, string? task = null)
    {
        var key = configuration["AiApi:ApiKey"];
        if (!configuration.GetValue("AiApi:Enabled", false) || string.IsNullOrWhiteSpace(key)) return null;
        var endpoint = configuration["AiApi:BaseUrl"];
        endpoint = string.IsNullOrWhiteSpace(endpoint) ? AiApiConfiguration.DefaultBaseUrl : endpoint.Trim().TrimEnd('/') + "/";
        return new() { Endpoint = endpoint, ApiKey = key, Model = AiApiConfiguration.Model(configuration, task),
            Provider = endpoint.StartsWith("https://router.huggingface.co/", StringComparison.Ordinal) ? AiProviderType.HuggingFace
                : endpoint.StartsWith("https://api.openai.com/", StringComparison.Ordinal) ? AiProviderType.OpenAI : AiProviderType.Gemini,
            TimeoutSeconds = Math.Clamp(configuration.GetValue("AiApi:TimeoutSeconds", 45), 1, 120) };
    }
}

// All supported provider types use their official OpenAI-compatible chat transport.
public sealed class AiProviderConnectionTester(IHttpClientFactory clients) : IAiProviderConnectionTester
{
    public async Task<AiConnectionStatus> TestAsync(ResolvedAiProviderConfiguration configuration, CancellationToken cancellationToken = default)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(configuration.TimeoutSeconds));
        try
        {
            using var client = clients.CreateClient("AiRuntime");
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(configuration.Endpoint), AiApiConfiguration.ChatEndpoint));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", configuration.ApiKey);
            request.Content = JsonContent.Create(new { model = configuration.Model, max_tokens = 16,
                messages = new[] { new { role = "user", content = "Reply OK." } } });
            using var response = await client.SendAsync(request, timeout.Token);
            if (response.IsSuccessStatusCode)
            {
                using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(timeout.Token));
                return json.RootElement.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0
                    ? AiConnectionStatus.Success : AiConnectionStatus.InvalidResponse;
            }
            return response.StatusCode switch
            {
                HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => AiConnectionStatus.AuthenticationFailed,
                HttpStatusCode.BadRequest or HttpStatusCode.NotFound => AiConnectionStatus.ModelUnavailable,
                HttpStatusCode.TooManyRequests => AiConnectionStatus.RateLimited,
                _ => AiConnectionStatus.Unavailable
            };
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) { return AiConnectionStatus.Timeout; }
        catch (OperationCanceledException) { throw; }
        catch (JsonException) { return AiConnectionStatus.InvalidResponse; }
        catch (Exception ex) when (ex is HttpRequestException or UriFormatException or ArgumentException)
        { return AiConnectionStatus.Unavailable; }
    }
}
