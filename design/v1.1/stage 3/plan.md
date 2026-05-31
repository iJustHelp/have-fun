# v1.1 Stage 3 Plan: Decouple PlayerGameBoard From PlayerRoundState

## Summary

Refactor `PlayerGameBoard` so it no longer accepts `PlayerRoundState` directly. The component will receive explicit tile/display state from the game page while keeping existing tile click callbacks, because tile button rendering currently lives inside the shared board and must remain interactive.

## Key Changes

- Update `PlayerGameBoard` parameters:
  - Remove `PlayerRoundState`.
  - Add `IReadOnlyList<Tile> AvailableTiles`.
  - Add `IReadOnlyList<Tile> SelectedTiles`.
  - Add `bool IsSubmitted`.
  - Add `string? SubmittedText`.
  - Add `TimeSpan? SpentTime`.
  - Keep `Title`, `Rules`, `RemainingTimeText`, `CanSubmit`.
  - Keep `Action<Guid>? OnSelectItem`, `Action<Guid>? OnReturnItem` as private , and `Func<Task>? OnSubmit` as public for page subscription.

- Update `PlayerGameBoard` markup:
  - Render `SelectedTiles` instead of `PlayerRoundState.SelectedTiles`.
  - Render `AvailableTiles` instead of `PlayerRoundState.AvailableTiles`.
  - Render submitted alert from `IsSubmitted`, `SubmittedText`, and `SpentTime`.
  - Keep existing visual layout, timer, empty-state text, tile buttons, and submit button behavior.

- Update `/player-sentence-scrambler` page usage:
  - Pass `PlayerRoundState?.AvailableTiles ?? []`.
  - Pass `PlayerRoundState?.SelectedTiles ?? []`.
  - Pass `PlayerRoundState?.IsSubmitted == true`.
  - Pass `PlayerRoundState?.SubmittedSentence`.
  - Pass `PlayerRoundState?.SpentTime`.
  - Keep existing page-owned session checks, redirects, timer, service calls, select/return methods, and submit method.

## Test Plan

- Run `dotnet build .\HaveFun.sln --no-restore` from `src`.
- Verify `/player-sentence-scrambler` still shows the same title, rules, timer, selected sentence area, available words, submitted alert, and submit button.
- Verify selecting an available tile still moves it into the selected area.
- Verify clicking a selected tile still returns it to available words.
- Verify submit still records the selected sentence and shows submitted time.
- Verify unregistered/player/host redirects remain unchanged.

## Assumptions

- Although the BRD says `PlayerGameBoard` should have only `OnSubmit`, the chosen implementation keeps select/return callbacks so the shared board can remain the owner of tile button rendering.
- This stage only removes the direct `PlayerRoundState` dependency from `PlayerGameBoard`; it does not change `PlayerRoundState`, game services, routes, scoring, or host behavior.
