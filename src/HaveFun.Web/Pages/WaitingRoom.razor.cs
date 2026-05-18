using HaveFun.Core;
using Microsoft.AspNetCore.Components;

namespace HaveFun.Web;

public partial class WaitingRoom : ComponentBase, IAsyncDisposable
{
    private bool IsSessionChecked { get; set; }

    private string? DisplayName { get; set; }

    private string? PlayerName { get; set; }

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

        if (GameState.CurrentRound is not null)
        {
            NavigationManager.NavigateTo("/word-scramble", replace: true);
            return;
        }

        DisplayName = registeredPlayer.DisplayName;
        PlayerName = registeredPlayer.DisplayName;
        PlayerRegistry.PlayerRemoved += HandlePlayerRemoved;
        GameState.CurrentRoundChanged += HandleCurrentRoundChanged;
        IsSessionChecked = true;
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        PlayerRegistry.PlayerRemoved -= HandlePlayerRemoved;
        GameState.CurrentRoundChanged -= HandleCurrentRoundChanged;
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
        _ = InvokeAsync(() => NavigationManager.NavigateTo("/word-scramble", replace: true));
    }

    private async Task RedirectToRegisterAsync()
    {
        await UserSessionStorageService.ClearCurrentUserAsync();
        NavigationManager.NavigateTo("/register", replace: true);
    }
}
