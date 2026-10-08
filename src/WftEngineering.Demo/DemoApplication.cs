using System.Diagnostics;
using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Spectre.Console;

namespace WftEngineering.Demo;

internal sealed class DemoApplication(
    IAnsiConsole console, HarnessAgent agent, DemoDeploymentStore store, ActivitySource activities)
{
    public async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        using var run = activities.StartActivity("demo.run")
            ?? new Activity("demo.run").SetIdFormat(ActivityIdFormat.W3C).Start();
        run.SetTag("demo.scenario", "happy");
        var activeTime = new Stopwatch();
        console.Write(new FigletText("WTF").Color(Color.Cyan1));
        console.Write(new Panel("[bold]Production Change Assistant[/]\nPrompt · Context · Harness")
            .Header("Feature 01 — simulated production change").RoundedBorder().BorderColor(Color.Cyan1));
        console.WriteLine(store.Requested.ToPrompt());
        console.MarkupLine("[dim]Available Skill: production-change (loaded on demand). Ctrl+C cancels.[/]");

        try
        {
            var session = await agent.CreateSessionAsync(cancellationToken);
            var response = await RunAgentAsync(
                [new ChatMessage(ChatRole.User, store.Requested.ToPrompt())], session, activeTime, cancellationToken);
            await ShowProgressAsync(session, response, cancellationToken);

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
                await ShowProgressAsync(session, response, cancellationToken);
                if (ApprovalRequests(response).Length > 0)
                {
                    store.Stop(DemoOutcome.ValidationFailed, "A second approval round is outside feature 01.");
                }
            }

            if (store.Outcome == DemoOutcome.Pending && store.Deployment is null)
            {
                store.Stop(DemoOutcome.Error, "The agent returned without starting the approved deployment.");
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

        run.SetTag("demo.outcome", store.Outcome.ToString());
        run.SetTag("demo.active_ms", activeTime.ElapsedMilliseconds);
        ShowOutcome(run.TraceId.ToString());
        return store.Outcome == DemoOutcome.Pending ? 0 : 1;
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
        try
        {
            return await console.Status().Spinner(Spinner.Known.Dots).StartAsync("Agent working…",
                _ => agent.RunAsync(messages, session, cancellationToken: deadline.Token));
        }
        finally
        {
            activeTime.Stop();
        }
    }

    private async Task ShowProgressAsync(AgentSession session, AgentResponse response, CancellationToken cancellationToken)
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
        if (store.PriorHealthWasRead)
        {
            facts.AddRow("GetServiceHealth (prior)", $"{store.Health.Status} · v{store.Health.Version}");
        }
        console.Write(facts);

        var todos = await agent.GetService<TodoProvider>()!.GetAllTodosAsync(session, cancellationToken);
        var progress = new Table().RoundedBorder().AddColumn("Agent todos (declared progress)").AddColumn("State");
        foreach (var todo in todos)
        {
            progress.AddRow(new Text(todo.Title), new Text(todo.IsComplete ? "Done" : "Pending"));
        }
        console.Write(progress);
        if (!string.IsNullOrWhiteSpace(response.Text))
        {
            console.Write(new Panel(new Text(response.Text)).Header("Agent message").BorderColor(Color.Grey));
        }
    }

    private void ShowOutcome(string traceId)
    {
        var facts = new Table().RoundedBorder().BorderColor(Color.Cyan1)
            .AddColumn("Operational outcome (store facts)").AddColumn("Value");
        facts.AddRow("Outcome", store.Outcome.ToString());
        facts.AddRow("Deployment", store.Deployment is { } deployment
            ? $"{deployment.DeploymentId} · {deployment.Status}" : "None");
        facts.AddRow("Deployments started", store.Deployment is null ? "0" : "1");
        facts.AddRow("Verification", "Pending — feature 02");
        facts.AddRow("Trace ID (export in feature 03)", traceId);
        console.Write(facts);
        if (store.StopReason is { } reason)
        {
            console.Write(new Text(reason, new Style(Color.Yellow)));
            console.WriteLine();
        }
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
