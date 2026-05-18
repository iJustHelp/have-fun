# Create Stage 3 Plan

## Summary

Create `design/v1.0/stage 3/plan.md` from the Stage 3 BRD. Stage 3 moves player management to Home, makes Waiting Room a simple waiting-only page, and moves Word Scramble gameplay into its own player page.

## Key Changes

- Add a registered players grid to Home showing all current players.
- Add a remove action per player in the Home grid.
- Extend player registry behavior so Host can remove a player from memory.
- When a removed player still has an open waiting/game browser page, redirect that browser to `/register`.
- Keep Waiting Room as a no-nav page with only player greeting and waiting messaging.
- Create a dedicated Word Scramble player page using the current game UI/functionality from `WaitingRoom`.
- Route players from Waiting Room to Word Scramble when a round is active, while preserving the empty/no-nav layout for player game screens.

## Interfaces and Behavior

- Add a remove method to `IPlayerRegistryService`, such as removing by player id.
- Add a player registry change notification so Waiting Room and Word Scramble pages can react when a player is removed.
- Home refreshes its player grid after player removal.
- Waiting Room and Word Scramble validate the current player session against the registry on load and on registry changes.
- If validation fails, clear or ignore the stale local player session as needed and navigate to `/register`.

## Test Plan

- Verify Home shows all registered players.
- Verify Host can remove a player from Home.
- Verify removed players disappear from the Home grid.
- Verify a removed player with an open Waiting Room page is redirected to `/register`.
- Verify a removed player with an open Word Scramble page is redirected to `/register`.
- Verify Waiting Room contains only greeting and waiting messaging.
- Verify Word Scramble gameplay still works after moving from Waiting Room.
- Verify no database, authentication, or persistence is introduced.

## Assumptions

- `design/v1.0/stage 3/plan.md` is the implementation source for Stage 3.
- Player removal only affects in-memory registry state.
- Redirect-on-remove should be event-based, not polling.
- Existing Word Scramble gameplay behavior should be preserved while moving it to a dedicated page.
