namespace HaveFun.Core;

public sealed record JoinResult
{
    private JoinResult(
        bool isSuccess,
        UserRole? role,
        Guid? playerId,
        string displayName,
        string? validationError)
    {
        IsSuccess = isSuccess;
        Role = role;
        PlayerId = playerId;
        DisplayName = displayName;
        ValidationError = validationError;
    }

    public bool IsSuccess { get; }

    public UserRole? Role { get; }

    public Guid? PlayerId { get; }

    public string DisplayName { get; }

    public string? ValidationError { get; }

    public static JoinResult PlayerJoined(PlayerSession player)
    {
        return new JoinResult(true, UserRole.Player, player.Id, player.DisplayName, null);
    }

    public static JoinResult MasterJoined(string displayName)
    {
        return new JoinResult(true, UserRole.Master, null, displayName, null);
    }

    public static JoinResult Failed(string validationError, string displayName = "")
    {
        return new JoinResult(false, null, null, displayName, validationError);
    }
}
