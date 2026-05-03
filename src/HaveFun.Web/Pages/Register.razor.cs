using HaveFun.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace HaveFun.Web;

public partial class Register : ComponentBase
{
    private string? LanUrl { get; set; }

    private string SubmittedName { get; set; } = string.Empty;

    private string? ValidationError { get; set; }

    private bool IsJoining { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IJoinUrlProviderService JoinUrlProvider { get; set; } = default!;

    [Inject]
    private IOptions<GameOptions> GameOptions { get; set; } = default!;

    [Inject]
    private IPlayerRegistryService PlayerRegistry { get; set; } = default!;

    [Inject]
    private IUserSessionStorageService UserSessionStorage { get; set; } = default!;

    protected override void OnInitialized()
    {
        var urls = JoinUrlProvider.GetJoinUrls(new Uri(NavigationManager.BaseUri));

        LanUrl = urls.LanUrl;
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
            await UserSessionStorage.SaveCurrentUserAsync(new StoredUserSession
            {
                Name = submittedName,
                Role = JoinRole.Master,
            });
            NavigationManager.NavigateTo("/dashboard");
            return;
        }

        var result = PlayerRegistry.RegisterPlayer(submittedName);

        if (!result.IsSuccess)
        {
            ValidationError = result.ValidationError ?? "Unable to join with that name.";
            IsJoining = false;
            return;
        }

        await UserSessionStorage.SaveCurrentUserAsync(new StoredUserSession
        {
            Name = result.DisplayName,
            Role = JoinRole.Player,
        });

        NavigationManager.NavigateTo("/player");
    }
}
