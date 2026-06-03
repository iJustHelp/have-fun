# Add Formula Scrambler Game

## Summary
Add Formula Scrambler as a third tile-based game using the existing host/player flow, file loading pattern, shared `PlayerGameBoard`, and isolated per-game state. Players arrange character tiles into a formula. A submission is mathematically correct when both sides of one equals sign evaluate to the same numeric value.

## Key Changes
- Add Formula Scrambler services:
  - `FormulaScramblerGameStateService : GameStateService`
  - `FormulaScramblerFileService : FileService`
  - Register both in DI as singletons.
- Add configuration and assets:
  - Add `Game:FormulaScramblerPath` with default `assets/formula-scrambler`.
  - Add initial formula text files under that folder.
  - Each non-empty line is one formula round, for example `(1+3)/2=2`.
- Add host/player pages:
  - Routes: `/host-formula-scrambler` and `/player-formula-scrambler`.
  - Host flow matches Spelling Bee: select file, set timeout, start/stop each formula, show players, submit time, submitted formula, round score, and total score.
  - Player flow uses `PlayerGameBoard` with character tiles and no separator.
  - Add Formula Scrambler to the host nav menu.
- Extend player routing:
  - Waiting room subscribes to `FormulaScramblerGameStateService`.
  - When a Formula Scrambler round starts, registered players route to `/player-formula-scrambler`.
  - Formula Scrambler player page redirects to another game page if another game starts.

## Formula Rules
- Tokenization is character-based:
  - Whitespace is ignored.
  - Supported characters: digits, `+`, `-`, `*`, `/`, `(`, `)`, and one `=`.
  - Each character becomes one tile and counts as one score unit.
- Correctness:
  - A submitted formula must contain exactly one `=`.
  - Evaluate the arithmetic expression on the left and right sides.
  - If the two numeric values are equal, the formula is mathematically correct.
  - Use normal arithmetic precedence and parentheses.
  - Division by zero, invalid syntax, missing sides, or unsupported characters are incorrect.
- Scoring:
  - Total score denominator is the count of non-whitespace characters in the source formula.
  - If the submitted formula is mathematically correct, score is the full denominator.
  - If not mathematically correct, score is the number of submitted characters matching the source formula at the same character position.
  - Keep existing first-finisher bonus behavior: only the first submitted player gets the +1 bonus when their base score is fully correct.

## Tasks
- `tasks/01-add-formula-services-and-assets.md`
- `tasks/02-add-formula-parsing-and-scoring.md`
- `tasks/03-build-host-formula-scrambler.md`
- `tasks/04-build-player-formula-scrambler.md`
- `tasks/05-wire-formula-routing-and-menu.md`
- `tasks/06-stage-3-verification.md`

## Test Plan
- Run `dotnet build .\HaveFun.sln --no-restore`.
- Verify host can open `/host-formula-scrambler`, select a formula file, start/stop rounds, and see results.
- Verify registered players wait after registration and route to `/player-formula-scrambler` only when Formula Scrambler starts.
- Verify correct equivalent submissions pass, for example `2=(1+3)/2` for source `(1+3)/2=2`.
- Verify incorrect submissions receive position-match partial score.
- Verify invalid formulas, missing `=`, multiple `=`, unsupported characters, and division by zero score as incorrect.
- Verify Sentence Scrambler and Spelling Bee still start independently and route players correctly.

## Assumptions
- This stage is additive: do not rename or remove existing Sentence Scrambler or Spelling Bee behavior.
- Formula files are plain text, one formula per line, matching the current file-service style.
- Numeric comparison can use a small tolerance for division results to avoid floating-point precision surprises.
