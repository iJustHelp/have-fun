# Task 04: Add Submit Flow

## Goal

Let players submit after selecting all words.

## Work

- Add a Submit button or automatic submit after the final word.
- Disable Submit until all words are selected.
- Submit the collected sentence through `IGameStateService`.
- Record submitted sentence.
- Record spent time from round start to submission.
- Show submitted/completed state on the player screen.

## Done Criteria

- Player can submit only after selecting all words.
- Server records submitted sentence.
- Server records spent time.
- Player sees completed/submitted state after submission.

## Verification

- Build the solution from `src`.
- Select all words and submit.
- Confirm submitted sentence and spent time are visible or stored.
