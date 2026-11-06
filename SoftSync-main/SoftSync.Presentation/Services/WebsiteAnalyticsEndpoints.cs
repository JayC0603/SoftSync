using SoftSync.BLL.Services;

namespace SoftSync.Presentation.Services;

public static class WebsiteAnalyticsEndpoints
{
    public sealed record VisitUpdate(int Sequence, bool Active, bool End = false);
    public static void MapWebsiteAnalytics(this WebApplication app)
    {
        var group = app.MapGroup("/api/visits").AllowAnonymous();
        // JSON bodies and same-origin checks prevent cross-site beacon submissions.
        group.AddEndpointFilter(async (context, next) =>
        {
            var request = context.HttpContext.Request;
            var origin = request.Headers.Origin.ToString();
            if (request.Headers["Sec-Fetch-Site"] == "cross-site" ||
                (origin.Length > 0 && (!Uri.TryCreate(origin, UriKind.Absolute, out var url) || url.Authority != request.Host.Value)))
                return Results.StatusCode(403);
            return await next(context);
        });
        group.MapPost("/", async (VisitUpdate body, WebsiteAnalyticsService service, CancellationToken ct) =>
            Results.Ok(new { id = await service.StartAsync(body.Active, ct) }));
        group.MapPost("/{id:guid}", async Task<IResult> (Guid id, VisitUpdate body, WebsiteAnalyticsService service, CancellationToken ct) =>
            await service.UpdateAsync(id, body.Sequence, body.Active, body.End, ct) ? Results.NoContent() : Results.NotFound());
    }
}
