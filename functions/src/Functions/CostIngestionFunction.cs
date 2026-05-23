using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FinOpsFunctions.Services;

namespace FinOpsFunctions.Functions;

public class CostIngestionFunction
{
    private readonly ILogger<CostIngestionFunction> _logger;
    private readonly CosmosService _cosmos;
    private readonly CostIngestionService _ingestionService;

    public CostIngestionFunction(
        ILogger<CostIngestionFunction> logger,
        CosmosService cosmos,
        CostIngestionService ingestionService)
    {
        _logger = logger;
        _cosmos = cosmos;
        _ingestionService = ingestionService;
    }

    [Function("CostIngestion")]
    public async Task Run([TimerTrigger("0 0 6 * * *")] TimerInfo timer)
    {
        _logger.LogInformation("Cost ingestion started at {time}", DateTime.UtcNow);

        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var startDate = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");

        _logger.LogInformation("Querying costs from {start} to {end}", startDate, endDate);

        var records = await _ingestionService.QueryCostsAsync(startDate, endDate);
        _logger.LogInformation("Retrieved {count} cost records", records.Count);

        foreach (var record in records)
        {
            await _cosmos.UpsertCostRecordAsync(record);
        }

        _logger.LogInformation("Cost ingestion completed. Upserted {count} records", records.Count);
    }
}
