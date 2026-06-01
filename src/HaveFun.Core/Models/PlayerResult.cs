namespace HaveFun.Core;

public sealed record PlayerResult
{
    public required int Rank { get; init; }

    public required string PlayerName { get; init; }

    public required IReadOnlyList<Tile> SelectedTiles { get; init; }

    public required int CorrectnessCount { get; init; }

    public required int TotalSentenceCount { get; init; }

    public required TimeSpan SpentTime { get; init; }

    public required DateTimeOffset SubmittedAt { get; init; }

    public string CorrectnessDisplay => $"{CorrectnessCount} / {TotalSentenceCount}";
}
