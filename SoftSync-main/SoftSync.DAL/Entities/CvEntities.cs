namespace SoftSync.DAL.Entities;

public sealed class CvDocument
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public string OriginalFileName { get; set; } = "";
    public string ExtractedText { get; set; } = "";
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<CvAnalysis> Analyses { get; set; } = [];
}
public sealed class CvAnalysis
{
    public int Id { get; set; }
    public int CvDocumentId { get; set; }
    public CvDocument Document { get; set; } = null!;
    public string ResultJson { get; set; } = "";
    public string Model { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
