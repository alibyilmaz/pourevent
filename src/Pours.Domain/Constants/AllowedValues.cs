namespace Pours.Domain.Constants;

public static class AllowedValues
{
    public static readonly IReadOnlySet<string> ProductIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "guinness",
        "ipa",
        "lager",
        "pilsner",
        "stout",
        "efes-pilsen",
        "efes-malt",
        "bomonti-filtresiz",
        "tuborg-gold",
        "tuborg-amber"
    };

    public static readonly IReadOnlySet<string> LocationIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "istanbul-kadikoy-01",
        "istanbul-besiktas-01",
        "izmir-alsancak-01",
        "ankara-cankaya-01",
        "london-soho-01"
    };

    public static readonly IReadOnlySet<int> Volumes = new HashSet<int>
    {
        200, 250, 284, 330, 355, 400, 473, 500, 568, 1000
    };
}
