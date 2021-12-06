﻿using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UKHO.FileShareClient.Internal
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
            var bodyJson = await httpResponseMessage.Content?.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(bodyJson);
        }

        /// <summary>
        /// Reads response body json as given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpResponseMessage"></param>
        /// <returns></returns>
        public static async Task<T> ReadAsTypeAsync1<T>(this HttpResponseMessage httpResponseMessage, string batchId)
        {
            string bodyJson = string.Empty;

            typeof(T).GetProperty("BatchId").SetValue("BatchId", batchId);
            typeof(T).GetProperty("IsSuccess").SetValue("IsSuccess", httpResponseMessage.IsSuccessStatusCode);


            if (httpResponseMessage.Content != null)
            {
                bodyJson = await httpResponseMessage.Content.ReadAsStringAsync();
            }

            return JsonConvert.DeserializeObject<T>(bodyJson);

        }

        /// <summary>
        /// Sets Authorization header
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="authTokenProvider"></param>
        /// <returns></returns>
        public static async Task<HttpClient> SetAuthenticationHeader(this HttpClient httpClient, IAuthTokenProvider authTokenProvider)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", await authTokenProvider.GetToken());
            return httpClient;
        }
    }

    internal static class Md5HashHelper
    {
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