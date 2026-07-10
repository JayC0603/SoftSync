using SoftSync.Common.Enums;

namespace SoftSync.Common;

/// <summary>
/// Maps a per-skill raw assessment score to a result band.
///
/// Each skill has 8 questions, each option scored 1–4, so a skill's raw score
/// ranges 8–32. Bands (inclusive):
///   8–14  Passive     (bị động)
///   15–20 Developing  (đang phát triển)
///   21–26 Proactive   (chủ động)
///   27–32 Mastery     (làm chủ)
///
/// The <see cref="AssessmentResult.Score"/> column stores this raw 8–32 value
/// (previously a 0–100 percentage).
/// </summary>
public static class AssessmentScoring
{
    /// <summary>Questions asked per skill (matches QuizSeedData.QuestionsPerSkill).</summary>
    public const int QuestionsPerSkill = 8;

    /// <summary>Lowest possible raw score for a fully-answered skill (8 × 1).</summary>
    public const int MinScore = 8;

    /// <summary>Highest possible raw score for a fully-answered skill (8 × 4).</summary>
    public const int MaxScore = 32;

    /// <summary>Classifies a raw 8–32 score into its band.</summary>
    public static AssessmentLevel BandFor(int rawScore) => rawScore switch
    {
        <= 14 => AssessmentLevel.Passive,
        <= 20 => AssessmentLevel.Developing,
        <= 26 => AssessmentLevel.Proactive,
        _ => AssessmentLevel.Mastery
    };
}
