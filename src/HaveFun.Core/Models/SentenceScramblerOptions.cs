namespace HaveFun.Core;

public sealed record SentenceScramblerOptions
{
    public string SentenceScramblerPath { get; init; } = Path.Combine("assets", "sentence-scrambler");
}
