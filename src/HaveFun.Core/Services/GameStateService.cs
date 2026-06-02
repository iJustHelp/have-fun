namespace HaveFun.Core;

public class GameStateService : IGameStateService
{
    private const int FirstSubmittedPlayerBonus = 1;
    private readonly object _syncRoot = new();
    private readonly Dictionary<PlayerRoundKey, PlayerRoundState> _playerRoundStates = [];
    private readonly Dictionary<string, PlayerTotalScore> _playerTotalScores = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<Guid> _totaledRoundIds = [];
    private Func<CurrentRound, IReadOnlyList<Tile>> _createAvailableTiles = static _ => [];
    private Func<CurrentRound, IReadOnlyList<Tile>, int> _calculateScore = static (_, _) => 0;
    private CurrentRound? _currentRound;

    public event Action<CurrentRound>? CurrentRoundChanged;

    public event Action<PlayerRoundState>? PlayerRoundStateChanged;

    public CurrentRound? CurrentRound
    {
        get
        {
            lock (_syncRoot)
            {
                return _currentRound;
            }
        }
    }

    public CurrentRound StartRound(
        TextDefinition sentence,
        IReadOnlyList<string> expectedPlayerNames,
        Func<CurrentRound, IReadOnlyList<Tile>> createAvailableTiles,
        Func<CurrentRound, IReadOnlyList<Tile>, int> calculateScore)
    {
        if (string.IsNullOrWhiteSpace(sentence.Text))
        {
            throw new ArgumentException("Sentence text is required.", nameof(sentence));
        }

        ArgumentNullException.ThrowIfNull(createAvailableTiles);
        ArgumentNullException.ThrowIfNull(calculateScore);

        if (sentence.TimeLimitInSeconds <= 0)
        {
            throw new ArgumentException("Sentence time limit must be greater than zero.", nameof(sentence));
        }

        var originalSentences = SplitSentences(sentence.Text);
        var shuffledSentences = ShuffleSentences(originalSentences);
        var normalizedExpectedPlayerNames = expectedPlayerNames
            .Select(NormalizePlayerName)
            .Where(playerName => !string.IsNullOrWhiteSpace(playerName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var round = new CurrentRound
        {
            Id = Guid.NewGuid(),
            SentenceText = sentence.Text,
            TimeLimitInSeconds = sentence.TimeLimitInSeconds,
            OriginalSentences = originalSentences,
            ShuffledSentences = shuffledSentences,
            ExpectedPlayerNames = normalizedExpectedPlayerNames,
            Status = RoundStatus.Started,
            StartedAt = DateTimeOffset.UtcNow
        };

        lock (_syncRoot)
        {
            _currentRound = round;
            _createAvailableTiles = createAvailableTiles;
            _calculateScore = calculateScore;
            _playerRoundStates.Clear();
        }

        CurrentRoundChanged?.Invoke(round);

        return round;
    }

    public CurrentRound? CompleteCurrentRound()
    {
        CurrentRound? completedRound;

        lock (_syncRoot)
        {
            if (_currentRound is null)
            {
                return null;
            }

            if (_currentRound.Status == RoundStatus.Completed)
            {
                return _currentRound;
            }

            completedRound = _currentRound with
            {
                Status = RoundStatus.Completed,
                CompletedAt = DateTimeOffset.UtcNow
            };
            _currentRound = completedRound;
            RecordTotalScoresUnsafe(completedRound);
        }

        CurrentRoundChanged?.Invoke(completedRound);
        return completedRound;
    }

    public PlayerRoundState? GetPlayerRoundState(string playerName)
    {
        var normalizedName = NormalizePlayerName(playerName);

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return null;
        }

        lock (_syncRoot)
        {
            return _currentRound is null
                ? null
                : GetPlayerRoundStateUnsafe(_currentRound.Id, normalizedName);
        }
    }

    public PlayerRoundState? GetOrCreatePlayerRoundState(string playerName)
    {
        var normalizedName = NormalizePlayerName(playerName);

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return null;
        }

        lock (_syncRoot)
        {
            if (_currentRound is null)
            {
                return null;
            }

            var key = new PlayerRoundKey(_currentRound.Id, normalizedName);

            if (_playerRoundStates.TryGetValue(key, out var playerRoundState))
            {
                return playerRoundState;
            }

            playerRoundState = new PlayerRoundState
            {
                PlayerName = normalizedName,
                RoundId = _currentRound.Id,
                AvailableTiles = _createAvailableTiles(_currentRound),
                SelectedTiles = []
            };

            _playerRoundStates.Add(key, playerRoundState);

            return playerRoundState;
        }
    }

    public PlayerRoundState? SubmitPlayerRound(string playerName, IReadOnlyList<Tile> selectedTiles)
    {
        var normalizedName = NormalizePlayerName(playerName);
        PlayerRoundState? updatedState;

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return null;
        }

        lock (_syncRoot)
        {
            if (_currentRound?.StartedAt is null)
            {
                return null;
            }

            selectedTiles ??= [];

            if (_currentRound.Status == RoundStatus.Completed)
            {
                return GetOrCreatePlayerRoundStateUnsafe(_currentRound, normalizedName);
            }

            var playerRoundState = GetOrCreatePlayerRoundStateUnsafe(_currentRound, normalizedName);

            if (playerRoundState.IsSubmitted)
            {
                return playerRoundState;
            }

            var availableTilesById = playerRoundState.AvailableTiles.ToDictionary(tile => tile.Id);
            var selectedTileIds = new HashSet<Guid>();
            var validatedSelectedTiles = selectedTiles
                .Where(tile => availableTilesById.ContainsKey(tile.Id) && selectedTileIds.Add(tile.Id))
                .Select(tile => availableTilesById[tile.Id])
                .ToArray();

            if (validatedSelectedTiles.Length == 0)
            {
                return playerRoundState;
            }

            var submittedAt = DateTimeOffset.UtcNow;
            updatedState = playerRoundState with
            {
                IsSubmitted = true,
                SelectedTiles = validatedSelectedTiles,
                AvailableTiles = playerRoundState.AvailableTiles
                    .Where(tile => !selectedTileIds.Contains(tile.Id))
                    .ToArray(),
                SubmittedAt = submittedAt,
                SpentTime = submittedAt - _currentRound.StartedAt.Value
            };

            _playerRoundStates[new PlayerRoundKey(_currentRound.Id, normalizedName)] = updatedState;
        }

        PlayerRoundStateChanged?.Invoke(updatedState);
        CompleteIfAllExpectedPlayersSubmitted();

        return updatedState;
    }

    public IReadOnlyList<PlayerRoundState> GetSubmittedPlayerRoundStates()
    {
        lock (_syncRoot)
        {
            if (_currentRound is null)
            {
                return [];
            }

            return _playerRoundStates.Values
                .Where(playerRoundState => playerRoundState.RoundId == _currentRound.Id && playerRoundState.IsSubmitted)
                .OrderBy(playerRoundState => playerRoundState.SubmittedAt)
                .ToArray();
        }
    }

    public IReadOnlyList<PlayerTotalScore> GetPlayerTotalScores()
    {
        lock (_syncRoot)
        {
            return _playerTotalScores.Values
                .OrderBy(playerTotalScore => playerTotalScore.PlayerName, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }

    private static IReadOnlyList<string> SplitSentences(string sentenceText)
    {
        return sentenceText
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();
    }

    private static IReadOnlyList<string> ShuffleSentences(IReadOnlyList<string> sentences)
    {
        var shuffledSentences = sentences.ToArray();

        for (var index = shuffledSentences.Length - 1; index > 0; index--)
        {
            var swapIndex = Random.Shared.Next(index + 1);
            (shuffledSentences[index], shuffledSentences[swapIndex]) = (shuffledSentences[swapIndex], shuffledSentences[index]);
        }

        return shuffledSentences;
    }

    private void CompleteIfAllExpectedPlayersSubmitted()
    {
        CurrentRound? completedRound = null;

        lock (_syncRoot)
        {
            if (_currentRound is null ||
                _currentRound.Status == RoundStatus.Completed ||
                _currentRound.ExpectedPlayerNames.Count == 0)
            {
                return;
            }

            var submittedNames = _playerRoundStates.Values
                .Where(playerRoundState => playerRoundState.RoundId == _currentRound.Id && playerRoundState.IsSubmitted)
                .Select(playerRoundState => playerRoundState.PlayerName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var allExpectedPlayersSubmitted = _currentRound.ExpectedPlayerNames
                .All(submittedNames.Contains);

            if (!allExpectedPlayersSubmitted)
            {
                return;
            }

            completedRound = _currentRound with
            {
                Status = RoundStatus.Completed,
                CompletedAt = DateTimeOffset.UtcNow
            };
            _currentRound = completedRound;
            RecordTotalScoresUnsafe(completedRound);
        }

        CurrentRoundChanged?.Invoke(completedRound);
    }

    private PlayerRoundState? GetPlayerRoundStateUnsafe(Guid roundId, string playerName)
    {
        return _playerRoundStates.TryGetValue(new PlayerRoundKey(roundId, playerName), out var playerRoundState)
            ? playerRoundState
            : null;
    }

    private PlayerRoundState GetOrCreatePlayerRoundStateUnsafe(CurrentRound round, string playerName)
    {
        var key = new PlayerRoundKey(round.Id, playerName);

        if (_playerRoundStates.TryGetValue(key, out var playerRoundState))
        {
            return playerRoundState;
        }

        playerRoundState = new PlayerRoundState
        {
            PlayerName = playerName,
            RoundId = round.Id,
            AvailableTiles = _createAvailableTiles(round),
            SelectedTiles = []
        };

        _playerRoundStates.Add(key, playerRoundState);

        return playerRoundState;
    }

    private void RecordTotalScoresUnsafe(CurrentRound round)
    {
        if (!_totaledRoundIds.Add(round.Id))
        {
            return;
        }

        var submittedPlayerRoundStates = _playerRoundStates.Values
            .Where(playerRoundState =>
                playerRoundState.RoundId == round.Id &&
                playerRoundState.IsSubmitted)
            .ToArray();
        var roundTotalScore = _createAvailableTiles(round).Count;
        var firstSubmittedPlayer = submittedPlayerRoundStates
            .Select(playerRoundState => new
            {
                playerRoundState.PlayerName,
                playerRoundState.SubmittedAt,
                Score = _calculateScore(round, playerRoundState.SelectedTiles)
            })
            .Where(playerRoundState => playerRoundState.SubmittedAt is not null)
            .OrderBy(playerRoundState => playerRoundState.SubmittedAt)
            .ThenBy(playerRoundState => playerRoundState.PlayerName, StringComparer.Ordinal)
            .FirstOrDefault();
        var isFirstSubmittedPlayerCorrect = firstSubmittedPlayer?.Score == roundTotalScore;
        var firstSubmittedPlayerName = isFirstSubmittedPlayerCorrect
            ? firstSubmittedPlayer?.PlayerName
            : null;
        var submittedScores = submittedPlayerRoundStates
            .ToDictionary(
                playerRoundState => playerRoundState.PlayerName,
                playerRoundState => _calculateScore(round, playerRoundState.SelectedTiles) +
                    (isFirstSubmittedPlayerCorrect &&
                    string.Equals(playerRoundState.PlayerName, firstSubmittedPlayerName, StringComparison.OrdinalIgnoreCase)
                        ? FirstSubmittedPlayerBonus
                        : 0),
                StringComparer.OrdinalIgnoreCase);

        var playerNames = round.ExpectedPlayerNames
            .Concat(submittedScores.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var playerName in playerNames)
        {
            submittedScores.TryGetValue(playerName, out var score);
            _playerTotalScores.TryGetValue(playerName, out var existingScore);

            _playerTotalScores[playerName] = new PlayerTotalScore
            {
                PlayerName = playerName,
                Score = (existingScore?.Score ?? 0) + score,
                TotalScore = (existingScore?.TotalScore ?? 0) + roundTotalScore
            };
        }
    }

    private static string NormalizePlayerName(string playerName)
    {
        return playerName.Trim();
    }

    private sealed record PlayerRoundKey(Guid RoundId, string PlayerName);
}
