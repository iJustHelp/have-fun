# Task 03: Return Ranked Results

## Goal

Expose ranked current-round results through `IGameStateService`.

## Work

- Extend `IGameStateService` with a method to get ranked results for the current round.
- Include submitted players only.
- Sort by highest correctness first.
- Break ties by shortest spent time.
- Use player name as a stable final tie-breaker if needed.
- Clear results automatically when a new round starts.

## Done Criteria

- Server returns ranked results for the current round.
- Unsubmitted players are not ranked.
- New round has no stale previous results.
- No persistent leaderboard is added.

## Verification

- Build the solution from `src`.
- Submit multiple player results and confirm ranking order in service output.
