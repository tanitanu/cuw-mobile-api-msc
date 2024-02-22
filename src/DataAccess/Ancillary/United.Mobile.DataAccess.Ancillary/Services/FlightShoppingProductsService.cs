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
    public class FlightShoppingProductsService : IFlightShoppingProductsService
    {
        private readonly IResilientClient _resilientClient;
        private readonly ICacheLog<FlightShoppingProductsService> _logger;

        public FlightShoppingProductsService(
              [KeyFilter("FlightShoppingClientKey")] IResilientClient resilientClient
            , ICacheLog<FlightShoppingProductsService> logger)
        {
            _resilientClient = resilientClient;
            _logger = logger;
        }
        public async Task<string> GetProducts(string token, string sessionId, string request, string transationId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            string requestData = string.Format("/GetProducts");
            _logger.LogInformation("CSL Service-GetProducts  parameters Request:{request}", request);
            var requestUrl = "";
            try
            {
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(requestData, request, headers);
                requestUrl = responseData.url;
                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("CSL Service-GetProducts {@RequestUrl} error {response}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        return default;
                }

                _logger.LogInformation("CSL Service-GetProducts {@RequestUrl},{response}", responseData.url, responseData.response);
                return responseData.response;
            }
            catch (Exception ex)
            {
                _logger.LogError("CSL Dervice-GetProducts error {@RequestUrl}, {Exception}", requestUrl, ex);
            }

            return default;
        }

        public async Task<string> ApplyCSLMilesPlusMoneyOptions(string token, string action, string request, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };
            string path = string.Format("/{0}", action);

            _logger.LogInformation("CSL Service - ApplyCSLMilesPlusMoneyOptions- Request:{request}", request);

            try
            {
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers);
                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("CSL Service-ApplyCSLMilesPlusMoneyOptions {@RequestUrl} error {response}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        return default;
                }

                _logger.LogInformation("CSL Service-ApplyCSLMilesPlusMoneyOptions {@RequestUrl}, {response}", responseData.url, responseData.response);
                return responseData.response;
            }

            catch (Exception ex)
            {
                _logger.LogError("CSL Service-ApplyCSLMilesPlusMoneyOptions error {Exception}", JsonConvert.SerializeObject(ex));
            }

            return default;
        }

        public async Task<string> GetCSLMilesPlusMoneyOptions(string token, string action, string request, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };
            string path = string.Format("/{0}", action);

            _logger.LogInformation("CSL service - GetCSLMilesPlusMoneyOptions- Request:{request}", request);

            try
            {
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers);
                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("CSL service-GetCSLMilesPlusMoneyOptions {@RequestUrl} error {response}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        return default;
                }

                _logger.LogInformation("CSL service-GetCSLMilesPlusMoneyOptions {@RequestUrl} {response}", responseData.url, responseData.response);
                return responseData.response;
            }
            catch (Exception ex)
            {
                _logger.LogError("CSL service-GetCSLMilesPlusMoneyOptions error {Exception}", JsonConvert.SerializeObject(ex));
            }
            return default;
        }

        public async Task<string> GetTripInsuranceInfo(string token, string action, string request, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };
            string path = string.Format("/{0}", action);

            _logger.LogInformation("CSL service - GetTripInsuranceInfo- Request:{request}", request);
            try
            {
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers);
                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("CSL Service-GetTripInsuranceInfo {@RequestUrl} error {response}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        return default;
                }

                _logger.LogInformation("CSL Service-GetTripInsuranceInfo {@RequestUrl} {response}", responseData.url, responseData);
                return responseData.response;
            }
            catch (Exception ex)
            {
                _logger.LogError("CSL Service-GetTripInsuranceInfo error {Exception}", JsonConvert.SerializeObject(ex));
            }

            return default;
        }
    }
}
