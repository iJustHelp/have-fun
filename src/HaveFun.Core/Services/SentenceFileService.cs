namespace HaveFun.Core;

public sealed class SentenceFileService : ISentenceFileService
{
    private readonly string folderPath;

    public SentenceFileService(string folderPath)
    {
        this.folderPath = folderPath;
    }

    public IReadOnlyList<SentenceFileOption> GetSentenceFiles()
    {
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            throw new InvalidOperationException("SentenceScramblerPath is required.");
        }

        if (!Directory.Exists(folderPath))
        {
            throw new InvalidOperationException($"SentenceScramblerPath folder was not found: {folderPath}");
        }

        var files = Directory
            .EnumerateFiles(folderPath, "*.json", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .Where(fileName => !string.IsNullOrWhiteSpace(fileName))
            .OrderBy(fileName => fileName, StringComparer.OrdinalIgnoreCase)
            .Select(fileName => new SentenceFileOption
            {
                FileName = fileName!
            })
            .ToArray();

        if (files.Length == 0)
        {
            throw new InvalidOperationException($"SentenceScramblerPath folder contains no .json files: {folderPath}");
        }

        return files;
    }

    public IReadOnlyList<SentenceDefinition> LoadSentences(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("Sentence file name is required.", nameof(fileName));
        }

        var sentenceFile = GetSentenceFiles()
            .FirstOrDefault(file => string.Equals(file.FileName, fileName, StringComparison.OrdinalIgnoreCase));

        if (sentenceFile is null)
        {
            throw new InvalidOperationException($"Sentence file was not found in SentenceScramblerPath: {fileName}");
        }

        return SentenceFileLoaderService.Load(Path.Combine(folderPath, sentenceFile.FileName));
    }
}
