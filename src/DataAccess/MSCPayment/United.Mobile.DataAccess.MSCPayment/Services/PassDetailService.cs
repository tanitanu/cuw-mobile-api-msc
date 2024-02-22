using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Mobile.DataAccess.MSCPayment.Interfaces;
using United.Utility.Helper;
using United.Utility.Http;

namespace United.Mobile.DataAccess.MSCPayment.Services
{
    public class PassDetailService : IPassDetailService
    {
        private readonly ICacheLog<PassDetailService> _logger;
        private readonly IResilientClient _resilientClient;

        public PassDetailService(ICacheLog<PassDetailService> logger, [KeyFilter("PassDetailClientServiceKey")] IResilientClient resilientClient)
        {
            _logger = logger;
            _resilientClient = resilientClient;
        }
        public async Task<string> GetLoyaltyUnitedClubIssuePass(string token, string action, string request, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            string path = string.Format("/{0}", action);
            _logger.LogInformation("CSL service-GetLoyaltyUnitedClubIssuePass {path} and {request}", path, request);

            var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers);
            if (responseData.statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("CSL service-GetLoyaltyUnitedClubIssuePass {@RequestUrl} error {response}", responseData.url, responseData.response);
                if (responseData.statusCode != HttpStatusCode.BadRequest)
                    return default;
            }

            _logger.LogInformation("CSL service-GetLoyaltyUnitedClubIssuePass {@RequestUrl} and {response}", responseData.url, responseData.response);
            return responseData.response;
        }
    }
}
