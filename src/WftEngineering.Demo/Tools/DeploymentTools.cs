using System.ComponentModel;
using System.Diagnostics;

namespace WftEngineering.Demo;

internal sealed class DeploymentTools(DemoDeploymentStore store, ActivitySource activities)
{
    public event Action<string, object?>? ToolCompleted;

    [Description("Read a change request, its approved target, maintenance window, and current demo time. Returns null if unavailable.")]
    public ChangeRequest? GetChangeRequest(string changeId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var activity = activities.StartActivity("tool.GetChangeRequest");
        var change = store.ReadChange(changeId);
        activity?.SetTag("change.found", change is not null);
        activity?.SetTag("demo.outcome", store.Outcome.ToString());
        ToolCompleted?.Invoke(nameof(GetChangeRequest), change);
        return change;
    }

    [Description("Read current service health and version. Returns null if the service is unknown or the run has stopped.")]
    public ServiceHealth? GetServiceHealth(string service, string environment, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var activity = activities.StartActivity("tool.GetServiceHealth");
        var health = store.ReadHealth(service, environment);
        activity?.SetTag("health.status", health?.Status ?? "Unknown");
        activity?.SetTag("health.phase", health?.Phase);
        activity?.SetTag("health.version", health?.Version);
        ToolCompleted?.Invoke(nameof(GetServiceHealth), health);
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
        ToolCompleted?.Invoke(nameof(DeployService), deployment);
        return deployment;
    }

    [Description("Observe a deployment without starting another one. One new observation per agent iteration; repeat calls return a cached snapshot. After Succeeded, verify service health and version.")]
    public DeploymentSnapshot? GetDeploymentStatus(string deploymentId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var activity = activities.StartActivity("tool.GetDeploymentStatus");
        var snapshot = store.ReadDeploymentStatus(deploymentId);
        activity?.SetTag("deployment.status", snapshot?.Status ?? "Unknown");
        activity?.SetTag("deployment.observation", snapshot?.ObservationNumber);
        activity?.SetTag("loop.iteration", store.CurrentIteration);
        activity?.SetTag("loop.run", store.LoopRunNumber);
        activity?.SetTag("deployment.cached", snapshot?.IsCached);
        ToolCompleted?.Invoke(nameof(GetDeploymentStatus), snapshot);
        return snapshot;
    }
}
