using HaveFun.Core;
using Microsoft.AspNetCore.Components;

namespace HaveFun.Web;

public partial class Home : ComponentBase
{
    private string LocalhostUrl { get; set; } = string.Empty;

    private string? LanUrl { get; set; }

    private int SentenceCount { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IJoinUrlProvider JoinUrlProvider { get; set; } = default!;

    [Inject]
    private ISentenceLibrary SentenceLibrary { get; set; } = default!;

    [Inject]
    private IUserSessionStorage UserSessionStorage { get; set; } = default!;

    protected override void OnInitialized()
    {
        var urls = JoinUrlProvider.GetJoinUrls(new Uri(NavigationManager.BaseUri));

        LocalhostUrl = urls.LocalhostUrl;
        LanUrl = urls.LanUrl;
        SentenceCount = SentenceLibrary.Sentences.Count;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        var currentUser = await UserSessionStorage.GetCurrentUserAsync();

        if (currentUser is null)
        {
            NavigationManager.NavigateTo("/");
        }
    }
}
