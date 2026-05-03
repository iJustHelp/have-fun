namespace HaveFun.Core.Sentences;

public interface ISentenceLibrary
{
    IReadOnlyList<SentenceDefinition> Sentences { get; }
}
