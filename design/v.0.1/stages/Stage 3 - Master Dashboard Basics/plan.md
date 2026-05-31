# Stage 3 Plan: Master Dashboard Basics

## Objective

Build the first usable Master Dashboard without live gameplay. This stage should let the Game Master see the shared LAN URL, choose a predefined sentence, see connected players, and start a selected round into in-memory server state.

## Context

Stage 3 builds on the current app foundation:

- `HaveFun.Web` is the Blazor Web App with `InteractiveServer` render mode.
- MudBlazor is used for visible web UI.
- The first landing page registers a user name.
- Browser session storage keeps only the current user's `name` and `role`.
- The dashboard renders only when session storage role is `Master`.
- Players register into the in-memory player registry.
- Sentences are loaded from JSON through `HaveFun.Core`.
- LAN URL detection is available through the existing join URL provider.

## Scope

Included:

- Replace the Master Dashboard placeholder with a MudBlazor dashboard.
- Keep role-based dashboard rendering from session storage.
- Show the shared LAN URL.
- Show the connected player list.
- Show the predefined sentence selector.
- Show the selected sentence preview.
- Add a Start Round button.
- Add in-memory round state for selected sentence and round status.
- Allow the Game Master to start a selected round.
- Show round status on the dashboard.

Not included:

- Player screens receiving live round state.
- Shuffled word generation for players.
- Player gameplay.
- Player submissions.
- Correctness calculation.
- Ranking.
- Result grid with submitted answers.
- Database, persistence, cloud services, accounts, passwords, or real authentication.

## Architecture

Use `HaveFun.Core` for reusable round state and dashboard-facing game services.

Recommended Core additions:

- `RoundStatus` enum:
  - `NotStarted`.
  - `Started`.
- `CurrentRound` model:
  - Round id.
  - Selected sentence text.
  - Time limit in seconds.
  - Status.
  - Started timestamp when started.
- `IGameState` service contract:
  - Get current round state.
  - Select or start a round from a sentence.
  - Return connected players for dashboard display, if not already exposed by existing registry.
- In-memory `GameState` implementation.

Use `HaveFun.Web` for dashboard pages, MudBlazor components, and session-role checks.

Recommended Web updates:

- Update `Pages/Dashboard.razor` and `Pages/Dashboard.razor.cs`.
- Register the in-memory game state service in `Program.cs`.
- Keep `IUserSessionStorage` as the source for current browser role and name.
- Keep `IPlayerRegistry` as the source for connected players.
- Keep `ISentenceLibrary` as the source for predefined sentences.

## Session and Role Rules

Rules:

- Browser session storage stores only `name` and `role`.
- Dashboard content renders only when session role is `Master`.
- If session storage is missing or role is not `Master`, dashboard shows a clear message and a way back to registration.
- Do not store selected sentence, round id, or dashboard state in browser session storage.
- Server memory is the source of truth for players, sentences, and current round state.

## Dashboard UI

Use MudBlazor components and styles for all dashboard UI.

Required UI:

- Page title: `Master Dashboard`.
- Shared LAN URL.
- Connected players list or table.
- Sentence selector.
- Selected sentence preview.
- Start Round button.
- Round status display.

Recommended layout:

- Top area with shared LAN URL and round status.
- Main area with sentence selector and preview.
- Side or lower area with connected players.

Dashboard behavior:

- The Start Round button is disabled until a sentence is selected.
- Starting a round stores the selected sentence in server memory.
- Starting a round updates dashboard round status.
- Starting a round does not yet update player screens in Stage 3.
- Starting another round can replace the current selected round state, but full repeat-round polish remains Stage 7.

## Player List

Use the existing in-memory player registry.

Rules:

- Show connected player display names.
- If no players have joined, show an empty state.
- Do not implement disconnect detection in Stage 3 unless it already exists.
- Do not implement result columns yet; those arrive in later stages.

## Sentence Selector

Use the existing sentence library loaded at startup.

Rules:

- Show all predefined sentences.
- Display enough sentence text for the Game Master to choose.
- Show the selected sentence text and time limit.
- Do not allow custom sentence editing in Stage 3.

## Round State

Stage 3 stores only the basics needed to prove the Game Master can start a round.

State to store:

- Current round id.
- Selected sentence text.
- Selected sentence time limit.
- Round status.
- Round start time.

State not required yet:

- Shuffled words.
- Player-specific round views.
- Player submissions.
- Spent time.
- Correctness.
- Rank.

Restart behavior:

- Restarting the server clears current round state and connected player state.

## Done Criteria

Stage 3 is complete when:

- Dashboard renders only for session role `Master`.
- Dashboard shows the shared LAN URL.
- Dashboard shows connected players from the in-memory registry.
- Dashboard shows predefined sentences from the sentence library.
- Game Master can select a sentence.
- Game Master can start a selected round.
- Dashboard shows current round status after start.
- Server memory stores the current round basics.
- No player gameplay, result submission, correctness, ranking, database, cloud service, accounts, passwords, or auth system is added.

## Test Plan

Manual checks:

- Build from `src` with `dotnet build .\HaveFun.sln`.
- Run `HaveFun.Web`.
- Join as the configured master name.
- Confirm `/dashboard` renders.
- Confirm the shared LAN URL is visible.
- Confirm predefined sentences appear in the selector.
- Confirm connected players appear after players join in another tab or device.
- Confirm Start Round is disabled before selecting a sentence.
- Select a sentence and confirm the preview updates.
- Start the round and confirm round status updates.
- Refresh the dashboard and confirm role-based access still uses session storage `name` and `role`.

Failure checks:

- Open `/dashboard` without session storage and confirm dashboard content is not shown.
- Join as a player and open `/dashboard`; confirm dashboard content is not shown.
- Confirm the app still starts without any database or cloud dependency.

## Handoff to Stage 4

Stage 4 can build on:

- Master-only dashboard route.
- Connected player list.
- Selected/current round state.
- Started round status.
- Current sentence text and time limit in server memory.
