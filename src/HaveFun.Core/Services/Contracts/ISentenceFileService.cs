namespace HaveFun.Core;

public interface ISentenceFileService
{
    IReadOnlyList<SentenceFileOption> GetSentenceFiles();

    IReadOnlyList<SentenceDefinition> LoadSentences(string fileName);
}
