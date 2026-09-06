using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Qbs.Infrastructure.Serialization;

public static class StudioJson
{
    public static JsonSerializerOptions Options { get; } = Create();

    public static JsonSerializerOptions Create()
    {
        var o = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        o.Converters.Add(new DecimalStringConverter());
        o.Converters.Add(new JsonStringEnumConverter());
        return o;
    }
}
