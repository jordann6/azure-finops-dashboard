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

    public ApiFunction(ILogger<ApiFunction> logger)
    {
        _logger = logger;
    }

    private (CosmosService cosmos, string subscriptionId) GetServices()
    {
        var cosmosEndpoint = Environment.GetEnvironmentVariable("CosmosDb__Endpoint")
            ?? throw new InvalidOperationException("CosmosDb__Endpoint not configured");
        var databaseName = Environment.GetEnvironmentVariable("CosmosDb__DatabaseName")
            ?? throw new InvalidOperationException("CosmosDb__DatabaseName not configured");
        var subscriptionId = Environment.GetEnvironmentVariable("Azure__SubscriptionId")
            ?? throw new InvalidOperationException("Azure__SubscriptionId not configured");

        return (new CosmosService(cosmosEndpoint, databaseName), subscriptionId);
    }

    [Function("GetDailyCosts")]
    public async Task<HttpResponseData> GetDailyCosts(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "costs/daily")] HttpRequestData req)
    {
        var (cosmos, _) = GetServices();
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var startDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");

        var queryParams = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        if (queryParams["startDate"] != null) startDate = queryParams["startDate"]!;
        if (queryParams["endDate"] != null) endDate = queryParams["endDate"]!;

        var costs = await cosmos.GetCostsByDateRangeAsync(startDate, endDate);

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
        var (cosmos, _) = GetServices();
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var startDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");

        var costs = await cosmos.GetCostsByDateRangeAsync(startDate, endDate);

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
        var (_, subscriptionId) = GetServices();
        var tagService = new TagHygieneService(subscriptionId);
        var result = await tagService.EvaluateTagComplianceAsync();

        return await CreateJsonResponse(req, result);
    }

    [Function("GetAnomalies")]
    public async Task<HttpResponseData> GetAnomalies(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "anomalies")] HttpRequestData req)
    {
        var (cosmos, _) = GetServices();
        var anomalies = await cosmos.GetRecentAnomaliesAsync();

        return await CreateJsonResponse(req, anomalies);
    }

    [Function("GetForecasts")]
    public async Task<HttpResponseData> GetForecasts(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "forecasts")] HttpRequestData req)
    {
        var (cosmos, _) = GetServices();
        var forecasts = await cosmos.GetForecastsAsync();

        return await CreateJsonResponse(req, forecasts);
    }

    private static async Task<HttpResponseData> CreateJsonResponse<T>(HttpRequestData req, T data)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");
        response.Headers.Add("Access-Control-Allow-Origin", "*");
        await response.WriteStringAsync(JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        }));
        return response;
    }
}
