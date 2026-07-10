namespace SoftSync.Presentation.Services;

public sealed class UserProfileState
{
    public event Action<string>? AvatarChanged;
    public event Action<string>? DisplayNameChanged;

    public void NotifyAvatarChanged(string avatarUrl) => AvatarChanged?.Invoke(avatarUrl);

    public void NotifyDisplayNameChanged(string displayName) => DisplayNameChanged?.Invoke(displayName);
}
