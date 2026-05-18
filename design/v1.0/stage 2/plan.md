# Stage 2 Player Game Routing Plan

## Summary

Update Stage 2 so registered players no longer use the current `/player` gameplay page. After registration, players go to a bare waiting room page. The Host gets a simple game menu with Word Scramble only. When the selected game is active, players should see that game experience using an empty/no-nav layout.

## Key Changes

- Replace the current Player page concept with a player waiting/game entry route.
- After successful registration, route players to the waiting room page.
- Use a no-nav/no-app-bar layout for the player waiting/game page, similar to `RegisterLayout`.
- Add a Host game menu/selector with only Word Scramble enabled for now.
- Keep existing Word Scramble gameplay/state services reusable, but move player-facing gameplay out of the old generic Player page shape.
- Remove the Player link from the normal nav menu for player sessions, since player screens should use the empty layout.

## Player Flow

- Player opens `/register`, enters name, and joins.
- App saves `Role.Player` session.
- App navigates to the waiting room page.
- Waiting room shows minimal “waiting for host” content until Host selects/starts Word Scramble.
- Once Word Scramble is selected/active, the same no-nav page renders the proper Word Scramble player experience.

## Test Plan

- Verify `/register` still registers a unique player.
- Verify registered players land on the waiting room page, not the old full-layout `/player` experience.
- Verify player waiting/game page has no nav/app bar.
- Verify Host dashboard/menu shows Word Scramble only.
- Verify existing Word Scramble round start and player gameplay still work after routing changes.

## Assumptions

- “Remove Player page” means remove the current generic full-layout Player experience, not delete all player-facing game screens.
- “Empty layout” means no app bar and no nav menu.
- Stage 2 only needs Word Scramble in the Host game menu.
