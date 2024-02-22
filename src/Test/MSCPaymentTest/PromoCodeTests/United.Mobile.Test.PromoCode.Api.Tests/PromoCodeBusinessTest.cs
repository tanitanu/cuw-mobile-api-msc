using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Polly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Common.Helper.MSCPayment.Interfaces;
using United.Definition;
using United.Definition.FormofPayment;
using United.Definition.Shopping;
using United.Ebs.Logging.Enrichers;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.MSCPayment.Interfaces;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Mobile.Model.Common;
using United.Mobile.PromoCode.Domain;
using United.Persist.Definition.SeatChange;
using United.Persist.Definition.Shopping;
using United.Service.Presentation.PersonalizationResponseModel;
using United.Services.FlightShopping.Common.Cart;
using United.Services.FlightShopping.Common.DisplayCart;
using United.Services.FlightShopping.Common.FlightReservation;
using United.Utility.Helper;
using United.Utility.Http;
using Xunit;
using ProfileResponse = United.Persist.Definition.Shopping.ProfileResponse;

namespace United.Mobile.Test.PromoCode.Api.Tests
{
    public class PromoCodeBusinessTest
    {
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<ICacheLog<PromoCodeBusiness>> _logger;
        private readonly IConfiguration _configuration;
        private readonly Mock<IHeaders> _headers;
        private readonly Mock<ISessionHelperService> _sessionHelperService;
        private readonly Mock<ILogger> _logger1;
        private readonly Mock<ISessionOnCloudService> _sessionOnCloudService;
        private readonly Mock<IApplicationEnricher> _applicationEnricher;
        private readonly Mock<IMSCShoppingSessionHelper> _mSCShoppingSessionHelper;
        private readonly Mock<IDynamoDBService> _dynamoDBService;
        private readonly Mock<ILogger> _logger2;
        private readonly Mock<IDataPowerFactory> _dataPowerFactory;
        private readonly Mock<ICacheLog<DataPowerFactory>> _logger18;
        private readonly Mock<IDPService> _dPService;
        private readonly Mock<ICachingService> _cachingService;
        private readonly Mock<ILogger> _logger3;
        private readonly ICachingService _cachingService1;
        private readonly Mock<ICacheLog<CachingService>> _logger19;
        private readonly Mock<IShoppingUtility> _shoppingUtility;
        private readonly Mock<ILogger> _logger4;
        private readonly Mock<ILogger> _logger5;
        private readonly Mock<IMSCFormsOfPayment> _mSCFormsOfPayment;
        private readonly Mock<ILogger> _logger6;
        private readonly Mock<IPaymentService> _paymentService;
        private readonly Mock<ILogger> _logger7;
        private readonly Mock<ILogger> _logger8;
        private readonly Mock<IMSCPkDispenserPublicKey> _mSCPkDispenserPublicKey;
        private readonly Mock<IPKDispenserService> _pKDispenserService;
        private readonly Mock<ILogger> _logger9;
        private readonly Mock<IFlightShoppingProductsService> _flightShoppingProductsServices;
        private readonly Mock<ILogger> _logger10;
        private readonly Mock<IETCUtility> _eTCUtility;
        private readonly Mock<ILegalDocumentsForTitlesService> _legalDocumentsForTitlesService;
        private readonly Mock<ILogger> _logger11;
        private readonly Mock<IShoppingCartService> _shoppingCartService;
        private readonly Mock<ILogger> _logger12;
        private readonly Mock<IMSCPageProductInfoHelper> _mSCPageProductInfoHelper;
        private readonly Mock<ICMSContentService> _cMSContentService;
        private readonly Mock<ILogger> _logger13;
        private readonly Mock<IGetTermsandCondtionsService> _getTermsandCondtionsService;
        private readonly Mock<ILogger> _logger14;
        private readonly Mock<IGetCCEContentService> _getCCEContentService;
        private readonly PromoCodeBusiness _promoCodeBusiness;
        private readonly Mock<ILogger> _logger15;
        private ICacheLog<CachingService> _logger16;
        private Mock<ICacheLog<PromoCodeBusiness>> logger;
        private readonly Mock<IShoppingCartUtility> _shoppingCartUtility;
        private readonly IShoppingCartUtility _shoppingCartUtility1;
        private readonly Mock<ICacheLog<ShoppingCartUtility>> _logger17;
        private readonly IResilientClient _resilientClient;
        private readonly HttpClient _httpClient;
        private readonly IAsyncPolicy _policyWrap;
        private readonly string _baseUrl;
        private readonly Mock<IFeatureSettings> _featureSettings;
        private readonly Mock<IFeatureToggles> _featureToggles;


        public PromoCodeBusinessTest()
        {

            _configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appSettings.test.json", optional: false, reloadOnChange: true)
           .Build();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _sessionHelperService = new Mock<ISessionHelperService>();
            _logger = new Mock<ICacheLog<PromoCodeBusiness>>();
            _logger1 = new Mock<ILogger>();
            _sessionOnCloudService = new Mock<ISessionOnCloudService>();
            _logger2 = new Mock<ILogger>();
            _logger3 = new Mock<ILogger>();
            _logger4 = new Mock<ILogger>();
            _logger5 = new Mock<ILogger>();
            _logger6 = new Mock<ILogger>();
            _logger7 = new Mock<ILogger>();
            _logger8 = new Mock<ILogger>();
            _logger9 = new Mock<ILogger>();
            _logger10 = new Mock<ILogger>();
            _logger11 = new Mock<ILogger>();
            _logger12 = new Mock<ILogger>();
            _logger13 = new Mock<ILogger>();
            _logger14 = new Mock<ILogger>();
            _logger15 = new Mock<ILogger>();
            _logger17 = new Mock<ICacheLog<ShoppingCartUtility>>();
            _logger18 = new Mock<ICacheLog<DataPowerFactory>>();
            _logger19 = new Mock<ICacheLog<CachingService>>();
            _headers = new Mock<IHeaders>();
            _applicationEnricher = new Mock<IApplicationEnricher>();
            _mSCShoppingSessionHelper = new Mock<IMSCShoppingSessionHelper>();
            _dynamoDBService = new Mock<IDynamoDBService>();
            _dataPowerFactory = new Mock<IDataPowerFactory>();
            _dPService = new Mock<IDPService>();
            _shoppingUtility = new Mock<IShoppingUtility>();
            _mSCFormsOfPayment = new Mock<IMSCFormsOfPayment>();
            _paymentService = new Mock<IPaymentService>();
            _mSCPkDispenserPublicKey = new Mock<IMSCPkDispenserPublicKey>();
            _pKDispenserService = new Mock<IPKDispenserService>();
            _flightShoppingProductsServices = new Mock<IFlightShoppingProductsService>();
            _eTCUtility = new Mock<IETCUtility>();
            _legalDocumentsForTitlesService = new Mock<ILegalDocumentsForTitlesService>();
            _shoppingCartService = new Mock<IShoppingCartService>();
            _shoppingCartUtility = new Mock<IShoppingCartUtility>();
            _mSCPageProductInfoHelper = new Mock<IMSCPageProductInfoHelper>();
            _cMSContentService = new Mock<ICMSContentService>();
            _getTermsandCondtionsService = new Mock<IGetTermsandCondtionsService>();
            _getCCEContentService = new Mock<IGetCCEContentService>();
            _cachingService = new Mock<ICachingService>();

            _shoppingCartUtility1 = new ShoppingCartUtility(_configuration, _headers.Object, _logger17.Object, _sessionHelperService.Object, _cMSContentService.Object, _legalDocumentsForTitlesService.Object, _mSCPageProductInfoHelper.Object, _cachingService.Object, _featureSettings.Object, _featureToggles.Object);

            _resilientClient = new ResilientClient(_baseUrl);

            _cachingService1 = new CachingService(_resilientClient, _logger19.Object, _configuration);
            _featureSettings = new Mock<IFeatureSettings>();

            _promoCodeBusiness = new PromoCodeBusiness(_logger.Object, _configuration, _headers.Object, _sessionHelperService.Object,
                _mSCShoppingSessionHelper.Object, _shoppingUtility.Object, _mSCFormsOfPayment.Object, _dPService.Object,
                _flightShoppingProductsServices.Object, _eTCUtility.Object, _shoppingCartService.Object, _shoppingCartUtility.Object,
                _getTermsandCondtionsService.Object, _getCCEContentService.Object, _cMSContentService.Object, _legalDocumentsForTitlesService.Object,
                _mSCPkDispenserPublicKey.Object, _mSCPageProductInfoHelper.Object, _cachingService.Object, _featureSettings.Object, _featureToggles.Object);

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
        [MemberData(nameof(TestDataGenerator.ApplyPromoCode_Test), MemberType = typeof(TestDataGenerator))]
        public void ApplyPromoCode_Test(MOBApplyPromoCodeRequest mOBApplyPromoCodeRequest, MOBShoppingCart mOBShoppingCart, MOBCSLContentMessagesResponse mOBCSLContentMessagesResponse, LoadReservationAndDisplayCartResponse loadReservationAndDisplayCartResponse, Reservation reservation, ProfileResponse profileResponse, FlightReservationResponse flightReservationResponse, MOBFOPTravelerCertificateResponse mOBFOPTravelerCertificateResponse, Session session, MOBFOPResponse mOBFOPResponse, MOBApplyPromoCodeResponse response, List<FormofPaymentOption> formofPaymentOptions)
        {

            var _dataPowerFactory = new DataPowerFactory(_configuration, _sessionHelperService.Object);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _cMSContentService.Setup(p => p.GetSDLContentByGroupName<MOBCSLContentMessagesResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mOBCSLContentMessagesResponse);

            _shoppingCartService.Setup(p => p.GetCartInformation<LoadReservationAndDisplayCartResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(loadReservationAndDisplayCartResponse);

            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _sessionHelperService.Setup(p => p.GetSession<ProfileResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(profileResponse);

            _shoppingCartService.Setup(p => p.RegisterOrRemoveCoupon<FlightReservationResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(flightReservationResponse);

            _sessionHelperService.Setup(p => p.GetSession<MOBFOPTravelerCertificateResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBFOPTravelerCertificateResponse);

            _mSCShoppingSessionHelper.Setup(p => p.GetBookingFlowSession(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(session);

            _mSCPkDispenserPublicKey.Setup(p => p.GetCachedOrNewpkDispenserPublicKey(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<United.Mobile.Model.Common.MOBItem>>(), It.IsAny<string>())).ReturnsAsync(mOBFOPResponse.PkDispenserPublicKey);

            _dPService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Token");

            _shoppingCartUtility.Setup(p => p.ConfirmationPageProductInfo(It.IsAny<FlightReservationResponse>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<MOBApplication>(), It.IsAny<SeatChangeState>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<List<SCProductContext>>(), It.IsAny<List<string>>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart.Products);

            _sessionHelperService.Setup(p => p.GetSession<Session>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(session);

            DisplayCartResponse displayCartResponse = new DisplayCartResponse
            {
                CartId = "8729F996-9D2D-4059-AEAA-39211ABB78F6",
                Status = new Services.FlightShopping.Common.StatusType
                {
                  
                },
                Errors = new List<Services.FlightShopping.Common.ErrorInfo>
                {
                    new Services.FlightShopping.Common.ErrorInfo()
                    {
                        MajorCode = "2.1.2",
                        MinorCode = "1.2.5",
                        MajorDescription = "codes",
                        Message = "try again"
                    }
                },
                MerchProductOffer = new Service.Presentation.ProductResponseModel.ProductOffer
                {
                    Response = new Service.Presentation.CommonModel.ServiceResponse()
                    {
                        CartID = "8729F996-9D2D-4059-AEAA-39211ABB78F6",
                        DataCenter = "ABC",
                        LoyaltyProgramMemberID = "2",
                        ProcessTime = 4.0,
                        TransactionID = "446e943d-d749-4f2d-8bf2-e0ba5d6da685|a43aa991-9d57-40f0-9b83-79f34d880e7c",
                        RecordLocator = "xyz"
                    }
                },
                Characteristics = new System.Collections.ObjectModel.Collection<Service.Presentation.CommonModel.Characteristic>
                {
                    new Service.Presentation.CommonModel.Characteristic()
                    {
                         Code = "LoyaltyID",
                         Description = "xyz"
                    }
                }
                
            };

          var response1 =  JsonConvert.SerializeObject(displayCartResponse);

            _flightShoppingProductsServices.Setup(p => p.GetTripInsuranceInfo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response1);

            _eTCUtility.Setup(p => p.GetReservationFromPersist(It.IsAny<MOBSHOPReservation>(), It.IsAny<Session>())).ReturnsAsync(response.Reservation);

            _shoppingCartUtility.Setup(p => p.ReservationToShoppingCart_DataMigration(It.IsAny<MOBSHOPReservation>(), It.IsAny<MOBShoppingCart>(), It.IsAny<MOBRequest>(), It.IsAny<bool>(), It.IsAny<Session>())).ReturnsAsync(mOBShoppingCart);

            _mSCFormsOfPayment.Setup(p => p.GetEligibleFormofPayments(It.IsAny<MOBRequest>(), It.IsAny<Session>(), It.IsAny<MOBShoppingCart>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MOBSHOPReservation>(), It.IsAny<bool>(), It.IsAny<SeatChangeState>())).Returns(Task.FromResult((formofPaymentOptions, true)));

            //_eTCUtility.Setup(p => p.GetCultureInfo(It.IsAny<string>())).Returns(mOBSHOPPrice);



            var promoCodeBusiness = new PromoCodeBusiness(_logger.Object, _configuration, _headers.Object, _sessionHelperService.Object,
                _mSCShoppingSessionHelper.Object, _shoppingUtility.Object, _mSCFormsOfPayment.Object, _dPService.Object,
                _flightShoppingProductsServices.Object, _eTCUtility.Object, _shoppingCartService.Object, _shoppingCartUtility.Object,
                _getTermsandCondtionsService.Object, _getCCEContentService.Object, _cMSContentService.Object, _legalDocumentsForTitlesService.Object,
                _mSCPkDispenserPublicKey.Object, _mSCPageProductInfoHelper.Object, _cachingService1, _featureSettings.Object, _featureToggles.Object);


            //Act
            var result = promoCodeBusiness.ApplyPromoCode(mOBApplyPromoCodeRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);

        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.ApplyPromoCode_Test1), MemberType = typeof(TestDataGenerator))]
        public void ApplyPromoCode_Test1(MOBApplyPromoCodeRequest mOBApplyPromoCodeRequest, MOBShoppingCart mOBShoppingCart, MOBCSLContentMessagesResponse mOBCSLContentMessagesResponse, LoadReservationAndDisplayCartResponse loadReservationAndDisplayCartResponse, Reservation reservation, ProfileResponse profileResponse, FlightReservationResponse flightReservationResponse, MOBFOPTravelerCertificateResponse mOBFOPTravelerCertificateResponse, Session session, MOBFOPResponse mOBFOPResponse)
        {

            var _dataPowerFactory = new DataPowerFactory(_configuration, _sessionHelperService.Object);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _cMSContentService.Setup(p => p.GetSDLContentByGroupName<MOBCSLContentMessagesResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mOBCSLContentMessagesResponse);

            _shoppingCartService.Setup(p => p.GetCartInformation<LoadReservationAndDisplayCartResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(loadReservationAndDisplayCartResponse);

            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _sessionHelperService.Setup(p => p.GetSession<ProfileResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(profileResponse);

            _shoppingCartService.Setup(p => p.RegisterOrRemoveCoupon<FlightReservationResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(flightReservationResponse);

            _sessionHelperService.Setup(p => p.GetSession<MOBFOPTravelerCertificateResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBFOPTravelerCertificateResponse);

            _mSCShoppingSessionHelper.Setup(p => p.GetBookingFlowSession(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(session);

            _mSCPkDispenserPublicKey.Setup(p => p.GetCachedOrNewpkDispenserPublicKey(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<United.Mobile.Model.Common.MOBItem>>(), It.IsAny<string>())).ReturnsAsync(mOBFOPResponse.PkDispenserPublicKey);

            _dPService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Token");


            _flightShoppingProductsServices.Setup(p => p.ApplyCSLMilesPlusMoneyOptions(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("{\"CallTimeDomainFltRes\":\"63773450634.50\",\"CallTimeDomainShop\":\"0.74\",\"CartId\":\"04C1438A-302A-4B70-9D88-54A3880C8ED7\",\"SessionId\":\"ba14cdb6-ae98-4d46-93f8-210133bf4b1b\",\"LastBBXSolutionSetId\":\"04CciOprjqsIRu0ICE6xPL4\",\"LastCallDateTime\":\"20211125 152354\",\"ServerName\":\"flightshopping-dev-67749cbc9c-rmz5x\",\"Status\":1,\"DisplayCart\":{\"CartId\":\"04C1438A-302A-4B70-9D88-54A3880C8ED7\",\"CountryCode\":\"US\",\"LangCode\":\"en-US\",\"BookingCutOffMinutes\":60,\"BookingCutOffWarningMinutes\":30,\"GrandTotal\":500.2,\"SearchType\":1,\"MilitaryTravel\":0,\"DisplayPrices\":[{\"Amount\":471,\"Count\":1,\"Currency\":\"USD\",\"Description\":\"Adult (18-64)\",\"Type\":\"TravelerPrice\",\"SubItems\":[{\"Type\":\"AY\",\"Amount\":5.6,\"Currency\":\"USD\",\"Description\":\"September 11th Security Fee\",\"Key\":\"0\",\"Value\":\"Tax\",\"Count\":0,\"OriginalPrice\":0},{\"Type\":\"US\",\"Amount\":19.1,\"Currency\":\"USD\",\"Description\":\"U.S. Transportation Tax\",\"Key\":\"1\",\"Value\":\"Tax\",\"Count\":0,\"OriginalPrice\":0},{\"Type\":\"XF\",\"Amount\":4.5,\"Currency\":\"USD\",\"Description\":\"U.S. Passenger Facility Charge\",\"Key\":\"2\",\"Value\":\"Tax\",\"Count\":0,\"OriginalPrice\":0}],\"PaxTypeCode\":\"ADT\",\"Waived\":false,\"PricingPaxType\":\"ADT\",\"ResidualAmount\":0},{\"Amount\":500.2,\"Count\":0,\"Currency\":\"USD\",\"Description\":\"Total\",\"Type\":\"Total\",\"Waived\":false,\"ResidualAmount\":0},{\"Amount\":550.2,\"Count\":0,\"Currency\":\"USD\",\"Description\":\"Total\",\"Type\":\"TAX\",\"Waived\":false,\"ResidualAmount\":0},{\"Amount\":650.2,\"Count\":0,\"Currency\":\"USD\",\"Description\":\"Total\",\"Type\":\"ORIGINALTAXTOTAL\",\"Waived\":false,\"ResidualAmount\":0}],\"DisplayDetails\":[],\"bookingDetails\":{\"solution\":[{\"ext\":{\"bookingCodeCount\":0,\"fareFamily\":\"bookingDetails\",\"fareFlavor\":\"vanilla\",\"isOverbooked\":0,\"userSelection\":\"ECO-BASIC^0~\"},\"passenger\":[{\"ptc\":[\"ADT\",\"AUTOAC\",\"AUTOSP\"],\"id\":\"0\",\"age\":\"25\"}],\"pricing\":[{\"tax\":[{\"salePrice\":{\"amount\":5.6,\"currency\":\"USD\"},\"code\":\"AY\",\"status\":\"APPLIED\"},{\"salePrice\":{\"amount\":170,\"currency\":\"USD\"},\"code\":\"YQ\",\"status\":\"APPLIED\"},{\"salePrice\":{\"amount\":19.1,\"currency\":\"USD\"},\"code\":\"US\",\"status\":\"APPLIED\"},{\"salePrice\":{\"amount\":4.5,\"currency\":\"USD\"},\"code\":\"XF\",\"status\":\"APPLIED\"}],\"PaxPricingIndex\":0,\"ext\":{\"bookingCodeCount\":0,\"price\":{\"amount\":471,\"currency\":\"USD\"},\"taxTotalNoYQYR\":{\"totalSalePrice\":{\"amount\":29.2,\"currency\":\"USD\"}},\"referencePrice\":{\"amount\":471,\"currency\":\"USD\"},\"allPassengerPrice\":{\"amount\":500.2,\"currency\":\"USD\"},\"miles\":\"301\",\"endorsements\":[\"NONREF/NOASR\"],\"taxTotal\":{\"totalSalePrice\":{\"amount\":29.2,\"currency\":\"USD\"}},\"yqTaxTotal\":{\"totalSalePrice\":{\"amount\":170,\"currency\":\"USD\"}},\"isOverbooked\":0},\"saleFareTotal\":{\"amount\":301,\"currency\":\"USD\"},\"salePrice\":{\"amount\":500.2,\"currency\":\"USD\"},\"saleTaxTotal\":{\"amount\":199.2,\"currency\":\"USD\"},\"cocFareTotal\":{\"amount\":301,\"currency\":\"USD\"},\"additionalCollectionAwardPoints\":0,\"previousAwardPointsFareTotal\":0,\"refundAwardPoints\":0,\"fareCalculation\":[{\"line\":[\"CHI UA LON 301.00VL700LGT NUC 301.00 END ROE 1.00 XT 19.10US 5.60AY 170.00YQ 4.50XF ORD4.5\"]}],\"fare\":[{\"destinationCity\":\"LON\",\"globalIndicator\":\"AT\",\"originCity\":\"CHI\",\"tag\":\"ONE-WAY-ONLY\",\"extendedFareCode\":\"VL700LGT\",\"carrier\":\"UA\",\"endorsement\":\"NONREF/NOASR\",\"ptc\":\"ADT\",\"isMerchEnginePriced\":false,\"rkey\":\"Ag6Bhyv3hcZGZ/GpWIn+zUPEZIkKos5lsfjsueJAoQAU\",\"basePrice\":\"USD301.00\",\"bookingInfo\":[{\"segment\":{\"bookingInfo\":[{\"bookingCode\":\"V\",\"PaxPriceIndex\":0,\"marriedSegmentIndex\":0}],\"isConnection\":false},\"PaxPriceIndex\":0,\"marriedSegmentIndex\":0}],\"ruleSummary\":{},\"characteristics\":[]}],\"note\":[{},{}],\"paxCount\":1,\"passenger\":[{\"ptc\":[\"ADT\",\"AUTOAC\",\"AUTOSP\"],\"id\":\"0\",\"age\":\"25\"}],\"availabilityStatus\":\"AVAILABLE\"}],\"id\":\"8pO5AvLzMp3Mj2W9QZZ2VM001\",\"slice\":[{\"duration\":\"480\",\"ext\":{\"bookingCodeCount\":0,\"warnings\":{\"type\":[\"OVERNIGHT\"]}},\"segment\":[{\"hash\":\"G55DxlBzdlBoVnlj\",\"leg\":[{\"arrival\":\"2021-11-26T07:55+00:00\",\"departure\":\"2021-11-25T17:55-06:00\",\"destination\":\"LHR\",\"origin\":\"ORD\",\"destinationTerminal\":\"2\",\"originTerminal\":\"1\",\"flight\":\"UA931\",\"hash\":\"LpBMGyyEg3xbHJqI\",\"cabinCount\":3}],\"availability\":{\"bookingCodeCount\":[{\"code\":\"J\",\"count\":\"9\"},{\"code\":\"JN\",\"count\":\"9\"},{\"code\":\"C\",\"count\":\"9\"},{\"code\":\"D\",\"count\":\"9\"},{\"code\":\"Z\",\"count\":\"9\"},{\"code\":\"ZN\",\"count\":\"9\"},{\"code\":\"P\",\"count\":\"9\"},{\"code\":\"PN\",\"count\":\"9\"},{\"code\":\"PZ\",\"count\":\"9\"},{\"code\":\"IN\",\"count\":\"9\"},{\"code\":\"I\",\"count\":\"9\"},{\"code\":\"O\",\"count\":\"9\"},{\"code\":\"ON\",\"count\":\"9\"},{\"code\":\"A\",\"count\":\"9\"},{\"code\":\"R\",\"count\":\"1\"},{\"code\":\"RN\",\"count\":\"1\"},{\"code\":\"Y\",\"count\":\"9\"},{\"code\":\"YN\",\"count\":\"9\"},{\"code\":\"B\",\"count\":\"9\"},{\"code\":\"M\",\"count\":\"9\"},{\"code\":\"E\",\"count\":\"9\"},{\"code\":\"U\",\"count\":\"9\"},{\"code\":\"H\",\"count\":\"9\"},{\"code\":\"HN\",\"count\":\"9\"},{\"code\":\"Q\",\"count\":\"9\"},{\"code\":\"V\",\"count\":\"9\"},{\"code\":\"W\",\"count\":\"9\"},{\"code\":\"S\",\"count\":\"9\"},{\"code\":\"T\",\"count\":\"9\"},{\"code\":\"L\",\"count\":\"9\"},{\"code\":\"K\",\"count\":\"9\"},{\"code\":\"G\",\"count\":\"0\"},{\"code\":\"N\",\"count\":\"9\"},{\"code\":\"XN\",\"count\":\"9\"},{\"code\":\"X\",\"count\":\"9\"}]},\"isConnection\":false}],\"index\":\"0\"}]}]},\"TravelOptions\":[{\"Status\":\"Refund\",\"Type\":\"Premium Access\",\"Key\":\"FARELOCK\"}],\"ProductsDirty\":true,\"ProductCode\":\"IBE\",\"Characteristics\":[{\"Code\":\"SHOPREQPETTRAVEL\",\"Value\":\"False\"},{\"Code\":\"SHOPREQPETCOUNT\",\"Value\":\"0\"},{\"Code\":\"RECENTSEARCHKEY\",\"Value\":\"ORDLHR11/25/2021\"},{\"Code\":\"DEVICEID\",\"Value\":\"postman-Android-new\"},{\"Code\":\"IS_FROM_FSCLOUD\",\"Value\":\"true\"}]},\"Errors\":[{\"MajorCode\":\"2.1.4\",\"MinorCode\":\"10049\"}],\"Reservation\":{\"Travelers\":[{\"Person\":{\"Key\":\"1.1\",\"DateOfBirth\":\"11/25/1996\",\"Type\":\"ADT\",\"InfantIndicator\":\"\",\"ReservationIndex\":\"0\",\"Age\":25,\"PricingPaxType\":\"ADT\"},\"LoyaltyProgramProfile\":{\"LoyaltyProgramCarrierCode\":\"UA\",\"LoyaltyProgramID\":\"UA\",\"LoyaltyProgramMemberTierLevel\":\"0\",\"LoyaltyProgramMemberTierDescription\":0},\"Tickets\":[{\"TicketingCarrierCode\":\"UA\"}]}],\"FlightSegments\":[{\"FlightSegment\":{\"DepartureAirport\":{\"IATACode\":\"ORD\",\"Name\":\"Chicago, IL, US (ORD)\",\"IATACountryCode\":{\"CountryCode\":\"US\"},\"StateProvince\":{\"StateProvinceCode\":\"IL\"}},\"ArrivalAirport\":{\"IATACode\":\"LHR\",\"Name\":\"London, GB (LHR)\",\"IATACountryCode\":{\"CountryCode\":\"GB\"},\"StateProvince\":{\"StateProvinceCode\":\"\"}},\"DepartureDateTime\":\"2021-11-25 17:55\",\"ArrivalDateTime\":\"2021-11-26 07:55\",\"FlightNumber\":\"931\",\"OperatingAirlineCode\":\"UA\",\"Equipment\":{\"Model\":{\"Description\":\"Boeing 767-300\",\"Fleet\":\"763\"},\"CabinCount\":3},\"MarketedFlightSegment\":[{\"MarketingAirlineCode\":\"UA\",\"FlightNumber\":\"931\"}],\"MarriageGroup\":\"N\",\"Distance\":3939,\"SegmentNumber\":1,\"BookingClasses\":[{\"Code\":\"V\",\"Cabin\":{\"Key\":\"UE\",\"Name\":\"United Economy\",\"Layout\":\"3\"}}],\"FlightSegmentType\":\"NN\",\"Message\":[{\"Code\":\"ARRDAYDIFF\"},{\"Text\":\"VL700LGT\",\"Code\":\"BASIS_CODE\"},{\"Text\":\"False\",\"Code\":\"PETS_DISALLOWED\"},{\"Text\":\"United Airlines\",\"Code\":\"OPERATINGCARRIERDESC\"},{\"Text\":\"OBY\",\"Code\":\"OB\"}],\"Characteristic\":[{\"Code\":\"PET_COUNT\",\"Value\":\"0\"},{\"Code\":\"ProductCode\",\"Value\":\"IBE\"}],\"IsInternational\":\"True\",\"AvailBookingClasses\":[{\"Code\":\"J\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"JN\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"C\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"D\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"Z\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"ZN\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"P\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"PN\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"PZ\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"IN\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"I\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"O\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"ON\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"A\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"R\",\"Counts\":{\"Value\":\"1\"}},{\"Code\":\"RN\",\"Counts\":{\"Value\":\"1\"}},{\"Code\":\"Y\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"YN\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"B\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"M\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"E\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"U\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"H\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"HN\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"Q\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"V\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"W\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"S\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"T\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"L\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"K\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"G\",\"Counts\":{\"Value\":\"0\"}},{\"Code\":\"N\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"XN\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"X\",\"Counts\":{\"Value\":\"9\"}}],\"UpgradeEligibilityStatus\":0,\"UpgradeVisibilityType\":0,\"InstantUpgradable\":false},\"IsConnection\":\"False\",\"Characteristic\":[{\"Code\":\"SELECTED_FARE_TYPE\",\"Value\":\"ECO-BASIC\"}],\"SegmentNumber\":1,\"TripNumber\":\"1\"}],\"Prices\":[{\"BasePrice\":[{\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":301}],\"Taxes\":[{\"Code\":\"AY\",\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":5.6,\"Description\":\"September 11th Security Fee\",\"Status\":\"APPLIED\"},{\"Code\":\"YQ\",\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":170,\"Description\":\"International Surcharge\",\"Status\":\"APPLIED\"},{\"Code\":\"US\",\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":19.1,\"Description\":\"U.S. Transportation Tax\",\"Status\":\"APPLIED\"},{\"Code\":\"XF\",\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":4.5,\"Description\":\"U.S. Passenger Facility Charge\",\"Status\":\"APPLIED\"}],\"Surcharges\":[],\"Fees\":[],\"Totals\":[{\"Name\":\"TaxTotal\",\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":199.2},{\"Name\":\"GrandTotalForMileage\",\"Currency\":{\"Code\":\"MIL\"},\"Amount\":0},{\"Name\":\"GrandTotalForCurrency\",\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":500.2}],\"BasePriceEquivalent\":{\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":301},\"Type\":{\"Key\":\"Revenue\",\"Description\":\"fareFlavor\",\"Value\":\"ADT\"},\"Rules\":[{\"Description\":\"NONREF/NOASR\"}],\"FareCalculation\":\"CHI UA LON 301.00VL700LGT NUC 301.00 END ROE 1.00 XT 19.10US 5.60AY 170.00YQ 4.50XF ORD4.5\",\"FareComponents\":[{\"AirlineCode\":\"UA\",\"OriginCity\":\"CHI\",\"DestinationCity\":\"LON\",\"BasisCodes\":[\"VL700LGT\"],\"TripType\":\"ONE-WAY-ONLY\",\"BasePrice\":[{\"Currency\":{\"Code\":\"USD\"},\"Amount\":301}],\"DestinationAirportCode\":\"LON\",\"OriginAirportCode\":\"CHI\",\"SequenceNumber\":1,\"PassengerTypeCode\":\"ADT\",\"Characteristic\":[]}],\"Count\":{\"CountType\":\"Pax Count\",\"Value\":\"1\"},\"FareType\":0,\"PriceFlightSegments\":[{\"FareBasisCode\":\"VL700LGT\",\"TicketDesignator\":\"\",\"LOFNumber\":1,\"FareComponentSequenceNumber\":1,\"GlobalIndicator\":\"AT\",\"DepartureAirport\":{\"IATACode\":\"ORD\",\"ShortName\":\"ORD\"},\"ArrivalAirport\":{\"IATACode\":\"LHR\",\"ShortName\":\"LHR\"},\"DepartureDateTime\":\"2021-11-25 17:55\",\"ArrivalDateTime\":\"2021-11-26 07:55\",\"FlightNumber\":\"931\",\"MarketedFlightSegment\":[{\"MarketingAirlineCode\":\"UA\"}],\"MarriageGroup\":\"N\",\"SegmentNumber\":1,\"BookingClasses\":[{\"Code\":\"V\"}],\"Legs\":[{\"Equipment\":{\"Model\":{\"Fleet\":\"763\"}},\"CabinCount\":3}],\"UpgradeEligibilityStatus\":0,\"UpgradeVisibilityType\":0,\"InstantUpgradable\":false,\"AuxillaryText\":\"False\"}],\"PassengerTypeCode\":\"ADT\",\"Characteristics\":[],\"PassengerIDs\":{\"Key\":\"1.1\",\"Description\":\"ADT\"}}],\"Type\":[{\"Key\":\"NEW\"},{\"Key\":\"REVENUE\"}],\"Characteristic\":[{\"Code\":\"Refundable\",\"Value\":\"False\"},{\"Code\":\"ContentLookupID\",\"Value\":\"Messages:123\"},{\"Code\":\"BBXID\",\"Value\":\"07cUmDQxGBhQ4Cx4z0aXoh\"},{\"Code\":\"StationCode\",\"Value\":\"HOU\"},{\"Code\":\"TRIP_TYPE\",\"Value\":\"OW\",\"Description\":\"TRIP_TYPE\"},{\"Code\":\"ProductCode\",\"Value\":\"IBE\"}],\"NumberInParty\":1,\"GUID\":{\"ID\":\"04C1438A-302A-4B70-9D88-54A3880C8ED7\"},\"PointOfSale\":{\"Country\":{\"CountryCode\":\"US\"},\"CurrencyCode\":\"USD\"},\"CartId\":\"04C1438A-302A-4B70-9D88-54A3880C8ED7\"},\"LastResultId\":\"07cUmDQxGBhQ4Cx4z0aXoh\",\"Characteristics\":[{\"Code\":\"FLIGHTRECOMMENDATIONSCALL\",\"Value\":\"SHOPBOOKINGDETAILS\"}],\"Warnings\":[],\"Version\":\"1.1.142.0\"}");

            _shoppingCartService.Setup(p => p.RegisterFlights<FlightReservationResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(flightReservationResponse);

            var promoCodeBusiness = new PromoCodeBusiness(_logger.Object, _configuration, _headers.Object, _sessionHelperService.Object,
                _mSCShoppingSessionHelper.Object, _shoppingUtility.Object, _mSCFormsOfPayment.Object, _dPService.Object,
                _flightShoppingProductsServices.Object, _eTCUtility.Object, _shoppingCartService.Object, _shoppingCartUtility1,
                _getTermsandCondtionsService.Object, _getCCEContentService.Object, _cMSContentService.Object, _legalDocumentsForTitlesService.Object,
                _mSCPkDispenserPublicKey.Object, _mSCPageProductInfoHelper.Object, _cachingService.Object, _featureSettings.Object, _featureToggles.Object);



            //Act
            var result = promoCodeBusiness.ApplyPromoCode(mOBApplyPromoCodeRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);

        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.GetTermsandConditionsByPromoCode_Test), MemberType = typeof(TestDataGenerator))]
        public void GetTermsandConditionsByPromoCode_Test(MOBApplyPromoCodeRequest mOBApplyPromoCodeRequest, Session session)
        {

            _mSCShoppingSessionHelper.Setup(p => p.GetBookingFlowSession(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(session);

            _dPService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Token");


            //_httpClient.Setup(p => p.GetAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Token");
            // _resilientClient.Setup(p => p.PostHttpAsyncWithOptions(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>())).Returns();
            //Act
            var result = _promoCodeBusiness.GetTermsandConditionsByPromoCode(mOBApplyPromoCodeRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);


        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.GetTermsandConditionsByPromoCode_Test1), MemberType = typeof(TestDataGenerator))]
        public void GetTermsandConditionsByPromoCode_Test1(MOBApplyPromoCodeRequest mOBApplyPromoCodeRequest, Session session 
             )
        {

            //_mSCShoppingSessionHelper.Setup(p => p.GetBookingFlowSession(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(session);
            _sessionHelperService.Setup(p => p.GetSession<Session>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(session);
            _dPService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Token");

            //string cslresponse = JsonConvert.SerializeObject(sdlResponse);
            //_getCCEContentService.Setup(p => p.GetCCEContent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(cslresponse);

            //_httpClient.Setup(p => p.GetAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Token");
            // _resilientClient.Setup(p => p.PostHttpAsyncWithOptions(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>())).Returns();
            //Act
            var result = _promoCodeBusiness.GetTermsandConditionsByPromoCode(mOBApplyPromoCodeRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);


        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.RemovePromoCode_Test), MemberType = typeof(TestDataGenerator))]
        public void RemovePromoCode_Test(MOBApplyPromoCodeRequest mOBApplyPromoCodeRequest, Session session, MOBShoppingCart mOBShoppingCart, MOBApplyPromoCodeResponse mOBApplyPromoCodeResponse, Reservation reservation, FlightReservationResponse flightReservationResponse , List<FormofPaymentOption> formofPaymentOptions)
        {
            var _dataPowerFactory = new DataPowerFactory(_configuration, _sessionHelperService.Object);

            _mSCShoppingSessionHelper.Setup(p => p.GetBookingFlowSession(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(session);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _sessionHelperService.Setup(p => p.GetSession<MOBApplyPromoCodeResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBApplyPromoCodeResponse);

            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _dPService.Setup(p => p.GetAnonymousToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IConfiguration>())).ReturnsAsync("Bearer Token");

            _shoppingCartService.Setup(p => p.RegisterOrRemoveCoupon<FlightReservationResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(flightReservationResponse);

            _shoppingCartUtility.Setup(p => p.ConfirmationPageProductInfo(It.IsAny<FlightReservationResponse>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<MOBApplication>(), It.IsAny<SeatChangeState>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<List<SCProductContext>>(), It.IsAny<List<string>>(), It.IsAny<bool>(), It.IsAny<string>(),It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart.Products);

            _sessionHelperService.Setup(p => p.GetSession<Session>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(session);

            DisplayCartResponse displayCartResponse = new DisplayCartResponse
            {
                CartId = "8729F996-9D2D-4059-AEAA-39211ABB78F6",
                Status = new Services.FlightShopping.Common.StatusType
                {

                },
                Errors = new List<Services.FlightShopping.Common.ErrorInfo>
                {
                    new Services.FlightShopping.Common.ErrorInfo()
                    {
                        MajorCode = "2.1.2",
                        MinorCode = "1.2.5",
                        MajorDescription = "codes",
                        Message = "try again"
                    }
                },
                MerchProductOffer = new Service.Presentation.ProductResponseModel.ProductOffer
                {
                    Response = new Service.Presentation.CommonModel.ServiceResponse()
                    {
                        CartID = "8729F996-9D2D-4059-AEAA-39211ABB78F6",
                        DataCenter = "ABC",
                        LoyaltyProgramMemberID = "2",
                        ProcessTime = 4.0,
                        TransactionID = "446e943d-d749-4f2d-8bf2-e0ba5d6da685|a43aa991-9d57-40f0-9b83-79f34d880e7c",
                        RecordLocator = "xyz"
                    }
                },
                Characteristics = new System.Collections.ObjectModel.Collection<Service.Presentation.CommonModel.Characteristic>
                {
                    new Service.Presentation.CommonModel.Characteristic()
                    {
                         Code = "LoyaltyID",
                         Description = "xyz"
                    }
                }

            };

            var response1 = JsonConvert.SerializeObject(displayCartResponse);

            _flightShoppingProductsServices.Setup(p => p.GetTripInsuranceInfo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response1);

            _eTCUtility.Setup(p => p.GetReservationFromPersist(It.IsAny<MOBSHOPReservation>(), It.IsAny<Session>())).ReturnsAsync(mOBApplyPromoCodeResponse.Reservation);

            _shoppingCartUtility.Setup(p => p.ReservationToShoppingCart_DataMigration(It.IsAny<MOBSHOPReservation>(), It.IsAny<MOBShoppingCart>(), It.IsAny<MOBRequest>(), It.IsAny<bool>(), It.IsAny<Session>())).ReturnsAsync(mOBShoppingCart);

            _mSCFormsOfPayment.Setup(p => p.GetEligibleFormofPayments(It.IsAny<MOBRequest>(), It.IsAny<Session>(), It.IsAny<MOBShoppingCart>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MOBSHOPReservation>(), It.IsAny<bool>(), It.IsAny<SeatChangeState>())).Returns(Task.FromResult((formofPaymentOptions, true)));

            _shoppingUtility.Setup(p => p.GetGrandTotalPriceForShoppingCart(It.IsAny<bool>(), It.IsAny<FlightReservationResponse>(), It.IsAny<bool>(),It.IsAny<string>())).Returns(23.3456789);

            var promoCodeBusiness = new PromoCodeBusiness(_logger.Object, _configuration, _headers.Object, _sessionHelperService.Object,
                _mSCShoppingSessionHelper.Object, _shoppingUtility.Object, _mSCFormsOfPayment.Object, _dPService.Object,
                _flightShoppingProductsServices.Object, _eTCUtility.Object, _shoppingCartService.Object, _shoppingCartUtility.Object,
                _getTermsandCondtionsService.Object, _getCCEContentService.Object, _cMSContentService.Object, _legalDocumentsForTitlesService.Object,
                _mSCPkDispenserPublicKey.Object, _mSCPageProductInfoHelper.Object, _cachingService1, _featureSettings.Object, _featureToggles.Object);




            //Act
            var result = promoCodeBusiness.RemovePromoCode(mOBApplyPromoCodeRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);

        }
    }
}

