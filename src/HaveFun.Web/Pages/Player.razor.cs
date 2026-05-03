using HaveFun.Core;
using Microsoft.AspNetCore.Components;

namespace HaveFun.Web;

public partial class Player : ComponentBase
{
    [Parameter]
    public Guid PlayerId { get; set; }

    private bool IsSessionChecked { get; set; }

    private string? DisplayName { get; set; }

    private string? ErrorMessage { get; set; }

    [Inject]
    private IPlayerRegistry PlayerRegistry { get; set; } = default!;

    [Inject]
    private IPlayerSessionStorage PlayerSessionStorage { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        if (!PlayerRegistry.TryGetPlayer(PlayerId, out var registeredPlayer) || registeredPlayer is null)
        {
            ErrorMessage = "This player session is no longer active. Join again to continue.";
            IsSessionChecked = true;
            StateHasChanged();
            return;
        }

        var storedPlayer = await PlayerSessionStorage.GetCurrentPlayerAsync();

        if (storedPlayer is null || storedPlayer.PlayerId != PlayerId)
        {
            ErrorMessage = "This browser tab is not joined as that player. Join again to continue.";
            IsSessionChecked = true;
            StateHasChanged();
            return;
        }

        DisplayName = registeredPlayer.DisplayName;
        IsSessionChecked = true;
        StateHasChanged();
    }
}
