using System.ComponentModel.DataAnnotations;

namespace SoftSync.DAL.Entities;

public sealed class CvDocumentRecord
{
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    [MaxLength(255)] public string Name { get; set; } = string.Empty;
    [MaxLength(3)] public string Extension { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    [MaxLength(40_000)] public string Text { get; set; } = string.Empty;
    public byte[] OriginalFile { get; set; } = [];
    public string? ExtractionJson { get; set; }
}

public sealed class CvReviewRecord
{
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public Guid? DocumentId { get; set; }
    public CvDocumentRecord? Document { get; set; }
    [MaxLength(255)] public string Name { get; set; } = string.Empty;
    [MaxLength(255)] public string Role { get; set; } = string.Empty;
    [MaxLength(20_000)] public string JobDescription { get; set; } = string.Empty;
    public string ResultJson { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
