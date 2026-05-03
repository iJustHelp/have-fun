using HaveFun.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace HaveFun.Web;

public partial class Home : ComponentBase
{
    private string LocalhostUrl { get; set; } = string.Empty;

    private string? LanUrl { get; set; }

    private string? PreferredUrl { get; set; }

    private int SentenceCount { get; set; }

    private string SubmittedName { get; set; } = string.Empty;

    private string? ValidationError { get; set; }

    private bool IsJoining { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IJoinUrlProvider JoinUrlProvider { get; set; } = default!;

    [Inject]
    private ISentenceLibrary SentenceLibrary { get; set; } = default!;

    [Inject]
    private IOptions<GameOptions> GameOptions { get; set; } = default!;

    [Inject]
    private IPlayerRegistry PlayerRegistry { get; set; } = default!;

    [Inject]
    private IPlayerSessionStorage PlayerSessionStorage { get; set; } = default!;

    protected override void OnInitialized()
    {
        var urls = JoinUrlProvider.GetJoinUrls(new Uri(NavigationManager.BaseUri));

        LocalhostUrl = urls.LocalhostUrl;
        LanUrl = urls.LanUrl;
        PreferredUrl = urls.PreferredUrl;
        SentenceCount = SentenceLibrary.Sentences.Count;
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender && PreferredUrl is not null)
        {
            NavigationManager.NavigateTo(PreferredUrl, forceLoad: true);
        }
    }

    private async Task JoinAsync()
    {
        if (IsJoining)
        {
            return;
        }

        IsJoining = true;
        ValidationError = null;

        var submittedName = SubmittedName.Trim();

        if (string.IsNullOrWhiteSpace(submittedName))
        {
            ValidationError = "Name is required.";
            IsJoining = false;
            return;
        }

        if (submittedName.Equals(GameOptions.Value.MasterName.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            await PlayerSessionStorage.ClearCurrentPlayerAsync();
            NavigationManager.NavigateTo("/dashboard");
            return;
        }

        var result = PlayerRegistry.RegisterPlayer(submittedName);

        if (!result.IsSuccess || result.PlayerId is null)
        {
            ValidationError = result.ValidationError ?? "Unable to join with that name.";
            IsJoining = false;
            return;
        }

        await PlayerSessionStorage.SaveCurrentPlayerAsync(new StoredPlayerSession
        {
            PlayerId = result.PlayerId.Value,
            DisplayName = result.DisplayName
        });

        NavigationManager.NavigateTo($"/player/{result.PlayerId}");
    }
}
