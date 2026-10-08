# Demo scaffolding

**Status:** the original scaffold is implemented and extended by features 01–04. See the [current runbook](../demo-runbook.md) and [compilation evidence](features/04-conference-scenarios/rehearsal.md).

## Structure and decisions

One .NET 10 console project lives in `src/WftEngineering.Demo/`; `src/WftEngineering.slnx` contains only that project. `Program.cs` loads configuration and composes concrete services with built-in DI. `DemoSettings.cs` validates typed settings. `DemoApplication.cs` presents the session with an injected Spectre.Console `IAnsiConsole`.

Source, comments, and documentation are in English. The presenter removed the original testing project and explicitly chose no automated tests or testing infrastructure for this conference demo. All future implementation follows that decision; verification uses compilation, source review, and brief manual checks.

## Configuration

DotNetEnv loads `.env` from the working directory with `LoadOptions.NoEnvVars()`, without modifying the process environment. Environment variables are added last and override file values, including explicitly empty values. Run from the repository root. A missing `.env` is allowed when the required process variables are supplied.

| Variable | Requirement / default |
|---|---|
| `FOUNDRY_PROJECT_ENDPOINT` | Required HTTPS project URL without embedded credentials |
| `FOUNDRY_MODEL` | Required model deployment name |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | Optional; `http://localhost:4317` |
| `OTEL_EXPORTER_OTLP_PROTOCOL` | Optional; `grpc` |

`env.template` leaves every value empty. `.env` and `.env.*` are ignored by Git. Missing/invalid configuration exits 1 without printing values or raw exceptions. Help exits 0 without configuration; unsupported arguments exit 2 before model invocation.

Authentication in F1 uses `AzureCliCredential`; sign in with `az login`. Telemetry settings configure the implemented gRPC OTLP exporter.

## Pinned scaffold dependencies

| Component | Version |
|---|---|
| .NET SDK / target | `10.0.202` / `net10.0` |
| DotNetEnv | `3.2.0` |
| Spectre.Console | `0.57.2` |
| Microsoft.Extensions.Configuration / EnvironmentVariables / DependencyInjection | `10.0.6` |

`global.json` and the generated application `packages.lock.json` pin the environment. Agent dependencies and compiled API evidence are recorded in [F1 compatibility.md](features/01-governed-production-change/compatibility.md).

## Commands

```bash
cp env.template .env
# Fill the required Foundry variables.
az login
dotnet restore src/WftEngineering.slnx --locked-mode
dotnet build src/WftEngineering.slnx -c Release --no-restore
dotnet run --project src/WftEngineering.Demo -c Release --no-build -- --scenario happy
```

For help without configuration:

```bash
dotnet run --project src/WftEngineering.Demo -- --help
```

The presentation shows the Skill, tool-read facts, native todos, one human approval, streamed agent text, native iterations, and the operational summary/Trace ID. Approval starts a simulated Running deployment; the native loop verifies subsequent status/health/version. Traces are exported through local OTLP. Runtime/E2E checks remain with the presenter.

## Sources

- [DotNetEnv configuration provider](https://github.com/tonerdo/dotnet-env#using-net-configuration-provider)
- [Built-in .NET dependency injection](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection/basics)
- [Spectre.Console widgets](https://spectreconsole.net/console/widgets/figlet)
