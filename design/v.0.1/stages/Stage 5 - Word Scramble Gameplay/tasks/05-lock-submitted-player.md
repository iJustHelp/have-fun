# Task 05: Lock Submitted Player

## Goal

Prevent submitted players from editing their result for the current round.

## Work

- Detect when the current player has submitted for the current round.
- Disable or hide available word controls after submission.
- Prevent selecting more words after submission.
- Prevent duplicate submissions for the same player and round.
- Keep the submitted sentence visible.

## Done Criteria

- Submitted players cannot edit collected words.
- Submitted players cannot submit twice.
- Submitted state survives page refresh while server memory remains active.
- A new round resets the player into active gameplay state.

## Verification

- Build the solution from `src`.
- Submit as a player and confirm controls are locked.
- Refresh the player page and confirm submitted state remains.
- Start a new round and confirm controls unlock for the new round.
