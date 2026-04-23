using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Azure.Identity;
using Azure.Core;
using FinOpsFunctions.Models;

namespace FinOpsFunctions.Services;

public class CostIngestionService
{
    private readonly string _subscriptionId;
    private readonly DefaultAzureCredential _credential;
    private static readonly HttpClient _httpClient = new();

    public CostIngestionService(string subscriptionId)
    {
        _subscriptionId = subscriptionId;
        _credential = new DefaultAzureCredential();
    }

    public async Task<List<CostRecord>> QueryCostsAsync(string startDate, string endDate)
    {
        var token = await _credential.GetTokenAsync(
            new TokenRequestContext(new[] { "https://management.azure.com/.default" }));

        var url = $"https://management.azure.com/subscriptions/{_subscriptionId}/providers/Microsoft.CostManagement/query?api-version=2023-11-01";

        var requestBody = new
        {
            type = "ActualCost",
            timeframe = "Custom",
            timePeriod = new { from = startDate, to = endDate },
            dataset = new
            {
                granularity = "Daily",
                aggregation = new
                {
                    PreTaxCost = new { name = "PreTaxCost", function = "Sum" }
                },
                grouping = new[]
                {
                    new { type = "Dimension", name = "ResourceId" },
                    new { type = "Dimension", name = "ResourceType" },
                    new { type = "Dimension", name = "ResourceGroupName" }
                }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        var records = new List<CostRecord>();
        var root = doc.RootElement;

        if (!root.TryGetProperty("properties", out var properties)) return records;
        if (!properties.TryGetProperty("rows", out var rows)) return records;

        foreach (var row in rows.EnumerateArray())
        {
            var cost = row[0].GetDecimal();
            if (cost <= 0) continue;

            var resourceId = row[1].GetString() ?? string.Empty;
            var resourceType = row[2].GetString() ?? string.Empty;
            var resourceGroup = row[3].GetString() ?? string.Empty;
            var usageDateRaw = row[4].GetInt64();
            var usageDate = DateTimeOffset
                .FromUnixTimeMilliseconds(usageDateRaw)
                .ToString("yyyy-MM-dd");

            records.Add(new CostRecord
            {
                ResourceId = resourceId,
                ResourceName = ExtractResourceName(resourceId),
                ResourceType = resourceType,
                ResourceGroup = resourceGroup,
                Cost = cost,
                UsageDate = usageDate
            });
        }

        return records;
    }

    private static string ExtractResourceName(string resourceId)
    {
        if (string.IsNullOrEmpty(resourceId)) return "Unknown";
        var parts = resourceId.Split('/');
        return parts.Length > 0 ? parts[^1] : "Unknown";
    }
}
