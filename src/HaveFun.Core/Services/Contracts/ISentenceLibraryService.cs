namespace HaveFun.Core;

public interface ISentenceLibraryService
{
    IReadOnlyList<SentenceDefinition> Sentences { get; }
}
