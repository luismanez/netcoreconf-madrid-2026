# Feature 04: compilation evidence and presenter rehearsal

**Date:** 8 October 2026.
**Implementation status:** features 02–04 implemented in order and compiled. **Conference readiness:** pending presenter-owned E2E/rehearsal.

## Executed by the agent

| Checkpoint | Result |
|---|---|
| Feature 02 Release solution build | Passed; zero warnings/errors |
| OpenTelemetry exporter 1.19.1 restore and lock generation | Passed |
| Feature 03 Release solution build | Passed; zero warnings/errors |
| Feature 04 / final combined Release solution build | Passed; zero warnings/errors |

```bash
dotnet restore src/WftEngineering.slnx --use-lock-file
dotnet build src/WftEngineering.slnx -c Release --no-restore -m:1 -nodeReuse:false
```

The SDK is pinned to 10.0.202 in global.json; the app targets net10.0. Dependencies are pinned in the project/lock file. The code retains one console project, DI, DotNetEnv, Spectre.Console, native Skills/todos/approval/loop, and the in-memory deployment system.

Source inspection confirms the five scenario selectors, guarded targets/read evidence/permission/approval, independent post-health/version evidence, cached observations, terminal outcomes, and unchanged limits. It does not prove model behavior or trace delivery.

The requester explicitly assigned all Foundry/Aspire E2E work to the presenter. The agent did not run the console app, call Foundry, start Docker/Aspire, collect traces, execute automated tests, or measure rehearsal duration. No model name, image digest, live counters, or outcome has been invented.

## Presenter checklist — pending

- [ ] Confirm model/project access and record the exact deployment/model identity.
- [ ] Confirm the proposed Aspire image 13.6.0 is available, record its digest, and use its authenticated viewer.
- [ ] Complete five consecutive happy runs; record new observations across distinct invocations, subsequent Healthy v2.7.0, one deployment, and Completed.
- [ ] Run reject, EOF/unrecognized input, and Ctrl+C cases; confirm no pre-approval deployment.
- [ ] Run stuck; confirm at most four invocations after approval, Running, and ExecutionLimitReached without another deployment.
- [ ] Run validation-failed, deployment-failed, and post-health-failed; inspect expected stop branches.
- [ ] Inspect actual streamed approval binding/continuation, todo presentation, caching, and incomplete model behavior.
- [ ] Inspect actual trace coverage/correlation/privacy and missing-viewer/shutdown behavior.
- [ ] Measure active duration (target happy below 90 seconds, excluding human wait) and rehearse the twelve-minute script.
- [ ] Freeze local model/image/SDK setup and prepare a backup clearly labeled recorded.

## Record format

Add one row per actual run without copying prompts/responses or credentials:

| Date / environment | Model deployment | Scenario / decision | Outcome | Loop run / final iteration | Observations | Deployments | Active duration | Trace ID / privacy notes |
|---|---|---|---|---|---|---|---|---|

Image tag/digest and actual span names: pending. Live results: none recorded yet. Follow the [runbook](../../../demo-runbook.md); failures should be documented and resolved without bypassing approval or raising budgets.
