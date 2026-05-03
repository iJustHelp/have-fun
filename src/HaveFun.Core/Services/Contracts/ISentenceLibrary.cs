namespace HaveFun.Core;

public interface ISentenceLibrary
{
    IReadOnlyList<SentenceDefinition> Sentences { get; }
}
