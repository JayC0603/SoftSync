using SoftSync.Common.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SoftSync.DAL.Entities;

/// <summary>
/// The application user / auth principal. Inherits <see cref="IdentityUser{TKey}"/>
/// with an <c>int</c> key so every existing <c>int UserId</c> FK stays intact.
/// Email, PhoneNumber, PasswordHash, etc. come from the Identity base type; the
/// domain profile fields (FullName/Age/Role/Goal) are merged in here.
/// </summary>
public class ApplicationUser : IdentityUser<int>
{
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public UserRole Role { get; set; }
    public Gender Gender { get; set; }
    [MaxLength(500)]
    public string Goal { get; set; } = string.Empty;
    /// <summary>Avatar image source. User uploads are stored as data URLs so they survive Render restarts.</summary>
    public string AvatarUrl { get; set; } = string.Empty;
    /// <summary>Total XP earned; the current level is derived from this via LevelSystem.</summary>
    public int ExperiencePoints { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ---- Settings: profile / display ----
    /// <summary>Public display name shown to others (falls back to FullName when empty).</summary>
    [MaxLength(60)]
    public string DisplayName { get; set; } = string.Empty;

    // ---- Settings: learning personalization (used by the AI to tune the roadmap) ----
    public LearningLevel CurrentLevel { get; set; }
    /// <summary>Target study minutes per day.</summary>
    public int DailyStudyMinutes { get; set; }
    /// <summary>Target study days per week (0–7).</summary>
    public int StudyDaysPerWeek { get; set; }
    public StudyTime PreferredStudyTime { get; set; }

    // ---- Settings: appearance ----
    /// <summary>Preferred UI language code ("en"/"vi"); empty means follow the browser.</summary>
    [MaxLength(5)]
    public string PreferredLanguage { get; set; } = string.Empty;
    public ThemePreference Theme { get; set; }
    /// <summary>Accessibility: reduce non-essential motion/animation.</summary>
    public bool ReduceMotion { get; set; }

    // Navigation properties
    public ICollection<UserSkillSelection> SkillSelections { get; set; } = new List<UserSkillSelection>();
    public ICollection<AssessmentResult> AssessmentResults { get; set; } = new List<AssessmentResult>();
    public ICollection<RoadmapItem> RoadmapItems { get; set; } = new List<RoadmapItem>();
    public ICollection<ProgressLog> ProgressLogs { get; set; } = new List<ProgressLog>();
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
}

/// <summary>
/// A short-lived numeric code sent over SMS or email for OTP login, phone
/// confirmation, or password reset. The code itself is stored hashed; expiry and
/// an attempt counter guard against brute force.
/// </summary>
public class VerificationCode
{
    [Key]
    public int Id { get; set; }
    public int? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    [Required, MaxLength(200)]
    public string CodeHash { get; set; } = string.Empty;
    public VerificationPurpose Purpose { get; set; }
    [Required, MaxLength(256)]
    public string Destination { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public int AttemptCount { get; set; }
    public bool Consumed { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class Skill
{
    [Key]
    public int Id { get; set; }
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    [MaxLength(50)]
    public string IconName { get; set; } = string.Empty;

    public ICollection<AssessmentQuestion> Questions { get; set; } = new List<AssessmentQuestion>();
    public ICollection<CaseStudy> CaseStudies { get; set; } = new List<CaseStudy>();
}

public class UserSkillSelection
{
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public int SkillId { get; set; }
    public Skill Skill { get; set; } = null!;
}

public class AssessmentQuestion
{
    [Key]
    public int Id { get; set; }
    public int SkillId { get; set; }
    public Skill Skill { get; set; } = null!;
    [Required]
    public string QuestionText { get; set; } = string.Empty;
    /// <summary>Vietnamese version of <see cref="QuestionText"/> (falls back to English if empty).</summary>
    public string QuestionTextVi { get; set; } = string.Empty;
    public QuestionType Type { get; set; }

    public ICollection<AssessmentOption> Options { get; set; } = new List<AssessmentOption>();
}

public class AssessmentOption
{
    [Key]
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public AssessmentQuestion Question { get; set; } = null!;
    [Required]
    public string OptionText { get; set; } = string.Empty;
    /// <summary>Vietnamese version of <see cref="OptionText"/> (falls back to English if empty).</summary>
    public string OptionTextVi { get; set; } = string.Empty;
    public int ScoreValue { get; set; }
}

public class AssessmentResult
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public int SkillId { get; set; }
    public Skill Skill { get; set; } = null!;
    public int Score { get; set; }
    public AssessmentLevel Level { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class RoadmapItem
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public int WeekNumber { get; set; }
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    /// <summary>Set when the learner reaches the end of the linked video.</summary>
    public DateTime? VideoCompletedAtUtc { get; set; }
    public DateTime? ScriptCompletedAtUtc { get; set; }
    public DateTime? SummaryCompletedAtUtc { get; set; }
    /// <summary>Set when the learner passes the linked quiz/practice activity.</summary>
    public DateTime? PracticeCompletedAtUtc { get; set; }
    /// <summary>Set when the learner completes the applied scenario step.</summary>
    public DateTime? ScenarioCompletedAtUtc { get; set; }
    /// <summary>The learner's reflection after completing the weekly activities.</summary>
    [MaxLength(2000)]
    public string? ReflectionText { get; set; }
    /// <summary>Set when a non-empty reflection is saved.</summary>
    public DateTime? ReflectionCompletedAtUtc { get; set; }
    /// <summary>Last unlocked learning step, used to resume the lesson after signing in again.</summary>
    [MaxLength(20)]
    public string LastLearningStep { get; set; } = "video";
    public bool IsCompleted { get; set; }
}

public class CaseStudy
{
    [Key]
    public int Id { get; set; }
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    [Required]
    public string Scenario { get; set; } = string.Empty;
    public int SkillId { get; set; }
    public Skill Skill { get; set; } = null!;

    public ICollection<CaseStudyOption> Options { get; set; } = new List<CaseStudyOption>();
}

public class CaseStudyOption
{
    [Key]
    public int Id { get; set; }
    public int CaseStudyId { get; set; }
    public CaseStudy CaseStudy { get; set; } = null!;
    [Required]
    public string OptionText { get; set; } = string.Empty;
    public bool IsRecommended { get; set; }
    public string Feedback { get; set; } = string.Empty;
}

public class ProgressLog
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public int SkillId { get; set; }
    public Skill Skill { get; set; } = null!;
    public int PercentComplete { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class ChatMessage
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public ChatSender Sender { get; set; }
    [Required]
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Mentor
{
    [Key]
    public int Id { get; set; }
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [Required, MaxLength(100)]
    public string Expertise { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string ShortBio { get; set; } = string.Empty;
}

public class DataProtectionKey
{
    [Key]
    public int Id { get; set; }
    [MaxLength(200)]
    public string FriendlyName { get; set; } = string.Empty;
    public string Xml { get; set; } = string.Empty;
}
