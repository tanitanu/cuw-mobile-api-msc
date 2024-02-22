using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace United.Utility.Http
{
    public interface IResilientClient
    {
        string BaseURL { get; }

        Task<(string response, HttpStatusCode statusCode, string url)> GetHttpAsyncWithOptions(string endPoint, IDictionary<string, string> requestHeaders = null, bool throwWebException = false);

        Task<(string response, HttpStatusCode statusCode, string url)> PutAsync(string endPoint, string requestData, IDictionary<string, string> requestHeaders = null, string contentType = "application/json", bool throwWebException = false);

        Task<(string response, HttpStatusCode statusCode, string url)> DeleteAsync(string endPoint, IDictionary<string, string> requestHeaders = null, bool throwWebException = false);

        Task<(string response, HttpStatusCode statusCode, string url)> PostHttpAsyncWithOptions(string endPoint, string requestData, IDictionary<string, string> requestHeaders = null, string contentType = "application/json", bool throwWebException = false);
    }
}
