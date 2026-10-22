using System.Text.Json;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using SoftSync.DAL.Entities;

namespace SoftSync.Presentation.Services;

public static class CvReviewEndpoints
{
    public static void MapCvReviewEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/cv/review").RequireAuthorization();
        group.MapGet("/csrf", (HttpContext context, IAntiforgery antiforgery) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            var tokens = antiforgery.GetAndStoreTokens(context);
            return Results.Ok(new { token = tokens.RequestToken, header = tokens.HeaderName });
        });
        group.MapPost("/", HandleAsync);
        app.MapGet("/api/cv/documents/{id:guid}/download", DownloadAsync).RequireAuthorization();
    }

    private static async Task<IResult> DownloadAsync(Guid id, HttpContext context,
        SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users,
        SoftSync.BLL.Services.CvWorkspaceService workspace)
    {
        context.Response.Headers.CacheControl = "no-store";
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        var user = await signIn.ValidateSecurityStampAsync(context.User);
        if (user is null || await users.IsLockedOutAsync(user)) return Results.Unauthorized();
        var document = await workspace.GetDownloadAsync(user.Id, id);
        return document is null ? Results.NotFound() : Results.File(document.OriginalFile,
            document.Extension == "pdf" ? "application/pdf" : "text/plain; charset=utf-8", document.Name);
    }

    private static async Task<IResult> HandleAsync(HttpContext context, IAntiforgery antiforgery,
        SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users, CvReviewService review)
    {
        context.Response.Headers.CacheControl = "no-store";
        try { await antiforgery.ValidateRequestAsync(context); }
        catch (AntiforgeryValidationException) { return Results.BadRequest(new { code = "invalid_csrf", detail = "Invalid request token" }); }
        var user = await signIn.ValidateSecurityStampAsync(context.User);
        if (user is null || await users.IsLockedOutAsync(user)) return Results.Unauthorized();
        if (!context.Request.HasJsonContentType()) return Results.StatusCode(415);
        try
        {
            // Bound the actual stream too: Content-Length may be absent for chunked requests.
            using var body = new MemoryStream();
            var chunk = new byte[8192];
            int read;
            while ((read = await context.Request.Body.ReadAsync(chunk, context.RequestAborted)) > 0)
            {
                if (body.Length + read > 300_000) return Results.StatusCode(413);
                body.Write(chunk, 0, read);
            }
            var request = JsonSerializer.Deserialize<CvReviewRequest>(body.ToArray(), new JsonSerializerOptions(JsonSerializerDefaults.Web));
            if (request is null) return Results.BadRequest(new { code = "invalid_request", detail = "Request body is required" });
            return Results.Ok(await review.ReviewAsync(request, context.RequestAborted));
        }
        catch (JsonException) { return Results.BadRequest(new { code = "invalid_json", detail = "Invalid JSON request" }); }
        catch (CvReviewException ex)
        {
            return Results.Json(new { code = ex.Code, detail = ex.Feedback }, statusCode: ex.Code.StartsWith("ai_", StringComparison.Ordinal) ? 503 : 400);
        }
    }
}
