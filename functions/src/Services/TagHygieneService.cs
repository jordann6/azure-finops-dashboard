using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using FinOpsFunctions.Models;

namespace FinOpsFunctions.Services;

public class TagHygieneService
{
    private readonly ArmClient _armClient;
    private readonly string _subscriptionId;
    private readonly List<string> _requiredTags = new() { "project", "environment", "owner", "cost_center" };

    public TagHygieneService(string subscriptionId)
    {
        _armClient = new ArmClient(new DefaultAzureCredential());
        _subscriptionId = subscriptionId;
    }

    public async Task<TagHygieneResult> EvaluateTagComplianceAsync()
    {
        var subscription = await _armClient
            .GetSubscriptionResource(new Azure.Core.ResourceIdentifier($"/subscriptions/{_subscriptionId}"))
            .GetAsync();

        var result = new TagHygieneResult
        {
            RequiredTags = _requiredTags
        };

        var resources = subscription.Value.GetGenericResourcesAsync();

        await foreach (var resource in resources)
        {
            result.TotalResources++;

            var tags = resource.Data.Tags ?? new Dictionary<string, string>();
            var missingTags = _requiredTags
                .Where(t => !tags.ContainsKey(t))
                .ToList();

            if (missingTags.Count == 0)
            {
                result.TaggedResources++;
            }
            else
            {
                result.UntaggedResources++;
                result.Findings.Add(new TagFinding
                {
                    ResourceId = resource.Id.ToString(),
                    ResourceName = resource.Data.Name,
                    ResourceType = resource.Data.ResourceType.ToString(),
                    MissingTags = missingTags
                });
            }
        }

        result.CompliancePercent = result.TotalResources > 0
            ? Math.Round((double)result.TaggedResources / result.TotalResources * 100, 2)
            : 0;

        return result;
    }
}
