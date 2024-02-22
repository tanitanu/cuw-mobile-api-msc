using Autofac.Features.AttributeFilters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Mobile.DataAccess.MSCPayment.Interfaces;
using United.Utility.Helper;
using United.Utility.Http;

namespace United.Mobile.DataAccess.MSCPayment.Services
{
    public class GetTravelersService : IGetTravelersService
    {

        private readonly ICacheLog<GetTravelersService> _logger;
        private readonly IResilientClient _resilientClient;

        public GetTravelersService(ICacheLog<GetTravelersService> logger, [KeyFilter("GetTravelersClientKey")] IResilientClient resilientClient)
        {
            _logger = logger;
            _resilientClient = resilientClient;
        }

        public async Task<string> GetTravelersSavedCCDetails(string path, string request, string sessionId, string token)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                           {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            _logger.LogInformation("CSL service-GetTravelersSavedCCDetails  parameters Request:{@Request}", request);

            try
            {
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers).ConfigureAwait(true);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("CSL service-GetTravelersSavedCCDetails {RequestUrl} error {@response}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(responseData.response);
                }

                _logger.LogInformation("CSL service-GetTravelersSavedCCDetails {RequestUrl}, {@response}", responseData.url, responseData.response);

                return responseData.response;
            }
            catch (Exception ex)
            {
                _logger.LogError("CSL service-GetTravelersSavedCCDetails error {Exception} and {ExceptionStack}", ex.Message, JsonConvert.SerializeObject(ex));
            }

            return default;
        }

        public async Task<string> LookupAndSaveProfileCard(string path, string request, string sessionId, string token)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                           {"Accept", "application/json"},
                          { "Authorization", token }
                     };
            _logger.LogInformation("CSL service-LookupAndSaveProfileCard  parameters Request:{@Request} ", request);

            try
            {
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers).ConfigureAwait(true);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("CSL service-LookupAndSaveProfileCard {RequestUrl} error {@response}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(responseData.response);
                }

                _logger.LogInformation("CSL service-LookupAndSaveProfileCard {RequestUrl}, {@response}", responseData.url, responseData.response);

                return responseData.response;
            }
            catch (Exception ex)
            {
                _logger.LogError("CSL service-LookupAndSaveProfileCard error {Exception} and {ExceptionStack}",ex.Message, JsonConvert.SerializeObject(ex));
            }

            return default;
        }

        public async Task<string> GetCslApiResponse(string path, string request, string sessionId, string token)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                           {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            _logger.LogInformation("CSL service-TouchlessPaymentWallet  parameters Request:{@Request}", request);

            try
            {
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers).ConfigureAwait(true);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("CSL service-TouchlessPaymentWallet {RequestUrl} error {@response}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(responseData.response);
                }

                _logger.LogInformation("CSL service-TouchlessPaymentWallet {RequestUrl}, {@response}", responseData.url, responseData.response);

                return responseData.response;
            }
            catch (Exception ex)
            {
                _logger.LogError("CSL service-TouchlessPaymentWallet error {Exception} and {@ExceptionStack}", ex.Message, JsonConvert.SerializeObject(ex));
            }

            return default;
        }

        public async Task<string> OptOutMPCardInflightPurchase(string path, string request, string sessionId, string token)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                           {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            _logger.LogInformation("CSL service-OptOutMPCardInflightPurchase  parameters Request:{@Request}", request);

            try
            {
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers).ConfigureAwait(true);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("CSL service-OptOutMPCardInflightPurchase {RequestUrl} error {@response}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(responseData.response);
                }

                _logger.LogInformation("CSL service-OptOutMPCardInflightPurchase {RequestUrl}, {@response}", responseData.url, responseData.response);

                return responseData.response;
            }
            catch (Exception ex)
            {
                _logger.LogError("CSL service-OptOutMPCardInflightPurchase error {Exception} and {ExceptionStack}", ex.Message, JsonConvert.SerializeObject(ex));
            }

            return default;
        }
        public async Task<string> OptOutBookingCardInflightPurchase(string path, string request, string sessionId, string token)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                           {"Accept", "application/json"},
                          { "Authorization", token }
                     };
            _logger.LogInformation("CSL service-OptOutBookingCardInflightPurchase  parameters Request:{@Request} Path:{Path}", request, path);

            try
            {
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers).ConfigureAwait(true);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("CSL service-OptOutBookingCardInflightPurchase {RequestUrl} error {@response}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        throw new Exception(responseData.response);
                }

                _logger.LogInformation("CSL service-OptOutBookingCardInflightPurchase {RequestUrl}, {@response}", responseData.url, responseData.response);

                return responseData.response;
            }
            catch (Exception ex)
            {
                _logger.LogError("CSL service-OptOutBookingCardInflightPurchase error {Exception} and {ExceptionStack}", ex.Message, JsonConvert.SerializeObject(ex));
            }

            return default;
        }
    }
}
