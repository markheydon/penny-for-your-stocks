namespace PennyForYourStocks.Domain;

public static class TrialRegionParser
{
    private static readonly HashSet<string> Allowed = Enum.GetNames<TrialRegion>().ToHashSet(StringComparer.Ordinal);

    public static IReadOnlyList<TrialRegion> ParsePermittedRegions(string stored) =>
        stored.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => Enum.Parse<TrialRegion>(s, ignoreCase: true))
            .Distinct()
            .ToList();

    public static string SerializePermittedRegions(IEnumerable<TrialRegion> regions) =>
        string.Join(',', regions.Distinct().OrderBy(r => r.ToString()));

    public static bool IsValidSubset(IEnumerable<TrialRegion> regions) =>
        regions.Any() && regions.All(r => Allowed.Contains(r.ToString()));
}
