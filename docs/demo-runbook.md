# Production Change Assistant: conference runbook

**Implementation:** features 01–04 are implemented and compile. Foundry/Aspire E2E checks and conference rehearsal belong to the presenter and have not been run by the agent.

The demo uses a real Foundry model and an in-memory deployment system. CHG-1042, Atlas API, DEP-742, health, permissions, and the clock are fictional. Every launch starts a fresh store and native session. The clock is fixed at 8 October 2026, 12:30 UTC; the normal change window is 12:00–14:00 UTC.

## Prepare before the talk

Use the SDK pinned in `global.json` (`10.0.202`), an Azure CLI identity with project/model access, and a model deployment supporting Responses and function calling. Docker is needed only for the standalone Aspire viewer. No cloud provisioning, AppHost, collector, or additional application project is required.

Run from the repository root:

```bash
cp env.template .env
# Fill FOUNDRY_PROJECT_ENDPOINT and FOUNDRY_MODEL in .env.
az login
dotnet restore src/WftEngineering.slnx --locked-mode
dotnet build src/WftEngineering.slnx -c Release --no-restore
```

The template contains empty values. Process variables override `.env`. Optional OTLP variables default to `http://localhost:4317` and `grpc`; only gRPC is supported. Keep credentials and `.env` out of Git. Azure CLI identity authenticates model access; `OperatorCanDeploy` separately simulates deployment authorization inside the fake.

Start the proposed pinned viewer image before rehearsal:

```bash
docker run --rm -d --name wft-dashboard \
  -p 127.0.0.1:18888:18888 -p 127.0.0.1:4317:18889 \
  mcr.microsoft.com/dotnet/aspire-dashboard:13.6.0
docker logs wft-dashboard
```

Use the authenticated access link from the logs. Image availability, the access link, and successful OTLP delivery remain for the presenter to verify; this command has not been executed. Keep the verified image locally and record its digest before the talk. Do not restore packages, download images, or provision resources on stage.

## Run the demo

```bash
dotnet run --project src/WftEngineering.Demo -c Release --no-build -- --scenario happy
```

The model loads the local Skill, creates five native todos, and queries the approved change and prior Healthy v2.6.3 service. The host displays the proposed change/service/version/environment from the native approval request. Type exactly `APPROVE` to proceed. Any other input, including `REJECT` or EOF, rejects; Ctrl+C cancels waiting/execution.

The approval response is bound by the framework and sent to the same session. The tool still checks request/change/window/health/permission/read evidence. Human approval does not replace authorization.

Expected after approval: one DEP-742; observations Running, Running, Succeeded in separate native invocations; then a new Healthy v2.7.0 read with phase AfterDeployment. Only those facts allow Completed. The agent's streamed text and completed todos cannot establish operational success.

## Available scenarios

Change only the final scenario argument in the command above:

| Scenario / human decision | Fake difference | Expected operational outcome |
|---|---|---|
| `happy` + `APPROVE` | Normal data | Completed after subsequent health/version verification |
| `happy` + `REJECT` | Human rejection | Rejected; zero deployments |
| `stuck` + `APPROVE` | Deployment stays Running | ExecutionLimitReached; DEP-742 still Running |
| `validation-failed` | Window ends at 12:00, before fixed current time | ValidationFailed; no approval question or deployment |
| `deployment-failed` + `APPROVE` | Third observation is Failed | DeploymentFailed; no rollback |
| `post-health-failed` + `APPROVE` | Health becomes Unhealthy after Succeeded | PostHealthFailed; no completion claim |

These are expected results, not recorded E2E outcomes. A model that omits necessary tool calls may finish incomplete or hit a budget instead of reaching the intended scenario outcome. Limits and guards are identical in every scenario; no flag automatically approves.

Help (`--help`) and no-argument startup are supported; no arguments selects happy. Unknown arguments exit 2 before model invocation. Completed/help exit 0; other operational outcomes/configuration failures exit 1.

## Explain the boundaries

- `MaxIterations = 4` limits each native loop run. Approval yields to the host; resumption starts a new run with its own four-invocation budget and the same session. One approval round trip is allowed.
- `MaximumIterationsPerRequest = 12` separately limits internal function calling. A single invocation can contain several model/tool calls.
- The first valid status query in an invocation consumes one new observation. Repeats return `IsCached = true`; they cannot fast-forward the fake. Omitting status does not advance it.
- Active model/tool execution has a shared 120-second deadline; keyboard waiting is excluded. Model transport permits one retry. Repeated deployment is idempotent, not a transport retry of the side effect.
- A terminal outcome stops further business actions. Stopping the harness does not cancel or roll back an already Running deployment.

## Twelve-minute walkthrough

| Time | Show / explain |
|---|---|
| 0–2 min | The five Engineering concepts; short instructions, the Skill, and external data through tools |
| 2–4 min | Run happy; show the actual Skill load, change/window/health facts, and native todos |
| 4–6 min | Pause on exact approval arguments; explain approval versus code authorization, then type APPROVE |
| 6–8 min | Follow native iteration headers and observation numbers; show the cached-read guard and external-state evaluator |
| 8–10 min | Compare subsequent health/version and Completed with declared todos; copy Trace ID into Aspire |
| 10–12 min | Run stuck or reject a fresh happy run; explain bounded autonomy and unresolved external work |

Seven code elements to locate quickly:

| Element | Location / teaching point |
|---|---|
| Model connection | [Program.cs](../src/WftEngineering.Demo/Program.cs): AIProjectClient → Responses → IChatClient, with Azure CLI identity |
| Skill | [SKILL.md](../src/WftEngineering.Demo/skills/production-change/SKILL.md): procedure loaded on demand |
| Four business tools | [DeploymentTools.cs](../src/WftEngineering.Demo/Tools/DeploymentTools.cs): typed reads/deployment/status; the framework adds auxiliary Skill/todo tools |
| Approval wrapper and host | Program.cs: ApprovalRequiredAIFunction; [DemoApplication.cs](../src/WftEngineering.Demo/DemoApplication.cs): exact arguments, CreateResponse, same session |
| External state / graph | [DemoDeploymentStore.cs](../src/WftEngineering.Demo/Demo/DemoDeploymentStore.cs): validation → deployment → verification, terminal branches and independent evidence |
| Native loop and limits | Program.cs: one DelegateLoopEvaluator and MaxIterations; store snapshot caching, host deadline and limit classification |
| OpenTelemetry | Program.cs: tracer/source/exporter; tool activities, approval.wait, loop.evaluate, demo.run, and Trace ID |

Use the graph in the [spec](specs/production-change-demo.md) to explain branches. No workflow engine is involved.

## Trace and recovery

Find the final Trace ID in Aspire under service `WftEngineering.Demo`. Expected coverage includes model/agent spans from the harness, four dedicated tool activities, approval duration/decision, loop evaluation, resumption, and final operational tags. The fourth invocation may end at the limit without another evaluator call; use root outcome/counters too. Exact automatic span names/nesting depend on the pinned framework.

Sensitive capture is explicitly disabled. No transcript, full tool payloads, or raw exceptions are intentionally exported; `SafeErrorProcessor` clears error status descriptions before export. Inspect actual delivery, correlation, and privacy during your E2E checks. A displayed Trace ID does not prove successful delivery.

Exporter request/processor timeouts are 1 second; shutdown attempts a 2-second flush and a 2-second shutdown. Export warnings do not change the business outcome. If traces are missing, check the container, authenticated viewer, configured endpoint, and matching source; do not enable sensitive capture to diagnose it.

If startup fails, check required variables, `az login`, project access, and model compatibility. If an invocation fails or times out, show the store's incomplete outcome and start a fresh run only after resolving the cause. Do not raise limits or bypass approval to make the demo appear successful.

Prepare a rehearsal recording or exported trace yourself as a backup, clearly labeled **recorded**. No recording, slides, or publication were generated by this implementation.

## Presenter-owned rehearsal record

Record E2E results in [rehearsal.md](specs/features/04-conference-scenarios/rehearsal.md): model/deployment, SDK/packages/image digest, scenario/decision, outcome, counters, side effects, active duration, Trace ID and privacy observations. Aim for five consecutive happy runs below 90 seconds of active work, plus reject and stuck. Do not copy conversations or credentials into the record. Conference readiness remains pending until you complete these checks.
