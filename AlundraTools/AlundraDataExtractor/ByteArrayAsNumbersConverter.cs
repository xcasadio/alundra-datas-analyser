using System.Text.Json;
using System.Text.Json.Serialization;

namespace AlundraDataExtractor;

public sealed class ByteArrayAsNumbersConverter : JsonConverter<byte[]>
{
    public override byte[] Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected byte array.");
        }

        var bytes = new List<byte>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return bytes.ToArray();
            }

            if (reader.TokenType != JsonTokenType.Number)
            {
                throw new JsonException("Expected byte value.");
            }

            bytes.Add(reader.GetByte());
        }

        throw new JsonException("Unexpected end of JSON.");
    }

    public override void Write(
        Utf8JsonWriter writer,
        byte[] value,
        JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (byte b in value)
        {
            writer.WriteNumberValue(b);
        }

        writer.WriteEndArray();
    }
}