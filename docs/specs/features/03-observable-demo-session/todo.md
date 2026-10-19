# Tasks: 03-observable-demo-session

**Status:** implemented and compiled. Actual trace delivery, console behavior, and privacy inspection are pending presenter-owned E2E checks.

## F3-T1 — Export the session trace

- [x] OpenTelemetry exporter 1.19.1 added and locked; tracer subscribes to WftEngineering.Demo and uses the configured gRPC endpoint.
- [x] Native harness instrumentation reused, with dedicated root/tool/approval/evaluation activities and resumption/outcome tags.
- [x] Root spans the approval round trip; native agent/client content capture defaults to disabled and follows `DEMO_CAPTURE_CONTENT`. Detailed function errors remain disabled.
- [x] Fixed `demo.stop_reason` codes added to root/tool/evaluator spans when available; first terminal reason preserved, including `maintenance_window_closed`.
- [x] Export requests/processors bounded to 1 second; 2-second flush and shutdown attempted after root closure. Export errors preserve the operational outcome.
- [x] SafeErrorProcessor clears raw error status descriptions before the exporter; no separate client telemetry wrapper added.

## F3-T2 — Present the session

- [x] Native RunStreamingAsync updates rendered with Spectre.Console and Agent labels; native ToAgentResponse preserves approval content.
- [x] Harness iteration headers use host/evaluator counters. Tool facts come from actual typed results, including new/cached observations.
- [x] Skill load checked in native history; todos read through the same session provider and labeled declared progress.
- [x] Exact approval arguments and final store facts/health/status/counters/Trace ID displayed. No private reasoning rendered or second planner added.

## F3-T3 — Prepare privacy and visualization

- [x] Only trace export configured; optional native content capture supports the fictional demo. No transcript log exporter, HTTP body logging, TUI, AppHost, or collector added.
- [x] Standalone authenticated viewer instructions and source/privacy details documented in the runbook and telemetry-verification.md.
- [ ] Presenter confirms actual trace coverage/correlation, automatic span names, privacy, delivery, and flush behavior in Aspire.
- [ ] Presenter reviews readability and outcome consistency for happy, reject, stuck, and cancellation.
- [ ] Presenter checks native content with `DEMO_CAPTURE_CONTENT=true`, omission with false, and expected stop-reason codes in both modes.

## Compilation evidence

```bash
dotnet restore src/WftEngineering.slnx --use-lock-file
dotnet build src/WftEngineering.slnx -c Release --no-restore -m:1 -nodeReuse:false
```

Exporter restore and F3 build passed with zero warnings and zero errors. The final combined F2–F4 build also passed. Native streaming/merge, tracer, OTLP options, processor, flush, and shutdown APIs compile against pinned packages. See [telemetry-verification.md](telemetry-verification.md).

No application/E2E execution, live trace collection, Docker launch, or automated tests were performed, as requested.

18 October 2026 telemetry extension: `dotnet build src/WftEngineering.slnx -c Release --no-restore --nologo` and `git diff --check` passed; the build reported zero warnings/errors. Content delivery and the new stop-reason tags remain pending a presenter-run demo. `env.template` retains empty values; local `.env` enables content capture for the next run.
