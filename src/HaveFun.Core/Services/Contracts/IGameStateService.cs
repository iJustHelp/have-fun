namespace HaveFun.Core;

public interface IGameStateService
{
    event Action<CurrentRound>? CurrentRoundChanged;

    event Action<PlayerRoundState>? PlayerRoundStateChanged;

    CurrentRound? CurrentRound { get; }

    CurrentRound StartRound(SentenceDefinition sentence, IReadOnlyList<string> expectedPlayerNames);

    CurrentRound StartRound(
        SentenceDefinition sentence,
        IReadOnlyList<string> expectedPlayerNames,
        Func<CurrentRound, IReadOnlyList<Tile>> createAvailableTiles,
        Func<CurrentRound, string, int> calculateScore);

    CurrentRound? CompleteCurrentRound();

    PlayerRoundState? GetPlayerRoundState(string playerName);

    PlayerRoundState? GetOrCreatePlayerRoundState(string playerName);

    PlayerRoundState? SelectTile(string playerName, Guid wordId);

    PlayerRoundState? ReturnTile(string playerName, Guid wordId);

    PlayerRoundState? SubmitPlayerRound(string playerName);

    IReadOnlyList<PlayerRoundState> GetSubmittedPlayerRoundStates();

    IReadOnlyList<PlayerTotalScore> GetPlayerTotalScores();
}
