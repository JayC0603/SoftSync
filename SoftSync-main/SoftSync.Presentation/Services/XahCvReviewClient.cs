using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace SoftSync.Presentation.Services;

public interface ICvReviewLanguageModel
{
    string Model { get; }
    string ResponseModel => Model;
    Task<CvReviewAiResponse> AnalyzeAsync(string systemPrompt, object input, CancellationToken cancellationToken);
}

// CV-only transport. Other AI features retain their existing provider and credentials.
public sealed class XahCvReviewClient(IHttpClientFactory clients, IConfiguration configuration,
    ILogger<XahCvReviewClient> logger) : ICvReviewLanguageModel
{
    public const string DefaultModel = "tranhieu13102003/gpt-6-luna";
    public string Model => configuration["CvLlm:Model"]?.Trim() is { Length: > 0 } model ? model : DefaultModel;
    public string ResponseModel { get; private set; } = string.Empty;
    // This JSON is model input inside an HTTP message, never embedded in HTML.
    // Preserve Vietnamese text instead of making the model tokenize literal \uXXXX sequences.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public async Task<CvReviewAiResponse> AnalyzeAsync(string systemPrompt, object input, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = configuration["CvLlm:ApiKey"]?.Trim();
        if (!configuration.GetValue("CvLlm:Enabled", true))
            throw Error("ai_disabled", "Tính năng đánh giá CV đang tạm tắt.", "CV review is currently disabled.");
        if (string.IsNullOrWhiteSpace(key) || key is "sk-...." or "YOUR_API_KEY")
            throw Error("ai_not_configured", "Dịch vụ đánh giá CV chưa được cấu hình. Vui lòng liên hệ quản trị viên.", "CV review is not configured. Please contact your administrator.");
        if (Model.Length > 200 || Model.Any(char.IsWhiteSpace) || Model.Any(char.IsControl))
            throw Error("ai_model_invalid", "Tên model CV không hợp lệ.", "The CV model name is invalid.");

        var timer = System.Diagnostics.Stopwatch.StartNew();
        var body = new Dictionary<string, object>
        {
            ["model"] = Model,
            ["stream"] = false,
            ["temperature"] = 0.15,
            ["max_tokens"] = Math.Clamp(configuration.GetValue("CvLlm:MaxOutputTokens", 7500), 2000, 12000),
            ["messages"] = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = JsonSerializer.Serialize(input, JsonOptions) }
            }
        };
        if (configuration.GetValue("CvLlm:UseJsonMode", true)) body["response_format"] = new { type = "json_object" };
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(configuration.GetValue("CvLlm:TimeoutSeconds", 110), 5, 110)));
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.xah.io/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
            request.Content = JsonContent.Create(body);
            using var response = await clients.CreateClient("CvLlm").SendAsync(request, HttpCompletionOption.ResponseHeadersRead, deadline.Token);
            ResponseModel = response.Headers.TryGetValues(XahFallbackHandler.ModelHeader, out var models) ? models.First() : Model;
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("CV provider returned HTTP {StatusCode}", (int)response.StatusCode);
                throw response.StatusCode switch
                {
                    HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => Error("ai_invalid_key", "Khóa XAH chưa hợp lệ hoặc chưa có quyền dùng model. Hãy kiểm tra cấu hình server.", "The XAH key is invalid or cannot access the model. Check server configuration."),
                    HttpStatusCode.NotFound => Error("ai_model_unavailable", "XAH chưa tìm thấy model CV đã cấu hình. Hãy kiểm tra tên model trên server.", "XAH could not find the configured CV model."),
                    HttpStatusCode.TooManyRequests => Error("ai_rate_limited", "AI đang giới hạn lượt gọi. Hãy thử lại sau ít phút.", "AI requests are rate limited. Retry in a few minutes."),
                    HttpStatusCode.BadRequest => Error("ai_request_rejected", "XAH chưa chấp nhận cấu hình gọi AI. Hãy kiểm tra model và chế độ JSON trên server.", "XAH rejected the request. Check the model and JSON mode settings."),
                    _ => Error("ai_unavailable", "Dịch vụ AI đánh giá CV chưa sẵn sàng. Hãy thử lại sau.", "The CV AI service is unavailable. Retry later.")
                };
            }
            await response.Content.LoadIntoBufferAsync(1_000_000, deadline.Token);
            using var envelope = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(deadline.Token), cancellationToken: deadline.Token);
            if (!envelope.RootElement.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
                throw InvalidResponse();
            var choice = choices[0];
            if (choice.TryGetProperty("finish_reason", out var reason) && reason.GetString() is "length" or "content_filter")
                throw InvalidResponse();
            if (!choice.TryGetProperty("message", out var message) || !message.TryGetProperty("content", out var content)
                || content.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(content.GetString())) throw InvalidResponse();
            var json = content.GetString()!.Trim();
            if (json.StartsWith("```", StringComparison.Ordinal))
            {
                var line = json.IndexOf('\n');
                if (line < 0 || !json.EndsWith("```", StringComparison.Ordinal)) throw InvalidResponse();
                json = json[(line + 1)..^3].Trim();
            }
            var result = JsonSerializer.Deserialize<CvReviewAiResponse>(json, JsonOptions) ?? throw InvalidResponse();
            logger.LogInformation("CV AI response: model {Model}, provider milliseconds {ProviderMilliseconds}", ResponseModel, timer.ElapsedMilliseconds);
            return result;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        { throw Error("ai_timeout", "AI phản hồi quá lâu. CV đã lưu vẫn còn; bạn có thể phân tích lại.", "AI took too long. Your saved CV is retained; you can analyze it again."); }
        catch (JsonException) { throw InvalidResponse(); }
        catch (InvalidOperationException) { throw InvalidResponse(); }
        catch (HttpRequestException)
        { throw Error("ai_connection_failed", "Chưa kết nối được XAH. Vui lòng thử lại sau.", "Unable to connect to XAH. Retry later."); }
    }

    private static CvReviewException Error(string code, string vi, string en) => new(code, vi, en);
    private static CvReviewException InvalidResponse() => Error("ai_invalid_response", "AI chưa trả về nhận xét đầy đủ. Vui lòng phân tích lại.", "AI returned an incomplete review. Please analyze again.");
}
