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

The historical builds above used SDK 10.0.202. Since 19 October 2026, global.json pins SDK 10.0.402; the app still targets net10.0. Dependencies are pinned in the project/lock file. The code retains one console project, DI, DotNetEnv, Spectre.Console, native Skills/todos/approval/loop, and the in-memory deployment system.

Source inspection confirms the five scenario selectors, guarded targets/read evidence/permission/approval, independent post-health/version evidence, cached observations, terminal outcomes, and unchanged limits. It does not prove model behavior or trace delivery.

The requester explicitly assigned all Foundry/Aspire E2E work to the presenter. The agent did not run the console app, call Foundry, start Docker/Aspire, collect traces, execute automated tests, or measure rehearsal duration. No model name, image digest, live counters, or outcome has been invented.

## SDK update — 19 October 2026

`global.json` now requests SDK `10.0.402`, retaining `rollForward: latestPatch` and `allowPrerelease: false`. Current documentation follows the new pin; the F1 compatibility record remains historical. The project still targets `net10.0`, with unchanged NuGet versions.

The existing active SDK was a Microsoft macOS Arm64 package installation under `/usr/local/share/dotnet`; another installation exists under `~/.dotnet`. The official `10.0.402` Arm64 installer was downloaded to `/private/tmp/dotnet-sdk-10.0.402-osx-arm64.pkg`. Its SHA512 matches Microsoft's release metadata, its Microsoft signature/notarization is trusted, and Gatekeeper accepts it.

The presenter completed the macOS administrator installation. Both `.zprofile` and `.zshrc` now select `/usr/local/share/dotnet` through `DOTNET_ROOT` and `PATH`; their prior workshop blocks were backed up as `.zprofile.before-dotnet-10.0.402.bak` and `.zshrc.before-dotnet-10.0.402.bak`. The separate `~/.dotnet` installation and global tools were preserved.

Verified: a fresh login/interactive shell resolves `/usr/local/share/dotnet/dotnet`, reports the matching `DOTNET_ROOT`, and selects SDK `10.0.402`. The system package receipt confirms `10.0.402`; `dotnet --info` reports SDK `10.0.402` and host/runtime `10.0.13`. Shell syntax validation and `git diff --check` pass.

`dotnet restore src/WftEngineering.slnx --locked-mode` succeeded. `dotnet build src/WftEngineering.slnx -c Release --no-restore --nologo` then succeeded with zero warnings/errors. New-SDK first-use initialization required access outside the workspace; verification disabled ASP.NET development-certificate generation for these console-only commands. The project file and package lock file are unchanged. No tests or Foundry runs were performed; scenario rehearsal remains with the presenter.

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
