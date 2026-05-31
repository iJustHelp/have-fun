# Task 03: Load Sentence Source

## Goal

Load predefined Word Scramble sentences from JSON into memory during server startup.

## Work

- Add `src/HaveFun.Web/assets/sentences.json`.
- Add sentence models in `HaveFun.Core`.
- Add sentence loading and validation logic in `HaveFun.Core`.
- Register an in-memory sentence service from `HaveFun.Web`.
- Load the sentence file when the server starts.

## JSON Shape

```json
[
  {
    "text": "The quick brown fox jumps over the lazy dog",
    "timeLimitInSeconds": 30
  }
]
```

## Validation Rules

- Sentence file must exist.
- Sentence list must not be empty.
- `text` must not be empty or whitespace.
- `timeLimitInSeconds` must be greater than zero.

## Done Criteria

- Sentences are available in memory after startup.
- Sentence model and validation are reusable from `HaveFun.Core`.
- Missing or invalid sentence data fails startup clearly.

## Verification

- Run with a valid sentence file.
- Test missing file, empty list, empty text, and invalid time limit.
