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
var sentenceScramblerPath = Path.IsPathRooted(sentenceScramblerOptions.SentenceScramblerPath)
    ? sentenceScramblerOptions.SentenceScramblerPath
    : Path.Combine(builder.Environment.ContentRootPath, sentenceScramblerOptions.SentenceScramblerPath);

builder.Services.AddSingleton(sentenceScramblerOptions);
builder.Services.AddSingleton<ISentenceFileService>(_ => new SentenceFileService(sentenceScramblerPath));

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
