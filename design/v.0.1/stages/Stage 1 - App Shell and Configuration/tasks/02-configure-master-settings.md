# Task 02: Configure Master Settings

## Goal

Load and validate the configured Game Master name from `appsettings.json`.

## Work

- Add `Game:MasterName` to `src/HaveFun.Web/appsettings.json`.
- Add a typed settings model in `HaveFun.Core`.
- Bind `Game:MasterName` during `HaveFun.Web` startup.
- Validate that `Game:MasterName` is not empty or whitespace.
- Fail startup clearly when the setting is invalid.

## Done Criteria

- Master settings are represented by a reusable model in `HaveFun.Core`.
- `HaveFun.Web` can read the configured master name.
- Invalid or missing master name produces a clear startup error.
- The master name is not displayed on the first page.

## Verification

- Run the app with a valid `Game:MasterName`.
- Temporarily test an empty value and confirm startup fails clearly.
