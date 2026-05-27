namespace HaveFun.Core;

public interface IPlayerRegistryService
{
    event Action? PlayersChanged;

    event Action<PlayerSession>? PlayerRemoved;

    JoinResult RegisterPlayer(string submittedName);

    bool RemovePlayer(Guid playerId);

    bool IsPlayerNameTaken(string submittedName);

    bool TryGetPlayer(Guid playerId, out PlayerSession? player);

    bool TryGetPlayerByName(string submittedName, out PlayerSession? player);

    IReadOnlyList<PlayerSession> GetPlayers();
}
