namespace HaveFun.Core;

public sealed class PlayerRegistryService : IPlayerRegistryService
{
    private readonly object _syncRoot = new();
    private readonly Dictionary<Guid, PlayerSession> _playersById = [];
    private readonly Dictionary<string, Guid> _playerIdsByName = new(StringComparer.OrdinalIgnoreCase);

    public event Action? PlayersChanged;

    public event Action<PlayerSession>? PlayerRemoved;

    public JoinResult RegisterPlayer(string submittedName)
    {
        var displayName = submittedName.Trim();

        if (string.IsNullOrWhiteSpace(displayName))
        {
            return JoinResult.Failed("Name is required.");
        }

        PlayerSession player;

        lock (_syncRoot)
        {
            if (_playerIdsByName.ContainsKey(displayName))
            {
                return JoinResult.Failed("That player name is already in use.", displayName);
            }

            player = new PlayerSession
            {
                Id = Guid.NewGuid(),
                DisplayName = displayName,
                JoinedAt = DateTimeOffset.UtcNow
            };

            _playersById.Add(player.Id, player);
            _playerIdsByName.Add(displayName, player.Id);
        }

        PlayersChanged?.Invoke();
        return JoinResult.PlayerJoined(player);
    }

    public bool RemovePlayer(Guid playerId)
    {
        PlayerSession? removedPlayer;

        lock (_syncRoot)
        {
            if (!_playersById.Remove(playerId, out removedPlayer))
            {
                return false;
            }

            _playerIdsByName.Remove(removedPlayer.DisplayName);
        }

        PlayersChanged?.Invoke();
        PlayerRemoved?.Invoke(removedPlayer);
        return true;
    }

    public bool IsPlayerNameTaken(string submittedName)
    {
        var displayName = submittedName.Trim();

        if (string.IsNullOrWhiteSpace(displayName))
        {
            return false;
        }

        lock (_syncRoot)
        {
            return _playerIdsByName.ContainsKey(displayName);
        }
    }

    public bool TryGetPlayer(Guid playerId, out PlayerSession? player)
    {
        lock (_syncRoot)
        {
            return _playersById.TryGetValue(playerId, out player);
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

        lock (_syncRoot)
        {
            if (!_playerIdsByName.TryGetValue(displayName, out var playerId))
            {
                player = null;
                return false;
            }

            return _playersById.TryGetValue(playerId, out player);
        }
    }

    public IReadOnlyList<PlayerSession> GetPlayers()
    {
        lock (_syncRoot)
        {
            return _playersById.Values
                .OrderBy(player => player.JoinedAt)
                .ToArray();
        }
    }
}
