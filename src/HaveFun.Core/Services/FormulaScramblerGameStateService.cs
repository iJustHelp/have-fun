namespace HaveFun.Core;

// Separate DI type so Formula Scrambler has its own in-memory round state.
public sealed class FormulaScramblerGameStateService : GameStateService
{
    private readonly FormulaScramblerService _formulaScramblerService;

    public FormulaScramblerGameStateService(FormulaScramblerService formulaScramblerService)
    {
        _formulaScramblerService = formulaScramblerService;
    }

    protected override int CalculateRoundTotalScore(CurrentRound round)
    {
        return _formulaScramblerService.GetTotalScore(round.Text);
    }
}
