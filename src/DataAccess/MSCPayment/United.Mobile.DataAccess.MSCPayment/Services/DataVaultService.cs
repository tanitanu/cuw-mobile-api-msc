using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Mobile.DataAccess.MSCPayment.Interfaces;
using United.Utility.Helper;
using United.Utility.Http;
using United.Utility.Serilog;

namespace United.Mobile.DataAccess.MSCPayment.Services
{
    public class DataVaultService : IDataVaultService
    {
        private readonly ICacheLog<DataVaultService> _logger;
        private readonly IResilientClient _resilientClient;
        private readonly IConfiguration _configuration;

        public DataVaultService(ICacheLog<DataVaultService> logger, [KeyFilter("DataVaultTokenClientKey")] IResilientClient resilientClient
            , IConfiguration configuration)
        {
            _logger = logger;
            _resilientClient = resilientClient;
            _configuration = configuration;
            ConfigUtility.UtilityInitialize(_configuration);
        }

        public async Task<string> GetPersistentToken(string token, string requestData, string url, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            var gPTokenData = await _resilientClient.GetHttpAsyncWithOptions(url, headers);

            if (gPTokenData.statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("CSL Call-GetPersistentToken-service {@RequestUrl} {url} error {response}", _resilientClient?.BaseURL, gPTokenData.url, gPTokenData.statusCode);
                if (gPTokenData.statusCode != HttpStatusCode.BadRequest)
                    throw new Exception(gPTokenData.response);
            }

            _logger.LogInformation("CSL Call-GetPersistentToken-service {@RequestUrl}, {response}", gPTokenData.url, gPTokenData.response);

            return gPTokenData.response;
        }
        public async Task<string> GetCCTokenWithDataVault(string token, string request, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          { "Authorization", token },
                          {"Accept","application/json" }
                     };
            string path = "/AddPayment";

            _logger.LogInformation("CSL service-GetCCTokenWithDataVault  parameters Request:{@Request}", ConfigUtility.RemoveEncryptedCardNumberFromDatavaultCSLRequest(request));

            var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers).ConfigureAwait(false);

            if (responseData.statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("CSL  service-GetCCTokenWithDataVault {@RequestUrl} error {response}", responseData.url, responseData.response);
                if (responseData.statusCode != HttpStatusCode.BadRequest)
                    throw new Exception(responseData.response);
            }

            _logger.LogInformation("CSL  service-GetCCTokenWithDataVault {@RequestUrl} , {response}", responseData.url, ConfigUtility.RemoveEncryptedCardNumberFromDatavaultCSLResponse(responseData));
            return responseData.response;

        }

    }
}
