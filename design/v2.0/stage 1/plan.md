# v2.0 Stage 1 Plan: Word Scrambler Game

## Summary

Add a Word Scrambler game using the existing in-memory round, player, tile, and score models. Word Scrambler gets its own host and player pages, appears in the host menu, uses each selected word as the round sentence text, renders letters as `Tile` values in `PlayerGameBoard`, and calculates current-round results inside `HostWordScrambler`.

## Key Changes

- Add Word Scrambler routes:
  - `HostWordScrambler.razor` and code-behind at `/host-word-scrambler`.
  - `PlayerWordScrambler.razor` and code-behind at `/player-word-scrambler`.
  - Add a host menu link for Word Scrambler next to Sentence Scrambler.

- Reuse existing game models and services:
  - Do not add new entity/model types.
  - Use `SentenceDefinition.Text` as the Word Scrambler word.
  - Use `CurrentRound`, `PlayerRoundState`, `Tile`, `RoundResults`, and `PlayerResult`.
  - Keep app state in memory and do not add persistence.

- Support game-specific tile creation:
  - Extend `IGameStateService.StartRound` with an overload or parameter that lets the host page provide tile creation for the round.
  - Keep the existing Sentence Scrambler behavior using the current word-based tile service.
  - For Word Scrambler, create one `Tile` per character from `CurrentRound.SentenceText`, with `Tile.Text` set to the letter.
  - Exclude whitespace from Word Scrambler tiles; preserve repeated letters as separate tiles with distinct IDs.

- Implement `HostWordScrambler` by following the current host flow:
  - Host role/session check.
  - File selection, word index, timeout, start/stop button, active timer, current word progress, player list, and result table.
  - Start each round from the selected word line as `SentenceDefinition.Text`.
  - Calculate current-round Word Scrambler results in `HostWordScrambler`, not in shared game services.
  - Score by comparing submitted letters joined together against the target word, position by position.

- Implement `PlayerWordScrambler` by following the current player flow:
  - Player role/session check.
  - Redirect unregistered users to `/register`.
  - Redirect hosts to `/host-word-scrambler`.
  - Redirect players to `/waiting-room` when no active round exists.
  - Render `PlayerGameBoard` with Word Scrambler title/rules and letter tiles.
  - Use existing select, return, submit, submitted alert, and timer behavior.

## Test Plan

- Run `dotnet build .\HaveFun.sln --no-restore` from `src`.
- Verify the host menu shows both Sentence Scrambler and Word Scrambler for host users.
- Verify `/host-word-scrambler` starts and stops rounds using selected word lines and shows letter-based results.
- Verify `/player-word-scrambler` shows letter tiles, lets players build a word, return letters, and submit.
- Verify submitted Word Scrambler results are scored in `HostWordScrambler` and the correct word is shown after round completion.
- Verify Sentence Scrambler still creates word tiles and its host/player routes still behave the same.
- Verify host, player, unregistered, and waiting-room redirects still work for both games.

## Assumptions

- Word Scrambler uses the same sentence file service and treats each non-empty line as one word.
- No new game selector is added in this stage; players reach Word Scrambler through `/player-word-scrambler`.
- The app continues to support one active shared round at a time across games.
- Total score storage may continue using the existing service totals, but current-round Word Scrambler result calculation belongs to `HostWordScrambler`.
