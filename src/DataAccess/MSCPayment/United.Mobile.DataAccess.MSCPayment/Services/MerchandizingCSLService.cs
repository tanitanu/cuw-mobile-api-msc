using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.DataAccess.MSCPayment.Interfaces;
using United.Utility.Helper;
using United.Utility.Http;

namespace United.Mobile.DataAccess.MSCPayment.Services
{
    public class MerchandizingCSLService : IMerchandizingCSLService
    {
        private readonly IResilientClient _resilientClient;
        private readonly ICacheLog<MerchandizingCSLService> _logger;
        private readonly IConfiguration _configuration;
        public MerchandizingCSLService([KeyFilter("MerchandizingCSLServiceClientKey")] IResilientClient resilientClient
            , ICacheLog<MerchandizingCSLService> logger
            , IConfiguration configuration)
        {
            _resilientClient = resilientClient;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<string> DeclineTPIOffer(string token, string request)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            string path = "/DeclineOffers";

            _logger.LogInformation("CSL service-TPIDeclineOffers  parameters Request:{Request} Path:{Path}", request, path);

            var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers).ConfigureAwait(false);
            if (responseData.statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("CSL-DeclineOffers-DeclineOffers {@RequestUrl} error {response}", responseData.url, responseData.response);
                if (responseData.statusCode != HttpStatusCode.BadRequest)
                    throw new Exception(responseData.response);
            }

            _logger.LogInformation("CSL-DeclineOffers-DeclineOffers {@RequestUrl}, {response}", responseData.url, responseData.response);

            return responseData.response;
        }
    }
}
