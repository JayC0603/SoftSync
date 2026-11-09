using System.Text.Json;
using SoftSync.Presentation.Services;
using Xunit;

namespace SoftSync.Tests;

public sealed class CvSpeedSamplesFact : FactAttribute
{
    public CvSpeedSamplesFact()
    {
        var directory = Environment.GetEnvironmentVariable("SOFTSYNC_CV_SPEED_SAMPLES");
        if (string.IsNullOrEmpty(directory) || !File.Exists(Path.Combine(directory, "cv-speed-before.json"))
            || !File.Exists(Path.Combine(directory, "cv-speed-after.json")))
            Skip = "Requires explicitly supplied synthetic before/after provider samples.";
    }
}

public sealed class CvSpeedAcceptanceTests
{
    [CvSpeedSamplesFact]
    public void Recorded_provider_samples_preserve_bilingual_feedback_and_grounded_CV_JD_evidence()
    {
        var directory = Environment.GetEnvironmentVariable("SOFTSYNC_CV_SPEED_SAMPLES")!;
        foreach (var name in new[] { "before", "after" })
        {
            using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(directory, $"cv-speed-{name}.json")));
            var sample = document.RootElement;
            var cv = sample.GetProperty("cv").GetString()!;
            var jd = sample.GetProperty("jd").GetString()!;
            var response = sample.GetProperty("review").Deserialize<CvReviewAiResponse>(new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
            var issues = CvReviewService.GetValidationIssues(response, cv, jd);
            Assert.True(issues.Count == 0, name + ": " + string.Join("; ", issues));
            Assert.Contains(response.JobMatch!.Requirements, r => r.Requirement.Contains("TypeScript") && r.Status == "NotDemonstrated");
            Assert.DoesNotContain(response.Rewrites, r => r.Suggested.En.Contains("TypeScript"));
        }
    }
}
