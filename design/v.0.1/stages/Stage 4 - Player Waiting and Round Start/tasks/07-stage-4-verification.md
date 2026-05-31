# Task 07: Stage 4 Verification

## Goal

Verify the complete Stage 4 player waiting and round-start flow.

## Work

- Build the solution.
- Run `HaveFun.Web`.
- Test player waiting state.
- Test missing-session and wrong-role player blocking.
- Test master starting a round.
- Test player live updates.
- Test same shuffled words across players.
- Test player countdown timer.
- Confirm no database, cloud service, accounts, passwords, real auth, gameplay, submissions, correctness, or ranking were added.

## Done Criteria

- All Stage 4 done criteria from `plan.md` pass.
- The app still starts without persistence or external services.
- Server restart clears current round state and connected player state.
- Stage 5 has active-round player screens to build on.

## Verification

- Run `dotnet build .\HaveFun.sln` from `src`.
- Manually run the web app and test with one master tab and at least two player tabs.
