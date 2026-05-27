namespace HaveFun.Core;

public sealed record PlayerRoundState
{
    public required string PlayerName { get; init; }

    public required Guid RoundId { get; init; }

    public required IReadOnlyList<SentenceTile> AvailableSentences { get; init; }

    public required IReadOnlyList<SentenceTile> CollectedSentences { get; init; }

    public bool IsSubmitted { get; init; }

    public string? SubmittedSentence { get; init; }

    public DateTimeOffset? SubmittedAt { get; init; }

    public TimeSpan? SpentTime { get; init; }

    public string CollectedSentence => string.Join(' ', CollectedSentences.Select(sentence => sentence.Text));

    public bool CanSubmit => !IsSubmitted && AvailableSentences.Count == 0 && CollectedSentences.Count > 0;
}
