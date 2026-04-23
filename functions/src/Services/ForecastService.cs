using FinOpsFunctions.Models;

namespace FinOpsFunctions.Services;

public class ForecastService
{
    private readonly CosmosService _cosmosService;

    public ForecastService(CosmosService cosmosService)
    {
        _cosmosService = cosmosService;
    }

    public async Task<List<ForecastRecord>> GenerateForecastAsync(int forecastDays = 14)
    {
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var startDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");

        var costs = await _cosmosService.GetCostsByDateRangeAsync(startDate, endDate);

        var dailyTotals = costs
            .GroupBy(c => c.UsageDate)
            .Select(g => new { Date = g.Key, Total = g.Sum(c => c.Cost) })
            .OrderBy(d => d.Date)
            .ToList();

        if (dailyTotals.Count < 7)
        {
            return new List<ForecastRecord>();
        }

        var values = dailyTotals.Select(d => (double)d.Total).ToList();
        var mean = values.Average();
        var stdDev = Math.Sqrt(values.Sum(v => Math.Pow(v - mean, 2)) / values.Count);

        var trend = CalculateTrend(values);

        var forecasts = new List<ForecastRecord>();
        for (int i = 1; i <= forecastDays; i++)
        {
            var forecastDate = DateTime.UtcNow.AddDays(i);
            var projected = mean + (trend * i);
            var confidence = Math.Max(0.5, 1.0 - (i * 0.03));

            forecasts.Add(new ForecastRecord
            {
                ForecastDate = forecastDate.ToString("yyyy-MM-dd"),
                ProjectedCost = (decimal)Math.Max(0, projected),
                LowerBound = (decimal)Math.Max(0, projected - (stdDev * 1.5)),
                UpperBound = (decimal)(projected + (stdDev * 1.5)),
                Confidence = Math.Round(confidence, 2)
            });
        }

        return forecasts;
    }

    private static double CalculateTrend(List<double> values)
    {
        int n = values.Count;
        double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;

        for (int i = 0; i < n; i++)
        {
            sumX += i;
            sumY += values[i];
            sumXY += i * values[i];
            sumX2 += i * i;
        }

        var denominator = (n * sumX2) - (sumX * sumX);
        if (denominator == 0) return 0;

        return ((n * sumXY) - (sumX * sumY)) / denominator;
    }
}
