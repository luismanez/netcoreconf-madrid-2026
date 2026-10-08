# Tasks: 01-governed-production-change

**Historical F1 checkpoint:** F1 ended at Running. The current application includes F2–F4 and verifies happy through Completed; use the [current runbook](../../../demo-runbook.md). E2E/rehearsal is presenter-owned and remains pending.

**Status:** implementation complete; compilation and startup checks passed. Live Foundry approval/rejection rehearsal is pending because no `.env` is configured.

Verification follows the presenter's decision: no test project, automated tests, scripted model client, or testing dependencies. Use compilation, source review, and brief manual checks.

## F1-T1 — Start the console and verify compatibility

- [x] Pinned agent dependencies restored and the .NET 10 solution compiled.
- [x] Foundry `IChatClient`, Azure CLI identity, settings, concrete services, and Harness registered through DI in `Program.cs`.
- [x] Native Skills, todos, history, approval binding, and telemetry APIs checked against the restored packages and compiled.
- [x] Only the console project remains in the solution; obsolete testing references removed.
- [x] Help, unsupported scenario, and missing configuration checked manually with expected exit codes 0, 2, and 1.

Files: `Program.cs`, project/lock files, `src/WftEngineering.slnx`.

## F1-T2 — Validate through a Skill, tools, and external state

- [x] Deterministic CHG-1042, fixed UTC time, Healthy v2.6.3, and independent host-controlled permission implemented in `DemoDeploymentStore.cs`.
- [x] `GetChangeRequest` and `GetServiceHealth` record reads; invalid targets/conditions stop the run and cannot erase a failure.
- [x] Skill copied to the output directory and resolved from `AppContext.BaseDirectory` without a script runner.
- [x] Five todos defined in the Skill; the console reads native session todos and labels them as declared progress.
- [x] Tool-read facts and actual native Skill load are presented separately from agent text.
- [ ] Observe actual Skill loading, five todos, and both tool reads with the configured live model.

Files: `Demo/DemoDeploymentStore.cs`, `Tools/DeploymentTools.cs`, `skills/production-change/SKILL.md`, `DemoApplication.cs`.

## F1-T3 — Start deployment through native human approval

- [x] Only `DeployService` is wrapped in `ApprovalRequiredAIFunction`; only read-only Skill tools have an auto-approval rule.
- [x] Host accepts one approval request with exactly the four expected arguments and checks conditions before asking.
- [x] `APPROVE` is the only accepted input; other input/EOF rejects, and Ctrl+C cancels. Native `CreateResponse` resumes the same session once.
- [x] Tool revalidates request/change/window/health/permission/read evidence and host approval. Identical repeated calls reuse DEP-742.
- [x] Operational summary uses store facts: Running, verification pending, zero or one deployment; no Completed outcome exists in F1.
- [x] Internal tool-calling limit 12, active execution budget 120 seconds excluding keyboard wait, and one transport retry configured.
- [x] Minimal session/tool/approval activities added with sensitive capture disabled; no exporter or external loop added.
- [ ] Live acceptance starts one DEP-742 in Running and leaves both verification todos pending.
- [ ] Separate live rejection and EOF/cancellation checks leave zero deployments before approval.

## Evidence and remaining work

See [compatibility.md](compatibility.md) for exact dependencies, APIs, commands, results, and limitations. No Foundry inference or actual native approval exchange has been run in this environment. Features 02–04 remain unimplemented.
