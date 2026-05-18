using HaveFun.Core;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace HaveFun.Web;

public partial class Dashboard : ComponentBase, IAsyncDisposable
{
    private bool IsSessionChecked { get; set; }

    private string? ErrorMessage { get; set; }

    private string? LanUrl { get; set; }

    private string MasterName { get; set; } = "Master";

    private IReadOnlyList<PlayerSession> Players { get; set; } = [];

    private IReadOnlyList<PlayerRoundState> SubmittedPlayerRoundStates { get; set; } = [];

    private RoundResults? CurrentRoundResults { get; set; }

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
    private IUrlService UrlService { get; set; } = default!;

    [Inject]
    private IPlayerRegistryService PlayerRegistry { get; set; } = default!;

    [Inject]
    private ISentenceLibraryService SentenceLibrary { get; set; } = default!;

    [Inject]
    private IGameStateService GameState { get; set; } = default!;

    [Inject]
    private ISessionStorageService SessionStorageService { get; set; } = default!;

    protected override void OnInitialized()
    {
        var urls = UrlService.GetLanBaseUrl(NavigationManager.BaseUri);

        LanUrl = urls ?? NavigationManager.BaseUri;
        Sentences = SentenceLibrary.Sentences;
        RefreshPlayers();
        RefreshSubmissionProgress();
        CurrentRound = GameState.CurrentRound;
        GameState.CurrentRoundChanged += HandleCurrentRoundChanged;
        GameState.PlayerRoundStateChanged += HandlePlayerRoundStateChanged;
    }

    public async ValueTask DisposeAsync()
    {
        GameState.CurrentRoundChanged -= HandleCurrentRoundChanged;
        GameState.PlayerRoundStateChanged -= HandlePlayerRoundStateChanged;
        await ValueTask.CompletedTask;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        var currentUser = await SessionStorageService.GetCurrentUserAsync();

        if (currentUser?.Role != UserRole.Master)
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
        RefreshSubmissionProgress();
    }

    private void RefreshPlayers()
    {
        Players = PlayerRegistry.GetPlayers();
    }

    private void RefreshSubmissionProgress()
    {
        SubmittedPlayerRoundStates = GameState.GetSubmittedPlayerRoundStates();
        CurrentRoundResults = GameState.GetCurrentRoundResults();
    }

    private void HandleCurrentRoundChanged(CurrentRound round)
    {
        _ = InvokeAsync(() =>
        {
            CurrentRound = round;
            RefreshSubmissionProgress();
            StateHasChanged();
        });
    }

    private void HandlePlayerRoundStateChanged(PlayerRoundState playerRoundState)
    {
        if (!playerRoundState.IsSubmitted)
        {
            return;
        }

        _ = InvokeAsync(() =>
        {
            RefreshSubmissionProgress();
            StateHasChanged();
        });
    }

    private static string GetSentenceLabel(SentenceDefinition sentence)
    {
        const int maxLength = 64;

        return sentence.Text.Length <= maxLength
            ? sentence.Text
            : $"{sentence.Text[..maxLength]}...";
    }

    private static string FormatSpentTime(TimeSpan spentTime)
    {
        return $"{(int)spentTime.TotalMinutes:00}:{spentTime.Seconds:00}";
    }
}
