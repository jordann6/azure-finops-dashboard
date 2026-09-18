using FinOpsFunctions.Models;

namespace FinOpsFunctions.Services;

// Pure, dependency-free mapping from an Azure CostRecord to a FOCUS row. Kept
// free of Cosmos/HTTP so it is unit-testable and so the same logic could run in
// an ingester or at read time. Category mapping mirrors the AWS dashboard's so
// a "Compute" row means the same thing on both clouds.
public static class FocusMapper
{
    // Matched as substrings against the lowercased Azure resource type
    // (e.g. "microsoft.compute/virtualmachines"). First match wins.
    private static readonly (string Needle, string Category)[] _categories =
    {
        ("compute/", "Compute"),
        ("web/sites", "Compute"),
        ("web/serverfarms", "Compute"),
        ("containerservice", "Compute"),
        ("storage/", "Storage"),
        ("sql", "Databases"),
        ("documentdb", "Databases"),
        ("dbfor", "Databases"),
        ("cache/redis", "Databases"),
        ("network/", "Networking"),
        ("cdn/", "Networking"),
        ("keyvault", "Security"),
        ("insights", "Management and Governance"),
        ("operationalinsights", "Management and Governance"),
        ("cognitiveservices", "AI and Machine Learning"),
        ("machinelearningservices", "AI and Machine Learning"),
    };

    public static string Category(string resourceType)
    {
        var t = (resourceType ?? string.Empty).ToLowerInvariant();
        foreach (var (needle, category) in _categories)
        {
            if (t.Contains(needle)) return category;
        }
        return "Other";
    }

    // A friendly service name from an Azure resource type. The provider segment
    // ("microsoft.compute") is dropped and the remainder title-cased loosely.
    public static string ServiceName(string resourceType)
    {
        if (string.IsNullOrWhiteSpace(resourceType)) return "Unknown";
        var tail = resourceType.Contains('/') ? resourceType[(resourceType.IndexOf('/') + 1)..] : resourceType;
        return tail.Length == 0 ? resourceType : tail;
    }

    public static FocusRecord Map(CostRecord c)
    {
        var start = c.UsageDate;
        var end = DateTime.TryParse(c.UsageDate, out var d)
            ? d.AddDays(1).ToString("yyyy-MM-dd")
            : c.UsageDate;

        return new FocusRecord
        {
            ProviderName = "Azure",
            BillingCurrency = string.IsNullOrWhiteSpace(c.Currency) ? "USD" : c.Currency,
            ChargePeriodStart = string.IsNullOrEmpty(start) ? string.Empty : $"{start}T00:00:00Z",
            ChargePeriodEnd = string.IsNullOrEmpty(end) ? string.Empty : $"{end}T00:00:00Z",
            ChargeCategory = "Usage",
            ServiceName = ServiceName(c.ResourceType),
            ServiceCategory = Category(c.ResourceType),
            BilledCost = c.Cost,
            EffectiveCost = c.Cost,
            ResourceId = c.ResourceId,
            ResourceName = c.ResourceName,
            SubAccountId = c.ResourceGroup,
            Tags = c.Tags ?? new(),
        };
    }
}
