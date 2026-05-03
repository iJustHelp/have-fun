namespace HaveFun.Core;

public sealed class InMemorySentenceLibraryService : ISentenceLibraryService
{
    public InMemorySentenceLibraryService(IReadOnlyList<SentenceDefinition> sentences)
    {
        Sentences = sentences;
    }

    public IReadOnlyList<SentenceDefinition> Sentences { get; }
}
