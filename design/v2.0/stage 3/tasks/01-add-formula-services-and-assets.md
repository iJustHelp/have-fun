# Task 01: Add Formula Services and Assets

## Goal

Add the Formula Scrambler service registrations, configuration path, and starter formula files needed before UI pages can load rounds.

## Work

- Add `FormulaScramblerGameStateService : GameStateService` in `HaveFun.Core`.
- Add `FormulaScramblerFileService : FileService` in `HaveFun.Core`.
- Register `FormulaScramblerGameStateService` as a singleton in core services.
- Add `Game:FormulaScramblerPath` to app settings.
- Resolve the formula path in `Program.cs` with default `assets/formula-scrambler`.
- Register `FormulaScramblerFileService` as a singleton using the resolved path.
- Add `src/HaveFun.Web/assets/formula-scrambler` with starter formula text files.
- Store one formula per non-empty line.

## Done Criteria

- The web app can resolve `FormulaScramblerGameStateService` and `FormulaScramblerFileService` from DI.
- Formula Scrambler has its own isolated in-memory game state.
- Formula files are loaded from `Game:FormulaScramblerPath`.
- Existing Sentence Scrambler and Word Scrambler service registrations still work.

## Verification

- Run `dotnet build .\HaveFun.sln --no-restore` from `src`.
- Confirm app settings include `FormulaScramblerPath`.
- Confirm the formula asset folder contains at least one playable formula file.
