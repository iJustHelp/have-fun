# Task 02: Calculate Correctness

## Goal

Calculate submitted sentence correctness by word position.

## Work

- Compare submitted words against current round original words.
- Count positions where submitted word equals original word.
- Return correctness count and total word count.
- Use exact word tokens from the round.
- Do not add fuzzy matching, punctuation normalization, spelling tolerance, or case normalization.

## Done Criteria

- Correctness is calculated by matching words in the same position.
- Fully correct submissions score total word count.
- Partially correct submissions score only matching positions.

## Verification

- Build the solution from `src`.
- Verify a known example such as `The brown quick fox` against `The quick brown fox` scores `2 of 4`.
