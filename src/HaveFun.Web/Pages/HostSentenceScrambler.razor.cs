using HaveFun.Core;
using Microsoft.AspNetCore.Components;

namespace HaveFun.Web;

public partial class HostSentenceScrambler : ComponentBase, IAsyncDisposable
{
    private bool IsSessionChecked { get; set; }

    private string? ErrorMessage { get; set; }

    private string? FileLoadError { get; set; }

    private IReadOnlyList<SentenceFileOption> SentenceFiles { get; set; } = [];

    private string? SelectedFileName { get; set; }

    private bool CanStart => !string.IsNullOrWhiteSpace(SelectedFileName);

    private IReadOnlyList<PlayerSession> Players { get; set; } = [];

    [Inject]
    private IPlayerRegistryService PlayerRegistry { get; set; } = default!;

    [Inject]
    private ISentenceFileService SentenceFileService { get; set; } = default!;

    [Inject]
    private IGameStateService GameState { get; set; } = default!;

    [Inject]
    private ISessionStorageService SessionStorageService { get; set; } = default!;

    protected override void OnInitialized()
    {
        RefreshPlayers();
        LoadSentenceFiles();
        PlayerRegistry.PlayersChanged += HandlePlayersChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        var currentUser = await SessionStorageService.GetCurrentUserAsync();

        if (currentUser?.Role != Role.Host)
        {
            ErrorMessage = "Open the host Home page before using Host Sentence Scrambler.";
        }

        IsSessionChecked = true;
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        PlayerRegistry.PlayersChanged -= HandlePlayersChanged;
        await ValueTask.CompletedTask;
    }

    private void SelectSentenceFile(string fileName)
    {
        SelectedFileName = fileName;
        FileLoadError = null;
    }

    private void StartRound()
    {
        if (string.IsNullOrWhiteSpace(SelectedFileName))
        {
            return;
        }

        try
        {
            var sentences = SentenceFileService.LoadSentences(SelectedFileName);
            GameState.StartRound(sentences[0]);
            FileLoadError = null;
        }
        catch (InvalidOperationException exception)
        {
            FileLoadError = exception.Message;
        }
        catch (ArgumentException exception)
        {
            FileLoadError = exception.Message;
        }
    }

    private void LoadSentenceFiles()
    {
        try
        {
            SentenceFiles = SentenceFileService.GetSentenceFiles();
            FileLoadError = null;
        }
        catch (InvalidOperationException exception)
        {
            SentenceFiles = [];
            SelectedFileName = null;
            FileLoadError = exception.Message;
        }
    }

    private void RefreshPlayers()
    {
        Players = PlayerRegistry.GetPlayers();
    }

    private void HandlePlayersChanged()
    {
        _ = InvokeAsync(() =>
        {
            RefreshPlayers();
            StateHasChanged();
        });
    }
}
