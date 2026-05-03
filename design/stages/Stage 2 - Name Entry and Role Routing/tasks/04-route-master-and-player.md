# Task 04: Route Master and Player

## Goal

Route submitted names to the correct Stage 2 destination.

## Work

- Compare the trimmed submitted name with `Game:MasterName` case-insensitively.
- Navigate the configured master name to `/dashboard`.
- Register normal names as players.
- Store the registered player id and display name in browser session storage.
- Navigate registered players to `/player/{playerId}`.
- Reject player names that match the configured master name.
- Reject duplicate player names case-insensitively.

## Done Criteria

- The configured master name opens the dashboard placeholder.
- A normal unique player name opens the player placeholder.
- A successful player route can be restored from browser session storage after refresh.
- Duplicate player names are rejected.
- Player names cannot use the configured master name.

## Verification

- Build the solution from `src`.
- Submit the configured master name and confirm `/dashboard` opens.
- Submit a unique player name and confirm `/player/{playerId}` opens.
- Refresh the player page and confirm the player identity is restored from session storage.
- Submit the same player name with different casing in another tab and confirm it is rejected.
