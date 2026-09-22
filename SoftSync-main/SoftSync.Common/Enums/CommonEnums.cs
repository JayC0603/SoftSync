namespace SoftSync.Common.Enums;

public enum SkillType
{
    Communication,
    Teamwork,
    TimeManagement,
    CriticalThinking,
    ProblemSolving,
    EmotionalManagement,
    Adaptability
}

/// <summary>
/// Result band for a single skill, from a raw score of 8–32 (8 questions × 1–4).
/// Bands: 8–14 Passive, 15–20 Developing, 21–26 Proactive, 27–32 Mastery.
/// </summary>
public enum AssessmentLevel
{
    Passive,
    Developing,
    Proactive,
    Mastery
}

public enum ChatSender
{
    User,
    Assistant
}

public enum UserRole
{
    Student,
    Teacher,
    Admin
}

public enum Gender
{
    Unspecified,
    Male,
    Female
}

public enum QuestionType
{
    MultipleChoice,
    Scenario
}

public enum RoadmapActivityType
{
    VideoLesson,
    LessonScript,
    LessonSummary,
    Quiz,
    Roleplay,
    Reflection
}

public enum RoadmapContentOrigin
{
    SoftSyncCurated,
    AiPersonalized
}

public enum RoleplaySessionStatus
{
    InProgress,
    Completed,
    Failed
}

public enum RoleplaySpeaker
{
    Persona,
    Learner
}

public enum CourseStatus { Draft, Published, Archived }
public enum ChallengeDifficulty { Beginner, Intermediate, Advanced }
public enum ChallengeQuestionType { Knowledge, Scenario }
public enum QuizAttemptResult { NotPass, Pass }

/// <summary>Self-reported current level, used by the AI to tune the roadmap.</summary>
public enum LearningLevel
{
    Unspecified,
    Beginner,
    Intermediate,
    Advanced
}

/// <summary>Preferred time of day to study.</summary>
public enum StudyTime
{
    Unspecified,
    Morning,
    Afternoon,
    Evening,
    Night
}

/// <summary>UI theme preference (System follows the OS setting).</summary>
public enum ThemePreference
{
    System,
    Light,
    Dark
}

/// <summary>What a <c>VerificationCode</c> is issued for.</summary>
public enum VerificationPurpose
{
    PhoneLoginOtp,
    PhoneNumberConfirmation,
    PasswordResetSms,
    PasswordResetEmail
}
