using HaveFun.Core;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace HaveFun.Web;

public partial class HostSentenceScrambler : ComponentBase, IAsyncDisposable
{
    private CancellationTokenSource? timerCancellation;
    private Task? timerTask;
    private bool IsSessionChecked { get; set; }
    private string? ErrorMessage { get; set; }
    private string? FileLoadError { get; set; }
    private IReadOnlyList<SentenceFileOption> SentenceFiles { get; set; } = [];
    private IReadOnlyList<string> SentenceLines { get; set; } = [];
    private string? SelectedFileName { get; set; }
    private int TimeLimitInSeconds { get; set; } = 30;
    private int CurrentSentenceIndex { get; set; } = -1;
    private int CurrentSentenceNumber => CurrentSentenceIndex >= 0 ? CurrentSentenceIndex + 1 : 0;
    private int TotalSentenceCount => SentenceLines.Count;
    private int DisplaySentenceNumber
    {
        get
        {
            if (TotalSentenceCount == 0)
            {
                return 0;
            }

            if (IsRoundActive)
            {
                return CurrentSentenceNumber;
            }

            if (CurrentRound?.IsCompleted == true)
            {
                return Math.Min(CurrentSentenceNumber + 1, TotalSentenceCount);
            }

            return CurrentSentenceNumber == 0 ? 1 : CurrentSentenceNumber;
        }
    }

    private bool IsFileComplete => TotalSentenceCount > 0 && CurrentSentenceIndex >= TotalSentenceCount - 1 && CurrentRound?.IsCompleted == true;
    private bool IsRoundActive => CurrentRound?.Status == RoundStatus.Started;
    private bool CanStart => !string.IsNullOrWhiteSpace(SelectedFileName) &&
        SentenceLines.Count > 0 &&
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
    private IPlayerRegistryService PlayerRegistry { get; set; } = default!;

    [Inject]
    private ISentenceFileService SentenceFileService { get; set; } = default!;

    [Inject]
    private IGameStateService GameState { get; set; } = default!;

    [Inject]
    private ISessionStorageService SessionStorageService { get; set; } = default!;

    protected override void OnInitialized()
    {
        CurrentRound = GameState.CurrentRound;
        RefreshPlayers();
        RefreshPlayerResults();
        LoadSentenceFiles();
        PlayerRegistry.PlayersChanged += HandlePlayersChanged;
        GameState.CurrentRoundChanged += HandleCurrentRoundChanged;
        GameState.PlayerRoundStateChanged += HandlePlayerRoundStateChanged;
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
            ErrorMessage = "Open the host Home page before using Host Sentence Scrambler.";
        }

        IsSessionChecked = true;
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        PlayerRegistry.PlayersChanged -= HandlePlayersChanged;
        GameState.CurrentRoundChanged -= HandleCurrentRoundChanged;
        GameState.PlayerRoundStateChanged -= HandlePlayerRoundStateChanged;
        StopTimer();
        await ValueTask.CompletedTask;
    }

    private void SelectSentenceFile(string fileName)
    {
        SelectedFileName = fileName;
        CurrentSentenceIndex = -1;
        CurrentRound = null;
        RemainingTime = TimeSpan.Zero;
        StopTimer();
        LoadSelectedSentenceLines();
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
            var nextSentenceIndex = CurrentSentenceIndex + 1;
            var sentence = new SentenceDefinition
            {
                Text = SentenceLines[nextSentenceIndex],
                TimeLimitInSeconds = TimeLimitInSeconds
            };
            var expectedPlayerNames = Players
                .Select(player => player.DisplayName)
                .ToArray();

            CurrentSentenceIndex = nextSentenceIndex;
            CurrentRound = GameState.StartRound(sentence, expectedPlayerNames);
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

        CurrentRound = GameState.CompleteCurrentRound();
        StopTimer();
        RefreshPlayerResults();
    }

    private void LoadSentenceFiles()
    {
        try
        {
            SentenceFiles = SentenceFileService.GetSentenceFiles();
            SelectedFileName = SentenceFiles.FirstOrDefault()?.FileName;
            LoadSelectedSentenceLines();
            FileLoadError = null;
        }
        catch (InvalidOperationException exception)
        {
            SentenceFiles = [];
            SelectedFileName = null;
            SentenceLines = [];
            FileLoadError = exception.Message;
        }
    }

    private void LoadSelectedSentenceLines()
    {
        if (string.IsNullOrWhiteSpace(SelectedFileName))
        {
            SentenceLines = [];
            return;
        }

        try
        {
            SentenceLines = SentenceFileService.LoadSentenceLines(SelectedFileName);
            FileLoadError = null;
        }
        catch (InvalidOperationException exception)
        {
            SentenceLines = [];
            FileLoadError = exception.Message;
        }
        catch (ArgumentException exception)
        {
            SentenceLines = [];
            FileLoadError = exception.Message;
        }
    }

    private void RefreshPlayers()
    {
        Players = PlayerRegistry.GetPlayers();
    }

    private void RefreshPlayerResults()
    {
        var roundResults = GameState.GetCurrentRoundResults();
        var resultsByPlayerName = roundResults?.Results.ToDictionary(result => result.PlayerName, StringComparer.OrdinalIgnoreCase)
            ?? [];
        var totalScoresByPlayerName = GameState.GetPlayerTotalScores()
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
                    SubmittedSentence = result?.SubmittedSentence,
                    SubmittedWords = BuildSubmittedWords(result?.SubmittedSentence, CurrentRound?.OriginalSentences),
                    Score = result?.CorrectnessCount,
                    TotalScore = result?.TotalSentenceCount,
                    AggregateScore = totalScore?.Score,
                    AggregateTotalScore = totalScore?.TotalScore
                };
            })
            .ToArray();
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
        timerCancellation = new CancellationTokenSource();
        timerTask = RunTimerAsync(timerCancellation.Token);
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
                        CurrentRound = GameState.CompleteCurrentRound();
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
        if (timerCancellation is null)
        {
            return;
        }

        timerCancellation.Cancel();
        timerCancellation.Dispose();
        timerCancellation = null;
        timerTask = null;
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

    private static string GetSubmittedWordStyle(bool isCorrect)
    {
        return isCorrect
            ? "background-color: #dcfce7; color: #166534; border-radius: 4px; padding: 0 4px; font-weight: 600;"
            : string.Empty;
    }

    private static IReadOnlyList<SubmittedWordPart> BuildSubmittedWords(
        string? submittedSentence,
        IReadOnlyList<string>? correctWords)
    {
        if (string.IsNullOrWhiteSpace(submittedSentence))
        {
            return [];
        }

        var submittedWords = SplitWords(submittedSentence);
        correctWords ??= [];

        return submittedWords
            .Select((word, index) => new SubmittedWordPart
            {
                Text = word,
                IsCorrect = index < correctWords.Count && word == correctWords[index]
            })
            .ToArray();
    }

    private static IReadOnlyList<string> SplitWords(string sentence)
    {
        return sentence
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();
    }

    private sealed record HostPlayerResultRow
    {
        public required string PlayerName { get; init; }

        public TimeSpan? TimeBeforeSubmit { get; init; }

        public double TimeBeforeSubmitSeconds => TimeBeforeSubmit?.TotalSeconds ?? -1;

        public string? SubmittedSentence { get; init; }

        public IReadOnlyList<SubmittedWordPart> SubmittedWords { get; init; } = [];

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

    private sealed record SubmittedWordPart
    {
        public required string Text { get; init; }

        public required bool IsCorrect { get; init; }
    }
}
