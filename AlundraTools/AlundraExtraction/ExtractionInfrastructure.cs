using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Text.Json;
using AlundraDataExtractor;

namespace AlundraExtraction;

/// <summary>Receives the files produced by an extraction. Paths use '/' and are relative to the extraction root.</summary>
public interface IExtractionSink
{
    void WriteBytes(string relativePath, byte[] bytes);
}

/// <summary>Writes the extraction files under a root folder on disk.</summary>
public sealed class DirectorySink : IExtractionSink
{
    private readonly string _root;

    public DirectorySink(string root)
    {
        _root = root;
    }

    public void WriteBytes(string relativePath, byte[] bytes)
    {
        var path = Path.Combine(_root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, bytes);
    }
}

/// <summary>The extractor's JSON options and serializer (UTF-8 without BOM, as File.WriteAllText wrote them).</summary>
public static class ExtractionJson
{
    public static JsonSerializerOptions Options { get; } = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions { WriteIndented = true, IncludeFields = true };
        options.Converters.Add(new ByteArrayAsNumbersConverter());
        return options;
    }

    public static byte[] Serialize<T>(T value) =>
        new UTF8Encoding(false).GetBytes(JsonSerializer.Serialize(value, Options));
}

public static class ExtractionImages
{
    public static byte[] EncodePng(Image image)
    {
        using var stream = new MemoryStream();
        image.Save(stream, ImageFormat.Png);
        return stream.ToArray();
    }
}
