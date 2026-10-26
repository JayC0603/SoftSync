using System.ComponentModel.DataAnnotations;

namespace SoftSync.DAL.Entities;

public sealed class AiProviderSetting
{
    [Key] public int Id { get; set; } = 1;
    public bool Enabled { get; set; }
    [MaxLength(100)] public string ChatModel { get; set; } = "gemini-3.5-flash-lite";
    [MaxLength(100)] public string CvModel { get; set; } = "gemini-3.5-flash-lite";
    [MaxLength(100)] public string EvaluationModel { get; set; } = "gemini-3.5-flash-lite";
    public int Version { get; set; } = 1;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public int? UpdatedByUserId { get; set; }
}

public sealed class AiApiCredential
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(80)] public string Name { get; set; } = string.Empty;
    [Required] public string ProtectedKey { get; set; } = string.Empty;
    [MaxLength(12)] public string Fingerprint { get; set; } = string.Empty;
    [MaxLength(80)] public string QuotaGroup { get; set; } = string.Empty;
    public int Priority { get; set; }
    public bool Enabled { get; set; } = true;
    public DateTime? CooldownUntilUtc { get; set; }
    [MaxLength(40)] public string LastStatus { get; set; } = "Untested";
    [MaxLength(200)] public string LastMessage { get; set; } = string.Empty;
    public DateTime? LastUsedAtUtc { get; set; }
    public DateTime? LastCheckedAtUtc { get; set; }
}

public sealed class AiOperationRecord
{
    [Key] public Guid Id { get; set; }
    [Required, MaxLength(40)] public string Task { get; set; } = string.Empty;
    [MaxLength(100)] public string Model { get; set; } = string.Empty;
    [MaxLength(40)] public string Status { get; set; } = string.Empty;
    public int AttemptCount { get; set; }
    public int? CredentialId { get; set; }
    public long? InputTokens { get; set; }
    public long? OutputTokens { get; set; }
    public long DurationMs { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class SystemMetricMinute
{
    public DateTime MinuteStartUtc { get; set; }
    [Required, MaxLength(100)] public string InstanceId { get; set; } = string.Empty;
    public double? AverageCpuPercent { get; set; }
    public double? PeakCpuPercent { get; set; }
    public long? PeakWorkingSetBytes { get; set; }
    public int CpuSampleCount { get; set; }
    public long RequestCount { get; set; }
    public long ErrorCount { get; set; }
    public double RequestDurationTotalMs { get; set; }
    public string LatencyBucketsJson { get; set; } = "[0,0,0,0,0,0,0]";
}

public sealed class LearningActivitySession
{
    [Key] public Guid Id { get; set; }
    public int UserId { get; set; }
    [Required, MaxLength(80)] public string ActivityType { get; set; } = string.Empty;
    [MaxLength(120)] public string ResourceId { get; set; } = string.Empty;
    public DateTime StartedAtUtc { get; set; }
    public DateTime LastHeartbeatAtUtc { get; set; }
    public DateTime? EndedAtUtc { get; set; }
    public int MeasuredSeconds { get; set; }
}

public sealed class LearningHeartbeat
{
    [Key] public long Id { get; set; }
    public Guid SessionId { get; set; }
    public int Sequence { get; set; }
    public DateTime FromUtc { get; set; }
    public DateTime ToUtc { get; set; }
    public DateTime AcceptedAtUtc { get; set; }
    public int CreditedSeconds { get; set; }
}
