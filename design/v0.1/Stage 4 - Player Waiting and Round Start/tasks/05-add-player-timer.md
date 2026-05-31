# Task 05: Add Player Timer

## Goal

Show a player-visible countdown timer for the active round.

## Work

- Calculate remaining time from current round start time and time limit.
- Show remaining time on the player page after a round starts.
- Update the timer while the round is active.
- Clamp remaining time at zero.
- Show zero or an expired state after time runs out.
- Dispose timer resources when the player page is disposed.

## Done Criteria

- Player page shows a countdown timer after round start.
- Timer decreases while the round is active.
- Timer reaches zero when the time limit expires.
- Timer does not require a database, polling endpoint, or external service.

## Verification

- Build the solution from `src`.
- Start a round and confirm the timer appears.
- Wait and confirm the timer decreases.
- Confirm the timer reaches zero or expired state.
