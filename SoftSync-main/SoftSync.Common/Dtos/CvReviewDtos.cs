using System.Text.Json.Serialization;
namespace SoftSync.Common.Dtos;

public sealed class CvAnalysisResult
{
    public string JobDescription { get; set; } = "";
    public string Language { get; set; } = "vi";
    public List<CvRequirementMatch> RequirementMatches { get; set; } = [];
    [JsonRequired] public string Summary { get; set; } = "";
    public List<string> Strengths { get; set; } = [];
    public List<string> Weaknesses { get; set; } = [];
    public List<string> SkillsDetected { get; set; } = [];
    public List<string> MissingInformation { get; set; } = [];
    [JsonRequired] public List<string> Suggestions { get; set; } = [];
    [JsonRequired] public List<CvSectionFeedback> Sections { get; set; } = [];
}
public sealed class CvRequirementMatch
{
    public string Requirement { get; set; } = "";
    public string Status { get; set; } = "";
    public string Evidence { get; set; } = "";
    public string Recommendation { get; set; } = "";
}
public sealed class CvSectionFeedback
{
    [JsonRequired] public string Section { get; set; } = "";
    [JsonRequired] public string Feedback { get; set; } = "";
    public List<string> Suggestions { get; set; } = [];
}
public sealed record CvReviewDto(int AnalysisId, int DocumentId, string FileName, DateTime CreatedAtUtc, CvAnalysisResult Result);
