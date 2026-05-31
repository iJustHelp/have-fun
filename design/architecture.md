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

## Application Layers

```mermaid
flowchart TB
    subgraph Web["HaveFun.Web"]
        Program["Program.cs\nDI, config, middleware,\nRazor components"]
        Pages["Pages\nHome, Register,\nWaitingRoom, HostSentenceScrambler,\nPlayerSentenceScrambler"]
        Components["Components\nPlayerGameBoard"]
        Layouts["Layouts\nMain, Register,\nNav, Reconnect"]
        Assets["Web assets\napp.css, images,\nsentence-scrambler txt files"]
    end

    subgraph Core["HaveFun.Core"]
        Models["Models\nPlayerSession, CurrentRound,\nPlayerRoundState, Results,\nScores"]
        Contracts["Service contracts\nIGameStateService,\nIPlayerRegistryService,\nISessionStorageService,\nIUrlService, IQrCodeService,\nISentenceFileService"]
        Services["Services\nGameStateService,\nPlayerRegistryService,\nSessionStorageService,\nUrlService,\nQrCodeService,\nSentenceFileService"]
    end

    Program --> Pages
    Program --> Components
    Program --> Layouts
    Program --> Services
    Pages --> Components
    Pages --> Contracts
    Components --> Models
    Services --> Models
    Services --> Assets
```

## Runtime Services

```mermaid
flowchart LR
    DI["ASP.NET Core DI"]
    GameState["IGameStateService\nSingleton\ncurrent round, player round state,\nresults, total scores"]
    Registry["IPlayerRegistryService\nSingleton\nregistered players"]
    SentenceFiles["ISentenceFileService\nSingleton\nloads configured .txt files"]
    Urls["IUrlService\nSingleton\nLAN/register URL"]
    Qr["IQrCodeService\nSingleton\nregistration QR code"]
    Session["ISessionStorageService\nScoped\nbrowser sessionStorage interop"]

    DI --> GameState
    DI --> Registry
    DI --> SentenceFiles
    DI --> Urls
    DI --> Qr
    DI --> Session
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
