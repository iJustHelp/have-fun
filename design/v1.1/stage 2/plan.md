# v1.1 Stage 2 Plan: Tile-Based Game Service Refactor

## Summary

Refactor the current Sentence Scrambler game flow so tile selection is explicitly game-service owned and reusable for future tile-based games. Stage 2 does not add Word Scrambler or any other new game. `PlayerGameBoard` remains based on `PlayerRoundState` and continues to render available/selected tiles through page-provided state and callbacks.

## Key Changes

- Keep `PlayerGameBoard` as the shared tile-selection UI:
  - It continues to receive `PlayerRoundState`.
  - It renders `AvailableTiles` and `SelectedTiles`.
  - It raises `OnSelectItem`, `OnReturnItem`, and `OnSubmit` callbacks.
- Keep tile selection state changes in services, not pages:
  - Pages call service methods for select, return, and submit.
  - Services update `PlayerRoundState`.
  - Pages refresh state from services after actions.
- Introduce a game-specific tile service abstraction in `HaveFun.Core`:
  - `ITileCollectionService` defines tile creation/collection behavior for a game.
  - Sentence Scrambler gets the first implementation.
  - The implementation converts shuffled sentence words into `Tile` values.
- Update `GameStateService` to depend on the tile service instead of directly constructing tiles from sentence words.
- Keep existing routes and gameplay behavior unchanged:
  - `/player-sentence-scrambler`
  - `/host-sentence-scrambler`
  - waiting-room routing
  - scoring, timing, submit behavior, and total scores.

## Interfaces and Types

- Add `ITileCollectionService` in `HaveFun.Core` service contracts.
- Minimum service behavior:
  - Accept the active `CurrentRound` or its shuffled sentence parts.
  - Return the initial available `Tile` collection for a player round.
- Add `SentenceScramblerTileCollectionService` as the concrete implementation.
- Register the tile service in `AddCoreServices`.
- Do not change `PlayerRoundState` shape in this stage.
- Do not make `PlayerGameBoard` independent of `PlayerRoundState`.

## Out of Scope

- Do not add Word Scrambler in Stage 2.
- Do not add a game selector.
- Do not add new routes for other games.
- Do not move tile selection state into pages.
- Do not change scoring rules.
- Do not add persistence, database tables, authentication, or cloud services.

## Test Plan

- Run `dotnet build .\HaveFun.sln` from `src`.
- Verify Sentence Scrambler still starts a round and creates available tiles.
- Verify selecting a tile moves it from available to selected.
- Verify returning a tile moves it from selected to available.
- Verify submit still records the selected tile sentence.
- Verify incomplete-submit confirmation still works.
- Verify Host results grid still updates after player submit.
- Verify no new game UI or route appears.

## Assumptions

- “Word Scrambler” is only an example of a future tile-based game.
- `Tile.Text` is sufficient for current Stage 2 needs.
- Stage 2 is an internal service refactor plus behavior preservation stage.
- Future stages can add game-specific pages and result grids on top of this service boundary.
