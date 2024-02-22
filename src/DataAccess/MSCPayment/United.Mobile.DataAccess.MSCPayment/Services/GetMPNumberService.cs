using Autofac.Features.AttributeFilters;
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
    public class GetMPNumberService : IGetMPNumberService
    {

        private readonly ICacheLog<GetMPNumberService> _logger;
        private readonly IResilientClient _resilientClient;

        public GetMPNumberService(ICacheLog<GetMPNumberService> logger, [KeyFilter("GetMPNumberClientKey")] IResilientClient resilientClient)
        {
            _logger = logger;
            _resilientClient = resilientClient;
        }

        public async Task<string> GetMPNumberByEmployeeId(string path, string sessionId, string token)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                           {"Accept", "application/json"},
                          { "Authorization", token }
                     };
            try
            {
                var responseData = await _resilientClient.GetHttpAsyncWithOptions(path, headers).ConfigureAwait(true);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("CSL service-GetMPNumberByEmployeeId {@RequestUrl} {url} error {response}", _resilientClient?.BaseURL, responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(responseData.response);
                }

                _logger.LogInformation("CSL service-GetMPNumberByEmployeeId {@RequestUrl}, {response}", responseData.url, responseData.response);

                return responseData.response;
            }
            catch (Exception ex)
            {
                _logger.LogError("CSL service-GetMPNumberByEmployeeId error {Exception}", JsonConvert.SerializeObject(ex));
            }

            return default;
        }
        public async Task<string> GetSavedCCForMileaguePlusMember(string path, string sessionId, string token)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                           {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            try
            {
                var responseData = await _resilientClient.GetHttpAsyncWithOptions(path, headers).ConfigureAwait(true);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("CSL service-GetSavedCCForMileaguePlusMember {@RequestUrl} {url} error {response}", _resilientClient?.BaseURL, responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(responseData.response);
                }

                _logger.LogInformation("CSL service-GetSavedCCForMileaguePlusMember {@RequestUrl}, {response}", responseData.url, responseData.response);

                return responseData.response;
            }
            catch (Exception ex)
            {
                _logger.LogError("CSL service-GetSavedCCForMileaguePlusMember error {Exception}", JsonConvert.SerializeObject(ex), sessionId);
            }

            return default;
        }
        public async Task<string> GetProfileAddressByKey(string path, string sessionId, string token)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                           {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            try
            {
                var responseData = await _resilientClient.GetHttpAsyncWithOptions(path, headers).ConfigureAwait(true);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("CSL service-GetProfileAddressByKey {@RequestUrl} {url} error {response}", _resilientClient?.BaseURL, responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(responseData.response);
                }

                _logger.LogInformation("CSL service-GetProfileAddressByKey {@RequestUrl}, {response}", responseData.url, responseData.response);

                return responseData.response;
            }
            catch (Exception ex)
            {
                _logger.LogError("CSL service-GetProfileAddressByKey error {Exception}", JsonConvert.SerializeObject(ex));
            }

            return default;
        }
    }
}
