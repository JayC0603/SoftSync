using SoftSync.Common.Dtos;
using SoftSync.Common.Enums;

namespace SoftSync.BLL.Services;

public sealed class LearningContextBuilder
{
    public AiTutorLessonRequestDto BuildLessonContext(CourseLessonDto lesson, string learningLevel, PreferredLearningMode learningMode = PreferredLearningMode.Text, string? progressSummary = null, string? evidenceSummary = null)
    {
        var context = lesson.Description;
        if (!string.IsNullOrWhiteSpace(lesson.Transcript)) context += $"\nTranscript:\n{lesson.Transcript}";
        if (!string.IsNullOrWhiteSpace(progressSummary)) context += $"\nProgress: {progressSummary.Trim()}";
        if (!string.IsNullOrWhiteSpace(evidenceSummary)) context += $"\nRelevant evidence: {evidenceSummary.Trim()}";
        return new() { LessonTitle = lesson.Title, LessonContent = context, LearningLevel = learningLevel, PreferredLearningMode = learningMode };
    }
}
