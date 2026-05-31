# Task 06: Start Selected Round

## Goal

Let the Game Master start a selected sentence as the current round.

## Work

- Add Start Round button behavior.
- Store selected sentence text and time limit in the in-memory game state.
- Create a new current round id.
- Set round status to started.
- Store the round start timestamp.
- Update dashboard round status after start.
- Keep player screens unchanged in Stage 3.

## Done Criteria

- Game Master can start a selected round.
- Current round basics are stored in server memory.
- Dashboard status updates after start.
- Starting a round does not implement shuffled words, player gameplay, submissions, results, correctness, or ranking.

## Verification

- Build the solution from `src`.
- Join as master.
- Select a sentence.
- Click Start Round.
- Confirm dashboard status updates and selected round details remain visible.
