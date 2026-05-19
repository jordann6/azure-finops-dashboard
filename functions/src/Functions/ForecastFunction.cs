using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FinOpsFunctions.Services;

namespace FinOpsFunctions.Functions;

public class ForecastFunction
{
    private readonly ILogger<ForecastFunction> _logger;
    private readonly ForecastService _forecastService;
    private readonly CosmosService _cosmos;

    public ForecastFunction(
        ILogger<ForecastFunction> logger,
        ForecastService forecastService,
        CosmosService cosmos)
    {
        _logger = logger;
        _forecastService = forecastService;
        _cosmos = cosmos;
    }

    [Function("Forecast")]
    public async Task Run([TimerTrigger("0 0 7 * * *")] TimerInfo timer)
    {
        _logger.LogInformation("Forecast generation started at {time}", DateTime.UtcNow);

        var forecasts = await _forecastService.GenerateForecastAsync();
        _logger.LogInformation("Generated {count} forecast records", forecasts.Count);

        foreach (var forecast in forecasts)
        {
            await _cosmos.UpsertForecastAsync(forecast);
        }

        _logger.LogInformation("Forecast generation completed");
    }
}
