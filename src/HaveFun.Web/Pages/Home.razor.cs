using HaveFun.Core;
using Microsoft.AspNetCore.Components;

namespace HaveFun.Web;

public partial class Home : ComponentBase
{
    private string RegisterUrl { get; set; } = string.Empty;

    private string? QrCodeDataUri { get; set; }

    private string? QrCodeError { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IUrlService UrlService { get; set; } = default!;

    [Inject]
    private IQrCodeService QrCodeService { get; set; } = default!;

    [Inject]
    private ISessionStorageService UserSessionStorageService { get; set; } = default!;

    protected override void OnInitialized()
    {
        var urls = UrlService.GetLanBaseUrl(NavigationManager.BaseUri);
        var baseUrl = urls ?? NavigationManager.BaseUri;

        RegisterUrl = BuildRegisterUrl(baseUrl);
        CreateQrCode();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        var currentUser = await UserSessionStorageService.GetCurrentUserAsync();

        if (currentUser is null)
        {
            await UserSessionStorageService.SaveCurrentUserAsync(new SessionStorageModel
            {
                Name = "Host",
                Role = HaveFun.Core.Role.Host,
            });
        }
    }

    private static string BuildRegisterUrl(string baseUrl)
    {
        return new Uri(new Uri(baseUrl), "register").ToString();
    }

    private void CreateQrCode()
    {
        try
        {
            QrCodeDataUri = QrCodeService.CreateSvgDataUri(RegisterUrl);
            QrCodeError = null;
        }
        catch (InvalidOperationException)
        {
            QrCodeDataUri = null;
            QrCodeError = "The player registration URL is too long to display as a QR code.";
        }
    }
}
