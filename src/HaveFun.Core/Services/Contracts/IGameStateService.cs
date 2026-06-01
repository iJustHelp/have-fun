namespace HaveFun.Core;

public interface IGameStateService
{
    event Action<CurrentRound>? CurrentRoundChanged;

    event Action<PlayerRoundState>? PlayerRoundStateChanged;

    CurrentRound? CurrentRound { get; }

    CurrentRound StartRound(
        TextDefinition sentence,
        IReadOnlyList<string> expectedPlayerNames,
        Func<CurrentRound, IReadOnlyList<Tile>> createAvailableTiles,
        Func<CurrentRound, IReadOnlyList<Tile>, int> calculateScore);

    CurrentRound? CompleteCurrentRound();

    PlayerRoundState? GetPlayerRoundState(string playerName);

    PlayerRoundState? GetOrCreatePlayerRoundState(string playerName);

    PlayerRoundState? SubmitPlayerRound(string playerName, IReadOnlyList<Tile> selectedTiles);

    IReadOnlyList<PlayerRoundState> GetSubmittedPlayerRoundStates();

    IReadOnlyList<PlayerTotalScore> GetPlayerTotalScores();
}
