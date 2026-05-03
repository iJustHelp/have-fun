namespace HaveFun.Core;

public interface IGameStateService
{
    CurrentRound? CurrentRound { get; }

    CurrentRound StartRound(SentenceDefinition sentence);
}
