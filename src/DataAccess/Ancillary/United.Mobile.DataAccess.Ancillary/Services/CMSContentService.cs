using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Utility.Helper;
using United.Utility.Http;
using United.Utility.Serilog;

namespace United.Mobile.DataAccess.Product.Services
{
    public class CMSContentService : ICMSContentService
    {
        private readonly ICacheLog<CMSContentService> _logger;
        private readonly IResilientClient _resilientClient;

        public CMSContentService(ICacheLog<CMSContentService> logger, [KeyFilter("CMSContentClientKey")] IResilientClient resilientClient)
        {
            _logger = logger;
            _resilientClient = resilientClient;
        }

        public async Task<T> GetSDLContentByGroupName<T>(string token, string action, string request, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };
            string path = string.Format("/{0}", action);

            _logger.LogInformation("CSL Service-GetSDLContentByGroupName Request:{request}", JsonConvert.SerializeObject(request));
            var requestUrl = "";
            try
            {
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers);
                requestUrl = responseData.url;
                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("CSL Service-GetSDLContentByGroupName {@RequestUrl} error {response}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        return default;
                }

                if (string.IsNullOrEmpty(responseData.response))
                {
                    _logger.LogWarning("CSL Service-GetSDLContentByGroupName CSL-CallError-{CSL-CallError}, {@RequestUrl}", "CSL response is empty or null", requestUrl);
                }
                else
                {
                    _logger.LogInformation("CSL Service-GetSDLContentByGroupName {@RequestUrl}, {response}", responseData.url, responseData);
                }
                return (responseData.response == null) ? default : JsonConvert.DeserializeObject<T>(responseData.response);
            }
            catch (Exception ex)
            {
                _logger.LogError("CSL Service-GetSDLContentByGroupName error {@RequestUrl}, {Exception}", requestUrl, ex);
            }

            return default;
        }
        public async Task<string> GetETCByEmail(string path, string request, string sessionId, string token)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                           {"Accept", "application/json"},
                          { "Authorization", token }
                     };
            _logger.LogInformation("CSL Service-GetETCByEmail Request:{request}", request);
            var requestUrl = "";
            try
            {
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers).ConfigureAwait(true);
                requestUrl = responseData.url;
                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("CSL Service-GetETCByEmail {@RequestUrl} error {response}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(responseData.response);
                }

                _logger.LogInformation("CSL Service-GetETCByEmail {@RequestUrl} {response}", responseData.url, responseData.response);

                return responseData.response;
            }
            catch (Exception ex)
            {
                _logger.LogError("CSL Service-GetETCByEmail {@RequestUrl}, error {Exception}", requestUrl, ex);
            }

            return default;
        }

    }
}
