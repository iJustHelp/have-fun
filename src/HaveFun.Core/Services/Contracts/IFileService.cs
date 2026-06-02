namespace HaveFun.Core;

public interface IFileService
{
    IReadOnlyList<GameFilePathOption> GetGameFilePathes();

    IReadOnlyList<string> LoadLines(string filePath);
}
