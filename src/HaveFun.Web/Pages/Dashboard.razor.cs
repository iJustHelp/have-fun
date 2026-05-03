using HaveFun.Core;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace HaveFun.Web;

public partial class Dashboard : ComponentBase
{
    private bool IsSessionChecked { get; set; }

    private string? ErrorMessage { get; set; }

    private string? LanUrl { get; set; }

    private string MasterName { get; set; } = "Master";

    private int PlayerCount { get; set; }

    private CurrentRound? CurrentRound { get; set; }

    private string RoundStatusText => CurrentRound?.Status.ToString() ?? RoundStatus.NotStarted.ToString();

    private Color RoundStatusColor => CurrentRound?.Status == RoundStatus.Started ? Color.Success : Color.Default;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IJoinUrlProvider JoinUrlProvider { get; set; } = default!;

    [Inject]
    private IPlayerRegistry PlayerRegistry { get; set; } = default!;

    [Inject]
    private IGameState GameState { get; set; } = default!;

    [Inject]
    private IUserSessionStorage UserSessionStorage { get; set; } = default!;

    protected override void OnInitialized()
    {
        var urls = JoinUrlProvider.GetJoinUrls(new Uri(NavigationManager.BaseUri));

        LanUrl = urls.LanUrl ?? urls.LocalhostUrl;
        PlayerCount = PlayerRegistry.GetPlayers().Count;
        CurrentRound = GameState.CurrentRound;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        var currentUser = await UserSessionStorage.GetCurrentUserAsync();

        if (currentUser?.Role != JoinRole.Master)
        {
            ErrorMessage = "Join as the configured master to open the dashboard.";
        }
        else
        {
            MasterName = currentUser.Name;
        }

        IsSessionChecked = true;
        StateHasChanged();
    }
}
