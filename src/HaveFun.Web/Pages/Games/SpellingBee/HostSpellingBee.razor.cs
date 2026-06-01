using HaveFun.Core;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace HaveFun.Web;

public partial class HostSpellingBee : ComponentBase, IAsyncDisposable
{
    private CancellationTokenSource? _timerCancellation;
    private Task? _timerTask;
    private bool IsSessionChecked { get; set; }
    private string? ErrorMessage { get; set; }
    private string? FileLoadError { get; set; }
    private IReadOnlyList<GameFilePathOption> WordFiles { get; set; } = [];
    private IReadOnlyList<string> WordLines { get; set; } = [];
    private string? SelectedFileName { get; set; }
    private int TimeLimitInSeconds { get; set; } = 30;
    private int CurrentWordIndex { get; set; } = -1;
    private int CurrentWordNumber => CurrentWordIndex >= 0 ? CurrentWordIndex + 1 : 0;
    private int TotalWordCount => WordLines.Count;
    private int DisplayWordNumber
    {
        get
        {
            if (TotalWordCount == 0)
            {
                return 0;
            }

            if (IsRoundActive)
            {
                return CurrentWordNumber;
            }

            if (CurrentRound?.IsCompleted == true)
            {
                return Math.Min(CurrentWordNumber + 1, TotalWordCount);
            }

            return CurrentWordNumber == 0 ? 1 : CurrentWordNumber;
        }
    }

    private bool IsFileComplete => TotalWordCount > 0 && CurrentWordIndex >= TotalWordCount - 1 && CurrentRound?.IsCompleted == true;
    private bool IsRoundActive => CurrentRound?.Status == RoundStatus.Started;
    private bool CanStart => !string.IsNullOrWhiteSpace(SelectedFileName) &&
        WordLines.Count > 0 &&
        TimeLimitInSeconds > 0 &&
        !IsRoundActive &&
        !IsFileComplete;
    private bool IsRoundActionDisabled => IsRoundActive ? false : !CanStart;
    private Color RoundActionButtonColor => IsRoundActive ? Color.Error : Color.Primary;
    private string RoundActionButtonIcon => IsRoundActive ? Icons.Material.Filled.Stop : Icons.Material.Filled.PlayArrow;
    private string RoundActionButtonText => IsRoundActive ? "Stop" : "Start";
    private TimeSpan RemainingTime { get; set; }
    private string RemainingTimeText => $"{(int)RemainingTime.TotalMinutes:00}:{RemainingTime.Seconds:00}";
    private CurrentRound? CurrentRound { get; set; }
    private IReadOnlyList<PlayerSession> Players { get; set; } = [];
    private IReadOnlyList<HostPlayerResultRow> PlayerResults { get; set; } = [];

    [Inject]
    private IPlayerRegistryService PlayerRegistryService { get; set; } = default!;

    [Inject]
    private SpellingBeeFileService WordFileService { get; set; } = default!;

    [Inject]
    private IGameStateService GameStateService { get; set; } = default!;

    [Inject]
    private ISessionStorageService SessionStorageService { get; set; } = default!;

    protected override void OnInitialized()
    {
        CurrentRound = GameStateService.CurrentRound;
        RefreshPlayers();
        RefreshPlayerResults();
        LoadWordFiles();
        PlayerRegistryService.PlayersChanged += HandlePlayersChanged;
        GameStateService.CurrentRoundChanged += HandleCurrentRoundChanged;
        GameStateService.PlayerRoundStateChanged += HandlePlayerRoundStateChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        var currentUser = await SessionStorageService.GetCurrentUserAsync();

        if (currentUser?.Role != Role.Host)
        {
            ErrorMessage = "Open the host Home page before using Host Spelling Bee.";
        }

        IsSessionChecked = true;
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        PlayerRegistryService.PlayersChanged -= HandlePlayersChanged;
        GameStateService.CurrentRoundChanged -= HandleCurrentRoundChanged;
        GameStateService.PlayerRoundStateChanged -= HandlePlayerRoundStateChanged;
        StopTimer();
        await ValueTask.CompletedTask;
    }

    private void SelectWordFile(string fileName)
    {
        SelectedFileName = fileName;
        CurrentWordIndex = -1;
        CurrentRound = null;
        RemainingTime = TimeSpan.Zero;
        StopTimer();
        LoadSelectedWordLines();
        RefreshPlayerResults();
    }

    private void ToggleRound()
    {
        if (IsRoundActive)
        {
            StopRound();
            return;
        }

        StartRound();
    }

    private void StartRound()
    {
        if (!CanStart)
        {
            return;
        }

        try
        {
            var nextWordIndex = CurrentWordIndex + 1;
            var sentence = new TextDefinition
            {
                Text = WordLines[nextWordIndex],
                TimeLimitInSeconds = TimeLimitInSeconds
            };
            var expectedPlayerNames = Players
                .Select(player => player.DisplayName)
                .ToArray();

            CurrentWordIndex = nextWordIndex;
            CurrentRound = GameStateService.StartRound(sentence, expectedPlayerNames, CreateLetterTiles, CalculateCorrectness);
            FileLoadError = null;
            RefreshPlayerResults();
            StartTimerIfRoundIsActive();
        }
        catch (InvalidOperationException exception)
        {
            FileLoadError = exception.Message;
        }
        catch (ArgumentException exception)
        {
            FileLoadError = exception.Message;
        }
    }

    private void StopRound()
    {
        if (!IsRoundActive)
        {
            return;
        }

        CurrentRound = GameStateService.CompleteCurrentRound();
        StopTimer();
        RefreshPlayerResults();
    }

    private void LoadWordFiles()
    {
        try
        {
            WordFiles = WordFileService.GetGameFilePathes();
            SelectedFileName = WordFiles.FirstOrDefault()?.FilePath;
            LoadSelectedWordLines();
            FileLoadError = null;
        }
        catch (InvalidOperationException exception)
        {
            WordFiles = [];
            SelectedFileName = null;
            WordLines = [];
            FileLoadError = exception.Message;
        }
    }

    private void LoadSelectedWordLines()
    {
        if (string.IsNullOrWhiteSpace(SelectedFileName))
        {
            WordLines = [];
            return;
        }

        try
        {
            WordLines = WordFileService.LoadLines(SelectedFileName);
            FileLoadError = null;
        }
        catch (InvalidOperationException exception)
        {
            WordLines = [];
            FileLoadError = exception.Message;
        }
        catch (ArgumentException exception)
        {
            WordLines = [];
            FileLoadError = exception.Message;
        }
    }

    private void RefreshPlayers()
    {
        Players = PlayerRegistryService.GetPlayers();
    }

    private void RefreshPlayerResults()
    {
        var roundResults = GetCurrentRoundResults();
        var resultsByPlayerName = roundResults?.Results.ToDictionary(result => result.PlayerName, StringComparer.OrdinalIgnoreCase)
            ?? [];
        var totalScoresByPlayerName = GameStateService.GetPlayerTotalScores()
            .ToDictionary(playerTotalScore => playerTotalScore.PlayerName, StringComparer.OrdinalIgnoreCase);

        PlayerResults = Players
            .Select(player =>
            {
                resultsByPlayerName.TryGetValue(player.DisplayName, out var result);
                totalScoresByPlayerName.TryGetValue(player.DisplayName, out var totalScore);
                return new HostPlayerResultRow
                {
                    PlayerName = player.DisplayName,
                    TimeBeforeSubmit = result is null ? null : TimeSpan.FromSeconds(CurrentRound?.TimeLimitInSeconds ?? 0) - result.SpentTime,
                    SubmittedLetters = BuildSubmittedLetters(result?.SelectedTiles, CurrentRound?.SentenceText),
                    Score = result?.CorrectnessCount,
                    TotalScore = result?.TotalSentenceCount,
                    AggregateScore = totalScore?.Score,
                    AggregateTotalScore = totalScore?.TotalScore
                };
            })
            .ToArray();
    }

    private RoundResults? GetCurrentRoundResults()
    {
        var currentRound = CurrentRound;

        if (currentRound is null)
        {
            return null;
        }

        var rankedResults = GameStateService.GetSubmittedPlayerRoundStates()
            .Where(playerRoundState =>
                playerRoundState.RoundId == currentRound.Id &&
                playerRoundState.SelectedTiles.Count > 0 &&
                playerRoundState.SpentTime is not null &&
                playerRoundState.SubmittedAt is not null)
            .Select(playerRoundState => new
            {
                playerRoundState.PlayerName,
                playerRoundState.SelectedTiles,
                CorrectnessCount = CalculateCorrectness(currentRound, playerRoundState.SelectedTiles),
                TotalLetterCount = GetLetters(currentRound.SentenceText).Count,
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
                SelectedTiles = playerResult.SelectedTiles,
                CorrectnessCount = playerResult.CorrectnessCount,
                TotalSentenceCount = playerResult.TotalLetterCount,
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

    private void HandlePlayersChanged()
    {
        _ = InvokeAsync(() =>
        {
            RefreshPlayers();
            RefreshPlayerResults();
            StateHasChanged();
        });
    }

    private void HandleCurrentRoundChanged(CurrentRound round)
    {
        _ = InvokeAsync(() =>
        {
            CurrentRound = round;

            if (round.Status == RoundStatus.Completed)
            {
                UpdateRemainingTime();
                StopTimer();
            }
            else
            {
                StartTimerIfRoundIsActive();
            }

            RefreshPlayerResults();
            StateHasChanged();
        });
    }

    private void HandlePlayerRoundStateChanged(PlayerRoundState playerRoundState)
    {
        _ = InvokeAsync(() =>
        {
            RefreshPlayerResults();
            StateHasChanged();
        });
    }

    private void StartTimerIfRoundIsActive()
    {
        StopTimer();

        if (CurrentRound?.Status != RoundStatus.Started)
        {
            RemainingTime = TimeSpan.Zero;
            return;
        }

        UpdateRemainingTime();
        _timerCancellation = new CancellationTokenSource();
        _timerTask = RunTimerAsync(_timerCancellation.Token);
    }

    private async Task RunTimerAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await InvokeAsync(() =>
                {
                    UpdateRemainingTime();

                    if (RemainingTime == TimeSpan.Zero && CurrentRound?.Status == RoundStatus.Started)
                    {
                        CurrentRound = GameStateService.CompleteCurrentRound();
                    }

                    StateHasChanged();
                });

                if (RemainingTime == TimeSpan.Zero)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void StopTimer()
    {
        if (_timerCancellation is null)
        {
            return;
        }

        _timerCancellation.Cancel();
        _timerCancellation.Dispose();
        _timerCancellation = null;
        _timerTask = null;
    }

    private void UpdateRemainingTime()
    {
        if (CurrentRound?.StartedAt is null)
        {
            RemainingTime = TimeSpan.Zero;
            return;
        }

        var elapsed = (CurrentRound.CompletedAt ?? DateTimeOffset.UtcNow) - CurrentRound.StartedAt.Value;
        var remaining = TimeSpan.FromSeconds(CurrentRound.TimeLimitInSeconds) - elapsed;
        RemainingTime = remaining <= TimeSpan.Zero ? TimeSpan.Zero : remaining;
    }

    private static IReadOnlyList<Tile> CreateLetterTiles(CurrentRound round)
    {
        var letters = GetLetters(round.SentenceText).ToArray();

        for (var index = letters.Length - 1; index > 0; index--)
        {
            var swapIndex = Random.Shared.Next(index + 1);
            (letters[index], letters[swapIndex]) = (letters[swapIndex], letters[index]);
        }

        return letters
            .Select(letter => new Tile
            {
                Id = Guid.NewGuid(),
                Text = letter
            })
            .ToArray();
    }

    private static int CalculateCorrectness(CurrentRound round, IReadOnlyList<Tile> selectedTiles)
    {
        var correctLetters = GetLetters(round.SentenceText);
        var submittedLetters = selectedTiles.Select(tile => tile.Text).ToArray();
        var comparedLetterCount = Math.Min(correctLetters.Count, submittedLetters.Length);
        var correctnessCount = 0;

        for (var index = 0; index < comparedLetterCount; index++)
        {
            if (submittedLetters[index] == correctLetters[index])
            {
                correctnessCount++;
            }
        }

        return correctnessCount;
    }

    private static string FormatTimeBeforeSubmit(TimeSpan? timeBeforeSubmit)
    {
        if (timeBeforeSubmit is null)
        {
            return string.Empty;
        }

        var value = timeBeforeSubmit.Value <= TimeSpan.Zero ? TimeSpan.Zero : timeBeforeSubmit.Value;
        return $"{(int)value.TotalMinutes:00}:{value.Seconds:00}";
    }

    private static string FormatScore(int? score, int? totalScore)
    {
        return score is null || totalScore is null ? string.Empty : $"{score} / {totalScore}";
    }

    private static string GetSubmittedLetterStyle(bool isCorrect)
    {
        return isCorrect
            ? "background-color: #dcfce7; color: #166534; border-radius: 4px; padding: 0 4px; font-weight: 600;"
            : string.Empty;
    }

    private static IReadOnlyList<SubmittedLetterPart> BuildSubmittedLetters(
        IReadOnlyList<Tile>? submittedTiles,
        string? correctWord)
    {
        if (submittedTiles is null || submittedTiles.Count == 0)
        {
            return [];
        }

        var submittedLetters = submittedTiles.Select(tile => tile.Text).ToArray();
        var correctLetters = correctWord is null ? [] : GetLetters(correctWord);

        return submittedLetters
            .Select((letter, index) => new SubmittedLetterPart
            {
                Text = letter,
                IsCorrect = index < correctLetters.Count && letter == correctLetters[index]
            })
            .ToArray();
    }

    private static IReadOnlyList<string> GetLetters(string text)
    {
        return text
            .Where(character => !char.IsWhiteSpace(character))
            .Select(character => character.ToString())
            .ToArray();
    }

    private sealed record HostPlayerResultRow
    {
        public required string PlayerName { get; init; }

        public TimeSpan? TimeBeforeSubmit { get; init; }

        public double TimeBeforeSubmitSeconds => TimeBeforeSubmit?.TotalSeconds ?? -1;

        public IReadOnlyList<SubmittedLetterPart> SubmittedLetters { get; init; } = [];

        public int? Score { get; init; }

        public int? TotalScore { get; init; }

        public double ScoreSortValue => CalculateScoreSortValue(Score, TotalScore);

        public int? AggregateScore { get; init; }

        public int? AggregateTotalScore { get; init; }

        public double AggregateScoreSortValue => CalculateScoreSortValue(AggregateScore, AggregateTotalScore);

        private static double CalculateScoreSortValue(int? score, int? totalScore)
        {
            return score is null || totalScore is null || totalScore == 0
                ? -1
                : (double)score.Value / totalScore.Value;
        }
    }

    private sealed record SubmittedLetterPart
    {
        public required string Text { get; init; }

        public required bool IsCorrect { get; init; }
    }
}
