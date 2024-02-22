using Autofac.Features.AttributeFilters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Utility.Http;

namespace United.Mobile.DataAccess.Product.Services
{
    public class MemberInformationService : IMemberInformationService
    {
        private readonly IResilientClient _resilientClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MemberInformationService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MemberInformationService(
           ILogger<MemberInformationService> logger,
           [KeyFilter("memberProfileConfigKey")] IResilientClient resilientClient,
           IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _resilientClient = resilientClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<string> GetMemberInformation(string token, string loyaltyId, bool returnMilesBalanceOnly)
        {
            var headers = new Dictionary<string, string> {
                {
                    "Authorization",token
                }
            };
            var path = loyaltyId;

            string ucbVersion = _configuration.GetSection("memberProfileConfig").GetValue<string>("ucbVersion");
            if (!string.IsNullOrEmpty(ucbVersion))
                path = $"{path}/{ucbVersion}?returnMilesBalanceOnly={returnMilesBalanceOnly}";
            else
                path = $"{path}?returnMilesBalanceOnly={returnMilesBalanceOnly}";

            try
            {
                var memberInfo = await _resilientClient.GetHttpAsyncWithOptions(path, headers).ConfigureAwait(false);


                var response = memberInfo.Item1;
                var statusCode = memberInfo.Item2;
                var url = memberInfo.Item3;

                if (statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("csl service-GetMemberInformation-service {@requestUrl} error {response}", url, response);

                    if (statusCode == HttpStatusCode.InternalServerError)
                        throw new Exception(response);
                }
                _logger.LogInformation("csl-GetMemberInformation-service {@requestUrl},{response}", url, response);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError("CSL service-GetMemberInformation-service error {Exception} and {ExceptionStack}", ex.Message, JsonConvert.SerializeObject(ex));
            }

            return default;
        }
    }
}
