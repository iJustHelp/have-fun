using System.Text.Json;

namespace HaveFun.Core;

public static class SentenceFileLoaderService
{
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static IReadOnlyList<TextDefinition> Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new InvalidOperationException($"Sentence file was not found: {filePath}");
        }

        List<TextDefinition>? sentences;

        try
        {
            using var stream = File.OpenRead(filePath);
            sentences = JsonSerializer.Deserialize<List<TextDefinition>>(stream, _serializerOptions);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException($"Sentence file is not valid JSON: {filePath}", exception);
        }

        Validate(sentences, filePath);

        return sentences!.AsReadOnly();
    }

    private static void Validate(List<TextDefinition>? sentences, string filePath)
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
