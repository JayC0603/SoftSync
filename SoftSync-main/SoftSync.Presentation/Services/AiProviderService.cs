using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using SoftSync.DAL.Data;
using SoftSync.DAL.Entities;

namespace SoftSync.Presentation.Services;

public sealed record AiCredentialView(int Id, string Name, string Fingerprint, string QuotaGroup,
    int Priority, bool Enabled, string Status, string LastMessage, DateTime? CooldownUntilUtc,
    DateTime? LastUsedAtUtc, DateTime? LastCheckedAtUtc);
public sealed record AiProviderSettingsView(bool Enabled, string ChatModel, string CvModel,
    string EvaluationModel, int Version, IReadOnlyList<AiCredentialView> Credentials);
public sealed class AiCredentialInput(int id, string name, string key, string quotaGroup, int priority, bool enabled)
{
    public int Id { get; set; } = id;
    public string Name { get; set; } = name;
    public string Key { get; set; } = key;
    public string QuotaGroup { get; set; } = quotaGroup;
    public int Priority { get; set; } = priority;
    public bool Enabled { get; set; } = enabled;
    public string Fingerprint { get; set; } = "";
    public string Status { get; set; } = "Untested";
    public string LastMessage { get; set; } = "";
    public DateTime? CooldownUntilUtc { get; set; }
}
public sealed record AiSettingsInput(bool Enabled, string ChatModel, string CvModel,
    string EvaluationModel, List<AiCredentialInput> Credentials);
public sealed record AiModelView(string Name, string DisplayName, bool SupportsGenerateContent);
public sealed record GeminiResult(string Text, string Model, int CredentialId, int AttemptCount,
    long? InputTokens, long? OutputTokens, long DurationMs);

public sealed class AiProviderService(
    SoftSyncDbContext db,
    IDataProtectionProvider protectionProvider,
    IHttpClientFactory clients,
    ILogger<AiProviderService> logger)
{
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(42);
    private static readonly TimeSpan OperationTimeout = TimeSpan.FromSeconds(90);
    private readonly IDataProtector protector = protectionProvider.CreateProtector("SoftSync.AiCredential.v1");

    public async Task<AiProviderSettingsView> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await db.AiProviderSettings.AsNoTracking().SingleOrDefaultAsync(x => x.Id == 1, cancellationToken);
        var credentials = await db.AiApiCredentials.AsNoTracking().OrderBy(x => x.Priority).ThenBy(x => x.Id)
            .Select(x => new AiCredentialView(x.Id, x.Name, "••••" + x.Fingerprint, x.QuotaGroup,
                x.Priority, x.Enabled, x.LastStatus, x.LastMessage, x.CooldownUntilUtc, x.LastUsedAtUtc, x.LastCheckedAtUtc))
            .ToListAsync(cancellationToken);
        return new(settings?.Enabled ?? false, settings?.ChatModel ?? "gemini-3.5-flash-lite",
            settings?.CvModel ?? "gemini-3.5-flash-lite", settings?.EvaluationModel ?? "gemini-3.5-flash-lite",
            settings?.Version ?? 0, credentials);
    }

    public async Task SaveAsync(AiSettingsInput input, int adminUserId, CancellationToken cancellationToken = default)
    {
        if (input.Credentials is null || input.Credentials.Count > 3)
            throw new InvalidOperationException("Thêm tối đa 3 API key.");
        ValidateModel(input.ChatModel); ValidateModel(input.CvModel); ValidateModel(input.EvaluationModel);
        if (input.Credentials.Count(x => x.Enabled) > 3) throw new InvalidOperationException("Chỉ bật tối đa 3 API key.");

        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? transaction = null;
        if (db.Database.IsRelational()) transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var settings = await db.AiProviderSettings.SingleOrDefaultAsync(x => x.Id == 1, cancellationToken);
        if (settings is null)
        {
            settings = new AiProviderSetting { Id = 1 };
            db.AiProviderSettings.Add(settings);
        }
        settings.Enabled = input.Enabled;
        settings.ChatModel = input.ChatModel.Trim();
        settings.CvModel = input.CvModel.Trim();
        settings.EvaluationModel = input.EvaluationModel.Trim();
        settings.Version++;
        settings.UpdatedAtUtc = DateTime.UtcNow;
        settings.UpdatedByUserId = adminUserId;

        var existing = await db.AiApiCredentials.ToDictionaryAsync(x => x.Id, cancellationToken);
        foreach (var row in input.Credentials)
        {
            AiApiCredential credential;
            if (row.Id > 0 && existing.TryGetValue(row.Id, out var saved)) credential = saved;
            else
            {
                credential = new AiApiCredential();
                db.AiApiCredentials.Add(credential);
            }
            if (!string.IsNullOrWhiteSpace(row.Key))
            {
                if (row.Key.Length > 512 || !row.Key.StartsWith("AIza", StringComparison.Ordinal))
                    throw new InvalidOperationException("API key Google AI Studio có định dạng không hợp lệ.");
                credential.ProtectedKey = protector.Protect(row.Key.Trim());
                credential.Fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(row.Key.Trim())))[..8];
                credential.LastStatus = "Untested";
                credential.LastMessage = string.Empty;
                credential.CooldownUntilUtc = null;
            }
            else if (credential.Id == 0 || string.IsNullOrWhiteSpace(credential.ProtectedKey))
                throw new InvalidOperationException("Dán API key cho từng dòng mới.");
            credential.Name = string.IsNullOrWhiteSpace(row.Name) ? $"Key {row.Priority}" : row.Name.Trim()[..Math.Min(80, row.Name.Trim().Length)];
            credential.QuotaGroup = string.IsNullOrWhiteSpace(row.QuotaGroup) ? "default" : row.QuotaGroup.Trim()[..Math.Min(80, row.QuotaGroup.Trim().Length)];
            credential.Priority = Math.Clamp(row.Priority, 1, 3);
            credential.Enabled = row.Enabled;
        }
        var retained = input.Credentials.Where(x => x.Id > 0).Select(x => x.Id).ToHashSet();
        db.AiApiCredentials.RemoveRange(existing.Values.Where(x => !retained.Contains(x.Id)));
        await db.SaveChangesAsync(cancellationToken);
        if (transaction is not null) { await transaction.CommitAsync(cancellationToken); await transaction.DisposeAsync(); }
    }

    public async Task<IReadOnlyList<AiModelView>> ListModelsAsync(int? credentialId = null, CancellationToken cancellationToken = default)
    {
        var credentials = await GetCredentialsAsync(cancellationToken);
        if (credentialId.HasValue) credentials = credentials.Where(x => x.Id == credentialId.Value).ToList();
        var output = new Dictionary<string, AiModelView>(StringComparer.OrdinalIgnoreCase);
        foreach (var credential in credentials.Where(x => x.Enabled && !Cooling(x)))
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, "v1beta/models");
                request.Headers.TryAddWithoutValidation("x-goog-api-key", Unprotect(credential.ProtectedKey));
                using var response = await clients.CreateClient("Gemini").SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode) continue;
                using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
                if (!json.RootElement.TryGetProperty("models", out var models)) continue;
                foreach (var item in models.EnumerateArray())
                {
                    var name = item.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                    if (!name.StartsWith("models/", StringComparison.Ordinal) || !item.TryGetProperty("supportedGenerationMethods", out var methods)) continue;
                    var supported = methods.EnumerateArray().Any(x => x.GetString() == "generateContent");
                    if (!supported) continue;
                    var id = name[7..];
                    var display = item.TryGetProperty("displayName", out var d) ? d.GetString() ?? id : id;
                    output[id] = new AiModelView(id, display, true);
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException or CryptographicException)
            { logger.LogWarning("Could not list Gemini models for credential {CredentialId}: {Reason}", credential.Id, ex.GetType().Name); }
        }
        return output.Values.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public async Task TestCredentialAsync(int credentialId, CancellationToken cancellationToken = default)
    {
        var credential = await db.AiApiCredentials.SingleOrDefaultAsync(x => x.Id == credentialId, cancellationToken)
            ?? throw new InvalidOperationException("Không tìm thấy key.");
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "v1beta/models");
            request.Headers.TryAddWithoutValidation("x-goog-api-key", Unprotect(credential.ProtectedKey));
            using var response = await clients.CreateClient("Gemini").SendAsync(request, cancellationToken);
            credential.LastCheckedAtUtc = DateTime.UtcNow;
            credential.LastStatus = response.IsSuccessStatusCode ? "Ready" : response.StatusCode == HttpStatusCode.Unauthorized ? "InvalidCredential" : "Unavailable";
            credential.LastMessage = response.IsSuccessStatusCode ? "Kết nối và liệt kê model thành công." : $"Gemini trả HTTP {(int)response.StatusCode}.";
            if (response.StatusCode == HttpStatusCode.Unauthorized) credential.Enabled = false;
            await db.SaveChangesAsync(cancellationToken);
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException(credential.LastMessage);
        }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex) when (ex is HttpRequestException or CryptographicException)
        {
            credential.LastCheckedAtUtc = DateTime.UtcNow;
            credential.LastStatus = "Unavailable";
            credential.LastMessage = "Không kết nối được; kiểm tra mạng và cấu hình key.";
            await db.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException(credential.LastMessage);
        }
    }

    public async Task<GeminiResult?> GenerateJsonAsync(string task, string prompt, object input,
        CancellationToken cancellationToken = default)
    {
        var settings = await db.AiProviderSettings.AsNoTracking().SingleOrDefaultAsync(x => x.Id == 1, cancellationToken);
        if (settings is not { Enabled: true }) return null;
        var model = task switch { "cv-review" => settings.CvModel, "evaluation" => settings.EvaluationModel, _ => settings.ChatModel };
        var keys = await GetCredentialsAsync(cancellationToken);
        var triedGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var attempts = 0;
        var timer = Stopwatch.StartNew();
        using var operationTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        operationTimeout.CancelAfter(OperationTimeout);
        foreach (var credential in keys.Where(x => x.Enabled && !Cooling(x)))
        {
            if (attempts >= 3) break;
            var group = string.IsNullOrWhiteSpace(credential.QuotaGroup) ? "default" : credential.QuotaGroup;
            if (triedGroups.Contains(group)) continue;
            attempts++;
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(operationTimeout.Token);
            timeout.CancelAfter(RequestTimeout);
            try
            {
                var modelPath = Uri.EscapeDataString(model);
                using var request = new HttpRequestMessage(HttpMethod.Post, $"v1beta/models/{modelPath}:generateContent")
                {
                    Content = JsonContent.Create(new
                    {
                        systemInstruction = new { parts = new[] { new { text = prompt } } },
                        contents = new[] { new { role = "user", parts = new[] { new { text = JsonSerializer.Serialize(input) } } } },
                        generationConfig = new { responseMimeType = "application/json" }
                    })
                };
                request.Headers.TryAddWithoutValidation("x-goog-api-key", Unprotect(credential.ProtectedKey));
                using var response = await clients.CreateClient("Gemini").SendAsync(request, timeout.Token);
                credential.LastUsedAtUtc = DateTime.UtcNow;
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(timeout.Token), cancellationToken: timeout.Token);
                        var text = document.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                        var usage = document.RootElement.TryGetProperty("usageMetadata", out var metadata) ? metadata : default;
                        long? inputTokens = usage.ValueKind == JsonValueKind.Object && usage.TryGetProperty("promptTokenCount", out var inValue) ? inValue.GetInt64() : null;
                        long? outputTokens = usage.ValueKind == JsonValueKind.Object && usage.TryGetProperty("candidatesTokenCount", out var outValue) ? outValue.GetInt64() : null;
                        credential.LastStatus = "Ready"; credential.LastMessage = string.Empty;
                        await db.SaveChangesAsync(cancellationToken);
                        await SaveOperationAsync(task, model, string.IsNullOrWhiteSpace(text) ? "InvalidResponse" : "Success", attempts, inputTokens, outputTokens, timer.ElapsedMilliseconds, credential.Id, cancellationToken);
                        return string.IsNullOrWhiteSpace(text) ? null : new GeminiResult(text, model, credential.Id, attempts, inputTokens, outputTokens, timer.ElapsedMilliseconds);
                    }
                    catch (Exception ex) when (ex is JsonException or InvalidOperationException or IndexOutOfRangeException)
                    {
                        logger.LogWarning("Gemini returned an invalid response for task {Task}: {Type}", task, ex.GetType().Name);
                        await SaveOperationAsync(task, model, "InvalidResponse", attempts, null, null, timer.ElapsedMilliseconds, credential.Id, cancellationToken);
                        return null;
                    }
                }
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    var retryAt = response.Headers.RetryAfter?.Date
                        ?? (response.Headers.RetryAfter?.Delta is { } delay ? DateTimeOffset.UtcNow.Add(delay) : DateTimeOffset.UtcNow.AddMinutes(1));
                    credential.CooldownUntilUtc = retryAt.UtcDateTime < DateTime.UtcNow.AddSeconds(5)
                        ? DateTime.UtcNow.AddSeconds(5)
                        : retryAt.UtcDateTime > DateTime.UtcNow.AddHours(24) ? DateTime.UtcNow.AddHours(24) : retryAt.UtcDateTime;
                    credential.LastStatus = "QuotaExhausted";
                    credential.LastMessage = "Gemini trả giới hạn quota; thời gian thử lại được lấy từ Retry-After nếu có.";
                    triedGroups.Add(group);
                    await db.SaveChangesAsync(cancellationToken);
                    continue;
                }
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    credential.Enabled = false; credential.LastStatus = "InvalidCredential";
                    credential.LastMessage = "Key bị từ chối; đã tắt key này.";
                    await db.SaveChangesAsync(cancellationToken);
                    continue;
                }
                if ((int)response.StatusCode >= 500)
                {
                    logger.LogWarning("Gemini unavailable: HTTP {StatusCode}, model {Model}", (int)response.StatusCode, model);
                    break;
                }
                credential.LastStatus = response.StatusCode == HttpStatusCode.Forbidden ? "ModelUnavailable" : "Unavailable";
                credential.LastMessage = $"Gemini trả HTTP {(int)response.StatusCode}.";
                await db.SaveChangesAsync(cancellationToken);
                break;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                if (operationTimeout.IsCancellationRequested) break;
                credential.LastStatus = "Unavailable"; credential.LastMessage = "Gemini vượt quá thời gian chờ.";
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                logger.LogWarning("Gemini network failure for credential {CredentialId}: {Type}", credential.Id, ex.GetType().Name);
            }
        }
        await SaveOperationAsync(task, model, "Unavailable", attempts, null, null, timer.ElapsedMilliseconds, null, cancellationToken);
        return null;
    }

    private async Task<List<AiApiCredential>> GetCredentialsAsync(CancellationToken cancellationToken) =>
        await db.AiApiCredentials.OrderBy(x => x.Priority).ThenBy(x => x.Id).ToListAsync(cancellationToken);

    private static bool Cooling(AiApiCredential x) => x.CooldownUntilUtc > DateTime.UtcNow;
    private string Unprotect(string value) => protector.Unprotect(value);
    private static void ValidateModel(string model)
    {
        if (string.IsNullOrWhiteSpace(model) || model.Length > 100 || model.Contains('/') || model.Contains("..", StringComparison.Ordinal))
            throw new InvalidOperationException("Model Gemini không hợp lệ.");
    }

    private async Task SaveOperationAsync(string task, string model, string status, int attempts,
        long? inputTokens, long? outputTokens, long duration, int? credentialId, CancellationToken cancellationToken)
    {
        db.AiOperations.Add(new AiOperationRecord
        {
            Id = Guid.NewGuid(), Task = task, Model = model, Status = status, AttemptCount = attempts,
            CredentialId = credentialId, InputTokens = inputTokens, OutputTokens = outputTokens, DurationMs = duration, CreatedAtUtc = DateTime.UtcNow
        });
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException ex) { logger.LogWarning("Could not persist AI operation metadata: {Type}", ex.GetType().Name); }
    }
}
