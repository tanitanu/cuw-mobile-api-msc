using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Definition;
using United.Definition.Shopping;
using United.Mobile.Model.Common;
using United.Mobile.Payment.Domain;
using United.Utility.Helper;
using United.Utility.Serilog;


namespace United.Mobile.Payment.Api.Controllers
{
    [Route("mscpaymentservice/api")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ICacheLog<PaymentController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHeaders _headers;
        private readonly IPaymentBusiness _paymentBusiness;
        private readonly IFeatureSettings _featureSettings;
        public PaymentController(ICacheLog<PaymentController> logger
            , IConfiguration configuration
            , IHeaders headers
            , IPaymentBusiness paymentBusiness
            , IFeatureSettings featureSettings)
        {
            _logger = logger;
            _configuration = configuration;
            _headers = headers;
            _paymentBusiness = paymentBusiness;
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
        [Route("Payment/GetPaymentToken")]
        public async Task<MOBFOPAcquirePaymentTokenResponse> GetPaymentToken(MOBFOPAcquirePaymentTokenRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);

            var response = new MOBFOPAcquirePaymentTokenResponse();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("GetPaymentToken {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for GetPaymentToken business call", transationId: request.TransactionId))
                {
                    response = await _paymentBusiness.GetPaymentToken(request);
                }

            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("GetPaymentToken UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException();
                response.Exception.Message = uaex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("GetPaymentToken Error {@Exception}", JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = 0;
            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }
            _logger.LogInformation("GetPaymentToken {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("Payment/PersistFormofPaymentDetails")]
        public async Task<MOBPersistFormofPaymentResponse> PersistFormofPaymentDetails(MOBPersistFormofPaymentRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);

            var response = new MOBPersistFormofPaymentResponse();
            IDisposable timer = null;
            try
            {
                #region Remove Encrypted CardNumber from request while logging (Change from Security Complaince team)
                MOBPersistFormofPaymentRequest clonedRequest = request.Clone();
                ConfigUtility.RemoveEncyptedCreditcardNumber(clonedRequest?.FormofPaymentDetails?.Uplift);
                #endregion
                _logger.LogInformation("PersistFormofPaymentDetails {@ClientRequest}", JsonConvert.SerializeObject(clonedRequest));
                using (timer = _logger.BeginTimedOperation("Total time taken for PersistFormofPaymentDetails business call", transationId: request.TransactionId))
                {
                    response = await _paymentBusiness.PersistFormofPaymentDetails(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("PersistFormofPaymentDetails UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException("9999", "");
                response.Exception.Message = uaex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("PersistFormofPaymentDetails Error {@Exception}", JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = 0;
            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }
            _logger.LogInformation("PersistFormofPaymentDetails {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("Payment/GetCreditCardToken")]
        public async Task<MOBPersistFormofPaymentResponse> GetCreditCardToken(MOBPersistFormofPaymentRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);

            var response = new MOBPersistFormofPaymentResponse();
            IDisposable timer = null;
            try
            {
                #region Remove Encrypted CardNumber from request while logging (Change from Security Complaince team)
                MOBPersistFormofPaymentRequest clonedRequest = request.Clone();
                ConfigUtility.RemoveEncyptedCreditcardNumber(clonedRequest?.FormofPaymentDetails?.CreditCard);
                #endregion
                _logger.LogInformation("GetCreditCardToken {@ClientRequest}", JsonConvert.SerializeObject(clonedRequest));
                using (timer = _logger.BeginTimedOperation("Total time taken for GetCreditCardToken business call", transationId: request.TransactionId))
                {
                    response = await _paymentBusiness.GetCreditCardToken(request);
                }

            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("GetCreditCardToken UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException();
                response.Exception.Message = uaex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("GetCreditCardToken Error {@Exception}", JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = 0;
            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }
            _logger.LogInformation("GetCreditCardToken {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("Payment/GetCartInformation")]
        public async Task<MOBRegisterOfferResponse> GetCartInformation(MOBSHOPMetaSelectTripRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, string.Empty);

            var response = new MOBRegisterOfferResponse();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("GetCartInformation {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for GetCartInformation business call", transationId: request.TransactionId))
                {
                    response = await _paymentBusiness.GetCartInformation(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("GetCartInformation UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException();
                response.Exception.Message = uaex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("GetCartInformation Error {@Exception}", JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }
            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }
            _logger.LogInformation("GetCartInformation {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("Payment/TravelBankCredit")]
        public async Task<MOBFOPResponse> TravelBankCredit(MOBFOPTravelerBankRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);

            var response = new MOBFOPResponse();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("TravelBankCredit {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for TravelBankCredit business call", transationId: request.TransactionId))
                {
                    response = await _paymentBusiness.TravelBankCredit(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("TravelBankCredit UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException();
                response.Exception.Message = uaex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("TravelBankCredit Error {@Exception}", JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = 0;
            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }
            _logger.LogInformation("TravelBankCredit {@ClientResponse}", JsonConvert.SerializeObject(response));

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
                request.ServiceName = ServiceNames.PAYMENT.ToString();
                await _featureSettings.RefreshFeatureSettingCache(request);
            }
            catch (Exception ex)
            {
                _logger.LogError("RefreshFeatureSettingCache Error {exceptionstack} and {transactionId}", JsonConvert.SerializeObject(ex), "RefreshRetrieveAllFeatureSettings_TransId");
                response.Exception = new MOBException("9999", JsonConvert.SerializeObject(ex));
            }
            return response;
        }

        [HttpPost]
        [Route("Payment/CreateProvision")]
        public async Task<MOBProvisionResponse> CreateProvision(MOBProvisionRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);

            var response = new MOBProvisionResponse();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("CreateChaseProvision {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for CreateChaseProvision business call", transationId: request.TransactionId))
                {
                    response = await _paymentBusiness.CreateProvision(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("CreateChaseProvision UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException();
                response.Exception.Message = uaex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("CreateChaseProvision Error {@Exception}", JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = 0;
            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }
            _logger.LogInformation("CreateChaseProvision {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("Payment/GetProvisionDetails")]
        public async Task<MOBProvisionAccountDetails> GetProvisionDetails(MOBProvisionRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);

            var response = new MOBProvisionAccountDetails();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("GetProvisionDetails {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for GetProvisionDetails business call", transationId: request.TransactionId))
                {
                    response = await _paymentBusiness.GetProvisionDetails(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("GetProvisionDetails UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException();
                response.Exception.Message = uaex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("GetProvisionDetails Error {@Exception}", JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = 0;
            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }
            _logger.LogInformation("GetProvisionDetails {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("Payment/UpdateProvisionLinkStatus")]
        public async Task<MOBUpdateProvisionLinkStatusResponse> UpdateProvisionLinkStatus(MOBProvisionRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, request.SessionId);

            var response = new MOBUpdateProvisionLinkStatusResponse();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("UpdateProvisionLinkStatus {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for UpdateProvisionLinkStatus business call", transationId: request.TransactionId))
                {
                    response = await _paymentBusiness.UpdateProvisionLinkStatus(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("UpdateProvisionLinkStatus UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException();
                response.Exception.Message = uaex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError("UpdateProvisionLinkStatus Error {@Exception}", JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = 0;
            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }
            _logger.LogInformation("UpdateProvisionLinkStatus {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }
    }
}