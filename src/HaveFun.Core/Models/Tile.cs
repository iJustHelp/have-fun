namespace HaveFun.Core;

public sealed record Tile
{
    public required Guid Id { get; init; }

    public required string Text { get; init; }
}
