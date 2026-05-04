# Task 08: Stage 5 Verification

## Goal

Verify the complete Word Scramble gameplay loop.

## Work

- Build the solution.
- Run `HaveFun.Web`.
- Join as player.
- Join as master and start a round.
- Test word selection.
- Test collected sentence display.
- Test submit availability after all words are selected.
- Test submitted/completed state.
- Test post-submit lockout.
- Test refresh restoration.
- Test new round state reset.
- Confirm no correctness, ranking, database, cloud service, accounts, passwords, or real auth were added.

## Done Criteria

- All Stage 5 done criteria from `plan.md` pass.
- Server records submitted sentence and spent time.
- Stage 6 has submissions to score and rank.

## Verification

- Run `dotnet build .\HaveFun.sln` from `src`.
- Manually run the web app and test with one master tab and at least one player tab.
