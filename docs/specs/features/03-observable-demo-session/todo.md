# Tasks: 03-observable-demo-session

Initial status: pending; requires F2 complete and F1 regression passing.

## F3-T1 — Export a trace covering the complete session

- [ ] Implemented and verified.

**Description:** connect source/exporter and complete correlation/lifecycle of the trace instrumented during earlier features.

**Dependencies:** F2 complete.
**Scope:** M, up to 5 edited files.

**Planned files:**

- `src/WftEngineering.Demo/Program.cs`
- `src/WftEngineering.Demo/WftEngineering.Demo.csproj`, only if the OTLP reference is still missing
- `src/WftEngineering.Demo/Tools/DeploymentTools.cs`, only for gaps in operational coverage
- `src/WftEngineering.Demo/Demo/DemoDeploymentStore.cs`, only for gaps in state events
- `src/WftEngineering.Demo.Tests/ProductionChangeTests.cs`
- Generated if the reference changes: lock files for affected projects

**Acceptance:**

- [ ] TracerProvider listens to WftEngineering.Demo, exports to configurable local OTLP, and preserves harness instrumentation without another wrapper around the same client.
- [ ] demo.run correlates validation, approval, and subsequent runs; spans/events contain operations and state, not a transcript. The pause includes decision and duration.
- [ ] Closure, cancellation, and failure flush/dispose within a bounded budget and show TraceId; an exporter error is not confused with deployment failure.

**Verification:**

```bash
dotnet test src/WftEngineering.Demo.Tests/WftEngineering.Demo.Tests.csproj -c Release --filter 'FullyQualifiedName~Telemetry'
dotnet build src/WftEngineering.Demo/WftEngineering.Demo.csproj -c Release
```

Use names containing `Telemetry`. Collect activities with an ActivityListener/test exporter and verify TraceId/ParentSpanId, round-trip coverage, and closure in happy/reject/stuck. Do not add a different production architecture to enable the test collector.

## F3-T2 — Present facts, tasks, and iterations clearly

- [ ] Implemented and verified.

**Description:** prepare the console for the audience with streaming and simple blocks. Keep harness control visible and prevent model text from appearing to be evidence.

**Dependencies:** F3-T1.
**Scope:** M, up to 3 edited files.

**Planned files:**

- `src/WftEngineering.Demo/Program.cs`
- `src/WftEngineering.Demo/Tools/DeploymentTools.cs`, if fact notifications are missing
- `src/WftEngineering.Demo.Tests/ProductionChangeTests.cs`

**Acceptance:**

- [ ] Agent and Harness are distinct; the loaded Skill, verified change/window/health, and five todos come from actual events/results and the session.
- [ ] Approval shows exact bound arguments, accepts only an explicit decision, and preserves safe behavior on rejection/EOF/invalid input.
- [ ] Iteration and observation are separate; the summary uses external state in happy/reject/stuck, shows TraceId, and does not announce success or external cancellation without evidence.

**Verification:**

```bash
dotnet test src/WftEngineering.Demo.Tests/WftEngineering.Demo.Tests.csproj -c Release --filter 'FullyQualifiedName~ConsoleOutput'
dotnet test src/WftEngineering.Demo.Tests/WftEngineering.Demo.Tests.csproj -c Release --filter 'FullyQualifiedName~Approval'
dotnet build src/WftEngineering.Demo/WftEngineering.Demo.csproj -c Release --no-restore
```

Use names containing `ConsoleOutput`. Check labels and critical facts with a scripted client and captured output; do not snapshot variable model text. Manually review readability and the seven teaching elements in §9 of the spec.

### Checkpoint after T2

- [ ] Happy, reject, and stuck are readable and their outcomes match the store.
- [ ] Counters do not derive from model phrase/ResponseId changes.
- [ ] All previous security and looping tests are preserved.

## F3-T3 — Verify privacy and local visualization

- [ ] Implemented and verified.

**Description:** demonstrate that export contains sufficient operational data without capturing content and prepare the local Aspire smoke test.

**Dependencies:** F3-T2.
**Scope:** M, up to 3 edited files.

**Planned files:**

- `src/WftEngineering.Demo/Program.cs`, privacy/lifecycle adjustments only
- `src/WftEngineering.Demo.Tests/ProductionChangeTests.cs`
- `docs/specs/features/03-observable-demo-session/telemetry-verification.md`, brief evidence during implementation

**Acceptance:**

- [ ] A canary included in the scripted client's prompt, response, and payloads does not appear in exported tags/events/logs; both layers keep sensitive capture disabled.
- [ ] The trace contains approval/status/health/closure facts in happy and negative branches; no duplicate client instrumentation or raw exceptions/bodies.
- [ ] Local command and dashboard authentication are verified when the environment exists; the record distinguishes offline tests from a pending live smoke test.

**Verification:**

```bash
dotnet test src/WftEngineering.Demo.Tests/WftEngineering.Demo.Tests.csproj -c Release --filter 'FullyQualifiedName~Telemetry'
dotnet test src/WftEngineering.Demo.Tests/WftEngineering.Demo.Tests.csproj -c Release
dotnet format src/WftEngineering.Demo/WftEngineering.Demo.csproj --verify-no-changes --no-restore
```

Dashboard smoke test: use commands from `plan.md` and configuration from §8 of the spec. With Foundry configured, run happy with manual acceptance and find the TraceId. Record observed span names/structure, privacy checks, and limitations in `telemetry-verification.md`; do not save conversation content.

## Feature completion

- [ ] T1–T3 verified and F1/F2 regression passing.
- [ ] Composition remains small; no TUI, collector, AppHost, or capabilities have been added.
- [ ] Telemetry evidence and this checklist separate offline automation from pending local/live smoke tests.

## Execution evidence

Pending. Record commands, outcomes, trace coverage, privacy, and actual viewer status. Keep fictional IDs and avoid copying prompts/responses into evidence.
