using SoftSync.Common.Dtos;
namespace SoftSync.DAL.Entities;

public sealed class AiProviderConfiguration
{
    public Guid Id { get; set; }
    public AiProviderType Provider { get; set; }
    public string DisplayName { get; set; } = "";
    public string Model { get; set; } = "";
    public string CvModel { get; set; } = "";
    public string Endpoint { get; set; } = "";
    public string EncryptedApiKey { get; set; } = "";
    public bool IsEnabled { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int? UpdatedByUserId { get; set; }
    public Guid Revision { get; set; }
}
