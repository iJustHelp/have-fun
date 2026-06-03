using HaveFun.Core;
using Microsoft.AspNetCore.Components;

namespace HaveFun.Web;

public partial class WaitingRoom : ComponentBase, IAsyncDisposable
{
    private bool IsSessionChecked { get; set; }

    private string? DisplayName { get; set; }

    private string? PlayerName { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IPlayerRegistryService PlayerRegistry { get; set; } = default!;

    [Inject]
    private ISessionStorageService UserSessionStorageService { get; set; } = default!;

    [Inject]
    private SentenceScramblerGameStateService SentenceScramblerGameState { get; set; } = default!;

    [Inject]
    private WordScramblerGameStateService WordScramblerGameState { get; set; } = default!;

    [Inject]
    private FormulaScramblerGameStateService FormulaScramblerGameState { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        var currentUser = await UserSessionStorageService.GetCurrentUserAsync();

        if (currentUser?.Role != Role.Player)
        {
            await RedirectToRegisterAsync();
            return;
        }

        if (!PlayerRegistry.TryGetPlayerByName(currentUser.Name, out var registeredPlayer) || registeredPlayer is null)
        {
            await RedirectToRegisterAsync();
            return;
        }

        var activeGamePath = GetActiveGamePath();

        if (activeGamePath is not null)
        {
            NavigationManager.NavigateTo(activeGamePath, replace: true);
            return;
        }

        DisplayName = registeredPlayer.DisplayName;
        PlayerName = registeredPlayer.DisplayName;
        PlayerRegistry.PlayerRemoved += HandlePlayerRemoved;
        SentenceScramblerGameState.CurrentRoundChanged += HandleSentenceScramblerRoundChanged;
        WordScramblerGameState.CurrentRoundChanged += HandleWordScramblerRoundChanged;
        FormulaScramblerGameState.CurrentRoundChanged += HandleFormulaScramblerRoundChanged;
        IsSessionChecked = true;
        StateHasChanged();
    }

    public async ValueTask DisposeAsync()
    {
        PlayerRegistry.PlayerRemoved -= HandlePlayerRemoved;
        SentenceScramblerGameState.CurrentRoundChanged -= HandleSentenceScramblerRoundChanged;
        WordScramblerGameState.CurrentRoundChanged -= HandleWordScramblerRoundChanged;
        FormulaScramblerGameState.CurrentRoundChanged -= HandleFormulaScramblerRoundChanged;
        await ValueTask.CompletedTask;
    }

    private void HandlePlayerRemoved(PlayerSession player)
    {
        if (!string.Equals(PlayerName, player.DisplayName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _ = InvokeAsync(RedirectToRegisterAsync);
    }

    private void HandleSentenceScramblerRoundChanged(CurrentRound round)
    {
        HandleRoundChanged(round, "/player-sentence-scrambler");
    }

    private void HandleWordScramblerRoundChanged(CurrentRound round)
    {
        HandleRoundChanged(round, "/player-word-scrambler");
    }

    private void HandleFormulaScramblerRoundChanged(CurrentRound round)
    {
        HandleRoundChanged(round, "/player-formula-scrambler");
    }

    private void HandleRoundChanged(CurrentRound round, string path)
    {
        if (round.Status != RoundStatus.Started)
        {
            return;
        }

        _ = InvokeAsync(() => NavigationManager.NavigateTo(path, replace: true));
    }

    private async Task RedirectToRegisterAsync()
    {
        await UserSessionStorageService.ClearCurrentUserAsync();
        NavigationManager.NavigateTo("/register", replace: true);
    }

    private string? GetActiveGamePath()
    {
        var activeGames = new[]
        {
            new
            {
                Round = SentenceScramblerGameState.CurrentRound,
                Path = "/player-sentence-scrambler"
            },
            new
            {
                Round = WordScramblerGameState.CurrentRound,
                Path = "/player-word-scrambler"
            },
            new
            {
                Round = FormulaScramblerGameState.CurrentRound,
                Path = "/player-formula-scrambler"
            }
        };

        return activeGames
            .Where(game => game.Round?.Status == RoundStatus.Started)
            .OrderByDescending(game => game.Round!.StartedAt)
            .Select(game => game.Path)
            .FirstOrDefault();
    }

}
