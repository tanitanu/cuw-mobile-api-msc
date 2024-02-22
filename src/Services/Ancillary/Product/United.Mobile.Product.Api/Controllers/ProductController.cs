using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Definition;
using United.Definition.CFOP;
using United.Definition.FormofPayment;
using United.Definition.Shopping;
using United.Definition.Shopping.UnfinishedBooking;
using United.Ebs.Logging.Enrichers;
using United.Mobile.DataAccess.Common;
using United.Mobile.Model;
using United.Mobile.Model.Common;
using United.Mobile.Product.Domain;
using United.Utility.Helper;
using United.Utility.Serilog;

namespace United.Mobile.Product.Api.Controllers
{

    [Route("productservice/api")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ICacheLog<ProductController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHeaders _headers;
        private readonly IProductBusiness _productBusiness;
        private readonly IApplicationEnricher _applicationEnricher;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly IFeatureSettings _featureSettings;
        public ProductController(ICacheLog<ProductController> logger
            , IConfiguration configuration
            , IHeaders headers
            , IProductBusiness productBusiness
            , IApplicationEnricher applicationEnricher
            , ISessionHelperService sessionHelperService
            , IFeatureSettings featureSettings)
        {
            _logger = logger;
            _configuration = configuration;
            _headers = headers;
            _productBusiness = productBusiness;
            _applicationEnricher = applicationEnricher;
            _sessionHelperService = sessionHelperService;
            _featureSettings=featureSettings;
    }

        [HttpGet]
        [Route("version")]
        public virtual string Version()
        {
            string serviceVersionNumber = null;

            try
            {
                serviceVersionNumber = Environment.GetEnvironmentVariable("SERVICE_VERSION_NUMBER");
            }
            catch
            {
                // Suppress any exceptions
            }
            finally
            {
                serviceVersionNumber = (null == serviceVersionNumber) ? "0.0.0" : serviceVersionNumber;
            }

            return serviceVersionNumber;
        }

        [HttpGet]
        [Route("HealthCheck")]
        public string HealthCheck()
        {
            return "Healthy";
        }
        [HttpGet]
        [Route("environment")]
        public virtual string ApiEnvironment()
        {
            try
            {
                return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            }
            catch
            {
            }
            return "Unknown";
        }
        [HttpPost]
        [Route("Product/RegisterOffersForBooking")]
        public async Task<MOBBookingRegisterOfferResponse> RegisterOffersForBooking(MOBRegisterOfferRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);

            var response = new MOBBookingRegisterOfferResponse();
            var shoppingCart = new MOBShoppingCart();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("RegisterOffersForBooking {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for RegisterOffersForBooking business call", request.SessionId))
                {

                    response = await _productBusiness.RegisterOffersForBooking(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                response.PromoCodeRemoveAlertForProducts = _productBusiness.GetPromoCodeAlertMessage(request.ContinueToRegisterAncillary);
                _logger.LogWarning("RegisterOffersForBooking UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException();
                if (uaex != null && !string.IsNullOrEmpty(uaex.Message) && request.CartKey != "TPI")
                {
                    response.Exception.Code = (string.IsNullOrEmpty(uaex.Code)) ? "9999" : uaex.Code.ToString();
                    response.Exception.Message = uaex.Message.Trim();
                    response.Exception.ErrMessage = $"{uaex.InnerException?.Message} - {uaex.StackTrace}";
                }
                else
                {
                    response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
            }
            catch (Exception ex)
            {

                response.PromoCodeRemoveAlertForProducts = _productBusiness.GetPromoCodeAlertMessage(request.ContinueToRegisterAncillary);
                MOBExceptionWrapper exceptionWrapper = new MOBExceptionWrapper(ex);
                
                if (!_configuration.GetValue<bool>("SurfaceErrorToClient"))
                {
                    if ((request.CartKey != null && request.CartKey.Equals("ProductBundles")) || (request.CartKey != null && request.CartKey.Equals("OTP")))
                    {

                        // [mobile:6414]iOS : When we get exception message in bundle screen after dismissing its going to OLD non CFOP Flow
                        shoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, new MOBShoppingCart().ObjectName, new List<string> { request.SessionId, new MOBShoppingCart().ObjectName }).ConfigureAwait(false);
                        shoppingCart.Flow = request.Flow;
                        response.ShoppingCart = shoppingCart;
                        response.Exception = new MOBException("9999", _configuration.GetValue<string>("EPlusRegisterOfferErrormsg"));
                    }
                    else
                    {
                        response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                    }
                }
                else
                {
                    response.Exception = new MOBException("9999", ex.Message + " :: " + ex.StackTrace);
                }
                if (response.Exception != null && _configuration.GetValue<bool>("EnableOmniCartMVP2Changes"))
                {
                    if (!request.IsContinue)
                    {
                        response.Exception.Message = _configuration.GetValue<string>("RegisterBundleGenericErrorMessage");
                    }
                }

                _logger.LogError("RegisterOffersForBooking Exception {@Exception} ", JsonConvert.SerializeObject(exceptionWrapper));
                
            }

            //assigning bundle response null when the count is zero to match onPrem logic
            if (request.MerchandizingOfferDetails.Count == 0)
                response.BundleResponse = null;

            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("RegisterOffersForBooking {@ClientResponse} ", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("Product/RegisterOffersForReshop")]
        public async Task<MOBRESHOPRegisterOfferResponse> RegisterOffersForReshop(MOBRegisterOfferRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);

            var response = new MOBRESHOPRegisterOfferResponse();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("RegisterOffersForReshop {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for RegisterOffersForReshop business call", request.SessionId))
                {
                    response = await _productBusiness.RegisterOffersForReshop(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("RegisterOffersForReshop UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException();
                response.Exception.Message = uaex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("RegisterOffersForReshop Error {@Exception}", JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("RegisterOffersForReshop {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("Product/RegisterSeatsForBooking")]
        public async Task<MOBBookingRegisterSeatsResponse> RegisterSeatsForBooking(MOBRegisterSeatsRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);

            var response = new MOBBookingRegisterSeatsResponse();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("RegisterSeatsForBooking {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for RegisterSeatsForBooking business call", request.SessionId))
                {
                    response = await _productBusiness.RegisterSeatsForBooking(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("RegisterSeatsForBooking UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException();
                response.Exception.Message = uaex.Message;
                if (_configuration.GetValue<bool>("EnableInhibitBooking"))
                {
                    if (uaex.InnerException != null && !string.IsNullOrEmpty(uaex.InnerException.Message.Trim()) && uaex.InnerException.Message.Trim().Equals("10050"))
                    {
                        response.Exception.Code = "10050";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("RegisterSeatsForBooking Exception {@Exception} ", JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));

                if (!_configuration.GetValue<bool>("SurfaceErrorToClient"))
                {
                    response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
                else
                {
                    response.Exception = new MOBException("9999", ex.Message);
                }
                if (response.Exception != null && _configuration.GetValue<bool>("EnableOmniCartMVP2Changes"))
                {
                    if (!request.IsDone)
                    {
                        response.Exception.Message = _configuration.GetValue<string>("RegisterSeatGenericErrorMessage");
                    }
                }
            }

            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("RegisterSeatsForBooking {@ClientResponse}", JsonConvert.SerializeObject(response));

            //var options = new Newtonsoft.Json.JsonSerializerSettings();

            //options.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            //return new JsonResult(response, options)
            //{
            //    StatusCode = (int)HttpStatusCode.OK
            //};

            return response;
        }

        [HttpPost]
        [Route("Product/RegisterSeatsForReshop")]
        public async Task<MOBReshopRegisterSeatsResponse> RegisterSeatsForReshop(MOBRegisterSeatsRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);

            var response = new MOBReshopRegisterSeatsResponse();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("RegisterSeatsForReshop {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for RegisterSeatsForBooking business call", request.SessionId))
                {
                    response = await _productBusiness.RegisterSeatsForReshop(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("RegisterSeatsForReshop UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException();
                response.Exception.Message = uaex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("RegisterSeatsForReshop Error {@Exception}", JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("RegisterSeatsForReshop {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("Product/ClearCartOnBackNavigation")]
        public async Task<MOBBookingRegisterOfferResponse> ClearCartOnBackNavigation(MOBClearCartOnBackNavigationRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);

            var response = new MOBBookingRegisterOfferResponse();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("ClearCartOnBackNavigation {@ClientRequest}", JsonConvert.SerializeObject(request));

                using (timer = _logger.BeginTimedOperation("Total time taken for ClearCartOnBackNavigation business call", request.SessionId))
                {
                    response = await _productBusiness.ClearCartOnBackNavigation(request);
                }
                    
            }
            catch (System.Net.WebException wex)
            {
                _logger.LogError("ClearCartOnBackNavigation WebException {@WebException}", JsonConvert.SerializeObject(wex));
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("ClearCartOnBackNavigation UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException();
                response.Exception.Message = uaex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("ClearCartOnBackNavigation Error {@Exception}", JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }

            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("ClearCartOnBackNavigation {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("Product/RemoveAncillaryOffer")]
        public async Task<MOBBookingRegisterOfferResponse> RemoveAncillaryOffer(MOBRemoveAncillaryOfferRequest request)
        {
            var response = new MOBBookingRegisterOfferResponse();
            IDisposable timer = null;
            try
            {
                await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);
                // Add Request Enrichers that would be propagated to all the logs within the same service
                _applicationEnricher.Add(Constants.TransactionIdText, request.TransactionId);
                _applicationEnricher.Add(Constants.ApplicationIdText, request.Application.Id);
                _applicationEnricher.Add(Constants.ApplicationVersionText, request.Application.Version.Major);
                _applicationEnricher.Add(Constants.DeviceIdText, request.DeviceId);

                _logger.LogInformation("RemoveAncillaryOffer {@ClientRequest} ", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for RemoveAncillaryOffer business call", request.SessionId))
                {
                    response = await _productBusiness.RemoveAncillaryOffer(request);
                }
            }
            catch (System.Net.WebException wex)
            {
                _logger.LogError("RemoveAncillaryOffer WebException {@WebException}", JsonConvert.SerializeObject(wex));

                response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                response.Exception.ErrMessage = wex.Message;
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("RemoveAncillaryOffer UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                if (uaex != null && !string.IsNullOrEmpty(uaex.Message))
                {
                    response.Exception.Message = uaex.Message;
                }
                else
                {
                    response.Exception.Message = _configuration.GetValue<string>("Booking2OGenericExceptionMessage");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("RemoveAncillaryOffer Error {@Exception}", JsonConvert.SerializeObject(ex));
                if (!_configuration.GetValue<bool>("SurfaceErrorToClient"))
                {
                    response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
                else
                {
                    response.Exception = new MOBException("9999", ex.Message);
                }
            }

            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }
            _logger.LogInformation("RemoveAncillaryOffer {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("Product/RegisterOffersForOmniCartSavedTrip")]
        public async Task<MOBSHOPSelectTripResponse> RegisterOffersForOmniCartSavedTrip(MOBSHOPUnfinishedBookingRequestBase request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);

            var response = new MOBSHOPSelectTripResponse();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("RegisterOffersForOmniCartSavedTrips {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for RegisterOffersForOmniCartSavedTrip business call", request.SessionId))
                {
                    response = await _productBusiness.RegisterOffersForOmniCartSavedTrip(request);
                }
            }
            catch (System.Net.WebException wex)
            {
                _logger.LogError("RegisterOffersForOmniCartSavedTrip WebException {@WebException}", JsonConvert.SerializeObject(wex));

                response.Exception = new MOBException("9999", _configuration.GetValue<string>("OmnicartExceptionMessage") ?? _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                response.Exception.ErrMessage = wex.Message;
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("RegisterOffersForOmniCartSavedTrip UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException();
                if (uaex != null && !string.IsNullOrEmpty(uaex.Message))
                {
                    response.Exception.Message = uaex.Message;
                }
                else
                {
                    response.Exception.Message = _configuration.GetValue<string>("Booking2OGenericExceptionMessage");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("RegisterOffersForOmniCartSavedTrip Error {@Exception}", JsonConvert.SerializeObject(ex));
                if (!_configuration.GetValue<bool>("SurfaceErrorToClient"))
                {
                    response.Exception = new MOBException("9999", _configuration.GetValue<string>("OmnicartExceptionMessage") ?? _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
                else
                {
                    response.Exception = new MOBException("9999", ex.Message);
                }
            }

            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("RegisterOffersForOmniCartSavedTrip {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("Product/UnRegisterAncillaryOffersForBooking")]
        public async Task<MOBBookingRegisterOfferResponse> UnRegisterAncillaryOffersForBooking(MOBRegisterOfferRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);

            var response = new MOBBookingRegisterOfferResponse();

            IDisposable timer = null;
            try
            {
                _logger.LogInformation("UnRegisterAncillaryOffersForBooking {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for UnRegisterAncillaryOffersForBooking business call", request.SessionId))
                {
                    response = await _productBusiness.UnRegisterAncillaryOffersForBooking(request);
                }
            }
            catch (System.Net.WebException wex)
            {
                _logger.LogError("UnRegisterAncillaryOffersForBooking WebException {@WebException}", JsonConvert.SerializeObject(wex));
                response.Exception = new MOBException();
                response.Exception.Message = wex.Message;
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("UnRegisterAncillaryOffersForBooking UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException();
                response.Exception.Message = uaex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("UnRegisterAncillaryOffersForBooking Error {@Exception}", JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException();
                response.Exception.Message = ex.Message;
            }
            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("UnRegisterAncillaryOffersForBooking {@ClientResponse}", JsonConvert.SerializeObject(response));
            //var options = new JsonSerializerSettings();
            //options.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            //options.ContractResolver = new CamelCasePropertyNamesContractResolver();
            //return new JsonResult(response, options)
            //{
            //    StatusCode = (int)HttpStatusCode.OK
            //};
            return response;
        }

        [HttpPost]
        [Route("Product/CreateShoppingSession")]
        public async Task<MOBCreateShoppingSessionResponse> CreateShoppingSession(MOBCreateShoppingSessionRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, string.Empty);

            var response = new MOBCreateShoppingSessionResponse();

            IDisposable timer = null;
            try
            {
                _logger.LogInformation("CreateShoppingSession {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for CreateShoppingSession business call", string.Empty))
                {
                    response = await _productBusiness.CreateShoppingSession(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogError("CreateShoppingSession UnitedException {@UnitedException} and {exceptionstack}", uaex.Message, JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException("9999", !string.IsNullOrEmpty(uaex?.Message) ? uaex.Message : _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));

            }
            catch (Exception ex)
            {
                _logger.LogError("CreateShoppingSession Error {@Exception} and {exceptionstack}", ex.Message, JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));

            }
            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("CreateShoppingSession {@ClientResponse}", JsonConvert.SerializeObject(response));
            return response;

        }

        [HttpPost]
        [Route("Product/PopulateMerchOffers")]
        public async Task<MOBResponse> PopulateMerchOffers(MOBPopulateMerchOffersRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, string.Empty);

            var response = new MOBResponse();

            IDisposable timer = null;
            try
            {
                _logger.LogInformation("PopulateMerchOffers {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for PopulateMerchOffers business call", string.Empty))
                {
                    response = await _productBusiness.PopulateMerchOffers(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogError("PopulateMerchOffers UnitedException {@UnitedException} and {exceptionstack}", uaex.Message, JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException("9999", !string.IsNullOrEmpty(uaex?.Message) ? uaex.Message : _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));

            }
            catch (Exception ex)
            {
                _logger.LogError("PopulateMerchOffers Error {@Exception} and {exceptionstack}", ex.Message, JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));

            }
            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("PopulateMerchOffers {@ClientResponse}", JsonConvert.SerializeObject(response));
            return response;

        }

        [HttpPost]
        [Route("Product/UpdateReservation")]
        public async Task<MOBResponse> UpdateReservation(MOBUpdateReservationRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, string.Empty);

            var response = new MOBResponse();

            IDisposable timer = null;
            try
            {
                _logger.LogInformation("UpdateReservation {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for UpdateReservation business call", string.Empty))
                {
                    response = await _productBusiness.UpdateReservation(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogError("UpdateReservation UnitedException {@UnitedException} and {exceptionstack}", uaex.Message, JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException("9999", !string.IsNullOrEmpty(uaex?.Message) ? uaex.Message : _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));

            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateReservation Error {@Exception} and {exceptionstack}", ex.Message, JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));

            }
            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("UpdateReservation {@ClientResponse}", JsonConvert.SerializeObject(response));
            return response;

        }

        [HttpGet("GetFeatureSettings")]
        public GetFeatureSettingsResponse GetFeatureSettings()
        {
            GetFeatureSettingsResponse response = new GetFeatureSettingsResponse();
            try
            {
                response = _featureSettings.GetFeatureSettings();
            }
            catch (Exception ex)
            {
                _logger.LogError("GetFeatureSettings Error {exceptionstack} and {transactionId}", JsonConvert.SerializeObject(ex), "GetFeatureSettings_TransId");
                response.Exception = new MOBException("9999", JsonConvert.SerializeObject(ex));
            }
            return response;
        }
        [HttpPost("RefreshFeatureSettingCache")]
        public async Task<MOBResponse> RefreshFeatureSettingCache(MOBFeatureSettingsCacheRequest request)
        {
            MOBResponse response = new MOBResponse();
            try
            {
                request.ServiceName = ServiceNames.PRODUCT.ToString();
                await _featureSettings.RefreshFeatureSettingCache(request);
            }
            catch (Exception ex)
            {
                _logger.LogError("RefreshFeatureSettingCache Error {exceptionstack} and {transactionId}", JsonConvert.SerializeObject(ex), "RefreshRetrieveAllFeatureSettings_TransId");
                response.Exception = new MOBException("9999", JsonConvert.SerializeObject(ex));
            }
            return response;
        }
    }

}
