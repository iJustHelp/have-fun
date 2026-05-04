# Task 01: Add Player Round State

## Goal

Add server-side player gameplay state for the current round.

## Work

- Add a `PlayerRoundState` model in `HaveFun.Core`.
- Track player name, round id, available words, collected words, submission status, submitted sentence, submitted timestamp, and spent time.
- Use stable word instances or indexes so duplicate words are handled correctly.
- Keep player gameplay state in server memory only.
- Clear player gameplay state when a new round starts.

## Done Criteria

- Each player can have separate state for the current round.
- Duplicate words can be represented as separate selectable items.
- Starting a new round clears previous player gameplay state.
- No correctness, ranking, database, or persistence is added.

## Verification

- Build the solution from `src`.
- Confirm a player state can be created for the current round.
