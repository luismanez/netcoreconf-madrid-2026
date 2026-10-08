# Tasks: 02-bounded-deployment-verification

Initial status: pending; requires F1 implemented and verified.

## F2-T1 — Observe deployment and preserve evidence

- [ ] Implemented and verified.

**Description:** add the fourth tool and the fake's minimal state machine. Separate snapshots, observation count, effective version, and subsequent health.

**Dependencies:** F1 complete.
**Scope:** M, 3 edited files.

**Planned files:**

- `src/WftEngineering.Demo/Demo/DemoDeploymentStore.cs`
- `src/WftEngineering.Demo/Tools/DeploymentTools.cs`
- `src/WftEngineering.Demo/skills/production-change/SKILL.md`

**Acceptance:**

- [ ] Valid status produces Running/Running/Succeeded with an observation counter; repeats within an iteration do not advance the fake, and unknown IDs fail closed.
- [ ] The effective version changes on Succeeded; Completed requires subsequent health/version, not the prior read.
- [ ] Each run has fresh state and fixtures include Failed, subsequent Unhealthy/wrong version, and permanently Running; tools are instrumented with operational data only.

**Verification:**

```bash
dotnet build src/WftEngineering.slnx -c Release --no-restore
```

Review snapshot caching, unknown-ID guards, effective-version changes, and subsequent health evidence. Use deterministic fixtures; no real clock, sleeps, or randomness. Observe counters during a configured live happy run; record unavailable live checks as pending.

## F2-T2 — Drive the flow with LoopAgent and external state

- [ ] Implemented and verified.

**Description:** enable the native loop around the approval pipeline and follow the path through Completed while preserving todos/session state.

**Dependencies:** F2-T1.
**Scope:** M, 3 edited files.

**Planned files:**

- `src/WftEngineering.Demo/Program.cs`
- `src/WftEngineering.Demo/Demo/DemoDeploymentStore.cs`
- `src/WftEngineering.Demo/skills/production-change/SKILL.md`

**Acceptance:**

- [ ] LoopEvaluators contains a single DelegateLoopEvaluator; Continue/Stop are determined by external evidence and concrete feedback. No autonomous while loop reruns the LLM.
- [ ] After acceptance, Running/Running/Succeeded status occurs in distinct invocations; only subsequent Healthy health and the expected version allow Completed.
- [ ] Pending approval exits the loop; rejection/binding/exact arguments remain protected. A success claim or prematurely completed todos does not complete the operation.

**Verification:**

```bash
dotnet build src/WftEngineering.slnx -c Release --no-restore
```

With Foundry configured, manually observe distinct native invocations and status observations, approval pause/resume, and subsequent health. Review the evaluator and cached snapshots in source. Keep invocation, model-call, and observation counters distinct.

### Checkpoint after T2

- [ ] Complete happy path implemented through the native harness; live behavior checked when configured.
- [ ] F1 native approval and store guards remain intact.
- [ ] The fourth tool, evaluator, counters, and MaxIterations are easy to locate in code without new layers.

## F2-T3 — Stop autonomy and classify the pending outcome

- [ ] Implemented and verified.

**Description:** complete all branches and budgets; the console distinguishes loop limits, interruption, and failed or still-unknown business outcomes.

**Dependencies:** F2-T2.
**Scope:** M, 3 edited files.

**Planned files:**

- `src/WftEngineering.Demo/Program.cs`
- `src/WftEngineering.Demo/Demo/DemoDeploymentStore.cs`
- `src/WftEngineering.Demo/Tools/DeploymentTools.cs`

**Acceptance:**

- [ ] --scenario stuck ends after at most four invocations following approval, with ExecutionLimitReached and Running; no fifth invocation, redeployment, or loop restart. Success with evidence in the fourth remains Completed.
- [ ] Failed, invalid subsequent health/version, rejection, and cancellation produce terminal outcomes without new actions or rollback; state takes precedence over messages/todos.
- [ ] Internal limit 12, active deadline 120 s, Ctrl+C, and bounded transport retry are implemented; human waiting does not exhaust active time and the side effect is not automatically retried.

**Verification:**

```bash
dotnet build src/WftEngineering.slnx -c Release --no-restore
dotnet format src/WftEngineering.slnx --verify-no-changes --no-restore
```

Manually run happy and stuck with actual human approval, and cancel a run with Ctrl+C. Review terminal branches, the fourth-invocation boundary, active-time accounting, and retry limits in source. Do not automatically approve or extend limits to obtain a successful rehearsal.

## Feature completion

- [ ] T1–T3 and F1 regression verified.
- [ ] All §6 branches are implemented and source-reviewed; available manual §9 checks are recorded against actual store facts.
- [ ] Output never claims that stopping the harness canceled an external deployment.

## Execution evidence

Pending. Record invocation, observation, and side-effect counts, commands/results, and any live rehearsal that has not yet run.
