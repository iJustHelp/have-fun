# Update V1.0 Stage 1 Plan

## Summary

Revise Stage 1 around the new Host/Register workflow described by `brd.md`. This plan is the implementation source for v1.0 Stage 1 and supersedes older `design/spec.md` sections that describe Game Master role naming, configured master-name entry, or master-name dashboard access.

## Key Changes

- State that this v1.0 Stage 1 plan overrides any older `design/spec.md` sections that describe Game Master role naming, configured master-name entry, or master-name dashboard access.
- Replace Game Master/Master terminology with Host in product language, UI text, routing/session role naming, and practical code model references.
- Rename the role model from `UserRole` to `Role`, with only `Host` and `Player` values.
- Remove `Game:MasterName` from configuration and remove startup validation or join logic that depends on it.
- Define role assignment:
  - Opening the app with no query parameters assigns the current browser session `Role.Host`.
  - Players join through the Register page route.
- Keep `/dashboard` for now as the Host dashboard route.
- Define Home as the Host landing page for now:
  - Home displays the player registration URL.
  - The registration URL points directly to the Register page route.
  - QR code work is explicitly out of scope for now.

## Test Plan

- Verify the updated plan no longer requires `Game:MasterName`.
- Verify the updated plan clearly distinguishes Host/no-query flow from Player/Register-page flow.
- Verify `/dashboard` remains in scope and is not replaced by `/home`.
- Verify QR code generation is not required by this stage plan.

## Assumptions

- The Register page route is the player entry point; no player-identifying query parameter is required.
- The old `design/spec.md` remains present for historical context, but the v1.0 Stage 1 plan takes precedence where the two conflict.
