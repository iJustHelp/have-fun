namespace HaveFun.Core;

public sealed class GameStateService : IGameStateService
{
    private readonly object syncRoot = new();
    private CurrentRound? currentRound;

    public event Action<CurrentRound>? CurrentRoundChanged;

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

        var originalWords = SplitWords(sentence.Text);
        var shuffledWords = ShuffleWords(originalWords);
        var round = new CurrentRound
        {
            Id = Guid.NewGuid(),
            SentenceText = sentence.Text,
            TimeLimitInSeconds = sentence.TimeLimitInSeconds,
            OriginalWords = originalWords,
            ShuffledWords = shuffledWords,
            Status = RoundStatus.Started,
            StartedAt = DateTimeOffset.UtcNow
        };

        lock (syncRoot)
        {
            currentRound = round;
        }

        CurrentRoundChanged?.Invoke(round);

        return round;
    }

    private static IReadOnlyList<string> SplitWords(string sentenceText)
    {
        return sentenceText
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();
    }

    private static IReadOnlyList<string> ShuffleWords(IReadOnlyList<string> words)
    {
        var shuffledWords = words.ToArray();

        for (var index = shuffledWords.Length - 1; index > 0; index--)
        {
            var swapIndex = Random.Shared.Next(index + 1);
            (shuffledWords[index], shuffledWords[swapIndex]) = (shuffledWords[swapIndex], shuffledWords[index]);
        }

        return shuffledWords;
    }
}
