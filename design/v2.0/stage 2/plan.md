# Plan: Move Tile Draft Selection Into PlayerGameBoard

## Summary

Refactor tile selection so `PlayerGameBoard` owns the player's local draft tile movement, and `GameStateService` only receives submitted selected tiles. `SelectedTiles` becomes the submitted answer source of truth; do not keep a separate `SubmittedSentence` value.

## Key Changes

- Update `IGameStateService`:
  - Remove `SelectTile(string playerName, Guid tileId)`.
  - Remove `ReturnTile(string playerName, Guid tileId)`.
  - Replace `SubmitPlayerRound(string playerName)` with `SubmitPlayerRound(string playerName, IReadOnlyList<Tile> selectedTiles)`.

- Update submitted answer models:
  - Remove `SubmittedSentence` from `PlayerRoundState`.
  - Remove `SubmittedSentence` from `PlayerResult`.
  - Use `PlayerRoundState.SelectedTiles` and `PlayerResult.SelectedTiles` as the submitted answer.
  - Keep derived display text local to UI/helper methods, for example joining selected tile text when rendering an alert or calculating score.

- Update `GameStateService`:
  - Delete service-owned select/return tile mutation.
  - Keep `GetOrCreatePlayerRoundState` creating the player's initial available tiles for the active round.
  - In `SubmitPlayerRound`, validate submitted tiles against that player's original available tiles by tile ID, preserve submitted order, ignore invalid/stale tile IDs, and reject empty valid submissions.
  - Store final submitted state with `SelectedTiles`, remaining `AvailableTiles`, `SubmittedAt`, `SpentTime`, and `IsSubmitted`.
  - Update score calculation to use selected tiles, not submitted text. The game-specific score delegate should accept `CurrentRound` and `IReadOnlyList<Tile> selectedTiles`.
  - Raise `PlayerRoundStateChanged` only on submit.

- Update `PlayerGameBoard`:
  - Maintain private draft state fields, for example `_availableTiles` and `_selectedTiles`.
  - Remove `OnSelectItem` and `OnReturnItem` parameters.
  - Change `OnSubmit` to `Func<IReadOnlyList<Tile>, Task>?`.
  - Move clicked tiles locally between `_availableTiles` and `_selectedTiles`.
  - Render submitted text by joining submitted `SelectedTiles`.
  - Disable submit when the page says submit is unavailable, the board is submitted, or `_selectedTiles.Count == 0`.
  - Reset local draft tiles when a new round/source tile set arrives; add a `Guid? RoundId` parameter so reset behavior is deterministic.

- Update player and host pages:
  - Player pages pass `RoundId="@CurrentRound?.Id"` to `PlayerGameBoard`.
  - Player pages stop passing select/return callbacks.
  - Player page `CanSubmit` means "round is active and not submitted," not `PlayerRoundState.CanSubmit`.
  - Player page submit handlers accept `IReadOnlyList<Tile> selectedTiles` and call `SubmitPlayerRound(PlayerName, selectedTiles)`.
  - Host pages build current-round result rows from submitted `SelectedTiles`, not `SubmittedSentence`.
  - Sentence Scrambler scoring joins selected tile text as words; Spelling Bee scoring joins selected tile text as letters.

- Update architecture documentation:
  - Show tile selection as local to `PlayerGameBoard`.
  - Show only `SubmitPlayerRound(player, selectedTiles)` going to `GameStateService`.
  - Note that `PlayerRoundStateChanged` is raised for submitted state, not every draft tile move.
  - Document that `SelectedTiles` is the submitted answer and display text is derived.

## Test Plan

- Run `dotnet build .\HaveFun.sln --no-restore` from `src`.
- Verify Sentence Scrambler player can select tiles, return tiles, and submit.
- Verify Spelling Bee player can select letter tiles, return letters, and submit.
- Verify submit stays disabled until at least one tile is selected.
- Verify submitted alert shows text derived from selected tiles and spent time.
- Verify host result grids update after submit, not during draft tile movement.
- Verify all-player submission completion still completes the active round.
- Verify Sentence Scrambler and Spelling Bee host flows still start rounds and create correct initial tiles.

## Assumptions

- Draft tile movement is private to each player browser and does not need host visibility.
- `PlayerRoundState.AvailableTiles` remains the source for initial board tiles, but `SelectedTiles` is only authoritative after submit.
- `SubmittedSentence` is removed rather than kept as derived duplicate state.
- No new entities, database tables, routes, or persistence are added.
