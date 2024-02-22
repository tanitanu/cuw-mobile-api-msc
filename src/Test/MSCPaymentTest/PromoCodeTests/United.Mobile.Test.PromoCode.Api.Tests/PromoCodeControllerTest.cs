using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Definition;
using United.Definition.FormofPayment;
using United.Mobile.Model.Common;
using United.Mobile.PromoCode.Api.Controllers;
using United.Mobile.PromoCode.Domain;
using United.Utility.Helper;
using Xunit;
using Constants = United.Mobile.Model.Constants;
namespace United.Mobile.Test.PromoCode.Api.Tests
{
    public class PromoCodeControllerTest
    {
        private readonly Mock<ICacheLog<PromoCodeController>> _logger;
        private readonly IConfiguration _configuration;
        private readonly Mock<IHeaders> _headers;
        private readonly Mock<IPromoCodeBusiness> _promoCodeBusiness;
        private readonly PromoCodeController _promoCodeController;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<IFeatureSettings> _featureSettings;

        public PromoCodeControllerTest()
        {
            _logger = new Mock<ICacheLog<PromoCodeController>>();
            _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true)
            .Build();
            _headers = new Mock<IHeaders>();
            _promoCodeBusiness = new Mock<IPromoCodeBusiness>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _promoCodeController = new PromoCodeController(_logger.Object, _configuration, _headers.Object, _promoCodeBusiness.Object,_featureSettings.Object);
            SetupHttpContextAccessor();
            SetHeaders();

        }

        private void SetupHttpContextAccessor()
        {
            var guid = Guid.NewGuid().ToString();
            var context = new DefaultHttpContext();
            context.Request.Headers[Constants.HeaderAppIdText] = "1";
            context.Request.Headers[Constants.HeaderAppMajorText] = "1";
            context.Request.Headers[Constants.HeaderAppMinorText] = "0";
            context.Request.Headers[Constants.HeaderDeviceIdText] = guid;
            context.Request.Headers[Constants.HeaderLangCodeText] = "en-us";
            context.Request.Headers[Constants.HeaderRequestTimeUtcText] = DateTime.UtcNow.ToString();
            context.Request.Headers[Constants.HeaderTransactionIdText] = guid;
            _httpContextAccessor.Setup(_ => _.HttpContext).Returns(context);
        }

        private void SetHeaders(string deviceId = "589d7852-14e7-44a9-b23b-a6db36657579"
           , string applicationId = "2"
           , string appVersion = "4.1.29"
           , string transactionId = "589d7852-14e7-44a9-b23b-a6db36657579|8f46e040-a200-495c-83ca-4fca2d7175fb"
           , string languageCode = "en-US"
           , string sessionId = "17C979E184CC495EA083D45F4DD9D19D")
        {
            _headers.Setup(_ => _.ContextValues).Returns(
           new HttpContextValues
           {
               Application = new Application()
               {
                   Id = Convert.ToInt32(applicationId),
                   Version = new Mobile.Model.Version
                   {
                       Major = string.IsNullOrEmpty(appVersion) ? 0 : int.Parse(appVersion.ToString().Substring(0, 1)),
                       Minor = string.IsNullOrEmpty(appVersion) ? 0 : int.Parse(appVersion.ToString().Substring(2, 1)),
                       Build = string.IsNullOrEmpty(appVersion) ? 0 : int.Parse(appVersion.ToString().Substring(4, 2))
                   }
               },
               DeviceId = deviceId,
               LangCode = languageCode,
               TransactionId = transactionId,
               SessionId = sessionId
           });

        }
        [Fact]
        public void HealthCheck_Test()
        {
            string result = _promoCodeController.HealthCheck();
            Assert.True(result == "Healthy");
        }

        [Fact]
        public void ApplyPromoCode_Test()
        {
            var response = new MOBApplyPromoCodeResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44", CallDuration = 0 };
            //var request = new Request<MOBApplyPromoCodeRequest>()
            var request = new MOBApplyPromoCodeRequest()
            {


                Application = new MOBApplication()
                {
                    Id = 1,
                    Version = new MOBVersion()
                    {
                        Major = "4.1.25",
                        Minor = "4.1.25"
                    }
                },
                TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44"

            };
            _promoCodeController.ControllerContext = new ControllerContext();
            _promoCodeController.ControllerContext.HttpContext = new DefaultHttpContext();
            _promoCodeController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _promoCodeBusiness.Setup(p => p.ApplyPromoCode(request)).Returns(Task.FromResult(response));


            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _promoCodeController.ApplyPromoCode(request);
            Assert.True(result.Result.TransactionId != null);
            Assert.True(result.Result.Exception == null);
        }
        [Fact]
        public void ApplyPromoCode_SystemException_Test()
        {
            var request = new MOBApplyPromoCodeRequest()
            {

                Application = new MOBApplication()
                {
                    Id = 1,
                    Version = new MOBVersion()
                    {
                        Major = "4.1.25",
                        Minor = "4.1.25"
                    }
                }

            };
            _promoCodeController.ControllerContext = new ControllerContext();
            _promoCodeController.ControllerContext.HttpContext = new DefaultHttpContext();
            _promoCodeController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _promoCodeBusiness.Setup(p => p.ApplyPromoCode(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _promoCodeController.ApplyPromoCode(request);
            Assert.True(result.Result.Exception != null);
        }
        [Fact]
        public void ApplyPromoCode_UnitedException_Test()
        {
            var request = new MOBApplyPromoCodeRequest()
            {

                Application = new MOBApplication()
                {
                    Id = 1,
                    Version = new MOBVersion()
                    {
                        Major = "4.1.25",
                        Minor = "4.1.25"
                    }
                },

            };
            _promoCodeController.ControllerContext = new ControllerContext();
            _promoCodeController.ControllerContext.HttpContext = new DefaultHttpContext();
            _promoCodeController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _promoCodeBusiness.Setup(p => p.ApplyPromoCode(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _promoCodeController.ApplyPromoCode(request);
            Assert.True(result.Result.Exception.Message != null);
        }
        public void GetTermsandConditionsByPromoCode_Test()
        {
            var response = new MOBPromoCodeTermsandConditionsResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44", CallDuration = 0 };
            var request = new MOBApplyPromoCodeRequest()
            {

                Application = new MOBApplication()
                {
                    Id = 1,
                    Version = new MOBVersion()
                    {
                        Major = "4.1.25",
                        Minor = "4.1.25"
                    }
                },
                TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44"

            };
            _promoCodeController.ControllerContext = new ControllerContext();
            _promoCodeController.ControllerContext.HttpContext = new DefaultHttpContext();
            _promoCodeController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _promoCodeBusiness.Setup(p => p.GetTermsandConditionsByPromoCode(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _promoCodeController.GetTermsandConditionsByPromoCode(request);
            Assert.True(result.Result.TransactionId != null);
            Assert.True(result.Result.Exception == null);
        }
        [Fact]
        public void GetTermsandConditionsByPromoCode_SystemException_Test()
        {
            var request = new MOBApplyPromoCodeRequest()
            {

                Application = new MOBApplication()
                {
                    Id = 1,
                    Version = new MOBVersion()
                    {
                        Major = "4.1.25",
                        Minor = "4.1.25"
                    }
                }

            };
            _promoCodeController.ControllerContext = new ControllerContext();
            _promoCodeController.ControllerContext.HttpContext = new DefaultHttpContext();
            _promoCodeController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _promoCodeBusiness.Setup(p => p.GetTermsandConditionsByPromoCode(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _promoCodeController.GetTermsandConditionsByPromoCode(request);
            Assert.True(result.Result.Exception != null);
        }
        [Fact]
        public void GetTermsandConditionsByPromoCode_UnitedException_Test()
        {
            var request = new MOBApplyPromoCodeRequest()
            {

                Application = new MOBApplication()
                {
                    Id = 1,
                    Version = new MOBVersion()
                    {
                        Major = "4.1.25",
                        Minor = "4.1.25"
                    }
                },

            };
            _promoCodeController.ControllerContext = new ControllerContext();
            _promoCodeController.ControllerContext.HttpContext = new DefaultHttpContext();
            _promoCodeController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _promoCodeBusiness.Setup(p => p.GetTermsandConditionsByPromoCode(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _promoCodeController.GetTermsandConditionsByPromoCode(request);
            Assert.True(result.Result.Exception.Message != null);
        }

        [Fact]
        public void RemovePromoCode_Test()
        {
            var response = new MOBApplyPromoCodeResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44", CallDuration = 0 };
            var request = new MOBApplyPromoCodeRequest()
            {

                Application = new MOBApplication()
                {
                    Id = 1,
                    Version = new MOBVersion()
                    {
                        Major = "4.1.25",
                        Minor = "4.1.25"
                    }
                },
                TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44"

            };
            _promoCodeController.ControllerContext = new ControllerContext();
            _promoCodeController.ControllerContext.HttpContext = new DefaultHttpContext();
            _promoCodeController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _promoCodeBusiness.Setup(p => p.RemovePromoCode(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _promoCodeController.RemovePromoCode(request);
            Assert.True(result.Result.TransactionId != null);
            Assert.True(result.Result.Exception == null);
        }
        [Fact]
        public void RemovePromoCode_SystemException_Test()
        {
            var request = new MOBApplyPromoCodeRequest()
            {

                Application = new MOBApplication()
                {
                    Id = 1,
                    Version = new MOBVersion()
                    {
                        Major = "4.1.25",
                        Minor = "4.1.25"
                    }
                }

            };
            _promoCodeController.ControllerContext = new ControllerContext();
            _promoCodeController.ControllerContext.HttpContext = new DefaultHttpContext();
            _promoCodeController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _promoCodeBusiness.Setup(p => p.RemovePromoCode(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _promoCodeController.RemovePromoCode(request);
            Assert.True(result.Result.Exception != null);
        }
        [Fact]
        public void RemovePromoCode_UnitedException_Test()
        {
            var request = new MOBApplyPromoCodeRequest()
            {

                Application = new MOBApplication()
                {
                    Id = 1,
                    Version = new MOBVersion()
                    {
                        Major = "4.1.25",
                        Minor = "4.1.25"
                    }
                },

            };
            _promoCodeController.ControllerContext = new ControllerContext();
            _promoCodeController.ControllerContext.HttpContext = new DefaultHttpContext();
            _promoCodeController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _promoCodeBusiness.Setup(p => p.RemovePromoCode(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _promoCodeController.RemovePromoCode(request);
            Assert.True(result.Result.Exception.Message != null);
        }

    }
}

