using HaveFun.Core;
using Microsoft.AspNetCore.Components;

namespace HaveFun.Web;

public partial class PlayerGameBoard
{
    private Guid? _roundId;
    private IReadOnlyList<Guid> _sourceAvailableTileIds = [];
    private IReadOnlyList<Tile> _availableTiles = [];
    private IReadOnlyList<Tile> _selectedTiles = [];

    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string Rules { get; set; } = string.Empty;

    [Parameter]
    public Guid? RoundId { get; set; }

    [Parameter]
    public IReadOnlyList<Tile> AvailableTiles { get; set; } = [];

    [Parameter]
    public IReadOnlyList<Tile> SelectedTiles { get; set; } = [];

    [Parameter]
    public bool IsSubmitted { get; set; }

    [Parameter]
    public string TileTextSeparator { get; set; } = " ";

    [Parameter]
    public TimeSpan? SpentTime { get; set; }

    [Parameter]
    public string RemainingTimeText { get; set; } = string.Empty;

    [Parameter]
    public bool CanSubmit { get; set; }

    [Parameter]
    public Func<IReadOnlyList<Tile>, Task>? OnSubmit { get; set; }

    private bool CanSubmitDraft => CanSubmit && !IsSubmitted && _selectedTiles.Count > 0;

    private string SubmittedText => JoinTileText(_selectedTiles);

    protected override void OnParametersSet()
    {
        var availableTileIds = AvailableTiles.Select(tile => tile.Id).ToArray();
        var shouldResetDraft = RoundId != _roundId || !_sourceAvailableTileIds.SequenceEqual(availableTileIds);

        if (IsSubmitted)
        {
            _availableTiles = AvailableTiles.ToArray();
            _selectedTiles = SelectedTiles.ToArray();
            _roundId = RoundId;
            _sourceAvailableTileIds = availableTileIds;
            return;
        }

        if (!shouldResetDraft)
        {
            return;
        }

        _availableTiles = AvailableTiles.ToArray();
        _selectedTiles = [];
        _roundId = RoundId;
        _sourceAvailableTileIds = availableTileIds;
    }

    private void SelectTile(Guid tileId)
    {
        if (IsSubmitted)
        {
            return;
        }

        var selectedTile = _availableTiles.FirstOrDefault(tile => tile.Id == tileId);

        if (selectedTile is null)
        {
            return;
        }

        _availableTiles = _availableTiles
            .Where(tile => tile.Id != tileId)
            .ToArray();
        _selectedTiles = _selectedTiles
            .Append(selectedTile)
            .ToArray();
    }

    private void ReturnTile(Guid tileId)
    {
        if (IsSubmitted)
        {
            return;
        }

        var returnedTile = _selectedTiles.FirstOrDefault(tile => tile.Id == tileId);

        if (returnedTile is null)
        {
            return;
        }

        _availableTiles = _availableTiles
            .Append(returnedTile)
            .ToArray();
        _selectedTiles = _selectedTiles
            .Where(tile => tile.Id != tileId)
            .ToArray();
    }

    private Task SubmitAsync()
    {
        return OnSubmit?.Invoke(_selectedTiles) ?? Task.CompletedTask;
    }

    private static string FormatSpentTime(TimeSpan spentTime)
    {
        return $"{(int)spentTime.TotalMinutes:00}:{spentTime.Seconds:00}";
    }

    private string JoinTileText(IReadOnlyList<Tile> tiles)
    {
        return string.Join(TileTextSeparator, tiles.Select(tile => tile.Text));
    }
}
