using Microsoft.Extensions.DependencyInjection;

namespace HaveFun.Core;

public static class ServicesExtension
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IPlayerRegistryService, PlayerRegistryService>();
        services.AddSingleton<IGameStateService, GameStateService>();
        services.AddSingleton<IUrlService, UrlService>();
        services.AddSingleton<IQrCodeService, QrCodeService>();
        services.AddScoped<ISessionStorageService, SessionStorageService>();

        return services;
    }
}
