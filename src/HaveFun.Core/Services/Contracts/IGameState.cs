namespace HaveFun.Core;

public interface IGameState
{
    CurrentRound? CurrentRound { get; }

    CurrentRound StartRound(SentenceDefinition sentence);
}
