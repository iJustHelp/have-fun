# Task 02: Add Formula Parsing and Scoring

## Goal

Implement the Formula Scrambler rules for character tiles, mathematical correctness, and partial scoring.

## Work

- Add reusable Formula Scrambler logic for:
  - Removing whitespace from formulas.
  - Validating supported characters.
  - Splitting a submitted formula around exactly one `=`.
  - Evaluating arithmetic expressions on both sides of the equation.
  - Comparing evaluated values with a small numeric tolerance.
  - Counting source-character position matches for partial scores.
- Support digits, `+`, `-`, `*`, `/`, parentheses, one `=`, and ignored whitespace.
- Treat invalid syntax, unsupported characters, missing sides, missing `=`, multiple `=`, and division by zero as incorrect.
- Create formula tiles from each non-whitespace character.
- Score a mathematically correct submitted formula as the full source character count.
- Score an incorrect submitted formula by matching submitted characters against the source formula at the same character positions.

## Done Criteria

- Formula correctness accepts mathematically equivalent equations, not only exact text order.
- Invalid formulas do not throw during scoring.
- The score denominator is the non-whitespace character count of the source formula.
- The first-finisher bonus still applies only when the base score is fully correct.

## Verification

- Run `dotnet build .\HaveFun.sln --no-restore` from `src`.
- Verify `(1+3)/2=2` and `2=(1+3)/2` are both fully correct.
- Verify an incorrect but partially matching formula receives only position-match points.
- Verify invalid formulas, multiple equals signs, unsupported characters, and division by zero are incorrect.
