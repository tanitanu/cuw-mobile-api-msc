using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Definition;
using United.Definition.FormofPayment;
using United.Mobile.InFlightPurchase.Domain;
using United.Mobile.Model.Common;
using United.Utility.Helper;
using United.Utility.Serilog;

namespace United.Mobile.InFlightPurchase.Api.Controllers
{
    [Route("inflightpurchaseservice/api")]
    [ApiController]
    public class InFlightPurchaseController : ControllerBase
    {
        private readonly ICacheLog<InFlightPurchaseController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHeaders _headers;
        private readonly IInFlightPurchaseBusiness _inFlightPurchaseBusiness;

        public InFlightPurchaseController(ICacheLog<InFlightPurchaseController> logger
            , IConfiguration configuration
            , IHeaders headers
            , IInFlightPurchaseBusiness inFlightPurchaseBusiness
            )
        {
            _logger = logger;
            _configuration = configuration;
            _headers = headers;
            _inFlightPurchaseBusiness = inFlightPurchaseBusiness;

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
        [Route("Payment/EiligibilityCheckToAddNewCCForInFlightPurchase")]
        public async Task<MOBInFlightPurchaseResponse> EiligibilityCheckToAddNewCCForInFlightPurchase(MOBInFlightPurchaseRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, string.Empty);

            var response = new MOBInFlightPurchaseResponse();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("EiligibilityCheckToAddNewCCForInFlightPurchase {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for EiligibilityCheckToAddNewCCForInFlightPurchase business call", string.Empty))
                {
                    response = await _inFlightPurchaseBusiness.EiligibilityCheckToAddNewCCForInFlightPurchase(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogError("EiligibilityCheckToAddNewCCForInFlightPurchase UnitedException {exception} and {exceptionstack}", uaex.Message, JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException("9999", !string.IsNullOrEmpty(uaex?.Message) ? uaex.Message : _configuration.GetValue<string>("GenericExceptionMessage"));

            }
            catch (Exception ex)
            {
                _logger.LogError("EiligibilityCheckToAddNewCCForInFlightPurchase Error {exception} and {exceptionstack}",ex.Message, JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("EiligibilityCheckToAddNewCCForInFlightPurchase {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("Payment/SaveCCPNROnlyForInflightPurchase")]
        public async Task<MOBInFlightPurchaseResponse> SaveCCPNROnlyForInflightPurchase(MOBSavedCCInflightPurchaseRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, string.Empty);

            var response = new MOBInFlightPurchaseResponse();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("SaveCCPNROnlyForInflightPurchase {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for SaveCCPNROnlyForInflightPurchase business call", string.Empty))
                {
                    response = await _inFlightPurchaseBusiness.SaveCCPNROnlyForInflightPurchase(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogError("SaveCCPNROnlyForInflightPurchase UnitedException {exception} and {exceptionstack}",uaex.Message, JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException("9999", !string.IsNullOrEmpty(uaex?.Message) ? uaex.Message : _configuration.GetValue<string>("GenericExceptionMessage"));

            }
            catch (Exception ex)
            {
                _logger.LogError("SaveCCPNROnlyForInflightPurchase Error {exception} and {exceptionstack}",ex.Message, JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("SaveCCPNROnlyForInflightPurchase {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }

        [HttpPost]
        [Route("Payment/VerifySavedCCForInflightPurchase")]
        public async Task<MOBInFlightPurchaseResponse> VerifySavedCCForInflightPurchase(MOBSavedCCInflightPurchaseRequest request)
        {
            await _headers.SetHttpHeader(request.DeviceId, request.Application.Id.ToString(), request.Application.Version.Major, request.TransactionId, request.LanguageCode, string.Empty);

            var response = new MOBInFlightPurchaseResponse();
            IDisposable timer = null;
            try
            {
                _logger.LogInformation("VerifySavedCCForInflightPurchase {@ClientRequest}", JsonConvert.SerializeObject(request));
                using (timer = _logger.BeginTimedOperation("Total time taken for VerifySavedCCForInflightPurchase business call", string.Empty))
                {
                    response = await _inFlightPurchaseBusiness.VerifySavedCCForInflightPurchase(request);
                }
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogError("VerifySavedCCForInflightPurchase UnitedException {exception} and {exceptionstack}",uaex.Message, JsonConvert.SerializeObject(uaex));
                response.Exception = new MOBException("9999", !string.IsNullOrEmpty(uaex?.Message) ? uaex.Message : _configuration.GetValue<string>("GenericExceptionMessage"));

            }
            catch (Exception ex)
            {
                _logger.LogError("VerifySavedCCForInflightPurchase Error {exception} and {exceptionstack}",ex.Message, JsonConvert.SerializeObject(ex));
                response.Exception = new MOBException("9999", _configuration.GetValue<string>("GenericExceptionMessage"));
            }

            response.CallDuration = 0;

            if (timer != null)
            {
                response.CallDuration = ((TimedOperation)timer).GetElapseTime();
                timer.Dispose();
            }

            _logger.LogInformation("VerifySavedCCForInflightPurchase {@ClientResponse}", JsonConvert.SerializeObject(response));

            return response;
        }

    }
}
