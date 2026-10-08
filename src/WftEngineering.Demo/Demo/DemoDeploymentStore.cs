namespace WftEngineering.Demo;

internal sealed class DemoDeploymentStore
{
    public RequestedChange Requested { get; } = new("CHG-1042", "Atlas API", "2.7.0", "Production");
    public DateTimeOffset Now { get; } = new(2026, 10, 8, 12, 30, 0, TimeSpan.Zero);
    public ChangeRequest Change { get; } = new(
        "CHG-1042", "Atlas API", "2.7.0", "Production", "Approved",
        new(2026, 10, 8, 12, 0, 0, TimeSpan.Zero),
        new(2026, 10, 8, 14, 0, 0, TimeSpan.Zero),
        new(2026, 10, 8, 12, 30, 0, TimeSpan.Zero));
    public ServiceHealth Health { get; } = new("Atlas API", "Production", "Healthy", "2.6.3", "BeforeDeployment");
    public bool OperatorCanDeploy { get; } = true;
    public bool ChangeWasRead { get; private set; }
    public bool PriorHealthWasRead { get; private set; }
    public Deployment? Deployment { get; private set; }
    public DemoOutcome Outcome { get; private set; } = DemoOutcome.Pending;
    public string? StopReason { get; private set; }
    private RequestedChange? ApprovedTarget { get; set; }

    public ChangeRequest? ReadChange(string changeId)
    {
        if (Outcome != DemoOutcome.Pending)
        {
            return null;
        }

        if (changeId != Change.ChangeId)
        {
            Stop(DemoOutcome.ValidationFailed, "Change not found.");
            return null;
        }

        ChangeWasRead = true;
        if (Change.Status != "Approved" || Now < Change.WindowStart || Now > Change.WindowEnd
            || Requested != new RequestedChange(Change.ChangeId, Change.Service, Change.Version, Change.Environment))
        {
            Stop(DemoOutcome.ValidationFailed, "Change approval, target, or window is invalid.");
        }

        return Change;
    }

    public ServiceHealth? ReadHealth(string service, string environment)
    {
        if (Outcome != DemoOutcome.Pending)
        {
            return null;
        }

        if (service != Health.Service || environment != Health.Environment || Health.Status != "Healthy")
        {
            Stop(DemoOutcome.ValidationFailed, "Service health is unknown, unhealthy, or does not match the target.");
            return null;
        }

        if (Deployment is null)
        {
            PriorHealthWasRead = true;
        }

        return Deployment is null ? Health : Health with { Phase = "DeploymentRunning" };
    }

    public string? DeploymentDenial(RequestedChange target)
    {
        if (Outcome != DemoOutcome.Pending)
        {
            return "The run has stopped; new actions are not allowed.";
        }

        if (target != Requested || target.Environment != "Production"
            || target != new RequestedChange(Change.ChangeId, Change.Service, Change.Version, Change.Environment))
        {
            return "Deployment arguments must match both the operator request and the approved change.";
        }

        if (!ChangeWasRead || !PriorHealthWasRead)
        {
            return "Read the change and current service health before requesting deployment.";
        }

        if (Change.Status != "Approved" || Now < Change.WindowStart || Now > Change.WindowEnd
            || Health.Status != "Healthy" || !OperatorCanDeploy)
        {
            return "Change approval, window, health, or operator permission does not allow deployment.";
        }

        return null;
    }

    public void RecordApproval(RequestedChange target) => ApprovedTarget = target;

    public Deployment? StartDeployment(RequestedChange target)
    {
        // Approval allows the protocol to continue; these guards still authorize the operation.
        var denial = DeploymentDenial(target);
        if (denial is not null || ApprovedTarget != target)
        {
            Stop(DemoOutcome.ValidationFailed, denial ?? "Explicit human approval is required.");
            return null;
        }

        // Repeated identical calls return the existing operation, never a second deployment.
        return Deployment ??= new Deployment("DEP-742", target, "Running");
    }

    public void Stop(DemoOutcome outcome, string reason)
    {
        if (Outcome == DemoOutcome.Pending)
        {
            Outcome = outcome;
            StopReason = reason;
        }
    }
}

internal sealed record RequestedChange(string ChangeId, string Service, string Version, string Environment)
{
    public string ToPrompt() => $"Deploy {Service} v{Version} to {Environment} using change {ChangeId}.";
}

internal sealed record ChangeRequest(
    string ChangeId, string Service, string Version, string Environment, string Status,
    DateTimeOffset WindowStart, DateTimeOffset WindowEnd, DateTimeOffset CurrentTime);

internal sealed record ServiceHealth(string Service, string Environment, string Status, string Version, string Phase);
internal sealed record Deployment(string DeploymentId, RequestedChange Target, string Status);
internal enum DemoOutcome { Pending, ValidationFailed, Rejected, Cancelled, Error }
