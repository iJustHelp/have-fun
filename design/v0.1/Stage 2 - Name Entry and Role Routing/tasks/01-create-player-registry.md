# Task 01: Create Player Registry

## Goal

Add the reusable in-memory join and player-registration model for Stage 2.

## Work

- Add Core models for player sessions and join results.
- Add a Core role enum for `Master` and `Player`.
- Add an `IPlayerRegistry` service contract.
- Add an in-memory `PlayerRegistry` implementation.
- Store player ids, display names, and joined timestamps.
- Return enough data from registration to let the web app write the current player identity to session storage.
- Compare player names case-insensitively.
- Keep all state in memory only.

## Done Criteria

- `HaveFun.Core` exposes reusable join/player registration types.
- Player names can be registered and listed.
- Registered players have stable ids for browser session storage.
- Duplicate player names are rejected case-insensitively.
- The registry does not use a database, file storage, or cloud service.

## Verification

- Build the solution from `src`.
- Confirm `HaveFun.Core` builds without web-specific dependencies.
