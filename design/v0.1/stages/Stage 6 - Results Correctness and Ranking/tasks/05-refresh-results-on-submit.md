# Task 05: Refresh Results on Submit

## Goal

Keep the Master Dashboard results current as players submit.

## Work

- Use existing player submission notifications or add a focused result notification if needed.
- Refresh dashboard result state when a player submits.
- Refresh dashboard result state when a new round starts.
- Keep result updates in memory and process-local.
- Do not add manual SignalR hubs, polling endpoints, databases, or message brokers.

## Done Criteria

- Dashboard updates results when players submit.
- Dashboard clears results when a new round starts.
- Existing submission progress remains compatible with result updates.

## Verification

- Build the solution from `src`.
- Keep dashboard open, submit as a player, and confirm results appear without manual refresh.
