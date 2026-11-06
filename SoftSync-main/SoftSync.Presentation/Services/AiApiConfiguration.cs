namespace SoftSync.Presentation.Services;

/// <summary>Shared Gemini OpenAI-compatible transport settings; no credentials are stored here.</summary>
public static class AiApiConfiguration
{
    public const string DefaultBaseUrl = "https://generativelanguage.googleapis.com/v1beta/openai/";
    public const string DefaultModel = "gemini-3.8-flash";
    public const string DefaultCvModel = "gemini-3.1-flash-lite";
    public const string ChatEndpoint = "chat/completions";

    public static string Model(IConfiguration configuration, string? task = null)
    {
        var key = task == "cv-review" ? "AiApi:CvModel" : "AiApi:Model";
        var fallback = task == "cv-review" ? DefaultCvModel : DefaultModel;
        return string.IsNullOrWhiteSpace(configuration[key]) ? fallback : configuration[key]!.Trim();
    }

    public static void Configure(HttpClient client, IConfiguration configuration)
    {
        var baseUrl = configuration["AiApi:BaseUrl"];
        client.BaseAddress = new Uri(string.IsNullOrWhiteSpace(baseUrl) ? DefaultBaseUrl : baseUrl.Trim().TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromSeconds(configuration.GetValue("AiApi:TimeoutSeconds", 45));
    }
}
