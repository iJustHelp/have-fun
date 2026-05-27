using HaveFun.Core;
using Microsoft.AspNetCore.Components;

namespace HaveFun.Web;

public partial class Register : ComponentBase
{
    private string? _lanUrl;
    private string _submittedName = string.Empty;
    private string? _validationError;
    private bool _isJoining;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IUrlService UrlService { get; set; } = default!;

    [Inject]
    private IPlayerRegistryService PlayerRegistry { get; set; } = default!;

    [Inject]
    private ISessionStorageService UserSessionStorageService { get; set; } = default!;

    protected override void OnInitialized()
    {
        var baseUrl = UrlService.GetLanBaseUrl(NavigationManager.BaseUri) ?? NavigationManager.BaseUri;
        _lanUrl = BuildRegisterUrl(baseUrl);
    }

    private async Task JoinAsync()
    {
        if (_isJoining)
        {
            return;
        }

        _isJoining = true;
        _validationError = null;

        var submittedName = _submittedName.Trim();

        if (string.IsNullOrWhiteSpace(submittedName))
        {
            _validationError = "Name is required.";
            _isJoining = false;
            return;
        }

        var result = PlayerRegistry.RegisterPlayer(submittedName);

        if (!result.IsSuccess)
        {
            _validationError = result.ValidationError ?? "Unable to join with that name.";
            _isJoining = false;
            return;
        }

        await UserSessionStorageService.SaveCurrentUserAsync(new SessionStorageModel
        {
            Name = result.DisplayName,
            Role = Role.Player,
        });

        NavigationManager.NavigateTo("/waiting-room");
    }

    private static string BuildRegisterUrl(string baseUrl)
    {
        return new Uri(new Uri(baseUrl), "register").ToString();
    }
}
