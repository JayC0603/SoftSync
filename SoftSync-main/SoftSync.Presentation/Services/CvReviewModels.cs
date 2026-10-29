namespace SoftSync.Presentation.Services;

public sealed record CvReviewRequest(string CvText, string? JdText = null, string? Language = null);

public sealed class CvReviewText
{
    public string Vi { get; set; } = string.Empty;
    public string En { get; set; } = string.Empty;
    public string ForLanguage(string language) => language == "vi"
        ? (string.IsNullOrWhiteSpace(Vi) ? En : Vi)
        : (string.IsNullOrWhiteSpace(En) ? Vi : En);
}

public sealed class CvReviewCriterion
{
    public string Id { get; set; } = string.Empty;
    public int? Score { get; set; }
    public CvReviewText Feedback { get; set; } = new();
}

public sealed class CvReviewRewrite
{
    public string Requirement { get; set; } = string.Empty;
    public string Original { get; set; } = string.Empty;
    public CvReviewText Suggested { get; set; } = new();
    public CvReviewText Reason { get; set; } = new();
}

public sealed class CvReviewJobMatch
{
    public int? Score { get; set; }
    public CvReviewText Summary { get; set; } = new();
    public List<CvReviewText> MatchedRequirements { get; set; } = [];
    public List<CvReviewText> MissingRequirements { get; set; } = [];
    public List<CvRequirementMatch> Requirements { get; set; } = [];
}

public sealed class CvRequirementMatch
{
    public string Importance { get; set; } = "Unspecified";
    public string Requirement { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Evidence { get; set; } = string.Empty;
    public CvReviewText Recommendation { get; set; } = new();
}

public sealed class CvReviewAction
{
    public string Requirement { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public CvReviewText Section { get; set; } = new();
    public string Evidence { get; set; } = string.Empty;
    public bool MissingInformation { get; set; }
    public CvReviewText Issue { get; set; } = new();
    public CvReviewText Action { get; set; } = new();
}

public sealed record CvReviewResult(int QualityScore, CvReviewText Summary,
    IReadOnlyList<CvReviewCriterion> Criteria, IReadOnlyList<CvReviewText> Strengths,
    IReadOnlyList<CvReviewText> Improvements, IReadOnlyList<CvReviewRewrite> Rewrites,
    CvReviewJobMatch? JobMatch)
{
    public IReadOnlyList<CvReviewAction> Actions { get; init; } = [];
    public IReadOnlyList<CvReviewText> Questions { get; init; } = [];
    public long ElapsedMilliseconds { get; init; }
    public string? Model { get; init; }
    public string? OutputLanguage { get; init; }
    public string? CacheKey { get; init; }
}

public sealed class CvReviewAiResponse
{
    public CvReviewText Summary { get; set; } = new();
    public List<CvReviewCriterion> Criteria { get; set; } = [];
    public List<CvReviewText> Strengths { get; set; } = [];
    public List<CvReviewText> Improvements { get; set; } = [];
    public List<CvReviewRewrite> Rewrites { get; set; } = [];
    public CvReviewJobMatch? JobMatch { get; set; }
    public List<CvReviewAction> Actions { get; set; } = [];
    public List<CvReviewText> Questions { get; set; } = [];
}

public sealed class CvReviewException(string code, string vi, string en) : Exception(en)
{
    public string Code { get; } = code;
    public CvReviewText Feedback { get; } = new() { Vi = vi, En = en };
}
