# Stage 4 Plan: Player Waiting and Round Start

## Objective

Connect player screens to the current in-memory round state. Players should see a waiting state before the Game Master starts a round, and then see the started round with the same shuffled word set when the Game Master starts it.

## Context

Stage 4 builds on the current app foundation:

- `HaveFun.Web` is a Blazor Web App with `InteractiveServer` render mode.
- MudBlazor is used for visible web UI.
- Browser session storage stores only the current user's `name` and `role`.
- Player names are registered in the in-memory player registry.
- The Master Dashboard can select a sentence and start a round.
- `IGameStateService` stores current round basics in server memory.
- Services and interfaces use the `Service` suffix.

## Scope

Included:

- Extend round state so a started round includes shuffled words.
- Clear previous round submission placeholders when starting a new round.
- Show a player waiting state before a round starts.
- Show current round status on the player screen.
- Show the current round shuffled words on the player screen after start.
- Show a player-visible countdown timer after a round starts.
- Ensure all joined players see the same shuffled word order for the current round.
- Refresh player screens when round state changes, using Blazor server-side state notifications or another local in-memory mechanism.
- Keep player identity based on session storage `name` and `role`.

Not included:

- Clicking words to build the collected sentence.
- Moving words between available and collected areas.
- Player submission.
- Spent time recording.
- Correctness calculation.
- Ranking.
- Result grid.
- Persistence, database, cloud service, accounts, passwords, or real authentication.

## Architecture

Use `HaveFun.Core` for reusable round state and word preparation.

Recommended Core updates:

- Extend `CurrentRound` with:
  - Shuffled words.
  - Original words if helpful for later stages.
- Add a small sentence word-splitting/shuffling helper or service.
- Extend `IGameStateService` with:
  - Current round read access.
  - Start round with shuffled words.
  - Round-changed notification.
- Keep the game state implementation in memory.

Use `HaveFun.Web` for player UI updates and role/session rendering.

Recommended Web updates:

- Update `Pages/Player.razor` and `Pages/Player.razor.cs`.
- Subscribe the player page to round-changed notifications.
- Unsubscribe when the player page is disposed.
- Update dashboard start behavior if needed to create shuffled words.

## Round State Rules

Rules:

- Starting a round creates a new round id.
- Starting a round stores selected sentence text and time limit.
- Starting a round stores a start time that can be used to calculate remaining time.
- Starting a round splits the selected sentence into words.
- Starting a round stores one shuffled word order for the round.
- Every player sees the same shuffled word order for that round.
- Starting a new round replaces the current round.
- Starting a new round clears previous submission-related state if any placeholder state exists.
- Server memory remains the source of truth.

Shuffling guidance:

- Shuffle on the server.
- Keep the shuffled list stable for the round.
- Do not reshuffle per player in Stage 4.
- A simple unbiased Fisher-Yates shuffle is enough for V1.

## Player Screen

Required UI:

- Player display name.
- Round status.
- Waiting state before any round starts.
- Shuffled word buttons or chips after a round starts.
- Current round time limit.
- Remaining countdown timer after a round starts.

Behavior:

- If session storage is missing or role is not `Player`, show a clear message and a way back to registration.
- If the session player name is no longer registered in server memory, show a clear message and a way back to registration.
- Before a round starts, show a waiting state.
- After a round starts, show the current round status, countdown timer, and shuffled words.
- Timer should count down from the current round time limit using the server round start time.
- Timer should show zero or an expired state after time runs out.
- Shuffled words are visible but not interactive beyond basic display in Stage 4.

## Live Updates

Stage 4 should make player screens update when the Game Master starts a round.

Acceptable implementation:

- Add an event or notification callback on `IGameStateService`.
- Player page subscribes during initialization after role/session validation.
- Player page calls `InvokeAsync(StateHasChanged)` when round state changes.
- Player page updates its timer while a round is active.
- Player page unsubscribes during disposal.

Rules:

- Do not add SignalR hubs manually unless needed; Blazor InteractiveServer already has a server-side circuit.
- Do not add polling unless notification-based updates become impractical.
- Do not add database or message broker dependencies.

## Dashboard Impact

Dashboard should keep the Stage 3 controls.

Additional Stage 4 dashboard expectations:

- Starting a round creates shuffled words in server state.
- Round status continues to update after start.
- Connected player list remains visible.
- Result grid and submissions remain out of scope.

## Done Criteria

Stage 4 is complete when:

- Player screen validates session storage `name` and `role`.
- Player screen shows waiting state before a round starts.
- Starting a round stores shuffled words in server memory.
- Starting a round updates all currently open player screens.
- Every player sees the same shuffled words for the current round.
- Player screen shows round status, time limit, and remaining timer after start.
- Player timer counts down and reaches zero after the round time limit.
- No word-click gameplay, submissions, correctness, ranking, database, cloud service, accounts, passwords, or real auth system is added.

## Test Plan

Manual checks:

- Build from `src` with `dotnet build .\HaveFun.sln`.
- Run `HaveFun.Web`.
- Join as a player in one browser tab and confirm waiting state.
- Join as another player in a second browser tab and confirm waiting state.
- Join as master and open the dashboard.
- Select a sentence and start a round.
- Confirm both player tabs update without manual refresh.
- Confirm both player tabs show the same shuffled word order.
- Confirm both player tabs show a countdown timer.
- Confirm the timer decreases while the round is active.
- Confirm the dashboard still shows round status.
- Start another round and confirm players update to the new shuffled words.

Failure checks:

- Open `/player` without session storage and confirm player content is blocked.
- Open `/player` with master role and confirm player content is blocked.
- Restart the server and confirm old player session storage does not restore invalid server state.
- Confirm the app still starts without any database or cloud dependency.

## Handoff to Stage 5

Stage 5 can build on:

- Current round id.
- Stable shuffled words for the round.
- Player-visible timer based on round start time and time limit.
- Player screens that update when a round starts.
- Player waiting and active-round states.
- Server-side game state notification mechanism.
