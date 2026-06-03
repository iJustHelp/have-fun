namespace HaveFun.Core;

public sealed class WordScramblerFileService : FileService
{
    public WordScramblerFileService(string folderPath)
        : base(folderPath, "WordScramblerPath")
    {
    }
}
