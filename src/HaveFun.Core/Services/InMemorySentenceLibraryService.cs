namespace HaveFun.Core;

public sealed class InMemorySentenceLibraryService : ISentenceLibraryService
{
    public InMemorySentenceLibraryService(IReadOnlyList<TextDefinition> texts)
    {
        Texts = texts;
    }

    public IReadOnlyList<TextDefinition> Texts { get; }
}
