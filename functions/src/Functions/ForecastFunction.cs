using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FinOpsFunctions.Services;

namespace FinOpsFunctions.Functions;

public class ForecastFunction
{
    private readonly ILogger<ForecastFunction> _logger;

    public ForecastFunction(ILogger<ForecastFunction> logger)
    {
        _logger = logger;
    }

    [Function("Forecast")]
    public async Task Run([TimerTrigger("0 0 7 * * *")] TimerInfo timer)
    {
        _logger.LogInformation("Forecast generation started at {time}", DateTime.UtcNow);

        var cosmosEndpoint = Environment.GetEnvironmentVariable("CosmosDb__Endpoint")
            ?? throw new InvalidOperationException("CosmosDb__Endpoint not configured");
        var databaseName = Environment.GetEnvironmentVariable("CosmosDb__DatabaseName")
            ?? throw new InvalidOperationException("CosmosDb__DatabaseName not configured");

        var cosmosService = new CosmosService(cosmosEndpoint, databaseName);
        var forecastService = new ForecastService(cosmosService);

        var forecasts = await forecastService.GenerateForecastAsync();
        _logger.LogInformation("Generated {count} forecast records", forecasts.Count);

        foreach (var forecast in forecasts)
        {
            await cosmosService.UpsertForecastAsync(forecast);
        }

        _logger.LogInformation("Forecast generation completed");
    }
}
