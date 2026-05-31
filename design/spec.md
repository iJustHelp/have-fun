# Have-Fun Specification

## Summary

Have-Fun is a local client-server party game app. One person hosts the server on a Windows or Mac computer, and all players join from browsers on the same LAN by opening a shared local URL.

V1 includes one game: Sentence Scrambler. The Host selects a configured sentence file, starts each sentence as a timed round, and watches player results on a host dashboard. Players reconstruct the original sentence by clicking shuffled words in the correct order.

V1.1 begins preparing the app for more games by extracting reusable player gameplay UI while preserving the Sentence Scrambler behavior.

## Goals

- Run without cloud deployment, paid services, accounts, or external infrastructure.
- Let friends play together from phones, tablets, or computers on the same LAN.
- Keep all game state in server memory.
- Make the Host flow fast enough to run repeated rounds.
- Build the app so more games can be added later.
- Keep reusable player gameplay UI separate from page-level routing and session checks.

## Non-Goals

- No public internet hosting in V1.
- No database or persistent storage in V1.
- No user accounts, passwords, or authentication in V1.
- No support for players outside the host computer's LAN.
- No custom sentence editing from the browser in V1.
- No additional games in V1.1 Stage 1.

## Users and Roles

### Host

The Host controls the round. The browser opened on the app home page becomes the Host automatically.

Responsibilities:

- Opens the app home page on the server computer.
- Sees the shared join URL.
- Sees a QR code for the player registration URL when it can be generated.
- Sees and can remove registered players.
- Selects a configured `.txt` sentence file.
- Sets the per-sentence time limit.
- Starts a Sentence Scrambler round for the next sentence in the file.
- Stops an active round early when needed.
- Watches player submissions.
- Sees the correct sentence, each player's submitted sentence, time before submit, score, and total score.
- Starts another round with the next sentence from the selected file.

### Player

Players join from a browser.

Responsibilities:

- Opens the shared URL.
- Enters a display name.
- Waits for the current round to start.
- Clicks shuffled words to rebuild the sentence.
- Sees their collected sentence while playing.
- Submits manually when ready.
- Can submit an incomplete sentence after confirming.
- Sees a completed state after submission.

## Core Flow

1. Host starts the server on a Windows or Mac computer.
2. Server displays or exposes a local LAN URL.
3. Host opens the app home page.
4. App stores the current browser session as Host.
5. Host shares the player registration URL or QR code.
6. Players open the registration URL and enter unique display names.
7. Host opens the Sentence Scrambler host page.
8. Host selects a sentence file.
9. Host sets the time limit for each sentence.
10. Host starts the next sentence round.
11. Server shuffles the sentence words for the round.
12. Players select words in order to reconstruct the sentence.
13. Each player submits the completed or confirmed incomplete sentence.
14. The round ends when all expected players submit, the timer expires, or the Host stops it.
15. Dashboard updates with results.
16. Dashboard ranks players and updates total scores.
17. Host starts the next sentence round.

## Sentence Scrambler Rules

- A round is based on one sentence.
- Sentences come from the selected `.txt` file, with one playable sentence per non-empty line.
- Sentence rounds progress sequentially through the selected file.
- The server splits the sentence into words.
- The server sends players the words in shuffled order.
- Players click one available word at a time.
- Clicked words move into the player's collected sentence.
- Players can return selected words to the available word list before submitting.
- Players can see their collected sentence while playing.
- A player can submit after selecting any word.
- If a player submits while words remain available, the app asks for confirmation.
- The result is the ordered list of selected words joined back into a sentence.
- Correctness is based on how many words are in the correct position.
- A fully correct answer has every word in the same position as the original sentence.
- Ranking is by correctness first, then shortest spent time.
- Total score accumulates across completed rounds.

## Screens

### Host Home Screen

Used by the Host. This is the first page shown when the Host opens the app.

Required UI:

- Player registration URL.
- QR code for the player registration URL when available.
- Registered players grid.
- Remove-player action.

Behavior:

- The Host browser session is saved automatically.
- The player registration URL points to `/register`.
- The registered players grid updates as players join or are removed.
- Removing a player removes that player from the in-memory player registry.

### Player Registration Screen

Used by Players joining from the shared registration URL.

Required UI:

- Name input.
- Join button.

Behavior:

- Name is required.
- The submitted name is trimmed before validation.
- Player names must be unique among connected players.
- Player name uniqueness is checked case-insensitively.
- Successful registration saves the browser session as a Player.
- Registered players wait until a round starts.

### Host Dashboard

Required UI:

- Shared LAN URL.
- Sentence file selector.
- Time limit input in seconds.
- Sentence progress label.
- Start/Stop round button.
- Current correct sentence.
- Player result grid.
- Round status.

Result grid columns:

- Player name.
- Submitted sentence.
- Time before submit.
- Score.
- Total score.

Behavior:

- The dashboard is available only to the Host session.
- The Host can start a round after selecting a sentence file.
- Start begins the next sentence from the selected file.
- Stop interrupts the active round and completes it early.
- The file selector and time limit are disabled while a round is active.
- Starting a new round clears previous round submissions.
- The dashboard shows the correct sentence after the round completes.
- The dashboard updates as players submit.
- The dashboard keeps Start disabled after the final sentence in the selected file completes.

### Player Game Screen

Required UI:

- Game title and rules.
- Timer.
- Shuffled word buttons.
- Collected sentence area.
- Submit button.
- Completion state.

Behavior:

- Before a round starts, player sees a waiting state.
- During a round, player sees shuffled words.
- Clicking a word removes it from available words and adds it to the collected sentence.
- Clicking a collected word returns it to the available words.
- The player can submit when at least one word is collected.
- If available words remain, submitting requires confirmation.
- After submission, the player cannot edit the result for that round.
- The visual gameplay UI is reusable and receives display values such as title and rules from the page.

## Data

### App Settings

Sentence Scrambler file location is configured in `appsettings.json`.

Recommended shape:

```json
{
  "Game": {
    "SentenceScramblerPath": "assets/sentence-scrambler"
  }
}
```

Rules:

- `Game:SentenceScramblerPath` defaults to `assets/sentence-scrambler` if omitted by startup code.
- `Game:SentenceScramblerPath` must resolve to a valid sentence file folder before games can be started.
- Player name matching uses the trimmed submitted name.

### Sentence Source

Sentence Scrambler sentences are stored in `.txt` files under the configured `SentenceScramblerPath` folder. Each non-empty line is one playable sentence.

Recommended shape:

```text
The quick brown fox jumps over the lazy dog
Never jump over the lazy dog quickly
```

Rules:

- `Game:SentenceScramblerPath` points to the folder containing sentence files.
- The folder must exist.
- The folder must contain at least one `.txt` file.
- Blank lines are ignored.
- Each `.txt` file must contain at least one non-empty line.
- The Host-entered time limit applies to the sentence round.

### In-Memory State

The server keeps all state in memory.

Required state:

- Connected Host session.
- Connected player names.
- Current round id.
- Selected sentence text.
- Selected sentence file.
- Current sentence index in the selected file.
- Shuffled words for the current round.
- Round start time.
- Round completion time.
- Expected player names for the round.
- Player submissions.
- Player spent time.
- Player correctness score.
- Player total scores.

Restart behavior:

- Restarting the server clears all connected users, rounds, and submissions.

## Local Network Behavior

- The server must listen on a local HTTP port.
- The app must provide a join URL usable by other devices on the same LAN.
- The URL should use the host computer's LAN IP address when possible.
- If LAN IP detection is unavailable, the app should still run on localhost for testing.

## Tech Stack 
- All code should be in src folder 
- App should use Blazor with InteractiveServer render mode. 
- Use MudBlazor for web UI components.
- Solution name is `HaveFun` has 2 projects
  - `HaveFun.Web` - UI
  - `HaveFun.Core` - backend
- Keep reusable player gameplay UI in `HaveFun.Web` components.
- Keep page-level routing, session checks, and service orchestration in pages.


## Acceptance Criteria

- The Host home page shows the player registration URL and registered players.
- Opening the app home page creates the Host session.
- Entering any other available name joins as a player.
- Duplicate player names are rejected.
- A Player can join from another browser tab or device using the shared URL.
- Host can select a sentence file and start a round.
- Host can stop an active round early.
- Player receives shuffled words and can reconstruct the sentence.
- Player can submit a completed sentence.
- Player can submit an incomplete sentence after confirmation.
- Dashboard shows the correct sentence, player sentence, time before submit, score, and total score.
- Ranking favors higher correctness, then lower time.
- Host can start the next sentence round without restarting the server.
- No database is required.
- Restarting the server resets all state.
- Sentence Scrambler player gameplay UI is reusable for future game pages.

## Future Ideas

- Add more party games.
- Add custom browser-created sentences.
- Add QR code for the shared join URL.
- Add undo while selecting words.
- Add persistent game history.
- Add teams.
- Add support for internet play through optional tunneling or deployment.
