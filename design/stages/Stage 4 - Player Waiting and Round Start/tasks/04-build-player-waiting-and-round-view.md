# Task 04: Build Player Waiting and Round View

## Goal

Update the player page to show waiting state before a round and active round details after start.

## Work

- Keep validating session storage `name` and `role`.
- Keep validating that the player name is registered in server memory.
- Show player display name.
- Show waiting state before any round starts.
- Show current round status after a round starts.
- Show current round time limit.
- Show shuffled words as visible MudBlazor buttons or chips.
- Do not make words selectable yet.

## Done Criteria

- `/player` blocks missing, master, or invalid player sessions.
- Registered players see a waiting state before a round starts.
- Registered players see round status, time limit, and shuffled words after a round starts.
- Shuffled words are display-only in Stage 4.

## Verification

- Build the solution from `src`.
- Join as player before a round starts and confirm waiting state.
- Start a round as master and confirm the player page shows shuffled words.
