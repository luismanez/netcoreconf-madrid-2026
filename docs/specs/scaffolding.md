# Demo scaffolding

Implemented on 8 October 2026. This checkpoint covers startup and presentation; the four agent features remain pending.

## Architecture

One .NET 10 console project and one xUnit project, both under `src/`. No application layers, custom service interfaces, background services, or generic host are needed for this short-lived console demo.

| File | Responsibility |
|---|---|
| `src/WftEngineering.Demo/Program.cs` | Load `.env`, validate settings, register DI services, resolve the application, and handle startup errors |
| `src/WftEngineering.Demo/DemoSettings.cs` | Immutable settings and validation with errors that identify variables without echoing values |
| `src/WftEngineering.Demo/DemoApplication.cs` | Spectre.Console banner, panel, and configuration table through injected `IAnsiConsole` and settings |
| `src/WftEngineering.Demo.Tests/DemoSettingsTests.cs` | Configuration validation tests without a live model |
| `src/WftEngineering.slnx` | Both projects for restore, build, test, and IDE navigation |

`Program.cs` is the composition root. Built-in `ServiceCollection` injects `IConfiguration`, `DemoSettings`, `IAnsiConsole`, and the concrete `DemoApplication`. The service provider is validated at creation and disposed at exit. Future agent services can use the same composition root; no additional application projects are planned.

The scaffold does not construct or call a model, start a deployment, request approval, or export telemetry. Startup success means configuration is syntactically valid, not that Foundry access has been verified. Agent SDK dependencies and API compatibility checks belong to F1; the existing task checkboxes remain pending.

## Configuration

Run from the repository root. Copy `env.template` to `.env` and edit it locally. All template values are empty. `.env` is not copied into build output and is ignored by git, along with `.env.*` variants.

DotNetEnv loads `.env` from the working directory into .NET configuration without changing the process environment. The environment-variable provider is added last, so process values override the file, including an explicitly empty value. A missing `.env` is allowed when the required process variables exist.

| Variable | Required | Validation / default |
|---|---|---|
| `FOUNDRY_PROJECT_ENDPOINT` | Yes | Absolute HTTPS URL without embedded credentials |
| `FOUNDRY_MODEL` | Yes | Nonempty model deployment name |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | No | HTTP(S) URL without embedded credentials; empty/unset defaults to `http://localhost:4317` |
| `OTEL_EXPORTER_OTLP_PROTOCOL` | No | `grpc`; empty/unset defaults to `grpc` |

Missing/invalid configuration exits with code 1. Malformed `.env` or another startup failure uses a generic error rather than printing exception details or configuration values. `--help` works without configuration and exits with code 0; unsupported arguments exit with code 2 before loading configuration. Scenario arguments will be implemented by the agent features.

Authentication remains the planned `AzureCliCredential`; no API key is required by the template. Settings are registered directly as an immutable object rather than introducing an options/binding layer for four values.

## Verified dependency baseline

| Component | Version |
|---|---|
| SDK in `global.json` | `10.0.202` (patch roll-forward within the feature band) |
| DotNetEnv | `3.2.0` |
| Spectre.Console | `0.57.2` |
| Microsoft.Extensions.Configuration | `10.0.6` |
| Microsoft.Extensions.Configuration.EnvironmentVariables | `10.0.6` |
| Microsoft.Extensions.DependencyInjection | `10.0.6` |
| Microsoft.NET.Test.Sdk | `18.0.1` |
| xUnit | `2.9.3` |
| xUnit Visual Studio runner | `3.1.4` |

Both projects include generated `packages.lock.json`. Restore with `--locked-mode` to detect dependency drift. The agent and OTLP exporter package versions in the [global spec](production-change-demo.md) have not yet been restored or verified by this scaffold.

## Run and verify

From the repository root:

```bash
cp env.template .env
# Fill in FOUNDRY_PROJECT_ENDPOINT and FOUNDRY_MODEL before running.
dotnet restore src/WftEngineering.slnx --locked-mode
dotnet build src/WftEngineering.slnx -c Release --no-restore
dotnet test src/WftEngineering.slnx -c Release --no-build --no-restore
dotnet format src/WftEngineering.slnx --verify-no-changes --no-restore
dotnet run --project src/WftEngineering.Demo -c Release --no-build
```

For usage without configuration:

```bash
dotnet run --project src/WftEngineering.Demo -c Release --no-build -- --help
```

The startup screen renders an ASCII-art banner, a panel for the five Engineering concepts, and a configuration status table. It states that the agent workflow is pending. Spectre.Console handles terminal capability detection; rendering stays in a single small application class.

## Execution evidence

- Restore and Release compilation succeeded with SDK `10.0.202` on macOS arm64.
- Solution restore in locked mode and Release build passed with zero warnings/errors; `dotnet format --verify-no-changes --no-restore` passed.
- Nine xUnit cases passed: required values, invalid/credential-bearing Foundry URLs, optional defaults, explicit OTLP endpoint, and invalid telemetry configuration. No tests skipped.
- Ten runtime checks used temporary `.env` files and fictional values: valid file, missing configuration, empty template, environment override (including explicitly empty values), process-only configuration, malformed file, unsupported protocol, help, and unknown arguments. Expected exit codes and messages were verified; canary values were absent from output.
- Startup output was inspected in a terminal: banner, panel, status table, and the explicit pending-workflow message rendered correctly.
- Live Foundry, Harness/Skills/approval/loop APIs, business tools, OTLP export, and conference rehearsal are still pending. Startup checks did not consume a model or create cloud resources.

## Official sources

- [DotNetEnv configuration provider](https://github.com/tonerdo/dotnet-env#using-net-configuration-provider): `.env` loading with `LoadOptions.NoEnvVars()`.
- [.NET DI basics](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection/basics): direct `ServiceCollection` and concrete-service injection without a generic host.
- [Spectre.Console FigletText](https://spectreconsole.net/console/widgets/figlet), [panels](https://spectreconsole.net/console/widgets/panel/), and [tables](https://spectreconsole.net/console/how-to/displaying-tabular-data/): presentation components used by the scaffold.
