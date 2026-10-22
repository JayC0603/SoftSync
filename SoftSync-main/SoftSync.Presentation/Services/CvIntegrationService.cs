using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SoftSync.DAL.Entities;

namespace SoftSync.Presentation.Services;

public sealed class CvIntegrationOptions
{
    public string FrontendUrl { get; set; } = string.Empty;
    public string ApiBaseUrl { get; set; } = string.Empty;
    public string SharedSecret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "softsync";

    public Uri? FrontendOrigin => Origin(FrontendUrl);
    public Uri? ApiOrigin => Origin(ApiBaseUrl);
    public bool Configured => FrontendOrigin is not null && ApiOrigin is not null && Encoding.UTF8.GetByteCount(SharedSecret) >= 32;
    private static Uri? Origin(string value) => Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && uri.Scheme is "http" or "https" && string.IsNullOrEmpty(uri.UserInfo)
        ? new Uri(uri.GetLeftPart(UriPartial.Authority)) : null;
}

public sealed record CvSessionUser(string Email, string Name, string Username);
public sealed record CvIntegrationSession(string AccessToken, CvSessionUser User, long ExpiresAt);
internal sealed record CvExchangeResult(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("expires_at")] long ExpiresAt);

public sealed class CvIntegrationService(HttpClient http, IOptions<CvIntegrationOptions> options)
{
    public async Task<CvIntegrationSession> CreateSessionAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var config = options.Value;
        if (!config.Configured) throw new InvalidOperationException("CV integration is not configured");
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var header = Encode(JsonSerializer.SerializeToUtf8Bytes(new { alg = "HS256", typ = "JWT" }));
        var payload = Encode(JsonSerializer.SerializeToUtf8Bytes(new
        {
            iss = config.Issuer, aud = "softsync-cv", sub = user.Id.ToString(),
            iat = now, exp = now + 60, jti = Guid.NewGuid().ToString("N"), type = "softsync_assertion",
            email = user.Email ?? string.Empty, name = user.FullName, username = user.UserName ?? string.Empty
        }));
        var unsigned = $"{header}.{payload}";
        var signature = Encode(HMACSHA256.HashData(Encoding.UTF8.GetBytes(config.SharedSecret), Encoding.UTF8.GetBytes(unsigned)));
        using var response = await http.PostAsJsonAsync(new Uri(config.ApiOrigin!, "/api/auth/softsync/exchange"),
            new { assertion = $"{unsigned}.{signature}" }, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CvExchangeResult>(cancellationToken)
            ?? throw new HttpRequestException("CV service returned an empty session");
        if (string.IsNullOrWhiteSpace(result.AccessToken) || result.ExpiresAt <= now)
            throw new HttpRequestException("CV service returned an invalid session");
        return new CvIntegrationSession(result.AccessToken,
            new CvSessionUser(user.Email ?? string.Empty, user.FullName, user.UserName ?? string.Empty), result.ExpiresAt);
    }

    private static string Encode(byte[] value) => Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}

public static class CvIntegrationEndpoints
{
    public static void MapCvIntegrationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/cv-integration").RequireAuthorization();
        group.MapGet("/csrf", (HttpContext context, IAntiforgery antiforgery) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            var tokens = antiforgery.GetAndStoreTokens(context);
            return Results.Ok(new { token = tokens.RequestToken, header = tokens.HeaderName });
        });
        group.MapPost("/session", async (HttpContext context, IAntiforgery antiforgery,
            SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users,
            CvIntegrationService integration, ILogger<CvIntegrationService> logger) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            try { await antiforgery.ValidateRequestAsync(context); }
            catch (AntiforgeryValidationException) { return Results.BadRequest(new { detail = "Invalid request token" }); }
            var user = await signIn.ValidateSecurityStampAsync(context.User);
            if (user is null || await users.IsLockedOutAsync(user)) return Results.Unauthorized();
            try { return Results.Ok(await integration.CreateSessionAsync(user, context.RequestAborted)); }
            catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or TaskCanceledException)
            {
                logger.LogWarning("CV session could not be created ({FailureType})", ex.GetType().Name);
                return Results.Json(new { detail = "Chưa thể kết nối dịch vụ CV. Vui lòng thử lại." }, statusCode: 503);
            }
        });
    }
}
