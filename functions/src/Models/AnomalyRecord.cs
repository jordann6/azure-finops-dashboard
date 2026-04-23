using System.Text.Json.Serialization;

namespace FinOpsFunctions.Models;

public class AnomalyRecord
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("detectedDate")]
    public string DetectedDate { get; set; } = string.Empty;

    [JsonPropertyName("resourceId")]
    public string ResourceId { get; set; } = string.Empty;

    [JsonPropertyName("resourceName")]
    public string ResourceName { get; set; } = string.Empty;

    [JsonPropertyName("expectedCost")]
    public decimal ExpectedCost { get; set; }

    [JsonPropertyName("actualCost")]
    public decimal ActualCost { get; set; }

    [JsonPropertyName("deviationPercent")]
    public double DeviationPercent { get; set; }

    [JsonPropertyName("severity")]
    public string Severity { get; set; } = "Low";

    [JsonPropertyName("detectedAt")]
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
}
