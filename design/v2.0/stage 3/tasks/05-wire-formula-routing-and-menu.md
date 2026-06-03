# Task 05: Wire Formula Routing and Menu

## Goal

Connect Formula Scrambler to host navigation and player waiting-room routing.

## Work

- Add Formula Scrambler to the host nav menu.
- Extend the waiting room to inject `FormulaScramblerGameStateService`.
- Subscribe and unsubscribe to Formula Scrambler round changes in the waiting room.
- Route waiting players to `/player-formula-scrambler` when a Formula Scrambler round starts.
- Include Formula Scrambler in active-game detection when a player opens the waiting room.
- Update existing player game pages so players redirect to `/player-formula-scrambler` if a Formula Scrambler round starts while they are on another player game page.

## Done Criteria

- Host users can navigate to Formula Scrambler from the menu.
- Registered waiting players route to the correct player page when Formula Scrambler starts.
- Formula Scrambler does not overwrite Sentence Scrambler or Word Scrambler state.
- Starting any one game routes players to that game's page.

## Verification

- Run `dotnet build .\HaveFun.sln --no-restore` from `src`.
- Register a player, leave them in the waiting room, and start a Formula Scrambler round.
- Confirm the player is routed to `/player-formula-scrambler`.
- Confirm Sentence Scrambler and Word Scrambler routing still work.
