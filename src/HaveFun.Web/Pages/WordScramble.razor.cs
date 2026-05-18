using HaveFun.Core;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace HaveFun.Web;

public partial class WordScramble : ComponentBase, IAsyncDisposable
{
    private CancellationTokenSource? timerCancellation;

    private Task? timerTask;

    private bool IsSessionChecked { get; set; }

    private string? DisplayName { get; set; }

    private string? ErrorMessage { get; set; }

    private CurrentRound? CurrentRound { get; set; }

    private string? PlayerName { get; set; }

    private PlayerRoundState? PlayerRoundState { get; set; }

    private TimeSpan RemainingTime { get; set; }

    private string RemainingTimeText => $"{(int)RemainingTime.TotalMinutes:00}:{RemainingTime.Seconds:00}";

    private int AvailableWordCount => PlayerRoundState?.AvailableWords.Count ?? CurrentRound?.ShuffledWords.Count ?? 0;

    private bool CanSubmit => PlayerRoundState?.CanSubmit == true;

    private bool IsTimerExpired => CurrentRound is not null && RemainingTime == TimeSpan.Zero;

    private Color RoundStatusColor => CurrentRound?.Status == RoundStatus.Started ? Color.Success : Color.Default;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IPlayerRegistryService PlayerRegistry { get; set; } = default!;

    [Inject]
    private ISessionStorageService UserSessionStorageService { get; set; } = default!;

    [Inject]
    private IGameStateService GameState { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        var currentUser = await UserSessionStorageService.GetCurrentUserAsync();

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

        if (CurrentRound is null)
        {
            NavigationManager.NavigateTo("/waiting-room", replace: true);
            return;
        }

        DisplayName = registeredPlayer.DisplayName;
        PlayerName = registeredPlayer.DisplayName;
        RefreshPlayerRoundState();
        PlayerRegistry.PlayerRemoved += HandlePlayerRemoved;
        GameState.CurrentRoundChanged += HandleCurrentRoundChanged;
        StartTimerIfRoundIsActive();
        IsSessionChecked = true;
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        PlayerRegistry.PlayerRemoved -= HandlePlayerRemoved;
        GameState.CurrentRoundChanged -= HandleCurrentRoundChanged;
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

    private void SelectWord(Guid wordId)
    {
        if (PlayerName is null)
        {
            return;
        }

        PlayerRoundState = GameState.SelectWord(PlayerName, wordId);
    }

    private void SubmitRound()
    {
        if (PlayerName is null)
        {
            return;
        }

        PlayerRoundState = GameState.SubmitPlayerRound(PlayerName);
    }

    private void StartTimerIfRoundIsActive()
    {
        StopTimer();

        if (CurrentRound is null)
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

    private static string FormatSpentTime(TimeSpan spentTime)
    {
        return $"{(int)spentTime.TotalMinutes:00}:{spentTime.Seconds:00}";
    }
}
