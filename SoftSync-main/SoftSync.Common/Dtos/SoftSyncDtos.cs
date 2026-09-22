using SoftSync.Common.Enums;

namespace SoftSync.Common.Dtos;

public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public UserRole Role { get; set; }
    public Gender Gender { get; set; }
    public string Goal { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public int ExperiencePoints { get; set; }
    public DateTime CreatedAt { get; set; }

    // Settings: profile / display
    public string DisplayName { get; set; } = string.Empty;

    // Settings: learning personalization
    public LearningLevel CurrentLevel { get; set; }
    public int DailyStudyMinutes { get; set; }
    public int StudyDaysPerWeek { get; set; }
    public StudyTime PreferredStudyTime { get; set; }

    // Settings: appearance
    public string PreferredLanguage { get; set; } = string.Empty;
    public ThemePreference Theme { get; set; } = ThemePreference.Light;
    public bool ReduceMotion { get; set; }

    // Derived from ExperiencePoints — not stored.
    public int Level => LevelSystem.GetLevel(ExperiencePoints);
    public int LevelProgressPercent => LevelSystem.ProgressPercent(ExperiencePoints);
    /// <summary>Name to show publicly: DisplayName if set, else FullName.</summary>
    public string PublicName => string.IsNullOrWhiteSpace(DisplayName) ? FullName : DisplayName;
}

public class SkillDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconName { get; set; } = string.Empty;
}

public class UserAnswerDto
{
    public int QuestionId { get; set; }
    public int OptionId { get; set; }
}

public class AssessmentResultDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int SkillId { get; set; }
    public int Score { get; set; }
    public AssessmentLevel Level { get; set; }
    public DateTime CreatedAt { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public AssessmentDiagnosisDto Diagnosis { get; set; } = new();
}

public class AssessmentAttemptDto
{
    public DateTime CompletedAt { get; set; }
    public List<AssessmentResultDto> Results { get; set; } = [];
}

public class AssessmentDiagnosisDto
{
    public AssessmentDiagnosisTextDto English { get; set; } = new();
    public AssessmentDiagnosisTextDto Vietnamese { get; set; } = new();
}

public class AssessmentDiagnosisTextDto
{
    public List<string> Strengths { get; set; } = [];
    public List<string> Weaknesses { get; set; } = [];
    public List<string> Evidence { get; set; } = [];
    public string Feedback { get; set; } = string.Empty;
    public List<string> RecommendedActions { get; set; } = [];
}

public class RoadmapDto
{
    public int UserId { get; set; }
    public List<RoadmapItemDto> Items { get; set; } = new();
    public int ProgressPercent { get; set; }
    public int? NextRecommendedItemId { get; set; }
    public RoadmapActivityType? NextRecommendedActivity { get; set; }
    public string PrioritySkill { get; set; } = string.Empty;
}

public class RoadmapItemDto
{
    public int Id { get; set; }
    public int WeekNumber { get; set; }
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public RoadmapContentOrigin ContentOrigin { get; set; } = RoadmapContentOrigin.SoftSyncCurated;
    public string SourceTitle { get; set; } = string.Empty;
    public string SourceOrganization { get; set; } = string.Empty;
    public string SourceReference { get; set; } = string.Empty;
    public string SourceReviewStatus { get; set; } = string.Empty;
    public int PriorityOrder { get; set; }
    public int CompletedActivityCount { get; set; }
    public int TotalActivityCount { get; set; } = RoadmapProgressCalculator.RequiredActivityCount;
    public int ProgressPercent { get; set; }
    public RoadmapActivityType? NextActivity { get; set; }
    public bool IsNextRecommended { get; set; }
    public List<string> CompletionEvidence { get; set; } = [];
    public bool IsVideoCompleted { get; set; }
    public bool IsPracticeCompleted { get; set; }
    public bool IsScenarioCompleted { get; set; }
    public bool IsReflectionCompleted { get; set; }
    public DateTime? VideoCompletedAtUtc { get; set; }
    public DateTime? ScriptCompletedAtUtc { get; set; }
    public DateTime? SummaryCompletedAtUtc { get; set; }
    public DateTime? PracticeCompletedAtUtc { get; set; }
    public DateTime? ScenarioCompletedAtUtc { get; set; }
    public DateTime? ReflectionCompletedAtUtc { get; set; }
    public string ReflectionText { get; set; } = string.Empty;
    public string LastLearningStep { get; set; } = "video";
    public bool IsCompleted { get; set; }
    public List<RoadmapQuizAttemptDto> QuizHistory { get; set; } = new();
    public List<RoadmapRoleplayAttemptDto> RoleplayHistory { get; set; } = new();
}

public class RoadmapQuizAttemptDto
{
    public int AttemptNumber { get; set; }
    public Dictionary<string, string> Answers { get; set; } = new();
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public bool Passed { get; set; }
    public string Feedback { get; set; } = string.Empty;
    public DateTime SubmittedAtUtc { get; set; }
}

public class RoadmapRoleplayAttemptDto
{
    public int AttemptNumber { get; set; }
    public int ScenarioId { get; set; }
    public List<string> UserMessages { get; set; } = new();
    public List<string> AiMessages { get; set; } = new();
    public double EmotionalIntelligenceScore { get; set; }
    public double ActiveListeningScore { get; set; }
    public double IStatementScore { get; set; }
    public double SolutionScore { get; set; }
    public double TotalScore { get; set; }
    public Dictionary<string, double> RubricScores { get; set; } = [];
    public string Feedback { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = [];
    public List<string> Weaknesses { get; set; } = [];
    public List<string> EvidenceFromConversation { get; set; } = [];
    public string SuggestedImprovement { get; set; } = string.Empty;
    public string RecommendedNextPractice { get; set; } = string.Empty;
    public DateTime SubmittedAtUtc { get; set; }
}

public class RoleplayScenarioDto
{
    public int Id { get; set; }
    public string TitleVi { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public int TargetSkillId { get; set; }
    public string ObjectiveVi { get; set; } = string.Empty;
    public string ObjectiveEn { get; set; } = string.Empty;
    public string PersonaVi { get; set; } = string.Empty;
    public string PersonaEn { get; set; } = string.Empty;
    public string ContextVi { get; set; } = string.Empty;
    public string ContextEn { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "Beginner";
    public string OpeningVi { get; set; } = string.Empty;
    public string OpeningEn { get; set; } = string.Empty;
    public int RequiredLearnerTurns { get; set; } = 3;
    public List<RoleplayRubricCriterionDto> Rubric { get; set; } = [];
}

public class RoleplayRubricCriterionDto
{
    public string Key { get; set; } = string.Empty;
    public string LabelVi { get; set; } = string.Empty;
    public string LabelEn { get; set; } = string.Empty;
    public double MaxScore { get; set; }
}

public class RoleplayTurnDto
{
    public int Id { get; set; }
    public int Sequence { get; set; }
    public RoleplaySpeaker Speaker { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

public class RoleplaySessionDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int RoadmapItemId { get; set; }
    public int ScenarioId { get; set; }
    public RoleplayScenarioDto Scenario { get; set; } = new();
    public RoleplaySessionStatus Status { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public List<RoleplayTurnDto> Turns { get; set; } = [];
    public RoadmapRoleplayAttemptDto? Evaluation { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

public class RoleplayAiEvaluationDto
{
    public Dictionary<string, double> Scores { get; set; } = [];
    public List<string> Strengths { get; set; } = [];
    public List<string> Weaknesses { get; set; } = [];
    public string SuggestedImprovement { get; set; } = string.Empty;
    public string RecommendedNextPractice { get; set; } = string.Empty;
}

public class CaseStudyDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Scenario { get; set; } = string.Empty;
    public int SkillId { get; set; }
    public List<CaseStudyOptionDto> Options { get; set; } = new();
}

public class CaseStudyOptionDto
{
    public int Id { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public bool IsRecommended { get; set; }
    public string Feedback { get; set; } = string.Empty;
}

public class ProgressDto
{
    public int UserId { get; set; }
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public int PercentComplete { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CourseDto
{
    public int Id { get; set; }
    public int CreatorUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int SkillId { get; set; } = 1;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string ThumbnailAltText { get; set; } = string.Empty;
    public CourseStatus Status { get; set; }
    public int ProgressPercentage { get; set; }
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public int? FinalQuizId { get; set; }
    public QuizAttemptResult? FinalQuizResult { get; set; }
    public bool IsCompleted => CompletedAtUtc.HasValue;
    public List<CourseLessonDto> Lessons { get; set; } = [];
    public List<ChallengeQuizDto> Quizzes { get; set; } = [];
}

public class CourseLessonDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public string VideoUrl { get; set; } = string.Empty;
    public string Transcript { get; set; } = string.Empty;
    public string CaptionUrl { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}

public class ChallengeQuizDto
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public int? CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int SkillId { get; set; } = 1;
    public ChallengeDifficulty Difficulty { get; set; }
    public CourseStatus Status { get; set; }
    public List<ChallengeQuestionEditorDto> Questions { get; set; } = [];
}

public class ChallengeQuestionEditorDto
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string Scenario { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string ImageAltText { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public int Order { get; set; }
    public ChallengeQuestionType Type { get; set; }
    public List<ChallengeOptionEditorDto> Options { get; set; } = [];
}

public class ChallengeOptionEditorDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int Order { get; set; }
}

public class AiQuizDraftRequestDto
{
    public int QuizId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public string LessonContent { get; set; } = string.Empty;
    public string TargetSkill { get; set; } = string.Empty;
    public ChallengeDifficulty Difficulty { get; set; }
    public ChallengeQuestionType QuestionType { get; set; }
}

public class AiQuizQuestionDraftDto
{
    public string QuestionText { get; set; } = string.Empty;
    public string Scenario { get; set; } = string.Empty;
    public List<string> Options { get; set; } = [];
    public int SuggestedCorrectOptionIndex { get; set; } = -1;
    public string Explanation { get; set; } = string.Empty;
    public string SuggestedAltText { get; set; } = string.Empty;
}

public class AiQuizFeedbackDto
{
    public string Summary { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = [];
    public List<string> WeakAreas { get; set; } = [];
    public List<string> SuggestedNextPractice { get; set; } = [];
}

public class StudentChallengeDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public ChallengeDifficulty Difficulty { get; set; }
    public int QuestionCount { get; set; }
}

public class StudentQuizDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SkillName { get; set; } = string.Empty;
    public List<StudentQuizQuestionDto> Questions { get; set; } = [];
}

public class StudentQuizQuestionDto
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string Scenario { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string ImageAltText { get; set; } = string.Empty;
    public int Order { get; set; }
    public List<StudentQuizOptionDto> Options { get; set; } = [];
}

public class StudentQuizOptionDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class StudentQuizAnswerDto
{
    public int QuestionId { get; set; }
    public int SelectedOptionId { get; set; }
}

public class QuizAttemptResultDto
{
    public int AttemptId { get; set; }
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public DateTime StartedAtUtc { get; set; }
    public DateTime CompletedAtUtc { get; set; }
    public int CorrectAnswers { get; set; }
    public int TotalQuestions { get; set; }
    public decimal ScorePercentage { get; set; }
    public QuizAttemptResult Result { get; set; }
    public bool Passed => Result == QuizAttemptResult.Pass;
}

public class StudentQuizSessionDto
{
    public int AttemptId { get; set; }
    public StudentQuizDto Quiz { get; set; } = new();
}

public class QuizAnswerReviewDto
{
    public int QuestionId { get; set; }
    public int Order { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string Scenario { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string ImageAltText { get; set; } = string.Empty;
    public string SelectedAnswer { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public string Explanation { get; set; } = string.Empty;
}

public class QuizAttemptReviewDto
{
    public QuizAttemptResultDto Result { get; set; } = new();
    public List<QuizAnswerReviewDto> Answers { get; set; } = [];
}

public class QuizAnalyticsDto
{
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public int Participants { get; set; }
    public int UniqueStudents { get; set; }
    public int CompletedAttempts { get; set; }
    public int InProgressAttempts { get; set; }
    public int PassAttempts { get; set; }
    public int NotPassAttempts { get; set; }
    public decimal? PassRate { get; set; }
    public decimal? AverageScore { get; set; }
    public int? HighestScore { get; set; }
    public int? LowestScore { get; set; }
    public int TotalQuestions { get; set; }
    public List<QuestionAnalyticsDto> Questions { get; set; } = [];
    public List<StudentQuizAnalyticsDto> Students { get; set; } = [];
}

public class QuestionAnalyticsDto
{
    public int QuestionId { get; set; }
    public int Order { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int Attempts { get; set; }
    public int Correct { get; set; }
    public int Incorrect => Attempts - Correct;
    public decimal? CorrectRate => Attempts == 0 ? null : Correct * 100m / Attempts;
}

public class StudentQuizAnalyticsDto
{
    public int UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int Attempts { get; set; }
    public int LatestScore { get; set; }
    public int BestScore { get; set; }
    public QuizAttemptResult LatestResult { get; set; }
    public DateTime CompletedAtUtc { get; set; }
}

public class CourseAnalyticsDto
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public int Enrollments { get; set; }
    public int Completed { get; set; }
    public int InProgress => Enrollments - Completed;
    public decimal? CompletionRate => Enrollments == 0 ? null : Completed * 100m / Enrollments;
    public int? FinalQuizId { get; set; }
    public decimal? FinalQuizPassRate { get; set; }
    public List<LessonAnalyticsDto> Lessons { get; set; } = [];
}

public class LessonAnalyticsDto
{
    public int LessonId { get; set; }
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public int CompletedStudents { get; set; }
    public int Enrollments { get; set; }
    public decimal? CompletionRate => Enrollments == 0 ? null : CompletedStudents * 100m / Enrollments;
}

public class MessageDto
{
    public int Id { get; set; }
    public ChatSender Sender { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ChatHistoryMessageDto
{
    public int Id { get; set; }
    public ChatSender Sender { get; set; }
    public string ViContent { get; set; } = string.Empty;
    public string EnContent { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ChatSessionDto
{
    public int Id { get; set; }
    public string TitleVi { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

public class MentorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Expertise { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string ShortBio { get; set; } = string.Empty;
}
