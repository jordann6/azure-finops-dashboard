using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using FinOpsFunctions.Services;

namespace FinOpsFunctions.Functions;

public class ApiFunction
{
    private readonly ILogger<ApiFunction> _logger;
    private readonly CosmosService _cosmos;
    private readonly TagHygieneService _tagService;
    private static readonly string _corsOrigin =
        Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGIN") ?? "*";

    public ApiFunction(ILogger<ApiFunction> logger, CosmosService cosmos, TagHygieneService tagService)
    {
        _logger = logger;
        _cosmos = cosmos;
        _tagService = tagService;
    }

    [Function("GetDailyCosts")]
    public async Task<HttpResponseData> GetDailyCosts(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "costs/daily")] HttpRequestData req)
    {
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var startDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");

        var queryParams = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        if (queryParams["startDate"] is string sd && IsValidDate(sd)) startDate = sd;
        if (queryParams["endDate"] is string ed && IsValidDate(ed)) endDate = ed;

        var costs = await _cosmos.GetCostsByDateRangeAsync(startDate, endDate);

        var dailyTotals = costs
            .GroupBy(c => c.UsageDate)
            .Select(g => new { date = g.Key, total = g.Sum(c => c.Cost), count = g.Count() })
            .OrderBy(d => d.date)
            .ToList();

        return await CreateJsonResponse(req, dailyTotals);
    }

    [Function("GetCostsByResource")]
    public async Task<HttpResponseData> GetCostsByResource(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "costs/by-resource")] HttpRequestData req)
    {
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var startDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");

        var costs = await _cosmos.GetCostsByDateRangeAsync(startDate, endDate);

        var byResource = costs
            .GroupBy(c => new { c.ResourceName, c.ResourceType })
            .Select(g => new
            {
                resourceName = g.Key.ResourceName,
                resourceType = g.Key.ResourceType,
                totalCost = g.Sum(c => c.Cost),
                avgDailyCost = g.Average(c => c.Cost)
            })
            .OrderByDescending(r => r.totalCost)
            .ToList();

        return await CreateJsonResponse(req, byResource);
    }

    [Function("GetTagHygiene")]
    public async Task<HttpResponseData> GetTagHygiene(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tags/hygiene")] HttpRequestData req)
    {
        var result = await _tagService.EvaluateTagComplianceAsync();
        return await CreateJsonResponse(req, result);
    }

    [Function("GetAnomalies")]
    public async Task<HttpResponseData> GetAnomalies(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "anomalies")] HttpRequestData req)
    {
        var anomalies = await _cosmos.GetRecentAnomaliesAsync();
        return await CreateJsonResponse(req, anomalies);
    }

    [Function("GetForecasts")]
    public async Task<HttpResponseData> GetForecasts(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "forecasts")] HttpRequestData req)
    {
        var forecasts = await _cosmos.GetForecastsAsync();
        return await CreateJsonResponse(req, forecasts);
    }

    private static bool IsValidDate(string date) =>
        DateTime.TryParseExact(date, "yyyy-MM-dd", null,
            System.Globalization.DateTimeStyles.None, out _);

    private static async Task<HttpResponseData> CreateJsonResponse<T>(HttpRequestData req, T data)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");
        response.Headers.Add("Access-Control-Allow-Origin", _corsOrigin);
        await response.WriteStringAsync(JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        }));
        return response;
    }
}
