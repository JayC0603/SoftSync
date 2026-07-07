namespace SoftSync.Common;

/// <summary>
/// Derives a user's level and progress from their total experience points (XP).
/// Uses a simple escalating curve: each level N needs 100 * N XP to clear, so
/// cumulative thresholds are 0, 100, 300, 600, 1000, ... (level 1 is the start).
/// </summary>
public static class LevelSystem
{
    /// <summary>XP awarded for finishing an assessment.</summary>
    public const int AssessmentXp = 50;
    /// <summary>XP awarded for completing a roadmap item.</summary>
    public const int RoadmapItemXp = 30;

    /// <summary>Current level (1-based) for the given total XP.</summary>
    public static int GetLevel(int totalXp)
    {
        var level = 1;
        while (totalXp >= CumulativeXpForLevel(level + 1))
        {
            level++;
        }
        return level;
    }

    /// <summary>Total XP required to have reached the start of <paramref name="level"/>.</summary>
    public static int CumulativeXpForLevel(int level)
    {
        // Sum of 100 * k for k = 1..(level-1) => 100 * (level-1) * level / 2.
        var n = level - 1;
        return 100 * n * (n + 1) / 2;
    }

    /// <summary>XP earned within the current level.</summary>
    public static int XpIntoCurrentLevel(int totalXp)
    {
        var level = GetLevel(totalXp);
        return totalXp - CumulativeXpForLevel(level);
    }

    /// <summary>XP needed to advance from the current level to the next.</summary>
    public static int XpForNextLevel(int totalXp)
    {
        var level = GetLevel(totalXp);
        return CumulativeXpForLevel(level + 1) - CumulativeXpForLevel(level);
    }

    /// <summary>Progress toward the next level as a 0-100 percentage.</summary>
    public static int ProgressPercent(int totalXp)
    {
        var needed = XpForNextLevel(totalXp);
        if (needed <= 0) return 0;
        return (int)Math.Round(100.0 * XpIntoCurrentLevel(totalXp) / needed);
    }
}
