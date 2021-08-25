using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TinyImagePS.Models;

namespace TinyImagePS
{
    public class TinifyApi
    {
        private readonly string _base64ApiKey;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _serializerOptions;

        public TinifyApi(string apiKey, HttpClient httpClient = null)
        {
            _base64ApiKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(apiKey));

            if (httpClient != null)
            {
                _httpClient = httpClient;
            }
            else
            {
                _httpClient = new HttpClient();
                _httpClient.BaseAddress = new Uri("https://api.tinify.com/");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _base64ApiKey);
            }

            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true
            };
        }

        public async Task<TinifyResponse> Shrink(Stream sourceFile)
        {
            var response = await _httpClient.PostAsync("shrink", new StreamContent(sourceFile));
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TinifyResponse>(_serializerOptions);
            }

            var error = await response.Content.ReadFromJsonAsync<TinifyError>();
            throw new TinifyException($"{error.Error}: {error.Message}");
        }

        public async Task<TinifyResponse> Shrink(Uri sourceUrl)
        {
            var response = await _httpClient.PostAsJsonAsync("shrink",
                new TinifyRequest { Source = new TinifyUrl { Url = sourceUrl.ToString() } });
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TinifyResponse>(_serializerOptions);
            }

            var error = await response.Content.ReadFromJsonAsync<TinifyError>();
            throw new TinifyException($"{error.Error}: {error.Message}");
        }

        public async Task<TinifyResponse> Shrink(string sourceFilePath)
        {
            using (var fs = File.OpenRead(sourceFilePath))
            {
                return await Shrink(fs);
            }
        }

        public async Task<Stream> GetStream(TinifyResponse tinifyResponse)
        {
            return await _httpClient.GetStreamAsync(tinifyResponse.Output.Url);
        }

        public async Task DownloadFile(TinifyResponse tinifyResponse, string targetPath)
        {
            using (var stream = await GetStream(tinifyResponse))
            {
                using (var fs = File.Open(targetPath, FileMode.Create, FileAccess.Write))
                {
                    await stream.CopyToAsync(fs);
                }
            }
        }

        public async Task<Stream> Resize(TinifyResponse response, ResizeMode resizeMode, int? width, int? height)
        {
            var options = new TinifyOptions
            {
                Resize = new TinifyOptionsResize
                {
                    Method = resizeMode.ToString().ToLowerInvariant(),
                    Width = width,
                    Height = height
                }
            };

            return await Resize(response, options);
        }

        public async Task<Stream> Resize(TinifyResponse response, TinifyOptions options)
        {
            return await Resize(response.Output.Url, options);
        }

        public async Task<Stream> Resize(string url, TinifyOptions options)
        {
            var resizeResponse = await _httpClient.PostAsJsonAsync(url, options, _serializerOptions);
            if (resizeResponse.IsSuccessStatusCode)
            {
                return await resizeResponse.Content.ReadAsStreamAsync();
            }

            var error = await resizeResponse.Content.ReadFromJsonAsync<TinifyError>();
            throw new TinifyException($"{error.Error}: {error.Message}");
        }

        public async Task Resize(TinifyResponse response, ResizeMode resizeMode, int? width, int? height,
            string targetPath)
        {
            var options = new TinifyOptions
            {
                Resize = new TinifyOptionsResize
                {
                    Method = resizeMode.ToString().ToLowerInvariant(),
                    Width = width,
                    Height = height
                }
            };

            using (var stream = await Resize(response, resizeMode, width, height))
            {
                using (var fs = File.Open(targetPath, FileMode.Create, FileAccess.Write))
                {
                    await stream.CopyToAsync(fs);
                }
            }
        }
    }

    public enum ResizeMode
    {
        Scale,
        Fit,
        Cover,
        Thumb
    }
}