namespace HaveFun.Core;

public sealed record SentenceTile
{
    public required Guid Id { get; init; }

    public required string Text { get; init; }
}
