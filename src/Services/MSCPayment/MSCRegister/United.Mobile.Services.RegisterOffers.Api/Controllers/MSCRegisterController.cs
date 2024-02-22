using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Definition;
using United.Mobile.Model.Common;
using United.Mobile.Services.RegisterOffers.Domain;
using United.Utility.Helper;
using United.Utility.Serilog;

namespace United.Mobile.Services.RegisterOffers.Api.Controllers
{
    [Route("mscregisterservice/api")]
    [ApiController]
    public class MSCRegisterController : ControllerBase
    {
        private readonly ICacheLog<MSCRegisterController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHeaders _headers;
        private readonly IMSCRegisterBusiness _mscRegisterBusiness;
        private readonly IFeatureSettings _featureSettings;
        public MSCRegisterController(ICacheLog<MSCRegisterController> logger
            , IConfiguration configuration
            , IHeaders headers
            , IMSCRegisterBusiness mscRegisterBusiness
            , IFeatureSettings featureSettings)
        {
            _logger = logger;
            _configuration = configuration;
            _headers = headers;
            _mscRegisterBusiness = mscRegisterBusiness;
            _featureSettings = featureSettings;
        }

        [HttpGet]
        [Route("HealthCheck")]
        public string HealthCheck()
        {
            return "Healthy";
        }

        /// <summary>
        /// HTTP GET
        /// Returns a version number of the service.
        /// </summary>
        /// <returns>
        /// Version Number of the Service.
        /// </returns>

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
        [Route("Payment/RegisterOffers")]
        public async Task<MOBRegisterOfferResponse> RegisterOffers(MOBRegisterOfferRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);
            var response = new MOBRegisterOfferResponse();
            IDisposable timer = null;

            try
            {
                _logger.LogInformation("RegisterOffers {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for RegisterOffers business call", transationId: request.TransactionId))
                {
                    response = await _mscRegisterBusiness.RegisterOffers(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("RegisterOffers UnitedException {exception} and {exceptionstack}", uaex.Message, JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException("9999", !string.IsNullOrEmpty(uaex?.Message) ? uaex.Message : _configuration.GetValue<string>("GenericExceptionMessage"));

            }
            catch (System.Exception ex)
            {
                _logger.LogError("RegisterOffers Error {exception} and {exceptionstack}", ex.Message, JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("RegisterOffers {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }


        [HttpPost]
        [Route("Payment/RegisterUsingMiles")]
        public async Task<MOBRegisterOfferResponse> RegisterUsingMiles(MOBRegisterUsingMilesRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);
            var response = new MOBRegisterOfferResponse();
            IDisposable timer = null;

            try
            {
                _logger.LogInformation("RegisterUsingMiles {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for RegisterUsingMiles business call", transationId: request.TransactionId))
                {
                    response = await _mscRegisterBusiness.RegisterUsingMiles(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("RegisterUsingMiles UnitedException {exception} and {exceptionstack}", uaex.Message, JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException("9999", !string.IsNullOrEmpty(uaex?.Message) ? uaex.Message : _configuration.GetValue<string>("GenericExceptionMessage"));

            }
            catch (System.Exception ex)
            {
                _logger.LogError("RegisterUsingMiles Error {exception} and {exceptionstack}", ex.Message, JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("RegisterUsingMiles {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("Payment/RegisterBags")]
        public async Task<MOBRegisterOfferResponse> RegisterBags(MOBRegisterBagsRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);
            var response = new MOBRegisterOfferResponse();
            IDisposable timer = null;

            try
            {
                _logger.LogInformation("RegisterBags {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for RegisterBags business call", transationId: request.TransactionId))
                {
                    response = await _mscRegisterBusiness.RegisterBags(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogError("RegisterBags UnitedException {exception} and {exceptionstack}", uaex.Message, JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException("9999", !string.IsNullOrEmpty(uaex?.Message) ? uaex.Message : _configuration.GetValue<string>("GenericExceptionMessage"));

            }
            catch (System.Exception ex)
            {
                _logger.LogError("RegisterBags Error {exception} and {exceptionstack}", ex.Message, JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("RegisterBags {@ClientResponse} ", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("Payment/RegisterBagDetails")]
        public async Task<MOBRegisterOfferResponse> RegisterBagDetails(MOBRegisterBagDetailsRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);
            var response = new MOBRegisterOfferResponse();
            IDisposable timer = null;

            try
            {
                _logger.LogInformation("RegisterBagDetails {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for RegisterBagDetails business call", transationId: request.TransactionId))
                {
                    response = await _mscRegisterBusiness.RegisterBagDetails(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogError("RegisterBagDetails UnitedException {exception} and {exceptionstack}", uaex.Message, JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException("9999", !string.IsNullOrEmpty(uaex?.Message) ? uaex.Message : _configuration.GetValue<string>("GenericExceptionMessage"));

            }
            catch (System.Exception ex)
            {
                _logger.LogError("RegisterBagDetails Error {exception} and {exceptionstack}", ex.Message, JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("RegisterBags {@ClientResponse} ", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("Payment/RegisterSameDayChange")]
        public async Task<MOBRegisterOfferResponse> RegisterSameDayChange(MOBRegisterSameDayChangeRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);
            var response = new MOBRegisterOfferResponse();
            IDisposable timer = null;

            try
            {
                _logger.LogInformation("RegisterSameDayChange {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for RegisterSameDayChange business call", transationId: request.TransactionId))
                {
                    response = await _mscRegisterBusiness.RegisterSameDayChange(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogError("RegisterSameDayChange UnitedException {exception} and {exceptionstack}", uaex.Message, JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException("9999", !string.IsNullOrEmpty(uaex?.Message) ? uaex.Message : _configuration.GetValue<string>("GenericExceptionMessage"));
            }
            catch (System.Exception ex)
            {
                _logger.LogError("RegisterSameDayChange Error {exception} and {exceptionstack}", ex.Message, JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("RegisterSameDayChange {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("Payment/RegisterCheckinSeats")]
        public async Task<MOBRegisterOfferResponse> RegisterCheckinSeats(MOBRegisterCheckinSeatsRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);
            var response = new MOBRegisterOfferResponse();
            IDisposable timer = null;

            try
            {
                _logger.LogInformation("RegisterCheckinSeats {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for RegisterCheckinSeats business call", transationId: request.TransactionId))
                {
                    response = await _mscRegisterBusiness.RegisterCheckinSeats(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogError("RegisterCheckinSeats UnitedException {exception} and {exceptionstack}", uaex.Message, JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException("9999", !string.IsNullOrEmpty(uaex?.Message) ? uaex.Message : _configuration.GetValue<string>("GenericExceptionMessage"));
            }
            catch (System.Exception ex)
            {
                _logger.LogError("RegisterCheckinSeats Error {exception} and {exceptionstack}", ex.Message, JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("RegisterCheckinSeats {@ClientResponse} ", JsonConvert.SerializeObject(response));

            return response;
        }
        [HttpPost]
        [Route("SeatMap/RegisterSeats")]
        public async Task<MOBRegisterSeatsResponse> RegisterSeats(MOBRegisterSeatsRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);
            var response = new MOBRegisterSeatsResponse();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("RegisterSeats {@clientRequest} {DeviceId} {SessionId} {TransactionId}", JsonConvert.SerializeObject(request), request.DeviceId, request.SessionId, request.TransactionId);

                timer = _logger.BeginTimedOperation("Total time taken for RegisterSeats business call", transationId: request.TransactionId);
                response = await _mscRegisterBusiness.RegisterSeats(request);
            }
            catch (MOBUnitedException coex)
            {
                _logger.LogWarning("RegisterSeats Error {exception} {exceptionstack} {SessionId} and {TransactionId}", coex.Message, JsonConvert.SerializeObject(coex), request.SessionId, request.TransactionId);

                response.Exception = new MOBException();
                response.Exception.Message = coex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("RegisterSeats Error {exception} {exceptionstack} {SessionId} and {TransactionId}", ex.Message, JsonConvert.SerializeObject(ex), request.SessionId, request.TransactionId);

                response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("RegisterSeats {@clientResponse} {SessionId} {transactionId}", JsonConvert.SerializeObject(response), request.SessionId, request.TransactionId);

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
                request.ServiceName = ServiceNames.MSCREGISTER.ToString();
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
