namespace SoftSync.Common;

/// <summary>The age bands used by onboarding and account settings.</summary>
public static class ProfileAgeGroups
{
    public static IReadOnlyList<(string Key, string LabelKey)> Options { get; } =
        Array.AsReadOnly(new[]
        {
            ("<18", "wizard.age.under18"), ("18-22", "wizard.age.1822"),
            ("23-27", "wizard.age.2327"), ("28+", "wizard.age.28plus")
        });

    public static string ForAge(int age) => age switch
    {
        <= 0 or > 120 => "",
        < 18 => "<18",
        <= 22 => "18-22",
        <= 27 => "23-27",
        _ => "28+"
    };

    public static string LabelKey(int age) =>
        Options.FirstOrDefault(x => x.Key == ForAge(age)).LabelKey ?? "";

    // The database stores an age rather than a band. Preserve an existing exact
    // age when it belongs to the selected band; otherwise retain the historic
    // onboarding representatives for compatibility with stored profiles.
    public static int Select(string group, int currentAge)
    {
        if (!string.IsNullOrEmpty(group) && ForAge(currentAge) == group) return currentAge;
        return group switch
        {
            "<18" => 17,
            "18-22" => 20,
            "23-27" => 25,
            "28+" => 30,
            _ => throw new ArgumentException("Unknown age group.", nameof(group))
        };
    }
}
