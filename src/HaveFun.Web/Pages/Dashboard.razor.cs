using HaveFun.Core;
using Microsoft.AspNetCore.Components;

namespace HaveFun.Web;

public partial class Dashboard : ComponentBase
{
    private string? LanUrl { get; set; }

    private int PlayerCount { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IJoinUrlProvider JoinUrlProvider { get; set; } = default!;

    [Inject]
    private IPlayerRegistry PlayerRegistry { get; set; } = default!;

    protected override void OnInitialized()
    {
        var urls = JoinUrlProvider.GetJoinUrls(new Uri(NavigationManager.BaseUri));

        LanUrl = urls.LanUrl ?? urls.LocalhostUrl;
        PlayerCount = PlayerRegistry.GetPlayers().Count;
    }
}
