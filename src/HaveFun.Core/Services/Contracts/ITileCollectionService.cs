namespace HaveFun.Core;

public interface ITileCollectionService
{
    IReadOnlyList<Tile> CreateAvailableTiles(CurrentRound round);
}
