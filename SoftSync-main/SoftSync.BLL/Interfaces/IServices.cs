using SoftSync.Common.Dtos;

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
    Task<IEnumerable<AssessmentResultDto>> GetLatestResultsAsync(int userId);
}

public class AssessmentQuestionDto // Local DTO for BLL to UI
{
    public int Id { get; set; }
    /// <summary>English text (fallback when no localized version is available).</summary>
    public string QuestionText { get; set; } = string.Empty;
    /// <summary>Vietnamese text; empty means fall back to <see cref="QuestionText"/>.</summary>
    public string QuestionTextVi { get; set; } = string.Empty;
    public int SkillId { get; set; }
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
    Task<RoadmapDto> GetUserRoadmapAsync(int userId);
    Task MarkCompleteAsync(int itemId);
}

public interface IProgressService
{
    Task<IEnumerable<ProgressDto>> GetUserProgressAsync(int userId);
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

public interface IAiRoadmapService
{
    Task<RoadmapDto> GenerateRoadmapAsync(int userId, List<string> weakSkills);
}
