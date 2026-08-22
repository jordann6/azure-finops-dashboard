using System.Text.Json.Serialization;

namespace FinOpsFunctions.Models;

// A cost-optimization finding from Azure Advisor: idle/underused resources,
// right-sizing, and reservation/savings-plan purchase recommendations.
public class WasteRecord
{
    [JsonPropertyName("category")]
    public string Category { get; set; } = "Cost";

    [JsonPropertyName("impact")]
    public string Impact { get; set; } = string.Empty;

    [JsonPropertyName("problem")]
    public string Problem { get; set; } = string.Empty;

    [JsonPropertyName("resourceName")]
    public string ResourceName { get; set; } = string.Empty;

    [JsonPropertyName("resourceType")]
    public string ResourceType { get; set; } = string.Empty;

    // Estimated monthly savings if the recommendation is acted on.
    [JsonPropertyName("estMonthlySavings")]
    public decimal EstMonthlySavings { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "USD";
}
