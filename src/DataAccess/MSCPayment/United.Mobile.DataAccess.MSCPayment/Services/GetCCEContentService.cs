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
    public class GetCCEContentService : IGetCCEContentService
    {
        private readonly ICacheLog<GetCCEContentService> _logger;
        private readonly IResilientClient _resilientClient;

        public GetCCEContentService(ICacheLog<GetCCEContentService> logger, [KeyFilter("GetCCEContentClientKey")] IResilientClient resilientClient)
        {
            _logger = logger;
            _resilientClient = resilientClient;
        }

        public async Task<string> GetCCEContent(string token, string action, string request, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };
            string path = string.Format("/{0}", action);

            _logger.LogInformation("CSL service- GetCCEContent parameters Request:{request} ", request);

            try
            {
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers);
                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("CSL service-GetCCEContent {@RequestUrl} error {response}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        return default;
                }

                _logger.LogInformation("CSL service-GetCCEContent {@RequestUrl}, {response}", responseData.url, responseData.response);
                return responseData.response;
            }
            catch (Exception ex)
            {
                _logger.LogError("CSL service-GetCCEContent error {Exception}", JsonConvert.SerializeObject(ex));
            }

            return default;
        }
    }
}
