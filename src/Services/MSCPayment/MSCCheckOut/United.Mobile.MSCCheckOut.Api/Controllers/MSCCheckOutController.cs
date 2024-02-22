using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Definition;
using United.Definition.Shopping;
using United.Mobile.DataAccess.Common;
using United.Mobile.Model.Common;
using United.Mobile.MSCCheckOut.Domain;
using United.Persist.Definition.Shopping;
using United.Utility.Helper;
using United.Utility.Serilog;

namespace United.Mobile.MSCCheckOut.Api.Controllers
{
    [Route("msccheckoutservice/api")]
    [ApiController]
    public class MSCCheckOutController : ControllerBase
    {
        private readonly ICacheLog<MSCCheckOutController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHeaders _headers;
        private readonly IMSCCheckOutBusiness _mSCCheckOutBusiness;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly ICachingService _cachingService;
        private readonly IFeatureSettings _featureSettings; 
        public MSCCheckOutController(ICacheLog<MSCCheckOutController> logger
            , IConfiguration configuration
            , IHeaders headers
            , IMSCCheckOutBusiness mSCCheckOutBusiness
            , ISessionHelperService sessionHelperService
            , ICachingService cachingService
            , IFeatureSettings featureSettings)
        {
            _logger = logger;
            _configuration = configuration;
            _headers = headers;
            _mSCCheckOutBusiness = mSCCheckOutBusiness;
            _sessionHelperService = sessionHelperService;
            _cachingService = cachingService;
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
        [Route("Payment/CheckOut")]
        public async Task<MOBCheckOutResponse> CheckOut(MOBCheckOutRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);

            var response = new MOBCheckOutResponse();

            //IDisposable timer = null;

            try
            {
                if (!_configuration.GetValue<bool>("EnableRemoveTaxIdInformation") && request?.TaxIdInformation != null)
                {
                    #region Remove ID Number from tax Id Information for security purposes
                    MOBCheckOutRequest clonedRequest = request.Clone();
                    ConfigUtility.RemoveIDNumberInTaxIdInformation(clonedRequest?.TaxIdInformation);
                    #endregion
                    _logger.LogInformation("CheckOut {@ClientRequest}", JsonConvert.SerializeObject(clonedRequest));
                }
                else 
                {
                    _logger.LogInformation("CheckOut {@ClientRequest}", JsonConvert.SerializeObject(request));
                }

                response = await _mSCCheckOutBusiness.CheckOut(request);
            }
            catch (MOBUnitedException uaex)
            {
                response.Exception = response.Exception == null ? new MOBException() : response.Exception;
                if (uaex.Message == "90518" || uaex.Message == "90510")
                {
                    response.Exception.Message = "There was a problem completing your reservation";
                    response.Exception.Code = "RESHOPCHANGEERROR";
                    response.ResponseStatusItem = new MOBSHOPResponseStatusItem();
                    response.Reservation = new MOBSHOPReservation(_configuration, _cachingService);

                    Reservation reservation = new Reservation();
                     reservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, reservation.ObjectName, new List<string> { request.SessionId, reservation.ObjectName }).ConfigureAwait(false);
                    response.Reservation.Reshop = reservation.Reshop;

                    if (uaex.Message == "90518")
                    {
                        response.ResponseStatusItem.Status = MOBSHOPResponseStatus.ReshopChangePending;
                        response.ResponseStatusItem.StatusMessages = await _mSCCheckOutBusiness.GetMPPINPWDTitleMessages("RTI_CHANGE_FLIGHT_PENDING_ERROR_MESSAGE");
                    }
                    else if (uaex.Message == "90510")
                    {
                        response.ResponseStatusItem.Status = MOBSHOPResponseStatus.ReshopUnableToComplete;
                        response.ResponseStatusItem.StatusMessages = await _mSCCheckOutBusiness.GetMPPINPWDTitleMessages("RTI_CHANGE_FLIGHT_UNABLE_COMPLETE_REQUEST_MESSAGE");
                    }
                }
                else
                    response.Exception.Message = uaex.Message;

                response.Exception.ErrMessage = $"{uaex.InnerException?.Message} - {uaex.StackTrace}";
                _logger.LogWarning("CheckOut UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));

            }
            catch (Exception ex)
            {
                _logger.LogError("CheckOut Error {@Exception}", JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = 0;

            //if (timer != null)
            //{
            //    response.CallDuration = ((TimedOperation)timer).GetElapseTime();
            //    timer.Dispose();
            //}

            response.TransactionId = request.TransactionId;
            response.LanguageCode = request.LanguageCode;
            response.SessionId = request.SessionId;
            response.CheckinSessionId = request.CheckinSessionId;
            response.Flow = request.Flow;
            _logger.LogInformation("CheckOut {@ClientResponse}", JsonConvert.SerializeObject(response));
           
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
                request.ServiceName = ServiceNames.MSCCHECKOUT.ToString();
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
