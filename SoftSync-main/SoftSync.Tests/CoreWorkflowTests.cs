using SoftSync.BLL.Interfaces;
using SoftSync.BLL.Auth;
using SoftSync.BLL.Services;
using SoftSync.Common;
using SoftSync.Common.Dtos;
using SoftSync.Common.Enums;
using SoftSync.DAL.Entities;
using SoftSync.DAL.Data;
using SoftSync.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace SoftSync.Tests;

public sealed class CoreWorkflowTests
{
    [Theory]
    [InlineData(4, false)]
    [InlineData(5, false)]
    [InlineData(10, true)]
    public void Challenge_publish_requires_exactly_ten_valid_questions(int count, bool expected)
    {
        var questions = Enumerable.Range(1, count).Select(index => new ChallengeQuestion
        {
            QuestionText = $"Question {index}",
            Options = Enumerable.Range(1, 4).Select(option => new ChallengeOption { Text = $"Option {option}", IsCorrect = option == 1 }).ToList()
        }).ToList();

        Assert.Equal(expected, ChallengeRules.CanPublish(questions));
    }

    [Fact]
    public void Challenge_question_requires_four_options_and_one_correct_answer()
    {
        var question = new ChallengeQuestion { QuestionText = "Scenario", Options = [new() { Text = "A", IsCorrect = true }, new() { Text = "B", IsCorrect = true }, new() { Text = "C" }, new() { Text = "D" }] };
        Assert.False(ChallengeRules.IsValidQuestion(question));
    }

    [Fact]
    public void Identity_roles_and_teacher_management_rule_are_fixed_server_side()
    {
        Assert.Equal(["User", "Teacher", "Admin"], CourseAuthorization.Roles);
        Assert.True(CourseAuthorization.CanManageTeacherRoles(true, 10, 20));
        Assert.False(CourseAuthorization.CanManageTeacherRoles(false, 10, 20));
        Assert.False(CourseAuthorization.CanManageTeacherRoles(true, 10, 10));
    }

    [Fact]
    public void Ai_question_draft_rejects_invalid_option_count_and_correct_index()
    {
        Assert.False(ChallengeRules.IsValidAiDraft(new AiQuizQuestionDraftDto { QuestionText = "Q", Explanation = "E", Options = ["A", "B", "C"], SuggestedCorrectOptionIndex = 0 }));
        Assert.False(ChallengeRules.IsValidAiDraft(new AiQuizQuestionDraftDto { QuestionText = "Q", Explanation = "E", Options = ["A", "B", "C", "D"], SuggestedCorrectOptionIndex = 4 }));
        Assert.True(ChallengeRules.IsValidAiDraft(new AiQuizQuestionDraftDto { QuestionText = "Q", Explanation = "E", Options = ["A", "B", "C", "D"], SuggestedCorrectOptionIndex = 2 }));
    }

    [Fact]
    public async Task Teacher_ai_draft_requires_owned_draft_quiz()
    {
        var ai = new QuizAiStub();
        await using var data = await QuizData.CreateAsync(ai, CourseStatus.Draft);
        var request = new AiQuizDraftRequestDto { QuizId = QuizData.QuizId };

        Assert.NotNull(await data.Service.GenerateQuestionDraftAsync(request, QuizData.TeacherId, false));
        Assert.Null(await data.Service.GenerateQuestionDraftAsync(request, QuizData.OtherTeacherId, false));
        Assert.Equal(1, ai.DraftCalls);
    }

    [Fact]
    public async Task Student_ai_feedback_requires_own_completed_attempt()
    {
        var ai = new QuizAiStub();
        await using var data = await QuizData.CreateAsync(ai);
        var session = await data.Service.StartAttemptAsync(QuizData.QuizId, QuizData.StudentId);
        await data.Service.SubmitAsync(session!.AttemptId, QuizData.StudentId, data.Answers(4));

        Assert.NotNull(await data.Service.GetPersonalizedFeedbackAsync(session.AttemptId, QuizData.StudentId));
        Assert.Null(await data.Service.GetPersonalizedFeedbackAsync(session.AttemptId, QuizData.OtherStudentId));
        Assert.Equal(1, ai.FeedbackCalls);
        Assert.Equal(QuizAttemptResult.NotPass, (await data.Service.GetAttemptAsync(session.AttemptId, QuizData.StudentId))!.Result);
    }

    [Fact]
    public async Task Ai_provider_failure_does_not_change_persisted_quiz_result()
    {
        await using var data = await QuizData.CreateAsync(new ThrowingQuizAiStub());
        var session = await data.Service.StartAttemptAsync(QuizData.QuizId, QuizData.StudentId);
        var graded = await data.Service.SubmitAsync(session!.AttemptId, QuizData.StudentId, data.Answers(5));

        Assert.Null(await data.Service.GetPersonalizedFeedbackAsync(session.AttemptId, QuizData.StudentId));
        var persisted = await data.Service.GetAttemptAsync(session.AttemptId, QuizData.StudentId);
        Assert.Equal(QuizAttemptResult.Pass, graded!.Result);
        Assert.Equal(5, persisted!.CorrectAnswers);
        Assert.Equal(QuizAttemptResult.Pass, persisted.Result);
    }

    [Theory]
    [InlineData(4, false)]
    [InlineData(5, true)]
    [InlineData(10, true)]
    public void Student_quiz_is_graded_deterministically_with_five_as_the_pass_mark(int correctAnswers, bool expectedPassed)
    {
        var questions = CreateChallengeQuestions();
        var selected = questions.ToDictionary(
            question => question.Id,
            question => question.Options.Single(option => option.Order == (question.Id <= correctAnswers ? 1 : 2)).Id);

        var score = ChallengeRules.Grade(questions, selected);

        Assert.Equal(correctAnswers, score);
        Assert.Equal(expectedPassed, score >= ChallengeRules.PassingScore);
    }

    [Fact]
    public void Student_quiz_rejects_an_option_from_another_question()
    {
        var questions = CreateChallengeQuestions();
        var selected = questions.ToDictionary(question => question.Id, question => question.Options.First().Id);
        selected[questions[0].Id] = questions[1].Options.First().Id;

        Assert.Throws<ArgumentException>(() => ChallengeRules.Grade(questions, selected));
    }

    [Fact]
    public void Student_quiz_dto_does_not_expose_correct_answer_state()
    {
        Assert.Null(typeof(StudentQuizOptionDto).GetProperty("IsCorrect"));
        Assert.Null(typeof(StudentQuizQuestionDto).GetProperty("CorrectOption"));
    }

    [Fact]
    public async Task Student_quiz_persists_attempt_and_all_answers()
    {
        await using var data = await QuizData.CreateAsync();
        var session = await data.Service.StartAttemptAsync(QuizData.QuizId, QuizData.StudentId);
        Assert.NotNull(session);

        var result = await data.Service.SubmitAsync(session.AttemptId, QuizData.StudentId, data.Answers(correctCount: 6));

        Assert.NotNull(result);
        Assert.Equal(6, result.CorrectAnswers);
        Assert.Equal(QuizAttemptResult.Pass, result.Result);
        Assert.Equal(10, await data.Context.QuizAttemptAnswers.CountAsync(x => x.AttemptId == session.AttemptId));
        var saved = await data.Context.QuizAttempts.SingleAsync(x => x.Id == session.AttemptId);
        Assert.NotNull(saved.CompletedAtUtc);
        Assert.Equal(60m, saved.ScorePercentage);
    }

    [Theory]
    [InlineData(4, QuizAttemptResult.NotPass)]
    [InlineData(5, QuizAttemptResult.Pass)]
    public async Task Student_quiz_uses_the_five_of_ten_pass_rule(int correct, QuizAttemptResult expected)
    {
        await using var data = await QuizData.CreateAsync();
        var session = await data.Service.StartAttemptAsync(QuizData.QuizId, QuizData.StudentId);
        var result = await data.Service.SubmitAsync(session!.AttemptId, QuizData.StudentId, data.Answers(correct));
        Assert.Equal(expected, result!.Result);
    }

    [Fact]
    public async Task Student_quiz_reload_returns_the_persisted_result()
    {
        await using var data = await QuizData.CreateAsync();
        var session = await data.Service.StartAttemptAsync(QuizData.QuizId, QuizData.StudentId);
        await data.Service.SubmitAsync(session!.AttemptId, QuizData.StudentId, data.Answers(7));

        var reloaded = await data.Service.GetAttemptAsync(session.AttemptId, QuizData.StudentId);

        Assert.NotNull(reloaded);
        Assert.Equal(7, reloaded.CorrectAnswers);
        Assert.Equal(70m, reloaded.ScorePercentage);
    }

    [Fact]
    public async Task Student_cannot_read_another_users_attempt()
    {
        await using var data = await QuizData.CreateAsync();
        var session = await data.Service.StartAttemptAsync(QuizData.QuizId, QuizData.StudentId);
        await data.Service.SubmitAsync(session!.AttemptId, QuizData.StudentId, data.Answers(5));

        Assert.Null(await data.Service.GetAttemptAsync(session.AttemptId, QuizData.OtherStudentId));
        Assert.Null(await data.Service.GetReviewAsync(session.AttemptId, QuizData.OtherStudentId, false, false));
    }

    [Fact]
    public async Task Teacher_cannot_read_results_for_another_teachers_quiz()
    {
        await using var data = await QuizData.CreateAsync();
        var session = await data.Service.StartAttemptAsync(QuizData.QuizId, QuizData.StudentId);
        await data.Service.SubmitAsync(session!.AttemptId, QuizData.StudentId, data.Answers(5));

        Assert.Empty(await data.Service.GetQuizResultsAsync(QuizData.QuizId, QuizData.OtherTeacherId, false));
        Assert.Null(await data.Service.GetReviewAsync(session.AttemptId, QuizData.OtherTeacherId, true, false));
    }

    [Fact]
    public async Task Review_is_unavailable_until_the_attempt_is_completed()
    {
        await using var data = await QuizData.CreateAsync();
        var session = await data.Service.StartAttemptAsync(QuizData.QuizId, QuizData.StudentId);

        Assert.Null(await data.Service.GetReviewAsync(session!.AttemptId, QuizData.StudentId, false, false));
    }

    [Fact]
    public async Task Starting_the_same_quiz_reuses_the_users_unfinished_attempt()
    {
        await using var data = await QuizData.CreateAsync();
        var first = await data.Service.StartAttemptAsync(QuizData.QuizId, QuizData.StudentId);
        var second = await data.Service.StartAttemptAsync(QuizData.QuizId, QuizData.StudentId);

        Assert.Equal(first!.AttemptId, second!.AttemptId);
        Assert.Single(await data.Context.QuizAttempts.ToListAsync());
    }

    [Theory]
    [InlineData(2, 2, true, false, false)]
    [InlineData(2, 2, true, true, true)]
    [InlineData(2, 1, true, true, false)]
    [InlineData(2, 2, false, false, true)]
    public void Course_completion_requires_all_lessons_and_a_passing_published_final_quiz(int total, int completed, bool hasFinal, bool passed, bool expected) =>
        Assert.Equal(expected, CourseCompletionRules.CanComplete(total, completed, hasFinal, passed));

    [Theory]
    [InlineData(CourseStatus.Draft, false)]
    [InlineData(CourseStatus.Archived, false)]
    [InlineData(CourseStatus.Published, true)]
    public void Only_published_quiz_is_eligible_as_final_quiz(CourseStatus status, bool expected) =>
        Assert.Equal(expected, CourseCompletionRules.IsEligibleFinalQuiz(status));

    [Fact]
    public async Task Retry_pass_completes_course_and_preserves_previous_attempt()
    {
        await using var data = await QuizData.CreateAsync();
        await data.CompleteAllLessonsAsync();
        var failed = await data.Service.StartAttemptAsync(QuizData.QuizId, QuizData.StudentId);
        await data.Service.SubmitAsync(failed!.AttemptId, QuizData.StudentId, data.Answers(4));
        Assert.Null((await data.Context.CourseEnrollments.SingleAsync()).CompletedAtUtc);

        var passed = await data.Service.StartAttemptAsync(QuizData.QuizId, QuizData.StudentId);
        await data.Service.SubmitAsync(passed!.AttemptId, QuizData.StudentId, data.Answers(5));
        var completedAt = (await data.Context.CourseEnrollments.SingleAsync()).CompletedAtUtc;

        Assert.NotNull(completedAt);
        Assert.Equal(2, await data.Context.QuizAttempts.CountAsync());
        Assert.Equal(completedAt, (await data.CourseService.GetPublishedAsync(QuizData.StudentId)).Single().CompletedAtUtc);
    }

    [Fact]
    public async Task User_cannot_complete_another_users_enrollment_and_completion_is_idempotent()
    {
        await using var data = await QuizData.CreateAsync();
        Assert.False(await data.CourseService.CompleteLessonAsync(QuizData.FirstLessonId, QuizData.OtherStudentId));
        Assert.True(await data.CourseService.CompleteLessonAsync(QuizData.FirstLessonId, QuizData.StudentId));
        Assert.True(await data.CourseService.CompleteLessonAsync(QuizData.FirstLessonId, QuizData.StudentId));
        Assert.Single(await data.Context.CourseLessonProgress.ToListAsync());
    }

    [Fact]
    public async Task Teacher_cannot_attach_another_teachers_quiz_to_course()
    {
        await using var data = await QuizData.CreateAsync();
        var id = await data.Service.SaveQuizAsync(new ChallengeQuizDto { Title = "Foreign", CourseId = QuizData.CourseId }, QuizData.OtherTeacherId, false);
        Assert.Null(id);
    }

    [Fact]
    public async Task Quiz_analytics_distinguishes_retries_unique_students_and_in_progress_attempts()
    {
        await using var data = await QuizData.CreateAsync();
        var failed = await data.Service.StartAttemptAsync(QuizData.QuizId, QuizData.StudentId);
        await data.Service.SubmitAsync(failed!.AttemptId, QuizData.StudentId, data.Answers(4));
        var passed = await data.Service.StartAttemptAsync(QuizData.QuizId, QuizData.StudentId);
        await data.Service.SubmitAsync(passed!.AttemptId, QuizData.StudentId, data.Answers(5));
        await data.Service.StartAttemptAsync(QuizData.QuizId, QuizData.OtherStudentId);

        var analytics = await data.Service.GetAnalyticsAsync(QuizData.QuizId, QuizData.TeacherId, false);

        Assert.NotNull(analytics);
        Assert.Equal(2, analytics.Participants);
        Assert.Equal(1, analytics.UniqueStudents);
        Assert.Equal(2, analytics.CompletedAttempts);
        Assert.Equal(1, analytics.InProgressAttempts);
        Assert.Equal(1, analytics.PassAttempts);
        Assert.Equal(1, analytics.NotPassAttempts);
        Assert.Equal(50m, analytics.PassRate);
        Assert.Equal(4.5m, analytics.AverageScore);
        Assert.Equal(5, analytics.HighestScore);
        Assert.Equal(4, analytics.LowestScore);
        Assert.Equal(100m, analytics.Questions.Single(x => x.Order == 1).CorrectRate);
        Assert.Equal(50m, analytics.Questions.Single(x => x.Order == 5).CorrectRate);
        Assert.Single(analytics.Students);
        Assert.Equal(2, analytics.Students[0].Attempts);
    }

    [Fact]
    public async Task Analytics_enforces_teacher_ownership_and_allows_admin_policy()
    {
        await using var data = await QuizData.CreateAsync();

        Assert.Null(await data.Service.GetAnalyticsAsync(QuizData.QuizId, QuizData.OtherTeacherId, false));
        Assert.Null(await data.Service.GetAnalyticsAsync(QuizData.QuizId, QuizData.StudentId, false));
        Assert.NotNull(await data.Service.GetAnalyticsAsync(QuizData.QuizId, QuizData.OtherTeacherId, true));
        Assert.Null(await data.CourseService.GetAnalyticsAsync(QuizData.CourseId, QuizData.OtherTeacherId, false));
        Assert.NotNull(await data.CourseService.GetAnalyticsAsync(QuizData.CourseId, QuizData.OtherTeacherId, true));
    }

    [Fact]
    public async Task Empty_analytics_returns_safe_na_values()
    {
        await using var data = await QuizData.CreateAsync();

        var analytics = await data.Service.GetAnalyticsAsync(QuizData.QuizId, QuizData.TeacherId, false);

        Assert.NotNull(analytics);
        Assert.Equal(0, analytics.CompletedAttempts);
        Assert.Null(analytics.PassRate);
        Assert.Null(analytics.AverageScore);
        Assert.All(analytics.Questions, question => Assert.Null(question.CorrectRate));
    }

    [Fact]
    public async Task Course_analytics_reports_completed_and_in_progress_enrollments()
    {
        await using var data = await QuizData.CreateAsync();
        var before = await data.CourseService.GetAnalyticsAsync(QuizData.CourseId, QuizData.TeacherId, false);
        await data.CompleteAllLessonsAsync();
        var session = await data.Service.StartAttemptAsync(QuizData.QuizId, QuizData.StudentId);
        await data.Service.SubmitAsync(session!.AttemptId, QuizData.StudentId, data.Answers(5));
        var after = await data.CourseService.GetAnalyticsAsync(QuizData.CourseId, QuizData.TeacherId, false);

        Assert.NotNull(before);
        Assert.Equal(1, before.Enrollments);
        Assert.Equal(0, before.Completed);
        Assert.Equal(1, before.InProgress);
        Assert.NotNull(after);
        Assert.Equal(1, after.Completed);
        Assert.Equal(100m, after.CompletionRate);
        Assert.All(after.Lessons, lesson => Assert.Equal(100m, lesson.CompletionRate));
        Assert.Equal(100m, after.FinalQuizPassRate);
    }

    [Theory]
    [InlineData(7, 7, false, true)]
    [InlineData(7, 8, false, false)]
    [InlineData(7, 8, true, true)]
    [InlineData(0, 8, true, false)]
    public void Course_content_ownership_requires_creator_or_admin(int actorId, int creatorId, bool isAdmin, bool expected)
    {
        Assert.Equal(expected, CourseAuthorization.CanManageOwnedContent(actorId, creatorId, isAdmin));
    }

    [Theory]
    [InlineData(4, 4, true)]
    [InlineData(4, 5, false)]
    [InlineData(0, 0, false)]
    public void Course_result_access_requires_the_authenticated_owner(int actorId, int ownerId, bool expected)
    {
        Assert.Equal(expected, CourseAuthorization.CanAccessOwnedResult(actorId, ownerId));
    }

    [Fact]
    public async Task Assessment_rejects_an_incomplete_submission()
    {
        var data = AssessmentData.Create(questionCount: 2);
        var service = new AssessmentService(data.Repository, new UserRepositoryStub(), new AssessmentAiStub());

        var answers = new List<UserAnswerDto>
        {
            new() { QuestionId = data.Questions[0].Id, OptionId = data.Options[0].Id }
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.SubmitAssessmentAsync(1, answers));
        Assert.Empty(data.Repository.Results);
    }

    [Fact]
    public async Task Assessment_awards_completion_xp_only_once()
    {
        var data = AssessmentData.Create(questionCount: 1);
        var users = new UserRepositoryStub();
        var service = new AssessmentService(data.Repository, users, new AssessmentAiStub());
        var answers = new List<UserAnswerDto>
        {
            new() { QuestionId = data.Questions[0].Id, OptionId = data.Options[0].Id }
        };

        await service.SubmitAssessmentAsync(1, answers);
        await service.SubmitAssessmentAsync(1, answers);

        Assert.Equal(SoftSync.Common.LevelSystem.AssessmentXp, users.User.ExperiencePoints);
        Assert.Equal(2, data.Repository.Results.Count);
    }

    [Fact]
    public async Task Assessment_score_is_deterministic_even_when_ai_returns_a_different_score()
    {
        var data = AssessmentData.Create(questionCount: 8, optionScore: 3);
        var service = new AssessmentService(data.Repository, new UserRepositoryStub(), new AssessmentAiStub(score: 999));
        var answers = data.Questions.Select((question, index) => new UserAnswerDto { QuestionId = question.Id, OptionId = data.Options[index].Id }).ToList();

        await service.SubmitAssessmentAsync(1, answers);

        var result = Assert.Single(data.Repository.Results);
        Assert.Equal(24, result.Score);
        Assert.Equal(AssessmentLevel.Proactive, result.Level);
    }

    [Fact]
    public async Task Assessment_persists_diagnosis_and_reload_returns_the_same_result()
    {
        var data = AssessmentData.Create(questionCount: 8, optionScore: 2);
        var service = new AssessmentService(data.Repository, new UserRepositoryStub(), new AssessmentAiStub());
        var answers = data.Questions.Select((question, index) => new UserAnswerDto { QuestionId = question.Id, OptionId = data.Options[index].Id }).ToList();

        await service.SubmitAssessmentAsync(1, answers);
        var first = (await service.GetLatestResultsAsync(1, 1)).Single();
        var reloaded = (await service.GetLatestResultsAsync(1, 1)).Single();

        Assert.Equal(first.Score, reloaded.Score);
        Assert.Equal(first.Diagnosis.English.Feedback, reloaded.Diagnosis.English.Feedback);
        Assert.NotEmpty(reloaded.Diagnosis.English.RecommendedActions);
    }

    [Fact]
    public async Task Assessment_history_rejects_a_different_authenticated_user()
    {
        var service = new AssessmentService(AssessmentData.Create(1).Repository, new UserRepositoryStub(), new AssessmentAiStub());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetHistoryAsync(authenticatedUserId: 2, ownerUserId: 1));
    }

    [Fact]
    public async Task Assessment_provider_failure_still_persists_a_safe_diagnosis()
    {
        var data = AssessmentData.Create(questionCount: 8, optionScore: 1);
        var service = new AssessmentService(data.Repository, new UserRepositoryStub(), new ThrowingAssessmentAiStub());
        var answers = data.Questions.Select((question, index) => new UserAnswerDto { QuestionId = question.Id, OptionId = data.Options[index].Id }).ToList();

        await service.SubmitAssessmentAsync(1, answers);
        var result = (await service.GetLatestResultsAsync(1, 1)).Single();

        Assert.Equal(8, result.Score);
        Assert.NotEmpty(result.Diagnosis.English.Evidence);
        Assert.NotEmpty(result.Diagnosis.Vietnamese.RecommendedActions);
    }

    [Fact]
    public async Task Roadmap_mutation_rejects_a_different_owner()
    {
        var roadmaps = new RoadmapRepositoryStub
        {
            Item = new RoadmapItem { Id = 10, UserId = 2, WeekNumber = 1, Title = "Communication" }
        };
        var service = new RoadmapService(
            roadmaps,
            new RoadmapAiStub(),
            new UserRepositoryStub(),
            AssessmentData.Create(1).Repository,
            new SkillRepositoryStub());

        var changed = await service.MarkVideoCompleteAsync(10, userId: 1);

        Assert.False(changed);
        Assert.Null(roadmaps.Item.VideoCompletedAtUtc);
        Assert.Equal(0, roadmaps.SaveCount);
    }

    [Fact]
    public async Task Progress_is_derived_from_persisted_roadmap_activity()
    {
        var now = DateTime.UtcNow;
        var roadmap = new RoadmapRepositoryStub
        {
            Item = new RoadmapItem
            {
                Id = 10,
                UserId = 1,
                Title = "Communication foundations",
                VideoCompletedAtUtc = now,
                ScriptCompletedAtUtc = now
            }
        };
        var service = new ProgressService(roadmap);

        var progress = (await service.GetUserProgressAsync(1)).Single();

        Assert.Equal(1, progress.SkillId);
        Assert.Equal(33, progress.PercentComplete);
        Assert.Equal(now, progress.UpdatedAt);
    }

    [Fact]
    public async Task Roadmap_generation_prioritizes_the_weakest_assessed_skill()
    {
        var assessments = AssessmentData.Create(1).Repository;
        assessments.Results.AddRange([
            new AssessmentResult { UserId = 1, SkillId = 1, Skill = new Skill { Id = 1, Name = "Communication" }, Score = 25, CreatedAt = DateTime.UtcNow },
            new AssessmentResult { UserId = 1, SkillId = 3, Skill = new Skill { Id = 3, Name = "Time Management" }, Score = 12, CreatedAt = DateTime.UtcNow }
        ]);
        var ai = new RoadmapAiStub();
        var service = new RoadmapService(new RoadmapRepositoryStub(), ai, new UserRepositoryStub(), assessments, new SkillRepositoryStub());

        await service.GetUserRoadmapAsync(1, 1);

        Assert.Equal("Time Management", ai.LastWeakSkills[0]);
        Assert.Equal("Communication", ai.LastWeakSkills[1]);
    }

    [Fact]
    public async Task Roadmap_completion_reload_and_next_action_use_persisted_state()
    {
        var roadmaps = new RoadmapRepositoryStub { Item = new RoadmapItem { Id = 10, UserId = 1, WeekNumber = 1, SkillId = 1, Title = "Communication foundations" } };
        var ai = new RoadmapAiStub { Items = [new RoadmapItemDto { WeekNumber = 1, SkillId = 1, SkillName = "Communication", Title = "Communication foundations" }] };
        var service = new RoadmapService(roadmaps, ai, new UserRepositoryStub(), AssessmentData.Create(1).Repository, new SkillRepositoryStub());

        var initial = await service.GetUserRoadmapAsync(1, 1);
        Assert.Equal(RoadmapActivityType.VideoLesson, initial.NextRecommendedActivity);

        Assert.True(await service.MarkVideoCompleteAsync(10, 1));
        var reloaded = await service.GetUserRoadmapAsync(1, 1);

        Assert.Equal(RoadmapActivityType.LessonScript, reloaded.NextRecommendedActivity);
        Assert.Equal(16, reloaded.Items.Single().ProgressPercent);
        Assert.NotEmpty(reloaded.Items.Single().CompletionEvidence);
    }

    [Fact]
    public async Task Roadmap_read_rejects_a_different_authenticated_user()
    {
        var service = new RoadmapService(new RoadmapRepositoryStub(), new RoadmapAiStub(), new UserRepositoryStub(), AssessmentData.Create(1).Repository, new SkillRepositoryStub());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetUserRoadmapAsync(2, 1));
    }

    private sealed class AssessmentAiStub(int score = 8) : IAiAssessmentService
    {
        public Task<AssessmentResultDto> EvaluateAsync(List<UserAnswerDto> answers) =>
            Task.FromResult(new AssessmentResultDto
            {
                SkillId = 1,
                Score = score,
                Level = AssessmentLevel.Mastery,
                Diagnosis = new AssessmentDiagnosisDto
                {
                    English = new AssessmentDiagnosisTextDto { Feedback = "Evidence-based feedback", Strengths = ["Observed strength"], Weaknesses = ["Observed gap"], Evidence = ["Submitted response evidence"], RecommendedActions = ["Practice once"] },
                    Vietnamese = new AssessmentDiagnosisTextDto { Feedback = "Phản hồi theo bằng chứng", Strengths = ["Điểm mạnh quan sát được"], Weaknesses = ["Điểm cần cải thiện"], Evidence = ["Bằng chứng từ câu trả lời"], RecommendedActions = ["Luyện tập một lần"] }
                }
            });
    }

    private static List<ChallengeQuestion> CreateChallengeQuestions() => Enumerable.Range(1, 10).Select(questionId => new ChallengeQuestion
    {
        Id = questionId,
        Order = questionId,
        QuestionText = $"Question {questionId}",
        Options = Enumerable.Range(1, 4).Select(order => new ChallengeOption
        {
            Id = questionId * 10 + order,
            QuestionId = questionId,
            Text = $"Option {order}",
            Order = order,
            IsCorrect = order == 1
        }).ToList()
    }).ToList();

    private sealed class QuizData : IAsyncDisposable
    {
        public const int QuizId = 50;
        public const int StudentId = 1;
        public const int OtherStudentId = 2;
        public const int TeacherId = 100;
        public const int OtherTeacherId = 101;
        public const int CourseId = 200;
        public const int FirstLessonId = 300;
        public required SoftSyncDbContext Context { get; init; }
        public required ChallengeService Service { get; init; }
        public required CourseService CourseService { get; init; }

        public static async Task<QuizData> CreateAsync(IAiQuizService? ai = null, CourseStatus quizStatus = CourseStatus.Published)
        {
            var options = new DbContextOptionsBuilder<SoftSyncDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var context = new SoftSyncDbContext(options);
            var skill = new Skill { Id = 1, Name = "Communication" };
            var quiz = new SkillChallenge
            {
                Id = QuizId,
                TeacherId = TeacherId,
                CourseId = CourseId,
                Title = "Communication quiz",
                SkillId = skill.Id,
                Skill = skill,
                Status = quizStatus,
                Questions = CreateChallengeQuestions()
            };
            var course = new Course { Id = CourseId, CreatorUserId = TeacherId, Title = "Course", SkillId = 1, Skill = skill, Status = CourseStatus.Published };
            course.Lessons = [new CourseLesson { Id = FirstLessonId, CourseId = CourseId, Title = "Lesson 1", Order = 1 }, new CourseLesson { Id = FirstLessonId + 1, CourseId = CourseId, Title = "Lesson 2", Order = 2 }];
            var enrollment = new CourseEnrollment { Id = 400, CourseId = CourseId, UserId = StudentId };
            foreach (var question in quiz.Questions) question.QuizId = quiz.Id;
            context.AddRange(
                new ApplicationUser { Id = StudentId, UserName = "student-a@example.test" },
                new ApplicationUser { Id = OtherStudentId, UserName = "student-b@example.test" },
                new ApplicationUser { Id = TeacherId, UserName = "teacher-a@example.test" },
                new ApplicationUser { Id = OtherTeacherId, UserName = "teacher-b@example.test" },
                skill, course, quiz, enrollment);
            await context.SaveChangesAsync();
            var repository = new CourseRepository(context);
            return new QuizData { Context = context, Service = new ChallengeService(repository, ai), CourseService = new CourseService(repository) };
        }

        public async Task CompleteAllLessonsAsync()
        {
            await CourseService.CompleteLessonAsync(FirstLessonId, StudentId);
            await CourseService.CompleteLessonAsync(FirstLessonId + 1, StudentId);
        }

        public List<StudentQuizAnswerDto> Answers(int correctCount) => Context.ChallengeQuestions
            .Include(x => x.Options).OrderBy(x => x.Order).AsEnumerable()
            .Select((question, index) => new StudentQuizAnswerDto
            {
                QuestionId = question.Id,
                SelectedOptionId = question.Options.Single(option => option.Order == (index < correctCount ? 1 : 2)).Id
            }).ToList();

        public ValueTask DisposeAsync() => Context.DisposeAsync();
    }

    private sealed class QuizAiStub : IAiQuizService
    {
        public int DraftCalls { get; private set; }
        public int FeedbackCalls { get; private set; }
        public Task<AiQuizQuestionDraftDto?> GenerateQuestionDraftAsync(AiQuizDraftRequestDto input, CancellationToken cancellationToken = default)
        {
            DraftCalls++;
            return Task.FromResult<AiQuizQuestionDraftDto?>(new() { QuestionText = "Draft", Explanation = "Because", Options = ["A", "B", "C", "D"], SuggestedCorrectOptionIndex = 0 });
        }
        public Task<string?> ExplainAnswerAsync(QuizAnswerReviewDto answer, CancellationToken cancellationToken = default) => Task.FromResult<string?>("Explanation");
        public Task<AiQuizFeedbackDto?> GenerateFeedbackAsync(QuizAttemptReviewDto review, CancellationToken cancellationToken = default)
        {
            FeedbackCalls++;
            return Task.FromResult<AiQuizFeedbackDto?>(new() { Summary = "Trong bài kiểm tra này...", SuggestedNextPractice = ["Practice"] });
        }
    }

    private sealed class ThrowingQuizAiStub : IAiQuizService
    {
        public Task<AiQuizQuestionDraftDto?> GenerateQuestionDraftAsync(AiQuizDraftRequestDto input, CancellationToken cancellationToken = default) => throw new HttpRequestException("Provider unavailable");
        public Task<string?> ExplainAnswerAsync(QuizAnswerReviewDto answer, CancellationToken cancellationToken = default) => throw new TimeoutException("Provider timeout");
        public Task<AiQuizFeedbackDto?> GenerateFeedbackAsync(QuizAttemptReviewDto review, CancellationToken cancellationToken = default) => throw new TimeoutException("Provider timeout");
    }

    private sealed class ThrowingAssessmentAiStub : IAiAssessmentService
    {
        public Task<AssessmentResultDto> EvaluateAsync(List<UserAnswerDto> answers) =>
            throw new HttpRequestException("Provider unavailable");
    }

    private sealed class RoadmapAiStub : IAiRoadmapService
    {
        public List<string> LastWeakSkills { get; private set; } = [];
        public List<RoadmapItemDto>? Items { get; init; }
        public Task<RoadmapDto> GenerateRoadmapAsync(int userId, List<string> weakSkills)
        {
            LastWeakSkills = [.. weakSkills];
            var items = Items ?? weakSkills.Select((skill, index) => new RoadmapItemDto { WeekNumber = index + 1, SkillName = skill, Title = $"{skill} foundations" }).ToList();
            return Task.FromResult(new RoadmapDto { UserId = userId, Items = items });
        }
    }

    private sealed class UserRepositoryStub : IUserRepository
    {
        public ApplicationUser User { get; } = new() { Id = 1, UserName = "learner@example.test" };
        public Task<ApplicationUser?> GetWithSkillSelectionsAsync(int userId) => Task.FromResult<ApplicationUser?>(User);
        public Task<IEnumerable<ApplicationUser>> GetAllAsync() => Task.FromResult<IEnumerable<ApplicationUser>>([User]);
        public Task<ApplicationUser?> GetByIdAsync(int id) => Task.FromResult<ApplicationUser?>(id == User.Id ? User : null);
        public Task AddAsync(ApplicationUser entity) => Task.CompletedTask;
        public void Update(ApplicationUser entity) { }
        public void Delete(ApplicationUser entity) { }
        public Task<bool> SaveChangesAsync() => Task.FromResult(true);
    }

    private sealed class SkillRepositoryStub : ISkillRepository
    {
        private readonly Skill skill = new() { Id = 1, Name = "Communication" };
        public Task<IEnumerable<Skill>> GetAllAsync() => Task.FromResult<IEnumerable<Skill>>([skill]);
        public Task<Skill?> GetByIdAsync(int id) => Task.FromResult<Skill?>(id == skill.Id ? skill : null);
        public Task AddAsync(Skill entity) => Task.CompletedTask;
        public void Update(Skill entity) { }
        public void Delete(Skill entity) { }
        public Task<bool> SaveChangesAsync() => Task.FromResult(true);
    }

    private sealed class RoadmapRepositoryStub : IRoadmapRepository
    {
        public RoadmapItem Item { get; set; } = new();
        public int SaveCount { get; private set; }
        public Task<IEnumerable<RoadmapItem>> GetByUserIdAsync(int userId) => Task.FromResult<IEnumerable<RoadmapItem>>(Item.UserId == userId ? [Item] : []);
        public Task<IEnumerable<RoadmapItem>> GetAllAsync() => Task.FromResult<IEnumerable<RoadmapItem>>([Item]);
        public Task<RoadmapItem?> GetByIdAsync(int id) => Task.FromResult<RoadmapItem?>(id == Item.Id ? Item : null);
        public Task AddAsync(RoadmapItem entity) { entity.Id = entity.Id == 0 ? 10 : entity.Id; Item = entity; return Task.CompletedTask; }
        public void Update(RoadmapItem entity) { }
        public void Delete(RoadmapItem entity) { }
        public Task<bool> SaveChangesAsync() { SaveCount++; return Task.FromResult(true); }
    }

    private sealed class AssessmentRepositoryStub : IAssessmentRepository
    {
        public required List<AssessmentQuestion> Questions { get; init; }
        public required List<AssessmentOption> Options { get; init; }
        public List<AssessmentResult> Results { get; } = [];
        public Task<IEnumerable<AssessmentQuestion>> GetQuestionsBySkillIdsAsync(List<int> skillIds) =>
            Task.FromResult<IEnumerable<AssessmentQuestion>>(Questions.Where(q => skillIds.Contains(q.SkillId)).ToList());
        public Task<List<int>> GetSelectedSkillIdsAsync(int userId) => Task.FromResult(new List<int> { 1 });
        public Task<IEnumerable<AssessmentOption>> GetAnsweredOptionsAsync(List<int> optionIds) =>
            Task.FromResult<IEnumerable<AssessmentOption>>(Options.Where(o => optionIds.Contains(o.Id)).ToList());
        public Task SaveResultAsync(AssessmentResult result) { Results.Add(result); return Task.CompletedTask; }
        public Task SaveResultsAsync(IEnumerable<AssessmentResult> results) { Results.AddRange(results); return Task.CompletedTask; }
        public Task<IEnumerable<AssessmentResult>> GetResultsByUserIdAsync(int userId) =>
            Task.FromResult<IEnumerable<AssessmentResult>>(Results.Where(r => r.UserId == userId).ToList());
    }

    private sealed record AssessmentData(
        AssessmentRepositoryStub Repository,
        List<AssessmentQuestion> Questions,
        List<AssessmentOption> Options)
    {
        public static AssessmentData Create(int questionCount, int optionScore = 1)
        {
            var skill = new Skill { Id = 1, Name = "Communication" };
            var questions = Enumerable.Range(1, questionCount)
                .Select(id => new AssessmentQuestion { Id = id, SkillId = 1, Skill = skill, QuestionText = $"Question {id}" })
                .ToList();
            var options = questions.Select(question => new AssessmentOption
            {
                Id = question.Id * 10,
                QuestionId = question.Id,
                Question = question,
                OptionText = "Answer",
                ScoreValue = optionScore
            }).ToList();
            foreach (var question in questions)
                question.Options = options.Where(option => option.QuestionId == question.Id).ToList();

            var repository = new AssessmentRepositoryStub { Questions = questions, Options = options };
            return new AssessmentData(repository, questions, options);
        }
    }
}
