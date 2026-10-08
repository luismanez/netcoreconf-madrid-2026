# Feature 03: telemetry implementation evidence

**Date:** 8 October 2026.
**Verification boundary:** code inspection and compilation only. The presenter will perform all Foundry/Aspire E2E checks. No application run, Docker command, live trace capture, or automated test was executed.

## Compiled composition

`OpenTelemetry.Exporter.OpenTelemetryProtocol` is pinned to `1.19.1` and recorded in the application lock file. The tracer subscribes to `WftEngineering.Demo`, sets the service resource, and exports gRPC to the validated configured endpoint. It reuses the Harness `1.23.0` instrumentation; there is no second client wrapper.

The `demo.run` root spans the human round trip. Dedicated activities cover the four business tools, `approval.wait`, and `loop.evaluate`; resumption/decision events and final status/outcome/observation/iteration tags come from host/store facts. Model responses, todos, and raw argument/result payloads are not exported by application code.

Agent/client sensitive capture is explicitly disabled. Detailed function errors are disabled. A small `SafeErrorProcessor` clears error status descriptions before the OTLP processor, since framework instrumentation may put raw exception messages there independently of content capture. Exceptions shown in the console are sanitized. No metrics/log exporters or HTTP instrumentation were added.

The OTLP request and batch processor use 1000 ms exporter timeouts. After application/root closure, the host attempts ForceFlush(2000) and Shutdown(2000), then disposes. Cleanup warnings preserve the operational exit code/outcome. Completion of a flush request is not proof of delivery to the viewer.

Streaming uses native RunStreamingAsync and ToAgentResponse. Iteration headers come from BeginLoopRun/BeginIteration and the evaluator, not ResponseId changes. Tool notifications carry actual typed results to the existing presentation class; no event bus or alternate planner was introduced. Only text content is rendered, not private reasoning.

## Evidence

Exporter restore succeeded. Feature 03 and final F2–F4 Release builds passed with zero warnings and zero errors:

```bash
dotnet restore src/WftEngineering.slnx --use-lock-file
dotnet build src/WftEngineering.slnx -c Release --no-restore -m:1 -nodeReuse:false
```

Native streaming/merge and telemetry APIs were checked against restored package signatures and compiled. Primary references:

- [Agent Framework looping and streaming behavior](https://learn.microsoft.com/en-us/agent-framework/agents/looping)
- [Agent Framework observability](https://learn.microsoft.com/en-us/agent-framework/agents/observability)
- [OpenTelemetry .NET OTLP exporter](https://opentelemetry.io/docs/languages/dotnet/exporters/#otlp)
- [Microsoft.Extensions.AI OpenTelemetry implementation](https://github.com/dotnet/extensions/blob/main/src/Libraries/Microsoft.Extensions.AI/ChatCompletion/OpenTelemetryChatClient.cs)

Documentation/examples on main informed composition; the pinned packages and compilation determine actual API compatibility. No runtime behavior is inferred from build success.

## Presenter checks — pending

Use [docs/demo-runbook.md](../../../demo-runbook.md). Confirm actual model/agent span nesting, tool/approval/continuation/root correlation, Trace ID lookup, privacy of exported tags/events, authenticated dashboard access, and exporter closure for happy/reject/stuck/cancellation. Also inspect console readability and streamed approval handling. These checks were intentionally left to the presenter.
