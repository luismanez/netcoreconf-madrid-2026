# Tasks: 04-conference-scenarios

**Status:** implementation and documentation complete; compilation passed. Conference rehearsal/E2E is owned by the presenter and remains pending.

## F4-T1 — Select prepared scenarios

- [x] happy, stuck, validation-failed, deployment-failed, and post-health-failed select deterministic store fixtures.
- [x] Unknown arguments are rejected before configuration/model creation; help works before configuration, and default startup selects happy.
- [x] Every launch creates fresh state/session. All scenarios share approval, authorization, privacy, and loop limits.
- [x] Rejection remains an actual human response; no approval bypass or extra executable capability exists.

## F4-T2 — Prepare the conference walkthrough

- [x] docs/demo-runbook.md created and linked from README with root commands, empty-template configuration, Azure CLI login, and authenticated standalone viewer instructions.
- [x] Five scenario outcomes, exit codes, counters, deadline/retry behavior, and recovery documented as expected behavior.
- [x] Twelve-minute script and seven code-element map cover Prompt/Context/Loop/Graph/Harness.
- [x] SDK/packages pinned and lock file generated; proposed image tag recorded. Live walkthrough uses --no-build.
- [x] Backup recording instructions provided; no deck, video, publication, provisioning, or additional architecture created.

## F4-T3 — Record implementation and pending rehearsal

- [x] Source review retains exact target/read/permission/approval guards, idempotency, terminal outcomes, post-health/version evidence, and loop limits.
- [x] Model arguments cannot mutate request, permissions, limits, or available capabilities; source instructions distinguish external data from instructions.
- [x] rehearsal.md created with compilation evidence, presenter checklist, and blank operational record format; no fabricated E2E results.

## Compilation evidence

```bash
dotnet build src/WftEngineering.slnx -c Release --no-restore -m:1 -nodeReuse:false
```

F4 checkpoint and final combined implementation: passed with zero warnings and zero errors. Verification for this request is code inspection and compilation only; no tests or application/E2E execution were performed.

## Pending — presenter-owned conference readiness

- [ ] Model/project access, actual model behavior, and all scenario/approval/cancellation paths checked with Foundry.
- [ ] Aspire image availability/digest, authenticated access, actual trace correlation/coverage/privacy, and exporter behavior checked.
- [ ] Five happy runs, reject, stuck, active duration, and teaching walkthrough recorded in rehearsal.md.
- [ ] Final model/image identity frozen locally and a clearly labeled recorded backup prepared before the talk.

See [rehearsal.md](rehearsal.md) and the [runbook](../../../demo-runbook.md). These pending checks are deliberately outside the agent's implementation request.
