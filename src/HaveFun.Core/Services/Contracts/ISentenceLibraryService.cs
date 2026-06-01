namespace HaveFun.Core;

public interface ISentenceLibraryService
{
    IReadOnlyList<TextDefinition> Texts { get; }
}
