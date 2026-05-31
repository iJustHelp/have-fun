# Task 03: Build Join Form

## Goal

Replace the Stage 1 first-page placeholder with the shared MudBlazor name entry form.

## Work

- Update the home page to show one name input and one join button.
- Use MudBlazor components for the form, input, button, and validation display.
- Keep showing the app name and LAN join URL when available.
- Trim submitted names before validation.
- Reject empty or whitespace-only names.
- Preserve the typed name when validation fails.
- After a successful player join, write the player id and display name to browser session storage.
- Do not write the configured master name to browser session storage.

## Done Criteria

- The first page has a working name input and join button.
- Empty names show a clear validation error.
- Successful player joins create browser session storage for the current tab.
- The LAN URL presentation from Stage 1 remains visible.
- The page uses MudBlazor components and styles.

## Verification

- Build the solution from `src`.
- Run `HaveFun.Web`.
- Open `/`.
- Submit an empty name and confirm a validation error appears.
- Submit a unique player name and confirm session storage contains the player id and display name.
