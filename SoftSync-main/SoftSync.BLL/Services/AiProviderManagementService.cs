using Microsoft.EntityFrameworkCore;
using SoftSync.BLL.Interfaces;
using SoftSync.Common.Dtos;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;

namespace SoftSync.BLL.Services;

public static class AiProviderRules
{
    public static string Endpoint(AiProviderType provider) => provider switch
    {
        AiProviderType.Gemini => "https://generativelanguage.googleapis.com/v1beta/openai/",
        AiProviderType.HuggingFace => "https://router.huggingface.co/v1/",
        AiProviderType.OpenAI => "https://api.openai.com/v1/",
        _ => throw new AiConfigurationException("Unsupported provider.")
    };
    public static void Validate(AiProviderEditDto input)
    {
        if (!Enum.IsDefined(input.Provider)) throw new AiConfigurationException("Unsupported provider.");
        // Exact official base URLs only: no userinfo/query/fragment, local/private hosts or custom proxies.
        if (!string.Equals(input.Endpoint.Trim().TrimEnd('/') + "/", Endpoint(input.Provider), StringComparison.Ordinal))
            throw new AiConfigurationException("Use the official endpoint for this provider.");
        if (string.IsNullOrWhiteSpace(input.DisplayName) || input.DisplayName.Trim().Length > 120)
            throw new AiConfigurationException("Display name is required (maximum 120 characters).");
        if (!ValidModel(input.Model) || (input.CvModel.Length > 0 && !ValidModel(input.CvModel)))
            throw new AiConfigurationException("Model is required (maximum 200 characters, no whitespace).");
        if (input.NewApiKey is not null && (input.NewApiKey.Length > 4096 || input.NewApiKey.Any(char.IsControl)))
            throw new AiConfigurationException("Invalid API key format.");
    }
    private static bool ValidModel(string value) => !string.IsNullOrWhiteSpace(value) && value.Trim().Length <= 200
        && !value.Trim().Any(char.IsWhiteSpace) && !value.Any(char.IsControl);
}

public sealed class AiProviderManagementService(IDbContextFactory<SoftSyncDbContext> contexts,
    IAiAdminAccess access, IAiSecretProtector protector, IAiProviderConnectionTester tester) : IAiProviderManagementService
{
    public async Task<IReadOnlyList<AiProviderDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        await access.RequireAdminAsync(cancellationToken);
        await using var db = await contexts.CreateDbContextAsync(cancellationToken);
        return (await db.AiProviderConfigurations.AsNoTracking().OrderBy(x => x.CreatedAt).ToListAsync(cancellationToken)).Select(Dto).ToList();
    }
    public async Task<AiProviderDto> SaveAsync(AiProviderEditDto input, CancellationToken cancellationToken = default)
    {
        var actor = await access.RequireAdminAsync(cancellationToken);
        AiProviderRules.Validate(input);
        await using var db = await contexts.CreateDbContextAsync(cancellationToken);
        var row = input.Id == Guid.Empty ? new AiProviderConfiguration { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow }
            : await FindAsync(db, input.Id, input.Revision, cancellationToken);
        var key = input.NewApiKey?.Trim();
        if (row.Provider != input.Provider && !string.IsNullOrWhiteSpace(row.EncryptedApiKey) && string.IsNullOrWhiteSpace(key))
            throw new AiConfigurationException("Enter a new key when changing provider.");
        if (!string.IsNullOrWhiteSpace(key)) row.EncryptedApiKey = protector.Protect(key);
        if (input.IsEnabled && string.IsNullOrWhiteSpace(row.EncryptedApiKey)) throw new AiConfigurationException("An enabled provider requires an API key.");
        if (row.IsDefault && !input.IsEnabled) throw new AiConfigurationException("Activate another provider before disabling the default.");
        row.Provider = input.Provider; row.DisplayName = input.DisplayName.Trim(); row.Model = input.Model.Trim();
        row.CvModel = input.CvModel.Trim(); row.Endpoint = AiProviderRules.Endpoint(input.Provider); row.IsEnabled = input.IsEnabled;
        Stamp(row, actor);
        if (input.Id == Guid.Empty) db.Add(row);
        await SaveChangesAsync(db, cancellationToken);
        return Dto(row);
    }
    public async Task SetActiveAsync(Guid id, Guid revision, CancellationToken cancellationToken = default)
    {
        var actor = await access.RequireAdminAsync(cancellationToken);
        await using var db = await contexts.CreateDbContextAsync(cancellationToken);
        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        // PostgreSQL transaction-scoped lock serializes competing default switches across app instances.
        await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(736921040)", cancellationToken);
        var target = await FindAsync(db, id, revision, cancellationToken);
        if (!target.IsEnabled || string.IsNullOrWhiteSpace(target.EncryptedApiKey)) throw new AiConfigurationException("Only enabled, configured providers can be active.");
        foreach (var old in await db.AiProviderConfigurations.Where(x => x.IsDefault && x.Id != id).ToListAsync(cancellationToken))
        { old.IsDefault = false; Stamp(old, actor); }
        // Clear first so the partial unique index is respected regardless of EF update ordering.
        await SaveChangesAsync(db, cancellationToken);
        target.IsDefault = true; Stamp(target, actor);
        await SaveChangesAsync(db, cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }
    public async Task SetEnabledAsync(Guid id, Guid revision, bool enabled, CancellationToken cancellationToken = default)
    {
        var actor = await access.RequireAdminAsync(cancellationToken);
        await using var db = await contexts.CreateDbContextAsync(cancellationToken);
        var row = await FindAsync(db, id, revision, cancellationToken);
        if (row.IsDefault && !enabled) throw new AiConfigurationException("Activate another provider before disabling the default.");
        if (enabled && string.IsNullOrWhiteSpace(row.EncryptedApiKey)) throw new AiConfigurationException("An enabled provider requires an API key.");
        row.IsEnabled = enabled; Stamp(row, actor); await SaveChangesAsync(db, cancellationToken);
    }
    public async Task<AiConnectionStatus> TestAsync(AiProviderEditDto input, CancellationToken cancellationToken = default)
    {
        await access.RequireAdminAsync(cancellationToken);
        AiProviderRules.Validate(input);
        var key = input.NewApiKey?.Trim();
        if (string.IsNullOrWhiteSpace(key) && input.Id != Guid.Empty)
        {
            await using var db = await contexts.CreateDbContextAsync(cancellationToken);
            var row = await FindAsync(db, input.Id, input.Revision, cancellationToken);
            if (row.Provider != input.Provider) throw new AiConfigurationException("Enter a new key when changing provider.");
            try { key = protector.Unprotect(row.EncryptedApiKey); }
            catch (System.Security.Cryptography.CryptographicException) { return AiConnectionStatus.NotConfigured; }
        }
        if (string.IsNullOrWhiteSpace(key)) return AiConnectionStatus.NotConfigured;
        return await tester.TestAsync(new() { Provider = input.Provider, Endpoint = AiProviderRules.Endpoint(input.Provider),
            Model = input.Model.Trim(), ApiKey = key, TimeoutSeconds = 15 }, cancellationToken);
    }
    private static void Stamp(AiProviderConfiguration row, int actor)
    { row.UpdatedAt = DateTime.UtcNow; row.UpdatedByUserId = actor; row.Revision = Guid.NewGuid(); }
    private static AiProviderDto Dto(AiProviderConfiguration x) => new(x.Id, x.Provider, x.DisplayName, x.Model, x.CvModel,
        x.Endpoint, !string.IsNullOrWhiteSpace(x.EncryptedApiKey), x.IsEnabled, x.IsDefault, x.CreatedAt, x.UpdatedAt, x.UpdatedByUserId, x.Revision);
    private static async Task<AiProviderConfiguration> FindAsync(SoftSyncDbContext db, Guid id, Guid revision, CancellationToken ct)
    {
        var row = await db.AiProviderConfigurations.SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AiConfigurationException("Configuration not found.");
        if (row.Revision != revision) throw new AiConfigurationException("Configuration changed. Reload and try again.");
        return row;
    }
    private static async Task SaveChangesAsync(SoftSyncDbContext db, CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { throw new AiConfigurationException("Configuration changed. Reload and try again."); }
        catch (DbUpdateException) { throw new AiConfigurationException("Configuration could not be saved. Reload and try again."); }
    }
}
