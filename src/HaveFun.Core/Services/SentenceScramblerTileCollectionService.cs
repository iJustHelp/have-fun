namespace HaveFun.Core;

public sealed class SentenceScramblerTileCollectionService : ITileCollectionService
{
    public IReadOnlyList<Tile> CreateAvailableTiles(CurrentRound round)
    {
        return round.ShuffledSentences
            .Select(sentence => new Tile
            {
                Id = Guid.NewGuid(),
                Text = sentence
            })
            .ToArray();
    }
}
