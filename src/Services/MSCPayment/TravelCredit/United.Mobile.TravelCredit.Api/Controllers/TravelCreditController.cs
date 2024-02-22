using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Definition;
using United.Definition.FormofPayment;
using United.Mobile.Model.Common;
using United.Mobile.TravelCredit.Domain;
using United.Utility.Helper;
using United.Utility.Serilog;

namespace United.Mobile.TravelCredit.Api.Controllers
{
    [Route("travelcreditservice/api/")]
    [ApiController]
    public class TravelCreditController : ControllerBase
    {
        private readonly ICacheLog<TravelCreditController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHeaders _headers;
        private readonly ITravelCreditBusiness _travelCreditBusiness;
        private readonly IFeatureSettings _featureSettings;
        public TravelCreditController(ICacheLog<TravelCreditController> logger
            , IConfiguration configuration
            , IHeaders headers
            , ITravelCreditBusiness travelCreditBusiness
            , IFeatureSettings featureSettings)
        {
            _logger = logger;
            _configuration = configuration;
            _headers = headers;
            _travelCreditBusiness = travelCreditBusiness;
            _featureSettings = featureSettings;
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
        [Route("Payment/FutureFlightCredit")]
        public async Task<MOBFutureFlightCreditResponse> FutureFlightCredit(MOBFutureFlightCreditRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);

            var response = new MOBFutureFlightCreditResponse();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("FutureFlightCredit {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for FutureFlightCredit business call", request.SessionId))
                {

                    response = await _travelCreditBusiness.FutureFlightCredit(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                if (uaex.Code == "90101")
                {
                    if (response.Captions == null)
                    {
                        response.Captions = new List<MOBTypeOption>();
                    }
                    response.Captions.Add(new MOBTypeOption { Key = "90101_ErrPopup_Learnmore_Btntxt", Value = _configuration.GetValue<string>("FFC_90101_ErrorPopup_Learnmore_Btntxt") });
                    response.Captions.Add(new MOBTypeOption { Key = "90101_ErrPopup_Learnmore_RedirectUrl", Value = _configuration.GetValue<string>("FFC_90101_ErrorPopup_Learnmore_RedirectUrl") });
                }

                _logger.LogWarning("FutureFlightCredit UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException();
                response.Exception.Code = (string.IsNullOrEmpty(uaex.Code)) ? "9999" : uaex.Code.ToString();
                response.Exception.ErrMessage = $"{uaex.InnerException?.Message} - {uaex.StackTrace}";
                response.Exception.Message = uaex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("FutureFlightCredit Error {@Exception}", JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("FutureFlightCredit {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("Payment/FindFutureFlightCreditByEmail")]
        public async Task<MOBFutureFlightCreditResponse> FindFutureFlightCreditByEmail(MOBFutureFlightCreditRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);

            var response = new MOBFutureFlightCreditResponse();
            IDisposable timer = null;
            try
            {
                if (string.IsNullOrEmpty(request.Email?.Trim()))
                {
                    _logger.LogWarning("FindFutureFlightCreditByEmail UnitedException {@UnitedException}", "Email is required");
                    response.Exception = new MOBException("400", "Email is required");
                }
                else
                {
                    _logger.LogInformation("FindFutureFlightCreditByEmail {@ClientRequest}", JsonConvert.SerializeObject(request));
                    using (timer = _logger.BeginTimedOperation("Total time taken for FindFutureFlightCreditByEmail business call", request.SessionId))
                    {
                        response = await _travelCreditBusiness.FindFutureFlightCreditByEmail(request);
                    }
                }
            }
            catch (MOBUnitedException uaex)
            {
                if (uaex.Code == "90101")
                {
                    if (response.Captions == null)
                    {
                        response.Captions = new List<MOBTypeOption>();
                    }
                    response.Captions.Add(new MOBTypeOption { Key = "90101_ErrPopup_Learnmore_Btntxt", Value = _configuration.GetValue<string>("FFC_90101_ErrorPopup_Learnmore_Btntxt") });
                    response.Captions.Add(new MOBTypeOption { Key = "90101_ErrPopup_Learnmore_RedirectUrl", Value = _configuration.GetValue<string>("FFC_90101_ErrorPopup_Learnmore_RedirectUrl") });
                }

                _logger.LogWarning("FindFutureFlightCreditByEmail UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException();
                response.Exception.Code = (string.IsNullOrEmpty(uaex.Code)) ? "9999" : uaex.Code.ToString();
                response.Exception.ErrMessage = $"{uaex.InnerException?.Message} - {uaex.StackTrace}";
                response.Exception.Message = uaex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("FindFutureFlightCreditByEmail Error {@Exception}", JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("FindFutureFlightCreditByEmail {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("Payment/RemoveFutureFlightCredit")]
        public async Task<MOBFutureFlightCreditResponse> RemoveFutureFlightCredit(MOBFOPFutureFlightCreditRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);

            var response = new MOBFutureFlightCreditResponse();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("RemoveFutureFlightCredit {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for RemoveFutureFlightCredit business call", request.SessionId))
                {

                    response = await _travelCreditBusiness.RemoveFutureFlightCredit(request);
                }
            }
            catch (MOBUnitedException uaex)
            {

                _logger.LogWarning("RemoveFutureFlightCredit UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException();
                response.Exception.Message = uaex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("RemoveFutureFlightCredit Error {@Exception}", JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("RemoveFutureFlightCredit {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("Payment/LookUpTravelCredit")]
        public async Task<MOBFOPResponse> LookUpTravelCredit(MOBFOPLookUpTravelCreditRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);

            var response = new MOBFOPResponse();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("LookUpTravelCredit {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for LookUpTravelCredit business call", request.SessionId))
                {
                    response = await _travelCreditBusiness.LookUpTravelCredit(request);
                }
            }
            catch (MOBUnitedException uaex)
            {

                _logger.LogWarning("LookUpTravelCredit UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException();
                response.Exception.Message = uaex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("LookUpTravelCredit Error {@Exception}", JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("LookUpTravelCredit {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("Payment/ManageTravelCredit")]
        public async Task<MOBFOPResponse> ManageTravelCredit(MOBFOPManageTravelCreditRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);

            var response = new MOBFOPResponse();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("ManageTravelCredit {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for ManageTravelCredit business call", request.SessionId))
                {

                    response = await _travelCreditBusiness.ManageTravelCredit(request);
                }
            }
            catch (MOBUnitedException uaex)
            {

                _logger.LogWarning("ManageTravelCredit UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException();
                response.Exception.Message = uaex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("ManageTravelCredit Error {@Exception}", JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("ManageTravelCredit {@ClientResponse}", JsonConvert.SerializeObject(response));

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
                request.ServiceName = ServiceNames.TRAVELCREDIT.ToString();
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