# Have-Fun V1 Specification

## Summary

Have-Fun is a local client-server party game app. One person hosts the server on a Windows or Mac computer, and all players join from browsers on the same LAN by opening a shared local URL.

V1 includes one game: Word Scramble. The Game Master selects a predefined sentence, starts a round, and watches player results on a dashboard. Players reconstruct the original sentence by clicking shuffled words in the correct order.

## Goals

- Run without cloud deployment, paid services, accounts, or external infrastructure.
- Let friends play together from phones, tablets, or computers on the same LAN.
- Keep all game state in server memory.
- Make the Game Master flow fast enough to run repeated rounds.
- Build the app so more games can be added later.

## Non-Goals

- No public internet hosting in V1.
- No database or persistent storage in V1.
- No user accounts, passwords, or authentication in V1.
- No support for players outside the host computer's LAN.
- No custom sentence editing from the browser in V1.

## Users and Roles

### Game Master

The Game Master controls the round. A user becomes the Game Master by entering the master name configured in `appsettings.json`.

Responsibilities:

- Enters the configured master name on the first page.
- Opens the master dashboard automatically.
- Sees the shared join URL.
- Selects one predefined sentence from a JSON-backed list.
- Starts a Word Scramble round.
- Watches player submissions.
- Sees the correct sentence, each player's submitted sentence, spent time, and correctness.
- Starts another round with a different sentence.

### Player

Players join from a browser.

Responsibilities:

- Opens the shared URL.
- Enters a display name.
- Waits for the current round to start.
- Clicks shuffled words to rebuild the sentence.
- Sees their collected sentence while playing.
- Submits automatically or manually after selecting the final word.
- Sees a completed state after submission.

## Core Flow

1. Host starts the server on a Windows or Mac computer.
2. Server displays or exposes a local LAN URL.
3. Game Master opens the shared URL and enters the configured master name.
4. App opens the master dashboard.
5. Players open the shared URL and enter unique display names.
6. Game Master selects a predefined sentence.
7. Game Master starts the round.
8. Server shuffles the sentence words for the round.
9. Players select words in order to reconstruct the sentence.
10. Each player submits the completed sentence.
11. Dashboard updates with results.
12. Dashboard ranks players.
13. Game Master starts another round.

## Word Scramble Rules

- A round is based on one predefined sentence.
- The server splits the sentence into words.
- The server sends players the words in shuffled order.
- Players click one available word at a time.
- Clicked words move into the player's collected sentence.
- Players can see their collected sentence while playing.
- A player finishes when all words are selected.
- The result is the ordered list of selected words joined back into a sentence.
- Correctness is based on how many words are in the correct position.
- A fully correct answer has every word in the same position as the original sentence.
- Ranking is by correctness first, then shortest spent time.

## Screens

### Join Screen

Used by both Game Master and Players. This is the first page users see.

Required UI:

- Name input.
- Join button.

Behavior:

- Name is required.
- The submitted name is trimmed before validation.
- If the name matches the configured master name from `appsettings.json`, the user opens the Master Dashboard.
- If the name does not match the configured master name, the user joins as a Player.
- Player names must be unique among connected players.
- Player name uniqueness is checked case-insensitively.
- A player cannot use the configured master name.

### Master Dashboard

Required UI:

- Shared LAN URL.
- Sentence selector.
- Start round button.
- Current correct sentence.
- Player result grid.
- Round status.

Result grid columns:

- Player name.
- Submitted sentence.
- Correctness.
- Spent time.
- Rank.

Behavior:

- The dashboard is available only to the configured master name.
- The Game Master can start a round after selecting a sentence.
- Starting a new round clears previous round submissions.
- The dashboard updates as players submit.

### Player Game Screen

Required UI:

- Round status.
- Shuffled word buttons.
- Collected sentence area.
- Completion state.

Behavior:

- Before a round starts, player sees a waiting state.
- During a round, player sees shuffled words.
- Clicking a word removes it from available words and adds it to the collected sentence.
- When all words are selected, the player can submit the result.
- After submission, the player cannot edit the result for that round.

## Data

### App Settings

The master name is configured in `appsettings.json`.

Recommended shape:

```json
{
  "Game": {
    "MasterName": "master"
  }
}
```

Rules:

- `Game:MasterName` must be non-empty.
- Name matching uses the trimmed submitted name.
- The master name comparison is case-insensitive.

### Sentence Source

Predefined sentences are stored in a JSON file.

Recommended shape:

```json
[
  {
    "text": "The quick brown fox jumps over the lazy dog",
    "timeLimitInSeconds": 30
  }
]
```

Rules:

- `text` must be non-empty.
- `timeLimitInSeconds` limit time when player must finish text.
- Sentences are loaded when the server starts.

### In-Memory State

The server keeps all state in memory.

Required state:

- Connected Game Master name.
- Connected player names.
- Current round id.
- Selected sentence id and text.
- Shuffled words for the current round.
- Round start time.
- Player submissions.
- Player spent time.
- Player correctness score.

Restart behavior:

- Restarting the server clears all connected users, rounds, and submissions.

## Local Network Behavior

- The server must listen on a local HTTP port.
- The app must provide a join URL usable by other devices on the same LAN.
- The URL should use the host computer's LAN IP address when possible.
- If LAN IP detection is unavailable, the app should still run on localhost for testing.

## Acceptance Criteria

- The first page asks for a name.
- Entering the configured master name opens the dashboard.
- Entering any other available name joins as a player.
- Duplicate player names are rejected.
- A Player can join from another browser tab or device using the shared URL.
- Game Master can select a predefined sentence and start a round.
- Player receives shuffled words and can reconstruct the sentence.
- Player can submit the completed sentence.
- Dashboard shows the correct sentence, player sentence, spent time, correctness, and rank.
- Ranking favors higher correctness, then lower time.
- Game Master can start a new round without restarting the server.
- No database is required.
- Restarting the server resets all state.

## Future Ideas

- Add more party games.
- Add custom browser-created sentences.
- Add QR code for the shared join URL.
- Add undo while selecting words.
- Add persistent game history.
- Add teams.
- Add support for internet play through optional tunneling or deployment.
