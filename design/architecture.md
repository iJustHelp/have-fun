# Have-Fun Architecture

Have-Fun is a local LAN party-game Blazor Web App. The web host serves the UI, keeps game state in memory, and lets the Host and Players interact through browser sessions on the same network.

## System Context

```mermaid
flowchart LR
    HostBrowser["Host Browser\n/ and /host-sentence-scrambler"]
    PlayerBrowser["Player Browsers\n/register, /waiting-room,\n/player-sentence-scrambler"]
    WebApp["HaveFun.Web\nASP.NET Core Blazor Web App\nInteractive Server"]
    Core["HaveFun.Core\nIn-memory models and services"]
    SessionStorage["Browser sessionStorage\nrole + display name"]
    SentenceFiles["Sentence files\nassets/sentence-scrambler/*.txt"]

    HostBrowser <--> WebApp
    PlayerBrowser <--> WebApp
    HostBrowser <--> SessionStorage
    PlayerBrowser <--> SessionStorage
    WebApp --> Core
    Core --> SentenceFiles
```

## Entity Relationships

These are in-memory records and service-owned collections, not database tables. Relationships that use player names are logical links, not foreign keys.

```mermaid
erDiagram
    PLAYER_SESSION {
        Guid Id
        string DisplayName
        DateTimeOffset JoinedAt
    }

    SESSION_STORAGE_MODEL {
        string Name
        Role Role
    }

    SENTENCE_FILE_OPTION {
        string FileName
    }

    SENTENCE_DEFINITION {
        string Text
        int TimeLimitInSeconds
    }

    CURRENT_ROUND {
        Guid Id
        string SentenceText
        int TimeLimitInSeconds
        string OriginalSentences
        string ShuffledSentences
        string ExpectedPlayerNames
        RoundStatus Status
        DateTimeOffset StartedAt
        DateTimeOffset CompletedAt
        bool IsCompleted
    }

    PLAYER_ROUND_STATE {
        string PlayerName
        Guid RoundId
        bool IsSubmitted
        string SubmittedSentence
        DateTimeOffset SubmittedAt
        TimeSpan SpentTime
        string CollectedSentence
        bool CanSubmit
    }

    WORD_TILE {
        Guid Id
        string Text
    }

    ROUND_RESULTS {
        Guid RoundId
        string CorrectSentence
    }

    PLAYER_RESULT {
        int Rank
        string PlayerName
        string SubmittedSentence
        int CorrectnessCount
        int TotalSentenceCount
        TimeSpan SpentTime
        DateTimeOffset SubmittedAt
        string CorrectnessDisplay
    }

    PLAYER_TOTAL_SCORE {
        string PlayerName
        int Score
        int TotalScore
        string ScoreDisplay
    }

    SESSION_STORAGE_MODEL }o--o| PLAYER_SESSION : "identifies player browser"
    SENTENCE_FILE_OPTION ||--o{ SENTENCE_DEFINITION : "loads lines as"
    SENTENCE_DEFINITION ||--o{ CURRENT_ROUND : "starts"
    CURRENT_ROUND ||--o{ PLAYER_ROUND_STATE : "tracks"
    PLAYER_SESSION ||--o{ PLAYER_ROUND_STATE : "plays"
    PLAYER_ROUND_STATE ||--o{ WORD_TILE : "available words"
    PLAYER_ROUND_STATE ||--o{ WORD_TILE : "collected words"
    CURRENT_ROUND ||--o| ROUND_RESULTS : "produces"
    ROUND_RESULTS ||--o{ PLAYER_RESULT : "contains"
    PLAYER_SESSION ||--o{ PLAYER_TOTAL_SCORE : "accumulates"
```

## Main Flow

```mermaid
sequenceDiagram
    participant H as Host Browser
    participant W as HaveFun.Web
    participant S as SessionStorageService
    participant P as Player Browser
    participant R as PlayerRegistryService
    participant G as GameStateService
    participant F as SentenceFileService

    H->>W: Open /
    W->>S: Save Host session
    W->>H: Show register URL, QR code, players

    P->>W: Open /register
    P->>W: Submit display name
    W->>R: Register player
    W->>S: Save Player session
    W->>P: Navigate to waiting room

    H->>W: Open /host-sentence-scrambler
    W->>F: List .txt sentence files
    H->>W: Select file and start round
    W->>F: Load sentence lines
    W->>G: StartRound(sentence, expected players)
    G-->>W: CurrentRoundChanged
    W-->>P: Waiting room navigates to player game

    P->>W: Select/return words in PlayerGameBoard
    W->>G: SelectWord/ReturnWord
    G-->>W: PlayerRoundStateChanged

    P->>W: Submit sentence
    W->>G: SubmitPlayerRound
    G-->>W: Results updated
    W-->>H: Host grid refreshes

    G-->>W: Complete round on all submissions
    H->>W: Or Stop active round
    W->>G: CompleteCurrentRound
    W-->>H: Show correct sentence and scores
```

## State Boundaries

- Server memory stores registered players, active round state, player submissions, results, and total scores.
- Browser `sessionStorage` stores only the current browser's role and display name.
- Sentence content is local file data from `Game:SentenceScramblerPath`.
- Restarting the server clears in-memory players, rounds, submissions, and scores.
- No database, accounts, cloud service, or persistent game history is part of V1.
