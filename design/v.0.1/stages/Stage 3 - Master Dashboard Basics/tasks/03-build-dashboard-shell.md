# Task 03: Build Dashboard Shell

## Goal

Replace the dashboard placeholder with the first usable master-only dashboard shell.

## Work

- Update `Dashboard.razor` and `Dashboard.razor.cs`.
- Keep dashboard rendering based on browser session storage role.
- Show dashboard content only when session role is `Master`.
- Show a clear message and join action when session storage is missing or role is not `Master`.
- Show the shared LAN URL.
- Show round status.
- Use MudBlazor components and styles.

## Done Criteria

- `/dashboard` renders dashboard content for the master role.
- `/dashboard` does not render dashboard content for player or missing role.
- Shared LAN URL is visible.
- Round status is visible.

## Verification

- Build the solution from `src`.
- Join as master and confirm dashboard content renders.
- Join as player and confirm dashboard content is blocked.
- Open `/dashboard` without session storage and confirm dashboard content is blocked.
