# Have-Fun Architecture

Have-Fun is a local LAN party-game Blazor Web App. The web host serves the UI, keeps game state in memory, and lets the Host and Players interact through browser sessions on the same network.

## System Context

```mermaid
flowchart LR
    HostBrowser["Host Browser"]
    PlayerBrowser["Player Browsers"]
    Core["HaveFun.Core\nIn-memory models and services"]
    SessionStorage["Browser sessionStorage\nrole + display name"]
    SentenceFiles["Sentence files\nassets/sentence-scrambler/*.txt"]

    subgraph WebApp["HaveFun.Web\nASP.NET Core Blazor Web App\nInteractive Server"]
        subgraph HostPages["Host pages"]
            HomePage["Home\n/"]
            HostGamePage["Host Sentence Scrambler\n/host-sentence-scrambler"]
        end

        subgraph PlayerPages["Player pages"]
            RegisterPage["Register\n/register"]
            WaitingRoomPage["Waiting Room\n/waiting-room"]
            PlayerGamePage["Player Sentence Scrambler\n/player-sentence-scrambler"]
        end
    end

    HostBrowser <--> HomePage
    HostBrowser <--> HostGamePage
    PlayerBrowser <--> RegisterPage
    PlayerBrowser <--> WaitingRoomPage
    PlayerBrowser <--> PlayerGamePage
    HostBrowser <--> SessionStorage
    PlayerBrowser <--> SessionStorage
    HomePage --> Core
    HostGamePage --> Core
    RegisterPage --> Core
    WaitingRoomPage --> Core
    PlayerGamePage --> Core
    Core --> SentenceFiles
```

## Entity Relationships

These are in-memory records and service-owned collections, not database tables. Relationships that use player names are logical links, not foreign keys.

```mermaid
erDiagram
    SESSION_STORAGE_MODEL }o--o| PLAYER_SESSION : "identifies player browser"
    SENTENCE_FILE_OPTION ||--o{ SENTENCE_DEFINITION : "loads lines as"
    SENTENCE_DEFINITION ||--o{ CURRENT_ROUND : "starts"
    CURRENT_ROUND ||--o{ PLAYER_ROUND_STATE : "tracks"
    PLAYER_SESSION ||--o{ PLAYER_ROUND_STATE : "plays"
    PLAYER_ROUND_STATE ||--o{ TILE : "available tiles"
    PLAYER_ROUND_STATE ||--o{ TILE : "selected tiles"
    CURRENT_ROUND ||--o| ROUND_RESULTS : "produces"
    ROUND_RESULTS ||--o{ PLAYER_RESULT : "contains"
    PLAYER_SESSION ||--o{ PLAYER_TOTAL_SCORE : "accumulates"
```

## Host Flow

```mermaid
sequenceDiagram
    participant H as Host Browser
    participant W as HaveFun.Web
    participant S as SessionStorageService
    participant G as GameStateService
    participant F as SentenceFileService

    H->>W: Open /
    W->>S: Save Host session
    W->>H: Show register URL, QR code, players

    H->>W: Open /host-sentence-scrambler
    W->>F: List .txt sentence files
    H->>W: Select file and start round
    W->>F: Load sentence lines
    W->>G: StartRound(sentence, expected players)
    G-->>W: CurrentRoundChanged

    G-->>W: Results updated
    W-->>H: Refresh players grid

    G-->>W: Complete round on all submissions
    H->>W: Or Stop active round
    W->>G: CompleteCurrentRound
    W-->>H: Show correct sentence and scores
```

## Player Flow

```mermaid
sequenceDiagram
    participant P as Player Browser
    participant W as HaveFun.Web
    participant S as SessionStorageService
    participant R as PlayerRegistryService
    participant G as GameStateService

    P->>W: Open /register
    P->>W: Submit display name
    W->>R: Register player
    W->>S: Save Player session
    W->>P: Navigate to waiting room

    G-->>W: CurrentRoundChanged
    W-->>P: Waiting room navigates to player game

    P->>W: Select/return draft tiles in PlayerGameBoard
    W-->>P: Refresh local available and selected tiles

    P->>W: Submit selected tiles
    W->>G: SubmitPlayerRound(player, selectedTiles)
    G-->>W: Results updated
    W-->>P: Show submitted or completed state
```

## Host and Player Page Communication

Host and player pages do not send messages directly to each other. Each browser has its own Blazor Server circuit connected to `HaveFun.Web`, and the pages coordinate through singleton in-memory services in `HaveFun.Core`.

The host page writes round changes into `GameStateService`. Player pages read that same service state and subscribe to service events. Player pages keep draft tile selection inside `PlayerGameBoard`, submit selected tiles to `GameStateService`, and the host page refreshes its result grid when the service raises player-state events.

```mermaid
sequenceDiagram
    participant H as Host Page
    participant G as GameStateService
    participant P as Player Page

    H->>G: StartRound(...)
    G-->>H: CurrentRoundChanged
    G-->>P: CurrentRoundChanged
    P->>G: GetOrCreatePlayerRoundState(player)
    G-->>P: Initial AvailableTiles

    P-->>P: Select/return draft tiles in PlayerGameBoard

    P->>G: SubmitPlayerRound(player, selectedTiles)
    G-->>P: PlayerRoundStateChanged
    G-->>H: PlayerRoundStateChanged
    H->>G: GetSubmittedPlayerRoundStates()
    H-->>H: Recalculate game-page results

    H->>G: CompleteCurrentRound()
    G-->>H: CurrentRoundChanged
    G-->>P: CurrentRoundChanged
```

Communication rules:

- `GameStateService` is the shared source of truth for the active round, submitted player tile state, submissions, and total scores.
- Host pages start/stop rounds and calculate game-specific current-round results for display.
- Player pages select/return draft tiles inside `PlayerGameBoard`; only submitted selected tiles are sent to `GameStateService`.
- `CurrentRoundChanged` tells host/player pages that a round started or completed.
- `PlayerRoundStateChanged` tells host/player pages that a player submitted tiles.
- `SelectedTiles` is the submitted answer; display text is derived from selected tile text when needed.
- Browser `sessionStorage` is only used to remember role and display name; it is not used for host/player messaging.

## State Boundaries

- Server memory stores registered players, active round state, player submissions, results, and total scores.
- Browser `sessionStorage` stores only the current browser's role and display name.
- Sentence content is local file data from `Game:SentenceScramblerPath`.
- Restarting the server clears in-memory players, rounds, submissions, and scores.
- No database, accounts, cloud service, or persistent game history is part of V1.
