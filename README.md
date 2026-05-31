# Have-Fun

Have-Fun is a local LAN party-game web app for friends playing from browsers on the same network.

## Main Features

- Host-controlled Sentence Scrambler rounds.
- Players join from phones, tablets, or computers using a local URL or QR code.
- Timed sentence rounds with shuffled words and click-to-build answers.
- Live host dashboard with player submissions, scores, and total scores.
- In-memory game state with no accounts, database, cloud hosting, or public internet requirement.

## Implementation

Have-Fun is a Blazor Web App using server-side interactive rendering. Pages are rendered for two local roles:

- Host pages create and control rounds, share the join link, and show live results.
- Player pages handle registration, waiting for a round, playing, and submission state.

The current browser role is kept in session storage, while game state stays in server memory.
