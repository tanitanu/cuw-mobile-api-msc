using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace United.Utility.HttpService
{
    public class ShoppingClientService : IShoppingClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ShoppingOptions _shoppingOptions;
        private readonly ILogger<ShoppingClientService> _logger;
        private readonly IAsyncPolicy _policyWrap;

        public ShoppingClientService(HttpClient httpClient, IOptions<ShoppingOptions> shoppingOptions, ILogger<ShoppingClientService> logger)
        {
            _httpClient = httpClient;
            _shoppingOptions = shoppingOptions.Value;
            _logger = logger;

            _policyWrap = Policy.WrapAsync(
                Policy.Handle<Exception>()
                    .CircuitBreakerAsync(_shoppingOptions.CircuitBreakerAllowExceptions, TimeSpan.FromSeconds(_shoppingOptions.CircuitBreakerBreakDuration)),
                Policy.Handle<Exception>()
                    .WaitAndRetryAsync(retryCount: _shoppingOptions.RetryCount,
                        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))),
                Policy.TimeoutAsync((int)_shoppingOptions.TimeOut));
        }

        public async Task<T> PostHttpAsyncWithOptions<T>(string token, string sessionId, string action, object request, string contentType = "application/json")
        {
            var headers = CreateHeaders(token);
            using var streamToHoldRequest = new MemoryStream();
            Serialize(request, streamToHoldRequest);
            using var postRequest = CreatePostRequest(headers, action);
            using var content = CreateHttpContent(streamToHoldRequest, contentType);
            postRequest.Content = content;
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(_shoppingOptions.TimeOut));
            T response;

            try
            {
                using var httpResponseMessage = await CallService(postRequest, cts).ConfigureAwait(false);
                if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
                    _logger.LogError("CSL service-GetShop request response ", httpResponseMessage.Content);
                using var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
                response = Deserialize<T>(responseStream);
            }
            catch (Exception ex)
            {
                _logger.LogError("CSL service-GetShop request is failed ", ex.Message);
                throw ex;
            }
            //TO-DO::Check for valid Response
            //if (HasValidResponse(httpResponseMessage))
            //{
            //}
            //var errorContent = ReadErrorResponse(await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false));
            // throw new APIException(errorContent);

            return response;

        }

        private static Dictionary<string, string> CreateHeaders(string token)
        {
            return new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };
        }

        private async Task<HttpResponseMessage> CallService(HttpRequestMessage postRequest, CancellationTokenSource cts)
        {
            return await _policyWrap.ExecuteAsync(async () =>
            {
                var response = await _httpClient.SendAsync(postRequest, HttpCompletionOption.ResponseHeadersRead, cts.Token);
                response.EnsureSuccessStatusCode();
                return response;
            });


        }


        private static HttpContent CreateHttpContent(Stream serializedRequest, string contentType)
        {
            serializedRequest.Seek(0, SeekOrigin.Begin);
            var httpContent = new StreamContent(serializedRequest);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            return httpContent;
        }

        private T Deserialize<T>(Stream stream)
        {
            if (stream == null || stream.CanRead == false)
            {
                return default;
            }
            using var sr = new StreamReader(stream);
            using var jr = new JsonTextReader(sr);
            //jr.ArrayPool = JsonArrayPool.Instance;
            var ntSerializer = new JsonSerializer();
            var response = ntSerializer.Deserialize<T>(jr);
            return response;

        }
        private Stream Serialize<T>(T request, Stream stream) where T : class
        {
            using var sw = new StreamWriter(stream, new UTF8Encoding(false), 1024, true);
            using var jtw = new JsonTextWriter(sw) { Formatting = Newtonsoft.Json.Formatting.None };
            //jtw.ArrayPool = JsonArrayPool.Instance;
            var js = new JsonSerializer();
            js.Serialize(jtw, request);
            jtw.Flush();
            return stream;
        }


        private HttpRequestMessage CreatePostRequest(IDictionary<string, string> headers, string action)
        {
            var url = _shoppingOptions.Url + action;
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            if (headers != null)
            {
                foreach (string key in headers.Keys)
                    requestMessage.Headers.Add(key, headers[key]);
            }
            return requestMessage;
        }
    }
}
