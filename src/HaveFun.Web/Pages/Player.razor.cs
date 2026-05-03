using HaveFun.Core;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace HaveFun.Web;

public partial class Player : ComponentBase, IAsyncDisposable
{
    private CancellationTokenSource? timerCancellation;

    private Task? timerTask;

    private bool IsSessionChecked { get; set; }

    private string? DisplayName { get; set; }

    private string? ErrorMessage { get; set; }

    private CurrentRound? CurrentRound { get; set; }

    private TimeSpan RemainingTime { get; set; }

    private string RemainingTimeText => $"{(int)RemainingTime.TotalMinutes:00}:{RemainingTime.Seconds:00}";

    private bool IsTimerExpired => CurrentRound is not null && RemainingTime == TimeSpan.Zero;

    private Color RoundStatusColor => CurrentRound?.Status == RoundStatus.Started ? Color.Success : Color.Default;

    [Inject]
    private IPlayerRegistryService PlayerRegistry { get; set; } = default!;

    [Inject]
    private IUserSessionStorageService UserSessionStorage { get; set; } = default!;

    [Inject]
    private IGameStateService GameState { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        var currentUser = await UserSessionStorage.GetCurrentUserAsync();

        if (currentUser?.Role != JoinRole.Player)
        {
            ErrorMessage = "This browser tab is not joined as a player. Join again to continue.";
            IsSessionChecked = true;
            StateHasChanged();
            return;
        }

        if (!PlayerRegistry.TryGetPlayerByName(currentUser.Name, out var registeredPlayer) || registeredPlayer is null)
        {
            ErrorMessage = "This player session is no longer active. Join again to continue.";
            IsSessionChecked = true;
            StateHasChanged();
            return;
        }

        DisplayName = registeredPlayer.DisplayName;
        CurrentRound = GameState.CurrentRound;
        GameState.CurrentRoundChanged += HandleCurrentRoundChanged;
        StartTimerIfRoundIsActive();
        IsSessionChecked = true;
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        GameState.CurrentRoundChanged -= HandleCurrentRoundChanged;
        StopTimer();
        await ValueTask.CompletedTask;
    }

    private void HandleCurrentRoundChanged(CurrentRound round)
    {
        _ = InvokeAsync(() =>
        {
            CurrentRound = round;
            StartTimerIfRoundIsActive();
            StateHasChanged();
        });
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
}
