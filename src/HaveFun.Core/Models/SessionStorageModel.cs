namespace HaveFun.Core;

public sealed record SessionStorageModel
{
    public required string Name { get; init; }

    public required UserRole Role { get; init; }
}
