using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using United.Ebs.Logging.Enrichers;
using United.Utility.AppVersion;
using United.Utility.Helper;
using United.Utility.Http;

namespace United.Mobile.DataAccess.Common
{
    public class VerifyMileagePlusHashpinService : IVerifyMileagePlusHashpinService
    {
        private readonly ICacheLog<VerifyMileagePlusHashpinService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IResilientClient _resilientClient;
        public VerifyMileagePlusHashpinService([KeyFilter("VerifyMileagePlusHashpinServiceKey")] IResilientClient resilientClient
            , ICacheLog<VerifyMileagePlusHashpinService> logger
            , IConfiguration configuration)
        {
            _resilientClient = resilientClient;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<T> VerifyMileagePlusHashpin<T>(string token, string request, string sessionId)
        {
            using (_logger.BeginTimedOperation("Total time taken for ValidateHashPin service call", sessionId))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept","application/json" }
                     };
                _logger.LogInformation("CSL service-SeatEngine  parameters Request:{Request} Path:{Path}", request, _resilientClient.BaseURL);
                var responseData = await _resilientClient.PostHttpAsyncWithOptions("/VerifyMileagePlusHashpin", request, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("VerifyMileagePlusHashpin service {@RequestUrl} {url} error {response}", _resilientClient?.BaseURL, responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(responseData.response);
                }

                _logger.LogInformation("VerifyMileagePlusHashpin service {@RequestUrl} {response}", responseData.url, responseData.response);

                var responseObject = JsonConvert.DeserializeObject<T>(responseData.response);

                return responseObject;
            }
        }
    }
}
