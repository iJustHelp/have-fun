namespace HaveFun.Core;

public sealed class GameState : IGameState
{
    private readonly object syncRoot = new();
    private CurrentRound? currentRound;

    public CurrentRound? CurrentRound
    {
        get
        {
            lock (syncRoot)
            {
                return currentRound;
            }
        }
    }

    public CurrentRound StartRound(SentenceDefinition sentence)
    {
        if (string.IsNullOrWhiteSpace(sentence.Text))
        {
            throw new ArgumentException("Sentence text is required.", nameof(sentence));
        }

        if (sentence.TimeLimitInSeconds <= 0)
        {
            throw new ArgumentException("Sentence time limit must be greater than zero.", nameof(sentence));
        }

        var round = new CurrentRound
        {
            Id = Guid.NewGuid(),
            SentenceText = sentence.Text,
            TimeLimitInSeconds = sentence.TimeLimitInSeconds,
            Status = RoundStatus.Started,
            StartedAt = DateTimeOffset.UtcNow
        };

        lock (syncRoot)
        {
            currentRound = round;
        }

        return round;
    }
}
