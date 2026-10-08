# Tasks: 04-conference-scenarios

Initial status: pending; requires F3 complete and F1/F2 implementation complete.

## F4-T1 — Run success and stopping scenarios with one command

- [ ] Implemented and verified.

**Description:** complete a minimal CLI selector using the existing store and fixtures. Keep harness controls identical across all scenarios.

**Dependencies:** F3 complete.
**Scope:** M, 3 edited files.

**Planned files:**

- `src/WftEngineering.Demo/Program.cs`
- `src/WftEngineering.Demo/Demo/DemoDeploymentStore.cs`

**Acceptance:**

- [ ] happy, stuck, validation-failed, deployment-failed, and post-health-failed use only fake variants and end with the outcomes in plan.md.
- [ ] Unknown arguments fail before invoking the model; each run resets session/store and RequestedChange remains under host control.
- [ ] Reject still comes from the human response; no scenario automatically approves, changes limits, or enables scripts/shell/other capabilities.

**Verification:**

```bash
dotnet build src/WftEngineering.slnx -c Release --no-restore
```

The invalid scenario must exit before model invocation. Review fresh state/session creation and shared controls across fixtures, then manually exercise the available scenarios with actual human decisions.

## F4-T2 — Prepare commands and the conference walkthrough

- [ ] Implemented and verified.

**Description:** write the operational/teaching runbook and freeze verified versions to avoid live environment changes.

**Dependencies:** F4-T1.
**Scope:** M, up to 3 edited files.

**Planned files:**

- `docs/demo-runbook.md`
- `README.md`, link to the demo/runbook while preserving other materials
- `global.json`, only to adjust the exact validated SDK

**Acceptance:**

- [ ] Runbook includes complete commands from the root, variables without secrets, login, authenticated dashboard, and actual paths/scenarios; README links to it.
- [ ] SDK, packages, and locks are pinned and the local image is verified/pinned before the talk; the walkthrough uses --no-build and does not download/provision live.
- [ ] A 12-minute script shows the seven code elements and five Engineering concepts; it offers rejection/stuck and a recorded backup labeled as such, without creating slides/video or changing the entire talk.

**Verification:**

```bash
dotnet build src/WftEngineering.slnx -c Release --no-restore
```

Review runbook commands, local links, and scenario names against code. Check the image/dashboard if Docker is available. Do not change versions automatically to latest; document any compatibility changes before freezing the environment again.

### Checkpoint after T2

- [ ] Runbook commands match the actual project and contain no secrets or external business dependencies.
- [ ] Scenarios and presentation fit the walkthrough without showing too many APIs.
- [ ] The demo remains concentrated in a single console project and Skill under `src/`.

## F4-T3 — Verify the matrix and rehearse the session

- [ ] Implemented and verified.

**Description:** review the complete scenario matrix and record manual rehearsal. Close relevant behavior gaps from §9 without adding testing infrastructure.

**Dependencies:** F4-T2.
**Scope:** M, up to 4 edited files; code changes only to fix discovered failures.

**Planned files:**

- `src/WftEngineering.Demo/Program.cs`, if integrated behavior fails
- `src/WftEngineering.Demo/Demo/DemoDeploymentStore.cs`, if an invariant fails
- `src/WftEngineering.Demo/Tools/DeploymentTools.cs`, if a guard fails
- `docs/specs/features/04-conference-scenarios/rehearsal.md`, evidence created during implementation

**Acceptance:**

- [ ] Source review and manual scenario checks cover negative validation, altered/unbound binding, changed permission/precondition, repeats/idempotency, limits, and subsequent failure; todos/text cannot replace evidence.
- [ ] A fake result with adversarial instructions does not change RequestedChange, permissions, limits, or available tools, or produce side effects without approval; available manual checks are recorded and privacy settings remain intact.
- [ ] Rehearsal records five happy runs with a live model, reject and stuck, trace/privacy, active duration, and the teaching walkthrough; missing inputs leave these checks pending without declaring the demo conference-ready.

**Verification:**

```bash
dotnet build src/WftEngineering.slnx -c Release --no-restore
dotnet format src/WftEngineering.slnx --verify-no-changes --no-restore
```

Use the runbook with human approval and a live model. Target happy under 90 seconds of active work. Record date/environment, SDK/packages/image/model, scenario, outcome, invocations, observations, side effects, active duration, and trace checks in rehearsal.md. Missing inputs remain pending. Fix discovered failures and repeat only affected checks.

## Implementation completion

- [ ] T1–T3 implementation, compilation, and available manual checks complete; pending live checks explicitly recorded.
- [ ] README/runbook and evidence updated, without expanding scope or adding architecture.

## Conference readiness

- [ ] Live-model/dashboard rehearsals and duration recorded; pending environment checks resolved.
- [ ] Model/image/SDK frozen and resources prepared before the session.
- [ ] The presenter can locate the seven code elements and has the main walkthrough and a brief alternative.

## Execution evidence

Pending. Keep compiled/source-reviewed implementation separate from conference readiness; link `rehearsal.md` once created and record actual limitations.
