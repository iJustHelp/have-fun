namespace HaveFun.Core;

public sealed class SentenceScramblerFileService : FileService
{
    public SentenceScramblerFileService(string folderPath)
        : base(folderPath, "SentenceScramblerPath")
    {
    }
}
