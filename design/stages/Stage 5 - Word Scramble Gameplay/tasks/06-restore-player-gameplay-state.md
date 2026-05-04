# Task 06: Restore Player Gameplay State

## Goal

Restore in-progress player gameplay state after a page refresh.

## Work

- Load the current player's round state from `IGameStateService` when `/player` opens.
- Restore available words.
- Restore collected words.
- Restore submitted state when applicable.
- Keep player identity based on session storage `name` and `role`.
- Handle missing server state clearly after a server restart.

## Done Criteria

- Refreshing mid-round keeps the player's in-progress state while the server is still running.
- Refreshing after submission keeps the submitted state.
- Restarted server still clears all in-memory gameplay state.

## Verification

- Build the solution from `src`.
- Select some words, refresh, and confirm state is restored.
- Submit, refresh, and confirm submitted state is restored.
