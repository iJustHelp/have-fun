# Task 02: Extend Game State Service

## Goal

Expose gameplay operations through `IGameStateService`.

## Work

- Add methods to get or create a player's round state.
- Add a method to select a word for a player.
- Add a method to submit a player result.
- Add a method to read player submissions or submitted state.
- Reject or ignore duplicate submissions for the same player and round.
- Keep all state in memory.

## Done Criteria

- `IGameStateService` supports player word selection.
- `IGameStateService` supports player submission.
- Player state is keyed by player name and current round id.
- No correctness or ranking behavior is added.

## Verification

- Build the solution from `src`.
- Confirm word selection and submission operations compile and are available to the web app.
