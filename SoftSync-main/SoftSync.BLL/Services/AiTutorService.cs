using SoftSync.BLL.Interfaces;
using SoftSync.Common.Dtos;

namespace SoftSync.BLL.Services;

public sealed class AiTutorService(IAiAssistantService assistant) : IAiTutorService
{
    public Task<AiTutorResponseDto> ExplainLessonAsync(AiTutorLessonRequestDto request, int authenticatedUserId, CancellationToken cancellationToken = default) =>
        AskAsync(request, authenticatedUserId, "Explain this lesson simply. Include key points, one practical example and one practice suggestion.", cancellationToken);

    public Task<AiTutorResponseDto> SummarizeLessonAsync(AiTutorLessonRequestDto request, int authenticatedUserId, CancellationToken cancellationToken = default) =>
        AskAsync(request, authenticatedUserId, "Summarize this lesson in 3 to 5 key points and end with one practice suggestion.", cancellationToken);

    private async Task<AiTutorResponseDto> AskAsync(AiTutorLessonRequestDto request, int userId, string instruction, CancellationToken cancellationToken)
    {
        if (userId <= 0 || request is null || string.IsNullOrWhiteSpace(request.LessonTitle) || string.IsNullOrWhiteSpace(request.LessonContent))
            return Fallback(request?.LessonTitle, request?.Language);

        cancellationToken.ThrowIfCancellationRequested();
        var content = request.LessonContent.Trim();
        if (content.Length > 6000) content = content[..6000];
        var prompt = $"{instruction}\nKnowledge boundary: use only the lesson context below; if insufficient, say so.\nLesson: {request.LessonTitle.Trim()}\nLevel: {request.LearningLevel.Trim()}\nLearning mode: {request.PreferredLearningMode}\nObjective: {request.LearningObjective.Trim()}\nContext:\n{content}";
        try
        {
            var reply = await assistant.GetReplyAsync((request.Language == "en" ? "[en]" : "[vi]") + prompt, userId);
            return string.IsNullOrWhiteSpace(reply) ? Fallback(request.LessonTitle, request.Language) : new AiTutorResponseDto { Text = reply.Trim(), IsFallback = false };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch { return Fallback(request.LessonTitle, request.Language); }
    }

    private static AiTutorResponseDto Fallback(string? title, string? language = null) => new()
    {
        IsFallback = true,
        Text = language == "en"
            ? string.IsNullOrWhiteSpace(title) ? "This lesson does not contain enough information to generate an explanation."
                : $"An AI explanation for lesson “{title.Trim()}” is not available yet. Read the lesson content and try again later."
            : string.IsNullOrWhiteSpace(title)
            ? "Chưa có đủ thông tin trong bài học này để tạo giải thích."
            : $"Chưa thể tạo giải thích AI cho bài “{title.Trim()}”. Hãy đọc lại nội dung bài học và thử lại sau."
    };
}
