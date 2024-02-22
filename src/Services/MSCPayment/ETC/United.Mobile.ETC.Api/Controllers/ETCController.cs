using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Definition;
using United.Definition.FormofPayment;
using United.Mobile.ETC.Business;
using United.Mobile.Model.Common;
using United.Mobile.Model.MSC.FormofPayment;
using United.Utility.Helper;
using United.Utility.Serilog;

namespace United.Mobile.ETC.Api.Controllers
{
    [Route("etcservice/api")]
    [ApiController]
    public class ETCController : ControllerBase
    {
        private readonly ICacheLog<ETCController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHeaders _headers;
        private readonly IETCBusiness _eTCBusiness;
        private readonly IFeatureSettings _featureSettings;
        public ETCController(ICacheLog<ETCController> logger
            , IConfiguration configuration
            , IHeaders headers
            , IETCBusiness eTCBusiness
            , IFeatureSettings featureSettings)
        {
            _logger = logger;
            _configuration = configuration;
            _headers = headers;
            _eTCBusiness = eTCBusiness;
            _featureSettings = featureSettings;
        }

        [HttpGet]
        [Route("HealthCheck")]
        public string HealthCheck()
        {
            return "Healthy";
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
        [Route("Payment/TravelerCertificate")]
        public async Task<MOBFOPTravelerCertificateResponse> TravelerCertificate(MOBFOPTravelerCertificateRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);

            var response = new MOBFOPTravelerCertificateResponse();
            IDisposable timer = null;
            try
            {
                using (timer = _logger.BeginTimedOperation("Total time taken for TravelerCertificate business call", request.SessionId))
                {
                    response = await _eTCBusiness.TravelerCertificate(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("TravelerCertificate UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException();
                response.Exception.Message = uaex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("TravelerCertificate Error {@Exception}", JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("TravelerCertificate {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("Payment/PersistFOPBillingContactInfo_ETC")]
        public async Task<MOBFOPTravelerCertificateResponse> PersistFOPBillingContactInfo_ETC(MOBFOPBillingContactInfoRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);

            var response = new MOBFOPTravelerCertificateResponse();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("PersistFOPBillingContactInfo_ETC {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for PersistFOPBillingContactInfo_ETC business call", request.SessionId))
                {
                    response = await _eTCBusiness.PersistFOPBillingContactInfo_ETC(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("PersistFOPBillingContactInfo_ETC UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException();
                response.Exception.Message = uaex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("PersistFOPBillingContactInfo_ETC Error {@Exception}", JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("PersistFOPBillingContactInfo_ETC {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("Payment/applymileagepricing")]
        public async Task<MOBFOPTravelerCertificateResponse> ApplyMileagePricing(MOBRTIMileagePricingRequest request)
        {

            MOBFOPTravelerCertificateResponse response = new MOBFOPTravelerCertificateResponse();
            try
            {
                await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);

                _logger.LogInformation("ApplyMileagePricing {@ClientRequest}", JsonConvert.SerializeObject(request));

                response = await _eTCBusiness.ApplyMileagePricing(request);
            }

            #region 
            catch (MOBUnitedException uaex)
            {
                #region
                MOBExceptionWrapper uaexWrapper = new MOBExceptionWrapper(uaex);
                _logger.LogWarning("ApplyMileagePricinghub Error {@UnitedException}", JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException();
                response.Exception.Message = uaex.Message;
                if (_configuration.GetValue<bool>("SessionExpiryMessageRedirectToHomerBookingMain"))
                {
                    response.Exception.Code = uaex.Code;
                }

                #endregion
            }
            catch (System.Exception ex)
            {
                string[] messages = ex.Message.Split('#');

                MOBExceptionWrapper exceptionWrapper = new MOBExceptionWrapper(ex);
                exceptionWrapper.Message = messages[0];
                _logger.LogError("ApplyMileagePricing Error {@Exception}", JsonConvert.SerializeObject(exceptionWrapper));

                if (!Convert.ToBoolean(_configuration.GetValue<string>("SurfaceErrorToClient")))
                {
                    response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
                else
                {
                    response.Exception = new MOBException("9999", messages[0]);
                }
            }
            #endregion

            var stringCount = JsonConvert.SerializeObject(response).Length;
            _logger.LogInformation("ApplyMileagePricing {@ClientResponse}", JsonConvert.SerializeObject(response));

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
                request.ServiceName = ServiceNames.ETC.ToString();
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
