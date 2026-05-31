namespace HaveFun.Core;

public sealed class GameStateService : IGameStateService
{
    private readonly object syncRoot = new();
    private readonly Dictionary<PlayerRoundKey, PlayerRoundState> playerRoundStates = [];
    private readonly Dictionary<string, PlayerTotalScore> playerTotalScores = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<Guid> totaledRoundIds = [];
    private CurrentRound? currentRound;

    public event Action<CurrentRound>? CurrentRoundChanged;

    public event Action<PlayerRoundState>? PlayerRoundStateChanged;

    public CurrentRound? CurrentRound
    {
        get
        {
            lock (syncRoot)
            {
                return currentRound;
            }
        }
    }

    public CurrentRound StartRound(SentenceDefinition sentence, IReadOnlyList<string> expectedPlayerNames)
    {
        if (string.IsNullOrWhiteSpace(sentence.Text))
        {
            throw new ArgumentException("Sentence text is required.", nameof(sentence));
        }

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

        lock (syncRoot)
        {
            currentRound = round;
            playerRoundStates.Clear();
        }

        CurrentRoundChanged?.Invoke(round);

        return round;
    }

    public CurrentRound? CompleteCurrentRound()
    {
        CurrentRound? completedRound;

        lock (syncRoot)
        {
            if (currentRound is null)
            {
                return null;
            }

            if (currentRound.Status == RoundStatus.Completed)
            {
                return currentRound;
            }

            completedRound = currentRound with
            {
                Status = RoundStatus.Completed,
                CompletedAt = DateTimeOffset.UtcNow
            };
            currentRound = completedRound;
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

        lock (syncRoot)
        {
            return currentRound is null
                ? null
                : GetPlayerRoundStateUnsafe(currentRound.Id, normalizedName);
        }
    }

    public PlayerRoundState? GetOrCreatePlayerRoundState(string playerName)
    {
        var normalizedName = NormalizePlayerName(playerName);

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return null;
        }

        lock (syncRoot)
        {
            if (currentRound is null)
            {
                return null;
            }

            var key = new PlayerRoundKey(currentRound.Id, normalizedName);

            if (playerRoundStates.TryGetValue(key, out var playerRoundState))
            {
                return playerRoundState;
            }

            playerRoundState = new PlayerRoundState
            {
                PlayerName = normalizedName,
                RoundId = currentRound.Id,
                AvailableTiles = currentRound.ShuffledSentences
                    .Select(sentence => new Tile
                    {
                        Id = Guid.NewGuid(),
                        Text = sentence
                    })
                    .ToArray(),
                SelectedTiles = []
            };

            playerRoundStates.Add(key, playerRoundState);

            return playerRoundState;
        }
    }

    public PlayerRoundState? SelectWord(string playerName, Guid wordId)
    {
        var normalizedName = NormalizePlayerName(playerName);
        PlayerRoundState? updatedState;

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return null;
        }

        lock (syncRoot)
        {
            if (currentRound is null)
            {
                return null;
            }

            var playerRoundState = GetOrCreatePlayerRoundStateUnsafe(currentRound, normalizedName);

            if (playerRoundState.IsSubmitted)
            {
                return playerRoundState;
            }

            var selectedSentence = playerRoundState.AvailableTiles.FirstOrDefault(sentence => sentence.Id == wordId);

            if (selectedSentence is null)
            {
                return playerRoundState;
            }

            updatedState = playerRoundState with
            {
                AvailableTiles = playerRoundState.AvailableTiles
                    .Where(sentence => sentence.Id != wordId)
                    .ToArray(),
                SelectedTiles = playerRoundState.SelectedTiles
                    .Append(selectedSentence)
                    .ToArray()
            };

            playerRoundStates[new PlayerRoundKey(currentRound.Id, normalizedName)] = updatedState;
        }

        PlayerRoundStateChanged?.Invoke(updatedState);

        return updatedState;
    }

    public PlayerRoundState? ReturnWord(string playerName, Guid wordId)
    {
        var normalizedName = NormalizePlayerName(playerName);
        PlayerRoundState? updatedState;

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return null;
        }

        lock (syncRoot)
        {
            if (currentRound is null)
            {
                return null;
            }

            var playerRoundState = GetOrCreatePlayerRoundStateUnsafe(currentRound, normalizedName);

            if (playerRoundState.IsSubmitted)
            {
                return playerRoundState;
            }

            var returnedWord = playerRoundState.SelectedTiles.FirstOrDefault(word => word.Id == wordId);

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
                    .Where(word => word.Id != wordId)
                    .ToArray()
            };

            playerRoundStates[new PlayerRoundKey(currentRound.Id, normalizedName)] = updatedState;
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

        lock (syncRoot)
        {
            if (currentRound?.StartedAt is null)
            {
                return null;
            }

            if (currentRound.Status == RoundStatus.Completed)
            {
                return GetOrCreatePlayerRoundStateUnsafe(currentRound, normalizedName);
            }

            var playerRoundState = GetOrCreatePlayerRoundStateUnsafe(currentRound, normalizedName);

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
                SpentTime = submittedAt - currentRound.StartedAt.Value
            };

            playerRoundStates[new PlayerRoundKey(currentRound.Id, normalizedName)] = updatedState;
        }

        PlayerRoundStateChanged?.Invoke(updatedState);
        CompleteIfAllExpectedPlayersSubmitted();

        return updatedState;
    }

    public IReadOnlyList<PlayerRoundState> GetSubmittedPlayerRoundStates()
    {
        lock (syncRoot)
        {
            if (currentRound is null)
            {
                return [];
            }

            return playerRoundStates.Values
                .Where(playerRoundState => playerRoundState.RoundId == currentRound.Id && playerRoundState.IsSubmitted)
                .OrderBy(playerRoundState => playerRoundState.SubmittedAt)
                .ToArray();
        }
    }

    public RoundResults? GetCurrentRoundResults()
    {
        lock (syncRoot)
        {
            if (currentRound is null)
            {
                return null;
            }

            var rankedResults = playerRoundStates.Values
                .Where(playerRoundState =>
                    playerRoundState.RoundId == currentRound.Id &&
                    playerRoundState.IsSubmitted &&
                    playerRoundState.SubmittedSentence is not null &&
                    playerRoundState.SpentTime is not null &&
                    playerRoundState.SubmittedAt is not null)
                .Select(playerRoundState => new
                {
                    playerRoundState.PlayerName,
                    SubmittedSentence = playerRoundState.SubmittedSentence!,
                    CorrectnessCount = CalculateCorrectness(currentRound.OriginalSentences, playerRoundState.SubmittedSentence!),
                    TotalSentenceCount = currentRound.OriginalSentences.Count,
                    SpentTime = playerRoundState.SpentTime!.Value,
                    SubmittedAt = playerRoundState.SubmittedAt!.Value
                })
                .OrderByDescending(playerResult => playerResult.CorrectnessCount)
                .ThenBy(playerResult => playerResult.SpentTime)
                .ThenBy(playerResult => playerResult.PlayerName, StringComparer.Ordinal)
                .Select((playerResult, index) => new PlayerResult
                {
                    Rank = index + 1,
                    PlayerName = playerResult.PlayerName,
                    SubmittedSentence = playerResult.SubmittedSentence,
                    CorrectnessCount = playerResult.CorrectnessCount,
                    TotalSentenceCount = playerResult.TotalSentenceCount,
                    SpentTime = playerResult.SpentTime,
                    SubmittedAt = playerResult.SubmittedAt
                })
                .ToArray();

            return new RoundResults
            {
                RoundId = currentRound.Id,
                CorrectSentence = currentRound.SentenceText,
                Results = rankedResults
            };
        }
    }

    public IReadOnlyList<PlayerTotalScore> GetPlayerTotalScores()
    {
        lock (syncRoot)
        {
            return playerTotalScores.Values
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

        lock (syncRoot)
        {
            if (currentRound is null ||
                currentRound.Status == RoundStatus.Completed ||
                currentRound.ExpectedPlayerNames.Count == 0)
            {
                return;
            }

            var submittedNames = playerRoundStates.Values
                .Where(playerRoundState => playerRoundState.RoundId == currentRound.Id && playerRoundState.IsSubmitted)
                .Select(playerRoundState => playerRoundState.PlayerName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var allExpectedPlayersSubmitted = currentRound.ExpectedPlayerNames
                .All(submittedNames.Contains);

            if (!allExpectedPlayersSubmitted)
            {
                return;
            }

            completedRound = currentRound with
            {
                Status = RoundStatus.Completed,
                CompletedAt = DateTimeOffset.UtcNow
            };
            currentRound = completedRound;
            RecordTotalScoresUnsafe(completedRound);
        }

        CurrentRoundChanged?.Invoke(completedRound);
    }

    private PlayerRoundState? GetPlayerRoundStateUnsafe(Guid roundId, string playerName)
    {
        return playerRoundStates.TryGetValue(new PlayerRoundKey(roundId, playerName), out var playerRoundState)
            ? playerRoundState
            : null;
    }

    private PlayerRoundState GetOrCreatePlayerRoundStateUnsafe(CurrentRound round, string playerName)
    {
        var key = new PlayerRoundKey(round.Id, playerName);

        if (playerRoundStates.TryGetValue(key, out var playerRoundState))
        {
            return playerRoundState;
        }

        playerRoundState = new PlayerRoundState
        {
            PlayerName = playerName,
            RoundId = round.Id,
            AvailableTiles = round.ShuffledSentences
                .Select(sentence => new Tile
                {
                    Id = Guid.NewGuid(),
                    Text = sentence
                })
                .ToArray(),
            SelectedTiles = []
        };

        playerRoundStates.Add(key, playerRoundState);

        return playerRoundState;
    }

    private void RecordTotalScoresUnsafe(CurrentRound round)
    {
        if (!totaledRoundIds.Add(round.Id))
        {
            return;
        }

        var submittedScores = playerRoundStates.Values
            .Where(playerRoundState =>
                playerRoundState.RoundId == round.Id &&
                playerRoundState.IsSubmitted &&
                playerRoundState.SubmittedSentence is not null)
            .ToDictionary(
                playerRoundState => playerRoundState.PlayerName,
                playerRoundState => CalculateCorrectness(round.OriginalSentences, playerRoundState.SubmittedSentence!),
                StringComparer.OrdinalIgnoreCase);

        var playerNames = round.ExpectedPlayerNames
            .Concat(submittedScores.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var playerName in playerNames)
        {
            submittedScores.TryGetValue(playerName, out var score);
            playerTotalScores.TryGetValue(playerName, out var existingScore);

            playerTotalScores[playerName] = new PlayerTotalScore
            {
                PlayerName = playerName,
                Score = (existingScore?.Score ?? 0) + score,
                TotalScore = (existingScore?.TotalScore ?? 0) + round.OriginalSentences.Count
            };
        }
    }

    private static string NormalizePlayerName(string playerName)
    {
        return playerName.Trim();
    }

    private sealed record PlayerRoundKey(Guid RoundId, string PlayerName);
}
