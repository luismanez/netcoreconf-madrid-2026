# Tasks: 01-governed-production-change

Initial status: planning complete; implementation pending. Checkboxes represent execution evidence, not implicit approval.

Scaffolding checkpoint (8 October 2026): project creation, `.env` loading, settings validation, DI, Spectre.Console, and nine configuration tests are complete. See [scaffolding evidence](../../scaffolding.md). F1-T1 remains pending until the Foundry client and pinned Harness/Skills/approval/loop APIs are verified; no business flow has been implemented.

## F1-T1 — Start the console and verify compatibility

- [ ] Implemented and verified.

**Description:** extend the existing scaffold with minimal client/harness configuration. Check the spec's API family and versions before establishing contracts on them. Preserve injected settings and Spectre.Console output. Incorrect configuration fails at startup; no real business systems are accessed.

**Dependencies:** none.
**Scope:** M, 5 edited files; additional generated lock files.

**Planned files:**

- `global.json`
- `src/WftEngineering.Demo/WftEngineering.Demo.csproj`
- `src/WftEngineering.Demo/Program.cs`
- `src/WftEngineering.Demo.Tests/WftEngineering.Demo.Tests.csproj`
- `docs/specs/features/01-governed-production-change/compatibility.md`, brief evidence written during implementation
- Generated: `src/WftEngineering.Demo/packages.lock.json`
- Generated: `src/WftEngineering.Demo.Tests/packages.lock.json`

**Acceptance:**

- [ ] .NET 10 and proposed dependencies restore/compile; incompatibilities are resolved in the spec with evidence, without changing frameworks.
- [ ] Foundry uses an explicit endpoint/model; missing variables produce a readable error before starting the flow.
- [ ] HarnessAgent, Skills, TodoProvider, approval, and looping signatures are verified against the package; additional capabilities and sensitive capture are disabled.

**Verification:**

```bash
dotnet --info
dotnet restore src/WftEngineering.Demo/WftEngineering.Demo.csproj --use-lock-file
dotnet restore src/WftEngineering.Demo.Tests/WftEngineering.Demo.Tests.csproj --use-lock-file
dotnet build src/WftEngineering.Demo/WftEngineering.Demo.csproj -c Release --no-restore
FOUNDRY_PROJECT_ENDPOINT='' FOUNDRY_MODEL='' dotnet run --project src/WftEngineering.Demo/WftEngineering.Demo.csproj -c Release --no-build -- --scenario happy
```

The last command explicitly overrides `.env` with empty required values and must terminate with a configuration error without invoking the model once F1 supports the happy scenario. Record the SDK, restored versions, and checked signatures in `compatibility.md`; native behavior tests are added in T2/T3 and F2.

## F1-T2 — Validate the change with a Skill, tools, and external state

- [ ] Implemented and verified.

**Description:** add deterministic data/reads, specialized context, and visible tasks. Create tests and the scripted client to check reads and guards without credentials.

**Dependencies:** F1-T1.
**Scope:** M, 5 edited files.

**Planned files:**

- `src/WftEngineering.Demo/Demo/DemoDeploymentStore.cs`
- `src/WftEngineering.Demo/Tools/DeploymentTools.cs`
- `src/WftEngineering.Demo/skills/production-change/SKILL.md`
- `src/WftEngineering.Demo.Tests/ProductionChangeTests.cs`
- `src/WftEngineering.Demo/Program.cs`

**Acceptance:**

- [ ] Change and health come from tools; the prompt contains only the requested target and general instructions. The Skill loads from the output directory.
- [ ] An unapproved/missing change, closed window, mismatch, and Unknown/Unhealthy health produce ValidationFailed with zero deployments; repeated reads do not erase a failure.
- [ ] Five todos remain in the session and are read through the provider; declared progress does not change permissions or evidence. Minimal operational read activities exist.

**Verification:**

```bash
dotnet restore src/WftEngineering.Demo.Tests/WftEngineering.Demo.Tests.csproj --use-lock-file
dotnet test src/WftEngineering.Demo.Tests/WftEngineering.Demo.Tests.csproj -c Release --filter 'FullyQualifiedName~Validation'
dotnet build src/WftEngineering.Demo/WftEngineering.Demo.csproj -c Release
```

Use test names containing `Validation`. The scripted client must inspect context and exercise the real harness without reimplementing its decisions. Check Skill loading outside the project's working directory with a file-resolution test.

### Checkpoint after T2

- [ ] The console compiles and the validation path is verifiable offline.
- [ ] Fakes do not introduce a real clock, randomness, external APIs, or executable capabilities.
- [ ] Diff reviewed: general instructions, procedure, and data remain separate.

## F1-T3 — Start deployment through native human approval

- [ ] Implemented and verified.

**Description:** connect the side effect to the native approval protocol and minimal UX. Check it against store conditions before asking and revalidate them during execution.

**Dependencies:** F1-T2.
**Scope:** M, 4 edited files.

**Planned files:**

- `src/WftEngineering.Demo/Program.cs`
- `src/WftEngineering.Demo/Tools/DeploymentTools.cs`
- `src/WftEngineering.Demo/Demo/DemoDeploymentStore.cs`
- `src/WftEngineering.Demo.Tests/ProductionChangeTests.cs`

**Acceptance:**

- [ ] DeployService is wrapped in ApprovalRequiredAIFunction; the question shows exact arguments and the native response uses the same session. Zero deployments before acceptance.
- [ ] Rejection, EOF, unrecognized input, altered/unbound arguments, false permission, or a changed precondition prevent deployment; no side effect is automatically approved.
- [ ] Acceptance starts DEP-742 once; repetition is idempotent and a different operation is denied. Output keeps verification pending even if the model announces success.

**Verification:**

```bash
dotnet test src/WftEngineering.Demo.Tests/WftEngineering.Demo.Tests.csproj -c Release --filter 'FullyQualifiedName~Approval'
dotnet test src/WftEngineering.Demo.Tests/WftEngineering.Demo.Tests.csproj -c Release
dotnet build src/WftEngineering.Demo/WftEngineering.Demo.csproj -c Release --no-restore
dotnet format src/WftEngineering.Demo/WftEngineering.Demo.csproj --verify-no-changes --no-restore
```

Tests named `Approval`: exercise the complete harness protocol and count store side effects. Include direct guards and idempotency. Manual inspection: instructions, tools, wrapper, and session are easy to locate in `Program.cs`; the pause and approval activities are present.

## Feature completion

- [ ] T1–T3 verified, without weakening controls to make the demo easier.
- [ ] Existing tests and build pass; no secrets or sensitive content capture.
- [ ] `compatibility.md` and this checklist contain evidence of completed work and any pending live rehearsal.

## Execution evidence

Pending. Record executed commands, results, manual checks, and limitations per task. Do not mark the Foundry rehearsal complete based on the scripted client.
