namespace HaveFun.Core;

public sealed record PlayerRoundState
{
    public required string PlayerName { get; init; }

    public required Guid RoundId { get; init; }

    public required IReadOnlyList<WordTile> AvailableWords { get; init; }

    public required IReadOnlyList<WordTile> CollectedWords { get; init; }

    public bool IsSubmitted { get; init; }

    public string? SubmittedSentence { get; init; }

    public DateTimeOffset? SubmittedAt { get; init; }

    public TimeSpan? SpentTime { get; init; }

    public string CollectedSentence => string.Join(' ', CollectedWords.Select(sentence => sentence.Text));

    public bool CanSubmit => !IsSubmitted && CollectedWords.Count > 0;
}
