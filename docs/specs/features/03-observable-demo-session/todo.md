# Tasks: 03-observable-demo-session

Initial status: pending; requires F2 complete and F1 implementation complete.

## F3-T1 — Export a trace covering the complete session

- [ ] Implemented and verified.

**Description:** connect source/exporter and complete correlation/lifecycle of the trace instrumented during earlier features.

**Dependencies:** F2 complete.
**Scope:** M, up to 4 edited files.

**Planned files:**

- `src/WftEngineering.Demo/Program.cs`
- `src/WftEngineering.Demo/WftEngineering.Demo.csproj`, only if the OTLP reference is still missing
- `src/WftEngineering.Demo/Tools/DeploymentTools.cs`, only for gaps in operational coverage
- `src/WftEngineering.Demo/Demo/DemoDeploymentStore.cs`, only for gaps in state events
- Generated if the reference changes: lock files for affected projects

**Acceptance:**

- [ ] TracerProvider listens to WftEngineering.Demo, exports to configurable local OTLP, and preserves harness instrumentation without another wrapper around the same client.
- [ ] demo.run correlates validation, approval, and subsequent runs; spans/events contain operations and state, not a transcript. The pause includes decision and duration.
- [ ] Closure, cancellation, and failure flush/dispose within a bounded budget and show TraceId; an exporter error is not confused with deployment failure.

**Verification:**

```bash
dotnet build src/WftEngineering.slnx -c Release --no-restore
```

Inspect the configured source, privacy settings, and bounded flush/disposal in source. With Foundry and the local dashboard available, follow TraceId and ParentSpanId through approval and continuation in the authenticated viewer. Record unavailable live checks as pending.

## F3-T2 — Present facts, tasks, and iterations clearly

- [ ] Implemented and verified.

**Description:** prepare the console for the audience with streaming and simple blocks. Keep harness control visible and prevent model text from appearing to be evidence.

**Dependencies:** F3-T1.
**Scope:** M, up to 3 edited files.

**Planned files:**

- `src/WftEngineering.Demo/DemoApplication.cs`
- `src/WftEngineering.Demo/Tools/DeploymentTools.cs`, if fact notifications are missing

**Acceptance:**

- [ ] Agent and Harness are distinct; the loaded Skill, verified change/window/health, and five todos come from actual events/results and the session.
- [ ] Approval shows exact bound arguments, accepts only an explicit decision, and preserves safe behavior on rejection/EOF/invalid input.
- [ ] Iteration and observation are separate; the summary uses external state in happy/reject/stuck, shows TraceId, and does not announce success or external cancellation without evidence.

**Verification:**

```bash
dotnet build src/WftEngineering.slnx -c Release --no-restore
```

Manually inspect happy, reject, and stuck output. Check labels, exact approval arguments, provider todos, independent counters, and operational summaries against the store. Do not compare or freeze generated model text.

### Checkpoint after T2

- [ ] Happy, reject, and stuck are readable and their outcomes match the store.
- [ ] Counters do not derive from model phrase/ResponseId changes.
- [ ] Existing authorization guards and loop limits remain intact.

## F3-T3 — Verify privacy and local visualization

- [ ] Implemented and verified.

**Description:** demonstrate that export contains sufficient operational data without capturing content and prepare the local Aspire viewer check.

**Dependencies:** F3-T2.
**Scope:** M, up to 3 edited files.

**Planned files:**

- `src/WftEngineering.Demo/Program.cs`, privacy/lifecycle adjustments only
- `docs/specs/features/03-observable-demo-session/telemetry-verification.md`, brief evidence during implementation

**Acceptance:**

- [ ] Exported tags/events/logs contain only allowed operational facts; sensitive capture remains disabled and privacy settings are reviewed in source.
- [ ] The trace contains approval/status/health/closure facts in happy and negative branches; no duplicate client instrumentation or raw exceptions/bodies.
- [ ] Local command and dashboard authentication are verified when the environment exists; the record distinguishes compilation/source review from pending live viewer checks.

**Verification:**

```bash
dotnet build src/WftEngineering.slnx -c Release --no-restore
dotnet format src/WftEngineering.slnx --verify-no-changes --no-restore
```

Use the dashboard commands in plan.md and the spec with actual human approval. Inspect exported tags/events for operational facts and absence of prompts, responses, complete arguments/results, and raw exceptions. Record actual span names, privacy checks, and limitations in telemetry-verification.md without saving conversation content.

## Feature completion

- [ ] T1–T3 verified and F1/F2 guards and limits preserved.
- [ ] Composition remains small; no TUI, collector, AppHost, or capabilities have been added.
- [ ] Telemetry evidence and this checklist separate compilation/source review from pending local/live checks.

## Execution evidence

Pending. Record commands, outcomes, trace coverage, privacy, and actual viewer status. Keep fictional IDs and avoid copying prompts/responses into evidence.
