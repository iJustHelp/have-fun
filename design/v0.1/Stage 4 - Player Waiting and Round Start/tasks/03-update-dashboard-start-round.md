# Task 03: Update Dashboard Start Round

## Goal

Make the Master Dashboard start action create the full Stage 4 round state.

## Work

- Keep the existing sentence selector and Start Round button.
- Ensure Start Round creates shuffled words through `IGameStateService`.
- Refresh dashboard round status after start.
- Keep connected player list visible.
- Keep result grid, submissions, correctness, and ranking out of scope.

## Done Criteria

- Starting a round creates full Stage 4 current round state.
- Dashboard shows started status after Start Round.
- Starting another round replaces the current round state.
- No player gameplay or results behavior is added.

## Verification

- Build the solution from `src`.
- Join as master, select a sentence, start a round, and confirm dashboard status updates.
