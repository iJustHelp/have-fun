namespace HaveFun.Core;

public interface IPlayerRegistryService
{
    JoinResult RegisterPlayer(string submittedName);

    bool IsPlayerNameTaken(string submittedName);

    bool TryGetPlayer(Guid playerId, out PlayerSession? player);

    bool TryGetPlayerByName(string submittedName, out PlayerSession? player);

    IReadOnlyList<PlayerSession> GetPlayers();
}
