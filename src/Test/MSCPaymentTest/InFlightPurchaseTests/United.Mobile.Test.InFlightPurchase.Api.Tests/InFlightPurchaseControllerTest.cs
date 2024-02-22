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
using United.Mobile.InFlightPurchase.Api.Controllers;
using United.Mobile.InFlightPurchase.Domain;
using United.Mobile.Model.Common;
using United.Utility.Helper;
using Xunit;
using Constants = United.Mobile.Model.Constants;

namespace United.Mobile.Test.InFlightPurchase.Api.Tests
{
    public class InFlightPurchaseControllerTest
    {
        private readonly Mock<ICacheLog<InFlightPurchaseController>> _logger;
        private readonly IConfiguration _configuration;
        private readonly Mock<IHeaders> _headers;
        private readonly Mock<IInFlightPurchaseBusiness> _inFlightPurchaseMilesBusiness;
        private readonly InFlightPurchaseController _inFlightPurchaseController;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        public InFlightPurchaseControllerTest()
        {
            _logger = new Mock<ICacheLog<InFlightPurchaseController>>();
            _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true)
            .Build();
            _headers = new Mock<IHeaders>();
            _inFlightPurchaseMilesBusiness = new Mock<IInFlightPurchaseBusiness>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _inFlightPurchaseController = new InFlightPurchaseController(_logger.Object, _configuration, _headers.Object, _inFlightPurchaseMilesBusiness.Object);
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
            string result = _inFlightPurchaseController.HealthCheck();
            Assert.True(result == "Healthy");
        }

        [Fact]
        public void EiligibilityCheckToAddNewCCForInFlightPurchase_Test()
        {
            var response = new MOBInFlightPurchaseResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44", CallDuration = 0 };
            //var request = new Request<MOBInFlightPurchaseRequest>()
            var request = new MOBInFlightPurchaseRequest()
            {
                
                    MileagePlusNumber = "AW791957",
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
            _inFlightPurchaseController.ControllerContext = new ControllerContext();
            _inFlightPurchaseController.ControllerContext.HttpContext = new DefaultHttpContext();
            _inFlightPurchaseController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _inFlightPurchaseMilesBusiness.Setup(p => p.EiligibilityCheckToAddNewCCForInFlightPurchase(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _inFlightPurchaseController.EiligibilityCheckToAddNewCCForInFlightPurchase(request);
            Assert.True(result.Result.TransactionId != null);
            Assert.True(result.Result.Exception == null);
        }

        [Fact]
        public void EiligibilityCheckToAddNewCCForInFlightPurchase_SystemException_Test()
        {
            //var request = new Request<MOBInFlightPurchaseRequest>()
            var request = new MOBInFlightPurchaseRequest()
            {
               
                    MileagePlusNumber = "AW791957",
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
            _inFlightPurchaseController.ControllerContext = new ControllerContext();
            _inFlightPurchaseController.ControllerContext.HttpContext = new DefaultHttpContext();
            _inFlightPurchaseController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _inFlightPurchaseMilesBusiness.Setup(p => p.EiligibilityCheckToAddNewCCForInFlightPurchase(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _inFlightPurchaseController.EiligibilityCheckToAddNewCCForInFlightPurchase(request);
            Assert.True(result.Result.Exception != null);
        }

        [Fact]
        public void EiligibilityCheckToAddNewCCForInFlightPurchase_UnitedException_Test()
        {
            //var request = new Request<MOBInFlightPurchaseRequest>()
            var request = new MOBInFlightPurchaseRequest()
            {
               
                    MileagePlusNumber = "AW791957",
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
            _inFlightPurchaseController.ControllerContext = new ControllerContext();
            _inFlightPurchaseController.ControllerContext.HttpContext = new DefaultHttpContext();
            _inFlightPurchaseController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

             _inFlightPurchaseMilesBusiness.Setup(p => p.EiligibilityCheckToAddNewCCForInFlightPurchase(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _inFlightPurchaseController.EiligibilityCheckToAddNewCCForInFlightPurchase(request);
            Assert.True(result.Result.Exception.Message != null);
        }

        [Fact]
        public void SaveCCPNROnlyForInflightPurchase_Test()
        {
            var response = new MOBInFlightPurchaseResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44", CallDuration = 0 };
            //var request = new Request<MOBSavedCCInflightPurchaseRequest>()
            var request = new MOBSavedCCInflightPurchaseRequest()
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
            _inFlightPurchaseController.ControllerContext = new ControllerContext();
            _inFlightPurchaseController.ControllerContext.HttpContext = new DefaultHttpContext();
            _inFlightPurchaseController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _inFlightPurchaseMilesBusiness.Setup(p => p.SaveCCPNROnlyForInflightPurchase(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _inFlightPurchaseController.SaveCCPNROnlyForInflightPurchase(request);
            Assert.True(result.Result.TransactionId != null);
            Assert.True(result.Result.Exception == null);
        }

        [Fact]
        public void SaveCCPNROnlyForInflightPurchase_SystemException_Test()
        {
            //var request = new Request<MOBSavedCCInflightPurchaseRequest>()
            var request = new MOBSavedCCInflightPurchaseRequest()
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
            _inFlightPurchaseController.ControllerContext = new ControllerContext();
            _inFlightPurchaseController.ControllerContext.HttpContext = new DefaultHttpContext();
            _inFlightPurchaseController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

             _inFlightPurchaseMilesBusiness.Setup(p => p.SaveCCPNROnlyForInflightPurchase(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _inFlightPurchaseController.SaveCCPNROnlyForInflightPurchase(request);
            Assert.True(result.Result.Exception != null);
        }

        [Fact]
        public void SaveCCPNROnlyForInflightPurchase_UnitedException_Test()
        {
            //var request = new Request<MOBSavedCCInflightPurchaseRequest>()
            var request = new MOBSavedCCInflightPurchaseRequest()
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
            _inFlightPurchaseController.ControllerContext = new ControllerContext();
            _inFlightPurchaseController.ControllerContext.HttpContext = new DefaultHttpContext();
            _inFlightPurchaseController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

             _inFlightPurchaseMilesBusiness.Setup(p => p. SaveCCPNROnlyForInflightPurchase(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _inFlightPurchaseController.SaveCCPNROnlyForInflightPurchase(request);
            Assert.True(result.Result.Exception.Message != null);
        }

        [Fact]
        public void VerifySavedCCForInflightPurchase_Test()
        {
            var response = new MOBInFlightPurchaseResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44", CallDuration = 0 };
            //var request = new Request<MOBSavedCCInflightPurchaseRequest>()
            var request = new MOBSavedCCInflightPurchaseRequest()
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
            _inFlightPurchaseController.ControllerContext = new ControllerContext();
            _inFlightPurchaseController.ControllerContext.HttpContext = new DefaultHttpContext();
            _inFlightPurchaseController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _inFlightPurchaseMilesBusiness.Setup(p => p.VerifySavedCCForInflightPurchase(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _inFlightPurchaseController.VerifySavedCCForInflightPurchase(request);
            Assert.True(result.Result.TransactionId != null);
            Assert.True(result.Result.Exception == null);
        }

        [Fact]
        public void VerifySavedCCForInflightPurchase_SystemException_Test()
        {
            //var request = new Request<MOBSavedCCInflightPurchaseRequest>()
            var request = new MOBSavedCCInflightPurchaseRequest()
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
            _inFlightPurchaseController.ControllerContext = new ControllerContext();
            _inFlightPurchaseController.ControllerContext.HttpContext = new DefaultHttpContext();
            _inFlightPurchaseController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _inFlightPurchaseMilesBusiness.Setup(p => p.VerifySavedCCForInflightPurchase(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _inFlightPurchaseController.VerifySavedCCForInflightPurchase(request);
            Assert.True(result.Result.Exception != null);
        }

        [Fact]
        public void VerifySavedCCForInflightPurchase_UnitedException_Test()
        {
            //var request = new Request<MOBSavedCCInflightPurchaseRequest>()
            var request = new MOBSavedCCInflightPurchaseRequest()
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
            _inFlightPurchaseController.ControllerContext = new ControllerContext();
            _inFlightPurchaseController.ControllerContext.HttpContext = new DefaultHttpContext();
            _inFlightPurchaseController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _inFlightPurchaseMilesBusiness.Setup(p => p. VerifySavedCCForInflightPurchase(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _inFlightPurchaseController.VerifySavedCCForInflightPurchase(request);
            Assert.True(result.Result.Exception.Message != null);
        }
    }
}

