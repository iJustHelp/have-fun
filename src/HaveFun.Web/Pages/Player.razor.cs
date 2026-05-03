using HaveFun.Core;
using Microsoft.AspNetCore.Components;

namespace HaveFun.Web;

public partial class Player : ComponentBase
{
    private bool IsSessionChecked { get; set; }

    private string? DisplayName { get; set; }

    private string? ErrorMessage { get; set; }

    [Inject]
    private IPlayerRegistryService PlayerRegistry { get; set; } = default!;

    [Inject]
    private IUserSessionStorageService UserSessionStorage { get; set; } = default!;

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
        IsSessionChecked = true;
        StateHasChanged();
    }
}
