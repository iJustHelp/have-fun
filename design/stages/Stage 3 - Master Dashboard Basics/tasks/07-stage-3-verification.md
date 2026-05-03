# Task 07: Stage 3 Verification

## Goal

Verify the complete Stage 3 dashboard basics flow.

## Work

- Build the solution.
- Run `HaveFun.Web`.
- Test master-only dashboard rendering.
- Test player and missing-session dashboard blocking.
- Test LAN URL display.
- Test sentence selector and preview.
- Test connected player list.
- Test Start Round behavior.
- Confirm session storage still stores only `name` and `role`.
- Confirm no database, cloud service, accounts, passwords, real auth, gameplay, submissions, correctness, or ranking were added.

## Done Criteria

- All Stage 3 done criteria from `plan.md` pass.
- The app still starts without persistence or external services.
- Server restart clears current round state and connected player state.
- Stage 4 has current round basics to build on.

## Verification

- Run `dotnet build .\HaveFun.sln` from `src`.
- Manually run the web app and test the dashboard flow in at least two browser tabs.
