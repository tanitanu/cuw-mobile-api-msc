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
    public class MSCCheckoutService: IMSCCheckoutService
    {
        private readonly ICacheLog<MSCCheckoutService> _logger;
        private readonly IResilientClient _resilientClient;

        public MSCCheckoutService(ICacheLog<MSCCheckoutService> logger
            , [KeyFilter("CustomerDataClientKeyNew")] IResilientClient resilientClient)
        {
            _logger = logger;
            _resilientClient = resilientClient;
        }

        public async Task<(T response, long callDuration)> GetCustomerData<T>(string token, string sessionId, string jsonRequest)
        {
            string returnValue = string.Empty;

            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };
            string path = string.Format("/GetProfile");
            _logger.LogError("CSL service-GetCustomerData-request {request}", jsonRequest);
            var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, jsonRequest, headers).ConfigureAwait(false);

            if (responseData.statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("CSL service-GetCustomerData {@RequestUrl} error {response}", responseData.url, responseData.response);
                if (responseData.statusCode != HttpStatusCode.BadRequest)
                    return default;
            }
            returnValue = responseData.response;

            _logger.LogInformation("CSL service-GetCustomerData {@RequestUrl} {response}", responseData.url, JsonConvert.SerializeObject(returnValue));
            return (returnValue == null) ? default : (JsonConvert.DeserializeObject<T>(returnValue), 0);
        }
    }
}
