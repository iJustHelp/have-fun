namespace HaveFun.Core;

public sealed record PlayerTotalScore
{
    public required string PlayerName { get; init; }

    public required int Score { get; init; }

    public required int TotalScore { get; init; }

    public string ScoreDisplay => $"{Score} / {TotalScore}";
}
