# Task 03: Build Word Selection UI

## Goal

Let players click available words and build a collected sentence.

## Work

- Update `Pages/Player.razor` and `Pages/Player.razor.cs`.
- Render available words as MudBlazor buttons or chips.
- Bind each available word to a select action.
- Remove clicked word instances from available words.
- Append clicked word instances to collected words.
- Show the collected sentence area while playing.
- Preserve the Stage 4 timer and round status UI.

## Done Criteria

- Player can click available words.
- Clicked words move out of available words.
- Clicked words appear in collected sentence order.
- Duplicate words are handled as separate selectable instances.

## Verification

- Build the solution from `src`.
- Start a round, click words, and confirm the collected sentence updates.
