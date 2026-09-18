using Microsoft.EntityFrameworkCore;
using SoftSync.BLL.Auth;
using SoftSync.BLL.Interfaces;
using SoftSync.Common.Dtos;
using SoftSync.Common.Enums;
using SoftSync.Common;
using SoftSync.DAL.Entities;
using SoftSync.DAL.Repositories;

namespace SoftSync.BLL.Services;

public sealed class CourseService(ICourseRepository repository) : ICourseService
{
    public async Task<IReadOnlyList<CourseDto>> GetPublishedAsync(int authenticatedUserId)
    {
        if (authenticatedUserId <= 0) return [];
        var courses = await repository.Courses.AsNoTracking().Where(x => x.Status == CourseStatus.Published)
            .Include(x => x.Lessons).Include(x => x.Quizzes).OrderBy(x => x.Title).ToListAsync();
        var enrollments = await repository.Enrollments.AsNoTracking().Include(x => x.LessonProgress)
            .Where(x => x.UserId == authenticatedUserId).ToDictionaryAsync(x => x.CourseId);
        var attempts = await repository.QuizAttempts.AsNoTracking().Where(x => x.UserId == authenticatedUserId && x.CompletedAtUtc != null)
            .ToListAsync();
        return courses.Select(course =>
        {
            enrollments.TryGetValue(course.Id, out var enrollment);
            var finalQuiz = course.Quizzes.Where(x => x.Status == CourseStatus.Published).OrderBy(x => x.Id).FirstOrDefault();
            var latest = finalQuiz is null ? null : attempts.Where(x => x.QuizId == finalQuiz.Id).OrderByDescending(x => x.CompletedAtUtc).FirstOrDefault();
            return Map(course, enrollment, finalQuiz, latest);
        }).ToList();
    }

    public async Task<IReadOnlyList<CourseDto>> GetManagedAsync(int userId, bool isAdmin) =>
        (await repository.Courses.AsNoTracking().Where(x => isAdmin || x.CreatorUserId == userId)
            .Include(x => x.Lessons).Include(x => x.Quizzes).OrderByDescending(x => x.CreatedAtUtc).ToListAsync()).Select(x => Map(x)).ToList();

    public async Task<CourseDto?> GetForManagementAsync(int id, int userId, bool isAdmin)
    {
        var entity = await repository.Courses.AsNoTracking().Include(x => x.Lessons).Include(x => x.Quizzes)
            .FirstOrDefaultAsync(x => x.Id == id);
        return entity is null || !CourseAuthorization.CanManageOwnedContent(userId, entity.CreatorUserId, isAdmin) ? null : Map(entity);
    }

    public async Task<int?> SaveCourseAsync(CourseDto input, int userId, bool isAdmin)
    {
        if (userId <= 0 || string.IsNullOrWhiteSpace(input.Title) || input.Title.Trim().Length > 200 || (!string.IsNullOrWhiteSpace(input.ThumbnailUrl) && string.IsNullOrWhiteSpace(input.ThumbnailAltText))) return null;
        Course entity;
        if (input.Id == 0)
        {
            entity = new Course { CreatorUserId = userId };
            repository.Add(entity);
        }
        else
        {
            entity = await repository.Courses.FirstOrDefaultAsync(x => x.Id == input.Id) ?? new();
            if (entity.Id == 0 || !CourseAuthorization.CanManageOwnedContent(userId, entity.CreatorUserId, isAdmin)) return null;
        }
        entity.Title = input.Title.Trim(); entity.Description = input.Description.Trim(); entity.SkillId = input.SkillId;
        entity.ThumbnailUrl = input.ThumbnailUrl.Trim(); entity.ThumbnailAltText = input.ThumbnailAltText.Trim();
        await repository.SaveChangesAsync(); return entity.Id;
    }

    public async Task<bool> PublishCourseAsync(int id, int userId, bool isAdmin)
    {
        var entity = await repository.Courses.Include(x => x.Lessons).FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null || !CourseAuthorization.CanManageOwnedContent(userId, entity.CreatorUserId, isAdmin) || entity.Lessons.Count == 0) return false;
        entity.Status = CourseStatus.Published; entity.PublishedAtUtc ??= DateTime.UtcNow; return await repository.SaveChangesAsync();
    }

    public async Task<int?> SaveLessonAsync(CourseLessonDto input, int userId, bool isAdmin)
    {
        var course = await repository.Courses.Include(x => x.Lessons).FirstOrDefaultAsync(x => x.Id == input.CourseId);
        if (course is null || !CourseAuthorization.CanManageOwnedContent(userId, course.CreatorUserId, isAdmin) || string.IsNullOrWhiteSpace(input.Title)) return null;
        var lesson = input.Id == 0 ? new CourseLesson { CourseId = course.Id } : course.Lessons.FirstOrDefault(x => x.Id == input.Id);
        if (lesson is null) return null;
        if (input.Id == 0) repository.Add(lesson);
        lesson.Title = input.Title.Trim(); lesson.Description = input.Description.Trim(); lesson.Order = input.Order > 0 ? input.Order : course.Lessons.Count + 1;
        lesson.VideoUrl = input.VideoUrl.Trim(); lesson.Transcript = input.Transcript.Trim(); lesson.CaptionUrl = input.CaptionUrl.Trim();
        await repository.SaveChangesAsync(); return lesson.Id;
    }

    public async Task<bool> EnrollAsync(int courseId, int userId)
    {
        if (userId <= 0 || !await repository.Courses.AnyAsync(x => x.Id == courseId && x.Status == CourseStatus.Published)) return false;
        if (await repository.Enrollments.AnyAsync(x => x.CourseId == courseId && x.UserId == userId)) return true;
        repository.Add(new CourseEnrollment { CourseId = courseId, UserId = userId }); return await repository.SaveChangesAsync();
    }

    public async Task<bool> CompleteLessonAsync(int lessonId, int userId)
    {
        var lesson = await repository.Lessons.FirstOrDefaultAsync(x => x.Id == lessonId);
        if (lesson is null) return false;
        var enrollment = await repository.Enrollments.Include(x => x.LessonProgress).FirstOrDefaultAsync(x => x.CourseId == lesson.CourseId && x.UserId == userId);
        if (enrollment is null) return false;
        var progress = enrollment.LessonProgress.FirstOrDefault(x => x.LessonId == lessonId);
        var wasAlreadyCompleted = progress?.IsCompleted == true;
        if (progress is null) { progress = new CourseLessonProgress { EnrollmentId = enrollment.Id, LessonId = lessonId }; repository.Add(progress); }
        progress.IsCompleted = true; progress.CompletedAtUtc ??= DateTime.UtcNow;
        var total = await repository.Lessons.CountAsync(x => x.CourseId == lesson.CourseId);
        var complete = enrollment.LessonProgress.Where(x => x.IsCompleted).Select(x => x.LessonId).Append(lessonId).Distinct().Count();
        enrollment.ProgressPercentage = total == 0 ? 0 : Math.Min(100, complete * 100 / total);
        var finalQuiz = await repository.Quizzes.AsNoTracking().Where(x => x.CourseId == lesson.CourseId && x.Status == CourseStatus.Published).OrderBy(x => x.Id).FirstOrDefaultAsync();
        var hasPass = finalQuiz is not null && await repository.QuizAttempts.AsNoTracking().AnyAsync(x => x.QuizId == finalQuiz.Id && x.UserId == userId && x.CompletedAtUtc != null && x.Result == QuizAttemptResult.Pass);
        if (CourseCompletionRules.CanComplete(total, complete, finalQuiz is not null, hasPass)) enrollment.CompletedAtUtc ??= DateTime.UtcNow;
        return await repository.SaveChangesAsync() || wasAlreadyCompleted;
    }

    public async Task<CourseAnalyticsDto?> GetAnalyticsAsync(int courseId, int userId, bool isAdmin)
    {
        var course = await repository.Courses.AsNoTracking()
            .Where(x => x.Id == courseId && (isAdmin || x.CreatorUserId == userId))
            .Select(x => new { x.Id, x.Title })
            .FirstOrDefaultAsync();
        if (course is null) return null;

        var enrollments = repository.Enrollments.AsNoTracking().Where(x => x.CourseId == courseId);
        var summary = await enrollments.GroupBy(_ => 1).Select(group => new
        {
            Total = group.Count(),
            Completed = group.Count(x => x.CompletedAtUtc != null)
        }).FirstOrDefaultAsync();
        var enrollmentCount = summary?.Total ?? 0;
        var finalQuiz = await repository.Quizzes.AsNoTracking()
            .Where(x => x.CourseId == courseId && x.Status == CourseStatus.Published)
            .Select(x => new { x.Id })
            .FirstOrDefaultAsync();

        decimal? finalQuizPassRate = null;
        if (finalQuiz is not null)
        {
            var finalSummary = await repository.QuizAttempts.AsNoTracking()
                .Where(x => x.QuizId == finalQuiz.Id && x.CompletedAtUtc != null)
                .GroupBy(_ => 1)
                .Select(group => new { Total = group.Count(), Passed = group.Count(x => x.Result == QuizAttemptResult.Pass) })
                .FirstOrDefaultAsync();
            if (finalSummary is not null) finalQuizPassRate = finalSummary.Passed * 100m / finalSummary.Total;
        }

        var lessons = await repository.Lessons.AsNoTracking().Where(x => x.CourseId == courseId)
            .OrderBy(x => x.Order)
            .Select(lesson => new LessonAnalyticsDto
            {
                LessonId = lesson.Id,
                Order = lesson.Order,
                Title = lesson.Title,
                Enrollments = enrollmentCount,
                CompletedStudents = repository.Enrollments
                    .Where(enrollment => enrollment.CourseId == courseId)
                    .SelectMany(enrollment => enrollment.LessonProgress)
                    .Count(progress => progress.LessonId == lesson.Id && progress.IsCompleted)
            }).ToListAsync();

        return new CourseAnalyticsDto
        {
            CourseId = course.Id,
            CourseTitle = course.Title,
            Enrollments = enrollmentCount,
            Completed = summary?.Completed ?? 0,
            FinalQuizId = finalQuiz?.Id,
            FinalQuizPassRate = finalQuizPassRate,
            Lessons = lessons
        };
    }

    private static CourseDto Map(Course x, CourseEnrollment? enrollment = null, SkillChallenge? finalQuiz = null, QuizAttempt? latestAttempt = null) => new() { Id = x.Id, CreatorUserId = x.CreatorUserId, Title = x.Title, Description = x.Description, SkillId = x.SkillId, ThumbnailUrl = x.ThumbnailUrl, ThumbnailAltText = x.ThumbnailAltText, Status = x.Status, ProgressPercentage = enrollment?.ProgressPercentage ?? 0, CompletedLessons = enrollment?.LessonProgress.Count(p => p.IsCompleted) ?? 0, TotalLessons = x.Lessons.Count, CompletedAtUtc = enrollment?.CompletedAtUtc, FinalQuizId = finalQuiz?.Id, FinalQuizResult = latestAttempt?.Result, Lessons = x.Lessons.OrderBy(l => l.Order).Select(l => new CourseLessonDto { Id = l.Id, CourseId = l.CourseId, Title = l.Title, Description = l.Description, Order = l.Order, VideoUrl = l.VideoUrl, Transcript = l.Transcript, CaptionUrl = l.CaptionUrl, IsCompleted = enrollment?.LessonProgress.Any(p => p.LessonId == l.Id && p.IsCompleted) == true }).ToList(), Quizzes = x.Quizzes.Select(q => new ChallengeQuizDto { Id = q.Id, TeacherId = q.TeacherId, CourseId = q.CourseId, Title = q.Title, Status = q.Status }).ToList() };
}

public sealed class ChallengeService(ICourseRepository repository, IAiQuizService? aiQuiz = null) : IChallengeService
{
    public async Task<IReadOnlyList<StudentChallengeDto>> GetPublishedAsync() => await repository.Quizzes.AsNoTracking()
        .Where(x => x.Status == CourseStatus.Published).Select(x => new StudentChallengeDto { Id = x.Id, Title = x.Title, Description = x.Description, SkillName = x.Skill.Name, Difficulty = x.Difficulty, QuestionCount = x.Questions.Count }).ToListAsync();

    public async Task<StudentQuizDto?> GetPublishedQuizAsync(int quizId)
    {
        var quiz = await repository.Quizzes.AsNoTracking()
            .Where(x => x.Id == quizId && x.Status == CourseStatus.Published)
            .Include(x => x.Skill)
            .Include(x => x.Questions).ThenInclude(x => x.Options)
            .FirstOrDefaultAsync();
        if (quiz is null || !ChallengeRules.CanPublish(quiz.Questions.ToList())) return null;

        return new StudentQuizDto
        {
            Id = quiz.Id,
            Title = quiz.Title,
            Description = quiz.Description,
            SkillName = quiz.Skill.Name,
            Questions = quiz.Questions.OrderBy(x => x.Order).Select(question => new StudentQuizQuestionDto
            {
                Id = question.Id,
                QuestionText = question.QuestionText,
                Scenario = question.Scenario,
                ImageUrl = question.ImageUrl,
                ImageAltText = question.ImageAltText,
                Order = question.Order,
                Options = question.Options.OrderBy(x => x.Order).Select(option => new StudentQuizOptionDto
                {
                    Id = option.Id,
                    Text = option.Text,
                    Order = option.Order
                }).ToList()
            }).ToList()
        };
    }

    public async Task<StudentQuizSessionDto?> StartAttemptAsync(int quizId, int authenticatedUserId)
    {
        if (authenticatedUserId <= 0) return null;
        var quiz = await GetPublishedQuizAsync(quizId);
        if (quiz is null) return null;
        var existingAttempt = await repository.QuizAttempts.AsNoTracking()
            .Where(x => x.QuizId == quizId && x.UserId == authenticatedUserId && x.CompletedAtUtc == null)
            .OrderByDescending(x => x.StartedAtUtc)
            .FirstOrDefaultAsync();
        if (existingAttempt is not null)
            return new StudentQuizSessionDto { AttemptId = existingAttempt.Id, Quiz = quiz };

        var attempt = new QuizAttempt
        {
            QuizId = quizId,
            UserId = authenticatedUserId,
            StartedAtUtc = DateTime.UtcNow,
            TotalQuestions = ChallengeRules.RequiredQuestionCount
        };
        repository.Add(attempt);
        return await repository.SaveChangesAsync() ? new StudentQuizSessionDto { AttemptId = attempt.Id, Quiz = quiz } : null;
    }

    public async Task<QuizAttemptResultDto?> SubmitAsync(int attemptId, int authenticatedUserId, IReadOnlyCollection<StudentQuizAnswerDto> answers)
    {
        if (authenticatedUserId <= 0 || answers.Count != ChallengeRules.RequiredQuestionCount) return null;
        var selected = answers.GroupBy(x => x.QuestionId).ToDictionary(x => x.Key, x => x.Select(a => a.SelectedOptionId).Distinct().ToList());
        if (selected.Count != ChallengeRules.RequiredQuestionCount || selected.Any(x => x.Value.Count != 1)) return null;

        var attempt = await repository.QuizAttempts.Include(x => x.Quiz).ThenInclude(x => x.Questions).ThenInclude(x => x.Options)
            .Include(x => x.Answers)
            .FirstOrDefaultAsync(x => x.Id == attemptId && x.UserId == authenticatedUserId);
        if (attempt is null || attempt.CompletedAtUtc is not null || attempt.Answers.Count != 0 || attempt.Quiz.Status != CourseStatus.Published) return null;
        var quiz = attempt.Quiz;

        int score;
        try
        {
            score = ChallengeRules.Grade(quiz.Questions.ToList(), selected.ToDictionary(x => x.Key, x => x.Value[0]));
        }
        catch (ArgumentException)
        {
            return null;
        }

        attempt.CorrectAnswers = score;
        attempt.TotalQuestions = ChallengeRules.RequiredQuestionCount;
        attempt.ScorePercentage = score * 100m / ChallengeRules.RequiredQuestionCount;
        attempt.Result = score >= ChallengeRules.PassingScore ? QuizAttemptResult.Pass : QuizAttemptResult.NotPass;
        attempt.CompletedAtUtc = DateTime.UtcNow;
        attempt.Answers = quiz.Questions.Select(question =>
            {
                var optionId = selected[question.Id][0];
                return new QuizAttemptAnswer
                {
                    QuestionId = question.Id,
                    SelectedOptionId = optionId,
                    IsCorrect = question.Options.Any(option => option.Id == optionId && option.IsCorrect)
                };
            }).ToList();
        if (!await repository.SaveChangesAsync()) return null;
        if (attempt.Result == QuizAttemptResult.Pass && quiz.CourseId is int courseId)
            await EvaluateCourseCompletionAsync(courseId, authenticatedUserId);
        return MapAttempt(attempt, quiz.Title);
    }

    public async Task<QuizAttemptResultDto?> GetAttemptAsync(int attemptId, int authenticatedUserId)
    {
        if (authenticatedUserId <= 0) return null;
        var attempt = await repository.QuizAttempts.AsNoTracking().Include(x => x.Quiz)
            .FirstOrDefaultAsync(x => x.Id == attemptId && x.UserId == authenticatedUserId && x.CompletedAtUtc != null);
        return attempt is null ? null : MapAttempt(attempt, attempt.Quiz.Title);
    }

    public async Task<IReadOnlyList<QuizAttemptResultDto>> GetHistoryAsync(int authenticatedUserId)
    {
        if (authenticatedUserId <= 0) return [];
        return await repository.QuizAttempts.AsNoTracking()
            .Where(x => x.UserId == authenticatedUserId && x.CompletedAtUtc != null)
            .OrderByDescending(x => x.CompletedAtUtc)
            .Select(x => new QuizAttemptResultDto
            {
                AttemptId = x.Id, QuizId = x.QuizId, QuizTitle = x.Quiz.Title,
                StartedAtUtc = x.StartedAtUtc, CompletedAtUtc = x.CompletedAtUtc!.Value,
                CorrectAnswers = x.CorrectAnswers, TotalQuestions = x.TotalQuestions,
                ScorePercentage = x.ScorePercentage, Result = x.Result
            }).ToListAsync();
    }

    public async Task<QuizAttemptReviewDto?> GetReviewAsync(int attemptId, int authenticatedUserId, bool isTeacher, bool isAdmin)
    {
        if (authenticatedUserId <= 0) return null;
        var attempt = await repository.QuizAttempts.AsNoTracking()
            .Include(x => x.Quiz)
            .Include(x => x.Answers).ThenInclude(x => x.Question).ThenInclude(x => x.Options)
            .Include(x => x.Answers).ThenInclude(x => x.SelectedOption)
            .FirstOrDefaultAsync(x => x.Id == attemptId && x.CompletedAtUtc != null);
        if (attempt is null || !(attempt.UserId == authenticatedUserId || isAdmin || (isTeacher && attempt.Quiz.TeacherId == authenticatedUserId))) return null;

        return new QuizAttemptReviewDto
        {
            Result = MapAttempt(attempt, attempt.Quiz.Title),
            Answers = attempt.Answers.OrderBy(x => x.Question.Order).Select(answer => new QuizAnswerReviewDto
            {
                QuestionId = answer.QuestionId,
                Order = answer.Question.Order,
                QuestionText = answer.Question.QuestionText,
                Scenario = answer.Question.Scenario,
                ImageUrl = answer.Question.ImageUrl,
                ImageAltText = answer.Question.ImageAltText,
                SelectedAnswer = answer.SelectedOption.Text,
                CorrectAnswer = answer.Question.Options.Single(x => x.IsCorrect).Text,
                IsCorrect = answer.IsCorrect,
                Explanation = answer.Question.Explanation
            }).ToList()
        };
    }

    public async Task<IReadOnlyList<QuizAttemptResultDto>> GetQuizResultsAsync(int quizId, int authenticatedUserId, bool isAdmin)
    {
        var quiz = await repository.Quizzes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == quizId);
        if (quiz is null || !CourseAuthorization.CanManageOwnedContent(authenticatedUserId, quiz.TeacherId, isAdmin)) return [];
        return await repository.QuizAttempts.AsNoTracking().Where(x => x.QuizId == quizId && x.CompletedAtUtc != null)
            .OrderByDescending(x => x.CompletedAtUtc)
            .Select(x => new QuizAttemptResultDto
            {
                AttemptId = x.Id, QuizId = x.QuizId, QuizTitle = x.Quiz.Title,
                StartedAtUtc = x.StartedAtUtc, CompletedAtUtc = x.CompletedAtUtc!.Value,
                CorrectAnswers = x.CorrectAnswers, TotalQuestions = x.TotalQuestions,
                ScorePercentage = x.ScorePercentage, Result = x.Result
            }).ToListAsync();
    }

    public async Task<QuizAnalyticsDto?> GetAnalyticsAsync(int quizId, int userId, bool isAdmin)
    {
        var quiz = await repository.Quizzes.AsNoTracking()
            .Where(x => x.Id == quizId && (isAdmin || x.TeacherId == userId))
            .Select(x => new { x.Id, x.Title, TotalQuestions = x.Questions.Count })
            .FirstOrDefaultAsync();
        if (quiz is null) return null;

        var allAttempts = repository.QuizAttempts.AsNoTracking().Where(x => x.QuizId == quizId);
        var completed = allAttempts.Where(x => x.CompletedAtUtc != null);
        var summary = await completed.GroupBy(_ => 1).Select(group => new
        {
            Attempts = group.Count(),
            UniqueStudents = group.Select(x => x.UserId).Distinct().Count(),
            Passed = group.Count(x => x.Result == QuizAttemptResult.Pass),
            NotPassed = group.Count(x => x.Result == QuizAttemptResult.NotPass),
            Average = group.Average(x => (decimal)x.CorrectAnswers),
            Highest = group.Max(x => x.CorrectAnswers),
            Lowest = group.Min(x => x.CorrectAnswers)
        }).FirstOrDefaultAsync();

        var completedAnswers = completed.SelectMany(x => x.Answers);
        var questions = await repository.Quizzes.AsNoTracking().Where(x => x.Id == quizId)
            .SelectMany(x => x.Questions)
            .OrderBy(x => x.Order)
            .Select(question => new QuestionAnalyticsDto
            {
                QuestionId = question.Id,
                Order = question.Order,
                QuestionText = question.QuestionText,
                Attempts = completedAnswers.Count(answer => answer.QuestionId == question.Id),
                Correct = completedAnswers.Count(answer => answer.QuestionId == question.Id && answer.IsCorrect)
            }).ToListAsync();

        var students = await completed.GroupBy(x => new
            {
                x.UserId,
                x.User.DisplayName,
                x.User.FullName,
                x.User.UserName
            })
            .Select(group => new StudentQuizAnalyticsDto
            {
                UserId = group.Key.UserId,
                DisplayName = group.Key.DisplayName != string.Empty ? group.Key.DisplayName
                    : group.Key.FullName != string.Empty ? group.Key.FullName
                    : group.Key.UserName ?? $"Student {group.Key.UserId}",
                Attempts = group.Count(),
                LatestScore = group.OrderByDescending(x => x.CompletedAtUtc).Select(x => x.CorrectAnswers).First(),
                BestScore = group.Max(x => x.CorrectAnswers),
                LatestResult = group.OrderByDescending(x => x.CompletedAtUtc).Select(x => x.Result).First(),
                CompletedAtUtc = group.Max(x => x.CompletedAtUtc)!.Value
            })
            .OrderByDescending(x => x.CompletedAtUtc)
            .ToListAsync();

        var participants = await allAttempts.Select(x => x.UserId).Distinct().CountAsync();
        var inProgress = await allAttempts.CountAsync(x => x.CompletedAtUtc == null);
        return new QuizAnalyticsDto
        {
            QuizId = quiz.Id,
            QuizTitle = quiz.Title,
            Participants = participants,
            UniqueStudents = summary?.UniqueStudents ?? 0,
            CompletedAttempts = summary?.Attempts ?? 0,
            InProgressAttempts = inProgress,
            PassAttempts = summary?.Passed ?? 0,
            NotPassAttempts = summary?.NotPassed ?? 0,
            PassRate = summary is null ? null : summary.Passed * 100m / summary.Attempts,
            AverageScore = summary?.Average,
            HighestScore = summary?.Highest,
            LowestScore = summary?.Lowest,
            TotalQuestions = quiz.TotalQuestions,
            Questions = questions,
            Students = students
        };
    }

    public async Task<ChallengeQuizDto?> GetForManagementAsync(int id, int userId, bool isAdmin)
    {
        var q = await repository.Quizzes.AsNoTracking().Include(x => x.Questions).ThenInclude(x => x.Options).FirstOrDefaultAsync(x => x.Id == id);
        if (q is null || !CourseAuthorization.CanManageOwnedContent(userId, q.TeacherId, isAdmin)) return null;
        return Map(q);
    }

    public async Task<int?> SaveQuizAsync(ChallengeQuizDto input, int userId, bool isAdmin)
    {
        if (userId <= 0 || string.IsNullOrWhiteSpace(input.Title)) return null;
        if (input.CourseId is int courseId)
        {
            var course = await repository.Courses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == courseId);
            if (course is null || !CourseAuthorization.CanManageOwnedContent(userId, course.CreatorUserId, isAdmin)) return null;
        }
        SkillChallenge q;
        if (input.Id == 0) { q = new SkillChallenge { TeacherId = userId }; repository.Add(q); }
        else { q = await repository.Quizzes.FirstOrDefaultAsync(x => x.Id == input.Id) ?? new(); if (q.Id == 0 || !CourseAuthorization.CanManageOwnedContent(userId, q.TeacherId, isAdmin)) return null; }
        q.Title = input.Title.Trim(); q.Description = input.Description.Trim(); q.SkillId = input.SkillId; q.CourseId = input.CourseId; q.Difficulty = input.Difficulty;
        await repository.SaveChangesAsync(); return q.Id;
    }

    public async Task<bool> SaveQuestionAsync(int quizId, ChallengeQuestionEditorDto input, int userId, bool isAdmin)
    {
        var quiz = await repository.Quizzes.Include(x => x.Questions).ThenInclude(x => x.Options).FirstOrDefaultAsync(x => x.Id == quizId);
        if (quiz is null || quiz.Status != CourseStatus.Draft || !CourseAuthorization.CanManageOwnedContent(userId, quiz.TeacherId, isAdmin)
            || string.IsNullOrWhiteSpace(input.QuestionText) || (!string.IsNullOrWhiteSpace(input.ImageUrl) && string.IsNullOrWhiteSpace(input.ImageAltText)) || input.Options.Count != 4 || input.Options.Count(x => x.IsCorrect) != 1 || input.Options.Any(x => string.IsNullOrWhiteSpace(x.Text))) return false;
        var question = input.Id == 0 ? new ChallengeQuestion { QuizId = quizId } : quiz.Questions.FirstOrDefault(x => x.Id == input.Id);
        if (question is null) return false;
        if (input.Id == 0) repository.Add(question); else repository.RemoveRange(question.Options);
        question.QuestionText = input.QuestionText.Trim(); question.Scenario = input.Scenario.Trim(); question.ImageUrl = input.ImageUrl.Trim(); question.ImageAltText = input.ImageAltText.Trim(); question.Explanation = input.Explanation.Trim(); question.Type = input.Type; question.Order = input.Order > 0 ? input.Order : quiz.Questions.Count + 1;
        question.Options = input.Options.Select((x, i) => new ChallengeOption { Text = x.Text.Trim(), IsCorrect = x.IsCorrect, Order = i + 1 }).ToList();
        return await repository.SaveChangesAsync();
    }

    public async Task<bool> PublishAsync(int id, int userId, bool isAdmin)
    {
        var quiz = await repository.Quizzes.Include(x => x.Questions).ThenInclude(x => x.Options).FirstOrDefaultAsync(x => x.Id == id);
        if (quiz is null || !CourseAuthorization.CanManageOwnedContent(userId, quiz.TeacherId, isAdmin) || !ChallengeRules.CanPublish(quiz.Questions.ToList())) return false;
        if (quiz.CourseId is int courseId && await repository.Quizzes.AnyAsync(x => x.Id != quiz.Id && x.CourseId == courseId && x.Status == CourseStatus.Published)) return false;
        quiz.Status = CourseStatus.Published; quiz.PublishedAtUtc ??= DateTime.UtcNow; return await repository.SaveChangesAsync();
    }

    public async Task<AiQuizQuestionDraftDto?> GenerateQuestionDraftAsync(AiQuizDraftRequestDto input, int userId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        if (aiQuiz is null || userId <= 0) return null;
        var quiz = await repository.Quizzes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == input.QuizId, cancellationToken);
        if (quiz is null || quiz.Status != CourseStatus.Draft || !CourseAuthorization.CanManageOwnedContent(userId, quiz.TeacherId, isAdmin)) return null;
        try { return await aiQuiz.GenerateQuestionDraftAsync(input, cancellationToken); }
        catch (Exception) when (!cancellationToken.IsCancellationRequested) { return null; }
    }

    public async Task<string?> ExplainCompletedAnswerAsync(int attemptId, int questionId, int userId, CancellationToken cancellationToken = default)
    {
        if (aiQuiz is null || userId <= 0) return null;
        var attempt = await LoadOwnedCompletedReviewAsync(attemptId, userId, cancellationToken);
        var answer = attempt?.Answers.FirstOrDefault(x => x.QuestionId == questionId);
        if (answer is null) return null;
        try { return await aiQuiz.ExplainAnswerAsync(answer, cancellationToken); }
        catch (Exception) when (!cancellationToken.IsCancellationRequested) { return null; }
    }

    public async Task<AiQuizFeedbackDto?> GetPersonalizedFeedbackAsync(int attemptId, int userId, CancellationToken cancellationToken = default)
    {
        if (aiQuiz is null || userId <= 0) return null;
        var review = await LoadOwnedCompletedReviewAsync(attemptId, userId, cancellationToken);
        if (review is null) return null;
        try { return await aiQuiz.GenerateFeedbackAsync(review, cancellationToken); }
        catch (Exception) when (!cancellationToken.IsCancellationRequested) { return null; }
    }

    private async Task<QuizAttemptReviewDto?> LoadOwnedCompletedReviewAsync(int attemptId, int userId, CancellationToken cancellationToken)
    {
        var attempt = await repository.QuizAttempts.AsNoTracking()
            .Include(x => x.Quiz)
            .Include(x => x.Answers).ThenInclude(x => x.Question).ThenInclude(x => x.Options)
            .Include(x => x.Answers).ThenInclude(x => x.SelectedOption)
            .FirstOrDefaultAsync(x => x.Id == attemptId && x.UserId == userId && x.CompletedAtUtc != null, cancellationToken);
        if (attempt is null) return null;
        return new QuizAttemptReviewDto
        {
            Result = MapAttempt(attempt, attempt.Quiz.Title),
            Answers = attempt.Answers.OrderBy(x => x.Question.Order).Select(answer => new QuizAnswerReviewDto
            {
                QuestionId = answer.QuestionId, Order = answer.Question.Order, QuestionText = answer.Question.QuestionText,
                Scenario = answer.Question.Scenario, SelectedAnswer = answer.SelectedOption.Text,
                CorrectAnswer = answer.Question.Options.Single(x => x.IsCorrect).Text,
                IsCorrect = answer.IsCorrect, Explanation = answer.Question.Explanation
            }).ToList()
        };
    }

    private async Task EvaluateCourseCompletionAsync(int courseId, int userId)
    {
        var enrollment = await repository.Enrollments.Include(x => x.LessonProgress)
            .FirstOrDefaultAsync(x => x.CourseId == courseId && x.UserId == userId);
        if (enrollment is null || enrollment.CompletedAtUtc.HasValue) return;
        var total = await repository.Lessons.CountAsync(x => x.CourseId == courseId);
        var completed = enrollment.LessonProgress.Count(x => x.IsCompleted);
        if (CourseCompletionRules.CanComplete(total, completed, true, true))
        {
            enrollment.CompletedAtUtc = DateTime.UtcNow;
            await repository.SaveChangesAsync();
        }
    }

    private static ChallengeQuizDto Map(SkillChallenge q) => new() { Id = q.Id, TeacherId = q.TeacherId, CourseId = q.CourseId, Title = q.Title, Description = q.Description, SkillId = q.SkillId, Difficulty = q.Difficulty, Status = q.Status, Questions = q.Questions.OrderBy(x => x.Order).Select(x => new ChallengeQuestionEditorDto { Id = x.Id, QuestionText = x.QuestionText, Scenario = x.Scenario, ImageUrl = x.ImageUrl, ImageAltText = x.ImageAltText, Explanation = x.Explanation, Order = x.Order, Type = x.Type, Options = x.Options.OrderBy(o => o.Order).Select(o => new ChallengeOptionEditorDto { Id = o.Id, Text = o.Text, IsCorrect = o.IsCorrect, Order = o.Order }).ToList() }).ToList() };

    private static QuizAttemptResultDto MapAttempt(QuizAttempt attempt, string quizTitle) => new()
    {
        AttemptId = attempt.Id,
        QuizId = attempt.QuizId,
        QuizTitle = quizTitle,
        StartedAtUtc = attempt.StartedAtUtc,
        CompletedAtUtc = attempt.CompletedAtUtc!.Value,
        CorrectAnswers = attempt.CorrectAnswers,
        TotalQuestions = attempt.TotalQuestions,
        ScorePercentage = attempt.ScorePercentage,
        Result = attempt.Result
    };
}
