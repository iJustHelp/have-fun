# Task 05: Show Connected Players

## Goal

Show connected players on the Master Dashboard.

## Work

- Read players from the existing in-memory player registry.
- Show connected player display names.
- Show a clear empty state when no players have joined.
- Keep player result columns out of Stage 3.
- Do not implement disconnect detection unless it already exists.

## Done Criteria

- Dashboard shows connected players from server memory.
- Dashboard shows an empty state when there are no players.
- No submitted sentence, correctness, spent time, rank, or result grid behavior is implemented.

## Verification

- Build the solution from `src`.
- Join as master and confirm the empty player state.
- Join as player in another tab or device.
- Return to dashboard and confirm the player name appears.
