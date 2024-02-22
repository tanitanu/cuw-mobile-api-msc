using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
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
    public class ProvisionService : IProvisionService
    {
        private readonly IResilientClient _resilientClient;
        private readonly ICacheLog<ProvisionService> _logger;
        private readonly IConfiguration _configuration;

        public ProvisionService([KeyFilter("ProvisionServiceKey")] IResilientClient resilientClient
          , ICacheLog<ProvisionService> logger
          , IConfiguration configuration)
        {
            _resilientClient = resilientClient;
            _logger = logger;
            _configuration = configuration;
        }
        public async Task<string> CSL_PartnerProvisionCall(string token, string path, string request)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            _logger.LogInformation("CSL Service-CSL_PartnerProvisionCall - Request:{@Request}", JsonConvert.SerializeObject(request));

            try
            {
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers);
                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("CSL Service-CSL_PartnerProvisionCall {@RequestUrl} error {Response}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        return default;
                }

                _logger.LogInformation("CSL Service-CSL_PartnerProvisionCall {@RequestUrl}, {Response}", responseData.url, responseData.response);
                return (responseData.response == null) ? default : responseData.response;
            }
            catch (Exception ex)
            {
                _logger.LogError("CSL Service-CSL_PartnerProvisionCall error {Exception} and {ExceptionStack}", ex.Message, JsonConvert.SerializeObject(ex));
            }

            return default;
        }
    }
}
