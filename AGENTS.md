# Have-Fun Agent Guide

## Project Intent

Have-Fun is a local LAN party-game web app. One computer hosts the server, and friends join from browsers on the same network. V1 focuses on a single Word Scramble game.

Use these files as the source of truth:

- `design/spec.md` for product behavior and constraints.
- `design/stages/*/plan.md` for stage-level implementation plans.
- `design/stages/*/tasks/*.md` for task-sized implementation work.

## Stack

- Use .NET / ASP.NET Core.
- Use Blazor Web App with `InteractiveServer` render mode.
- Use MudBlazor library including styles for all web components
- Keep implementation code under `src`.
- Solution: `src/HaveFun.sln`.
- Projects:
  - `src/HaveFun.Web`: Blazor UI and runnable web host.
  - `src/HaveFun.Core`: reusable backend models and services.

## Current Architecture Rules

- Keep app state in memory.
- Do not add a database for V1.
- Do not add cloud services, public hosting, accounts, passwords, or authentication for V1.
- Keep reusable game/configuration/sentence logic in `HaveFun.Core`.
- Keep Blazor pages, startup, app settings, and web-specific services in `HaveFun.Web`.
- Prefer typed models and services over ad hoc JSON or string handling.
- Keep Razor markup and code-behind in separate files when a component needs non-trivial logic.
- Add reusable services to `HaveFun.Core`.
- Do not add data access or persistence unless a later stage explicitly introduces it.

## Repository Layout

```text
src/
  HaveFun.sln                  Main solution
  HaveFun.Web/                 Blazor app, pages, layouts, static files, startup
  HaveFun.Core/                Reusable services, models, options, validation
design/                        Product spec, stage plans, and task notes
```

### Folder Structure

#### HaveFun.Core project

- Put reusable services in a `Services` folder.
- Add Service suffix to each service and its interface
- Put service interfaces in `Services/Contracts`.
- Put shared models in a `Models` folder.
- Put configuration/options models in a clear configuration folder.
- Do not let folder structure create deep namespaces by default.
- Use the root `HaveFun.Core` namespace for most classes unless a sub-namespace is already established.
- Do not add repository/data-access folders during V1 unless the spec changes to include persistence.

#### HaveFun.Web project

- Set an explicit namespace in each Razor file.
- Keep pages in a `Pages` folder.
- Keep layouts in a `Layouts` folder.
- Keep reusable UI components in a `Components` folder.
- Do not let folder structure create deep namespaces by default.
- Use the root `HaveFun.Web` namespace for most web classes and Razor components.


## Configuration and Data

- Master name comes from `Game:MasterName` in `src/HaveFun.Web/appsettings.json`.
- Predefined sentences live in `src/HaveFun.Web/assets/sentences.json`.
- Sentence shape:

```json
[
  {
    "text": "The quick brown fox jumps over the lazy dog",
    "timeLimitInSeconds": 30
  }
]
```

- Validate required configuration and sentence data at startup.
- Fail clearly when local configuration/data is invalid.

## Implementation Workflow

- Work stage by stage.
- For Stage 1, complete tasks in `design/stages/Stage 1 - App Shell and Configuration/tasks`.
- Do not implement behavior from later stages unless the current task explicitly requires it.
- Keep Stage 1 free of name entry, player registration, dashboard routing, rounds, and gameplay.

## Verification

Run builds from `src`:

```powershell
dotnet build .\HaveFun.sln
```

When running the web app:

```powershell
dotnet run --project .\HaveFun.Web\HaveFun.Web.csproj
```

Before finishing implementation work, verify:

- Solution builds successfully.
- `HaveFun.Web` starts.
- No database or cloud dependency is introduced.
- Changes match the relevant task file and `design/spec.md`.
