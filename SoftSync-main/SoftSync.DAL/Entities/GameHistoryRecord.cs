namespace SoftSync.DAL.Entities;

/// <summary>A completed mini-game session owned by one SoftSync account.</summary>
public sealed class GameHistoryRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public string GameKey { get; set; } = string.Empty;
    public string ClientEntryId { get; set; } = string.Empty;
    public DateTime PlayedAtUtc { get; set; }
    public string DataJson { get; set; } = "{}";
}
