using System.Text.Json;
using System.Text.Json.Serialization;

namespace API;

public static class ApiJsonSerializerOptions
{
    public static readonly JsonSerializerOptions Web = Create();

    private static JsonSerializerOptions Create()
    {
        var o = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        o.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
        return o;
    }
}
