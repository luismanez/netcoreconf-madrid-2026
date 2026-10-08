using System.Diagnostics;
using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Spectre.Console;

namespace WftEngineering.Demo;

internal sealed class DemoApplication(
    IAnsiConsole console, HarnessAgent agent, DemoDeploymentStore store, DeploymentTools tools, ActivitySource activities)
{
    private bool agentTextStarted;

    public async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        using var run = activities.StartActivity("demo.run")
            ?? new Activity("demo.run").SetIdFormat(ActivityIdFormat.W3C).Start();
        run.SetTag("demo.scenario", store.Scenario.ToString());
        var activeTime = new Stopwatch();
        console.Write(new FigletText("WTF").Color(Color.Cyan1));
        console.Write(new Panel("[bold]Production Change Assistant[/]\nPrompt · Context · Loop · Graph · Harness")
            .Header("Simulated production change").RoundedBorder().BorderColor(Color.Cyan1));
        console.WriteLine(store.Requested.ToPrompt());
        console.MarkupLine("[dim]Available Skill: production-change (loaded on demand). Ctrl+C cancels.[/]");
        store.IterationStarted += ShowIteration;
        tools.ToolCompleted += ShowToolResult;

        try
        {
            var session = await agent.CreateSessionAsync(cancellationToken);
            var response = await RunAgentAsync(
                [new ChatMessage(ChatRole.User, store.Requested.ToPrompt())], session, activeTime, cancellationToken);
            await ShowProgressAsync(session, cancellationToken);

            var requests = ApprovalRequests(response);
            if (requests.Length > 0)
            {
                var target = requests.Length == 1 ? DeploymentTarget(requests[0]) : null;
                var denial = target is null
                    ? "Expected one deployment approval with exactly four target arguments."
                    : store.DeploymentDenial(target);
                var approved = false;
                if (denial is not null)
                {
                    store.Stop(DemoOutcome.ValidationFailed, denial);
                    console.Write(new Text(denial, new Style(Color.Red)));
                    console.WriteLine();
                }
                else
                {
                    using var approval = activities.StartActivity("approval.wait");
                    approval?.SetTag("approval.decision", "pending");
                    console.Write(new Panel(new Text(
                        $"Change: {target!.ChangeId}\nService: {target.Service}\nVersion: {target.Version}\nEnvironment: {target.Environment}\nDeployments started: 0"))
                        .Header("Human approval — DeployService").BorderColor(Color.Yellow).RoundedBorder());
                    console.MarkupLine("Type [bold green]APPROVE[/] to start this simulated deployment; any other input rejects it:");
                    // Console input reads synchronously; allow Ctrl+C to stop waiting without another Enter.
                    approved = await Task.Run(() => Console.ReadLine(), cancellationToken)
                        .WaitAsync(cancellationToken) == "APPROVE";
                    approval?.SetTag("approval.decision", approved ? "approved" : "rejected");
                    run.AddEvent(new ActivityEvent(approved ? "approval.approved" : "approval.rejected"));
                    if (approved)
                    {
                        store.RecordApproval(target);
                    }
                    else
                    {
                        store.Stop(DemoOutcome.Rejected, "The operator did not approve deployment.");
                    }
                }

                // Resume the native protocol in the same session, at most once. No host polling loop.
                var reply = new ChatMessage(ChatRole.User,
                    requests.Select(request => (AIContent)request.CreateResponse(approved)).ToList());
                run.AddEvent(new ActivityEvent("agent.resume"));
                response = await RunAgentAsync([reply], session, activeTime, cancellationToken);
                await ShowProgressAsync(session, cancellationToken);
                if (ApprovalRequests(response).Length > 0)
                {
                    store.Stop(DemoOutcome.ValidationFailed, "Only one deployment approval round is allowed.");
                }
            }
        }
        catch (OperationCanceledException)
        {
            store.Stop(DemoOutcome.Cancelled, cancellationToken.IsCancellationRequested
                ? "The operator cancelled the run." : "The 120-second active execution budget expired.");
        }
        catch (Exception)
        {
            store.Stop(DemoOutcome.Error, "Agent execution failed. Check Azure CLI sign-in, Foundry access, and model compatibility.");
        }
        finally
        {
            store.IterationStarted -= ShowIteration;
            tools.ToolCompleted -= ShowToolResult;
        }

        run.SetTag("demo.outcome", store.Outcome.ToString());
        run.SetTag("demo.active_ms", activeTime.ElapsedMilliseconds);
        run.SetTag("deployment.id", store.Deployment?.DeploymentId);
        run.SetTag("deployment.status", store.Deployment?.Status);
        run.SetTag("deployment.observations", store.ObservationCount);
        run.SetTag("loop.run", store.LoopRunNumber);
        run.SetTag("loop.iteration", store.CurrentIteration);
        run.SetStatus(store.Outcome == DemoOutcome.Completed ? ActivityStatusCode.Ok : ActivityStatusCode.Error);
        ShowOutcome(run.TraceId.ToString());
        return store.Outcome == DemoOutcome.Completed ? 0 : 1;
    }

    private async Task<AgentResponse> RunAgentAsync(
        IReadOnlyList<ChatMessage> messages, AgentSession session, Stopwatch activeTime, CancellationToken cancellationToken)
    {
        // Only model/tool execution consumes this budget; keyboard approval time is excluded.
        var remaining = TimeSpan.FromSeconds(120) - activeTime.Elapsed;
        if (remaining <= TimeSpan.Zero)
        {
            throw new OperationCanceledException();
        }
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(remaining);
        activeTime.Start();
        store.BeginLoopRun();
        try
        {
            var updates = new List<AgentResponseUpdate>();
            await foreach (var update in agent.RunStreamingAsync(messages, session, cancellationToken: deadline.Token))
            {
                updates.Add(update);
                if (update.Role != ChatRole.User && update.Role != ChatRole.Tool && update.Role != ChatRole.System
                    && !string.IsNullOrEmpty(update.Text))
                {
                    if (!agentTextStarted)
                    {
                        console.Markup("[green]Agent:[/] ");
                        agentTextStarted = true;
                    }
                    console.Write(new Text(update.Text));
                }
            }
            console.WriteLine();
            // The framework merges streamed content, including native approval requests.
            var response = updates.ToAgentResponse();
            if (ApprovalRequests(response).Length == 0 && store.Outcome == DemoOutcome.Pending)
            {
                store.Stop(store.CurrentIteration >= 4 ? DemoOutcome.ExecutionLimitReached : DemoOutcome.Error,
                    store.CurrentIteration >= 4
                        ? "The harness reached its four-invocation limit with evidence still pending."
                        : "The agent returned before operational verification was complete.");
            }
            return response;
        }
        finally
        {
            activeTime.Stop();
        }
    }

    private async Task ShowProgressAsync(AgentSession session, CancellationToken cancellationToken)
    {
        var history = agent.GetService<InMemoryChatHistoryProvider>()?.GetMessages(session);
        var contents = history?.SelectMany(message => message.Contents).ToList() ?? [];
        var loaded = contents.OfType<FunctionCallContent>().Any(call =>
            call.Name == AgentSkillsProvider.LoadSkillToolName
            && contents.OfType<FunctionResultContent>().Any(result => result.CallId == call.CallId
                && result.Exception is null
                && ReadString(result.Result)?.Contains("# Production change procedure", StringComparison.Ordinal) == true));
        console.MarkupLine(loaded ? "[green]Skill loaded through the native provider.[/]" : "[yellow]Skill load not observed.[/]");

        var facts = new Table().RoundedBorder().AddColumn("External context (tool reads)").AddColumn("Evidence");
        if (store.ChangeWasRead)
        {
            facts.AddRow("GetChangeRequest", $"{store.Change.ChangeId} · {store.Change.Status}");
            facts.AddRow("UTC window / fixed demo time",
                $"{store.Change.WindowStart:HH:mm}–{store.Change.WindowEnd:HH:mm} / {store.Now:HH:mm}");
        }
        if (store.PriorHealth is { } prior)
        {
            facts.AddRow("GetServiceHealth (prior)", $"{prior.Status} · v{prior.Version}");
        }
        console.Write(facts);

        var todos = await agent.GetService<TodoProvider>()!.GetAllTodosAsync(session, cancellationToken);
        var progress = new Table().RoundedBorder().AddColumn("Agent todos (declared progress)").AddColumn("State");
        foreach (var todo in todos)
        {
            progress.AddRow(new Text(todo.Title), new Text(todo.IsComplete ? "Done" : "Pending"));
        }
        console.Write(progress);
    }

    private void ShowOutcome(string traceId)
    {
        var facts = new Table().RoundedBorder().BorderColor(Color.Cyan1)
            .AddColumn("Operational outcome (store facts)").AddColumn("Value");
        facts.AddRow("Outcome", store.Outcome.ToString());
        facts.AddRow("Deployment", store.Deployment is { } deployment
            ? $"{deployment.DeploymentId} · {deployment.Status}" : "None");
        facts.AddRow("Deployments started", store.Deployment is null ? "0" : "1");
        facts.AddRow("Loop run / last iteration", $"{store.LoopRunNumber} / {store.CurrentIteration} of 4");
        facts.AddRow("New deployment observations", store.ObservationCount.ToString());
        facts.AddRow("Post-deployment health", store.PostDeploymentHealth is { } post
            ? $"{post.Status} · v{post.Version}" : "Not yet observed");
        facts.AddRow("Verification", store.Outcome == DemoOutcome.Completed ? "Verified" : "Incomplete");
        facts.AddRow("Trace ID", traceId);
        console.Write(facts);
        if (store.Outcome is DemoOutcome.ExecutionLimitReached or DemoOutcome.Cancelled or DemoOutcome.Error
            && store.Deployment?.Status == "Running")
        {
            console.WriteLine("The harness stopped waiting; the deployment remains Running. It was not cancelled or rolled back.");
        }
        if (store.StopReason is { } reason)
        {
            console.Write(new Text(reason, new Style(Color.Yellow)));
            console.WriteLine();
        }
    }

    private void ShowIteration()
    {
        agentTextStarted = false;
        console.WriteLine();
        console.Write(new Rule($"[cyan]Harness · run {store.LoopRunNumber} · iteration {store.CurrentIteration}/4[/]"));
    }

    private void ShowToolResult(string toolName, object? result)
    {
        agentTextStarted = false;
        var evidence = result switch
        {
            ChangeRequest change => $"{change.ChangeId} · {change.Status} · UTC window {change.WindowStart:HH:mm}–{change.WindowEnd:HH:mm}, demo time {change.CurrentTime:HH:mm}",
            ServiceHealth health => $"{health.Phase} · {health.Status} · v{health.Version}",
            Deployment deployment => $"{deployment.DeploymentId} · {deployment.Status}",
            DeploymentSnapshot snapshot => $"{snapshot.DeploymentId} · {snapshot.Status} · observation {snapshot.ObservationNumber} · {(snapshot.IsCached ? "cached" : "new")}",
            _ => "Unavailable or denied"
        };
        console.WriteLine();
        console.Write(new Text($"Harness · {toolName}: {evidence}", new Style(Color.Cyan1)));
        console.WriteLine();
    }

    private static ToolApprovalRequestContent[] ApprovalRequests(AgentResponse response) =>
        response.Messages.SelectMany(message => message.Contents).OfType<ToolApprovalRequestContent>().ToArray();

    private static RequestedChange? DeploymentTarget(ToolApprovalRequestContent request)
    {
        if (request.ToolCall is not FunctionCallContent { Name: nameof(DeploymentTools.DeployService), Arguments: { Count: 4 } arguments })
        {
            return null;
        }
        string? Read(string name) => arguments.TryGetValue(name, out var value) ? ReadString(value) : null;
        return (Read("changeId"), Read("service"), Read("version"), Read("environment")) is
            (string changeId, string service, string version, string environment)
                ? new RequestedChange(changeId, service, version, environment) : null;
    }

    private static string? ReadString(object? value) => value switch
    {
        string text => text,
        JsonElement { ValueKind: JsonValueKind.String } json => json.GetString(),
        _ => null
    };
}
