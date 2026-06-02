using HaveFun.Core;
using HaveFun.Web;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();

var sentenceScramblerOptions = new SentenceScramblerOptions
{
    SentenceScramblerPath = builder.Configuration["Game:SentenceScramblerPath"]
        ?? Path.Combine("assets", "sentence-scrambler")
};
var sentenceScramblerPath = ResolveContentPath(builder, sentenceScramblerOptions.SentenceScramblerPath);
var wordScramblerPath = ResolveContentPath(
    builder,
    builder.Configuration["Game:WordScramblerPath"] ?? Path.Combine("assets", "word-scrambler"));

builder.Services.AddSingleton(sentenceScramblerOptions);
builder.Services.AddSingleton(_ => new SentenceScramblerFileService(sentenceScramblerPath));
builder.Services.AddSingleton(_ => new WordScramblerFileService(wordScramblerPath));

builder.Services.AddCoreServices();

var app = builder.Build();

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

static string ResolveContentPath(WebApplicationBuilder builder, string path)
{
    return Path.IsPathRooted(path)
        ? path
        : Path.Combine(builder.Environment.ContentRootPath, path);
}
