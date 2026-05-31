# Task 07: Stage 1 Verification

## Goal

Verify the full Stage 1 milestone before moving to Stage 2.

## Work

- Build the solution from `src`.
- Run `HaveFun.Web`.
- Open the local URL in a browser.
- Confirm the first page renders.
- Confirm `Game:MasterName` loads from `appsettings.json`.
- Confirm sentences load into memory through `HaveFun.Core`.
- Confirm local and LAN URL behavior.
- Confirm no database, cloud service, authentication, player routing, or name validation was added.

## Failure Checks

- Empty `Game:MasterName` causes a clear startup error.
- Missing sentence file causes a clear startup error.
- Empty sentence list causes a clear startup error.
- Empty sentence text causes a clear startup error.
- Invalid `timeLimitInSeconds` causes a clear startup error.

## Done Criteria

- All Stage 1 done criteria from `plan.md` are satisfied.
- The app is ready for Stage 2 name entry and role routing.
