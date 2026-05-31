# v1.1 Stage 1 Plan: Reusable Player Game Component

## Summary

Extract the visual Sentence Scrambler player gameplay UI from `/player-sentence-scrambler` into a reusable Blazor component while keeping the route, session checks, navigation, timers, service calls, scoring, and gameplay behavior unchanged.

## Key Changes

- Create a reusable component under `src/HaveFun.Web/Components`, using the root `HaveFun.Web` namespace.
- Remove the current `Components\**` exclusion from `HaveFun.Web.csproj` so Blazor components in that folder compile.
- Move only the rendered gameplay section into the new component:
  - game title/rules header
  - timer chip
  - collected sentence area
  - submitted-state alert
  - available words area
  - submit button
- Keep `/player-sentence-scrambler` as the route and keep its page code-behind responsible for:
  - session and role validation
  - redirecting host/player/register/waiting-room flows
  - subscribing/unsubscribing to player and round events
  - timer lifecycle
  - calling `IGameStateService`
  - incomplete-submission confirmation dialog

## Component API

- Name the component `PlayerGameBoard`.
- Add parameters:
  - `string Title`
  - `string Rules`
  - `PlayerRoundState? PlayerRoundState`
  - `string RemainingTimeText`
  - `bool CanSubmit`
  - `EventCallback<Guid> OnSelectWord`
  - `EventCallback<Guid> OnReturnWord`
  - `EventCallback OnSubmit`
- Keep formatting helpers that are purely visual, such as spent-time display, inside the component.
- Do not inject game services into the component; page-owned behavior is passed through parameters and callbacks.

## Page Updates

- Replace the duplicated gameplay markup in `PlayerSentenceScrambler.razor` with `PlayerGameBoard`.
- Pass:
  - `Title="Sentence Scrambler Game"`
  - `Rules="Rule: rearrange the available words to make a correct sentence."`
  - existing `PlayerRoundState`, `RemainingTimeText`, `CanSubmit`, and callback methods.
- Keep existing loading/error/register markup in the page.
- Keep existing `@page`, `@layout RegisterLayout`, and `PageTitle`.

## Test Plan

- Run `dotnet build .\HaveFun.sln` from `src`.
- Manually verify a player opening `/player-sentence-scrambler` sees the same title, rules, timer, word buttons, sentence area, and submit button.
- Verify selecting words, returning words, submitting, incomplete-submit confirmation, and submitted alert still work.
- Verify host users still redirect to `/host-sentence-scrambler`.
- Verify unregistered players still redirect to `/register`.
- Verify players still redirect to `/waiting-room` when no round is active.

## Assumptions

- This stage does not add new games or game selection.
- The reusable component is reusable for the current player gameplay layout, not a new generic game engine.
- No changes are needed in `HaveFun.Core`, routing, persistence, scoring, or host flow.
