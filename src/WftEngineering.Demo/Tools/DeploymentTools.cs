using System.ComponentModel;
using System.Diagnostics;

namespace WftEngineering.Demo;

internal sealed class DeploymentTools(DemoDeploymentStore store, ActivitySource activities)
{
    [Description("Read a change request, its approved target, maintenance window, and current demo time. Returns null if unavailable.")]
    public ChangeRequest? GetChangeRequest(string changeId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var activity = activities.StartActivity("tool.GetChangeRequest");
        var change = store.ReadChange(changeId);
        activity?.SetTag("change.found", change is not null);
        activity?.SetTag("demo.outcome", store.Outcome.ToString());
        return change;
    }

    [Description("Read current service health and version. Returns null if the service is unknown or the run has stopped.")]
    public ServiceHealth? GetServiceHealth(string service, string environment, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var activity = activities.StartActivity("tool.GetServiceHealth");
        var health = store.ReadHealth(service, environment);
        activity?.SetTag("health.status", health?.Status ?? "Unknown");
        return health;
    }

    [Description("Start an in-memory deployment after human approval and code authorization. Returns Running, not completed or verified. Repeating identical arguments is idempotent.")]
    public Deployment? DeployService(
        string changeId, string service, string version, string environment,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var activity = activities.StartActivity("tool.DeployService");
        var deployment = store.StartDeployment(new(changeId, service, version, environment));
        activity?.SetTag("deployment.id", deployment?.DeploymentId);
        activity?.SetTag("deployment.status", deployment?.Status ?? "Denied");
        return deployment;
    }
}
