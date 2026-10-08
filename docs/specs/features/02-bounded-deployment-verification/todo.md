# Tasks: 02-bounded-deployment-verification

**Status:** implemented and compiled. Live behavior is pending presenter-owned E2E checks.

The implementation request explicitly limits verification to compilation. No tests, scripted model substitute, application execution, Foundry calls, or Aspire runs were performed.

## F2-T1 — Observe deployment and preserve evidence

- [x] GetDeploymentStatus, typed snapshots, and deterministic Running/Running/Succeeded transitions implemented.
- [x] Repeated queries reuse the current snapshot with IsCached; only entering another native iteration enables a new observation. Missing queries do not advance status.
- [x] Unknown IDs stop the run; identical deployments remain idempotent and targets stay host-controlled.
- [x] Succeeded changes effective version; prior and subsequent health are separate records. Completed requires a subsequent Healthy read on the requested version.
- [x] Failed, post-health failure, and stuck fixtures prepared; their presentation selectors are exposed by F4.

## F2-T2 — Drive the flow with native LoopAgent

- [x] One DelegateLoopEvaluator composed through HarnessAgentOptions; feedback comes from external store facts.
- [x] MaxIterations 4, same session, and native approval pause/resume preserved. No host loop reruns the model.
- [x] Loop-run/iteration and observation counters are separate. State and observations survive the approval round trip; iteration budget resets on reentry.
- [x] Terminal outcomes stop continuation; todos and generated text cannot establish success or permission.

## F2-T3 — Stop autonomy and classify pending outcomes

- [x] Normal-return classification detects pending work at the fourth invocation without relying on a limit exception or final evaluator call.
- [x] Internal errors/cancellation remain distinct from normal external-loop exhaustion; Complete evidence in invocation four remains Completed.
- [x] Internal limit 12, shared 120-second active execution budget, Ctrl+C, one transport retry, and one human approval round trip retained.
- [x] Running output explicitly states that stopping the harness does not cancel or roll back deployment.

## Compilation evidence

```bash
dotnet build src/WftEngineering.slnx -c Release --no-restore -m:1 -nodeReuse:false
```

F2 checkpoint: passed with zero warnings and zero errors. The final combined F2–F4 build also passed. Native delegate/options signatures were checked against restored `Microsoft.Agents.AI`/Harness `1.23.0` package XML and compiled. [Official looping reference](https://learn.microsoft.com/en-us/agent-framework/agents/looping).

## Pending — presenter E2E

- [ ] Actual happy observations occur in distinct native invocations, followed by new health/version evidence and Completed.
- [ ] Stuck stops within four invocations after approval; repeated status is cached and no fifth invocation/redeployment occurs.
- [ ] Rejection/EOF/cancellation, failure branches, unchanged approval binding, and incomplete model behavior match the intended outcomes.

Use the [runbook](../../../demo-runbook.md). These unchecked items are not requirements for further implementation by the agent in this task.
