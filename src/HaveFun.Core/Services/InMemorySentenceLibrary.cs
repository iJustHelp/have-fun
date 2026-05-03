namespace HaveFun.Core;

public sealed class InMemorySentenceLibrary : ISentenceLibrary
{
    public InMemorySentenceLibrary(IReadOnlyList<SentenceDefinition> sentences)
    {
        Sentences = sentences;
    }

    public IReadOnlyList<SentenceDefinition> Sentences { get; }
}
