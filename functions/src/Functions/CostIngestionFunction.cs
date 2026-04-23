using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FinOpsFunctions.Services;

namespace FinOpsFunctions.Functions;

public class CostIngestionFunction
{
    private readonly ILogger<CostIngestionFunction> _logger;

    public CostIngestionFunction(ILogger<CostIngestionFunction> logger)
    {
        _logger = logger;
    }

    [Function("CostIngestion")]
    public async Task Run([TimerTrigger("0 0 6 * * *")] TimerInfo timer)
    {
        _logger.LogInformation("Cost ingestion started at {time}", DateTime.UtcNow);

        var subscriptionId = Environment.GetEnvironmentVariable("Azure__SubscriptionId")
            ?? throw new InvalidOperationException("Azure__SubscriptionId not configured");
        var cosmosEndpoint = Environment.GetEnvironmentVariable("CosmosDb__Endpoint")
            ?? throw new InvalidOperationException("CosmosDb__Endpoint not configured");
        var databaseName = Environment.GetEnvironmentVariable("CosmosDb__DatabaseName")
            ?? throw new InvalidOperationException("CosmosDb__DatabaseName not configured");

        var cosmosService = new CosmosService(cosmosEndpoint, databaseName);
        var ingestionService = new CostIngestionService(subscriptionId);

        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var startDate = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");

        _logger.LogInformation("Querying costs from {start} to {end}", startDate, endDate);

        var records = await ingestionService.QueryCostsAsync(startDate, endDate);
        _logger.LogInformation("Retrieved {count} cost records", records.Count);

        foreach (var record in records)
        {
            await cosmosService.UpsertCostRecordAsync(record);
        }

        _logger.LogInformation("Cost ingestion completed. Upserted {count} records", records.Count);
    }
}
