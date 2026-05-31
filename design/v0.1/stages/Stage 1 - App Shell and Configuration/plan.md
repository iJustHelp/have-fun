# Stage 1 Plan: App Shell and Configuration

## Objective

Create the runnable local web app foundation for Have-Fun. This stage should prove that the app can start without a database, load configuration and predefined sentences, and show a first browser page from the local server.

## Tech Stack

- Use .NET / ASP.NET Core Blazor Web App.
- Configure Blazor with `InteractiveServer` render mode.
- Place all implementation code under `src`.
- Create solution `HaveFun`.
- Create two projects:
  - `HaveFun.Web` for the Blazor UI and runnable web host.
  - `HaveFun.Core` for reusable backend models and services.

## Project Structure

Recommended structure:

```text
src/
  HaveFun.sln
  HaveFun.Web/
  HaveFun.Core/
design/
  idea.md
  spec.md
  stages/
```

Responsibilities:

- `src/HaveFun.Web`: Blazor UI, app startup, configuration binding, static assets, first page, and local/LAN URL presentation.
- `src/HaveFun.Core`: settings models, sentence models, sentence loading, sentence validation, and core services that are not UI-specific.
- `design/`: specs, plans, and implementation-stage documentation only.

## Scope

Included:

- Create the `src` folder.
- Create the `HaveFun` solution under `src`.
- Create the `HaveFun.Web` Blazor Web App project.
- Create the `HaveFun.Core` backend class library project.
- Add a project reference from `HaveFun.Web` to `HaveFun.Core`.
- Configure `HaveFun.Web` as the runnable app.
- Add app configuration for the Game Master name.
- Add a JSON sentence source file.
- Load master name and sentences during server startup.
- Add a first page that confirms the app is running.
- Show the local URL and, when available, the LAN join URL.

Not included:

- Name entry behavior.
- Player registration.
- Dashboard routing.
- Starting rounds.
- Real-time game state.
- Word Scramble gameplay.

## Configuration

Add `Game:MasterName` to `appsettings.json`.

Example:

```json
{
  "Game": {
    "MasterName": "master"
  }
}
```

Rules:

- `Game:MasterName` is required.
- The value must not be empty or whitespace.
- The loaded value should be available through a typed settings object in `HaveFun.Core`.
- `HaveFun.Web` should bind and validate the settings during startup.

## Sentence Source

Add a JSON file for predefined Word Scramble sentences.

Recommended file:

```text
src/HaveFun.Web/assets/sentences.json
```

Recommended shape:

```json
[
  {
    "text": "The quick brown fox jumps over the lazy dog",
    "timeLimitInSeconds": 30
  }
]
```

Rules:

- The file is loaded when the server starts.
- `text` is required and must not be empty or whitespace.
- `timeLimitInSeconds` is required and must be greater than zero.
- Startup should fail clearly if the file is missing or invalid.
- Sentence models and validation should live in `HaveFun.Core`.
- Loaded sentences should be available from an in-memory service registered by `HaveFun.Web`.

## App Shell

Build the first visible browser page as a Blazor page/component.

Required content:

- App name: `Have-Fun`.
- A short running status.
- Local URL for testing on the host machine.
- LAN join URL when the server can detect one.
- A simple placeholder area where Stage 2 will add name entry.

Behavior:

- The page loads without requiring a database.
- The page works from `localhost`.
- The page is ready for Stage 2 name entry but does not implement it.
- The page does not yet implement Game Master or Player routing.
- The page uses Blazor `InteractiveServer` render mode.

## LAN URL Detection

Add a small server-side service that provides possible join URLs.

Rules:

- Always provide a localhost URL.
- Try to detect a non-loopback IPv4 address for LAN use.
- If LAN IP detection fails, keep the app usable with the localhost URL.
- Do not require internet access.

## Validation and Error Handling

Startup validation should cover:

- Missing or empty `Game:MasterName`.
- Missing sentence file.
- Empty sentence list.
- Sentence with missing or empty `text`.
- Sentence with `timeLimitInSeconds` less than or equal to zero.

Errors should be clear enough for the host to fix local files.

## Done Criteria

Stage 1 is complete when:

- The app starts locally on Windows and Mac-compatible tooling.
- The `HaveFun` solution exists under `src`.
- `HaveFun.Web` runs successfully as the web app.
- `HaveFun.Core` contains reusable configuration and sentence models/services.
- `HaveFun.Web` references `HaveFun.Core`.
- Blazor `InteractiveServer` render mode is configured.
- The first page loads in a browser.
- `Game:MasterName` is read from `appsettings.json`.
- Sentences are loaded from JSON into memory.
- The first page shows the local URL and a LAN URL when available.
- No database, cloud service, authentication, player routing, or name validation is implemented in Stage 1.

## Test Plan

Manual checks:

- Build the solution from `src`.
- Run `HaveFun.Web`.
- Open the local URL.
- Confirm the first page renders.
- Confirm the displayed master name is not shown publicly.
- Confirm `Game:MasterName` loads from `appsettings.json`.
- Confirm at least one sentence loads into memory through `HaveFun.Core`.
- Confirm a LAN URL appears when a non-loopback IPv4 address is available.
- Confirm the app starts without a database or cloud dependency.

Failure checks:

- Empty `Game:MasterName` causes a clear startup error.
- Missing sentence file causes a clear startup error.
- Empty sentence list causes a clear startup error.
- Invalid `timeLimitInSeconds` causes a clear startup error.

## Handoff to Stage 2

Stage 2 can build on:

- Working app startup.
- Loaded master name.
- Loaded sentence list.
- First browser page.
- Local and LAN URL service.
- `HaveFun.Web` Blazor project configured with `InteractiveServer`.
- `HaveFun.Core` project for shared game models and services.
