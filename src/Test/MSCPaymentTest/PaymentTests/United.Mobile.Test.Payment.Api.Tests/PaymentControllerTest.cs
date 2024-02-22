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
using United.Definition.Shopping;
using United.Mobile.Model.Common;
using United.Mobile.Payment.Api.Controllers;
using United.Mobile.Payment.Domain;
using United.Utility.Helper;
using Xunit;
using Constants = United.Mobile.Model.Constants;

namespace United.Mobile.Test.Payment.Api.Tests
{
    public class PaymentControllerTests
    {
        private readonly Mock<ICacheLog<PaymentController>> _logger;
        private readonly IConfiguration _configuration;
        private readonly Mock<IHeaders> _headers;
        private readonly Mock<IPaymentBusiness> _paymentBusiness;
        private readonly PaymentController _paymentController;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<IFeatureSettings> _featureSettings;
        public PaymentControllerTests()
        {
            _logger = new Mock<ICacheLog<PaymentController>>();
            _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true)
            .Build();
            _headers = new Mock<IHeaders>();
            _paymentBusiness = new Mock<IPaymentBusiness>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _paymentController = new PaymentController(_logger.Object, _configuration, _headers.Object, _paymentBusiness.Object, _featureSettings.Object);
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
            string result = _paymentController.HealthCheck();
            Assert.True(result == "Healthy");
        }
        [Fact]
        public void GetPaymentToken_Test()
        {
            var response = new MOBFOPAcquirePaymentTokenResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44", CallDuration = 0 };
            //var request = new Request<MOBFOPAcquirePaymentTokenRequest>()
            var request = new MOBFOPAcquirePaymentTokenRequest()
            {
               
                    CountryCode = "",
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
            _paymentController.ControllerContext = new ControllerContext();
            _paymentController.ControllerContext.HttpContext = new DefaultHttpContext();
            _paymentController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";


            //todo
             _paymentBusiness.Setup(p => p.GetPaymentToken(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _paymentController.GetPaymentToken(request);
            Assert.True(result.Result.TransactionId != null);
            Assert.True(result.Result.Exception == null);
        }

        [Fact]
        public void GetPaymentToken_SystemException_Test()
        {
            var request = new MOBFOPAcquirePaymentTokenRequest()
            {
                
                    CountryCode = "",
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
            _paymentController.ControllerContext = new ControllerContext();
            _paymentController.ControllerContext.HttpContext = new DefaultHttpContext();
            _paymentController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            //todo
            _paymentBusiness.Setup(p => p.GetPaymentToken(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _paymentController.GetPaymentToken(request);
            Assert.True(result.Result.Exception != null);
        }

        [Fact]
        public void GetPaymentToken_UnitedException_Test()
        {
            var request = new MOBFOPAcquirePaymentTokenRequest()
            {
                
                    CountryCode = "",
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
            _paymentController.ControllerContext = new ControllerContext();
            _paymentController.ControllerContext.HttpContext = new DefaultHttpContext();
            _paymentController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            //todo
            _paymentBusiness.Setup(p => p.GetPaymentToken(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _paymentController.GetPaymentToken(request);
            Assert.True(result.Result.Exception.Message != null);
        }


        [Fact]
        public void PersistFormofPaymentDetails_Test()
        {
            var response = new MOBPersistFormofPaymentResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44", CallDuration = 0 };
            var request = new MOBPersistFormofPaymentRequest()

            //var request = new Request<Model.MSCPayment.MOBPersistFormofPaymentRequest>()
             //var request = new MOBFOPAcquirePaymentTokenRequest()
             {
               
                    Amount = "",
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
            _paymentController.ControllerContext = new ControllerContext();
            _paymentController.ControllerContext.HttpContext = new DefaultHttpContext();
            _paymentController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            //todo
             _paymentBusiness.Setup(p => p.PersistFormofPaymentDetails(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _paymentController.PersistFormofPaymentDetails(request);
            Assert.True(result.Result.TransactionId != null);
            Assert.True(result.Result.Exception == null);
        }

        [Fact]
        public void PersistFormofPaymentDetails_SystemException_Test()
        {
            var request = new MOBPersistFormofPaymentRequest()
            {
                
                
                    Amount = "",
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
            _paymentController.ControllerContext = new ControllerContext();
            _paymentController.ControllerContext.HttpContext = new DefaultHttpContext();
            _paymentController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            //todo
            _paymentBusiness.Setup(p => p.PersistFormofPaymentDetails(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _paymentController.PersistFormofPaymentDetails(request);
            Assert.True(result.Result.Exception != null);
        }

        [Fact]
        public void PersistFormofPaymentDetails_UnitedException_Test()
        {
            var request = new MOBPersistFormofPaymentRequest()
            {
               
                    Amount = "",
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
            _paymentController.ControllerContext = new ControllerContext();
            _paymentController.ControllerContext.HttpContext = new DefaultHttpContext();
            _paymentController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            //todo
            _paymentBusiness.Setup(p => p.PersistFormofPaymentDetails(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _paymentController.PersistFormofPaymentDetails(request);
            Assert.True(result.Result.Exception.Message != null);
        }


        [Fact]
        public void GetCreditCardToken_Test()
        {
            var response = new MOBPersistFormofPaymentResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44", CallDuration=0};
            //var request = new Request<Model.MSCPayment.MOBPersistFormofPaymentRequest>()
            var request = new MOBPersistFormofPaymentRequest()
            {
               
                    Amount = "",
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
            _paymentController.ControllerContext = new ControllerContext();
            _paymentController.ControllerContext.HttpContext = new DefaultHttpContext();
            _paymentController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            //todo
            _paymentBusiness.Setup(p => p.GetCreditCardToken(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _paymentController.GetCreditCardToken(request);
            Assert.True(result.Result.TransactionId !=null);
            Assert.True(result.Result.Exception == null);
        }

        [Fact]
        public void GetCreditCardToken_SystemException_Test()
        {
            //var request = new Request<United.Mobile.Model.MSCPayment.MOBPersistFormofPaymentRequest>()
            var request = new MOBPersistFormofPaymentRequest()
            {
               
                    Amount = "",
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
            _paymentController.ControllerContext = new ControllerContext();
            _paymentController.ControllerContext.HttpContext = new DefaultHttpContext();
            _paymentController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            //todo
             _paymentBusiness.Setup(p => p.GetCreditCardToken(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _paymentController.GetCreditCardToken(request);
            Assert.True(result.Result.Exception != null);
        }

        [Fact]
        public void GetCreditCardToken_UnitedException_Test()
        {
            var request = new MOBPersistFormofPaymentRequest()
            {
               
                    Amount = "",
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
            _paymentController.ControllerContext = new ControllerContext();
            _paymentController.ControllerContext.HttpContext = new DefaultHttpContext();
            _paymentController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            //todo
            _paymentBusiness.Setup(p => p.GetCreditCardToken(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            //todo
            var result = _paymentController.GetCreditCardToken(request);
            Assert.True(result.Result.Exception.Message != null);
        }



        [Fact]
        public void GetCartInformation_Test()
        {
            var response = new MOBRegisterOfferResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44", CallDuration = 0 };
            //var request = new MOBPersistFormofPaymentRequest()
            {
                var request = new MOBSHOPMetaSelectTripRequest()
                {
                    CartId = "DE5334A1-F7E8-4925-8F9B-12AF8E4557A7",
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
                _paymentController.ControllerContext = new ControllerContext();
                _paymentController.ControllerContext.HttpContext = new DefaultHttpContext();
                _paymentController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

                _paymentBusiness.Setup(p => p.GetCartInformation(request)).Returns(Task.FromResult(response));

                var httpContext = new DefaultHttpContext();
                httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
                var result = _paymentController.GetCartInformation(request);
                Assert.True(result.Result.TransactionId != null);
                Assert.True(result.Result.Exception == null);
            }
        }

        [Fact]

        public void GetCartInformation_SystemException_Test()
        {
                var request = new MOBSHOPMetaSelectTripRequest()
                {
               
                    CartId = "DE5334A1-F7E8-4925-8F9B-12AF8E4557A7",
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
            _paymentController.ControllerContext = new ControllerContext();
            _paymentController.ControllerContext.HttpContext = new DefaultHttpContext();
            _paymentController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _paymentBusiness.Setup(p => p.GetCartInformation(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _paymentController.GetCartInformation(request);
            Assert.True(result.Result.Exception != null);
        }

        [Fact]
        public void GetCartInformation_UnitedException_Test()
        {
            var request = new MOBSHOPMetaSelectTripRequest()
            {
             
                    CartId = "DE5334A1-F7E8-4925-8F9B-12AF8E4557A7",
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
            _paymentController.ControllerContext = new ControllerContext();
            _paymentController.ControllerContext.HttpContext = new DefaultHttpContext();
            _paymentController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

             _paymentBusiness.Setup(p => p.GetCartInformation(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _paymentController.GetCartInformation(request);
            Assert.True(result.Result.Exception.Message != null);
        }


        [Fact]
        public void TravelBankCredit_Test()
        {
            var response = new MOBFOPResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44", CallDuration = 0 };
            //var request = new Request<MOBFOPTravelerBankRequest>()
            var request = new MOBFOPTravelerBankRequest()
            {
               
                    CartId = "DE5334A1-F7E8-4925-8F9B-12AF8E4557A7",
                    DisplayAmount = "",
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
            _paymentController.ControllerContext = new ControllerContext();
            _paymentController.ControllerContext.HttpContext = new DefaultHttpContext();
            _paymentController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

              _paymentBusiness.Setup(p => p.TravelBankCredit(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _paymentController.TravelBankCredit(request);
            Assert.True(result.Result.TransactionId !=null);
            Assert.True(result.Result.Exception == null);
        }

        [Fact]
        public void TravelBankCredit_SystemException_Test()
        {
            var request = new MOBFOPTravelerBankRequest()
            {
               
                    CartId = "DE5334A1-F7E8-4925-8F9B-12AF8E4557A7",
                    DisplayAmount = "",
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
            _paymentController.ControllerContext = new ControllerContext();
            _paymentController.ControllerContext.HttpContext = new DefaultHttpContext();
            _paymentController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _paymentBusiness.Setup(p => p.TravelBankCredit(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _paymentController.TravelBankCredit(request);
            Assert.True(result.Result.Exception != null);
        }

        [Fact]
        public void TravelBankCredit_UnitedException_Test()
        {
            var request = new MOBFOPTravelerBankRequest()
            {
               
                    CartId = "DE5334A1-F7E8-4925-8F9B-12AF8E4557A7",
                    DisplayAmount = "",
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
            _paymentController.ControllerContext = new ControllerContext();
            _paymentController.ControllerContext.HttpContext = new DefaultHttpContext();
            _paymentController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

             _paymentBusiness.Setup(p => p. TravelBankCredit(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _paymentController.TravelBankCredit(request);
            Assert.True(result.Result.Exception.Message != null);
        }

    }
}
    


