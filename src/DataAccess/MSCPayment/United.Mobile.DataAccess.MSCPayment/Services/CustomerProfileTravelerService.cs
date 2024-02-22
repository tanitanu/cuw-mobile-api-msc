using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using United.Utility.Http;

namespace United.Mobile.DataAccess.MSCPayment.Services
{
    public class CustomerProfileTravelerService :ICustomerProfileTravelerService
    {
        private readonly ILogger<CustomerProfileTravelerService> _logger;
        private readonly IResilientClient _resilientClient;

        public CustomerProfileTravelerService(
              [KeyFilter("CSLGetProfileTravelerDetailsServiceKey")] IResilientClient resilientClient
            , ILogger<CustomerProfileTravelerService> logger)
        {
            _resilientClient = resilientClient;
            _logger = logger;
        }
        public async Task<string> GetProfileTravelerInfo(string token, string sessionId, string mpNumber)
        {

            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };
            string requestData = string.Format("{0}", mpNumber);

            _logger.LogInformation("CSL GetProfile all traveler service {Request}", requestData);

            var responseData = await _resilientClient.GetHttpAsyncWithOptions(requestData, headers).ConfigureAwait(false);

            if (responseData.statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("CSL GetProfile all traveler service {@RequestUrl} {url} error {Response}", _resilientClient?.BaseURL, responseData.url, responseData.response);
                if (responseData.statusCode != HttpStatusCode.BadRequest)
                    return default;
            }
            _logger.LogInformation("CSL GetProfile all traveler service {@RequestUrl} {Response}", responseData.url, responseData.response);
            return responseData.response;
        }
    }
}
