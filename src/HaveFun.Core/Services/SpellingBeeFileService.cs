namespace HaveFun.Core;

public sealed class SpellingBeeFileService : FileService
{
    public SpellingBeeFileService(string folderPath)
        : base(folderPath, "SpellingBeePath")
    {
    }
}
