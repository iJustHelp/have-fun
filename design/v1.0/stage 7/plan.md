# Stage 7 Player Sentence Scrambler Plan

## Summary
Implement Stage 7 by improving the player gameplay page: allow players to submit a partial collected sentence, let players move collected words back to the available list, and restyle the player timer as a `MudChip` matching the host timer style.

## Key Changes
- Update core submit rules so `PlayerRoundState.CanSubmit` is true when the player has at least one collected word and has not submitted, even if available words remain.
- Add a new game-state operation to move a collected word back to available words by tile ID.
  - Add `ReturnSentence(string playerName, Guid sentenceId)` to `IGameStateService`.
  - Implement it in `GameStateService` by removing the matching tile from `CollectedSentences` and appending it to `AvailableSentences`.
  - Do nothing if the player is submitted, the round is missing, the player name is invalid, or the tile is not in collected words.
- Update `/player-sentence-scrambler`.
  - Make collected `MudChip`s clickable before submission.
  - Clicking a collected chip calls the new return operation and refreshes local `PlayerRoundState`.
  - Keep submitted players locked as today.
  - Keep Submit disabled only when the player has collected no words, has already submitted, or the current round is not started.
  - When Submit is clicked with words still available, show a confirmation popup before submitting the incomplete sentence.
  - Submit complete collected sentences immediately without confirmation.
- Replace the player timer paper/card content with a `MudChip` using the timer icon and `@RemainingTimeText remaining`, visually consistent with `HostSentenceScrambler`.

## Public Interfaces
- `IGameStateService` gains:
  - `PlayerRoundState? ReturnSentence(string playerName, Guid sentenceId);`
- `PlayerRoundState.CanSubmit` changes meaning:
  - From “all words selected and at least one collected”
  - To “not submitted and at least one word collected”

## Test Plan
- Build with `dotnet build .\HaveFun.sln --no-restore -p:UseSharedCompilation=false -p:DebugType=none -p:OutputPath=.\artifacts\verify\`.
- Start a round, select one word, verify Submit becomes enabled.
- Submit with only one collected word, verify a confirmation popup appears.
- Cancel the incomplete-submit popup, verify the player remains unlocked and can keep editing.
- Confirm the incomplete-submit popup, verify the player locks and host receives the partial submitted sentence.
- Select all words and submit, verify no confirmation popup appears.
- Select multiple words, click a collected chip, verify it moves back to available words.
- Verify returned words can be selected again.
- Verify clicking collected chips after submission is not possible.
- Verify timer displays as a `MudChip` and still updates once per second.
- Verify existing host completion behavior still works when all expected players submit partial answers.

## Assumptions
- “Not finished sentence” means partial submissions are allowed only after at least one word is collected; empty submissions remain disabled.
- The incomplete-submit confirmation appears only when collected words are fewer than the full sentence and at least one word remains available.
- Returning a collected word appends it to the end of the available list.
- No database or persistence is added; all state remains in memory.
