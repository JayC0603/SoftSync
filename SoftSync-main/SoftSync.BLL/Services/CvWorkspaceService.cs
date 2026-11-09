using Microsoft.EntityFrameworkCore;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;
using System.Text.Json;

namespace SoftSync.BLL.Services;

public sealed record SavedCv(Guid Id, string Name, string Extension, long Size, DateTime CreatedAtUtc, string Text, string? ExtractionJson = null);
public sealed record SavedCvReview(Guid Id, Guid? DocumentId, string Name, string Role, string JobDescription, string ResultJson, DateTime CreatedAtUtc);

// Every operation is scoped to the authenticated user's ID supplied by the server.
public sealed class CvWorkspaceService(SoftSyncDbContext db)
{
    // Library navigation needs metadata only, not every document's extracted text.
    public Task<List<SavedCv>> ListDocumentSummariesAsync(int userId, CancellationToken cancellationToken = default) => db.CvDocuments.AsNoTracking()
        .Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAtUtc)
        .Select(x => new SavedCv(x.Id, x.Name, x.Extension, x.Size, x.CreatedAtUtc, "", null)).ToListAsync(cancellationToken);

    public Task<SavedCv?> GetDocumentAsync(int userId, Guid id, CancellationToken cancellationToken = default) => db.CvDocuments.AsNoTracking()
        .Where(x => x.UserId == userId && x.Id == id)
        .Select(x => new SavedCv(x.Id, x.Name, x.Extension, x.Size, x.CreatedAtUtc, x.Text, x.ExtractionJson)).SingleOrDefaultAsync(cancellationToken);

    public Task<List<SavedCv>> ListDocumentsAsync(int userId) => db.CvDocuments.AsNoTracking()
        .Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAtUtc)
        .Select(x => new SavedCv(x.Id, x.Name, x.Extension, x.Size, x.CreatedAtUtc, x.Text, x.ExtractionJson)).ToListAsync();

    public Task<List<SavedCvReview>> ListReviewsAsync(int userId, CancellationToken cancellationToken = default) => db.CvReviews.AsNoTracking()
        .Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAtUtc)
        .Select(x => new SavedCvReview(x.Id, x.DocumentId, x.Name, x.Role, x.JobDescription, x.ResultJson, x.CreatedAtUtc)).ToListAsync(cancellationToken);

    public async Task<SavedCvReview?> FindReusableReviewAsync(int userId, Guid documentId, string jobDescription,
        string cacheKey, CancellationToken cancellationToken = default)
    {
        if (userId <= 0) throw new UnauthorizedAccessException();
        var cutoff = DateTime.UtcNow.AddHours(-24);
        var jd = jobDescription.Trim();
        var candidates = await db.CvReviews.AsNoTracking()
            .Where(x => x.UserId == userId && x.DocumentId == documentId && x.JobDescription == jd && x.CreatedAtUtc >= cutoff
                && db.CvDocuments.Any(d => d.Id == documentId && d.UserId == userId))
            .OrderByDescending(x => x.CreatedAtUtc).Take(10)
            .Select(x => new SavedCvReview(x.Id, x.DocumentId, x.Name, x.Role, x.JobDescription, x.ResultJson, x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
        foreach (var candidate in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                using var json = JsonDocument.Parse(candidate.ResultJson);
                if (json.RootElement.ValueKind == JsonValueKind.Object
                    && json.RootElement.TryGetProperty("CacheKey", out var key)
                    && key.ValueKind == JsonValueKind.String && key.GetString() == cacheKey)
                    return candidate;
            }
            catch (JsonException) { /* Legacy/unreadable reviews must not block a fresh analysis. */ }
        }
        return null;
    }

    public async Task<SavedCv> AddDocumentAsync(int userId, string name, string extension, string text, byte[] file, string? extractionJson = null)
    {
        if (userId <= 0) throw new UnauthorizedAccessException();
        name = Path.GetFileName(name.Replace('\\', '/'));
        if (string.IsNullOrWhiteSpace(name) || name.Length > 255 || extension is not ("pdf" or "txt")
            || string.IsNullOrWhiteSpace(text) || text.Length > 40_000 || file.Length is < 1 or > 10 * 1024 * 1024
            || extractionJson?.Length > 32_000)
            throw new ArgumentException("Invalid CV document.");
        var entity = new CvDocumentRecord
        {
            Id = Guid.NewGuid(), UserId = userId, Name = name, Extension = extension,
            Size = file.Length, Text = text, OriginalFile = file, ExtractionJson = extractionJson, CreatedAtUtc = DateTime.UtcNow
        };
        db.CvDocuments.Add(entity);
        await db.SaveChangesAsync();
        db.Entry(entity).State = EntityState.Detached;
        return new(entity.Id, entity.Name, entity.Extension, entity.Size, entity.CreatedAtUtc, entity.Text, entity.ExtractionJson);
    }

    public Task<CvDocumentRecord?> GetDownloadAsync(int userId, Guid id) => db.CvDocuments.AsNoTracking()
        .SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId);

    public async Task<bool> DeleteDocumentAsync(int userId, Guid id)
    {
        var entity = await db.CvDocuments.SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (entity is null) return false;
        // Keep previously saved reviews even when their source CV is removed.
        var reviews = await db.CvReviews.Where(x => x.DocumentId == id && x.UserId == userId).ToListAsync();
        foreach (var review in reviews) review.DocumentId = null;
        db.CvDocuments.Remove(entity);
        await db.SaveChangesAsync();
        foreach (var review in reviews) db.Entry(review).State = EntityState.Detached;
        return true;
    }

    public async Task SaveReviewAsync(int userId, SavedCvReview review)
    {
        if (userId <= 0) throw new UnauthorizedAccessException();
        if (review.Id == Guid.Empty || string.IsNullOrWhiteSpace(review.Name) || review.Name.Length > 255
            || review.Role.Length > 255 || review.JobDescription.Length > 20_000
            || string.IsNullOrWhiteSpace(review.ResultJson) || review.ResultJson.Length > 150_000)
            throw new ArgumentException("Invalid CV review.");
        var existing = await db.CvReviews.AsNoTracking().SingleOrDefaultAsync(x => x.Id == review.Id);
        if (existing is not null)
        {
            if (existing.UserId != userId) throw new UnauthorizedAccessException();
            return; // Repeated clicks/retries do not duplicate a saved analysis.
        }
        if (review.DocumentId is null || !await db.CvDocuments.AnyAsync(x => x.Id == review.DocumentId && x.UserId == userId))
            throw new UnauthorizedAccessException("The CV is not owned by this account.");
        var entity = new CvReviewRecord
        {
            Id = review.Id, UserId = userId, DocumentId = review.DocumentId, Name = review.Name,
            Role = review.Role, JobDescription = review.JobDescription, ResultJson = review.ResultJson,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.CvReviews.Add(entity);
        await db.SaveChangesAsync();
        db.Entry(entity).State = EntityState.Detached;
    }
}
