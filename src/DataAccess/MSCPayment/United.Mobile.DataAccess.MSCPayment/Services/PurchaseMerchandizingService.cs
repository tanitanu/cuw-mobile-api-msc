using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Configuration;
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
    public class PurchaseMerchandizingService : IPurchaseMerchandizingService
    {
        private readonly IResilientClient _resilientClient;
        private readonly ICacheLog<PurchaseMerchandizingService> _logger;
        private readonly IConfiguration _configuration;
        public PurchaseMerchandizingService([KeyFilter("MerchandizingClientKey")] IResilientClient resilientClient
            , ICacheLog<PurchaseMerchandizingService> logger
            , IConfiguration configuration)
        {
            _resilientClient = resilientClient;
            _logger = logger;
            _configuration = configuration;

        }
        public async Task<string> GetInflightPurchaseEligibility(string token, string request, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            string path = "/GetProductEligibility";

            _logger.LogInformation("CSL service-GetInflightPurchaseEligibility  parameters Request:{@Request} Path:{Path}", request, path);
            try
            {
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers);
                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("CSL service-GetInflightPurchaseEligibility {RequestUrl} error {@response}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        return default;
                }
                _logger.LogInformation("CSL service-GetInflightPurchaseEligibility {RequestUrl}, {@response}", responseData.url, responseData.response);
                return responseData.response;
            }
            catch (Exception ex)
            {
                _logger.LogError("CSL service-GetInflightPurchaseEligibility error {@Exception} and {exceptionstack}", ex.Message, JsonConvert.SerializeObject(ex));
            }
            return default;
        }
    }
}
