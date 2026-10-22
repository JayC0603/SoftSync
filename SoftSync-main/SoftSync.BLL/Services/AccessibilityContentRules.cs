namespace SoftSync.BLL.Services;

/// <summary>Deterministic content checks shared by teacher workflows and tests.</summary>
public static class AccessibilityContentRules
{
    public static bool HasMeaningfulAltText(string? imageUrl, string? altText)
    {
        if (string.IsNullOrWhiteSpace(imageUrl)) return true;
        var value = altText?.Trim();
        return !string.IsNullOrWhiteSpace(value) && value.Length <= 300;
    }

    public static bool HasAccessibleVideoText(string? videoUrl, string? captionUrl, string? transcript)
    {
        if (string.IsNullOrWhiteSpace(videoUrl)) return true;
        return !string.IsNullOrWhiteSpace(captionUrl) || !string.IsNullOrWhiteSpace(transcript);
    }
}
