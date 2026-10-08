# Feature 01: Governed production change

**ID:** `01-governed-production-change`
**Depends on:** none.
**Source:** [global spec](../../production-change-demo.md), §§1–5, 7–9, and 12.
**Work:** [todo.md](todo.md).

## Feature outcome

Starting from the canonical request, the console loads `production-change`, queries change and health, shows the five todos, and requests native approval to start `DEP-742`. The outcome of this first feature is Running with verification pending. Rejection or a failed precondition produces zero deployments.

Startup and the business flow through approval belong together because they form a demonstrable capability. Do not create a separate feature for the csproj, model, Skill, or fake.

## Scope

- Create the .NET 10 project and xUnit project defined in the spec; pin dependencies and generate lock files. Verify the Harness/Skills/approval/loop APIs needed by subsequent features early.
- Configure Foundry through `IChatClient`, environment variables, and an explicit development identity. Missing or invalid configuration terminates before work begins.
- Compose `HarnessAgent` with brief general instructions, a local Skill source, `TodoProvider`, a session, the internal limit of 12, and unnecessary capabilities disabled. F2 enables the external loop.
- Implement `RequestedChange`, CHG-1042, prior health, the fixed clock, simulated permission, and evidence in the store.
- Expose `GetChangeRequest`, `GetServiceHealth`, and `DeployService`; add approval only to the last one. Do not expose `GetDeploymentStatus` yet.
- Preserve the framework approval protocol, check arguments before asking, and revalidate conditions/permissions during execution.
- Add minimal operational activities for the session, reads, approval, and deployment from the start; do not export sensitive content. Viewer configuration arrives in F3.

Outside this feature: autonomous polling, a Completed outcome, subsequent health as a final condition, failure-scenario CLI, OTLP exporter, and conference script. Do not replace pending work with an instant deployment.

## Contracts prepared by this feature

| Contract | Decision |
|---|---|
| Request / change | Keep IDs and the requested target outside state the model can modify; they match both arguments and the queried change |
| Prior reads | Obtain change and health through tools; missing evidence ⇒ deployment denied |
| Identity | Fixed permission in the store/host, never an LLM-controlled argument |
| Approval | At most one valid deployment question; same session and bound native response |
| Idempotency | Identical arguments return the existing DEP-742; different arguments do not create another operation |
| Todos | Five items; only validation, prior health, and startup can be completed here; verification remains pending |
| Outcome | Pending while deployment is Running; ValidationFailed/Rejected/Cancelled/Error as appropriate |
| Observability | Source `WftEngineering.Demo`, operational facts, and no prompts/full arguments/results |

The host communicates facts from the store even if the model claims deployment is complete. At this stage the Skill includes validation/approval and the five items; F2 completes polling and subsequent verification instructions.

## Implementation sequence

1. **Startup and compatibility:** projects, dependencies, client, and configuration; check framework signatures before developing the flow. Temporary experiments stay outside demo code.
2. **Validation with external context:** store, read tools, Skill, todos, and offline tests. Checkpoint for configuration and absence of side effects.
3. **Deployment with approval:** native wrapper, minimal UX, rejection, binding, simulated authorization, and idempotency. Verify the complete flow through Running.

See `todo.md` for tasks, acceptance, files, and commands. The project remains runnable after each step; the console explains the scope still awaiting verification.

## Feature acceptance criteria

1. A valid request produces a loaded Skill, change and health reads, and a question showing the actual change, service, version, and environment; there are zero deployments before acceptance.
2. Acceptance through the native protocol starts a single DEP-742 in Running; rejection, inconsistent data, missing evidence, and denied permission prevent side effects.
3. Build and offline tests pass; todos and operational state are separate, and the console does not declare Completed. Document verified compatibility and any pending live checks.

## Available manual verification

```bash
dotnet run --project src/WftEngineering.Demo/WftEngineering.Demo.csproj -c Release --no-build -- --scenario happy
```

With Foundry configured, observe the pause, accept or reject from the keyboard, and compare the outcome with the store. A live run requires presenter configuration; scripted tests are mandatory even without credentials.

## Risks and implementation boundaries

- Validate versions and signatures in the pinned package, not just examples from `main`. If incompatible, update the spec decision with evidence; do not recreate framework primitives.
- Load Skills as read-only without a runner; reject unexpected approvals and do not use “always approve”.
- Keep test helpers in `ProductionChangeTests.cs` initially. If they grow, they may be split within `tests/`; do not create production factories or interfaces to accommodate them.
- Record evidence in this folder. Do not modify the general README or slides unless a link is essential; F4 completes their walkthrough.

## Single implementation prompt

```text
Apply using-agent-skills and implement feature 01-governed-production-change.
Read docs/specs/production-change-demo.md, docs/specs/features/README.md, and the plan.md and todo.md files in docs/specs/features/01-governed-production-change/.
Complete its three tasks with implementation and tests. Keep harness composition visible and use native approvals; this feature's outcome is a started deployment awaiting verification.
Work only within its scope, without sub-agents, new layers, provisioning, or automatic commits. Check the actual APIs in the pinned package before relying on them.
Update todo.md with evidence and actual pending items. If credentials are missing, complete offline checks and record the Foundry run as pending. Do not implement feature 02. Write documentation in English.
```
