# Feature 01: implementation and compatibility evidence

**Historical F1 checkpoint:** F1 ended at Running. The current application includes F2–F4 and verifies happy through Completed; use the [current runbook](../../../demo-runbook.md). E2E/rehearsal is presenter-owned and remains pending.

**Date:** 8 October 2026.

One console project, concrete services, and a small in-memory business store implement the flow. Harness composition is in `Program.cs`; presentation and the single approval round trip are in `DemoApplication.cs`. There is no testing project or additional application layer.

## Restored and compiled dependencies

| Dependency | Version |
|---|---|
| .NET SDK / target | `10.0.202` / `net10.0` |
| Microsoft.Agents.AI.Harness / core | `1.23.0` |
| Azure.AI.Projects | `3.0.0-beta.3` |
| Azure.Identity | `1.21.0` |
| Microsoft.Extensions.AI.OpenAI | `10.10.1` |
| Azure.AI.Extensions.OpenAI (transitive) | `3.0.0-beta.1` |
| OpenAI (transitive) | `2.14.0` |
| DotNetEnv / Spectre.Console | `3.2.0` / `0.57.2` |

Generated `packages.lock.json` records all resolved dependencies. The Responses APIs require scoped acknowledgement of `OPENAI001`; the Harness APIs require `MAAI001`. These specific experimental diagnostics are acknowledged without suppressing ordinary warnings.

## Native APIs used

- `AIProjectClient` → `GetProjectOpenAIClient` → `GetResponsesClient` → `AsIChatClient(model)`, authenticated with `AzureCliCredential`. Both project and inference client options use `ClientRetryPolicy(1)`.
- `AsHarnessAgent`, `AgentFileSkillsSource`, and read-only Skill auto-approval. No script runner, file access, shell, background agents, web search, memory, modes, or compaction.
- `GetService<TodoProvider>().GetAllTodosAsync(session)` and native `InMemoryChatHistoryProvider.GetMessages(session)` for the presentation. No separate context builder.
- `ApprovalRequiredAIFunction`, `ToolApprovalRequestContent.ToolCall`, `CreateResponse`, and the same session. Native approval-response binding stays enabled.
- `MaximumIterationsPerRequest = 12`; no `LoopEvaluators` in F1. The native external loop belongs to F2.
- `OpenTelemetrySourceName = WftEngineering.Demo`, `EnableSensitiveData = false`, and operational activities. Export belongs to F3.

Signatures were checked in restored package XML and compiled, using the [official Harness sample](https://github.com/microsoft/agent-framework/blob/dotnet-1.23.0/dotnet/samples/02-agents/Harness/Harness_Step05_Loop/Program.cs) and [Harness documentation](https://learn.microsoft.com/en-us/agent-framework/concepts/harness) as references. Compilation confirms API compatibility, not live model behavior.

## Executed verification

```bash
dotnet restore src/WftEngineering.Demo/WftEngineering.Demo.csproj --use-lock-file
dotnet build src/WftEngineering.slnx -c Release --no-restore -m:1 -nodeReuse:false
dotnet format src/WftEngineering.slnx --no-restore
dotnet format src/WftEngineering.slnx --verify-no-changes --no-restore
```

Restore and Release build passed; the build reported zero warnings and zero errors. Manual CLI checks using the Release executable confirmed:

| Invocation | Observed result |
|---|---|
| `--help` | Usage, provider prerequisite, and F1 scope; exit 0 without configuration |
| `--scenario invalid` | Unsupported scenario; exit 2 before model invocation |
| `--scenario happy` with both required process variables empty | Named missing configuration error; exit 1 before client creation |

The copied Skill is present in the Release output. Source review confirms that business authorization remains in the store, failed runs stay terminal, and the host cannot approve altered arguments or claim operational completion from model text.

## Pending live rehearsal

No `.env` is configured. After `az login`, copy `env.template` to `.env` and set the project endpoint and a deployment supporting Responses and function calling. Run from the repository root:

```bash
dotnet run --project src/WftEngineering.Demo -c Release --no-build -- --scenario happy
```

Observe the loaded Skill, queried change/window/health, five native todos, and exact approval target. Type `APPROVE`: expect one DEP-742 in Running, service still on 2.6.3, and verification pending. Run again and reject: expect zero deployments. EOF/unrecognized input rejects; Ctrl+C cancels. Keyboard waiting is excluded from the active execution budget.

Console input uses a cancellable wait around the blocking read because [Console.In reads synchronously](https://learn.microsoft.com/en-us/dotnet/api/system.io.textreader.readlineasync?view=net-10.0). This lets Ctrl+C return control to the host without requiring Enter.

Model-specific behavior, actual native approval continuation, and cancellation during live inference remain unverified. No automatic tests or scripted model substitutes were added or run, following the presenter's explicit decision.
