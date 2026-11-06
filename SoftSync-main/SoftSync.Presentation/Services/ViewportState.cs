namespace SoftSync.Presentation.Services;

// Scoped to one circuit: subsequent routes render the known layout immediately.
// This contains presentation state only, never learner or course data.
public sealed class ViewportState(bool isDesktop = false)
{
    public bool IsDesktop { get; set; } = isDesktop;
}
