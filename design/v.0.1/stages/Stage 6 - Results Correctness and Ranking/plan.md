# Stage 6 Plan: Results, Correctness, and Ranking

## Objective

Finish the competitive Master Dashboard for a round. The server should score submitted player sentences, rank players by correctness and spent time, and show the Game Master a clear result grid.

## Context

Stage 6 builds on the current app foundation:

- `HaveFun.Web` is a Blazor Web App with `InteractiveServer` render mode.
- MudBlazor is used for visible web UI.
- Browser session storage stores only the current user's `name` and `role`.
- The Master Dashboard is available only to session role `Master`.
- `IGameStateService` stores current round and player round states in server memory.
- Current round state includes original words, shuffled words, start time, time limit, and status.
- Player submissions include player name, submitted sentence, submitted timestamp, and spent time.
- Dashboard already shows submission progress.
- Services and interfaces use the `Service` suffix.

## Scope

Included:

- Calculate correctness for submitted player sentences.
- Correctness is based on words in the correct position.
- Show current correct sentence on the Master Dashboard.
- Show player submitted sentence.
- Show correctness score.
- Show spent time.
- Show rank.
- Sort/rank by highest correctness first, then shortest spent time.
- Update dashboard results when players submit.

Not included:

- Persisted result history.
- Cross-round history.
- Player-side leaderboard polish.
- Teams.
- Custom sentence editing.
- Database, persistence, cloud services, accounts, passwords, or real authentication.

## Architecture

Use `HaveFun.Core` for correctness and ranking logic.

Recommended Core additions:

- `PlayerResult` model:
  - Rank.
  - Player name.
  - Submitted sentence.
  - Correctness count.
  - Total word count.
  - Correctness display or percentage if useful.
  - Spent time.
  - Submitted timestamp.
- `RoundResults` model if useful:
  - Round id.
  - Correct sentence.
  - Results list.
- Extend `IGameStateService` with:
  - Get ranked results for the current round.
  - Notify when results change if existing player submission notification is not enough.
- Add a correctness helper or service method in Core.

Use `HaveFun.Web` for the Master Dashboard result grid.

Recommended Web updates:

- Update `Pages/Dashboard.razor` and `Pages/Dashboard.razor.cs`.
- Render correct sentence.
- Render MudBlazor result table/grid.
- Subscribe to result/submission changes and refresh the table.

## Correctness Rules

Rules:

- Split/compare submitted words against current round original words.
- Correctness is the count of positions where submitted word equals original word.
- A fully correct answer has correctness equal to total word count.
- Comparisons should use exact word text from the round.
- Stage 6 does not need fuzzy matching, punctuation normalization, spelling tolerance, or case normalization beyond the existing word tokens unless the spec changes.

Examples:

- Correct: `The quick brown fox`
- Submitted: `The brown quick fox`
- Correctness: 2 of 4 (`The` and `fox` are in the correct positions).

## Ranking Rules

Rules:

- Rank submitted players only.
- Sort by highest correctness first.
- Break ties by shortest spent time.
- If correctness and spent time tie exactly, keep a stable deterministic order such as player name.
- Rank numbers should be visible to the Game Master.

Out of scope:

- Ranking players who have not submitted.
- Multi-round aggregate ranking.
- Persistent leaderboard.

## Dashboard UI

Required UI:

- Correct sentence.
- Result grid/table.
- Player name column.
- Submitted sentence column.
- Correctness column.
- Spent time column.
- Rank column.

Recommended UI:

- Use a MudBlazor table.
- Show an empty state before submissions.
- Keep submission progress from Stage 5.
- Highlight full correctness if simple to do without visual clutter.

Behavior:

- Results update as players submit.
- Starting a new round clears previous results.
- Dashboard remains blocked for non-master sessions.

## Player Impact

Player gameplay can remain mostly unchanged.

Optional Stage 6 player updates:

- After submission, continue showing submitted sentence and spent time.
- Do not reveal correctness on the player screen unless desired later.

## Done Criteria

Stage 6 is complete when:

- Server calculates correctness by word position.
- Server returns ranked current-round results.
- Dashboard shows correct sentence.
- Dashboard shows each submitted player's sentence.
- Dashboard shows correctness.
- Dashboard shows spent time.
- Dashboard shows rank.
- Ranking sorts by highest correctness first, then shortest spent time.
- Dashboard updates as players submit.
- Starting a new round clears previous results.
- No database, cloud service, accounts, passwords, real auth, or persistent leaderboard is added.

## Test Plan

Manual checks:

- Build from `src` with `dotnet build .\HaveFun.sln`.
- Run `HaveFun.Web`.
- Join as master and start a round.
- Join as at least two players.
- Submit a fully correct sentence with one player.
- Submit a partially correct sentence with another player.
- Confirm dashboard shows correct sentence.
- Confirm dashboard shows both submitted sentences.
- Confirm correctness values are accurate.
- Confirm ranking favors higher correctness.
- Submit equal-correctness results and confirm shorter spent time ranks higher.
- Start a new round and confirm old results clear.

Failure checks:

- Open dashboard as player and confirm results are blocked.
- Confirm no unsubmitted player is ranked.
- Confirm app still starts without any database or cloud dependency.

## Handoff to Stage 7

Stage 7 can build on:

- Ranked results for current round.
- Correct sentence display.
- Submitted sentence display.
- Correctness and spent time per submitted player.
- Result reset when a new round starts.
