# Task 04: Build Player Formula Scrambler

## Goal

Add the player page for solving Formula Scrambler rounds.

## Work

- Add a player page at `/player-formula-scrambler`.
- Use `RegisterLayout`.
- Inject `FormulaScramblerGameStateService`.
- Reuse the same player session, registration, player-removal, timer, and submit patterns as the other player game pages.
- Render `PlayerGameBoard` with Formula Scrambler title/rules and empty tile separator.
- Submit the selected character tiles to Formula Scrambler game state.
- Redirect unregistered users to `/register`.
- Redirect players to `/waiting-room` when no Formula Scrambler round is active.

## Done Criteria

- Registered players can solve an active Formula Scrambler round.
- Submitted players cannot edit or resubmit for the same round.
- Refreshing during an active round restores the player's current round state.
- Host users who open the player page are redirected to `/host-formula-scrambler`.

## Verification

- Run `dotnet build .\HaveFun.sln --no-restore` from `src`.
- Register as a player, start a Formula Scrambler round, select character tiles, and submit.
- Confirm the player board shows submitted state and submit time after submission.
