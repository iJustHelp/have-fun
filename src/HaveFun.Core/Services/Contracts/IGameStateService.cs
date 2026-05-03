namespace HaveFun.Core;

public interface IGameStateService
{
    event Action<CurrentRound>? CurrentRoundChanged;

    CurrentRound? CurrentRound { get; }

    CurrentRound StartRound(SentenceDefinition sentence);
}
