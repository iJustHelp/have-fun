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
            .EnumerateFiles(folderPath, "*.txt", SearchOption.TopDirectoryOnly)
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
            throw new InvalidOperationException($"SentenceScramblerPath folder contains no .txt files: {folderPath}");
        }

        return files;
    }

    public IReadOnlyList<string> LoadSentenceLines(string fileName)
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

        var filePath = Path.Combine(folderPath, sentenceFile.FileName);
        var sentences = File.ReadLines(filePath)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        if (sentences.Length == 0)
        {
            throw new InvalidOperationException($"Sentence file must contain at least one non-empty line: {fileName}");
        }

        return sentences;
    }
}
