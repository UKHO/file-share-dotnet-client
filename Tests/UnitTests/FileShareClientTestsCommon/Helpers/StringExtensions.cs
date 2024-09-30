using System.Text.Json;

namespace FileShareClientTestsCommon.Helpers
{
    public static class StringExtensions
    {
        /// <summary>
        /// Deserialises a json string to the given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static T? DeserialiseJson<T>(this string jsonString) => JsonSerializer.Deserialize<T>(jsonString);
    }
}
