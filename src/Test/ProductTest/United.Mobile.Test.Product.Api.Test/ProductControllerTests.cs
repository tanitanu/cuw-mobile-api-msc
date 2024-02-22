using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Definition;
using United.Definition.FormofPayment;
using United.Definition.Shopping;
using United.Definition.Shopping.UnfinishedBooking;
using United.Ebs.Logging.Enrichers;
using United.Mobile.DataAccess.Common;
using United.Mobile.Model.Common;
using United.Mobile.Product.Api.Controllers;
using United.Mobile.Product.Domain;
using United.Utility.Helper;
using Xunit;
using Constants = United.Mobile.Model.Constants;
using ControllerContext = Microsoft.AspNetCore.Mvc.ControllerContext;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace United.Mobile.Test.Product.Api.Test
{
    public class ProductControllerTests
    {
        private readonly Mock<ICacheLog<ProductController>> _logger;
        private readonly IConfiguration _configuration;
        private readonly Mock<IHeaders> _headers;
        private readonly Mock<IProductBusiness> _productBusiness;
        private readonly Mock<IApplicationEnricher> _applicationEnricher;
        private readonly ProductController _productController;
        private readonly Mock<ISessionHelperService> _sessionHelperService;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<IFeatureSettings> _featureSettings;
        public ProductControllerTests()
        {
            _logger = new Mock<ICacheLog<ProductController>>();
            _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true)
            .Build();
            _headers = new Mock<IHeaders>();
            _productBusiness = new Mock<IProductBusiness>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _sessionHelperService = new Mock<ISessionHelperService>();
            _applicationEnricher = new Mock<IApplicationEnricher>();    
            _productController = new ProductController(_logger.Object, _configuration, _headers.Object, _productBusiness.Object, _applicationEnricher.Object, _sessionHelperService.Object,_featureSettings.Object);
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
            string result = _productController.HealthCheck();
            Assert.True(result == "Healthy");
        }

        [Fact]
        public void RegisterOffersForBooking_Test()
        {

            var response = new MOBBookingRegisterOfferResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44", CallDuration = 0 };

            //MOBRegisterOfferRequest request = new MOBRegisterOfferRequest()
            var request = new MOBRegisterOfferRequest()
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
            _productController.ControllerContext = new ControllerContext();
            _productController.ControllerContext.HttpContext = new DefaultHttpContext();
            _productController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";


            _productBusiness.Setup(p => p.RegisterOffersForBooking(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _productController.RegisterOffersForBooking(request);
            //todo
            //Assert.True(result.Result.TransactionId != null);
            //Assert.True(result.Result.Exception == null);
        }

        [Fact]
        public void RegisterOffersForBooking_SystemException_Test()
        {
            var request = new MOBRegisterOfferRequest()
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
            _productController.ControllerContext = new ControllerContext();
            _productController.ControllerContext.HttpContext = new DefaultHttpContext();
            _productController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _productBusiness.Setup(p => p.RegisterOffersForBooking(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _productController.RegisterOffersForBooking(request);
            //todo
            //Assert.True(result.Result.Exception != null);
        }
        [Fact]
        public void RegisterOffersForBooking_UnitedException_Test()
        {
            var request = new MOBRegisterOfferRequest()
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
            _productController.ControllerContext = new ControllerContext();
            _productController.ControllerContext.HttpContext = new DefaultHttpContext();
            _productController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _productBusiness.Setup(p => p.RegisterOffersForBooking(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _productController.RegisterOffersForBooking(request);
            //todo
            //Assert.True(result.Result.Exception.Message != null);
        }


        [Fact]
        public void RegisterOffersForReshop_Test()
        {
            var response = new MOBRESHOPRegisterOfferResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44"};
            var request = new MOBRegisterOfferRequest()
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
            _productController.ControllerContext = new ControllerContext();
            _productController.ControllerContext.HttpContext = new DefaultHttpContext();
            _productController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";


            _productBusiness.Setup(p => p.RegisterOffersForReshop(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _productController.RegisterOffersForReshop(request);
            Assert.True(result.Result.TransactionId != null);
            Assert.True(result.Result.Exception == null);
            Assert.True(result.Result.CallDuration > 0);

        }


        [Fact]
        public void RegisterOffersForReshop_SystemException_Test()
        {
            var request = new MOBRegisterOfferRequest()
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
            _productController.ControllerContext = new ControllerContext();
            _productController.ControllerContext.HttpContext = new DefaultHttpContext();
            _productController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _productBusiness.Setup(p => p.RegisterOffersForReshop(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _productController.RegisterOffersForReshop(request);
            Assert.True(result.Result.Exception != null);
        }
        [Fact]
        public void RegisterOffersForReshop_UnitedException_Test()
        {
            var request = new MOBRegisterOfferRequest()
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
            _productController.ControllerContext = new ControllerContext();
            _productController.ControllerContext.HttpContext = new DefaultHttpContext();
            _productController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _productBusiness.Setup(p => p.RegisterOffersForReshop(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _productController.RegisterOffersForReshop(request);
            Assert.True(result.Result.Exception.Message != null);
        }

        [Fact]
        public void RegisterSeatsForBooking_Test()
        {
            var response = new MOBBookingRegisterSeatsResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44" };
            var request = new MOBRegisterSeatsRequest()
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
            _productController.ControllerContext = new ControllerContext();
            _productController.ControllerContext.HttpContext = new DefaultHttpContext();
            _productController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";


            _productBusiness.Setup(p => p.RegisterSeatsForBooking(request)).Returns(Task.FromResult(response));


            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _productController.RegisterSeatsForBooking(request);
            //todo
            //Assert.True(result.Result.TransactionId == "EE64E779-7B46-4836-B261-62AE35498B44");
            //Assert.True(result.Result.Exception == null);
        }
        [Fact]
        public void RegisterSeatsForBooking_SystemException_Test()
        {
            var request = new MOBRegisterSeatsRequest()
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
            _productController.ControllerContext = new ControllerContext();
            _productController.ControllerContext.HttpContext = new DefaultHttpContext();
            _productController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _productBusiness.Setup(p => p.RegisterSeatsForBooking(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _productController.RegisterSeatsForBooking(request);
            //todo
           // Assert.True(result.Result.Exception != null);
        }

        [Fact]
        public void RegisterSeatsForBooking_UnitedException_Test()
        {
            var request = new MOBRegisterSeatsRequest()
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
            _productController.ControllerContext = new ControllerContext();
            _productController.ControllerContext.HttpContext = new DefaultHttpContext();
            _productController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _productBusiness.Setup(p => p.RegisterSeatsForBooking(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _productController.RegisterSeatsForBooking(request);
            //todo
            //Assert.True(result.Result.Exception.Message != null);
        }

        [Fact]
        public void RegisterSeatsForReshop_Test()
        {
            var response = new MOBReshopRegisterSeatsResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44" };
            var request = new MOBRegisterSeatsRequest()
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
            _productController.ControllerContext = new ControllerContext();
            _productController.ControllerContext.HttpContext = new DefaultHttpContext();
            _productController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";


            _productBusiness.Setup(p => p.RegisterSeatsForReshop(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _productController.RegisterSeatsForReshop(request);
            Assert.True(result.Result.TransactionId == "EE64E779-7B46-4836-B261-62AE35498B44");
            Assert.True(result.Result.Exception == null);

        }

        [Fact]
        public void RegisterSeatsForReshop_SystemException_Test()
        {
            var request = new MOBRegisterSeatsRequest()
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
            _productController.ControllerContext = new ControllerContext();
            _productController.ControllerContext.HttpContext = new DefaultHttpContext();
            _productController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _productBusiness.Setup(p => p.RegisterSeatsForReshop(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _productController.RegisterSeatsForReshop(request);
            Assert.True(result.Result.Exception != null);
        }

        [Fact]
        public void RegisterSeatsForReshop_UnitedException_Test()
        {
            var request = new MOBRegisterSeatsRequest()
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
            _productController.ControllerContext = new ControllerContext();
            _productController.ControllerContext.HttpContext = new DefaultHttpContext();
            _productController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _productBusiness.Setup(p => p.RegisterSeatsForReshop(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _productController.RegisterSeatsForReshop(request);
            Assert.True(result.Result.Exception.Message != null);
        }
        [Fact]
        public void ClearCartOnBackNavigation_Test()
        {
            var response = new MOBBookingRegisterOfferResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44" };
            var request = new MOBClearCartOnBackNavigationRequest()
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
            _productController.ControllerContext = new ControllerContext();
            _productController.ControllerContext.HttpContext = new DefaultHttpContext();
            _productController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";


            _productBusiness.Setup(p => p.ClearCartOnBackNavigation(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _productController.ClearCartOnBackNavigation(request);
            //todo
            //Assert.True(result.Result.TransactionId == "EE64E779-7B46-4836-B261-62AE35498B44");
            //Assert.True(result.Result.Exception == null);
        }
        [Fact]
        public void ClearCartOnBackNavigation_SystemException_Test()
        {
            var request = new MOBClearCartOnBackNavigationRequest()
            {
                SessionId = "CC9CE97C4C2544D48396FA85F479D244",
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
            _productController.ControllerContext = new ControllerContext();
            _productController.ControllerContext.HttpContext = new DefaultHttpContext();
            _productController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _productBusiness.Setup(p => p.ClearCartOnBackNavigation(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _productController.ClearCartOnBackNavigation(request);
            //todo
           // Assert.True(result.Result.Exception != null);
        }
        [Fact]
        public void ClearCartOnBackNavigation_UnitedException_Test()
        {
            var request = new MOBClearCartOnBackNavigationRequest()
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
            _productController.ControllerContext = new ControllerContext();
            _productController.ControllerContext.HttpContext = new DefaultHttpContext();
            _productController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _productBusiness.Setup(p => p.ClearCartOnBackNavigation(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _productController.ClearCartOnBackNavigation(request);
            //todo
           // Assert.True(result.Result.Exception.Message != null);
        }

        [Fact]
        public void RemoveAncillaryOffer_Test()
        {
            var response = new MOBBookingRegisterOfferResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44", CallDuration = 0 };
            var request = new MOBRemoveAncillaryOfferRequest()
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
            _productController.ControllerContext = new ControllerContext();
            _productController.ControllerContext.HttpContext = new DefaultHttpContext();
            _productController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _productBusiness.Setup(p => p.RemoveAncillaryOffer(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _productController.RemoveAncillaryOffer(request);
            Assert.True(result!=null);
            Assert.True(result != null);
        }
        [Fact]
        public void RemoveAncillaryOffer_SystemException_Test()
        {
            var request = new MOBRemoveAncillaryOfferRequest()
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
            _productController.ControllerContext = new ControllerContext();
            _productController.ControllerContext.HttpContext = new DefaultHttpContext();
            _productController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _productBusiness.Setup(p => p.RemoveAncillaryOffer(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _productController.RemoveAncillaryOffer(request);
            Assert.False(result.Exception != null);
        }
        [Fact]
        public void RemoveAncillaryOffer_UnitedException_Test()
        {
            var request = new MOBRemoveAncillaryOfferRequest()
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
            _productController.ControllerContext = new ControllerContext();
            _productController.ControllerContext.HttpContext = new DefaultHttpContext();
            _productController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _productBusiness.Setup(p => p.RemoveAncillaryOffer(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _productController.RemoveAncillaryOffer(request);
            Assert.True(result != null);
        }

        [Fact]
        public void RemoveAncillaryOffer_WebException_Test()
        {
            var request = new MOBRemoveAncillaryOfferRequest()
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

            _productController.ControllerContext = new ControllerContext();
            _productController.ControllerContext.HttpContext = new DefaultHttpContext();
            _productController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _productBusiness.Setup(p => p.RemoveAncillaryOffer(request)).ThrowsAsync(new WebException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _productController.RemoveAncillaryOffer(request);
            Assert.True(result != null);
        }

        [Fact]
        public void RegisterOffersForOmniCartSavedTrip_Test()
        {
            var response = new MOBSHOPSelectTripResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44", CallDuration = 0 };
            var request = new MOBSHOPUnfinishedBookingRequestBase()
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
            _productController.ControllerContext = new ControllerContext();
            _productController.ControllerContext.HttpContext = new DefaultHttpContext();
            _productController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _productBusiness.Setup(p => p.RegisterOffersForOmniCartSavedTrip(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _productController.RegisterOffersForOmniCartSavedTrip(request);
            Assert.True(result.Result.TransactionId == "EE64E779-7B46-4836-B261-62AE35498B44");
            Assert.True(result.Result.Exception == null);
        }
        [Fact]
        public void RegisterOffersForOmniCartSavedTrip_SystemException_Test()
        {
            var request = new MOBSHOPUnfinishedBookingRequestBase()
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
            _productController.ControllerContext = new ControllerContext();
            _productController.ControllerContext.HttpContext = new DefaultHttpContext();
            _productController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _productBusiness.Setup(p => p.RegisterOffersForOmniCartSavedTrip(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _productController.RegisterOffersForOmniCartSavedTrip(request);
            Assert.True(result.Result.Exception != null);
        }
        [Fact]
        public void RegisterOffersForOmniCartSavedTrip_UnitedException_Test()
        {
            var request = new MOBSHOPUnfinishedBookingRequestBase()
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
            _productController.ControllerContext = new ControllerContext();
            _productController.ControllerContext.HttpContext = new DefaultHttpContext();
            _productController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _productBusiness.Setup(p => p.RegisterOffersForOmniCartSavedTrip(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _productController.RegisterOffersForOmniCartSavedTrip(request);
            Assert.True(result.Result.Exception.Message != null);
        }
        [Fact]
        public void UnRegisterAncillaryOffersForBooking_Test()
        {
            var response = new MOBBookingRegisterOfferResponse() { TransactionId = "EE64E779-7B46-4836-B261-62AE35498B44", CallDuration = 0 };
            var request = new MOBRegisterOfferRequest()
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
            _productController.ControllerContext = new ControllerContext();
            _productController.ControllerContext.HttpContext = new DefaultHttpContext();
            _productController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _productBusiness.Setup(p => p.UnRegisterAncillaryOffersForBooking(request)).Returns(Task.FromResult(response));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _productController.UnRegisterAncillaryOffersForBooking(request);

            //todo
            //Assert.True(result.Result.TransactionId == "EE64E779-7B46-4836-B261-62AE35498B44");

            //Assert.True(result.Result.ToString().Contains("EE64E779-7B46-4836-B261-62AE35498B44"));

            //Assert.True(result.Result.Exception == null);
        }

        [Fact]
        public void UnRegisterAncillaryOffersForBooking_SystemException_Test()
        {
            var request = new MOBRegisterOfferRequest()
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
            _productController.ControllerContext = new ControllerContext();
            _productController.ControllerContext.HttpContext = new DefaultHttpContext();
            _productController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _productBusiness.Setup(p => p.UnRegisterAncillaryOffersForBooking(request)).ThrowsAsync(new Exception("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _productController.UnRegisterAncillaryOffersForBooking(request);

            //todo
            // Assert.True(result.Result.Exception != null);

            //Assert.True(result.Result.Exception != null);

        }
      
        [Fact]
        public void UnRegisterAncillaryOffersForBooking_UnitedException_Test()
        {
            var request = new MOBRegisterOfferRequest()
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
            _productController.ControllerContext = new ControllerContext();
            _productController.ControllerContext.HttpContext = new DefaultHttpContext();
            _productController.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";

            _productBusiness.Setup(p => p.UnRegisterAncillaryOffersForBooking(request)).ThrowsAsync(new MOBUnitedException("Error Message"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[Constants.HeaderTransactionIdText].ToString();
            var result = _productController.UnRegisterAncillaryOffersForBooking(request);

            //todo

            //Assert.True(result.Result.Exception.Message != null);
        }

    }
}