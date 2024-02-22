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

namespace United.Mobile.DataAccess.MSCPayment.Services
{
    public class CustomerPreferencesService : ICustomerPreferencesService
    {
        private readonly ICacheLog<CustomerPreferencesService> _logger;
        private readonly IResilientClient _resilientClient;

        public CustomerPreferencesService(ICacheLog<CustomerPreferencesService> logger, [KeyFilter("CustomerPreferencesClientKey")] IResilientClient resilientClient)
        {
            _logger = logger;
            _resilientClient = resilientClient;
        }

        public async Task<T> GetCustomerPrefernce<T>(string token, string savedUnfinishedBookingActionName, string savedUnfinishedBookingAugumentName, int customerID, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };
            string requestData = string.Format("{0}/{1}/{2}", savedUnfinishedBookingActionName, savedUnfinishedBookingAugumentName, customerID);
            _logger.LogInformation("CSL service-GetCustomerPreference {RequestData}", requestData);
            var responseData = await _resilientClient.GetHttpAsyncWithOptions(requestData, headers);
            if (responseData.statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("CSL service-GetCustomerPrefernce {@RequestUrl} {url} error {response}", _resilientClient?.BaseURL, responseData.url, responseData.response);
                if (responseData.statusCode != HttpStatusCode.BadRequest)
                    return default;
            }

            _logger.LogInformation("CSL service-GetCustomerPrefernce {@RequestUrl}, {response}", responseData.url, responseData.response);
            return JsonConvert.DeserializeObject<T>(responseData.response);
        }


        public async Task<string> PurgeAnUnfinishedBooking(string token, string action, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            var responseData = await _resilientClient.DeleteAsync(action, headers, true);
            if (responseData.statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("CSL service-PurgeAnUnfinishedBooking {@RequestUrl} error {response}", responseData.url, responseData.response);
                if (responseData.statusCode != HttpStatusCode.BadRequest)
                    return default;
            }


            _logger.LogInformation("CSL service-PurgeAnUnfinishedBooking {@RequestUrl}, {response}", responseData.url, responseData.response);
            return responseData.response;
        }
    }
}

