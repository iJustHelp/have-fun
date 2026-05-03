namespace HaveFun.Core.Sentences;

public sealed record SentenceDefinition
{
    public string Text { get; init; } = string.Empty;

    public int TimeLimitInSeconds { get; init; }
}
