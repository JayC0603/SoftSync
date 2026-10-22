namespace SoftSync.Presentation.Services;

public sealed record CvReviewRequest(string CvText, string? JdText = null);

public sealed class CvReviewText
{
    public string Vi { get; set; } = string.Empty;
    public string En { get; set; } = string.Empty;
}

public sealed class CvReviewCriterion
{
    public string Id { get; set; } = string.Empty;
    public int? Score { get; set; }
    public CvReviewText Feedback { get; set; } = new();
}

public sealed class CvReviewRewrite
{
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
}

public sealed record CvReviewResult(int QualityScore, CvReviewText Summary,
    IReadOnlyList<CvReviewCriterion> Criteria, IReadOnlyList<CvReviewText> Strengths,
    IReadOnlyList<CvReviewText> Improvements, IReadOnlyList<CvReviewRewrite> Rewrites,
    CvReviewJobMatch? JobMatch);

public sealed class CvReviewAiResponse
{
    public CvReviewText Summary { get; set; } = new();
    public List<CvReviewCriterion> Criteria { get; set; } = [];
    public List<CvReviewText> Strengths { get; set; } = [];
    public List<CvReviewText> Improvements { get; set; } = [];
    public List<CvReviewRewrite> Rewrites { get; set; } = [];
    public CvReviewJobMatch? JobMatch { get; set; }
}

public sealed class CvReviewException(string code, string vi, string en) : Exception(en)
{
    public string Code { get; } = code;
    public CvReviewText Feedback { get; } = new() { Vi = vi, En = en };
}
