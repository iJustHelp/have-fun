namespace HaveFun.Core;

public sealed class PlayerRegistryService : IPlayerRegistryService
{
    private readonly object syncRoot = new();
    private readonly Dictionary<Guid, PlayerSession> playersById = [];
    private readonly Dictionary<string, Guid> playerIdsByName = new(StringComparer.OrdinalIgnoreCase);

    public JoinResult RegisterPlayer(string submittedName)
    {
        var displayName = submittedName.Trim();

        if (string.IsNullOrWhiteSpace(displayName))
        {
            return JoinResult.Failed("Name is required.");
        }

        lock (syncRoot)
        {
            if (playerIdsByName.ContainsKey(displayName))
            {
                return JoinResult.Failed("That player name is already in use.", displayName);
            }

            var player = new PlayerSession
            {
                Id = Guid.NewGuid(),
                DisplayName = displayName,
                JoinedAt = DateTimeOffset.UtcNow
            };

            playersById.Add(player.Id, player);
            playerIdsByName.Add(displayName, player.Id);

            return JoinResult.PlayerJoined(player);
        }
    }

    public bool IsPlayerNameTaken(string submittedName)
    {
        var displayName = submittedName.Trim();

        if (string.IsNullOrWhiteSpace(displayName))
        {
            return false;
        }

        lock (syncRoot)
        {
            return playerIdsByName.ContainsKey(displayName);
        }
    }

    public bool TryGetPlayer(Guid playerId, out PlayerSession? player)
    {
        lock (syncRoot)
        {
            return playersById.TryGetValue(playerId, out player);
        }
    }

    public bool TryGetPlayerByName(string submittedName, out PlayerSession? player)
    {
        var displayName = submittedName.Trim();

        if (string.IsNullOrWhiteSpace(displayName))
        {
            player = null;
            return false;
        }

        lock (syncRoot)
        {
            if (!playerIdsByName.TryGetValue(displayName, out var playerId))
            {
                player = null;
                return false;
            }

            return playersById.TryGetValue(playerId, out player);
        }
    }

    public IReadOnlyList<PlayerSession> GetPlayers()
    {
        lock (syncRoot)
        {
            return playersById.Values
                .OrderBy(player => player.JoinedAt)
                .ToArray();
        }
    }
}
