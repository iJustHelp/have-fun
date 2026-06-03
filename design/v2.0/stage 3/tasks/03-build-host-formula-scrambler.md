# Task 03: Build Host Formula Scrambler

## Goal

Add the host page for running Formula Scrambler rounds.

## Work

- Add a host page at `/host-formula-scrambler`.
- Use the existing Word Scrambler host flow as the behavior pattern.
- Inject `FormulaScramblerFileService` and `FormulaScramblerGameStateService`.
- Let the host select a formula file, set timeout, start each formula, stop an active formula, and advance through the selected file.
- Start rounds using character tiles from the selected formula.
- Show the correct formula after a round completes.
- Show players, submit time, submitted formula characters, round score, and total score.
- Highlight submitted characters that match the source formula at the same position.

## Done Criteria

- Host-only session checks match the other host game pages.
- Starting a Formula Scrambler round notifies players through the game state service.
- Stopping or timing out a round completes it and shows results.
- Results use Formula Scrambler scoring and show submitted formulas without spaces between tiles.

## Verification

- Run `dotnet build .\HaveFun.sln --no-restore` from `src`.
- Open `/host-formula-scrambler` as host.
- Select a formula file, start a round, stop it, and start the next formula.
- Confirm submitted formulas and scores appear in the host results table.
