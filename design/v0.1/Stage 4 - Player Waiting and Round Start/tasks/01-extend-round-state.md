# Task 01: Extend Round State

## Goal

Extend the in-memory round state so a started round includes stable shuffled words.

## Work

- Extend `CurrentRound` with shuffled words.
- Add original words if useful for later stages.
- Split selected sentence text into words when starting a round.
- Shuffle words on the server.
- Store one stable shuffled word order for the current round.
- Keep round state in server memory only.

## Done Criteria

- Starting a round creates a new round id.
- Starting a round stores sentence text, time limit, start time, and shuffled words.
- Every read of the current round returns the same shuffled word order until another round starts.
- No player submissions, correctness, ranking, database, or persistence is added.

## Verification

- Build the solution from `src`.
- Start a round and confirm current round state includes shuffled words.
