# Feature 03: Observable session that is easy to follow live

**ID:** `03-observable-demo-session`
**Depends on:** [02-bounded-deployment-verification](../02-bounded-deployment-verification/plan.md).
**Source:** [global spec](../../production-change-demo.md), §§3–4 and 7–11.
**Work:** [todo.md](todo.md).
**Status:** implementation complete and compiled. Presenter-owned E2E/rehearsal remains pending.

**Current verification scope:** the combined implementation request requires code and compilation only. Any manual/E2E requirements below are presenter follow-up, not agent execution requirements. No automated testing is permitted.

## Feature outcome

The audience can follow a complete run in the console and open a correlated trace in Aspire Dashboard: reads, approval question, deployment, iterations, and outcome. Output distinguishes harness facts, agent messages, and declared progress. Operational telemetry is sufficient without capturing conversations or payloads.

Operation instrumentation comes from F1/F2. This feature connects its export and makes the complete session visible; it adds no new business logic.

## Scope

- Configure the `TracerProvider`, demo resource, and OTLP in `Program.cs`, using source `WftEngineering.Demo` and a configurable local endpoint.
- Use instrumentation included in HarnessAgent and dedicated activities; avoid wrapping the same client in OpenTelemetry again.
- Ensure a `demo.run` root covers the human round trip, invocations, and outcome; include approval duration/decision and continuation events.
- Refine the existing Spectre.Console presentation and streaming for the Skill, verified data, tasks, approval arguments, iteration/observation, outcome, and TraceId; keep `IAnsiConsole` injected.
- Read todos through the session provider without inspecting internal reasoning or reconstructing progress from agent phrases.
- Check privacy, sanitized operational errors, final flush, and disposal on rejection/cancellation too.
- Make the spec's local dashboard command usable, without an AppHost or additional collector.

Out of scope: new tools, new policies, changed limits, full metrics/logs, optional prompt capture, a TUI, other exporters, or preparation of final conference material.

## Output and telemetry contracts

| Element | Source / presentation |
|---|---|
| Agent messages | Streaming labeled `Agent`; do not interpret it as the operational outcome |
| Validation and deployment | Typed results/store, labeled as `Harness` facts |
| Loaded Skill | An actually observed event/call; do not print a load that has not occurred |
| Tasks | GetAllTodosAsync from the same session; failures do not display pending verifications as completed |
| Approval | Arguments of the call bound by the framework; no “always approve” |
| Iteration / observation | F2 counters, separate from model calls and tool calls |
| Outcome / TraceId | Store plus trace context; Running still pending at the limit is not shown as canceled |

Always present final output from external state, even when model text announces false success. If todos are stale, show them as declared progress and preserve the operational outcome; do not create a second planner to reconcile them.

Allow tool names, durations, approval decision, statuses, counters, outcome, and fictional IDs. Keep `EnableSensitiveData` disabled in both client and agent layers; do not record prompts, responses, full arguments/results, HTTP bodies, secrets, or raw exceptions. Do not enable Debug indiscriminately for a more impressive trace.

Do not promise automatic span names or an exact tree. Check correlation, coverage, and operation evidence; add a dedicated activity only for a missing operational fact.

## Implementation sequence

1. **Session trace:** exporter, resource, source, correlation, and closure/flush over existing activities.
2. **Presentation console:** streaming and task/fact snapshots, human question, and an unambiguous summary. Checkpoint for a visible flow without regressions.
3. **Privacy and viewer:** privacy/source review, negative trace paths, and a manual local Aspire viewer check.

## Feature acceptance criteria

1. A run generates a correlated trace covering prior operations, approval waiting, deployment, iterations, and closure; the exporter can send it to the local dashboard.
2. The console is readable and consistent with the store in happy, reject, and stuck, with correct tasks and counters; the seven teaching elements remain visible in code.
3. Source review and available manual trace inspection confirm sensitive capture is disabled and client instrumentation is not duplicated; F1/F2 guards and limits remain intact.

## Planned manual viewer check

```bash
docker run --rm -d --name wft-dashboard \
  -p 127.0.0.1:18888:18888 -p 127.0.0.1:4317:18889 \
  mcr.microsoft.com/dotnet/aspire-dashboard:13.6.0
docker logs wft-dashboard
```

With the image available and Foundry configured, run `happy`, accept manually, and locate the TraceId through the authenticated dashboard link. Do not download images/packages during the public walkthrough. If Docker or credentials are missing, record compilation/source review and leave the live viewer check pending.

## Risks and simplification

- An exporter without a matching source delivers no data; check source configuration and inspect the actual trace when the viewer is available.
- Client OTel plus another wrapper can duplicate spans or content; use the harness pipeline and review the trace.
- Streaming can mix model calls with agent invocations; iteration labels use the external counter.
- Keep Spectre.Console presentation helpers small in `DemoApplication.cs`, with composition in `Program.cs`. Extract a file only if it clearly improves readability; do not add another console library or framework.

## Single implementation prompt

```text
Apply using-agent-skills and implement feature 03-observable-demo-session.
Read docs/specs/production-change-demo.md, docs/specs/features/README.md, and plan.md/todo.md in docs/specs/features/03-observable-demo-session/; check that features 01 and 02 are implemented and verified.
Complete the three tasks: connect OTel/OTLP to existing instrumentation, prepare readable console/streaming output, and manually inspect privacy and correlation. Preserve guards, external state, and limits.
Do not capture prompts, responses, or payloads, duplicate client instrumentation, or add a TUI/AppHost/collector. Use local standalone Aspire if the environment is available.
Update todo.md with evidence and actual pending viewer checks. Do not implement new scenarios or the conference script; no sub-agents, new layers, provisioning, or automatic commits. Do not add automated tests, a testing project, scripted model clients, or testing dependencies. Write documentation in English.
```
