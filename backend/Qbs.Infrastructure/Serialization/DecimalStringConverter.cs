using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Qbs.Infrastructure;

public sealed class DecimalStringConverter : JsonConverter<decimal>
{
    public override decimal Read(
        ref Utf8JsonReader reader,
        Type type,
        JsonSerializerOptions options
    ) =>
        reader.TokenType == JsonTokenType.String
            ? decimal.Parse(reader.GetString()!, CultureInfo.InvariantCulture)
            : reader.GetDecimal();

    public override void Write(
        Utf8JsonWriter writer,
        decimal value,
        JsonSerializerOptions options
    ) =>
        writer.WriteStringValue(
            value.ToString("0.00##########################", CultureInfo.InvariantCulture)
        );
}
