using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Utility.Http;
using United.Utility.Serilog;
using System.Linq;
using United.Utility.Helper;
using United.Services.FlightShopping.Common.FlightReservation;

namespace United.Mobile.DataAccess.Ancillary.Services
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IResilientClient _resilientClient;
        private readonly ICacheLog<ShoppingCartService> _logger;
        private readonly IConfiguration _configuration;
        public ShoppingCartService([KeyFilter("ShoppingCartClientKey")] IResilientClient resilientClient
            , ICacheLog<ShoppingCartService> logger
            , IConfiguration configuration)
        {
            _resilientClient = resilientClient;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<T> GetCartInformation<T>(string token, string action, string request, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };
            string path = string.Format("/{0}", action);

            _logger.LogInformation("CSL service- GetCartInformation parameters Request:{request}", JsonConvert.SerializeObject(request));

                try
                {
                    var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers);
                    if (responseData.statusCode != HttpStatusCode.OK)
                    {
                        _logger.LogError("CSL service-GetCartInformation {@RequestUrl} error {response}", responseData.url, responseData.response);
                        if (responseData.statusCode != HttpStatusCode.BadRequest)
                            return default;
                    }

                    _logger.LogInformation("CSL service-GetCartInformation {@RequestUrl}, {response}", responseData.url, responseData.response);
                    return (responseData.response == null) ? default : JsonConvert.DeserializeObject<T>(responseData.response);
                }
                catch (Exception ex)
                {
                    _logger.LogError("CSL service-GetCartInformation error {Exception}", JsonConvert.SerializeObject(ex));
                }

                return default;

        }

        public async Task<string> MetaSyncUserSession<T>(string token, string sessionId, string action, string request)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };
            string path = string.Format("/{0}", action);

            _logger.LogInformation("CSL service-MetaSyncUserSession  parameters Request:{Request}", request);

            var response = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers);

            if (response.statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("CSL service-MetaSyncUserSession {@RequestUrl} error {response}", response.url, response.response);
                if (response.statusCode != HttpStatusCode.BadRequest)
                    return default;
            }

            _logger.LogInformation("CSL service-MetaSyncUserSession {@RequestUrl}, {response}", response.url, response.response);

            return (response.response == null) ? default : response.response;
        }

        public async Task<string> CreateCart(string token, string request, string sessionId)
        {
            string returnValue = string.Empty;

                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };

                _logger.LogInformation("CSL service-CreateCart  parameters Request:{Request}", request);

                var responseData = await _resilientClient.PostHttpAsyncWithOptions("", request, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("CSL service-CreateCart {@RequestUrl} error {response}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        return default;
                }
                returnValue = responseData.response;

                _logger.LogInformation("CSL service-CreateCart {response} {@RequestUrl}", responseData.response, responseData.url);
            
            return JsonConvert.DeserializeObject<string>(returnValue);
        }

        public async Task<T> RegisterFlights<T>(string token, string action, string request, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };
            string path = string.Format("/{0}", action);

            _logger.LogInformation("CSL service-RegisterFlights  parameters Request:{Request}", request);

                try
                {
                    var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers);
                    if (responseData.statusCode != HttpStatusCode.OK)
                    {
                        _logger.LogError("CSL service-RegisterFlights {@RequestUrl} error {response}", responseData.url, responseData.response);
                        if (responseData.statusCode != HttpStatusCode.BadRequest)
                            return default;
                    }

                    _logger.LogInformation("CSL service-RegisterFlights {@RequestUrl} {response}", responseData.url, responseData.response);
                    return (responseData.response == null) ? default : JsonConvert.DeserializeObject<T>(responseData.response);
                }

                catch (Exception ex)
                {
                    _logger.LogError("CSL service-RegisterFlights error {Exception}", JsonConvert.SerializeObject(ex));
                }

                return default;
        }

        public async Task<T> RegisterOrRemoveCoupon<T>(string token, string action, string request, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };
            string path = string.Format("/{0}", action);

            _logger.LogInformation("CSL service-RegisterOrRemoveCoupon  parameters Request:{Request}", request);

                try
                {
                    var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers);
                    if (responseData.statusCode != HttpStatusCode.OK)
                    {
                        _logger.LogError("CSL service-RegisterOrRemoveCoupon {@RequestUrl} error {response}", responseData.url, responseData.response);
                        if (responseData.statusCode != HttpStatusCode.BadRequest)
                            return default;
                    }

                    _logger.LogInformation("CSL service-RegisterOrRemoveCoupon {@RequestUrl} {response}", responseData.url, responseData.response);
                    return (responseData.response == null) ? default : JsonConvert.DeserializeObject<T>(responseData.response);
                }
                catch (Exception ex)
                {
                    _logger.LogError("CSL service-RegisterOrRemoveCoupon error {Exception}", JsonConvert.SerializeObject(ex));
                }

                return default;

        }

        public async Task<T> RegisterOffers<T>(string token, string action, string request, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };
            string path = string.Format("/{0}", action);

            _logger.LogInformation("CSL service-RegisterOffers  parameters Request:{@Request}", request);

                try
                {
                    var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers);
                    if (responseData.statusCode != HttpStatusCode.OK)
                    {
                        _logger.LogError("CSL service-RegisterOffers {@RequestUrl} error {Response}", responseData.url, responseData.response);
                        if (responseData.statusCode != HttpStatusCode.BadRequest)
                            return default;
                    }

                    _logger.LogInformation("CSL service-RegisterOffers {@RequestUrl}, {Response}", responseData.url, responseData.response);
                    return (responseData.response == null) ? default : JsonConvert.DeserializeObject<T>(responseData.response);
                }

                catch (Exception ex)
                {
                    _logger.LogError("CSL service-RegisterOffers error {Exception}", JsonConvert.SerializeObject(ex));
                }

                return default;
        }

        public async Task<string> RegisterFareLockReservation(string token, string action, string request, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            string path = string.Format("/{0}", action);
            _logger.LogInformation($"CSL service-{action}" + " {@Request}", request);
            try
            {
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers);
                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError($"CSL service-{action}" + " {@RequestUrl} error {Response}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        return default;
                }
                if (!string.IsNullOrEmpty(responseData.response))
                    _logger.LogInformation($"CSL service-{action}" + " {@RequestUrl} {Response}", responseData.url, responseData.response);
                return responseData.response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"CSL service-{action}" + " error {Exception} and {ExceptionStack}", ex.Message, JsonConvert.SerializeObject(ex));
            }

            return default;
        }

        public async Task<string> RegisterCheckinSeats(string token, string action, string request, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            string path = string.Format("/{0}", action);

            _logger.LogInformation("CSL service-RegisterCheckinSeats {@Request}", request);
            try
            {
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers);
                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("CSL service-RegisterCheckinSeats {@RequestUrl} error {response}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        return default;
                }

                _logger.LogInformation("CSL service-RegisterCheckinSeats {@RequestUrl}, {response}", responseData.url, responseData.response);
                return responseData.response;
            }
            catch (Exception ex)
            {
                _logger.LogError("CSL service-RegisterCheckinSeats error {Exception} and {ExceptionStack}", ex.Message, JsonConvert.SerializeObject(ex));
            }

            return default;
        }

        public async Task<string> RegisterSameDayChange(string token, string action, string request, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            string path = string.Format("/{0}", action);

            _logger.LogInformation("CSL service-RegisterSameDayChange {@Request}", request);

            try
            {
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers);
                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("CSL service-RegisterSameDayChange {@RequestUrl} error {response}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        return default;
                }

                _logger.LogInformation("CSL service-RegisterSameDayChange {@RequestUrl}, {response}", responseData.url, responseData.response);
                return responseData.response;
            }
            catch (Exception ex)
            {
                _logger.LogError("CSL service-RegisterSameDayChange error {Exception} and {ExceptionStack}", ex.Message, JsonConvert.SerializeObject(ex));
            }

            return default;
        }

        public async Task<string> RegisterFormsOfPayments_CFOP(string token, string action, string request, string sessionId, Dictionary<string, string> additionalHeaders, string clonedRequest = null)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };
            if (_configuration.GetValue<bool>("EnableAdditionalHeadersForMosaicInRFOP"))
            {
                if (additionalHeaders != null && additionalHeaders.Any())
                {
                    foreach (var item in additionalHeaders)
                    {
                        headers.Add(item.Key, item.Value);
                    }
                }
            }
            string path = string.Format("/{0}", action);

            if (!_configuration.GetValue<bool>("EnableRemoveTaxIdInformation") && !string.IsNullOrEmpty(clonedRequest))
            {
                _logger.LogInformation("CSL service-RegisterFormsOfPayments_CFOP {Request}", clonedRequest);
            }
            else 
            {
                _logger.LogInformation("CSL service-RegisterFormsOfPayments_CFOP {Request}", request);
            }
            

            var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers);
            if (responseData.statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("CSL service-RegisterFormsOfPayments_CFOP {@RequestUrl} error {response}", responseData.url, responseData.response);
                if (responseData.statusCode != HttpStatusCode.BadRequest)
                    return default;
            }

            if (!_configuration.GetValue<bool>("EnableRemoveTaxIdInformation") && !string.IsNullOrEmpty(clonedRequest))
            {
                #region Remove description from tax Id Information for security purposes
                object clonedResponse = responseData.response.Clone();
                FlightReservationResponse response = JsonConvert.DeserializeObject<FlightReservationResponse>(clonedResponse.ToString());
                if(response?.Reservation?.Services?.Count > 0 
                    && response.CheckoutResponse?.ShoppingCart?.Items?.Count > 0)
                {
                    foreach (var service in response.Reservation.Services) 
                    {
                        if (response.CheckoutResponse.ShoppingCart.Items[0].ProductContext.Count > 0) 
                        {
                            if (response.CheckoutResponse.ShoppingCart.Items[0].ProductContext[0].Contains(service.Description)) 
                            {
                                var removeStr = response.CheckoutResponse.ShoppingCart.Items[0].ProductContext[0].Replace(service.Description, string.Empty);
                                response.CheckoutResponse.ShoppingCart.Items[0].ProductContext[0] = removeStr;
                            }
                        }
                        service.Description = string.Empty;
                    }
                }
                _logger.LogInformation("CSL service-RegisterFormsOfPayments_CFOP {@RequestUrl}, {response}", responseData.url, JsonConvert.SerializeObject(response));
                #endregion
            }
            else 
            {
                _logger.LogInformation("CSL service-RegisterFormsOfPayments_CFOP {@RequestUrl}, {response}", responseData.url, responseData.response);
            }
            return responseData.response;

        }

        public async Task<string> RegisterSeats_CFOP(string token, string action, string request, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            string path = string.Format("/{0}", action);

            _logger.LogInformation("CSL service-RegisterSeats_CFOP  parameters Request:{Request}", request);

            
                try
                {
                    var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers);
                    if (responseData.statusCode != HttpStatusCode.OK)
                    {
                        _logger.LogError("CSL service-RegisterSeats_CFOP {@RequestUrl} error {Response}", responseData.url, responseData.response);
                        if (responseData.statusCode != HttpStatusCode.BadRequest)
                            return default;
                    }

                    _logger.LogInformation("CSL service-RegisterSeats_CFOP {@RequestUrl}, {Response}", responseData.url, responseData.response);
                    return responseData.response;
                }
                catch (Exception ex)
                {
                    _logger.LogError("CSL service-RegisterSeats_CFOP error {Exception}", JsonConvert.SerializeObject(ex));
                }

                return default;

        }

        public async Task<string> ClearSeats(string token, string action, string request, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            string path = string.Format("{0}", action);

            _logger.LogInformation("CSL service-ClearSeats {Request}", request);

                try
                {
                    var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers);
                    if (responseData.statusCode != HttpStatusCode.OK)
                    {
                        _logger.LogError("CSL service-ClearSeats {@RequestUrl} error {Response}", responseData.url, responseData.response);
                        if (responseData.statusCode != HttpStatusCode.BadRequest)
                            return default;
                    }

                    _logger.LogInformation("CSL service-ClearSeats {@RequestUrl} {Response}", responseData.url, responseData.response);
                    return responseData.response;
                }
                catch (Exception ex)
                {
                    _logger.LogError("CSL service-ClearSeats error {Exception}", JsonConvert.SerializeObject(ex));
                }

                return default;
        }

        /// <summary>
        /// Shopping Cart Service call can be used for all shopping cart service no need to create individual service call.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="action">action path</param>
        /// <param name="request"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public async Task<string> ShoppingCartServiceCall(string token, string action, string request, string sessionId)
        {
            IDictionary<string, string> headers = new Dictionary<string, string> { { "Accept", "application/json" }, { "Authorization", token } };
            string path = string.Format("{0}", action);
            string message = $"CSL service-{action.Replace("/", string.Empty)}";
            message = $"{message} @{request},@{path}";
            _logger.LogInformation("CSL ShoppingCartServiceCall {@Message}", message);
            try
            {
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, request, headers);
                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    message = $"{message} @{responseData.url}, @{responseData.response}";
                    _logger.LogError("CSL ShoppingCartServiceCall-Error {@Message}", message);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        return default;
                }
                message = $"{message} @{responseData.url}, @{responseData.response}";
                _logger.LogInformation("CSL ShoppingCartServiceCall {@message}", message);
                return responseData.response;
            }
            catch (Exception ex)
            {
                _logger.LogError("CSL ShoppingCartServiceCall-Error {@Exception}", JsonConvert.SerializeObject(ex));
            }
            return default;
        }

        public async Task<string> DeletePayment(string token, string action, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };


            var responseData = await _resilientClient.DeleteAsync(action, headers, true);
            if (responseData.statusCode != HttpStatusCode.OK)
            {
                _logger.LogError("CSL service-DeletePayment {@RequestUrl} error {response}", responseData.url, responseData.response);
                if (responseData.statusCode != HttpStatusCode.BadRequest)
                    return default;
            }

            _logger.LogInformation("CSL service-DeletePayment {@RequestUrl}, {response}", responseData.url, responseData.response);
            return responseData.response;

        }
        public async Task<T> GetRegisterSeats<T>(string token, string action, string sessionId, string jsonRequest)
        {
            _logger.LogInformation("CSL service-GetRegisterSeats {token}, {action}, {request} for {sessionId}", token, action, jsonRequest, sessionId);
            
            string returnValue = string.Empty;

                Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };
                string path = string.Format("{0}", action);
                var responseData = await _resilientClient.PostHttpAsyncWithOptions(path, jsonRequest, headers).ConfigureAwait(false);

                if (responseData.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("CSL service-GetRegisterSeats {@RequestUrl} error {response}", responseData.url, responseData.response);
                    if (responseData.statusCode != HttpStatusCode.BadRequest)
                        return default;
                }
                returnValue = responseData.response;

                _logger.LogInformation("CSL service-GetRegisterSeats  {@RequestUrl} {response}", responseData.url, responseData.response);
           
            return (returnValue == null) ? default : (JsonConvert.DeserializeObject<T>(returnValue));
        }


        public async Task<T> GetAsync<T>(string actionName, string token, string sessionId)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>
                     {
                          {"Accept", "application/json"},
                          { "Authorization", token }
                     };

            _logger.LogInformation("CSL ShoppingCart Service GetAsync parameters ActionName:{actionName}", actionName);

            try
            {
                var response = await _resilientClient.GetHttpAsyncWithOptions(actionName, headers);
                if (response.statusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("CSL ShoppingCart Service GetAsync {@RequestUrl} {url} error {response}", _resilientClient?.BaseURL, response.url, response.response);
                    if (response.statusCode != HttpStatusCode.BadRequest)
                        return default;
                }

                _logger.LogInformation("CSL ShoppingCart Service GetAsync {@RequestUrl}, {response}", response.url, response.response);
                return (response.response == null) ? default : JsonConvert.DeserializeObject<T>(response.response);
            }
            catch (Exception ex)
            {
                _logger.LogError("CSL ShoppingCart Service GetAsync error {Exception}", JsonConvert.SerializeObject(ex));
            }

            return default;
        }
    }
}
