# Stage 5 Plan: Word Scramble Gameplay

## Objective

Build the player interaction loop for Word Scramble. Players should click shuffled words to build a collected sentence, submit when all words are selected, and have the server record their submitted sentence and spent time.

## Context

Stage 5 builds on the current app foundation:

- `HaveFun.Web` is a Blazor Web App with `InteractiveServer` render mode.
- MudBlazor is used for visible web UI.
- Browser session storage stores only the current user's `name` and `role`.
- Player names are registered in the in-memory player registry.
- The Master Dashboard can start a round from a selected predefined sentence.
- `IGameStateService` stores the current round in server memory.
- Current round state includes original words, shuffled words, start time, time limit, and status.
- Player screens update when a round starts.
- Player screens show waiting state, active round state, shuffled words, and countdown timer.
- Services and interfaces use the `Service` suffix.

## Scope

Included:

- Let players click available shuffled words.
- Move clicked words from available words into the player's collected sentence.
- Show the collected sentence while playing.
- Submit the completed sentence when all words are selected.
- Prevent editing after submission.
- Record player submitted sentence in server memory.
- Record spent time from round start to player submission.
- Show a completed/submitted state on the player screen.
- Keep each player's in-progress selection separate.

Not included:

- Correctness calculation.
- Ranking.
- Master result grid with correctness and rank.
- Sorting results.
- Repeat-round polish beyond clearing gameplay state when a new round starts.
- Database, persistence, cloud services, accounts, passwords, or real authentication.

## Architecture

Use `HaveFun.Core` for reusable gameplay state and submission recording.

Recommended Core additions:

- `PlayerRoundState` model:
  - Player name.
  - Round id.
  - Available words.
  - Collected words.
  - Submitted sentence.
  - Submitted timestamp.
  - Spent time.
  - Submission status.
- `PlayerSubmission` or equivalent model:
  - Player name.
  - Round id.
  - Submitted sentence.
  - Spent time.
- Extend `IGameStateService` with:
  - Get or create player round state.
  - Select a word for a player.
  - Submit a player result.
  - Check whether a player has submitted.
  - Clear player round states when a new round starts.

Use `HaveFun.Web` for player interactions and MudBlazor UI.

Recommended Web updates:

- Update `Pages/Player.razor` and `Pages/Player.razor.cs`.
- Bind available words to click actions.
- Render collected words/sentence.
- Render submitted/completed state.
- Disable or hide word controls after submission.
- Keep timer display from Stage 4.

## Gameplay Rules

Rules:

- Player can only play when a round is active.
- Player state is keyed by player name and current round id.
- Player starts with the current round shuffled words as available words.
- Clicking a word removes that word instance from available words.
- Clicking a word appends that word instance to collected words.
- Duplicate words must be handled as separate word instances.
- The collected sentence is the collected words joined with spaces.
- Player can submit only after all words are selected.
- After submission, the player cannot edit their collected words for that round.
- A new round clears previous in-progress selections and submissions.

Word identity guidance:

- Do not rely only on word text when removing clicked words, because sentences may contain repeated words.
- Use stable word ids or available-word indexes for the player's current word list.

## Submission Rules

Rules:

- Submission records the player name.
- Submission records the current round id.
- Submission records the submitted sentence.
- Submission records spent time from round start to submission.
- If the round timer has expired, Stage 5 may still allow final submission only if all words are already selected; stricter timeout enforcement can be polished later if needed.
- Duplicate submission for the same player and round is rejected or ignored.
- Submission state remains in server memory only.

## Player Screen

Required UI:

- Player display name.
- Round status.
- Countdown timer.
- Available word buttons.
- Collected sentence area.
- Submit button or automatic submit after final word.
- Submitted/completed state.

Behavior:

- Before a round starts, keep the waiting state from Stage 4.
- During a round, show available words and collected sentence.
- Clicking an available word moves it into collected sentence.
- Submit is disabled until all words are selected.
- After submit, show submitted sentence and spent time.
- After submit, word buttons are no longer editable.

Recommended UI:

- Use MudBlazor buttons or chips for available words.
- Use a distinct collected sentence area so progress is obvious.
- Use a clear completed state after submission.

## Dashboard Impact

Stage 5 can keep the dashboard mostly as-is.

Optional Stage 5 dashboard updates:

- Show player submission count.
- Show which players have submitted.

Out of scope for dashboard:

- Correct sentence display in result grid.
- Correctness score.
- Spent-time ranking.
- Rank.

## Done Criteria

Stage 5 is complete when:

- Player can click available words to build a collected sentence.
- Clicked words move out of available words.
- Collected sentence is visible while playing.
- Player can submit after selecting all words.
- Submitted players cannot edit their result for that round.
- Server records submitted sentence and spent time.
- New round clears previous player gameplay state.
- No correctness calculation, ranking, database, cloud service, accounts, passwords, or real auth system is added.

## Test Plan

Manual checks:

- Build from `src` with `dotnet build .\HaveFun.sln`.
- Run `HaveFun.Web`.
- Join as a player.
- Join as master and start a round.
- Confirm player sees available word buttons.
- Click several words and confirm they move into collected sentence.
- Click all words and confirm submit becomes available or auto-submit occurs.
- Submit the sentence.
- Confirm completed/submitted state appears.
- Confirm word controls cannot be edited after submission.
- Confirm server-side state records submitted sentence and spent time.
- Start a new round and confirm previous player state is cleared.

Failure checks:

- Try to submit before selecting all words and confirm it is blocked.
- Refresh player page mid-round and confirm server-side player round state is restored or handled clearly.
- Try to submit twice and confirm duplicate submission is blocked or ignored.
- Confirm the app still starts without any database or cloud dependency.

## Handoff to Stage 6

Stage 6 can build on:

- Player submissions stored in server memory.
- Submitted sentence per player.
- Spent time per player.
- Current round original words.
- Immutable submitted state per player and round.
