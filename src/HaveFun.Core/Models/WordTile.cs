namespace HaveFun.Core;

public sealed record WordTile
{
    public required Guid Id { get; init; }

    public required string Text { get; init; }
}
