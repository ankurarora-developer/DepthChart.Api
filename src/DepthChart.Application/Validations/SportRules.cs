namespace DepthChart.Application.Validation;

public static class SportRules
{
    private static readonly Dictionary<string, HashSet<string>> _positions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["NFL"] = new HashSet<string>(new[] {
            "QB","RB","LWR","RWR","TE","LT","LG","C","RG","RT"            
        }, StringComparer.OrdinalIgnoreCase)
    };

    public static bool IsValidPosition(string sport, string position)
    {
        return _positions.TryGetValue(sport, out var set) && set.Contains(position);
    }
}
