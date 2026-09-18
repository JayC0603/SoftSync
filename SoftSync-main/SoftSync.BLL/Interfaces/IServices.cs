using SoftSync.Common.Dtos;
using SoftSync.Common.Enums;

namespace SoftSync.BLL.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto> CreateUserAsync(UserDto userDto);
    Task UpdateProfileAsync(int userId, UserDto userDto);
    Task SetAvatarAsync(int userId, string avatarUrl);
    Task AddExperienceAsync(int userId, int amount);
    Task AddSkillSelectionsAsync(int userId, List<int> skillIds);
    /// <summary>Save the Settings page fields (account/display, learning, appearance).</summary>
    Task UpdateSettingsAsync(int userId, UserDto userDto);
    /// <summary>Skill ids the user currently has selected (for the learning tab).</summary>
    Task<List<int>> GetSelectedSkillIdsAsync(int userId);
}

public interface ISkillService
{
    Task<IEnumerable<SkillDto>> GetAllSkillsAsync();
}

public interface IAssessmentService
{
    Task<IEnumerable<AssessmentQuestionDto>> GetAssessmentQuestionsAsync(int userId);
    Task SubmitAssessmentAsync(int userId, List<UserAnswerDto> answers);
    Task<IEnumerable<AssessmentResultDto>> GetLatestResultsAsync(int authenticatedUserId, int ownerUserId);
    Task<IReadOnlyList<AssessmentAttemptDto>> GetHistoryAsync(int authenticatedUserId, int ownerUserId);
}

public class AssessmentQuestionDto // Local DTO for BLL to UI
{
    public int Id { get; set; }
    /// <summary>English text (fallback when no localized version is available).</summary>
    public string QuestionText { get; set; } = string.Empty;
    /// <summary>Vietnamese text; empty means fall back to <see cref="QuestionText"/>.</summary>
    public string QuestionTextVi { get; set; } = string.Empty;
    public int SkillId { get; set; }
    public QuestionType Type { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string SkillNameVi { get; set; } = string.Empty;
    public List<AssessmentOptionDto> Options { get; set; } = new();
}

public class AssessmentOptionDto
{
    public int Id { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public string OptionTextVi { get; set; } = string.Empty;
}

public interface IRoadmapService
{
    Task<RoadmapDto> GetUserRoadmapAsync(int authenticatedUserId, int ownerUserId);
    Task<bool> MarkVideoCompleteAsync(int itemId, int userId);
    Task<bool> MarkCompleteAsync(int itemId, int userId);
    Task<bool> MarkScenarioCompleteAsync(int itemId, int userId);
    Task<bool> SaveReflectionAsync(int itemId, int userId, string reflectionText);
    Task<bool> SaveReflectionDraftAsync(int itemId, int userId, string reflectionText);
    Task<bool> SaveLearningStepAsync(int itemId, int userId, string step);
    Task<bool> SaveQuizAttemptAsync(int itemId, int userId, RoadmapQuizAttemptDto attempt);
    Task<IReadOnlyList<RoadmapQuizAttemptDto>> GetQuizHistoryAsync(int itemId, int userId);
    Task<bool> SaveRoleplayAttemptAsync(int itemId, int userId, RoadmapRoleplayAttemptDto attempt);
}

public interface IProgressService
{
    Task<IEnumerable<ProgressDto>> GetUserProgressAsync(int userId);
}

public interface ICourseService
{
    Task<IReadOnlyList<CourseDto>> GetPublishedAsync(int authenticatedUserId);
    Task<IReadOnlyList<CourseDto>> GetManagedAsync(int authenticatedUserId, bool isAdmin);
    Task<CourseDto?> GetForManagementAsync(int id, int authenticatedUserId, bool isAdmin);
    Task<int?> SaveCourseAsync(CourseDto input, int authenticatedUserId, bool isAdmin);
    Task<bool> PublishCourseAsync(int id, int authenticatedUserId, bool isAdmin);
    Task<int?> SaveLessonAsync(CourseLessonDto input, int authenticatedUserId, bool isAdmin);
    Task<bool> EnrollAsync(int courseId, int authenticatedUserId);
    Task<bool> CompleteLessonAsync(int lessonId, int authenticatedUserId);
    Task<CourseAnalyticsDto?> GetAnalyticsAsync(int courseId, int authenticatedUserId, bool isAdmin);
}

public interface IChallengeService
{
    Task<IReadOnlyList<StudentChallengeDto>> GetPublishedAsync();
    Task<StudentQuizDto?> GetPublishedQuizAsync(int quizId);
    Task<StudentQuizSessionDto?> StartAttemptAsync(int quizId, int authenticatedUserId);
    Task<QuizAttemptResultDto?> SubmitAsync(int attemptId, int authenticatedUserId, IReadOnlyCollection<StudentQuizAnswerDto> answers);
    Task<QuizAttemptResultDto?> GetAttemptAsync(int attemptId, int authenticatedUserId);
    Task<IReadOnlyList<QuizAttemptResultDto>> GetHistoryAsync(int authenticatedUserId);
    Task<QuizAttemptReviewDto?> GetReviewAsync(int attemptId, int authenticatedUserId, bool isTeacher, bool isAdmin);
    Task<IReadOnlyList<QuizAttemptResultDto>> GetQuizResultsAsync(int quizId, int authenticatedUserId, bool isAdmin);
    Task<QuizAnalyticsDto?> GetAnalyticsAsync(int quizId, int authenticatedUserId, bool isAdmin);
    Task<ChallengeQuizDto?> GetForManagementAsync(int id, int authenticatedUserId, bool isAdmin);
    Task<int?> SaveQuizAsync(ChallengeQuizDto input, int authenticatedUserId, bool isAdmin);
    Task<bool> SaveQuestionAsync(int quizId, ChallengeQuestionEditorDto input, int authenticatedUserId, bool isAdmin);
    Task<bool> PublishAsync(int quizId, int authenticatedUserId, bool isAdmin);
    Task<AiQuizQuestionDraftDto?> GenerateQuestionDraftAsync(AiQuizDraftRequestDto input, int authenticatedUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<string?> ExplainCompletedAnswerAsync(int attemptId, int questionId, int authenticatedUserId, CancellationToken cancellationToken = default);
    Task<AiQuizFeedbackDto?> GetPersonalizedFeedbackAsync(int attemptId, int authenticatedUserId, CancellationToken cancellationToken = default);
}

public interface IAiQuizService
{
    Task<AiQuizQuestionDraftDto?> GenerateQuestionDraftAsync(AiQuizDraftRequestDto input, CancellationToken cancellationToken = default);
    Task<string?> ExplainAnswerAsync(QuizAnswerReviewDto answer, CancellationToken cancellationToken = default);
    Task<AiQuizFeedbackDto?> GenerateFeedbackAsync(QuizAttemptReviewDto review, CancellationToken cancellationToken = default);
}

public interface IRoleplayService
{
    IReadOnlyList<RoleplayScenarioDto> GetScenarios();
    Task<RoleplaySessionDto?> StartAsync(int authenticatedUserId, int roadmapItemId, int scenarioId, bool vietnamese);
    Task<RoleplaySessionDto?> GetAsync(int authenticatedUserId, int sessionId);
    Task<IReadOnlyList<RoleplaySessionDto>> GetHistoryAsync(int authenticatedUserId, int roadmapItemId);
    Task<RoleplaySessionDto?> SendAsync(int authenticatedUserId, int sessionId, string message, bool vietnamese);
}

public interface IRoleplayAiService
{
    Task<string?> ReplyAsync(RoleplayScenarioDto scenario, IReadOnlyList<RoleplayTurnDto> turns, bool vietnamese);
    Task<RoleplayAiEvaluationDto?> EvaluateAsync(RoleplayScenarioDto scenario, IReadOnlyList<RoleplayTurnDto> turns, bool vietnamese);
}

public interface ICaseStudyService
{
    Task<IEnumerable<CaseStudyDto>> GetCaseStudiesBySkillAsync(int skillId);
    Task<CaseStudyDto?> GetCaseStudyByIdAsync(int id);
}

public interface IMentorService
{
    Task<IEnumerable<MentorDto>> GetAllAsync();
}

// AI Interfaces (Specific names requested by user)
public interface IAiAssessmentService
{
    Task<AssessmentResultDto> EvaluateAsync(List<UserAnswerDto> answers);
}

public interface IAiAssistantService
{
    Task<string> GetReplyAsync(string userMessage, int userId);
}

public interface IChatHistoryService
{
    Task<IReadOnlyList<ChatSessionDto>> GetSessionsAsync(int userId);
    Task<IReadOnlyList<ChatHistoryMessageDto>> GetHistoryAsync(int userId, int sessionId = 0);
    Task<int> SaveAsync(int userId, int sessionId, ChatSender sender, string viContent, string enContent);
}

public interface IAiRoadmapService
{
    Task<RoadmapDto> GenerateRoadmapAsync(int userId, List<string> weakSkills);
}
