using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Utility.Http;
using Autofac.Features.AttributeFilters;
using Newtonsoft.Json;
using United.Utility.Serilog;
using United.Mobile.DataAccess.MSCPayment.Interfaces;
using United.Utility.Helper;

namespace United.Mobile.DataAccess.MSCPayment.Services
{
    public class ReferencedataService : IReferencedataService
    {
        private readonly ICacheLog<ReferencedataService> _logger;
        private readonly IResilientClient _resilientClient;
        public ReferencedataService(ICacheLog<ReferencedataService> logger, [KeyFilter("ReferencedataClientKey")] IResilientClient resilientClient)
        {
            _logger = logger;
            _resilientClient = resilientClient;
        }

        public async Task<T> GetDataPostHttpAsyncWithOptions<T>(string path, string token, string sessionId, string request)
        {

            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            _logger.LogInformation("CSL service-GetDataPostHttpAsyncWithOptions  parameters Request:{Request} Path:{Path}", request, path);

            var response = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers);
            if (response.statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("CSL service-Referencedata PostHttpAsyncWithOptions {@RequestUrl} error {response}", response.url, response.response);
                if (response.statusCode != HttpStatusCode.BadRequest)
                    return default;
            }

            _logger.LogInformation("CSL service-Referencedata PostHttpAsyncWithOptions {@RequestUrl}, {response}", response.url, response.response);
            return (response.response == null) ? default : JsonConvert.DeserializeObject<T>(response.response);
        }

        public async Task<T> GetDataGetAsync<T>(string actionName, string token, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            _logger.LogInformation("CSL service Referencedata parameters ActionName:{actionName}", actionName);

            try
            {
                var response = await _resilientClient.GetHttpAsyncWithOptions(actionName, headers);
                if (response.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("CSL service-Referencedata GetAsync {@RequestUrl} {url} error {response}", _resilientClient?.BaseURL, response.url, response.response);
                    if (response.statusCode != HttpStatusCode.BadRequest)
                        return default;
                }

                _logger.LogInformation("CSL service-Referencedata Referencedata {@RequestUrl}, {response}", response.url, response.response);
                return (response.response == null) ? default : JsonConvert.DeserializeObject<T>(response.response);
            }
            catch (Exception ex)
            {
                _logger.LogError("CSL service-Referencedata error {Exception}", JsonConvert.SerializeObject(ex));
            }

            return default;
        }
    }
}
