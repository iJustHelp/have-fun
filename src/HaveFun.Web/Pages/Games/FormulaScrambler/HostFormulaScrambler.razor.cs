using HaveFun.Core;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace HaveFun.Web;

public partial class HostFormulaScrambler : ComponentBase, IAsyncDisposable
{
    private const int FirstSubmittedPlayerBonus = 1;
    private CancellationTokenSource? _timerCancellation;
    private Task? _timerTask;
    private bool IsSessionChecked { get; set; }
    private string? ErrorMessage { get; set; }
    private string? FileLoadError { get; set; }
    private IReadOnlyList<GameFilePathOption> FormulaFiles { get; set; } = [];
    private IReadOnlyList<string> FormulaLines { get; set; } = [];
    private string? SelectedFileName { get; set; }
    private int TimeLimitInSeconds { get; set; } = 30;
    private int CurrentFormulaIndex { get; set; } = -1;
    private int CurrentFormulaNumber => CurrentFormulaIndex >= 0 ? CurrentFormulaIndex + 1 : 0;
    private int TotalFormulaCount => FormulaLines.Count;
    private int DisplayFormulaNumber
    {
        get
        {
            if (TotalFormulaCount == 0)
            {
                return 0;
            }

            if (IsRoundActive)
            {
                return CurrentFormulaNumber;
            }

            if (CurrentRound?.IsCompleted == true)
            {
                return Math.Min(CurrentFormulaNumber + 1, TotalFormulaCount);
            }

            return CurrentFormulaNumber == 0 ? 1 : CurrentFormulaNumber;
        }
    }

    private bool IsFileComplete => TotalFormulaCount > 0 && CurrentFormulaIndex >= TotalFormulaCount - 1 && CurrentRound?.IsCompleted == true;
    private bool IsRoundActive => CurrentRound?.Status == RoundStatus.Started;
    private bool CanStart => !string.IsNullOrWhiteSpace(SelectedFileName) &&
        FormulaLines.Count > 0 &&
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
    private FormulaScramblerFileService FormulaFileService { get; set; } = default!;

    [Inject]
    private FormulaScramblerGameStateService GameStateService { get; set; } = default!;

    [Inject]
    private FormulaScramblerService FormulaScramblerService { get; set; } = default!;

    [Inject]
    private ISessionStorageService SessionStorageService { get; set; } = default!;

    protected override void OnInitialized()
    {
        CurrentRound = GameStateService.CurrentRound;
        RefreshPlayers();
        RefreshPlayerResults();
        LoadFormulaFiles();
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
            ErrorMessage = "Open the host Home page before using Host Formula Scrambler.";
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

    private void SelectFormulaFile(string fileName)
    {
        SelectedFileName = fileName;
        CurrentFormulaIndex = -1;
        CurrentRound = null;
        RemainingTime = TimeSpan.Zero;
        StopTimer();
        LoadSelectedFormulaLines();
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
            var nextFormulaIndex = CurrentFormulaIndex + 1;
            var sentence = new TextDefinition
            {
                Text = FormulaLines[nextFormulaIndex],
                TimeLimitInSeconds = TimeLimitInSeconds
            };
            var expectedPlayerNames = Players
                .Select(player => player.DisplayName)
                .ToArray();

            CurrentFormulaIndex = nextFormulaIndex;
            CurrentRound = GameStateService.StartRound(
                sentence,
                expectedPlayerNames,
                FormulaScramblerService.CreateFormulaTiles,
                FormulaScramblerService.CalculateScore);
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

    private void LoadFormulaFiles()
    {
        try
        {
            FormulaFiles = FormulaFileService.GetGameFilePathes();
            SelectedFileName = FormulaFiles.FirstOrDefault()?.FilePath;
            LoadSelectedFormulaLines();
            FileLoadError = null;
        }
        catch (InvalidOperationException exception)
        {
            FormulaFiles = [];
            SelectedFileName = null;
            FormulaLines = [];
            FileLoadError = exception.Message;
        }
    }

    private void LoadSelectedFormulaLines()
    {
        if (string.IsNullOrWhiteSpace(SelectedFileName))
        {
            FormulaLines = [];
            return;
        }

        try
        {
            FormulaLines = FormulaFileService.LoadLines(SelectedFileName);
            FileLoadError = null;
        }
        catch (InvalidOperationException exception)
        {
            FormulaLines = [];
            FileLoadError = exception.Message;
        }
        catch (ArgumentException exception)
        {
            FormulaLines = [];
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
                    SubmitTime = result?.SpentTime,
                    SubmittedFormulaCharacters = BuildSubmittedFormulaCharacters(result?.SelectedTiles, CurrentRound?.SentenceText),
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

        var submittedPlayerRoundStates = GameStateService.GetSubmittedPlayerRoundStates()
            .Where(playerRoundState =>
                playerRoundState.RoundId == currentRound.Id &&
                playerRoundState.SelectedTiles.Count > 0 &&
                playerRoundState.SpentTime is not null &&
                playerRoundState.SubmittedAt is not null)
            .ToArray();
        var totalFormulaCharacterCount = FormulaScramblerService.GetTotalScore(currentRound.SentenceText);
        var firstSubmittedPlayer = submittedPlayerRoundStates
            .Select(playerRoundState => new
            {
                playerRoundState.PlayerName,
                playerRoundState.SubmittedAt,
                BaseCorrectnessCount = FormulaScramblerService.CalculateScore(currentRound, playerRoundState.SelectedTiles)
            })
            .OrderBy(playerRoundState => playerRoundState.SubmittedAt)
            .ThenBy(playerRoundState => playerRoundState.PlayerName, StringComparer.Ordinal)
            .FirstOrDefault();
        var isFirstSubmittedPlayerCorrect = firstSubmittedPlayer?.BaseCorrectnessCount == totalFormulaCharacterCount;
        var firstSubmittedPlayerName = isFirstSubmittedPlayerCorrect
            ? firstSubmittedPlayer?.PlayerName
            : null;
        var rankedResults = submittedPlayerRoundStates
            .Select(playerRoundState => new
            {
                playerRoundState.PlayerName,
                playerRoundState.SelectedTiles,
                BaseCorrectnessCount = FormulaScramblerService.CalculateScore(currentRound, playerRoundState.SelectedTiles),
                SpentTime = playerRoundState.SpentTime!.Value,
                SubmittedAt = playerRoundState.SubmittedAt!.Value
            })
            .Select(playerResult => new
            {
                playerResult.PlayerName,
                playerResult.SelectedTiles,
                CorrectnessCount = playerResult.BaseCorrectnessCount +
                    (isFirstSubmittedPlayerCorrect &&
                        string.Equals(playerResult.PlayerName, firstSubmittedPlayerName, StringComparison.OrdinalIgnoreCase)
                            ? FirstSubmittedPlayerBonus
                            : 0),
                TotalFormulaCharacterCount = totalFormulaCharacterCount,
                playerResult.SpentTime,
                playerResult.SubmittedAt
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
                TotalSentenceCount = playerResult.TotalFormulaCharacterCount,
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

    private static string FormatSubmitTime(TimeSpan? submitTime)
    {
        if (submitTime is null)
        {
            return string.Empty;
        }

        var value = submitTime.Value <= TimeSpan.Zero ? TimeSpan.Zero : submitTime.Value;
        return $"{(int)value.TotalMinutes:00}:{value.Seconds:00}";
    }

    private static string FormatScore(int? score, int? totalScore)
    {
        return score is null || totalScore is null ? string.Empty : $"{score} / {totalScore}";
    }

    private static string GetSubmittedCharacterStyle(bool isCorrect)
    {
        return isCorrect
            ? "background-color: #dcfce7; color: #166534; border-radius: 4px; padding: 0 4px; font-weight: 600;"
            : string.Empty;
    }

    private IReadOnlyList<SubmittedFormulaCharacterPart> BuildSubmittedFormulaCharacters(
        IReadOnlyList<Tile>? submittedTiles,
        string? correctFormula)
    {
        if (submittedTiles is null || submittedTiles.Count == 0)
        {
            return [];
        }

        var submittedCharacters = submittedTiles.Select(tile => tile.Text).ToArray();
        var correctCharacters = correctFormula is null
            ? []
            : FormulaScramblerService.NormalizeFormula(correctFormula).Select(character => character.ToString()).ToArray();

        return submittedCharacters
            .Select((character, index) => new SubmittedFormulaCharacterPart
            {
                Text = character,
                IsCorrect = index < correctCharacters.Length && character == correctCharacters[index]
            })
            .ToArray();
    }

    private sealed record HostPlayerResultRow
    {
        public required string PlayerName { get; init; }

        public TimeSpan? SubmitTime { get; init; }

        public double SubmitTimeSeconds => SubmitTime?.TotalSeconds ?? -1;

        public IReadOnlyList<SubmittedFormulaCharacterPart> SubmittedFormulaCharacters { get; init; } = [];

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

    private sealed record SubmittedFormulaCharacterPart
    {
        public required string Text { get; init; }

        public required bool IsCorrect { get; init; }
    }
}
