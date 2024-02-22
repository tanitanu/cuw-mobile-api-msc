using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using United.Definition;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Utility.Helper;
using United.Utility.Http;
using United.Utility.Serilog;

namespace United.Mobile.DataAccess.Product.Services
{
    public class LookUpTravelCreditService : ILookUpTravelCreditService
    {
        private readonly IResilientClient _resilientClient;
        private readonly ICacheLog<PaymentService> _logger;
        private readonly IConfiguration _configuration;
        public LookUpTravelCreditService([KeyFilter("LookUpTravelCreditClientKey")] IResilientClient resilientClient
            , ICacheLog<PaymentService> logger
            , IConfiguration configuration)
        {
            _resilientClient = resilientClient;
            _logger = logger;
            _configuration = configuration;
        }
        public async Task<MOBFOPResponse> LookUpTravelCredit(string token, string path, MOBFOPLookUpTravelCreditRequest request, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            _logger.LogInformation("CSL service-LookUpTravelCredit Request:{Request}", request);

            try
            {
                var lookUpTravelCreditRequest = JsonConvert.SerializeObject(request);
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, lookUpTravelCreditRequest, headers);
                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("CSL Service-LookUpTravelCredit {@RequestUrl} error {Response}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        return default;
                }

                _logger.LogInformation("CSL Service-LookUpTravelCredit {@RequestUrl}, {Response}", responseData.url, responseData.response);

                return (responseData.response == null) ? default : JsonConvert.DeserializeObject<MOBFOPResponse>(responseData.response);
            }
            catch (Exception ex)
            {
                _logger.LogError("CSL Service-LookUpTravelCredit error {Exception}", ex);
            }

            return default;
        }
    }
}
