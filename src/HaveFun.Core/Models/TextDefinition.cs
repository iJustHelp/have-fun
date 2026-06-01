namespace HaveFun.Core;

public sealed record TextDefinition
{
    public string Text { get; init; } = string.Empty;

    public int TimeLimitInSeconds { get; init; }
}
