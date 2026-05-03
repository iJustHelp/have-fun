# Stage 2 Plan: Name Entry and Role Routing

## Objective

Build the shared first-page join experience for Game Master and Players. This stage should let a user enter a name, route the configured master name to the Master Dashboard placeholder, and route unique player names to the Player placeholder.

## Context

Stage 2 builds on the Stage 1 foundation:

- `HaveFun.Web` is the runnable Blazor Web App.
- Blazor uses `InteractiveServer` render mode.
- MudBlazor is the UI component library for web components.
- `Game:MasterName` is loaded and validated from `appsettings.json`.
- Sentences are loaded into memory through `HaveFun.Core`.
- Localhost and LAN join URLs are available through the existing URL provider.

## Scope

Included:

- Replace the Stage 1 first-page placeholder with a MudBlazor name entry form.
- Validate that name input is required after trimming.
- Compare the submitted name with `Game:MasterName` case-insensitively.
- Route the configured master name to a Master Dashboard placeholder page.
- Register non-master names as Players.
- Enforce unique player names case-insensitively.
- Reject player names that match the configured master name.
- Route successfully registered players to a Player placeholder page.
- Show clear validation errors for empty, duplicate, or reserved names.
- Keep player registration state in server memory.
- Keep the current browser tab's player id and display name in session storage after a successful player join.

Not included:

- Real authentication, passwords, or accounts.
- A database or persistent player list.
- Full Master Dashboard controls.
- Sentence selection.
- Starting rounds.
- Player waiting state connected to live round state.
- Word Scramble gameplay.
- Correctness, ranking, or results.

## Architecture

Use `HaveFun.Core` for reusable join and player-registration logic.

Recommended Core additions:

- `PlayerSession` model:
  - Player id.
  - Display name.
  - Joined timestamp.
- `JoinRole` enum:
  - `Master`.
  - `Player`.
- `JoinResult` model:
  - Success flag.
  - Role.
  - Player id when applicable.
  - Display name.
  - Validation error when rejected.
- `IPlayerRegistry` service contract:
  - Register a submitted name.
  - Check connected player names.
  - Return connected players for later dashboard stages.
- In-memory `PlayerRegistry` implementation.

Use `HaveFun.Web` for Blazor pages, navigation, and form components.

Recommended Web additions:

- Update `Pages/Home.razor` and `Pages/Home.razor.cs` for the join form.
- Add `Pages/Dashboard.razor` as a placeholder Master Dashboard page.
- Add `Pages/Player.razor` as a placeholder Player page.
- Add a small web service or JS interop wrapper for browser session storage.
- Register the in-memory player registry in `Program.cs`.

## Name Rules

Rules:

- Submitted names are trimmed before validation.
- Empty or whitespace-only names are rejected.
- Master name matching is case-insensitive.
- The configured master name always opens the dashboard.
- Players cannot use the configured master name.
- Player name uniqueness is case-insensitive.
- Duplicate player names show a validation error and stay on the join screen.

Recommended behavior:

- Preserve the user's typed value when validation fails.
- Display validation errors near the input.
- Disable or show loading state while a join submission is being processed.

## Routing

Recommended routes:

- `/` for the join screen.
- `/dashboard` for the Master Dashboard placeholder.
- `/player/{playerId}` for the Player placeholder.

Rules:

- The dashboard route should only open after the submitted name matches `Game:MasterName`.
- Normal player names should not navigate to the dashboard from the join flow.
- Player pages should be reached only after successful player registration.
- Player pages should restore the current player from session storage when the browser tab refreshes.
- If session storage is missing or does not match a registered in-memory player, the player route should fail clearly or return to the join screen.
- Stage 2 can use in-memory checks and simple route parameters; it does not need authentication.

## UI

Use MudBlazor components and styles for all visible web UI.

Join screen required UI:

- App name.
- LAN join URL, when available.
- Name input.
- Join button.
- Validation message area.

Dashboard placeholder required UI:

- Page title showing Master Dashboard.
- Shared LAN URL.
- Short message that dashboard controls arrive in Stage 3.

Player placeholder required UI:

- Player display name.
- Waiting message.
- Short message that live round state arrives in Stage 4.

Keep the navigation shell from Stage 1. The left menu toggle should remain available on all screen sizes.

## State Management

Use singleton in-memory services for server-side Stage 2 state.

Server state to store:

- Connected player ids.
- Connected player display names.
- Player joined timestamp.

Browser session storage state to store:

- Current player id for the browser tab.
- Current player display name for the browser tab.

Session storage rules:

- Write player session storage only after successful player registration.
- Do not store the configured master name in session storage.
- Use session storage, not local storage, so closing the tab clears the browser-side player identity.
- Treat server memory as the source of truth for uniqueness and connected players.

State not required yet:

- Connected Game Master session persistence.
- Round id.
- Selected sentence.
- Shuffled words.
- Submissions.
- Scores.

Restart behavior:

- Restarting the server clears all registered players.
- Existing browser session storage may remain until the tab is closed, but it is invalid if the player id is no longer present in server memory.

## Validation and Error Handling

The join flow should fail clearly when:

- The submitted name is empty after trimming.
- The submitted name is already used by a connected player.
- The submitted player name matches the configured master name.

The app should continue to fail at startup for invalid Stage 1 data:

- Missing or empty `Game:MasterName`.
- Missing or invalid sentence JSON.

## Done Criteria

Stage 2 is complete when:

- The first page shows a MudBlazor name input and join button.
- Empty names are rejected.
- The configured master name opens the Master Dashboard placeholder.
- Non-master names register as players and open the Player placeholder.
- Successful player joins store the player id and display name in browser session storage.
- Refreshing the player page in the same tab restores the current player from session storage.
- Duplicate player names are rejected case-insensitively.
- Player names matching the configured master name are rejected.
- Server join state remains in memory.
- No database, cloud service, accounts, passwords, auth system, rounds, gameplay, results, or ranking are added.

## Test Plan

Manual checks:

- Build from `src` with `dotnet build .\HaveFun.sln`.
- Run `HaveFun.Web`.
- Open `/`.
- Submit an empty or whitespace-only name and confirm validation appears.
- Submit the configured `Game:MasterName` and confirm `/dashboard` opens.
- Submit a normal player name and confirm `/player/{playerId}` opens.
- Refresh the player page and confirm the same player identity is restored from session storage.
- Open a second browser tab and submit the same player name with different casing; confirm it is rejected.
- Submit a player name with leading or trailing spaces; confirm validation and routing use the trimmed name.
- Confirm the LAN URL still appears where expected.
- Restart the server and confirm player registrations are cleared even if the browser tab still has session storage.

Failure checks:

- Confirm invalid `Game:MasterName` still fails at startup.
- Confirm invalid sentence data still fails at startup.

## Handoff to Stage 3

Stage 3 can build on:

- A working Master Dashboard route.
- Registered in-memory players.
- Browser tab session storage for the current player identity.
- A player registry service that can list connected players.
- The shared LAN URL display.
- Existing sentence library loaded from Stage 1.
