# Stage 8 Plan: Start Button Becomes Stop During Active Round

## Summary

Implement Stage 8 on the Host Sentence Scrambler page so the round control button changes from `Start` to `Stop` while a sentence round is active. Clicking `Stop` immediately interrupts the active round and completes it using the same behavior as timer expiration.

## Key Changes

- Update the host round control button behavior:
  - When no round is active, show `Start` with the existing play icon and start behavior.
  - When a round is active, show `Stop` with a stop/cancel icon.
  - Keep the button enabled during an active round so the Host can interrupt.
- Add a host-side stop handler:
  - Call the existing round completion flow through `IGameStateService.CompleteCurrentRound()`.
  - Stop the host timer.
  - Refresh player results.
  - Reveal the correct sentence.
  - Allow the Host to start the next sentence after the stopped round completes.
- Preserve existing round rules:
  - Timer expiration still completes the round.
  - All expected players submitting still completes the round.
  - Submitted player results still count.
  - Unsubmitted players remain without submitted sentence/current-round score.
  - Sentence progression continues forward; stopping sentence N means the next `Start` begins sentence N+1.

## Interfaces

- No new public service interface is required.
- Reuse existing `IGameStateService.CompleteCurrentRound()`.
- Update only Host Sentence Scrambler UI/code-behind behavior unless implementation reveals a missing helper is needed.

## Test Plan

- Build the solution with `dotnet build .\HaveFun.sln` from `src`.
- Start the web app and verify the Host Sentence Scrambler page loads.
- Select a sentence file and click `Start`; verify:
  - A round starts.
  - Timer begins.
  - Button changes to `Stop`.
- Click `Stop`; verify:
  - Round completes immediately.
  - Timer stops.
  - Correct sentence is shown.
  - Submitted results remain visible.
  - Button changes back to `Start`.
- Click `Start` again; verify the next sentence starts, not the stopped sentence.
- Verify existing completion paths still work:
  - Timeout completes the round.
  - All expected players submitting completes the round.

## Assumptions

- “Interrupt” means complete the current round early, equivalent to timer expiration.
- Stopping a round does not discard submitted answers.
- Stopping a round advances sentence progression the same way as any completed round.
- Stage 8 does not introduce pause/resume, restart-current-sentence, or confirmation prompts.
