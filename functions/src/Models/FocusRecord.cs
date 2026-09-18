using System.Text.Json.Serialization;

namespace FinOpsFunctions.Models;

// A FOCUS-conformant cost row. FOCUS (the FinOps Open Cost & Usage Specification)
// is the open schema that lets Azure, AWS, GCP, and SaaS cost data share one
// shape, so chargeback and unit economics work across providers instead of
// per-provider. The core columns here (ProviderName, BillingCurrency,
// ChargePeriodStart/End, ChargeCategory, ServiceName, ServiceCategory,
// BilledCost, EffectiveCost) are the same set the AWS dashboard emits, so the
// two clouds join on one schema. Column names follow the FOCUS PascalCase
// convention deliberately and are serialized verbatim.
public class FocusRecord
{
    [JsonPropertyName("ProviderName")]
    public string ProviderName { get; set; } = "Azure";

    [JsonPropertyName("BillingCurrency")]
    public string BillingCurrency { get; set; } = "USD";

    [JsonPropertyName("ChargePeriodStart")]
    public string ChargePeriodStart { get; set; } = string.Empty;

    [JsonPropertyName("ChargePeriodEnd")]
    public string ChargePeriodEnd { get; set; } = string.Empty;

    [JsonPropertyName("ChargeCategory")]
    public string ChargeCategory { get; set; } = "Usage";

    [JsonPropertyName("ServiceName")]
    public string ServiceName { get; set; } = string.Empty;

    [JsonPropertyName("ServiceCategory")]
    public string ServiceCategory { get; set; } = "Other";

    [JsonPropertyName("BilledCost")]
    public decimal BilledCost { get; set; }

    [JsonPropertyName("EffectiveCost")]
    public decimal EffectiveCost { get; set; }

    // Azure-side richness FOCUS also defines. AWS omits these where the source
    // data is service-level; that is fine, FOCUS treats them as optional.
    [JsonPropertyName("ResourceId")]
    public string ResourceId { get; set; } = string.Empty;

    [JsonPropertyName("ResourceName")]
    public string ResourceName { get; set; } = string.Empty;

    [JsonPropertyName("SubAccountId")]
    public string SubAccountId { get; set; } = string.Empty;

    [JsonPropertyName("Tags")]
    public Dictionary<string, string> Tags { get; set; } = new();
}
