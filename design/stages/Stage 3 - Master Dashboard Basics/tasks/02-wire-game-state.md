# Task 02: Wire Game State

## Goal

Register the Stage 3 game state service in the web app.

## Work

- Register the in-memory game state service in `HaveFun.Web`.
- Keep the game state service lifetime compatible with server-wide in-memory state.
- Keep `ISentenceLibrary`, `IPlayerRegistry`, `IJoinUrlProvider`, and `IUserSessionStorage` registrations intact.
- Confirm browser session storage still stores only `name` and `role`.

## Done Criteria

- `HaveFun.Web` can resolve the game state service from dependency injection.
- Existing Stage 1 and Stage 2 services still resolve.
- No database, cloud service, account system, password, or real auth service is introduced.

## Verification

- Build the solution from `src`.
- Run the web app and confirm startup succeeds with valid settings and sentence data.
