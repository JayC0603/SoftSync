using SoftSync.Common.Dtos;
namespace SoftSync.BLL.Interfaces;

public interface IAiSecretProtector
{
    string Protect(string value);
    string Unprotect(string value);
}
public interface IAiAdminAccess
{
    Task<int> RequireAdminAsync(CancellationToken cancellationToken = default);
}
// Internal server transport model, not a UI DTO. ToString never prints the key.
public sealed class ResolvedAiProviderConfiguration
{
    public AiProviderType Provider { get; init; }
    public string Endpoint { get; init; } = "";
    public string Model { get; init; } = "";
    [System.Text.Json.Serialization.JsonIgnore]
    public string ApiKey { get; init; } = "";
    public int TimeoutSeconds { get; init; } = 45;
    public override string ToString() => "Resolved AI configuration (secret redacted)";
}
public interface IAiProviderConfigurationResolver
{
    Task<ResolvedAiProviderConfiguration?> ResolveAsync(string? task = null, CancellationToken cancellationToken = default);
}
public interface IAiProviderConnectionTester
{
    Task<AiConnectionStatus> TestAsync(ResolvedAiProviderConfiguration configuration, CancellationToken cancellationToken = default);
}
public sealed class AiConfigurationException(string message) : Exception(message);

public interface IAiProviderManagementService
{
    Task<IReadOnlyList<AiProviderDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<AiProviderDto> SaveAsync(AiProviderEditDto input, CancellationToken cancellationToken = default);
    Task SetActiveAsync(Guid id, Guid revision, CancellationToken cancellationToken = default);
    Task SetEnabledAsync(Guid id, Guid revision, bool enabled, CancellationToken cancellationToken = default);
    Task<AiConnectionStatus> TestAsync(AiProviderEditDto input, CancellationToken cancellationToken = default);
}
