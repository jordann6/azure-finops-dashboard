using System.Text.Json.Serialization;

namespace FinOpsFunctions.Models;

public class CostRecord
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("resourceId")]
    public string ResourceId { get; set; } = string.Empty;

    [JsonPropertyName("resourceName")]
    public string ResourceName { get; set; } = string.Empty;

    [JsonPropertyName("resourceType")]
    public string ResourceType { get; set; } = string.Empty;

    [JsonPropertyName("resourceGroup")]
    public string ResourceGroup { get; set; } = string.Empty;

    [JsonPropertyName("cost")]
    public decimal Cost { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "USD";

    [JsonPropertyName("usageDate")]
    public string UsageDate { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public Dictionary<string, string> Tags { get; set; } = new();

    [JsonPropertyName("ingestedAt")]
    public DateTime IngestedAt { get; set; } = DateTime.UtcNow;
}
