# v2.0 Stage 1 Plan: Spelling Bee Game

## Summary

Add a Spelling Bee game using the existing in-memory round, player, tile, and score models. Spelling Bee gets its own host and player pages, appears in the host menu, uses each selected word as the round sentence text, renders letters as `Tile` values in `PlayerGameBoard`, and calculates current-round results inside `HostSpellingBee`.

## Key Changes

- Add Spelling Bee routes:
  - `HostSpellingBee.razor` and code-behind at `/host-spelling-bee`.
  - `PlayerSpellingBee.razor` and code-behind at `/player-spelling-bee`.
  - Add a host menu link for Spelling Bee next to Sentence Scrambler.

- Reuse existing game models and services:
  - Do not add new entity/model types.
  - Use `SentenceDefinition.Text` as the Spelling Bee word.
  - Use `CurrentRound`, `PlayerRoundState`, `Tile`, `RoundResults`, and `PlayerResult`.
  - Keep app state in memory and do not add persistence.

- Support game-specific tile creation:
  - Extend `IGameStateService.StartRound` with an overload or parameter that lets the host page provide tile creation for the round.
  - Keep the existing Sentence Scrambler behavior using the current word-based tile service.
  - For Spelling Bee, create one `Tile` per character from `CurrentRound.SentenceText`, with `Tile.Text` set to the letter.
  - Exclude whitespace from Spelling Bee tiles; preserve repeated letters as separate tiles with distinct IDs.

- Implement `HostSpellingBee` by following the current host flow:
  - Host role/session check.
  - File selection, word index, timeout, start/stop button, active timer, current word progress, player list, and result table.
  - Start each round from the selected word line as `SentenceDefinition.Text`.
  - Calculate current-round Spelling Bee results in `HostSpellingBee`, not in shared game services.
  - Score by comparing submitted letters joined together against the target word, position by position.

- Implement `PlayerSpellingBee` by following the current player flow:
  - Player role/session check.
  - Redirect unregistered users to `/register`.
  - Redirect hosts to `/host-spelling-bee`.
  - Redirect players to `/waiting-room` when no active round exists.
  - Render `PlayerGameBoard` with Spelling Bee title/rules and letter tiles.
  - Use existing select, return, submit, submitted alert, and timer behavior.

## Test Plan

- Run `dotnet build .\HaveFun.sln --no-restore` from `src`.
- Verify the host menu shows both Sentence Scrambler and Spelling Bee for host users.
- Verify `/host-spelling-bee` starts and stops rounds using selected word lines and shows letter-based results.
- Verify `/player-spelling-bee` shows letter tiles, lets players build a word, return letters, and submit.
- Verify submitted Spelling Bee results are scored in `HostSpellingBee` and the correct word is shown after round completion.
- Verify Sentence Scrambler still creates word tiles and its host/player routes still behave the same.
- Verify host, player, unregistered, and waiting-room redirects still work for both games.

## Assumptions

- Spelling Bee uses the same sentence file service and treats each non-empty line as one word.
- No new game selector is added in this stage; players reach Spelling Bee through `/player-spelling-bee`.
- The app continues to support one active shared round at a time across games.
- Total score storage may continue using the existing service totals, but current-round Spelling Bee result calculation belongs to `HostSpellingBee`.
