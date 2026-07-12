using SoftSync.Common.Enums;

namespace SoftSync.Presentation.Services;

/// <summary>
/// Resolves the SYNCY mascot image for a user. Two variants live under
/// wwwroot/images: syncy-male.png (has a mohawk fin) and syncy-female.png
/// (has a starfish hair-clip + eyelashes). Unspecified gender falls back to the
/// male variant. If the image file is missing, the UI shows a 🐟 emoji instead.
/// </summary>
public static class Mascot
{
    public const string Emoji = "🐟";

    public static string ImageFor(Gender gender) => gender switch
    {
        Gender.Female => "/images/syncy-fish.png",
        _ => "/images/syncy-fish.png"
    };
}
