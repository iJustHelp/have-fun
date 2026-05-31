using HaveFun.Core;
using Microsoft.AspNetCore.Components;

namespace HaveFun.Web;

public partial class PlayerGameBoard
{
    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string Rules { get; set; } = string.Empty;

    [Parameter]
    public IReadOnlyList<Tile> AvailableTiles { get; set; } = [];

    [Parameter]
    public IReadOnlyList<Tile> SelectedTiles { get; set; } = [];

    [Parameter]
    public bool IsSubmitted { get; set; }

    [Parameter]
    public string? SubmittedText { get; set; }

    [Parameter]
    public TimeSpan? SpentTime { get; set; }

    [Parameter]
    public string RemainingTimeText { get; set; } = string.Empty;

    [Parameter]
    public bool CanSubmit { get; set; }

    [Parameter]
    public Action<Guid>? OnSelectItem { get; set; }

    [Parameter]
    public Action<Guid>? OnReturnItem { get; set; }

    [Parameter]
    public Func<Task>? OnSubmit { get; set; }

    private void SelectItem(Guid tileId)
    {
        OnSelectItem?.Invoke(tileId);
    }

    private void ReturnItem(Guid tileId)
    {
        OnReturnItem?.Invoke(tileId);
    }

    private Task SubmitAsync()
    {
        return OnSubmit?.Invoke() ?? Task.CompletedTask;
    }

    private static string FormatSpentTime(TimeSpan spentTime)
    {
        return $"{(int)spentTime.TotalMinutes:00}:{spentTime.Seconds:00}";
    }
}
