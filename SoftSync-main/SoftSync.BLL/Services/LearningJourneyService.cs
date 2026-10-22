using SoftSync.BLL.Interfaces;
using SoftSync.Common.Dtos;
using SoftSync.Common.Enums;

namespace SoftSync.BLL.Services;

public sealed class LearningJourneyService(ICourseService courses, IChallengeService challenges) : ILearningJourneyService
{
    public async Task<LearningJourneyDto> GetAsync(int authenticatedUserId)
    {
        if (authenticatedUserId <= 0) return new();
        var courseList = (await courses.GetPublishedAsync(authenticatedUserId)).ToList();
        var quizList = (await challenges.GetPublishedAsync()).ToList();
        var history = await challenges.GetHistoryAsync(authenticatedUserId);
        var evidence = new List<SkillEvidenceDto>();

        foreach (var course in courseList.Where(x => x.IsCompleted))
            evidence.Add(new() { SourceId = course.Id, SkillId = course.SkillId, Skill = $"Skill {course.SkillId}", SourceType = "Course", Title = $"Completed course: {course.Title}", Description = "All required lessons and the final quiz were completed.", CreatedAtUtc = course.CompletedAtUtc!.Value });

        foreach (var attempt in history.Where(x => x.Passed))
        {
            var quiz = quizList.FirstOrDefault(x => x.Id == attempt.QuizId);
            if (quiz is null) continue;
            evidence.Add(new() { SourceId = attempt.AttemptId, Skill = quiz.SkillName, SourceType = "Quiz", Title = $"Passed quiz: {attempt.QuizTitle}", Description = $"Score {attempt.CorrectAnswers}/{attempt.TotalQuestions} ({attempt.ScorePercentage:0.#}%).", CreatedAtUtc = attempt.CompletedAtUtc });
        }

        var failedQuiz = history.FirstOrDefault(x => x.Result == QuizAttemptResult.NotPass);
        var unfinished = courseList.FirstOrDefault(x => !x.IsCompleted)?.Lessons.FirstOrDefault(x => !x.IsCompleted) is not null;
        var next = AdaptiveLearningRules.NextAction(failedQuiz?.Result, unfinished, courseList.Any(x => x.IsCompleted));
        return new() { Courses = courseList, Evidence = evidence.OrderByDescending(x => x.CreatedAtUtc).ToList(), RecommendedNextActivity = next };
    }
}
