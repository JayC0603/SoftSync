using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SoftSync.Presentation.Services;

public sealed record RecruitmentUpload(string FileName, string ContentType, Stream Content);

public sealed class RecruitmentMatchResult
{
    [JsonPropertyName("rank")] public int Rank { get; set; }
    [JsonPropertyName("cv_file")] public string CvFile { get; set; } = string.Empty;
    [JsonPropertyName("score")] public double Score { get; set; }
    [JsonPropertyName("confidence")] public double Confidence { get; set; }
    [JsonPropertyName("match")] public bool IsMatch { get; set; }
    [JsonPropertyName("label")] public string Label { get; set; } = string.Empty;
    [JsonPropertyName("cv_view_coverage")] public string CvCoverage { get; set; } = string.Empty;
    [JsonPropertyName("jd_view_coverage")] public string JdCoverage { get; set; } = string.Empty;
}

public sealed class RecruitmentMatchingData
{
    [JsonPropertyName("results")] public List<RecruitmentMatchResult> Results { get; set; } = [];
    [JsonPropertyName("elapsed_seconds")] public double ElapsedSeconds { get; set; }
    [JsonPropertyName("extraction_seconds")] public double ExtractionSeconds { get; set; }
    [JsonPropertyName("inference_seconds")] public double InferenceSeconds { get; set; }
    [JsonPropertyName("model_used")] public string? ModelUsed { get; set; }
}

internal sealed class RecruitmentMatchingEnvelope
{
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("data")] public RecruitmentMatchingData? Data { get; set; }
}

public sealed class RecruitmentAiClient(HttpClient httpClient)
{
    public async Task<RecruitmentMatchingData> MatchAsync(
        IReadOnlyList<RecruitmentUpload> cvs,
        RecruitmentUpload? jdFile,
        string? jdText,
        string modelId,
        CancellationToken cancellationToken = default)
    {
        if (cvs.Count == 0) throw new ArgumentException("At least one CV is required.", nameof(cvs));
        if ((jdFile is null) == string.IsNullOrWhiteSpace(jdText))
            throw new ArgumentException("Provide exactly one JD file or JD text.");

        using var form = new MultipartFormDataContent();
        foreach (var cv in cvs)
        {
            var content = new StreamContent(cv.Content);
            content.Headers.ContentType = new(cv.ContentType);
            form.Add(content, "cv_files", cv.FileName);
        }

        if (jdFile is not null)
        {
            var content = new StreamContent(jdFile.Content);
            content.Headers.ContentType = new(jdFile.ContentType);
            form.Add(content, "jd_file", jdFile.FileName);
        }
        else
        {
            form.Add(new StringContent(jdText!.Trim()), "jd_text");
        }

        form.Add(new StringContent(modelId), "model_id");
        form.Add(new StringContent("false"), "include_structured");

        using var response = await httpClient.PostAsync("api/v1/matching/predict", form, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var detail = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"AI service returned {(int)response.StatusCode}: {detail}");
        }

        var envelope = await response.Content.ReadFromJsonAsync<RecruitmentMatchingEnvelope>(cancellationToken: cancellationToken);
        return envelope?.Data ?? throw new InvalidOperationException("AI service returned an invalid response.");
    }
}
