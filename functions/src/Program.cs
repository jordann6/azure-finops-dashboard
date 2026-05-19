using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FinOpsFunctions.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddSingleton(sp =>
{
    var endpoint = Environment.GetEnvironmentVariable("CosmosDb__Endpoint")
        ?? throw new InvalidOperationException("CosmosDb__Endpoint not configured");
    var databaseName = Environment.GetEnvironmentVariable("CosmosDb__DatabaseName")
        ?? throw new InvalidOperationException("CosmosDb__DatabaseName not configured");
    return new CosmosService(endpoint, databaseName);
});

builder.Services.AddSingleton(sp =>
{
    var subscriptionId = Environment.GetEnvironmentVariable("Azure__SubscriptionId")
        ?? throw new InvalidOperationException("Azure__SubscriptionId not configured");
    return new CostIngestionService(subscriptionId);
});

builder.Services.AddSingleton(sp =>
{
    var subscriptionId = Environment.GetEnvironmentVariable("Azure__SubscriptionId")
        ?? throw new InvalidOperationException("Azure__SubscriptionId not configured");
    return new TagHygieneService(subscriptionId);
});

builder.Services.AddSingleton<AnomalyDetectionService>();
builder.Services.AddSingleton<ForecastService>();

builder.Build().Run();
