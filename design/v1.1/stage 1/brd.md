# Stage 1: Reusable Player Game Component

## Goal

Enable more games by extracting the current Sentence Scrambler player gameplay UI into a reusable component that can later be configured by game-specific pages.

## Current State

The `/player-sentence-scrambler` page currently owns both player routing/session checks and the visual game UI for Sentence Scrambler.

## Desired Behavior

- Keep `/player-sentence-scrambler` as the Sentence Scrambler player route.
- Move the visual gameplay experience into a reusable component.
- Pass configurable display values, such as title and rules, into the reusable component.
- Preserve the existing Sentence Scrambler player behavior exactly.

## Public APIs and Interfaces

- The BRD only requires component parameters at the requirements level, not final C# signatures.
- Minimum expected configurable values:
  - `Title`
  - `Rules`
- No service, model, database, persistence, or route contract changes are required.

## Out of Scope

- Do not add new games in this stage.
- Do not change Host flow.
- Do not change scoring.
- Do not change round state.
- Do not change routing beyond preserving the existing `/player-sentence-scrambler` route.
- Do not add persistence, database behavior, accounts, authentication, cloud services, or public hosting.

## Acceptance Criteria

- The Sentence Scrambler player page still works the same after the component extraction.
- The player game title and rules are supplied through component parameters.
- The reusable component can later be used by another game page.
- The solution builds successfully.

## Test Plan

- Review this BRD for a clear goal, current state, desired behavior, out-of-scope items, and acceptance criteria.
- After later implementation, run `dotnet build .\HaveFun.sln` from `src`.
- Manually verify `/player-sentence-scrambler` still shows the same Sentence Scrambler gameplay.

## Assumptions

- Stage 1 is a requirements clarification and component-extraction stage only.
- New game selection and additional games will be handled in later v1.1 stages.
