using System.Text.Json;

namespace HaveFun.Core;

public static class SentenceFileLoaderService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static IReadOnlyList<SentenceDefinition> Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new InvalidOperationException($"Sentence file was not found: {filePath}");
        }

        List<SentenceDefinition>? sentences;

        try
        {
            using var stream = File.OpenRead(filePath);
            sentences = JsonSerializer.Deserialize<List<SentenceDefinition>>(stream, SerializerOptions);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException($"Sentence file is not valid JSON: {filePath}", exception);
        }

        Validate(sentences, filePath);

        return sentences!.AsReadOnly();
    }

    private static void Validate(List<SentenceDefinition>? sentences, string filePath)
    {
        if (sentences is null || sentences.Count == 0)
        {
            throw new InvalidOperationException($"Sentence file must contain at least one sentence: {filePath}");
        }

        for (var index = 0; index < sentences.Count; index++)
        {
            var sentence = sentences[index];

            if (string.IsNullOrWhiteSpace(sentence.Text))
            {
                throw new InvalidOperationException($"Sentence at index {index} must have non-empty text.");
            }

            if (sentence.TimeLimitInSeconds <= 0)
            {
                throw new InvalidOperationException($"Sentence at index {index} must have a positive timeLimitInSeconds value.");
            }
        }
    }
}
