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

public enum AssessmentLevel
{
    Weak,
    Average,
    Good
}

public enum ChatSender
{
    User,
    Assistant
}

public enum UserRole
{
    Student
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

/// <summary>What a <c>VerificationCode</c> is issued for.</summary>
public enum VerificationPurpose
{
    PhoneLoginOtp,
    PhoneNumberConfirmation,
    PasswordResetSms,
    PasswordResetEmail
}
