using Microsoft.Extensions.DependencyInjection;

namespace HaveFun.Core;

public static class ServicesExtension
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IPlayerRegistryService, PlayerRegistryService>();
        // Each game needs an isolated singleton because game state is mutable and in-memory.
        services.AddSingleton<SentenceScramblerGameStateService>();
        services.AddSingleton<SpellingBeeGameStateService>();
        services.AddSingleton<FormulaScramblerGameStateService>();
        services.AddSingleton<FormulaScramblerService>();
        services.AddSingleton<IUrlService, UrlService>();
        services.AddSingleton<IQrCodeService, QrCodeService>();
        services.AddScoped<ISessionStorageService, SessionStorageService>();

        return services;
    }
}
