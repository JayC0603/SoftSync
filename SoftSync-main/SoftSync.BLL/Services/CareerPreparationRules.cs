namespace SoftSync.BLL.Services;

public static class CareerPreparationRules
{
    public static bool CanCreateInterviewEvidence(bool sessionCompleted) => sessionCompleted;

    public static string StarGap(string? situation, string? task, string? action, string? result)
    {
        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(situation)) missing.Add("Situation");
        if (string.IsNullOrWhiteSpace(task)) missing.Add("Task");
        if (string.IsNullOrWhiteSpace(action)) missing.Add("Action");
        if (string.IsNullOrWhiteSpace(result)) missing.Add("Result");
        return missing.Count == 0 ? "Your answer covers Situation, Task, Action and Result." : $"Consider adding: {string.Join(", ", missing)}.";
    }

    public static bool CanViewPrivatePortfolio(int authenticatedUserId, int ownerUserId) =>
        authenticatedUserId > 0 && authenticatedUserId == ownerUserId;
}
