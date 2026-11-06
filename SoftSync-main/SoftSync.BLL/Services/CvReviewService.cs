using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SoftSync.BLL.Interfaces;
using SoftSync.Common.Dtos;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;
namespace SoftSync.BLL.Services;

public sealed class CvReviewService(IDbContextFactory<SoftSyncDbContext> factory, ICvTextExtractor extractor, ICvAnalysisService ai, long maxBytes) : ICvReviewService
{
    public const int MaxJobDescriptionLength = 8000;
    public static string NormalizeLanguage(string? value) => string.Equals(value, "en", StringComparison.OrdinalIgnoreCase) ? "en" : "vi";
    public static string NormalizeJobDescription(string? value)
    {
        if (value?.Length > MaxJobDescriptionLength)
            throw new CvReviewException("Mô tả tuyển dụng tối đa 8.000 ký tự.");
        return value?.Trim() ?? "";
    }
    public static bool ValidRequirementMatches(CvAnalysisResult result, string jobDescription) =>
        result.RequirementMatches is not null && result.RequirementMatches.Count <= 30
        && (jobDescription.Length == 0 || result.RequirementMatches.Count > 0)
        && result.RequirementMatches.All(x => x is not null
            && x.Status is "Met" or "Partial" or "NotDemonstrated"
            && !string.IsNullOrWhiteSpace(x.Requirement) && x.Requirement.Length <= 2000
            && !string.IsNullOrWhiteSpace(x.Evidence) && x.Evidence.Length <= 2000
            && !string.IsNullOrWhiteSpace(x.Recommendation) && x.Recommendation.Length <= 2000);
    public static void ValidateFile(string name, string mime, long size, long limit)
    {
        var extension = Path.GetExtension(name).ToLowerInvariant();
        var valid = extension == ".pdf" && mime == "application/pdf" || extension == ".docx" && mime == "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        if (!valid || size <= 0 || size > limit) throw new CvReviewException("Chỉ nhận PDF/DOCX hợp lệ trong giới hạn dung lượng.");
    }
    public static bool IsValid(CvAnalysisResult? r) => r is not null && !string.IsNullOrWhiteSpace(r.Summary) && r.Summary.Length <= 4000
        && ValidList(r.Strengths) && ValidList(r.Weaknesses) && ValidList(r.SkillsDetected) && ValidList(r.MissingInformation)
        && ValidList(r.Suggestions) && r.Suggestions.Count > 0 && r.Sections is not null && r.Sections.Count is > 0 and <= 20
        && r.Sections.All(x => x is not null && !string.IsNullOrWhiteSpace(x.Section) && x.Section.Length <= 200 && !string.IsNullOrWhiteSpace(x.Feedback) && x.Feedback.Length <= 4000 && ValidList(x.Suggestions));
    private static bool ValidList(List<string>? values) => values is not null && values.Count <= 30 && values.All(x => !string.IsNullOrWhiteSpace(x) && x.Length <= 2000);
    public async Task<CvReviewDto> UploadAsync(int userId, Stream stream, string fileName, string mimeType, long size, CancellationToken cancellationToken = default, string? jobDescription = null, string? language = null)
    {
        if (userId <= 0) throw new UnauthorizedAccessException();
        var description = NormalizeJobDescription(jobDescription);
        ValidateFile(fileName, mimeType, size, maxBytes);
        var text = await extractor.ExtractAsync(stream, fileName, cancellationToken);
        if (text.Trim().Length < 40 || text.Length > 30000) throw new CvReviewException("CV cần có 40–30.000 ký tự văn bản có thể trích xuất. PDF scan chưa được hỗ trợ.");
        var response = await ai.AnalyzeAsync(text, cancellationToken, description, NormalizeLanguage(language));
        if (!IsValid(response.Result) || !ValidRequirementMatches(response.Result, description)) throw new CvReviewException("AI trả kết quả không hợp lệ. Hãy thử lại.");
        response.Result.JobDescription = description; // Authoritative user input, never an AI-generated snapshot.
        response.Result.Language = NormalizeLanguage(language);
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var safeFileName = Path.GetFileName(fileName.Replace('\\', '/'));
        var document = new CvDocument { UserId = userId, OriginalFileName = safeFileName[..Math.Min(safeFileName.Length, 255)], ExtractedText = text };
        var analysis = new CvAnalysis { Document = document, ResultJson = JsonSerializer.Serialize(response.Result), Model = response.Model };
        db.Add(analysis);
        await db.SaveChangesAsync(cancellationToken);
        return Map(analysis, document);
    }
    public async Task<IReadOnlyList<CvReviewDto>> HistoryAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0) return [];
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var rows = await db.Set<CvAnalysis>().AsNoTracking().Where(x => x.Document.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc).ThenByDescending(x => x.Id).Take(100)
            .Select(x => new { x.Id, x.CvDocumentId, x.Document.OriginalFileName, x.CreatedAtUtc, x.ResultJson })
            .ToListAsync(cancellationToken);
        return rows.Select(x => new CvReviewDto(x.Id, x.CvDocumentId, x.OriginalFileName, x.CreatedAtUtc,
            JsonSerializer.Deserialize<CvAnalysisResult>(x.ResultJson)!)).ToList();
    }
    public async Task<CvReviewDto?> GetAsync(int userId, int analysisId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0) return null;
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var row = await db.Set<CvAnalysis>().AsNoTracking()
            .Where(x => x.Id == analysisId && x.Document.UserId == userId)
            .Select(x => new { x.Id, x.CvDocumentId, x.Document.OriginalFileName, x.CreatedAtUtc, x.ResultJson })
            .SingleOrDefaultAsync(cancellationToken);
        return row is null ? null : new(row.Id, row.CvDocumentId, row.OriginalFileName, row.CreatedAtUtc,
            JsonSerializer.Deserialize<CvAnalysisResult>(row.ResultJson)!);
    }
    public async Task<CvReviewDto?> ReanalyzeAsync(int userId, int documentId, CancellationToken cancellationToken = default, string? language = null)
    {
        if (userId <= 0) return null;
        CvDocument? document;
        var description = "";
        var savedLanguage = "vi";
        await using (var readDb = await factory.CreateDbContextAsync(cancellationToken))
        {
            document = await readDb.Set<CvDocument>().AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == documentId && x.UserId == userId, cancellationToken);
            if (document is not null)
            {
                var latestJson = await readDb.Set<CvAnalysis>().AsNoTracking()
                    .Where(x => x.CvDocumentId == documentId && x.Document.UserId == userId)
                    .OrderByDescending(x => x.CreatedAtUtc).ThenByDescending(x => x.Id)
                    .Select(x => x.ResultJson).FirstOrDefaultAsync(cancellationToken);
                var latest = latestJson is null ? null : JsonSerializer.Deserialize<CvAnalysisResult>(latestJson);
                description = NormalizeJobDescription(latest?.JobDescription);
                savedLanguage = NormalizeLanguage(latest?.Language);
            }
        }
        if (document is null) return null;
        var responseLanguage = NormalizeLanguage(language ?? savedLanguage);
        var response = await ai.AnalyzeAsync(document.ExtractedText, cancellationToken, description, responseLanguage);
        if (!IsValid(response.Result) || !ValidRequirementMatches(response.Result, description)) throw new CvReviewException("AI trả kết quả không hợp lệ.");
        response.Result.JobDescription = description;
        response.Result.Language = responseLanguage;
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        // Re-check ownership/existence after network latency (the document may have been deleted).
        if (!await db.Set<CvDocument>().AnyAsync(x => x.Id == documentId && x.UserId == userId, cancellationToken)) return null;
        var analysis = new CvAnalysis { CvDocumentId = document.Id, Model = response.Model, ResultJson = JsonSerializer.Serialize(response.Result) };
        db.Add(analysis); await db.SaveChangesAsync(cancellationToken); return Map(analysis, document);
    }
    public async Task<bool> DeleteAsync(int userId, int documentId, CancellationToken cancellationToken = default)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var document = await db.Set<CvDocument>().Include(x => x.Analyses)
            .SingleOrDefaultAsync(x => x.Id == documentId && x.UserId == userId && userId > 0, cancellationToken);
        if (document is null) return false;
        db.Remove(document); await db.SaveChangesAsync(cancellationToken); return true;
    }
    private static CvReviewDto Map(CvAnalysis a, CvDocument d) => new(a.Id, d.Id, d.OriginalFileName, a.CreatedAtUtc, JsonSerializer.Deserialize<CvAnalysisResult>(a.ResultJson)!);
}
