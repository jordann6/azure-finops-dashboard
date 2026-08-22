using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using FinOpsFunctions.Models;

namespace FinOpsFunctions.Services;

// Reads Azure Advisor cost recommendations (idle/underused resources,
// right-sizing, reservation purchases). This is the Azure equivalent of the
// AWS "waste scan + RI/Savings Plans coverage" panel: instead of scanning for
// unattached disks ourselves, Advisor already surfaces them with an estimated
// dollar saving. Requires the Reader role on the subscription.
public class AdvisorService
{
    private readonly string _subscriptionId;
    private readonly DefaultAzureCredential _credential;
    private static readonly HttpClient _httpClient = new();

    public AdvisorService(string subscriptionId)
    {
        _subscriptionId = subscriptionId;
        _credential = new DefaultAzureCredential();
    }

    public async Task<List<WasteRecord>> GetCostRecommendationsAsync()
    {
        var token = await _credential.GetTokenAsync(
            new TokenRequestContext(new[] { "https://management.azure.com/.default" }));

        var url = $"https://management.azure.com/subscriptions/{_subscriptionId}" +
                  "/providers/Microsoft.Advisor/recommendations?api-version=2023-01-01" +
                  "&$filter=" + Uri.EscapeDataString("Category eq 'Cost'");

        var results = new List<WasteRecord>();

        // Advisor pages results via nextLink.
        while (!string.IsNullOrEmpty(url))
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("value", out var value))
            {
                foreach (var item in value.EnumerateArray())
                {
                    if (!item.TryGetProperty("properties", out var props)) continue;

                    var record = new WasteRecord
                    {
                        Category = GetString(props, "category"),
                        Impact = GetString(props, "impact"),
                    };

                    if (props.TryGetProperty("shortDescription", out var sd))
                        record.Problem = GetString(sd, "problem");

                    if (props.TryGetProperty("impactedValue", out var iv))
                        record.ResourceName = iv.GetString() ?? string.Empty;
                    record.ResourceType = GetString(props, "impactedField");

                    if (props.TryGetProperty("extendedProperties", out var ext))
                    {
                        record.Currency = GetString(ext, "savingsCurrency");
                        // Prefer annual savings (÷12) for a monthly figure; fall
                        // back to the term "savingsAmount" if annual is absent.
                        if (TryGetDecimal(ext, "annualSavingsAmount", out var annual))
                            record.EstMonthlySavings = Math.Round(annual / 12m, 2);
                        else if (TryGetDecimal(ext, "savingsAmount", out var savings))
                            record.EstMonthlySavings = Math.Round(savings, 2);
                    }

                    results.Add(record);
                }
            }

            url = root.TryGetProperty("nextLink", out var next) ? next.GetString() : null;
        }

        return results
            .OrderByDescending(r => r.EstMonthlySavings)
            .ToList();
    }

    private static string GetString(JsonElement el, string name) =>
        el.TryGetProperty(name, out var v) ? v.GetString() ?? string.Empty : string.Empty;

    private static bool TryGetDecimal(JsonElement el, string name, out decimal result)
    {
        result = 0m;
        if (!el.TryGetProperty(name, out var v)) return false;
        if (v.ValueKind == JsonValueKind.Number) { result = v.GetDecimal(); return true; }
        if (v.ValueKind == JsonValueKind.String &&
            decimal.TryParse(v.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            return true;
        return false;
    }
}
