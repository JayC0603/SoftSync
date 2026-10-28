using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;

namespace SoftSync.Presentation.Services;

public static class GameHistoryEndpoints
{
    private const int MaxEntries = 50;
    private const int MaxEntryJsonLength = 50_000;
    private const int MaxRequestBytes = 2_000_000;
    private const string ClearMarkerId = "__clear-marker__";
    private static readonly HashSet<string> GameKeys = ["inbox-rush", "eisenhower", "focus-fortress", "day-tetris", "luat-an", "voice-practice"];
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static void MapGameHistoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/game-history").RequireAuthorization();
        group.MapGet("/csrf", (HttpContext context, IAntiforgery antiforgery) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            var tokens = antiforgery.GetAndStoreTokens(context);
            return Results.Ok(new { token = tokens.RequestToken, header = tokens.HeaderName });
        });
        group.MapGet("/{gameKey}", GetAsync);
        group.MapPost("/{gameKey}/sync", SyncAsync).WithMetadata(new RequestSizeLimitAttribute(MaxRequestBytes));
        group.MapDelete("/{gameKey}", ClearAsync);
    }

    private static async Task<IResult> GetAsync(string gameKey, HttpContext context,
        SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users, SoftSyncDbContext db)
    {
        context.Response.Headers.CacheControl = "no-store";
        if (!GameKeys.Contains(gameKey)) return Results.NotFound();
        var user = await GetCurrentUserAsync(context, signIn, users);
        if (user is null) return Results.Unauthorized();
        var clearAt = await db.GameHistoryRecords.AsNoTracking()
            .Where(x => x.UserId == user.Id && x.GameKey == gameKey && x.ClientEntryId == ClearMarkerId)
            .Select(x => (DateTime?)x.PlayedAtUtc).FirstOrDefaultAsync(context.RequestAborted);
        var rows = await db.GameHistoryRecords.AsNoTracking()
            .Where(x => x.UserId == user.Id && x.GameKey == gameKey && x.ClientEntryId != ClearMarkerId &&
                        (clearAt == null || x.PlayedAtUtc > clearAt))
            .OrderByDescending(x => x.PlayedAtUtc).ThenByDescending(x => x.Id)
            .Take(MaxEntries).ToListAsync(context.RequestAborted);
        return Results.Ok(new { entries = rows.Select(ToResponse), clearedAtUtc = clearAt });
    }

    private static async Task<IResult> SyncAsync(string gameKey, HttpContext context, IAntiforgery antiforgery,
        SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users, SoftSyncDbContext db)
    {
        context.Response.Headers.CacheControl = "no-store";
        if (!GameKeys.Contains(gameKey)) return Results.NotFound();
        try { await antiforgery.ValidateRequestAsync(context); }
        catch (AntiforgeryValidationException) { return Results.BadRequest(new { code = "invalid_csrf" }); }
        var user = await GetCurrentUserAsync(context, signIn, users);
        if (user is null) return Results.Unauthorized();
        if (!context.Request.HasJsonContentType()) return Results.StatusCode(StatusCodes.Status415UnsupportedMediaType);

        using var body = new MemoryStream();
        var buffer = new byte[8192];
        int read;
        while ((read = await context.Request.Body.ReadAsync(buffer, context.RequestAborted)) > 0)
        {
            if (body.Length + read > MaxRequestBytes) return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
            body.Write(buffer, 0, read);
        }

        GameHistorySyncRequest? request;
        try { request = JsonSerializer.Deserialize<GameHistorySyncRequest>(body.ToArray(), JsonOptions); }
        catch (JsonException) { return Results.BadRequest(new { code = "invalid_json" }); }
        if (request?.Entries is null || request.Entries.Count > MaxEntries)
            return Results.BadRequest(new { code = "invalid_entries", maximum = MaxEntries });

        var entries = request.Entries;
        if (entries.Any(x => string.IsNullOrWhiteSpace(x.ClientEntryId) || x.ClientEntryId == ClearMarkerId || x.ClientEntryId.Length > 80 ||
                             x.Data.ValueKind != JsonValueKind.Object || x.Data.GetRawText().Length > MaxEntryJsonLength))
            return Results.BadRequest(new { code = "invalid_entry" });

        var clearAt = await db.GameHistoryRecords
            .Where(x => x.UserId == user.Id && x.GameKey == gameKey && x.ClientEntryId == ClearMarkerId)
            .Select(x => (DateTime?)x.PlayedAtUtc).FirstOrDefaultAsync(context.RequestAborted);
        var clientIds = entries.Select(x => x.ClientEntryId!).Distinct().ToArray();
        if (clientIds.Length != entries.Count) return Results.BadRequest(new { code = "duplicate_entry_id" });
        var existing = await db.GameHistoryRecords
            .Where(x => x.UserId == user.Id && x.GameKey == gameKey && clientIds.Contains(x.ClientEntryId))
            .Select(x => x.ClientEntryId).ToListAsync(context.RequestAborted);
        var existingIds = existing.ToHashSet(StringComparer.Ordinal);
        foreach (var entry in entries.Where(x => !existingIds.Contains(x.ClientEntryId!) &&
                                                  (clearAt == null || x.PlayedAtUtc.UtcDateTime > clearAt)))
        {
            db.GameHistoryRecords.Add(new GameHistoryRecord
            {
                UserId = user.Id,
                GameKey = gameKey,
                ClientEntryId = entry.ClientEntryId!,
                PlayedAtUtc = entry.PlayedAtUtc.UtcDateTime,
                DataJson = entry.Data.GetRawText()
            });
        }
        await db.SaveChangesAsync(context.RequestAborted);

        var stale = await db.GameHistoryRecords
            .Where(x => x.UserId == user.Id && x.GameKey == gameKey && x.ClientEntryId != ClearMarkerId)
            .OrderByDescending(x => x.PlayedAtUtc).ThenByDescending(x => x.Id)
            .Skip(MaxEntries).ToListAsync(context.RequestAborted);
        if (stale.Count > 0)
        {
            db.GameHistoryRecords.RemoveRange(stale);
            await db.SaveChangesAsync(context.RequestAborted);
        }

        var latest = await db.GameHistoryRecords.AsNoTracking()
            .Where(x => x.UserId == user.Id && x.GameKey == gameKey && x.ClientEntryId != ClearMarkerId &&
                        (clearAt == null || x.PlayedAtUtc > clearAt))
            .OrderByDescending(x => x.PlayedAtUtc).ThenByDescending(x => x.Id)
            .Take(MaxEntries).ToListAsync(context.RequestAborted);
        return Results.Ok(new { entries = latest.Select(ToResponse), clearedAtUtc = clearAt });
    }

    private static async Task<IResult> ClearAsync(string gameKey, HttpContext context, IAntiforgery antiforgery,
        SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users, SoftSyncDbContext db)
    {
        context.Response.Headers.CacheControl = "no-store";
        if (!GameKeys.Contains(gameKey)) return Results.NotFound();
        try { await antiforgery.ValidateRequestAsync(context); }
        catch (AntiforgeryValidationException) { return Results.BadRequest(new { code = "invalid_csrf" }); }
        var user = await GetCurrentUserAsync(context, signIn, users);
        if (user is null) return Results.Unauthorized();

        var marker = await db.GameHistoryRecords.FirstOrDefaultAsync(x => x.UserId == user.Id &&
            x.GameKey == gameKey && x.ClientEntryId == ClearMarkerId, context.RequestAborted);
        var clearedAt = DateTime.UtcNow;
        if (marker is null)
        {
            marker = new GameHistoryRecord { UserId = user.Id, GameKey = gameKey, ClientEntryId = ClearMarkerId, DataJson = "{}" };
            db.GameHistoryRecords.Add(marker);
        }
        marker.PlayedAtUtc = clearedAt;
        var entries = await db.GameHistoryRecords.Where(x => x.UserId == user.Id && x.GameKey == gameKey &&
            x.ClientEntryId != ClearMarkerId).ToListAsync(context.RequestAborted);
        db.GameHistoryRecords.RemoveRange(entries);
        await db.SaveChangesAsync(context.RequestAborted);
        return Results.Ok(new { clearedAtUtc = clearedAt });
    }

    private static async Task<ApplicationUser?> GetCurrentUserAsync(HttpContext context,
        SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users)
    {
        var user = await signIn.ValidateSecurityStampAsync(context.User);
        if (user is null || await users.IsLockedOutAsync(user)) return null;
        var claimId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claimId, out var id) && id == user.Id ? user : null;
    }

    private static object ToResponse(GameHistoryRecord row)
    {
        using var document = JsonDocument.Parse(row.DataJson);
        return new
        {
            clientEntryId = row.ClientEntryId,
            playedAtUtc = DateTime.SpecifyKind(row.PlayedAtUtc, DateTimeKind.Utc),
            data = document.RootElement.Clone()
        };
    }
}

public sealed record GameHistorySyncRequest(List<GameHistorySyncEntry>? Entries);
public sealed record GameHistorySyncEntry(string? ClientEntryId, DateTimeOffset PlayedAtUtc, JsonElement Data);
