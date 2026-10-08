namespace WftEngineering.Demo;

internal sealed class DemoDeploymentStore(DemoScenario scenario)
{
    public DemoScenario Scenario { get; } = scenario;
    public RequestedChange Requested { get; } = new("CHG-1042", "Atlas API", "2.7.0", "Production");
    public DateTimeOffset Now { get; } = new(2026, 10, 8, 12, 30, 0, TimeSpan.Zero);
    public ChangeRequest Change { get; } = new(
        "CHG-1042", "Atlas API", "2.7.0", "Production", "Approved",
        new(2026, 10, 8, 12, 0, 0, TimeSpan.Zero),
        new(2026, 10, 8, scenario == DemoScenario.ValidationFailed ? 12 : 14, 0, 0, TimeSpan.Zero),
        new(2026, 10, 8, 12, 30, 0, TimeSpan.Zero));
    public ServiceHealth Health { get; private set; } = new("Atlas API", "Production", "Healthy", "2.6.3", "BeforeDeployment");
    public bool OperatorCanDeploy { get; } = true;
    public bool ChangeWasRead { get; private set; }
    public bool PriorHealthWasRead => PriorHealth is not null;
    public ServiceHealth? PriorHealth { get; private set; }
    public ServiceHealth? PostDeploymentHealth { get; private set; }
    public Deployment? Deployment { get; private set; }
    public DemoOutcome Outcome { get; private set; } = DemoOutcome.Pending;
    public string? StopReason { get; private set; }
    public int LoopRunNumber { get; private set; }
    public int CurrentIteration { get; private set; }
    public int ObservationCount { get; private set; }
    private RequestedChange? ApprovedTarget { get; set; }
    private DeploymentSnapshot? Snapshot { get; set; }
    public event Action? IterationStarted;

    public void BeginLoopRun()
    {
        LoopRunNumber++;
        BeginIteration(1);
    }

    public void BeginIteration(int iteration)
    {
        CurrentIteration = iteration;
        Snapshot = null;
        IterationStarted?.Invoke();
    }

    public string? ContinuationFeedback()
    {
        if (Outcome != DemoOutcome.Pending)
        {
            return null;
        }
        if (!ChangeWasRead || !PriorHealthWasRead)
        {
            return "Validation is incomplete. Follow the production-change Skill and query the change and prior service health.";
        }
        if (Deployment is null)
        {
            return "Prior evidence is valid. Propose DeployService for the exact requested target and wait for native human approval.";
        }
        if (Deployment.Status == "Succeeded")
        {
            return "Deployment succeeded, but post-deployment verification is missing. Query service health and verify the requested version.";
        }
        return $"Deployment {Deployment.DeploymentId} remains Running after {ObservationCount} observations. Query GetDeploymentStatus once in this next iteration; do not redeploy.";
    }

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
        if (service != Health.Service || environment != Health.Environment)
        {
            Stop(DemoOutcome.ValidationFailed, "Service health does not match the requested target.");
            return null;
        }
        if (Deployment is null)
        {
            if (Health.Status != "Healthy")
            {
                Stop(DemoOutcome.ValidationFailed, "Prior service health is unknown or unhealthy.");
                return null;
            }
            return PriorHealth = Health;
        }
        if (Deployment.Status != "Succeeded")
        {
            return Health with { Phase = "DeploymentRunning" };
        }

        PostDeploymentHealth = Health with { Phase = "AfterDeployment" };
        var verified = PostDeploymentHealth.Status == "Healthy" && PostDeploymentHealth.Version == Requested.Version;
        Stop(verified ? DemoOutcome.Completed : DemoOutcome.PostHealthFailed,
            verified
                ? "Deployment and subsequent health/version are verified."
                : "Post-deployment health or version does not match the expected result.");
        return PostDeploymentHealth;
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

    public DeploymentSnapshot? ReadDeploymentStatus(string deploymentId)
    {
        if (Outcome != DemoOutcome.Pending)
        {
            return null;
        }
        if (Deployment is null || deploymentId != Deployment.DeploymentId || CurrentIteration < 1)
        {
            Stop(DemoOutcome.ValidationFailed, "Deployment ID is unknown or no agent iteration is active.");
            return null;
        }
        if (Snapshot is not null)
        {
            return Snapshot with { IsCached = true };
        }

        // Only a new iteration can consume another observation. Repeated tool calls cannot fast-forward.
        ObservationCount++;
        if (Deployment.Status == "Running")
        {
            if (Scenario != DemoScenario.Stuck && ObservationCount >= 3)
            {
                Deployment = Deployment with { Status = Scenario == DemoScenario.DeploymentFailed ? "Failed" : "Succeeded" };
                if (Deployment.Status == "Failed")
                {
                    Stop(DemoOutcome.DeploymentFailed, "The simulated deployment failed. No rollback was attempted.");
                }
                else
                {
                    Health = Health with
                    {
                        Version = Requested.Version,
                        Status = Scenario == DemoScenario.PostHealthFailed ? "Unhealthy" : "Healthy"
                    };
                }
            }
        }
        Snapshot = new(Deployment.DeploymentId, Deployment.Status, ObservationCount, LoopRunNumber, CurrentIteration, false);
        return Snapshot;
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
internal sealed record DeploymentSnapshot(
    string DeploymentId, string Status, int ObservationNumber, int LoopRunNumber, int Iteration, bool IsCached);
internal enum DemoScenario { Happy, Stuck, ValidationFailed, DeploymentFailed, PostHealthFailed }
internal enum DemoOutcome
{
    Pending, ValidationFailed, Rejected, DeploymentFailed, PostHealthFailed,
    Completed, ExecutionLimitReached, Cancelled, Error
}
