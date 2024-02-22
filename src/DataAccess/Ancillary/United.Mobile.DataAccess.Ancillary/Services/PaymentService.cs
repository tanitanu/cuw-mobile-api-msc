using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Configuration;
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
    public class PaymentService : IPaymentService
    {
        private readonly IResilientClient _resilientClient;
        private readonly ICacheLog<PaymentService> _logger;
        private readonly IConfiguration _configuration;
        public PaymentService([KeyFilter("PaymentServiceClientKey")] IResilientClient resilientClient
            , ICacheLog<PaymentService> logger
            , IConfiguration configuration)
        {
            _resilientClient = resilientClient;
            _logger = logger;
            _configuration = configuration;
        }
        public async Task<string> GetEligibleFormOfPayments(string token, string path, string request, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            _logger.LogInformation("CSL Service-GetEligibleFormOfPayments - Request:{@Request}", JsonConvert.SerializeObject(request));

            try
            {
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers);
                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("CSL Service-GetEligibleFormOfPayments {@RequestUrl} error {Response}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        return default;
                }

                _logger.LogInformation("CSL Service-GetEligibleFormOfPayments {@RequestUrl}, {Response}", responseData.url, responseData.response);
                return (responseData.response == null) ? default : responseData.response;
            }
            catch (Exception ex)
            {
                _logger.LogError("CSL Service-GetEligibleFormOfPayments error {Exception} and {ExceptionStack}", ex.Message, JsonConvert.SerializeObject(ex));
            }

            return default;
        }

        public async Task<string> GetFFCByEmail(string token, string path, string request, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            _logger.LogInformation("CSL service-GetFFCByEmail  parameters Request:{Request} ", request);

            var (response, statusCode, url) = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers);
            if (statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("CSL service-GetFFCByEmail {@RequestUrl} error {response}", url, response);
                if (statusCode != HttpStatusCode.BadRequest)
                    return default;
            }

            _logger.LogInformation("CSL service-GetFFCByEmail {@RequestUrl} {response}", url, response);

            return (response == null) ? default : response;
        }

        public async Task<string> GetETCByEmail(string token, string path, string request, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            _logger.LogInformation("CSL service-GetETCByEmail  parameters Request:{Request} ", request);

            var (response, statusCode, url) = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers);
            if (statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("CSL service-GetETCByEmail {@RequestUrl} error {response}", url, response);
                if (statusCode != HttpStatusCode.BadRequest)
                    return default;
            }

            _logger.LogInformation("CSL service-GetETCByEmail {@RequestUrl} {response}", url, response);

            return (response == null) ? default : response;
        }

        public async Task<string> GetFFCByPnr(string token, string path, string request, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            _logger.LogInformation("CSL service-GetFFCByPnr  parameters Request:{Request}", request);

            var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers);
            if (responseData.statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("CSL service-GetFFCByPnr {@RequestUrl} error {response}", responseData.url, responseData.response);
                if (responseData.statusCode != HttpStatusCode.BadRequest)
                    return default;
            }

            _logger.LogInformation("CSL service-GetFFCByPnr {@RequestUrl} {response}", responseData.url, responseData.response);
            return (responseData.response == null) ? default : responseData.response;
        }

        public async Task<string> GetLookUpTravelCredit(string token, string path, string request, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            _logger.LogInformation("CSL service-GetLookUpTravelCredit  parameters Request:{Request}", request);

            var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers);
            if (responseData.statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("CSL service-GetLookUpTravelCredit {@RequestUrl} error {response}", responseData.url, responseData.response);
                if (responseData.statusCode != HttpStatusCode.BadRequest)
                    return default;
            }

            _logger.LogInformation("CSL service-GetLookUpTravelCredit {@RequestUrl} {response}", responseData.url, responseData.response);
            return (responseData.response == null) ? default : responseData.response;
        }

    }
}
