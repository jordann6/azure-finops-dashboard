using System.Text.Json.Serialization;

namespace FinOpsFunctions.Models;

// Spend attributed to one value of a cost allocation tag (e.g. project=finops).
// Untagged spend is surfaced under the literal "(untagged)" value.
public class CostByTagRecord
{
    [JsonPropertyName("tagKey")]
    public string TagKey { get; set; } = string.Empty;

    [JsonPropertyName("tagValue")]
    public string TagValue { get; set; } = string.Empty;

    [JsonPropertyName("cost")]
    public decimal Cost { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "USD";
}
