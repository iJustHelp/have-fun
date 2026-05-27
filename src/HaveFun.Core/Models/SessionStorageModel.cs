namespace HaveFun.Core;

public sealed record SessionStorageModel
{
    public required string Name { get; init; }

    public required Role Role { get; init; }
}
