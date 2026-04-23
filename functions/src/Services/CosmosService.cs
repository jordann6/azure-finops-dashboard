using Azure.Identity;
using Microsoft.Azure.Cosmos;
using FinOpsFunctions.Models;

namespace FinOpsFunctions.Services;

public class CosmosService
{
    private readonly CosmosClient _client;
    private readonly string _databaseName;

    public CosmosService(string endpoint, string databaseName)
    {
        _client = new CosmosClient(endpoint, new DefaultAzureCredential());
        _databaseName = databaseName;
    }

    private Container GetContainer(string containerName) =>
        _client.GetContainer(_databaseName, containerName);

    public async Task UpsertCostRecordAsync(CostRecord record)
    {
        var container = GetContainer("daily-costs");
        await container.UpsertItemAsync(record, new PartitionKey(record.ResourceId));
    }

    public async Task UpsertAnomalyAsync(AnomalyRecord record)
    {
        var container = GetContainer("anomalies");
        await container.UpsertItemAsync(record, new PartitionKey(record.DetectedDate));
    }

    public async Task UpsertForecastAsync(ForecastRecord record)
    {
        var container = GetContainer("forecasts");
        await container.UpsertItemAsync(record, new PartitionKey(record.ForecastDate));
    }

    public async Task<List<CostRecord>> GetCostsByDateRangeAsync(string startDate, string endDate)
    {
        var container = GetContainer("daily-costs");
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.usageDate >= @start AND c.usageDate <= @end")
            .WithParameter("@start", startDate)
            .WithParameter("@end", endDate);

        var results = new List<CostRecord>();
        using var iterator = container.GetItemQueryIterator<CostRecord>(query);
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }
        return results;
    }

    public async Task<List<AnomalyRecord>> GetRecentAnomaliesAsync(int days = 30)
    {
        var container = GetContainer("anomalies");
        var cutoff = DateTime.UtcNow.AddDays(-days).ToString("yyyy-MM-dd");
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.detectedDate >= @cutoff ORDER BY c.detectedDate DESC")
            .WithParameter("@cutoff", cutoff);

        var results = new List<AnomalyRecord>();
        using var iterator = container.GetItemQueryIterator<AnomalyRecord>(query);
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }
        return results;
    }

    public async Task<List<ForecastRecord>> GetForecastsAsync()
    {
        var container = GetContainer("forecasts");
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.forecastDate >= @today ORDER BY c.forecastDate ASC")
            .WithParameter("@today", today);

        var results = new List<ForecastRecord>();
        using var iterator = container.GetItemQueryIterator<ForecastRecord>(query);
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }
        return results;
    }
}
