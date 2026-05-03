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

builder.Services.AddSingleton<ISentenceLibrary>(_ =>
{
    var sentenceFilePath = Path.Combine(builder.Environment.ContentRootPath, "assets", "sentences.json");
    var sentences = SentenceFileLoader.Load(sentenceFilePath);

    return new InMemorySentenceLibrary(sentences);
});
builder.Services.AddSingleton<IPlayerRegistry, PlayerRegistry>();
builder.Services.AddSingleton<IGameState, GameState>();
builder.Services.AddSingleton<IJoinUrlProvider, JoinUrlProvider>();
builder.Services.AddScoped<IUserSessionStorage, UserSessionStorage>();

var app = builder.Build();

_ = app.Services.GetRequiredService<IOptions<GameOptions>>().Value;
_ = app.Services.GetRequiredService<ISentenceLibrary>();
_ = app.Services.GetRequiredService<IJoinUrlProvider>();

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
