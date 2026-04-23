using FinOpsFunctions.Models;

namespace FinOpsFunctions.Services;

public class AnomalyDetectionService
{
    private readonly CosmosService _cosmosService;

    public AnomalyDetectionService(CosmosService cosmosService)
    {
        _cosmosService = cosmosService;
    }

    public async Task<List<AnomalyRecord>> DetectAnomaliesAsync()
    {
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var startDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");

        var costs = await _cosmosService.GetCostsByDateRangeAsync(startDate, endDate);

        var anomalies = new List<AnomalyRecord>();

        var costsByResource = costs
            .GroupBy(c => c.ResourceId)
            .Where(g => g.Count() >= 5);

        foreach (var group in costsByResource)
        {
            var orderedCosts = group.OrderBy(c => c.UsageDate).ToList();
            var values = orderedCosts.Select(c => (double)c.Cost).ToList();

            var mean = values.Average();
            var stdDev = CalculateStdDev(values, mean);

            if (stdDev == 0) continue;

            var latest = orderedCosts.Last();
            var latestCost = (double)latest.Cost;
            var zScore = (latestCost - mean) / stdDev;

            if (Math.Abs(zScore) >= 2.0)
            {
                var severity = Math.Abs(zScore) >= 3.0 ? "High"
                    : Math.Abs(zScore) >= 2.5 ? "Medium"
                    : "Low";

                anomalies.Add(new AnomalyRecord
                {
                    DetectedDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                    ResourceId = latest.ResourceId,
                    ResourceName = latest.ResourceName,
                    ExpectedCost = (decimal)mean,
                    ActualCost = latest.Cost,
                    DeviationPercent = Math.Round(zScore * 100, 2),
                    Severity = severity
                });
            }
        }

        return anomalies;
    }

    private static double CalculateStdDev(List<double> values, double mean)
    {
        var sumOfSquares = values.Sum(v => Math.Pow(v - mean, 2));
        return Math.Sqrt(sumOfSquares / values.Count);
    }
}
