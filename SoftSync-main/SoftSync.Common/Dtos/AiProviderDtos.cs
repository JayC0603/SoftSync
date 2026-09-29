namespace SoftSync.Common.Dtos;

public enum AiProviderType { HuggingFace, Gemini, OpenAI }

// Read model: never contains secret material.
public sealed record AiProviderDto(Guid Id, AiProviderType Provider, string DisplayName,
    string Model, string CvModel, string Endpoint, bool HasApiKey, bool IsEnabled,
    bool IsDefault, DateTime CreatedAt, DateTime UpdatedAt, int? UpdatedByUserId, Guid Revision);

public sealed class AiProviderEditDto
{
    public Guid Id { get; set; }
    public Guid Revision { get; set; }
    public AiProviderType Provider { get; set; } = AiProviderType.Gemini;
    public string DisplayName { get; set; } = "";
    public string Model { get; set; } = "";
    public string CvModel { get; set; } = "";
    public string Endpoint { get; set; } = "https://generativelanguage.googleapis.com/v1beta/openai/";
    public string? NewApiKey { get; set; }
    public bool IsEnabled { get; set; } = true;
}

public enum AiConnectionStatus { Success, NotConfigured, AuthenticationFailed, ModelUnavailable, RateLimited, Unavailable, Timeout, InvalidResponse }
