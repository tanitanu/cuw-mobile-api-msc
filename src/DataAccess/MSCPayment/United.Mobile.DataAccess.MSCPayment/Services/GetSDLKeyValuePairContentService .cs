using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Mobile.DataAccess.MSCPayment.Interfaces;
using United.Utility.Helper;
using United.Utility.Http;
using United.Utility.Serilog;

namespace United.Mobile.DataAccess.MSCPayment.Services
{
    public class GetSDLKeyValuePairContentService : IGetSDLKeyValuePairContentService
    {

        private readonly ICacheLog<GetSDLKeyValuePairContentService> _logger;
        private readonly IResilientClient _resilientClient;

        public GetSDLKeyValuePairContentService(ICacheLog<GetSDLKeyValuePairContentService> logger, [KeyFilter("GetSDLKeyValuePairContentClientKey")] IResilientClient resilientClient)
        {
            _logger = logger;
            _resilientClient = resilientClient;
        }

        public async Task<string> GetSDLKeyValuePairContentByPageName(string path, string sessionId, string token)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                           {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            try
            {
                var responseData = await _resilientClient.GetHttpAsyncWithOptions(path, headers).ConfigureAwait(true);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("CSL service-GetSDLKeyValuePairContentByPageName {@RequestUrl} {url} error {response}", _resilientClient?.BaseURL, responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(responseData.response);
                }

                _logger.LogInformation("CSL service-GetSDLKeyValuePairContentByPageName {@RequestUrl}, {response}", responseData.url, responseData.response);

                return responseData.response;
            }
            catch (Exception ex)
            {
                _logger.LogError("CSL service-GetSDLKeyValuePairContentByPageName error {Exception}", JsonConvert.SerializeObject(ex));
            }

            return default;
        }
    }
}
