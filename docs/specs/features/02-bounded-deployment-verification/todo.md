# Tasks: 02-bounded-deployment-verification

Initial status: pending; requires F1 implemented and verified.

## F2-T1 — Observe deployment and preserve evidence

- [ ] Implemented and verified.

**Description:** add the fourth tool and the fake's minimal state machine. Separate snapshots, observation count, effective version, and subsequent health.

**Dependencies:** F1 complete.
**Scope:** M, 4 edited files.

**Planned files:**

- `src/WftEngineering.Demo/Demo/DemoDeploymentStore.cs`
- `src/WftEngineering.Demo/Tools/DeploymentTools.cs`
- `src/WftEngineering.Demo/skills/production-change/SKILL.md`
- `src/WftEngineering.Demo.Tests/ProductionChangeTests.cs`

**Acceptance:**

- [ ] Valid status produces Running/Running/Succeeded with an observation counter; repeats within an iteration do not advance the fake, and unknown IDs fail closed.
- [ ] The effective version changes on Succeeded; Completed requires subsequent health/version, not the prior read.
- [ ] Each run has fresh state and fixtures include Failed, subsequent Unhealthy/wrong version, and permanently Running; tools are instrumented with operational data only.

**Verification:**

```bash
dotnet test src/WftEngineering.Demo.Tests/WftEngineering.Demo.Tests.csproj -c Release --filter 'FullyQualifiedName~DeploymentState'
dotnet build src/WftEngineering.Demo/WftEngineering.Demo.csproj -c Release --no-restore
```

Use names containing `DeploymentState`. Test repeated reads, reentry after approval, and subsequent health with independent fixtures. Do not use a real clock, sleeps, or Random.

## F2-T2 — Drive the flow with LoopAgent and external state

- [ ] Implemented and verified.

**Description:** enable the native loop around the approval pipeline and follow the path through Completed while preserving todos/session state.

**Dependencies:** F2-T1.
**Scope:** M, 4 edited files.

**Planned files:**

- `src/WftEngineering.Demo/Program.cs`
- `src/WftEngineering.Demo/Demo/DemoDeploymentStore.cs`
- `src/WftEngineering.Demo/skills/production-change/SKILL.md`
- `src/WftEngineering.Demo.Tests/ProductionChangeTests.cs`

**Acceptance:**

- [ ] LoopEvaluators contains a single DelegateLoopEvaluator; Continue/Stop are determined by external evidence and concrete feedback. No autonomous while loop reruns the LLM.
- [ ] After acceptance, Running/Running/Succeeded status occurs in distinct invocations; only subsequent Healthy health and the expected version allow Completed.
- [ ] Pending approval exits the loop; rejection/binding/exact arguments remain protected. A success claim or prematurely completed todos does not complete the operation.

**Verification:**

```bash
dotnet test src/WftEngineering.Demo.Tests/WftEngineering.Demo.Tests.csproj -c Release --filter 'FullyQualifiedName~Loop'
dotnet test src/WftEngineering.Demo.Tests/WftEngineering.Demo.Tests.csproj -c Release --filter 'FullyQualifiedName~Approval'
dotnet build src/WftEngineering.Demo/WftEngineering.Demo.csproj -c Release --no-restore
```

Use names containing `Loop`. The test client must count agent invocations through observed instrumentation/state, as well as model and status calls; do not confuse the counters. Test an invocation with several queries to demonstrate caching and another that omits status to check feedback.

### Checkpoint after T2

- [ ] Complete happy path verifiable offline through the real harness.
- [ ] F1 approval tests pass without weakening guards.
- [ ] The fourth tool, evaluator, counters, and MaxIterations are easy to locate in code without new layers.

## F2-T3 — Stop autonomy and classify the pending outcome

- [ ] Implemented and verified.

**Description:** complete all branches and budgets; the console distinguishes loop limits, interruption, and failed or still-unknown business outcomes.

**Dependencies:** F2-T2.
**Scope:** M, 4 edited files.

**Planned files:**

- `src/WftEngineering.Demo/Program.cs`
- `src/WftEngineering.Demo/Demo/DemoDeploymentStore.cs`
- `src/WftEngineering.Demo/Tools/DeploymentTools.cs`
- `src/WftEngineering.Demo.Tests/ProductionChangeTests.cs`

**Acceptance:**

- [ ] --scenario stuck ends after at most four invocations following approval, with ExecutionLimitReached and Running; no fifth invocation, redeployment, or loop restart. Success with evidence in the fourth remains Completed.
- [ ] Failed, invalid subsequent health/version, rejection, and cancellation produce terminal outcomes without new actions or rollback; state takes precedence over messages/todos.
- [ ] Internal limit 12, active deadline 120 s, Ctrl+C, and bounded transport retry are implemented; human waiting does not exhaust active time and the side effect is not automatically retried.

**Verification:**

```bash
dotnet test src/WftEngineering.Demo.Tests/WftEngineering.Demo.Tests.csproj -c Release --filter 'FullyQualifiedName~Loop'
dotnet test src/WftEngineering.Demo.Tests/WftEngineering.Demo.Tests.csproj -c Release
dotnet format src/WftEngineering.Demo/WftEngineering.Demo.csproj --verify-no-changes --no-restore
```

Include `LoopLimits` and `LoopTermination` in test names. For deadlines, use controlled cancellation in tests without waiting two minutes. Check that the evaluator does not need to execute after the fourth invocation to classify the outcome. Prepare the spec's stuck command for a later live rehearsal, without automatically approving it.

## Feature completion

- [ ] T1–T3 and F1 regression verified.
- [ ] All §6 branches and corresponding §9 cases have tests with evidence of the fake's actual state.
- [ ] Output never claims that stopping the harness canceled an external deployment.

## Execution evidence

Pending. Record invocation, observation, and side-effect counts, commands/results, and any live rehearsal that has not yet run.
