namespace HaveFun.Core;

public class FileService : IFileService
{
    private readonly string _folderPath;
    private readonly string _configurationKey;

    public FileService(string folderPath)
        : this(folderPath, "FilePath")
    {
    }

    public FileService(string folderPath, string configurationKey)
    {
        _folderPath = folderPath;
        _configurationKey = configurationKey;
    }

    public IReadOnlyList<GameFilePathOption> GetGameFilePathes()
    {
        if (string.IsNullOrWhiteSpace(_folderPath))
        {
            throw new InvalidOperationException($"{_configurationKey} is required.");
        }

        if (!Directory.Exists(_folderPath))
        {
            throw new InvalidOperationException($"{_configurationKey} folder was not found: {_folderPath}");
        }

        var files = Directory
            .EnumerateFiles(_folderPath, "*.txt", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .Where(fileName => !string.IsNullOrWhiteSpace(fileName))
            .OrderBy(fileName => fileName, StringComparer.OrdinalIgnoreCase)
            .Select(fileName => new GameFilePathOption
            {
                FilePath = fileName!
            })
            .ToArray();

        if (files.Length == 0)
        {
            throw new InvalidOperationException($"{_configurationKey} folder contains no .txt files: {_folderPath}");
        }

        return files;
    }

    public IReadOnlyList<string> LoadLines(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("Sentence file name is required.", nameof(fileName));
        }

        var sentenceFile = GetGameFilePathes()
            .FirstOrDefault(file => string.Equals(file.FilePath, fileName, StringComparison.OrdinalIgnoreCase));

        if (sentenceFile is null)
        {
            throw new InvalidOperationException($"File was not found in {_configurationKey}: {fileName}");
        }

        var filePath = Path.Combine(_folderPath, sentenceFile.FilePath);
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
