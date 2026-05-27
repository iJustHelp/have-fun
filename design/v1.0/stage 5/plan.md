# Stage 5 Sentence Scrambler Round Control Plan

## Summary

Implement Stage 5 by switching sentence sources from JSON files to `.txt` files, where each non-empty line is one playable sentence. The Host Sentence Scrambler page becomes the live round controller: choose a text file, set the per-sentence timer, start the next sentence in order, show progress, reveal the correct sentence when the round ends, and display sortable player results.

## Key Changes

- Replace Stage 4 JSON file selection with TXT file selection.
- `SentenceScramblerPath` continues to point to the sentence-file folder, but the Host dropdown lists `.txt` files only.
- Add a text-file sentence loader that reads non-empty lines and converts each line into a playable sentence using the Host-entered time value.
- Add a Time textbox on `/host-sentence-scrambler`, defaulting to `30` seconds.
- Start button starts the next sentence in file order, then disables while the round is active.
- Show current sentence index as `N of Total`.
- End the active round when either the timer expires or all players registered at round start submit.
- When the round ends, reveal the correct sentence and enable Start for the next sentence.
- After the final sentence, keep Start disabled and show that the file is complete.

## Core/Game State Changes

- Extend round state so it can represent active vs completed rounds and expose whether the correct sentence should be visible.
- Track the registered players expected for each round at start time.
- Add round completion behavior to `IGameStateService`, including completing on timeout and completing when all expected players have submitted.
- Keep scoring as current positional word matching: score is the count of words in the submitted sentence that match the correct word at the same index.
- Keep state in memory only; no database or persistence.

## Host Page Changes

- Keep route `/host-sentence-scrambler`.
- Keep only:
  - title/rules
  - `.txt` file dropdown
  - Time textbox
  - Start button
  - sentence index display
  - correct sentence display after completion
  - sortable players/results grid
- Player grid columns:
  - Player name
  - Time before submit
  - Submitted sentence
  - Score
- Remove Joined column.
- Sort grid by score/time/name using MudBlazor table sorting.

## Player Flow

- Waiting players still navigate to `/player-sentence-scrambler` when Host starts a round.
- Player gameplay continues to use the active round’s shuffled words.
- Player submission updates Host grid immediately through existing game-state change notifications.
- Removed players still redirect to `/register` as in earlier stages.

## Test Plan

- Verify Host dropdown lists `.txt` files from `SentenceScramblerPath` and ignores JSON files.
- Verify Time defaults to `30` and controls the round timer.
- Verify Start is disabled until a file is selected.
- Verify Start begins sentence `1 of Total`, disables while active, and routes waiting players to the player game page.
- Verify all registered players submitting ends the round before timeout.
- Verify timeout ends the round even if not all players submit.
- Verify correct sentence appears only after completion.
- Verify Start advances to the next sentence in order.
- Verify Start remains disabled after the final sentence completes.
- Verify Host grid shows player name, time before submit, submitted sentence, and score.
- Verify Host grid sorting works.
- Verify build succeeds with `dotnet build .\HaveFun.sln --no-restore -c Release`.

## Assumptions

- Stage 5 replaces JSON sentence-file support for this flow; `.txt` is the only Host dropdown source.
- A blank line in a `.txt` file is ignored.
- Each `.txt` line uses the Host-entered time value for that round.
- “All players submit” means all players registered at the moment the Host starts that sentence.
- Sentence progression is sequential and stops after the last line.
