# Task 06: Stage 2 Verification

## Goal

Verify the complete Stage 2 name entry and role-routing flow.

## Work

- Build the solution.
- Run `HaveFun.Web`.
- Test empty name validation.
- Test master-name routing.
- Test unique player registration.
- Test browser session storage is written after player registration.
- Test player page refresh restores the current player from session storage.
- Test duplicate player-name rejection with different casing.
- Test trimming behavior for submitted names.
- Test server restart invalidates old browser session storage because server memory is cleared.
- Confirm Stage 1 configuration and sentence validation still works.
- Confirm no database, auth, cloud service, rounds, gameplay, results, or ranking were added.

## Done Criteria

- All Stage 2 done criteria from `plan.md` pass.
- The app still starts without persistence or external services.
- Restarting the app clears registered player state.
- Browser session storage is tab-scoped and does not replace server-side uniqueness checks.
- Stage 3 has a dashboard route and player registry to build on.

## Verification

- Run `dotnet build .\HaveFun.sln` from `src`.
- Manually run the web app and test the join flow in at least two browser tabs.
- Confirm each browser tab keeps only its own current player identity in session storage.
