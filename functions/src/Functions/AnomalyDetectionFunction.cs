using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FinOpsFunctions.Services;

namespace FinOpsFunctions.Functions;

public class AnomalyDetectionFunction
{
    private readonly ILogger<AnomalyDetectionFunction> _logger;
    private readonly AnomalyDetectionService _detectionService;
    private readonly CosmosService _cosmos;

    public AnomalyDetectionFunction(
        ILogger<AnomalyDetectionFunction> logger,
        AnomalyDetectionService detectionService,
        CosmosService cosmos)
    {
        _logger = logger;
        _detectionService = detectionService;
        _cosmos = cosmos;
    }

    [Function("AnomalyDetection")]
    public async Task Run([TimerTrigger("0 30 6 * * *")] TimerInfo timer)
    {
        _logger.LogInformation("Anomaly detection started at {time}", DateTime.UtcNow);

        var anomalies = await _detectionService.DetectAnomaliesAsync();
        _logger.LogInformation("Detected {count} anomalies", anomalies.Count);

        foreach (var anomaly in anomalies)
        {
            await _cosmos.UpsertAnomalyAsync(anomaly);
            _logger.LogWarning("Anomaly: {resource} - Expected {expected}, Actual {actual} ({severity})",
                anomaly.ResourceName, anomaly.ExpectedCost, anomaly.ActualCost, anomaly.Severity);
        }

        _logger.LogInformation("Anomaly detection completed");
    }
}
