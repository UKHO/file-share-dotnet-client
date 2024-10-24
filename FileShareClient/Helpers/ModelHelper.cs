using System.Text.Json;
using System.Text.Json.Serialization;

namespace UKHO.FileShareClient.Helpers
{
    public static class ModelHelper
    {
        public static JsonSerializerOptions JsonSerializerOptions { get; }

        static ModelHelper()
        {
            JsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
            };
        }

        public static string ToJson<T>(T obj) => JsonSerializer.Serialize(obj, JsonSerializerOptions);
    }
}
