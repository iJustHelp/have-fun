# Task 05: Add Placeholder Pages

## Goal

Add the Stage 2 destination pages that later stages will expand.

## Work

- Add a MudBlazor Master Dashboard placeholder page at `/dashboard`.
- Show the shared LAN URL on the dashboard placeholder.
- Add a MudBlazor Player placeholder page at `/player/{playerId}`.
- Show the registered player display name on the player page.
- Read the current player id and display name from browser session storage when the player page loads.
- Verify the session storage player id still exists in the in-memory player registry.
- Show waiting or next-stage copy without implementing round state.
- Keep the existing layout and left navigation toggle working.

## Done Criteria

- `/dashboard` renders a Master Dashboard placeholder.
- `/player/{playerId}` renders a Player placeholder for a registered player.
- Refreshing `/player/{playerId}` in the same browser tab keeps the player visible.
- Unknown or unregistered player ids fail clearly or return to the join screen.
- No sentence selector, round start, gameplay, results, or ranking is implemented.

## Verification

- Build the solution from `src`.
- Navigate as master and confirm the dashboard placeholder renders.
- Navigate as player and confirm the player placeholder renders with the display name.
- Refresh the player page and confirm the display name still renders from session storage plus server registry validation.
