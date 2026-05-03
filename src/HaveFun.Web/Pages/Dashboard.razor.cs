using HaveFun.Core;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace HaveFun.Web;

public partial class Dashboard : ComponentBase
{
    private bool IsSessionChecked { get; set; }

    private string? ErrorMessage { get; set; }

    private string? LanUrl { get; set; }

    private string MasterName { get; set; } = "Master";

    private IReadOnlyList<PlayerSession> Players { get; set; } = [];

    private IReadOnlyList<SentenceDefinition> Sentences { get; set; } = [];

    private int SelectedSentenceIndex { get; set; } = -1;

    private SentenceDefinition? SelectedSentence => SelectedSentenceIndex >= 0 && SelectedSentenceIndex < Sentences.Count
        ? Sentences[SelectedSentenceIndex]
        : null;

    private CurrentRound? CurrentRound { get; set; }

    private string RoundStatusText => CurrentRound?.Status.ToString() ?? RoundStatus.NotStarted.ToString();

    private Color RoundStatusColor => CurrentRound?.Status == RoundStatus.Started ? Color.Success : Color.Default;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IJoinUrlProviderService JoinUrlProvider { get; set; } = default!;

    [Inject]
    private IPlayerRegistryService PlayerRegistry { get; set; } = default!;

    [Inject]
    private ISentenceLibraryService SentenceLibrary { get; set; } = default!;

    [Inject]
    private IGameStateService GameState { get; set; } = default!;

    [Inject]
    private IUserSessionStorageService UserSessionStorage { get; set; } = default!;

    protected override void OnInitialized()
    {
        var urls = JoinUrlProvider.GetJoinUrls(new Uri(NavigationManager.BaseUri));

        LanUrl = urls.LanUrl ?? urls.LocalhostUrl;
        Sentences = SentenceLibrary.Sentences;
        RefreshPlayers();
        CurrentRound = GameState.CurrentRound;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        var currentUser = await UserSessionStorage.GetCurrentUserAsync();

        if (currentUser?.Role != JoinRole.Master)
        {
            ErrorMessage = "Join as the configured master to open the dashboard.";
        }
        else
        {
            MasterName = currentUser.Name;
        }

        IsSessionChecked = true;
        StateHasChanged();
    }

    private void SelectSentence(int sentenceIndex)
    {
        SelectedSentenceIndex = sentenceIndex;
    }

    private void StartRound()
    {
        if (SelectedSentence is null)
        {
            return;
        }

        CurrentRound = GameState.StartRound(SelectedSentence);
    }

    private void RefreshPlayers()
    {
        Players = PlayerRegistry.GetPlayers();
    }

    private static string GetSentenceLabel(SentenceDefinition sentence)
    {
        const int maxLength = 64;

        return sentence.Text.Length <= maxLength
            ? sentence.Text
            : $"{sentence.Text[..maxLength]}...";
    }
}
