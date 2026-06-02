using HaveFun.Core;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace HaveFun.Web;

public partial class PlayerSpellingBee : ComponentBase, IAsyncDisposable
{
    private CancellationTokenSource? _timerCancellation;
    private Task? _timerTask;

    private bool IsSessionChecked { get; set; }

    private string? DisplayName { get; set; }

    private string? ErrorMessage { get; set; }

    private CurrentRound? CurrentRound { get; set; }

    private string? PlayerName { get; set; }

    private PlayerRoundState? PlayerRoundState { get; set; }

    private TimeSpan RemainingTime { get; set; }

    private string RemainingTimeText => $"{(int)RemainingTime.TotalMinutes:00}:{RemainingTime.Seconds:00}";
    private bool CanSubmit => CurrentRound?.Status == RoundStatus.Started && PlayerRoundState?.IsSubmitted != true;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IPlayerRegistryService PlayerRegistry { get; set; } = default!;

    [Inject]
    private ISessionStorageService UserSessionStorageService { get; set; } = default!;

    [Inject]
    private SpellingBeeGameStateService GameState { get; set; } = default!;

    [Inject]
    private SentenceScramblerGameStateService SentenceScramblerGameState { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        var currentUser = await UserSessionStorageService.GetCurrentUserAsync();

        if (currentUser?.Role == Role.Host)
        {
            NavigationManager.NavigateTo("/host-spelling-bee", replace: true);
            return;
        }

        if (currentUser?.Role != Role.Player)
        {
            await RedirectToRegisterAsync();
            return;
        }

        if (!PlayerRegistry.TryGetPlayerByName(currentUser.Name, out var registeredPlayer) || registeredPlayer is null)
        {
            await RedirectToRegisterAsync();
            return;
        }

        CurrentRound = GameState.CurrentRound;

        if (CurrentRound?.Status != RoundStatus.Started)
        {
            NavigationManager.NavigateTo("/waiting-room", replace: true);
            return;
        }

        DisplayName = registeredPlayer.DisplayName;
        PlayerName = registeredPlayer.DisplayName;
        RefreshPlayerRoundState();
        PlayerRegistry.PlayerRemoved += HandlePlayerRemoved;
        GameState.CurrentRoundChanged += HandleCurrentRoundChanged;
        SentenceScramblerGameState.CurrentRoundChanged += HandleSentenceScramblerRoundChanged;
        StartTimerIfRoundIsActive();
        IsSessionChecked = true;
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        PlayerRegistry.PlayerRemoved -= HandlePlayerRemoved;
        GameState.CurrentRoundChanged -= HandleCurrentRoundChanged;
        SentenceScramblerGameState.CurrentRoundChanged -= HandleSentenceScramblerRoundChanged;
        StopTimer();
        await ValueTask.CompletedTask;
    }

    private void HandlePlayerRemoved(PlayerSession player)
    {
        if (!string.Equals(PlayerName, player.DisplayName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _ = InvokeAsync(RedirectToRegisterAsync);
    }

    private void HandleCurrentRoundChanged(CurrentRound round)
    {
        _ = InvokeAsync(() =>
        {
            CurrentRound = round;
            RefreshPlayerRoundState();
            StartTimerIfRoundIsActive();
            StateHasChanged();
        });
    }

    private void HandleSentenceScramblerRoundChanged(CurrentRound round)
    {
        if (round.Status != RoundStatus.Started)
        {
            return;
        }

        _ = InvokeAsync(() => NavigationManager.NavigateTo("/player-sentence-scrambler", replace: true));
    }

    private async Task SubmitRound(IReadOnlyList<Tile> selectedTiles)
    {
        if (PlayerName is null || CurrentRound?.Status != RoundStatus.Started)
        {
            return;
        }

        PlayerRoundState = GameState.SubmitPlayerRound(PlayerName, selectedTiles);
        await Task.CompletedTask;
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

        var elapsed = DateTimeOffset.UtcNow - CurrentRound.StartedAt.Value;
        var remaining = TimeSpan.FromSeconds(CurrentRound.TimeLimitInSeconds) - elapsed;
        RemainingTime = remaining <= TimeSpan.Zero ? TimeSpan.Zero : remaining;
    }

    private void RefreshPlayerRoundState()
    {
        if (PlayerName is null || CurrentRound is null)
        {
            PlayerRoundState = null;
            return;
        }

        PlayerRoundState = GameState.GetOrCreatePlayerRoundState(PlayerName);
    }

    private async Task RedirectToRegisterAsync()
    {
        await UserSessionStorageService.ClearCurrentUserAsync();
        NavigationManager.NavigateTo("/register", replace: true);
    }
}
