namespace HaveFun.Core;

public interface ISentenceFileService
{
    IReadOnlyList<SentenceFileOption> GetSentenceFiles();

    IReadOnlyList<string> LoadSentenceLines(string fileName);
}
