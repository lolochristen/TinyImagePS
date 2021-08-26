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

        public TinifyResponse Shrink(Stream stream)
        {
            var task = ShrinkAsync(stream);
            task.Wait();
            return task.Result;
        }

        public TinifyResponse Shrink(string filePath)
        {
            var task = ShrinkAsync(filePath);
            task.Wait();
            return task.Result;
        }

        public async Task<TinifyResponse> ShrinkAsync(Stream stream)
        {
            var response = await _httpClient.PostAsync("shrink", new StreamContent(stream));
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TinifyResponse>(_serializerOptions);
            }

            var error = await response.Content.ReadFromJsonAsync<TinifyError>();
            throw new TinifyException($"{error.Error}: {error.Message}");
        }

        public async Task<TinifyResponse> ShrinkAsync(Uri sourceUrl)
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

        public async Task<TinifyResponse> ShrinkAsync(string filePath)
        {
            using (var fs = File.OpenRead(filePath))
            {
                return await ShrinkAsync(fs);
            }
        }

        public Stream GetStream(TinifyResponseOutput output)
        {
            var task = GetStreamAsync(output);
            task.Wait();
            return task.Result;
        }

        public async Task<Stream> GetStreamAsync(TinifyResponseOutput output)
        {
            return await _httpClient.GetStreamAsync(output.Url);
        }

        public void DownloadFile(TinifyResponseOutput output, string targetPath)
        {
            var task = DownloadFileAsync(output, targetPath);
            task.Wait();
        }

        public async Task DownloadFileAsync(TinifyResponseOutput output, string targetPath)
        {
            using (var stream = await GetStreamAsync(output))
            {
                using (var fs = File.Open(targetPath, FileMode.Create, FileAccess.Write))
                {
                    await stream.CopyToAsync(fs);
                }
            }
        }

        public Stream Resize(TinifyResponseOutput output, ResizeMode resizeMode, int? width, int? height)
        {
            var task = ResizeAsync(output, resizeMode, width, height);
            task.Wait();
            return task.Result;
        }

        public async Task<Stream> ResizeAsync(TinifyResponseOutput output, ResizeMode resizeMode, int? width, int? height)
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

            return await ResizeAsync(output, options);
        }


        public async Task<Stream> ResizeAsync(TinifyResponseOutput output, TinifyOptions options)
        {
            return await ResizeAsync(output.Url, options);
        }

        public async Task<Stream> ResizeAsync(string url, TinifyOptions options)
        {
            var resizeResponse = await _httpClient.PostAsJsonAsync(url, options, _serializerOptions);
            if (resizeResponse.IsSuccessStatusCode)
            {
                return await resizeResponse.Content.ReadAsStreamAsync();
            }

            var error = await resizeResponse.Content.ReadFromJsonAsync<TinifyError>();
            throw new TinifyException($"{error.Error}: {error.Message}");
        }

        public void Resize(TinifyResponseOutput output, ResizeMode resizeMode, int? width, int? height, string targetPath)
        {
            var task = ResizeAsync(output, resizeMode, width, height, targetPath);
            task.Wait();
        }

        public async Task ResizeAsync(TinifyResponseOutput output, ResizeMode resizeMode, int? width, int? height,
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

            using (var stream = await ResizeAsync(output, resizeMode, width, height))
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