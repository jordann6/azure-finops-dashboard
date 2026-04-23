using System.Text.Json.Serialization;

namespace FinOpsFunctions.Models;

public class TagHygieneResult
{
    [JsonPropertyName("totalResources")]
    public int TotalResources { get; set; }

    [JsonPropertyName("taggedResources")]
    public int TaggedResources { get; set; }

    [JsonPropertyName("untaggedResources")]
    public int UntaggedResources { get; set; }

    [JsonPropertyName("compliancePercent")]
    public double CompliancePercent { get; set; }

    [JsonPropertyName("requiredTags")]
    public List<string> RequiredTags { get; set; } = new();

    [JsonPropertyName("findings")]
    public List<TagFinding> Findings { get; set; } = new();

    [JsonPropertyName("generatedAt")]
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class TagFinding
{
    [JsonPropertyName("resourceId")]
    public string ResourceId { get; set; } = string.Empty;

    [JsonPropertyName("resourceName")]
    public string ResourceName { get; set; } = string.Empty;

    [JsonPropertyName("resourceType")]
    public string ResourceType { get; set; } = string.Empty;

    [JsonPropertyName("missingTags")]
    public List<string> MissingTags { get; set; } = new();
}
