using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using United.Common.Helper;
using United.Common.Helper.MSCPayment.Interfaces;
using United.Definition;
using United.Definition.FormofPayment;
using United.Definition.Shopping;
using United.Ebs.Logging.Enrichers;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.DynamoDB;
using United.Mobile.DataAccess.MSCPayment.Interfaces;
using United.Mobile.DataAccess.MSCPayment.Services;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Mobile.Model.Common;
using United.Mobile.MSCCheckOut.Domain;
using United.Persist.Definition.Shopping;
using United.Utility.Helper;
using United.Utility.Http;
using Xunit;

namespace United.Mobile.Test.MSCCheckOut.Api.Tests
{
    public class MSCCheckOutBusinessTest
    {
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<ICacheLog<MSCCheckOutBusiness>> _logger;
        private readonly IConfiguration _configuration;
        private readonly Mock<ISessionHelperService> _sessionHelperService;
        private readonly Mock<ISessionOnCloudService> _sessionOnCloudService;
        private readonly Mock<IApplicationEnricher> _applicationEnricher;
        private readonly Mock<IMSCShoppingSessionHelper> _mSCShoppingSessionHelper;
        private readonly Mock<ILogger> _logger1;
        private readonly Mock<IDataVaultService> _dataVaultService;
        private readonly Mock<IResilientClient> _resilientClient;
        private readonly Mock<ILogger> _logger2;
        private readonly Mock<IDynamoDBService> _dynamoDBService;
        private readonly Mock<IDataPowerFactory> _dataPowerFactory;
        private readonly Mock<ILogger> _logger3;
        private readonly Mock<DocumentLibraryDynamoDB> _documentLibraryDynamoDB;
        private readonly Mock<IShoppingCartService> _shoppingCartService;
        private readonly Mock<IReferencedataService> _referencedataService;
        private readonly Mock<ICheckoutUtiliy> _checkoutUtility;
        private readonly Mock<ILogger> _logger4;
        private readonly Mock<IMSCBaggageInfo> _mscBaggageInfoProvider;
        private readonly Mock<IDPService> _dPService;
        private readonly Mock<ILogger> _logger5;
        private readonly ICachingService _cachingService;
        private readonly Mock<IShoppingUtility> _shoppingUtility;
        private readonly Mock<IMSCFormsOfPayment> _mSCFormsOfPayment;
        private readonly Mock<IPaymentService> _paymentService;
        private readonly Mock<IShoppingCartUtility> _shoppingCartUtility;
        private readonly Mock<IMSCPkDispenserPublicKey> _mSCPkDispenserPublicKey;
        private readonly Mock<IPKDispenserService> _pKDispenserService;
        private readonly Mock<IFlightShoppingProductsService> _flightShoppingProductsServices;
        private readonly Mock<IETCUtility> _eTCUtility;
        private readonly Mock<IMSCShoppingSessionHelper> _shoppingSessionHelper;
        private readonly Mock<IMSCPageProductInfoHelper> _mSCPageProductInfoHelper;
        private readonly Mock<ICMSContentService> _cMSContentService;
        private readonly MSCCheckOutBusiness _mscCheckOutBusiness;
        private readonly Mock<ICustomerPreferencesService> _customerPreferencesService;
        private readonly Mock<IMSCCheckoutService> _mSCCheckoutService;
        private readonly Mock<ILogger> _logger6;
        private readonly IFlightShoppingService _flightShoppingService;
        private readonly Mock<ILogger> _logger7;
        private readonly Mock<IHeaders> _headers;
        private readonly Mock<IPassDetailService> _passDetailService;
        private readonly Mock<ILegalDocumentsForTitlesService> _legalDocumentsForTitlesService;
        private readonly Mock<IMerchandizingCSLService> _merchandizingCSLService;
        private ICacheLog<CachingService> _logger8;
        private Mock<IFeatureToggles> _featureToggles;

        public MSCCheckOutBusinessTest()
        {

            _configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appSettings.test.json", optional: false, reloadOnChange: true)
           .Build();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _sessionHelperService = new Mock<ISessionHelperService>();
            _logger = new Mock<ICacheLog<MSCCheckOutBusiness>>();
            _logger1 = new Mock<ILogger>();
            _sessionOnCloudService = new Mock<ISessionOnCloudService>();
            _resilientClient = new Mock<IResilientClient>();
            _logger2 = new Mock<ILogger>();
            _logger3 = new Mock<ILogger>();
            _logger4 = new Mock<ILogger>();
            _logger5 = new Mock<ILogger>();
            _logger6 = new Mock<ILogger>();
            _logger7 = new Mock<ILogger>();
            _headers = new Mock<IHeaders>();
            _applicationEnricher = new Mock<IApplicationEnricher>();
            _mSCShoppingSessionHelper = new Mock<IMSCShoppingSessionHelper>();
            _dynamoDBService = new Mock<IDynamoDBService>();
            _dataPowerFactory = new Mock<IDataPowerFactory>();
            _dPService = new Mock<IDPService>();
            _cachingService = new CachingService(_resilientClient.Object, _logger8, _configuration);
            _shoppingUtility = new Mock<IShoppingUtility>();
            _mSCFormsOfPayment = new Mock<IMSCFormsOfPayment>();
            _paymentService = new Mock<IPaymentService>();
            _pKDispenserService = new Mock<IPKDispenserService>();
            _flightShoppingProductsServices = new Mock<IFlightShoppingProductsService>();
            _eTCUtility = new Mock<IETCUtility>();
            _shoppingSessionHelper = new Mock<IMSCShoppingSessionHelper>();
            _legalDocumentsForTitlesService = new Mock<ILegalDocumentsForTitlesService>();
            _shoppingCartService = new Mock<IShoppingCartService>();
            _referencedataService = new Mock<IReferencedataService>();
            _checkoutUtility = new Mock<ICheckoutUtiliy>();
            _dataVaultService = new Mock<IDataVaultService>();
            _mSCPkDispenserPublicKey = new Mock<IMSCPkDispenserPublicKey>();
            _shoppingCartUtility = new Mock<IShoppingCartUtility>();
            _mSCPageProductInfoHelper = new Mock<IMSCPageProductInfoHelper>();
            _cMSContentService = new Mock<ICMSContentService>();
            _merchandizingCSLService = new Mock<IMerchandizingCSLService>();
            _featureToggles = new Mock<IFeatureToggles>();
            _mscCheckOutBusiness = new MSCCheckOutBusiness(_logger.Object, _checkoutUtility.Object, _configuration, _sessionHelperService.Object,
               _shoppingCartUtility.Object, _legalDocumentsForTitlesService.Object, _merchandizingCSLService.Object, _featureToggles.Object);

            SetupHttpContextAccessor();

        }
        private void SetupHttpContextAccessor()
        {
            string deviceId = "589d7852-14e7-44a9-b23b-a6db36657579";
            string applicationId = "2";
            string appVersion = "4.1.29";
            string transactionId = "589d7852-14e7-44a9-b23b-a6db36657579|8f46e040-a200-495c-83ca-4fca2d7175fb";
            string languageCode = "en-US";
            string sessionId = "17C979E184CC495EA083D45F4DD9D19D";
            var guid = Guid.NewGuid().ToString();
            var context = new DefaultHttpContext();
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
        public static string GetFileContent(string fileName)
        {
            fileName = string.Format("..\\..\\..\\TestData\\{0}", fileName);
            var path = Path.IsPathRooted(fileName) ? fileName : Path.GetRelativePath(Directory.GetCurrentDirectory(), fileName);
            return File.ReadAllText(path);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.CheckOut_Test), MemberType = typeof(TestDataGenerator))]
        public void CheckOut_Test(MOBCheckOutRequest mOBCheckOutRequest, MOBShoppingCart mOBShoppingCart, Reservation reservation, MOBApplyPromoCodeResponse mOBApplyPromoCodeResponse, MOBCSLContentMessagesResponse mOBCSLContentMessagesResponse, Session session, MOBCheckOutResponse mOBCheckOutResponse)
        {

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _sessionHelperService.Setup(p => p.GetSession<MOBApplyPromoCodeResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBApplyPromoCodeResponse);

            _cMSContentService.Setup(p => p.GetSDLContentByGroupName<MOBCSLContentMessagesResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mOBCSLContentMessagesResponse);

            _checkoutUtility.Setup(p => p.GetValidateSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(session);

            _shoppingCartUtility.Setup(p => p.ReservationToShoppingCart_DataMigration(It.IsAny<MOBSHOPReservation>(), It.IsAny<MOBShoppingCart>(), It.IsAny<MOBRequest>(), It.IsAny<bool>(), It.IsAny<Session>())).ReturnsAsync(mOBShoppingCart);

            _checkoutUtility.Setup(p => p.CheckOut(It.IsAny<MOBCheckOutRequest>())).ReturnsAsync(mOBCheckOutResponse);


            //_dPService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Token");

            //Act
            var result = _mscCheckOutBusiness.CheckOut(mOBCheckOutRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);

        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.CheckOut_Test1), MemberType = typeof(TestDataGenerator))]
        public void CheckOut_Test1(MOBCheckOutRequest mOBCheckOutRequest, MOBShoppingCart mOBShoppingCart, Reservation reservation, MOBApplyPromoCodeResponse mOBApplyPromoCodeResponse, MOBCSLContentMessagesResponse mOBCSLContentMessagesResponse, Session session, MOBCheckOutResponse mOBCheckOutResponse)
        {

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _sessionHelperService.Setup(p => p.GetSession<MOBApplyPromoCodeResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBApplyPromoCodeResponse);

            _cMSContentService.Setup(p => p.GetSDLContentByGroupName<MOBCSLContentMessagesResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mOBCSLContentMessagesResponse);

            _checkoutUtility.Setup(p => p.GetValidateSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(session);

            _shoppingCartUtility.Setup(p => p.ReservationToShoppingCart_DataMigration(It.IsAny<MOBSHOPReservation>(), It.IsAny<MOBShoppingCart>(), It.IsAny<MOBRequest>(), It.IsAny<bool>(), It.IsAny<Session>())).ReturnsAsync(mOBShoppingCart);

            _checkoutUtility.Setup(p => p.CheckOut(It.IsAny<MOBCheckOutRequest>())).ReturnsAsync(mOBCheckOutResponse);


            //_dPService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Token");

            //Act
            var result = _mscCheckOutBusiness.CheckOut(mOBCheckOutRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);

        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.CheckOut_Test2), MemberType = typeof(TestDataGenerator))]
        public void CheckOut_Test2(MOBCheckOutRequest mOBCheckOutRequest, MOBShoppingCart mOBShoppingCart, Reservation reservation, MOBApplyPromoCodeResponse mOBApplyPromoCodeResponse, MOBCSLContentMessagesResponse mOBCSLContentMessagesResponse, Session session, MOBCheckOutResponse mOBCheckOutResponse)
        {

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _sessionHelperService.Setup(p => p.GetSession<MOBApplyPromoCodeResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBApplyPromoCodeResponse);

            _cMSContentService.Setup(p => p.GetSDLContentByGroupName<MOBCSLContentMessagesResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mOBCSLContentMessagesResponse);

            _checkoutUtility.Setup(p => p.GetValidateSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(session);

            _shoppingCartUtility.Setup(p => p.ReservationToShoppingCart_DataMigration(It.IsAny<MOBSHOPReservation>(), It.IsAny<MOBShoppingCart>(), It.IsAny<MOBRequest>(), It.IsAny<bool>(), It.IsAny<Session>())).ReturnsAsync(mOBShoppingCart);

            _checkoutUtility.Setup(p => p.CheckOut(It.IsAny<MOBCheckOutRequest>())).ReturnsAsync(mOBCheckOutResponse);


            //_dPService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Token");

            //Act
            var result = _mscCheckOutBusiness.CheckOut(mOBCheckOutRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);

        }


        [Theory]
        [MemberData(nameof(TestDataGenerator.CheckOut_Flow), MemberType = typeof(TestDataGenerator))]
        public void CheckOut_Flow(MOBCheckOutRequest mOBCheckOutRequest, MOBShoppingCart mOBShoppingCart, Reservation reservation, MOBApplyPromoCodeResponse mOBApplyPromoCodeResponse, MOBCSLContentMessagesResponse mOBCSLContentMessagesResponse, Session session, MOBCheckOutResponse mOBCheckOutResponse)
        {

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _sessionHelperService.Setup(p => p.GetSession<MOBApplyPromoCodeResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBApplyPromoCodeResponse);

            _cMSContentService.Setup(p => p.GetSDLContentByGroupName<MOBCSLContentMessagesResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mOBCSLContentMessagesResponse);

            _checkoutUtility.Setup(p => p.GetValidateSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(session);

            _shoppingCartUtility.Setup(p => p.ReservationToShoppingCart_DataMigration(It.IsAny<MOBSHOPReservation>(), It.IsAny<MOBShoppingCart>(), It.IsAny<MOBRequest>(), It.IsAny<bool>(), It.IsAny<Session>())).ReturnsAsync(mOBShoppingCart);

            _checkoutUtility.Setup(p => p.CheckOut(It.IsAny<MOBCheckOutRequest>())).ReturnsAsync(mOBCheckOutResponse);


            //_dPService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Token");

            //Act
            var result = _mscCheckOutBusiness.CheckOut(mOBCheckOutRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);

        }
    }
}
