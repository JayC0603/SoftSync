using SoftSync.Common.Dtos;
namespace SoftSync.BLL.Interfaces;

public sealed class CvReviewException(string message) : Exception(message);

public interface ICvTextExtractor
{
    Task<string> ExtractAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
}
public interface ICvAnalysisService
{
    Task<(CvAnalysisResult Result, string Model)> AnalyzeAsync(string text, CancellationToken cancellationToken = default, string? jobDescription = null, string? language = null);
}
public interface ICvAiReadiness
{
    bool IsConfigured { get; }
    Task<bool> IsConfiguredAsync(CancellationToken cancellationToken = default) => Task.FromResult(IsConfigured);
}
public interface ICvReviewService
{
    Task<CvReviewDto> UploadAsync(int userId, Stream stream, string fileName, string mimeType, long size, CancellationToken cancellationToken = default, string? jobDescription = null, string? language = null);
    Task<IReadOnlyList<CvReviewDto>> HistoryAsync(int userId, CancellationToken cancellationToken = default);
    Task<CvReviewDto?> GetAsync(int userId, int analysisId, CancellationToken cancellationToken = default);
    Task<CvReviewDto?> ReanalyzeAsync(int userId, int documentId, CancellationToken cancellationToken = default, string? language = null);
    Task<bool> DeleteAsync(int userId, int documentId, CancellationToken cancellationToken = default);
}
