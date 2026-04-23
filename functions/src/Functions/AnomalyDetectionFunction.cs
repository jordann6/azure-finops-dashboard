using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FinOpsFunctions.Services;

namespace FinOpsFunctions.Functions;

public class AnomalyDetectionFunction
{
    private readonly ILogger<AnomalyDetectionFunction> _logger;

    public AnomalyDetectionFunction(ILogger<AnomalyDetectionFunction> logger)
    {
        _logger = logger;
    }

    [Function("AnomalyDetection")]
    public async Task Run([TimerTrigger("0 30 6 * * *")] TimerInfo timer)
    {
        _logger.LogInformation("Anomaly detection started at {time}", DateTime.UtcNow);

        var cosmosEndpoint = Environment.GetEnvironmentVariable("CosmosDb__Endpoint")
            ?? throw new InvalidOperationException("CosmosDb__Endpoint not configured");
        var databaseName = Environment.GetEnvironmentVariable("CosmosDb__DatabaseName")
            ?? throw new InvalidOperationException("CosmosDb__DatabaseName not configured");

        var cosmosService = new CosmosService(cosmosEndpoint, databaseName);
        var detectionService = new AnomalyDetectionService(cosmosService);

        var anomalies = await detectionService.DetectAnomaliesAsync();
        _logger.LogInformation("Detected {count} anomalies", anomalies.Count);

        foreach (var anomaly in anomalies)
        {
            await cosmosService.UpsertAnomalyAsync(anomaly);
            _logger.LogWarning("Anomaly: {resource} - Expected {expected}, Actual {actual} ({severity})",
                anomaly.ResourceName, anomaly.ExpectedCost, anomaly.ActualCost, anomaly.Severity);
        }

        _logger.LogInformation("Anomaly detection completed");
    }
}
