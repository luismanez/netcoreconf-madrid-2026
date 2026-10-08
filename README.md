# WTF Engineering

This repository contains the materials, demos, and resources used during the session **WTF Engineering** delivered during the NetCoreConf Madrid 2026 (23 October 2026).

![session-banner](./assets/session-banner.jpeg)

## 📝 Abstract

¿Cansado de que cada semana aparezca un nuevo «random string» engineering? Seguramente has oído hablar de Prompt Engineering, Context Engineering... Loop Engineering... ¡Oh, otro más! Graph Engineering... y mi favorito: Harness Engineering. Si es el caso, vente a esta sesión. Pondremos orden en todo esto, veremos qué aporta cada concepto, cuándo tiene sentido y profundizaremos en Harness Engineering y en cómo Microsoft Agent Framework nos lo pone fácil a la hora de montar nuestro propio harness.

## 📚 Repository contents

- [`slides`](./slides) — Presentation slides
- [`src`](./src) — Source code and demos
- [`docs`](./docs) — Additional documentation and supporting material
- [`assets`](./assets) — Images and other session assets

## 🚀 About this session

### Demo quick start

Install the .NET 10 SDK, then run these commands from the repository root:

```bash
cp env.template .env
```

Fill in `FOUNDRY_PROJECT_ENDPOINT` and `FOUNDRY_MODEL` in `.env`. The template leaves all values empty; optional OTLP settings have local defaults. Process environment variables override `.env`.

```bash
dotnet restore src/WftEngineering.slnx --locked-mode
dotnet build src/WftEngineering.slnx -c Release --no-restore
az login
dotnet run --project src/WftEngineering.Demo -c Release --no-build -- --scenario happy
```

The demo loads the production-change Skill, queries fake change/health data, shows native todos, and requests human approval. Type `APPROVE` to start DEP-742 and let the bounded native loop verify deployment and subsequent health/version; other input rejects. The console streams agent text separately from operational facts and exports traces over local OTLP. Use a Foundry deployment supporting Responses and function calling.

Available scenarios: `happy`, `stuck`, `validation-failed`, `deployment-failed`, and `post-health-failed`. See the [setup, scenario commands, and twelve-minute walkthrough](docs/demo-runbook.md). Features 01–04 are implemented and compile; Foundry/Aspire E2E and rehearsal remain with the presenter. [Compilation evidence and pending checks](docs/specs/features/04-conference-scenarios/rehearsal.md).

This conference demo uses one console project and no automated testing.

This repository is intended to accompany the live session and provide attendees with access to the code, examples, and resources discussed during the presentation.

Feel free to explore the demos, reuse the examples, and open an issue if you find something that could be improved.

## 👨‍💻 Author

**Luis Mañez**

- GitHub: [Luis Mañez](https://github.com/luismanez)
- Microsoft Foundry MVP
- Chief Architect @ [ClearPeople](https://www.clearpeople.com/)

## ❤️ Supported by ClearPeople

My participation in community events and conferences is supported by **ClearPeople**.

Learn more about ClearPeople at [clearpeople.com](https://www.clearpeople.com/).

## 📄 License

Unless otherwise stated, the source code in this repository is available under the terms of the repository license.
