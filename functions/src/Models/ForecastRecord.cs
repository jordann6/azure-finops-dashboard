using System.Text.Json.Serialization;

namespace FinOpsFunctions.Models;

public class ForecastRecord
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("forecastDate")]
    public string ForecastDate { get; set; } = string.Empty;

    [JsonPropertyName("projectedCost")]
    public decimal ProjectedCost { get; set; }

    [JsonPropertyName("lowerBound")]
    public decimal LowerBound { get; set; }

    [JsonPropertyName("upperBound")]
    public decimal UpperBound { get; set; }

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    [JsonPropertyName("generatedAt")]
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
