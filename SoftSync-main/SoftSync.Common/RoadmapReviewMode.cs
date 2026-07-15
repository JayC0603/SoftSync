namespace SoftSync.Common;

/// <summary>
/// Temporarily exposes the complete active curriculum to content editors.
/// Set Enabled to false to restore the personalized learner presentation.
/// </summary>
public static class RoadmapReviewMode
{
    public static bool Enabled { get; set; } = false;
}
