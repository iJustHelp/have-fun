# Stage 4 Host Sentence File Selection Plan

## Summary

Update the Host Sentence Scrambler page so the Host selects a sentence JSON file from a configured folder instead of selecting an individual sentence from the current global `sentences.json` list. Stage 4 also simplifies the Host game page to only the game title/rules, file selector, Start button, and players grid.

## Key Changes

- Add a `SentenceScramblerPath` setting to `appsettings.json`.
- Treat `SentenceScramblerPath` as the folder that contains one or more sentence JSON files.
- Load available sentence file names from the configured folder.
- Show only those file names in the Host Sentence Scrambler dropdown.
- Enable the Start button only after the Host selects a file.
- Remove the existing Host page controls and display sections that are not part of Stage 4:
  - round status panel
  - per-sentence dropdown
  - selected sentence preview
  - submission progress
  - results grid
- Keep the players grid on the Host Sentence Scrambler page.
- Keep the player Waiting Room and Player Sentence Scrambler routing behavior from Stage 3.

## Configuration

- Add `SentenceScramblerPath` to web app configuration.
- The configured path may be relative to the web content root or absolute.
- The folder should contain `.json` files using the existing sentence JSON shape:

```json
[
  {
    "text": "The quick brown fox jumps over the lazy dog",
    "timeLimitInSeconds": 30
  }
]
```

## Host Page Behavior

- Route remains `/host-sentence-scrambler`.
- Page shows the Sentence Scrambler title and rules.
- Page shows a dropdown of sentence file names from `SentenceScramblerPath`.
- Start button is disabled until a file is selected.
- On Start:
  - Load and validate the selected file.
  - Start the game using sentence data from that file.
  - Notify waiting players through the existing round-change flow.
- If the configured folder is missing, empty, or contains no valid `.json` files, show a clear Host-facing error.
- Do not add database, authentication, persistence, or external services.

## Service Changes

- Add a reusable Core service for listing sentence files and loading a selected file.
- Reuse the existing sentence JSON validation rules where practical.
- Keep file-system access server-side only.
- Avoid storing selected file state in browser session storage.

## Test Plan

- Verify app startup succeeds with a valid `SentenceScramblerPath`.
- Verify Host Sentence Scrambler lists JSON file names from the configured folder.
- Verify Start is disabled before a file is selected.
- Verify selecting a file enables Start.
- Verify starting a game sends registered/waiting players to the Player Sentence Scrambler page.
- Verify player gameplay still uses the selected file's sentence data.
- Verify Host page no longer shows round status, per-sentence selector, selected sentence preview, submission progress, or results grid.
- Verify Host page still shows the players grid.
- Verify a missing or empty configured folder shows a clear error.

## Assumptions

- `SentenceScramblerPath` is a folder path, not a single file path.
- The selected file contains one or more sentence records in the existing `sentences.json` shape.
- When Start is clicked, the app may choose the first sentence from the selected file for the round unless later requirements specify random selection or multi-sentence rounds.
- Existing in-memory game state remains the source of truth for the active round.
