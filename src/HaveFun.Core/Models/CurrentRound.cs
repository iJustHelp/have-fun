namespace HaveFun.Core;

public sealed record CurrentRound
{
    public required Guid Id { get; init; }

    public required string SentenceText { get; init; }

    public required int TimeLimitInSeconds { get; init; }

    public required IReadOnlyList<string> OriginalSentences { get; init; }

    public required IReadOnlyList<string> ShuffledSentences { get; init; }

    public required IReadOnlyList<string> ExpectedPlayerNames { get; init; }

    public required RoundStatus Status { get; init; }

    public DateTimeOffset? StartedAt { get; init; }

    public DateTimeOffset? CompletedAt { get; init; }

    public bool IsCompleted => Status == RoundStatus.Completed;
}
