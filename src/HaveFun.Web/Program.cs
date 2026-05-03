using HaveFun.Core;
using HaveFun.Web;
using Microsoft.Extensions.Options;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();

builder.Services.AddOptions<GameOptions>()
    .Bind(builder.Configuration.GetSection(GameOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.MasterName), "Game:MasterName is required.")
    .ValidateOnStart();

builder.Services.AddSingleton<ISentenceLibraryService>(_ =>
{
    var sentenceFilePath = Path.Combine(builder.Environment.ContentRootPath, "assets", "sentences.json");
    var sentences = SentenceFileLoaderService.Load(sentenceFilePath);

    return new InMemorySentenceLibraryService(sentences);
});
builder.Services.AddSingleton<IPlayerRegistryService, PlayerRegistryService>();
builder.Services.AddSingleton<IGameStateService, GameStateService>();
builder.Services.AddSingleton<IJoinUrlProviderService, JoinUrlProviderService>();
builder.Services.AddScoped<IUserSessionStorageService, UserSessionStorageService>();

var app = builder.Build();

_ = app.Services.GetRequiredService<IOptions<GameOptions>>().Value;
_ = app.Services.GetRequiredService<ISentenceLibraryService>();
_ = app.Services.GetRequiredService<IJoinUrlProviderService>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<HaveFun.Web.App>()
    .AddInteractiveServerRenderMode();

app.Run();
