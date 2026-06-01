namespace HaveFun.Core;

public sealed class GameStateService : IGameStateService
{
    private readonly ITileCollectionService _tileCollectionService;
    private readonly object _syncRoot = new();
    private readonly Dictionary<PlayerRoundKey, PlayerRoundState> _playerRoundStates = [];
    private readonly Dictionary<string, PlayerTotalScore> _playerTotalScores = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<Guid> _totaledRoundIds = [];
    private Func<CurrentRound, IReadOnlyList<Tile>> _createAvailableTiles;
    private Func<CurrentRound, string, int> _calculateScore = CalculateDefaultScore;
    private CurrentRound? _currentRound;

    public GameStateService(ITileCollectionService tileCollectionService)
    {
        _tileCollectionService = tileCollectionService;
        _createAvailableTiles = tileCollectionService.CreateAvailableTiles;
    }

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

    public CurrentRound StartRound(SentenceDefinition sentence, IReadOnlyList<string> expectedPlayerNames)
    {
        return StartRound(
            sentence,
            expectedPlayerNames,
            _tileCollectionService.CreateAvailableTiles,
            CalculateDefaultScore);
    }

    public CurrentRound StartRound(
        SentenceDefinition sentence,
        IReadOnlyList<string> expectedPlayerNames,
        Func<CurrentRound, IReadOnlyList<Tile>> createAvailableTiles,
        Func<CurrentRound, string, int> calculateScore)
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

    public PlayerRoundState? SelectTile(string playerName, Guid tileId)
    {
        var normalizedName = NormalizePlayerName(playerName);
        PlayerRoundState? updatedState;

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

            var playerRoundState = GetOrCreatePlayerRoundStateUnsafe(_currentRound, normalizedName);

            if (playerRoundState.IsSubmitted)
            {
                return playerRoundState;
            }

            var selectedSentence = playerRoundState.AvailableTiles.FirstOrDefault(sentence => sentence.Id == tileId);

            if (selectedSentence is null)
            {
                return playerRoundState;
            }

            updatedState = playerRoundState with
            {
                AvailableTiles = playerRoundState.AvailableTiles
                    .Where(sentence => sentence.Id != tileId)
                    .ToArray(),
                SelectedTiles = playerRoundState.SelectedTiles
                    .Append(selectedSentence)
                    .ToArray()
            };

            _playerRoundStates[new PlayerRoundKey(_currentRound.Id, normalizedName)] = updatedState;
        }

        PlayerRoundStateChanged?.Invoke(updatedState);

        return updatedState;
    }

    public PlayerRoundState? ReturnTile(string playerName, Guid tileId)
    {
        var normalizedName = NormalizePlayerName(playerName);
        PlayerRoundState? updatedState;

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

            var playerRoundState = GetOrCreatePlayerRoundStateUnsafe(_currentRound, normalizedName);

            if (playerRoundState.IsSubmitted)
            {
                return playerRoundState;
            }

            var returnedWord = playerRoundState.SelectedTiles.FirstOrDefault(word => word.Id == tileId);

            if (returnedWord is null)
            {
                return playerRoundState;
            }

            updatedState = playerRoundState with
            {
                AvailableTiles = playerRoundState.AvailableTiles
                    .Append(returnedWord)
                    .ToArray(),
                SelectedTiles = playerRoundState.SelectedTiles
                    .Where(word => word.Id != tileId)
                    .ToArray()
            };

            _playerRoundStates[new PlayerRoundKey(_currentRound.Id, normalizedName)] = updatedState;
        }

        PlayerRoundStateChanged?.Invoke(updatedState);

        return updatedState;
    }

    public PlayerRoundState? SubmitPlayerRound(string playerName)
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

            if (_currentRound.Status == RoundStatus.Completed)
            {
                return GetOrCreatePlayerRoundStateUnsafe(_currentRound, normalizedName);
            }

            var playerRoundState = GetOrCreatePlayerRoundStateUnsafe(_currentRound, normalizedName);

            if (playerRoundState.IsSubmitted)
            {
                return playerRoundState;
            }

            if (!playerRoundState.CanSubmit)
            {
                return playerRoundState;
            }

            var submittedAt = DateTimeOffset.UtcNow;
            updatedState = playerRoundState with
            {
                IsSubmitted = true,
                SubmittedSentence = playerRoundState.CollectedSentence,
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

    private static int CalculateCorrectness(IReadOnlyList<string> originalSentences, string submittedSentence)
    {
        var submittedSentences = SplitSentences(submittedSentence);
        var comparedSentenceCount = Math.Min(originalSentences.Count, submittedSentences.Count);
        var correctnessCount = 0;

        for (var index = 0; index < comparedSentenceCount; index++)
        {
            if (submittedSentences[index] == originalSentences[index])
            {
                correctnessCount++;
            }
        }

        return correctnessCount;
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

        var submittedScores = _playerRoundStates.Values
            .Where(playerRoundState =>
                playerRoundState.RoundId == round.Id &&
                playerRoundState.IsSubmitted &&
                playerRoundState.SubmittedSentence is not null)
            .ToDictionary(
                playerRoundState => playerRoundState.PlayerName,
                playerRoundState => _calculateScore(round, playerRoundState.SubmittedSentence!),
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
                TotalScore = (existingScore?.TotalScore ?? 0) + round.OriginalSentences.Count
            };
        }
    }

    private static int CalculateDefaultScore(CurrentRound round, string submittedSentence)
    {
        return CalculateCorrectness(round.OriginalSentences, submittedSentence);
    }

    private static string NormalizePlayerName(string playerName)
    {
        return playerName.Trim();
    }

    private sealed record PlayerRoundKey(Guid RoundId, string PlayerName);
}
