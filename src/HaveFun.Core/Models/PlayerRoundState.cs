namespace HaveFun.Core;

public sealed record PlayerRoundState
{
    public required string PlayerName { get; init; }

    public required Guid RoundId { get; init; }

    public required IReadOnlyList<Tile> AvailableTiles { get; init; }

    public required IReadOnlyList<Tile> SelectedTiles { get; init; }

    public bool IsSubmitted { get; init; }

    public string? SubmittedSentence { get; init; }

    public DateTimeOffset? SubmittedAt { get; init; }

    public TimeSpan? SpentTime { get; init; }

    public string CollectedSentence => string.Join(' ', SelectedTiles.Select(tile => tile.Text));

    public bool CanSubmit => !IsSubmitted && SelectedTiles.Count > 0;
}
