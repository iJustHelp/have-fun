using HaveFun.Core;

namespace HaveFun.Web;

public sealed record StoredUserSession
{
    public required string Name { get; init; }

    public required JoinRole Role { get; init; }
}
