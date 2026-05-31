# Task 01: Create Round State

## Goal

Add the reusable in-memory round state model for Stage 3.

## Work

- Add a `RoundStatus` enum in `HaveFun.Core`.
- Add a `CurrentRound` model in `HaveFun.Core`.
- Add an `IGameState` service contract.
- Add an in-memory `GameState` implementation.
- Store current round id, selected sentence text, time limit, status, and started timestamp.
- Keep round state in server memory only.

## Done Criteria

- `HaveFun.Core` exposes reusable round state types.
- The current round can be read from a service.
- A selected sentence can be stored as the current started round.
- No shuffled words, submissions, scores, ranking, database, or persistence is added.

## Verification

- Build the solution from `src`.
- Confirm `HaveFun.Core` builds without web-specific dependencies.
