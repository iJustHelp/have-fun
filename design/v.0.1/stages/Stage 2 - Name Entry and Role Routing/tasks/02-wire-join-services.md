# Task 02: Wire Join Services

## Goal

Register Stage 2 join services in the Blazor web app.

## Work

- Register the in-memory player registry in `HaveFun.Web`.
- Register a small browser session storage service or JS interop wrapper in `HaveFun.Web`.
- Keep the registry lifetime compatible with server-wide in-memory game state.
- Ensure `Game:MasterName` remains loaded through existing typed options.
- Keep existing sentence loading and LAN URL services intact.

## Done Criteria

- `HaveFun.Web` can resolve the player registry from dependency injection.
- `HaveFun.Web` can read and write the current player id and display name from browser session storage.
- Existing Stage 1 startup validation still runs.
- No new database, auth, or persistence service is introduced.

## Verification

- Build the solution from `src`.
- Run the web app and confirm startup still succeeds with valid settings and sentence data.
