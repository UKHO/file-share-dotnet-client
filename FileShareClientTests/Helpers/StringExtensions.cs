using Newtonsoft.Json;

namespace UKHO.FileShareClientTests.Helpers
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Reads response body json as given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpResponseMessage"></param>
        /// <returns></returns>
        public static T DeserialiseJson<T>(this string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }
    }
}