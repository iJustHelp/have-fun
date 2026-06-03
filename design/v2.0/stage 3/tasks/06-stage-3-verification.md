# Task 06: Stage 3 Verification

## Goal

Verify the complete Formula Scrambler flow and regress the existing games.

## Work

- Build the solution.
- Run `HaveFun.Web`.
- Open one host browser tab and at least one player browser tab.
- Verify Formula Scrambler host page loads and can start rounds from configured formula files.
- Verify player waiting-room routing into Formula Scrambler.
- Verify character-tile selection, return, submit, submitted lockout, and refresh restoration.
- Verify mathematical-equivalence scoring.
- Verify incorrect formulas receive position-match partial scores.
- Verify invalid formulas are handled as incorrect without crashing.
- Verify Sentence Scrambler and Spelling Bee still work independently.

## Done Criteria

- All Stage 3 plan acceptance checks pass.
- Formula Scrambler is playable end to end.
- Existing games still build, route, start rounds, submit results, and keep separate state.
- No database, cloud service, account system, password system, or persistence is introduced.

## Verification

- Run `dotnet build .\HaveFun.sln --no-restore` from `src`.
- Manually verify equivalent examples such as source `(1+3)/2=2` and submission `2=(1+3)/2`.
- Manually verify invalid examples such as `1/0=0`, `1+2=3=3`, and formulas containing unsupported letters.
