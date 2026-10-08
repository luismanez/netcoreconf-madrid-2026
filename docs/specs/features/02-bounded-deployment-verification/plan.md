# Feature 02: Deployment verification with bounded autonomy

**ID:** `02-bounded-deployment-verification`
**Depends on:** [01-governed-production-change](../01-governed-production-change/plan.md).
**Source:** [global spec](../../production-change-demo.md), §§4–6 and 9.
**Work:** [todo.md](todo.md).

## Feature outcome

Deployment started after approval progresses through Running/Running/Succeeded observations in distinct agent invocations. The harness continues only while evidence is missing and reports Completed only after checking Healthy health and version 2.7.0 after Succeeded. A deployment that stays Running stops driving work after four invocations.

This feature delivers Loop Engineering and Graph Engineering branches without introducing the workflow engine.

## Scope

- Add `GetDeploymentStatus` to the existing tools and complete the Skill procedure.
- Implement deployment state, polling snapshots, and separate invocation/observation identifiers in the store.
- Enable the native loop through `HarnessAgentOptions.LoopEvaluators`, a `DelegateLoopEvaluator`, and `LoopAgentOptions`.
- Preserve the session, approval protocols, and todos; the model marks tasks and the host determines the outcome from facts.
- Cover success, failed deployment, invalid subsequent health/version, rejection, cancellation, and work still pending at the limit.
- Enable `--scenario stuck` alongside `happy`. Other failures are covered by test fixtures; F4 exposes them as presentation scenarios.
- Add status, continuation, and outcome activities/events alongside flow implementation, using the established source.

Out of scope: an LLM judge, textual completion markers, another planner, multiple evaluators, workflows, rollback, automatic approvals, automatic budget restart, and exporter/viewer.

## Polling contract

The first valid `GetDeploymentStatus` query in an iteration consumes a new observation. Repeating it within the same invocation returns the same snapshot with `IsCached`; it cannot consume three transitions in a single run. Before returning Continue, the evaluator enables the next observation. It does not increment the sequence if the model omitted the status query.

When entering a run, the host starts its visible counter at 1; the evaluator prepares the next one with `LoopContext.Iteration + 1`. Preserve the deployment observation counter and evidence across approval. Reaching Succeeded changes the fake's effective version; a prior Healthy read does not become a subsequent read.

## Stop conditions and budgets

| Situation | Behavior |
|---|---|
| Failed validation or prior health | Stop; do not start any action |
| Pending approval | Return to the host through native loop behavior |
| Operator rejection | Native negative response; terminal state prevents new actions |
| Validation still incomplete / deployment Running | Continue with concrete feedback, within budget |
| Deployment Succeeded without a subsequent read | Continue requesting health/version verification |
| Succeeded + subsequent Healthy + expected version | Completed and Stop, regardless of text/todos |
| Deployment Failed or invalid subsequent health/version | Failure outcome and Stop; no rollback |
| Fourth invocation ends with evidence pending | ExecutionLimitReached; no fifth invocation or automatic reentry |

`MaxIterations = 4` includes the first invocation of each LoopAgent run. Reentry after approval is a new run with its own budget; do not describe it as a global session limit. The host allows one valid deployment question and one round trip.

Preserve `MaximumIterationsPerRequest = 12`, a 120-second active-work deadline, and Ctrl+C cancellation. To resolve the ambiguity of “bounded retries”, propose at most **one model transport retry**, within the deadline; a tool with a side effect is not automatically retried. Human waiting does not consume active work. Do not use delays to advance the fake.

The limit check happens before the evaluator; record the final invocation and classify the outcome from the store. If the fourth invocation already contains complete evidence, the outcome remains Completed. Exhausting internal work without consuming four invocations must not be falsely labeled as exhausting the external loop.

## Implementation sequence

1. **Verifiable state:** fourth tool, deterministic transitions, snapshot idempotency, and subsequent health evidence.
2. **Framework loop:** composition, feedback, and approval/session continuity; happy path through Completed. Checkpoint for security and actual invocation counts.
3. **Stopping and limits:** stuck, failure branches, deadline/cancellation/retry, and messages about pending external outcomes.

## Feature acceptance criteria

1. Approved happy completes one deployment with three new observations in distinct invocations and a subsequent Healthy read on version 2.7.0.
2. Stuck consumes at most four invocations after approval and stops with deployment still Running; repeated status, success claims, or completed todos do not bypass controls.
3. Failures, rejection, and cancellation stop safely; there is no approval/binding/idempotency regression or success based solely on text or declared progress.

## Risks to test explicitly

- The framework may return a transcript at the limit without an exception; do not rely on an assumed exception.
- The model may request many tools within a run; test both cached snapshots and the internal limit.
- The agent may omit status or subsequent health; feedback and the limit must preserve an incomplete outcome.
- The Skill and todos may encourage continuation after failure; the single evaluator and terminal guards take precedence.
- Do not turn scripted-client behavior tests into a promise about live-model text or latency.

## Single implementation prompt

```text
Apply using-agent-skills and implement feature 02-bounded-deployment-verification.
Read docs/specs/production-change-demo.md, docs/specs/features/README.md, and plan.md/todo.md in docs/specs/features/02-bounded-deployment-verification/; check that feature 01 is implemented and its tests pass.
Complete the three tasks using the harness LoopAgent and one evaluator over external state. Keep MaxIterations=4, one new snapshot per iteration, and native approvals.
Test happy, stuck, subsequent verification, and failure branches with the real harness and a scripted IChatClient. Preserve guards and the separation between todos, text, and evidence.
Update todo.md with results. Do not implement OTLP/viewer, the conference script, or subsequent features; no sub-agents, new layers, provisioning, or automatic commits. Write documentation in English.
```
