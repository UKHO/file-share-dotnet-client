using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FileShareClient
{
    internal static class Extensions
    {
        /// <summary>
        /// Reads response body json as given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpResponseMessage"></param>
        /// <returns></returns>
        public static async Task<T> ReadAsTypeAsync<T>(this HttpResponseMessage httpResponseMessage)
        {
            string bodyJson = await httpResponseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(bodyJson);
        }
    }

    public static class Md5HashHelper
    {
        public static byte[] CalculateMD5(this byte[] requestBytes)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(requestBytes);
                return hash;
            }
        }

        public static byte[] CalculateMD5(this Stream stream)
        {
            var position = stream.Position;
            stream.Seek(0, SeekOrigin.Begin);
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(stream);
                stream.Seek(position, SeekOrigin.Begin);

                return hash;
            }
        }
    }
}