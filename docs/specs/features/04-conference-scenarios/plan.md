# Feature 04: Reproducible scenarios and conference script

**ID:** `04-conference-scenarios`
**Depends on:** [03-observable-demo-session](../03-observable-demo-session/plan.md).
**Source:** [global spec](../../production-change-demo.md), §§5–12.
**Work:** [todo.md](todo.md).
**Status:** implementation complete and compiled. Presenter-owned E2E/rehearsal remains pending.

**Current verification scope:** the combined implementation request requires code and compilation only. Any manual/E2E requirements below are presenter follow-up, not agent execution requirements. No automated testing is permitted.

## Feature outcome

The presenter can launch prepared scenarios with one command, show the happy path and a stopping alternative, open the trace, and follow a 12-minute script. SDK, dependencies, and image are frozen, and evidence distinguishes compilation/source review, live rehearsal, and pending checks.

This feature prepares public execution; it does not turn the demo into a real deployment system or add another architecture.

## Scope

- Complete scenario selection using existing deterministic store fixtures, without introducing business configuration editable by the LLM.
- Keep `happy` and `stuck`; expose three small alternatives: `validation-failed` (closed window), `deployment-failed`, and `post-health-failed`.
- Demonstrate rejection by answering Reject in `happy`, not through a flag that bypasses the protocol. Review other negative matrix guards in source; they do not need a flag each.
- Add a runbook with prerequisites, restore/build/format, Foundry configuration, authenticated viewer, commands, expected output, and recovery from environment errors.
- Map what to show on slides and the seven code elements, with a 12-minute script and a brief rejection/stuck alternative.
- Verify the complete acceptance matrix, including adversarial tool input, and perform live rehearsal if environment inputs are available.
- Record actual versions/image and pending checks; prepare instructions for a recorded backup clearly labeled as such. Do not generate a video or complete slides as part of this feature.

Out of scope: a general natural-language parser, general multi-turn chat, random fixtures, scenarios for every validation, an automatic-approval CLI, cloud infrastructure, new models/providers, CI/CD, material publication, or real deployment.

## Scenario contract

| CLI / action | Difference from happy | Expected outcome |
|---|---|---|
| `--scenario happy` + Approve | None | Completed with subsequent health/version verified |
| `--scenario happy` + Reject | Negative human response | Rejected and zero deployments |
| `--scenario stuck` + Approve | Status always Running | ExecutionLimitReached; operation remains Running |
| `--scenario validation-failed` | Closed window | ValidationFailed; zero deployments and no valid approval question |
| `--scenario deployment-failed` + Approve | Final observation Failed | DeploymentFailed, no rollback |
| `--scenario post-health-failed` + Approve | Subsequent health Unhealthy | PostHealthFailed, no success message |

Flags only select fake data. They do not modify approvals, capability permissions, or harness limits. An unknown name fails before invoking the model and lists allowed values. Each launch creates a fresh store/session.

## Implementation sequence

1. **Reproducible paths:** minimal selector and the scenarios above, reusing existing data/rules.
2. **Runbook and freeze:** verifiable commands, pinned SDK/packages/image, and public walkthrough with backup. Checkpoint for execution from a prepared installation.
3. **Matrix and rehearsal:** complete regression, adversarial attempt, and live-model/dashboard evidence when available.

## Feature acceptance criteria

1. The five selectable scenarios and Reject produce the table's outcomes without bypassing the native protocol, allowing new capabilities, or reusing state between runs.
2. A runbook makes it possible to recreate the environment and follow a 12-minute demo; it identifies Prompt/Context/Loop/Graph/Harness and the seven code elements, without restoring/provisioning live.
3. The available manual §9 checks and source review are recorded; the rehearsal record confirms or explicitly leaves pending five live happy runs, reject, stuck, trace, and the target of under 90 seconds of active work.

Implementation can be compiled and source-reviewed with one prompt if credentials are missing; live behavior remains pending. Conference readiness is declared complete only when live rehearsal is also recorded; do not turn an environment requirement into fictitious success.

## Planned runbook

The final document will be `docs/demo-runbook.md`, linked from the repository README. It will contain:

- Prerequisites and actually verified versions, without secrets in examples.
- Restore with lock files, Release build, format, and image/dashboard preparation before the talk.
- Foundry endpoint/model and presenter login; prior connectivity/access check.
- Happy: request, prior facts, approval pause, acceptance, iterations, and subsequent health.
- Brief option: rejection or stuck, explaining that stopping the harness does not cancel an external deployment.
- Opening the trace, privacy checks, and returning to the slide showing the five Engineering concepts.
- What to do when credentials are missing, the viewer fails, or the model connection is lost; use a backup labeled as recorded.

A new slide deck is unnecessary. The mapping provides input for the talk's slides without changing existing content during this feature.

## Risks and checks

- Live-model variation does not justify silently raising limits, omitting approval, or replacing evidence with text.
- Review adversarial data handling: tools cannot change RequestedChange, permissions, limits, or configuration from model-provided arguments.
- Do not save sensitive transcripts in `rehearsal.md`: scenario, status, duration, counters, versions, and a fictional/local TraceId are sufficient.
- Rehearsal uses a live model; Docker is only the local viewer. Compilation does not prove model or dashboard availability.

## Single implementation prompt

```text
Apply using-agent-skills and implement feature 04-conference-scenarios.
Read docs/specs/production-change-demo.md, docs/specs/features/README.md, and plan.md/todo.md in docs/specs/features/04-conference-scenarios/; verify that features 01–03 are implemented with compilation and available manual verification recorded.
Complete the scenario selector, runbook, environment freeze, and acceptance matrix. Preserve native approvals, limits, privacy, and in-memory data exactly; do not add a general parser, new layers, or capabilities.
Compile and perform source review and brief manual checks. Perform the live-model and dashboard rehearsal when available; honestly record completed and pending checks, without automatically approving deployment in the public console or provisioning resources.
Update todo.md and create rehearsal.md with operational evidence. Do not publish materials or generate a complete deck/video; no sub-agents or automatic commits. Do not add automated tests, a testing project, scripted model clients, or testing dependencies. Write documentation in English.
```
