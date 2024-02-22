using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using United.Definition;
using United.Mobile.DataAccess.MSCPayment.Interfaces;
using United.Utility.Helper;
using United.Utility.Http;
using United.Utility.Serilog;

namespace United.Mobile.DataAccess.MSCPayment.Services
{
    public class FlightShoppingService : IFlightShoppingService
    {
        private readonly IResilientClient _resilientClient;
        private readonly ICacheLog<FlightShoppingService> _logger;
        private readonly IConfiguration _configuration;
        public FlightShoppingService([KeyFilter("FlightShoppingClientKey")] IResilientClient resilientClient
            , ICacheLog<FlightShoppingService> logger
            , IConfiguration configuration)
        {
            _resilientClient = resilientClient;
            _logger = logger;
            _configuration = configuration;

        }

        public async Task<string> GetProducts(string token, string request, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            string path = "/GetProducts";

            _logger.LogInformation("CSL service-GetProducts  parameters Request:{Request} Path:{Path}", request, path);

            var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers).ConfigureAwait(false);
            if (responseData.statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("CSL-GetProducts-GetProducts {@RequestUrl} error {response}", responseData.url, responseData.response);
                if (responseData.statusCode != HttpStatusCode.BadRequest)
                    throw new Exception(responseData.response);
            }

            _logger.LogInformation("CSL-GetProducts-GetProducts {@RequestUrl}, {response}", responseData.url, responseData.response);

            return responseData.response;
        }

        public async Task<string> GetCartInformation(string token, string action, string request, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };
            string path = string.Format("/{0}", action);

            _logger.LogInformation("CSL service-GetCartInformation  parameters Request:{Request} Path:{Path}", request, path);

            var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers);
            if (responseData.statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("CSL service-GetCartInformation {@RequestUrl} error {Response}", responseData.url, responseData.response);
                if (responseData.statusCode != HttpStatusCode.BadRequest)
                    return default;
            }

            _logger.LogInformation("CSL service-GetCartInformation {@RequestUrl}, {Response}", responseData.url, responseData.response);
            return (responseData.response == null) ? default : JsonConvert.DeserializeObject<string>(responseData.response);
        }
    }
}

