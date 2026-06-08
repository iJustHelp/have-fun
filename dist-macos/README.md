# Have Fun Host Guide

This folder contains the Windows version of Have Fun, a local LAN party-game app.

## Run the App

Double-click:

```bat
app.bat
```

This starts the app, opens the host computer's browser, and uses port `3333`.

> You may need to run `app.bat` as an administrator from `cmd`, especially if Windows Firewall blocks other devices from connecting.

The browser opened by the launcher becomes the Host automatically. There are no accounts, passwords, or cloud services.

## Invite Players

The home page shows a player registration URL and QR code.

Players should:

1. Join the same Wi-Fi/LAN as the host computer.
2. Open the displayed URL or scan the QR code.
3. Enter a display name.
4. Wait for the Host to start a round.
5. Read the game rules at the top of the page.

If players cannot open the link, check:

- They are on the same Wi-Fi/LAN as the host computer.
- Windows Firewall allows the app or port `3333`.
- The network is not blocking device-to-device connections.

## Start a Game

On the host computer:

1. Start the app.
2. Wait for players to register.
3. Open a game from the app menu.
4. Select a game file.
5. Set the round time limit.
6. Click **Start**.

The Host can watch results, stop the active round, and start the next prompt from the selected file.

## Sentence Scrambler

Open `Sentence Scramble` from the app menu. Players arrange shuffled words into the original sentence.

Files are stored here:

```text
assets\sentence-scrambler
```

Each `.txt` file is one selectable sentence set. Put one playable sentence on each non-empty line.

```text
The quick brown fox jumps over the lazy dog.
Never jump over the lazy dog quickly
```

> **Tip:** You can capitalize the first letter and/or add a period at the end of the sentence to make the game easier.

## Word Scrambler

Open `Word Scrambler` from the app menu. Players arrange shuffled letters into the original word.

Files are stored here:

```text
assets\word-scrambler
```

Each `.txt` file is one selectable word set. Put one playable word on each non-empty line.

```text
Planet
browser
Keyboard
```

> **Tip:** You can capitalize the first letter to make the game easier.

## Formula Scrambler

Open `Formula Scrambler` from the app menu. Players arrange shuffled formula characters into the original equation.

Files are stored here:

```text
assets\formula-scrambler
```

Each `.txt` file is one selectable formula set. Put one playable formula on each non-empty line.

```text
2 + 3 = 5
4 * 6 = 24
12 /3 =4
```

> **Tip:** Formulas are split on spaces. All characters without spaces appear together in one tile, making the game easier.

Restart the app after adding or changing files.

## Restart Behavior

Have Fun keeps game state in memory. Restarting the app clears registered players, active rounds, submissions, and scores.
