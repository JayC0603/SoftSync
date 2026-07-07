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

    // Derived from ExperiencePoints — not stored.
    public int Level => LevelSystem.GetLevel(ExperiencePoints);
    public int LevelProgressPercent => LevelSystem.ProgressPercent(ExperiencePoints);
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
}

public class RoadmapDto
{
    public int UserId { get; set; }
    public List<RoadmapItemDto> Items { get; set; } = new();
}

public class RoadmapItemDto
{
    public int Id { get; set; }
    public int WeekNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
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

public class MessageDto
{
    public int Id { get; set; }
    public ChatSender Sender { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class MentorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Expertise { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string ShortBio { get; set; } = string.Empty;
}
