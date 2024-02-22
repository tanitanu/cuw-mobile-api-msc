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
using System.Threading.Tasks;
using United.Common.Helper;
using United.Common.Helper.MSCPayment.Interfaces;
using United.Common.Helper.Product;
using United.Definition;
using United.Definition.FormofPayment;
using United.Definition.SeatCSL30;
using United.Definition.Shopping;
using United.Definition.Shopping.Bundles;
using United.Definition.Shopping.UnfinishedBooking;
using United.Ebs.Logging.Enrichers;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Mobile.Model.Common;
using United.Mobile.Model.DynamoDb.Common;
using United.Mobile.Product.Domain;
using United.Mobile.TravelCredit.Domain;
using United.Persist.Definition.CCE;
using United.Persist.Definition.FOP;
using United.Persist.Definition.Merchandizing;
using United.Persist.Definition.SeatChange;
using United.Persist.Definition.Shopping;
using United.Service.Presentation.ProductModel;
using United.Service.Presentation.ReservationResponseModel;
using United.Service.Presentation.SecurityResponseModel;
using United.Services.FlightShopping.Common.Cart;
using United.Services.FlightShopping.Common.FlightReservation;
using United.Utility.Helper;
using United.Utility.Http;
using Xunit;
using ProfileResponse = United.Persist.Definition.Shopping.ProfileResponse;
using TheoryAttribute = Xunit.TheoryAttribute;


namespace United.Mobile.Test.Product.Api.Tests
{
    public class ProductBusinessTests
    {
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<ICacheLog<ProductBusiness>> _logger;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly Mock<IHeaders> _headers;
        private readonly Mock<ILogger> _logger1;
        private readonly Mock<IRequestEnricher> _requestEnricher;
        private readonly Mock<IMSCShoppingSessionHelper> _shoppingSessionHelper;
        private readonly Mock<ILogger> _logger2;
        private readonly Mock<ISessionHelperService> _sessionHelperService;
        private readonly Mock<ILogger> _logger3;
        private readonly Mock<ILogger> _logger4;
        private readonly Mock<IShoppingUtility> _shoppingUtility;
        private readonly Mock<ICachingService> _cachingService;
        private readonly Mock<IMSCPageProductInfoHelper> _mSCPageProductInfoHelper;
        private readonly Mock<IValidateHashPinService> _validateHashPinService;
        private readonly Mock<IDynamoDBService> _dynamoDBService;
        private readonly Mock<IMSCFormsOfPayment> _mSCFormsOfPayment;
        private readonly ProductBusiness _productBusiness;
        private readonly Mock<IMSCFormsOfPayment> _formsOfPayment;
        private readonly Mock<ISeatMapCSL30Helper> _seatMapCSL30Helper;
        private readonly Mock<IProductUtility> _productUtility;
        private readonly Mock<IShoppingBuyMilesHelper> _shoppingBuyMilesHelper;
        private readonly Mock<IShoppingCartUtility> _shoppingCartUtility;
        private readonly IShoppingCartUtility _shoppingCartUtility1;
        private readonly Mock<ICacheLog<ShoppingCartUtility>> _logger6;
        private readonly Mock<IShoppingCartService> _shoppingCartService;
        private readonly IResilientClient _resilientClient;
        private readonly Mock<IDPService> _dPService;
        private readonly Mock<ILegalDocumentsForTitlesService> _legalDocumentsForTitlesService;
        private readonly Mock<ISessionOnCloudService> _sessionOnCloudService;
        private readonly Mock<IApplicationEnricher> _applicationEnricher;
        private readonly Mock<IPKDispenserService> _pKDispenserService;
        private readonly Mock<IFlightShoppingProductsService> _flightShoppingProductsServices;
        private readonly Mock<IMSCPageProductInfoHelper> _MSCpageProductInfoHelper;
        private readonly Mock<ISeatMapService> _seatMapService;
        private readonly Mock<ICMSContentService> _cMSContentService;
        private readonly Mock<IPaymentService> _paymentService;
        private readonly Mock<ITravelCreditBusiness> _travelBusiness;
        private readonly Mock<IMSCPkDispenserPublicKey> _pkmscDispenserService;
        private readonly Mock<ILookUpTravelCreditService> _lookUpTravelCreditService;
        private readonly ICachingService _cachingService1;
        private readonly Mock<ICacheLog<CachingService>> _logger5;
        private readonly HttpClient _httpClient;
        private readonly IAsyncPolicy _policyWrap;
        private readonly string _baseUrl;
        private readonly Mock<IETCUtility> _eTCUtility;
        private readonly Mock<IFeatureSettings> _featureSettings;
        private readonly Mock<IMoneyPlusMilesUtility> _moneyPLusMilesUtility;
        private readonly Mock<IFeatureToggles> _featureToggles;
        public ProductBusinessTests()
        {

            _logger = new Mock<ICacheLog<ProductBusiness>>();
            _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true)
            .Build();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _shoppingSessionHelper = new Mock<IMSCShoppingSessionHelper>();
            _logger1 = new Mock<ILogger>();
            _requestEnricher = new Mock<IRequestEnricher>();
            _logger2 = new Mock<ILogger>();
            _sessionHelperService = new Mock<ISessionHelperService>();
            _logger3 = new Mock<ILogger>();
            _logger4 = new Mock<ILogger>();
            _logger5 = new Mock<ICacheLog<CachingService>>();
            _headers = new Mock<IHeaders>();
            _logger6 = new Mock<ICacheLog<ShoppingCartUtility>>();
            _legalDocumentsForTitlesService = new Mock<ILegalDocumentsForTitlesService>();
            _sessionOnCloudService = new Mock<ISessionOnCloudService>();
            _applicationEnricher = new Mock<IApplicationEnricher>();
            _shoppingCartService = new Mock<IShoppingCartService>();
            _paymentService = new Mock<IPaymentService>();
            _pKDispenserService = new Mock<IPKDispenserService>();
            _legalDocumentsForTitlesService = new Mock<ILegalDocumentsForTitlesService>();
            _flightShoppingProductsServices = new Mock<IFlightShoppingProductsService>();
            _MSCpageProductInfoHelper = new Mock<IMSCPageProductInfoHelper>();
            _seatMapService = new Mock<ISeatMapService>();
            _seatMapCSL30Helper = new Mock<ISeatMapCSL30Helper>();
            _productUtility = new Mock<IProductUtility>();
            _shoppingBuyMilesHelper = new Mock<IShoppingBuyMilesHelper>();
            _shoppingCartUtility = new Mock<IShoppingCartUtility>();
            _cMSContentService = new Mock<ICMSContentService>();
            _shoppingCartService = new Mock<IShoppingCartService>();
            _shoppingUtility = new Mock<IShoppingUtility>();
            _sessionHelperService = new Mock<ISessionHelperService>();
            _dPService = new Mock<IDPService>();
            _shoppingUtility = new Mock<IShoppingUtility>();
            _cachingService = new Mock<ICachingService>();
            _mSCPageProductInfoHelper = new Mock<IMSCPageProductInfoHelper>();
            _validateHashPinService = new Mock<IValidateHashPinService>();
            _dynamoDBService = new Mock<IDynamoDBService>();
            _mSCFormsOfPayment = new Mock<IMSCFormsOfPayment>();
            _travelBusiness = new Mock<ITravelCreditBusiness>();
            _pkmscDispenserService = new Mock<IMSCPkDispenserPublicKey>();
            _lookUpTravelCreditService = new Mock<ILookUpTravelCreditService>();
            _featureSettings = new Mock<IFeatureSettings>();
            _eTCUtility = new Mock<IETCUtility>();
            _featureToggles = new Mock<IFeatureToggles>();

            _resilientClient = new ResilientClient(_baseUrl);

            _cachingService1 = new CachingService(_resilientClient, _logger5.Object, _configuration);

            _shoppingCartUtility1 = new ShoppingCartUtility(_configuration, _headers.Object, _logger6.Object, _sessionHelperService.Object, _cMSContentService.Object, _legalDocumentsForTitlesService.Object, _mSCPageProductInfoHelper.Object, _cachingService1, _featureSettings.Object, _featureToggles.Object);


            _productBusiness = new ProductBusiness(_logger.Object, _configuration, _headers.Object, _shoppingSessionHelper.Object,
                _sessionHelperService.Object, _shoppingUtility.Object, _shoppingCartService.Object, _dPService.Object,
                _legalDocumentsForTitlesService.Object,
                _pKDispenserService.Object, _flightShoppingProductsServices.Object,
                _seatMapService.Object, _seatMapCSL30Helper.Object, _productUtility.Object,
                _cachingService.Object, _shoppingBuyMilesHelper.Object, _shoppingCartUtility.Object, _mSCPageProductInfoHelper.Object, _validateHashPinService.Object, _dynamoDBService.Object,
                  _mSCFormsOfPayment.Object, _lookUpTravelCreditService.Object, _pkmscDispenserService.Object, _moneyPLusMilesUtility.Object, _featureSettings.Object, _featureToggles.Object);

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
        [MemberData(nameof(TestDataGenerator.RegisterOffersForBooking_Test), MemberType = typeof(TestDataGenerator))]
        public void RegisterOffersForBooking_Test(MOBRegisterOfferRequest mOBRegisterOfferRequest, Session session, MOBShoppingCart mOBShoppingCart, MOBApplyPromoCodeResponse response, FlightReservationResponse flightReservationResponse, GetOffers offers, MOBBookingBundlesResponse response1, Reservation reservation, GetVendorOffers VendorOffers, SelectTrip shopRequest, MOBFOPTravelerCertificateResponse mOBFOPTravelerCertificateResponse, ReservationDetail reservationDetail, CCEFlightReservationResponseByCartId cCEFlightReservationResponseByCartId, PKDispenserKey pkdispenser, List<FormofPaymentOption> eligibleFormofPayments, ShopBookingDetailsResponse shopBookingDetailsResponse)
        {

            var _dataPowerFactory = new DataPowerFactory(_configuration, _sessionHelperService.Object);

            _shoppingCartService.Setup(p => p.CreateCart(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("Cart");

            _shoppingSessionHelper.Setup(p => p.GetValidateSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(session);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _sessionHelperService.Setup(p => p.GetSession<MOBApplyPromoCodeResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(response);

            _shoppingCartService.Setup(p => p.RegisterOrRemoveCoupon<FlightReservationResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(flightReservationResponse);

            _sessionHelperService.Setup(p => p.GetSession<GetOffers>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(offers);

            _sessionHelperService.Setup(p => p.GetSession<MOBBookingBundlesResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(response1);

            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _sessionHelperService.Setup(p => p.GetSession<GetVendorOffers>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(VendorOffers);

            _sessionHelperService.Setup(p => p.GetSession<SelectTrip>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(shopRequest);


            _sessionHelperService.Setup(p => p.GetSession<MOBFOPTravelerCertificateResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBFOPTravelerCertificateResponse);


            _sessionHelperService.Setup(p => p.GetSession<ReservationDetail>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservationDetail);

            _sessionHelperService.Setup(p => p.GetSession<CCEFlightReservationResponseByCartId>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(cCEFlightReservationResponseByCartId);


            _sessionHelperService.Setup(p => p.GetSession<PKDispenserKey>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(pkdispenser);

            _shoppingCartUtility.Setup(p => p.ReservationToShoppingCart_DataMigration(It.IsAny<MOBSHOPReservation>(), It.IsAny<MOBShoppingCart>(), It.IsAny<MOBRequest>(), It.IsAny<bool>(), It.IsAny<Session>())).ReturnsAsync(mOBShoppingCart);

            _mSCFormsOfPayment.Setup(p => p.GetEligibleFormofPayments(It.IsAny<MOBRequest>(), It.IsAny<Session>(), It.IsAny<MOBShoppingCart>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MOBSHOPReservation>(), It.IsAny<bool>(), It.IsAny<SeatChangeState>())).Returns(Task.FromResult((eligibleFormofPayments, true)));

            //  _cachingService.Setup(p => p.GetCache<string>(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(("", "TID1")));

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

            var response2 = JsonConvert.SerializeObject(displayCartResponse);

            _flightShoppingProductsServices.Setup(p => p.GetTripInsuranceInfo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response2);

            _eTCUtility.Setup(p => p.GetReservationFromPersist(It.IsAny<MOBSHOPReservation>(), It.IsAny<Session>())).ReturnsAsync(response.Reservation);

            _shoppingCartUtility.Setup(p => p.ReservationToShoppingCart_DataMigration(It.IsAny<MOBSHOPReservation>(), It.IsAny<MOBShoppingCart>(), It.IsAny<MOBRequest>(), It.IsAny<bool>(), It.IsAny<Session>())).ReturnsAsync(mOBShoppingCart);

            _mSCFormsOfPayment.Setup(p => p.GetEligibleFormofPayments(It.IsAny<MOBRequest>(), It.IsAny<Session>(), It.IsAny<MOBShoppingCart>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MOBSHOPReservation>(), It.IsAny<bool>(), It.IsAny<SeatChangeState>())).Returns(Task.FromResult((eligibleFormofPayments, true)));

            _flightShoppingProductsServices.Setup(p => p.GetProducts(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(response2);

            _productUtility.Setup(p => p.GetReservationFromPersist(It.IsAny<MOBSHOPReservation>(), It.IsAny<Session>())).ReturnsAsync(response.Reservation);

            _sessionHelperService.Setup(p => p.GetSession<ShopBookingDetailsResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(shopBookingDetailsResponse);

            _shoppingCartService.Setup(p => p.RegisterOffers<FlightReservationResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(flightReservationResponse);

            var productBusiness = new ProductBusiness(_logger.Object, _configuration, _headers.Object, _shoppingSessionHelper.Object,
                  _sessionHelperService.Object, _shoppingUtility.Object, _shoppingCartService.Object, _dPService.Object,
                  _legalDocumentsForTitlesService.Object,
                  _pKDispenserService.Object, _flightShoppingProductsServices.Object,
                  _seatMapService.Object, _seatMapCSL30Helper.Object, _productUtility.Object,
                  _cachingService1, _shoppingBuyMilesHelper.Object, _shoppingCartUtility.Object, _mSCPageProductInfoHelper.Object, _validateHashPinService.Object, _dynamoDBService.Object,
                    _mSCFormsOfPayment.Object, _lookUpTravelCreditService.Object, _pkmscDispenserService.Object, _moneyPLusMilesUtility.Object, _featureSettings.Object, _featureToggles.Object);

            //Act
            var result = productBusiness.RegisterOffersForBooking(mOBRegisterOfferRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);

        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.RegisterOffersForReshop_Test), MemberType = typeof(TestDataGenerator))]
        public void RegisterOffersForReshop_Test(MOBRegisterOfferRequest mOBRegisterOfferRequest, Session session, Reservation reservation, MOBShoppingCart mOBShoppingCart, GetOffers offers, ShopBookingDetailsResponse shopBookingDetailsResponse, FlightReservationResponse flightReservationResponse, GetVendorOffers VendorOffers, PKDispenserKey pkdispenser, MOBFOPTravelerCertificateResponse response, MOBSHOPReservation mOBSHOPReservation)
        {
            _shoppingSessionHelper.Setup(p => p.GetValidateSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(session);

            _shoppingCartService.Setup(P => P.CreateCart(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mOBRegisterOfferRequest.CartId);

            _sessionHelperService.Setup(P => P.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);


            _sessionHelperService.Setup(P => P.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _sessionHelperService.Setup(p => p.GetSession<GetOffers>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(offers);

            _sessionHelperService.Setup(p => p.GetSession<ShopBookingDetailsResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(shopBookingDetailsResponse);


            _sessionHelperService.Setup(p => p.GetSession<GetVendorOffers>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(VendorOffers);


            _shoppingCartService.Setup(p => p.RegisterOrRemoveCoupon<FlightReservationResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(flightReservationResponse);

            _sessionHelperService.Setup(p => p.GetSession<PKDispenserKey>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(pkdispenser);

            _sessionHelperService.Setup(p => p.GetSession<MOBFOPTravelerCertificateResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(response);

            _shoppingCartUtility.Setup(p => p.ReservationToShoppingCart_DataMigration(It.IsAny<MOBSHOPReservation>(), It.IsAny<MOBShoppingCart>(), It.IsAny<MOBRequest>(), It.IsAny<bool>(), It.IsAny<Session>())).ReturnsAsync(mOBShoppingCart);

            //Act
            var result = _productBusiness.RegisterOffersForReshop(mOBRegisterOfferRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);
        }


        [Theory]
        [MemberData(nameof(TestDataGenerator.RegisterOffersForReshop_Flow), MemberType = typeof(TestDataGenerator))]
        public void RegisterOffersForReshop_Flow(MOBRegisterOfferRequest mOBRegisterOfferRequest, Session session, Reservation reservation, MOBShoppingCart mOBShoppingCart, GetOffers offers, ShopBookingDetailsResponse shopBookingDetailsResponse, FlightReservationResponse flightReservationResponse, GetVendorOffers VendorOffers, PKDispenserKey pkdispenser, MOBFOPTravelerCertificateResponse response, MOBSHOPReservation mOBSHOPReservation)
        {
            _shoppingSessionHelper.Setup(p => p.GetValidateSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(session);

            _shoppingCartService.Setup(P => P.CreateCart(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mOBRegisterOfferRequest.CartId);

            _sessionHelperService.Setup(P => P.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);


            _sessionHelperService.Setup(P => P.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _sessionHelperService.Setup(p => p.GetSession<GetOffers>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(offers);

            _sessionHelperService.Setup(p => p.GetSession<ShopBookingDetailsResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(shopBookingDetailsResponse);


            _sessionHelperService.Setup(p => p.GetSession<GetVendorOffers>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(VendorOffers);


            _shoppingCartService.Setup(p => p.RegisterOrRemoveCoupon<FlightReservationResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(flightReservationResponse);

            _sessionHelperService.Setup(p => p.GetSession<PKDispenserKey>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(pkdispenser);

            _sessionHelperService.Setup(p => p.GetSession<MOBFOPTravelerCertificateResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(response);

            _shoppingCartUtility.Setup(p => p.ReservationToShoppingCart_DataMigration(It.IsAny<MOBSHOPReservation>(), It.IsAny<MOBShoppingCart>(), It.IsAny<MOBRequest>(), It.IsAny<bool>(), It.IsAny<Session>())).ReturnsAsync(mOBShoppingCart);

            //Act
            var result = _productBusiness.RegisterOffersForReshop(mOBRegisterOfferRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.RegisterSeatsForBooking_Test), MemberType = typeof(TestDataGenerator))]
        public void RegisterSeatsForBooking_Test(MOBRegisterSeatsRequest mOBRegisterSeatsRequest, Session session, SelectSeats persistSelectSeatsResponse, Reservation reservation, MOBShoppingCart mOBShoppingCart, MOBApplyPromoCodeResponse response, FlightReservationResponse flightReservationResponse, ProfileResponse profileResponse, ProfileFOPCreditCardResponse profileFOPCreditCardResponse, MOBFutureFlightCreditResponse response1, MOBCSLContentMessagesResponse cSLContentMessagesResponse, MOBFOPTravelerCertificateResponse mOBFOPTravelerCertificateResponse, SeatChangeState seatChangeState, MOBBookingRegisterSeatsResponse mOBBookingRegisterSeatsResponse, CCEFlightReservationResponseByCartId cCEFlightReservationResponseByCartId)
        {
           

            _shoppingSessionHelper.Setup(p => p.GetValidateSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(session);

            //_shoppingCartService.Setup(p => p.CreateCart(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("[{\"hashPinCode\":\"98A3562A3BE781050042C85CEFD8A1BE2B178192\",\"lastName\":\"LSDM\",\"mileagePlusNumber\":\"AY305211\",\"override24HrFlex\":false,\"overrideATREEligible\":false,\"recordLocator\":\"FYEVHG\",\"sessionId\":\"CC9CE97C4C2544D48396FA85F479D244\",\"accessCode\":\"ACCESSCODE\",\"application\":{\"id\":2,\"isProduction\":false,\"name\":\"Android\",\"version\":{\"major\":\"4.1.41\",\"minor\":\"4.1.41\"}},\"deviceId\":\"2a04f532-614d-4fa8-a284-856e72798008\",\"languageCode\":\"en-US\",\"transactionId\":\"2a04f532-614d-4fa8-a284-856e72798008|588dd5bb-7831-4123-b8fd-4ba324136ecf\",\"CartId\":\"\",\"SeatAssignment\":\"IsCouponEligibleProduct\",\"IsOmniCartSavedTripFlow\":true,\"IsRemove\":true},{\"hashPinCode\":\"98A3562A3BE781050042C85CEFD8A1BE2B178192\",\"lastName\":\"LSDM\",\"mileagePlusNumber\":\"AY305211\",\"override24HrFlex\":false,\"overrideATREEligible\":false,\"recordLocator\":\"FYEVHG\",\"sessionId\":\"CC9CE97C4C2544D48396FA85F479D244\",\"accessCode\":\"ACCESSCODE\",\"application\":{\"id\":2,\"isProduction\":false,\"name\":\"Android\",\"version\":{\"major\":\"4.1.41\",\"minor\":\"4.1.41\"}},\"deviceId\":\"2a04f532-614d-4fa8-a284-856e72798008\",\"languageCode\":\"en-US\",\"transactionId\":\"2a04f532-614d-4fa8-a284-856e72798008|588dd5bb-7831-4123-b8fd-4ba324136ecf\",\"CartId\":\"375D3212-9BFF-42F7-94A6-42342E2E24D2\",\"SeatAssignment\":\"IsCouponEligibleProduct\",\"IsOmniCartSavedTripFlow\":false,\"IsRemove\":false}]");

            _sessionHelperService.Setup(p => p.GetSession<SelectSeats>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(persistSelectSeatsResponse);

            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _sessionHelperService.Setup(p => p.GetSession<MOBApplyPromoCodeResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(response);

            _shoppingCartService.Setup(p => p.RegisterOrRemoveCoupon<FlightReservationResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(flightReservationResponse);

            _sessionHelperService.Setup(p => p.GetSession<ProfileResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(profileResponse);

            _sessionHelperService.Setup(p => p.GetSession<ProfileFOPCreditCardResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(profileFOPCreditCardResponse);

            _sessionHelperService.Setup(p => p.GetSession<MOBFutureFlightCreditResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(response1);

            _sessionHelperService.Setup(p => p.GetSession<MOBCSLContentMessagesResponse>(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(cSLContentMessagesResponse);

            _sessionHelperService.Setup(p => p.GetSession<MOBFOPTravelerCertificateResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBFOPTravelerCertificateResponse);

            _sessionHelperService.Setup(p => p.GetSession<SeatChangeState>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(seatChangeState);

            _sessionHelperService.Setup(p => p.GetSession<CCEFlightReservationResponseByCartId>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(cCEFlightReservationResponseByCartId);


            //Act
            var result = _productBusiness.RegisterSeatsForBooking(mOBRegisterSeatsRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.RegisterSeatsForReshop_Test), MemberType = typeof(TestDataGenerator))]

        public void RegisterSeatsForReshop_Test(MOBRegisterSeatsRequest mOBRegisterSeatsRequest, Session session, SelectSeats persistSelectSeatsResponse, Reservation reservation, MOBShoppingCart mOBShoppingCart, SelectTrip shopRequest, MOBApplyPromoCodeResponse response, FlightReservationResponse flightReservationResponse, MOBFOPTravelerCertificateResponse mOBFOPTravelerCertificateResponse, MOBReshopRegisterSeatsResponse mOBReshopRegisterSeatsResponse)
        {
            _shoppingSessionHelper.Setup(p => p.GetValidateSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(session);


            _shoppingCartService.Setup(p => p.CreateCart(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mOBRegisterSeatsRequest.CartId);


            _sessionHelperService.Setup(p => p.GetSession<SelectSeats>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(persistSelectSeatsResponse);



            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);


            _sessionHelperService.Setup(p => p.GetSession<SelectTrip>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(shopRequest);

            _sessionHelperService.Setup(p => p.GetSession<MOBApplyPromoCodeResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(response);

            _shoppingCartService.Setup(p => p.RegisterOrRemoveCoupon<FlightReservationResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(flightReservationResponse);



            _sessionHelperService.Setup(p => p.GetSession<MOBFOPTravelerCertificateResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBFOPTravelerCertificateResponse);

            //Act
            var result = _productBusiness.RegisterSeatsForReshop(mOBRegisterSeatsRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.RegisterSeatsForReshop_Flow ), MemberType = typeof(TestDataGenerator))]

        public void RegisterSeatsForReshop_Flow(MOBRegisterSeatsRequest mOBRegisterSeatsRequest, Session session, SelectSeats persistSelectSeatsResponse, Reservation reservation, MOBShoppingCart mOBShoppingCart, SelectTrip shopRequest, MOBApplyPromoCodeResponse response, FlightReservationResponse flightReservationResponse, MOBFOPTravelerCertificateResponse mOBFOPTravelerCertificateResponse, MOBReshopRegisterSeatsResponse mOBReshopRegisterSeatsResponse)
        {
            _shoppingSessionHelper.Setup(p => p.GetValidateSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(session);


            _shoppingCartService.Setup(p => p.CreateCart(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(mOBRegisterSeatsRequest.CartId);


            _sessionHelperService.Setup(p => p.GetSession<SelectSeats>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(persistSelectSeatsResponse);



            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);


            _sessionHelperService.Setup(p => p.GetSession<SelectTrip>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(shopRequest);

            _sessionHelperService.Setup(p => p.GetSession<MOBApplyPromoCodeResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(response);

            _shoppingCartService.Setup(p => p.RegisterOrRemoveCoupon<FlightReservationResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(flightReservationResponse);



            _sessionHelperService.Setup(p => p.GetSession<MOBFOPTravelerCertificateResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBFOPTravelerCertificateResponse);

            //Act
            var result = _productBusiness.RegisterSeatsForReshop(mOBRegisterSeatsRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);
        }


        [Theory]
        [MemberData(nameof(TestDataGenerator.ClearCartOnBackNavigation_Test), MemberType = typeof(TestDataGenerator))]

        public void ClearCartOnBackNavigation_Test(MOBClearCartOnBackNavigationRequest mOBClearCartOnBackNavigationRequest,
            Session session, Reservation reservation, MOBShoppingCart mOBShoppingCart, MOBSeatMapCSL30 mOBSeatMapCSL30,
            ShoppingResponse shoppingResponse, FlightSeatMapDetail flightSeatMapDetail, MOBBookingBundlesResponse mOBBookingBundlesResponse,
            GetOffers offers, GetVendorOffers VendorOffers, CCEFlightReservationResponseByCartId cCEFlightReservationResponseByCartId,
            FlightReservationResponse FlightReservationResponse, PKDispenserResponse pKDispenserResponse, CSLShopRequest cSLShopRequest,
            MOBFutureFlightCreditResponse mOBFutureFlightCreditResponse, MOBCSLContentMessagesResponse mOBCSLContentMessagesResponse,
            MOBApplyPromoCodeResponse response, MOBFOPTravelerCertificateResponse mOBFOPTravelerCertificateResponse)
        {
            _shoppingSessionHelper.Setup(p => p.GetValidateSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(session);


            //_sessionHelperService.Setup(p => p.GetSession<Session>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(session);

            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);


            _sessionHelperService.Setup(p => p.GetSession<MOBSeatMapCSL30>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBSeatMapCSL30);

            _sessionHelperService.Setup(p => p.GetSession<ShoppingResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(shoppingResponse);

            _seatMapService.Setup(p => p.SeatEngine<FlightSeatMapDetail>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(flightSeatMapDetail);

            _sessionHelperService.Setup(p => p.GetSession<MOBBookingBundlesResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBBookingBundlesResponse);



            _sessionHelperService.Setup(p => p.GetSession<GetOffers>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(offers);

            _sessionHelperService.Setup(p => p.GetSession<GetVendorOffers>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(VendorOffers);

            _sessionHelperService.Setup(p => p.GetSession<CCEFlightReservationResponseByCartId>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(cCEFlightReservationResponseByCartId);

            _shoppingCartService.Setup(p => p.RegisterOrRemoveCoupon<FlightReservationResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(FlightReservationResponse);

            _pKDispenserService.Setup(p => p.GetPkDispenserPublicKey<PKDispenserResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(pKDispenserResponse);

            _sessionHelperService.Setup(p => p.GetSession<CSLShopRequest>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(cSLShopRequest);

            _sessionHelperService.Setup(p => p.GetSession<MOBFutureFlightCreditResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBFutureFlightCreditResponse);

            _sessionHelperService.Setup(p => p.GetSession<MOBCSLContentMessagesResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBCSLContentMessagesResponse);

            _sessionHelperService.Setup(p => p.GetSession<MOBApplyPromoCodeResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(response);

            _sessionHelperService.Setup(p => p.GetSession<MOBFOPTravelerCertificateResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBFOPTravelerCertificateResponse);


            //Act
            var result = _productBusiness.ClearCartOnBackNavigation(mOBClearCartOnBackNavigationRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);

        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.RemoveAncillaryOffer_Test), MemberType = typeof(TestDataGenerator))]
        public void RemoveAncillaryOffer_Test(MOBRemoveAncillaryOfferRequest mOBRemoveAncillaryOfferRequest, Session session, Reservation reservation, MOBShoppingCart mOBShoppingCart, ShopBookingDetailsResponse shopBookingDetailsResponse, CCEFlightReservationResponseByCartId cCEFlightReservationResponseByCartId, FlightReservationResponse flightReservationResponse, MOBBookingBundlesResponse mOBBookingBundlesResponse, PKDispenserResponse pKDispenserResponse, GetOffers offers, GetVendorOffers VendorOffers, MOBCSLContentMessagesResponse mOBCSLContentMessagesResponse)
        {

            var _dataPowerFactory = new DataPowerFactory(_configuration, _sessionHelperService.Object);

            _sessionHelperService.Setup(p => p.GetSession<Session>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(session);

            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);


            _sessionHelperService.Setup(p => p.GetSession<ShopBookingDetailsResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(shopBookingDetailsResponse);

            _sessionHelperService.Setup(p => p.GetSession<CCEFlightReservationResponseByCartId>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(cCEFlightReservationResponseByCartId);

            _shoppingCartService.Setup(p => p.RegisterOrRemoveCoupon<FlightReservationResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(flightReservationResponse);

            _sessionHelperService.Setup(p => p.GetSession<MOBBookingBundlesResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBBookingBundlesResponse);

            _pKDispenserService.Setup(p => p.GetPkDispenserPublicKey<PKDispenserResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(pKDispenserResponse);

            _sessionHelperService.Setup(p => p.GetSession<GetOffers>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(offers);

            _sessionHelperService.Setup(p => p.GetSession<GetVendorOffers>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(VendorOffers);

            _sessionHelperService.Setup(p => p.GetSession<MOBCSLContentMessagesResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBCSLContentMessagesResponse);

            _shoppingSessionHelper.Setup(p => p.GetValidateSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(session);

            _shoppingCartUtility.Setup(p => p.ReservationToShoppingCart_DataMigration(It.IsAny<MOBSHOPReservation>(), It.IsAny<MOBShoppingCart>(), It.IsAny<MOBRequest>(), It.IsAny<bool>(), It.IsAny<Session>())).ReturnsAsync(mOBShoppingCart);

            _shoppingCartUtility.Setup(p => p.IsEnableOmniCartMVP2Changes(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(true);


            var productBusiness = new ProductBusiness(_logger.Object, _configuration, _headers.Object, _shoppingSessionHelper.Object,
                _sessionHelperService.Object, _shoppingUtility.Object, _shoppingCartService.Object, _dPService.Object,
                _legalDocumentsForTitlesService.Object,
                _pKDispenserService.Object, _flightShoppingProductsServices.Object,
                _seatMapService.Object, _seatMapCSL30Helper.Object, _productUtility.Object,
                _cachingService1, _shoppingBuyMilesHelper.Object, _shoppingCartUtility.Object, _mSCPageProductInfoHelper.Object, _validateHashPinService.Object, _dynamoDBService.Object,
                  _mSCFormsOfPayment.Object, _lookUpTravelCreditService.Object, _pkmscDispenserService.Object, _moneyPLusMilesUtility.Object, _featureSettings.Object, _featureToggles.Object);

            //Act
            var result = productBusiness.RemoveAncillaryOffer(mOBRemoveAncillaryOfferRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);
        }


        [Theory]
        [MemberData(nameof(TestDataGenerator.RegisterOffersForOmniCartSavedTrip_Test), MemberType = typeof(TestDataGenerator))]
        public void RegisterOffersForOmniCartSavedTrip_Test(MOBSHOPUnfinishedBookingRequestBase mOBSHOPUnfinishedBookingRequestBase, Session session, Reservation reservation,
            MOBShoppingCart mOBShoppingCart, CCEFlightReservationResponseByCartId cCEFlightReservationResponseByCartId,
            SelectSeats selectSeats, MOBApplyPromoCodeResponse mOBApplyPromoCodeResponse, MOBSeatMapCSL30 mOBSeatMapCSL30,
            MOBFutureFlightCreditResponse mOBFutureFlightCreditResponse, MOBCSLContentMessagesResponse mOBCSLContentMessagesResponse,
            MOBFOPTravelerCertificateResponse mOBFOPTravelerCertificateResponse, SeatChangeState seatChangeState, HashPinValidate hashPinValidate, List<CCEFlightReservationResponseByCartId> cCEFlightReservationResponseByCartIds, MOBBookingBundlesResponse mOBBookingBundlesResponse, MOBBookingRegisterOfferResponse mOBBookingRegisterOfferResponse, List<FormofPaymentOption> eligibleFormofPayments)
        {

            var _dataPowerFactory = new DataPowerFactory(_configuration, _sessionHelperService.Object);

            _sessionHelperService.Setup(p => p.GetSession<Session>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(session);

            _shoppingSessionHelper.Setup(p => p.GetValidateSession(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(session);

            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _sessionHelperService.Setup(p => p.GetSession<CCEFlightReservationResponseByCartId>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(cCEFlightReservationResponseByCartId);

            _sessionHelperService.Setup(p => p.GetSession<SelectSeats>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(selectSeats);

            _sessionHelperService.Setup(p => p.GetSession<MOBApplyPromoCodeResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBApplyPromoCodeResponse);

            _sessionHelperService.Setup(p => p.GetSession<MOBSeatMapCSL30>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBSeatMapCSL30);

            _sessionHelperService.Setup(p => p.GetSession<MOBFutureFlightCreditResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBFutureFlightCreditResponse);

            _sessionHelperService.Setup(p => p.GetSession<MOBCSLContentMessagesResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBCSLContentMessagesResponse);

            _sessionHelperService.Setup(p => p.GetSession<MOBFOPTravelerCertificateResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBFOPTravelerCertificateResponse);

            _sessionHelperService.Setup(p => p.GetSession<SeatChangeState>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(seatChangeState);

            _dynamoDBService.Setup(p => p.GetRecords<string>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("{\"AppVersion\":\"4.1.48\",\"ApplicationID\":\"2\",\"AuthenticatedToken\":null,\"CallDuration\":0,\"CustomerID\":\"123901727\",\"DataPowerAccessToken\":\"BearerrlWuoTpvBvWFHmV1AvVfVzgcMPV6VzRlAzRjLmVkYGIwMwDgAQZlZl04LJDkYGxjZwAuAzMwLwL3MFW9.NNV8GJ9vnJkyYHShMUWinJEDnT9hMI9IDHksAwDmEGSSAQpgZGV0Zv00DwMQYHSPA0HgAwDjZwESARWQBQEQm_UZfeWVu4hFCrLSGjfTs8WRM4GadvbiNAMYdbxZoEh69D-IXfiLKeTCPU-4GE5RKBhYbkMOv0TrQzcMMhRx3TZVsqJDUMphaQTSKpAyFJUYriwVknmMvLXUrcYmDtLkXAuiEOgfNQWCUqqaUcY9HfqFtTcrIY03SjwHH296Ptu8FJ9OdNtnpEehMuNPLpYz.jwC0naNWpnrKScvZMHhSY2zEFTasGTG3JfCP1jhPoKxBeeuG_YkZkq15WhOLdA-erMVuY0e8MSqHEkQ3pNepiRHXo09f_Ht0f9PJfciIUOjA_haRN9x1WYfsd57mPCMZCLOrTI4tPDLbrFoyGFkElHLpmX1fly3mP_gR7ITMpM-s8Ynjr1XVxtZQ072wUOqfllxg8Dp17MPMdRD9VOpNMj-nXDAi0-9_vKE5d0Lm1xmDSh3R00DqkM0VQb2ScHfG5XChkjhux6vFm6Y8lgcgrfO6t5r-gM3Jq7DU6ZbT6Gk30d14PwAfm-35s5N5Bt39zDlBZ3wOcPjgZtnvGEk5Kt.rlW2MKVvBvVkYwNvYPWdqTxvBvWwAmLjAGOvBP1yLwIuYGEyLwRgLJL5Zv1xZzR1MGMuZwD0BJHvYPWmqJVvBvWwAmLjAGOvBP1yLwIuYGEyLwRgLJL5Zv1xZzR1MGMuZwD0BJHvYPWcp3ZvBvWbqUEjpmbiY2AmoJZhpJRhLKI0nP51ozy0MJDhL29gY29uqKEbZv92ZFVfVzS1MPV6Vx1iLzyfMF1OozElo2yxHTuiozIsIHSZKmL0Z0HkEGD3YGRlAQVgARV2Dl1ODwqSYGL0ZQV0EGEPDmt0DlVfVzyuqPV6ZGL1ZQHlBQR3ZljvMKujVwbkAwHjAGZ1ZmpmYPWuoKVvByfvpUqxVy0fVzS1qTusqTygMFV6ZGL1ZQHlBQR3Zljvoz9hL2HvBvVmnUpjMaN5pIuzqaRvYPWuqS9bLKAbVwbvHHqOIaykDHcUAwAmFSIDFUMZF1ScHG09VvjvqKOxLKEyMS9uqPV6ZGL1ZQHlBQR3ZljvL2kcMJ50K2SjpTkcL2S0nJ9hK2yxVwbvGJ9vnJkyYHShMUWinJEDnT9hMI9IDHksAwDmEGSSAQpgZGV0Zv00DwMQYHSPA0HgAwDjZwESARWQBQEQVvjvqKAypyE5pTHvBvWaqJImqPVfVzAfnJIhqS9cMPV6Vx1iLzyfMF1OozElo2yxHTuiozIsIHSZKmL0Z0HkEGD3YGRlAQVgARV2Dl1ODwqSYGL0ZQV0EGEPDmt0DlVfVzIhMSImMKWOM2IhqRyRVwbvAQD2MGx0Z2DgMQp0BF00MwWxYGuvMwVgMGOvLGIxAzEuAwt1VvjvMJ5xIKAypxSaMJ50FINvBvVkZwphZP4jYwRvsD\",\"DeviceID\":\"446e943d-d749-4f2d-8bf2-e0ba5d6da685\",\"HashPincode\":\"A5D3AFDAE0BF0E6543650D7B928EB77C94A4AD56\",\"IsTokenAnonymous\":\"False\",\"IsTokenValid\":\"True\",\"IsTouchIDSignIn\":\"False\",\"MPUserName\":\"AW719636\",\"MileagePlusNumber\":\"AW719636\",\"PinCode\":\"\",\"TokenExpireDateTime\":\"2022-04-2105:02:53.725\",\"TokenExpiryInSeconds\":\"7200\",\"UpdateDateTime\":\"2022-04-2103:02:54.565\"}");

            _validateHashPinService.Setup(p => p.ValidateHashPin<HashPinValidate>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(hashPinValidate);

            _sessionHelperService.Setup(p => p.GetSession<List<CCEFlightReservationResponseByCartId>>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(cCEFlightReservationResponseByCartIds);

            _shoppingCartUtility.Setup(p => p.IsEnableOmniCartMVP2Changes(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(true);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _sessionHelperService.Setup(p => p.GetSession<MOBBookingBundlesResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBBookingBundlesResponse);

            _productUtility.Setup(p => p.GetReservationFromPersist(It.IsAny<MOBSHOPReservation>(), It.IsAny<Session>())).ReturnsAsync(mOBBookingRegisterOfferResponse.Reservation);

            _shoppingCartUtility.Setup(p => p.ReservationToShoppingCart_DataMigration(It.IsAny<MOBSHOPReservation>(), It.IsAny<MOBShoppingCart>(), It.IsAny<MOBRequest>(), It.IsAny<bool>(), It.IsAny<Session>())).ReturnsAsync(mOBShoppingCart);

            _mSCFormsOfPayment.Setup(p => p.GetEligibleFormofPayments(It.IsAny<MOBRequest>(), It.IsAny<Session>(), It.IsAny<MOBShoppingCart>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MOBSHOPReservation>(), It.IsAny<bool>(), It.IsAny<SeatChangeState>())).Returns(Task.FromResult((eligibleFormofPayments, true)));

            //  _cachingService.Setup(p => p.GetCache<string>(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult((key, "TID1")));

            var productBusiness = new ProductBusiness(_logger.Object, _configuration, _headers.Object, _shoppingSessionHelper.Object,
                _sessionHelperService.Object, _shoppingUtility.Object, _shoppingCartService.Object, _dPService.Object,
                _legalDocumentsForTitlesService.Object,
                _pKDispenserService.Object, _flightShoppingProductsServices.Object,
                _seatMapService.Object, _seatMapCSL30Helper.Object, _productUtility.Object,
                _cachingService1, _shoppingBuyMilesHelper.Object, _shoppingCartUtility.Object, _mSCPageProductInfoHelper.Object, _validateHashPinService.Object, _dynamoDBService.Object,
                  _mSCFormsOfPayment.Object, _lookUpTravelCreditService.Object, _pkmscDispenserService.Object, _moneyPLusMilesUtility.Object, _featureSettings.Object, _featureToggles.Object);


            //Act
            var result = productBusiness.RegisterOffersForOmniCartSavedTrip(mOBSHOPUnfinishedBookingRequestBase);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);
        }

        [Theory]
        [MemberData(nameof(TestDataGenerator.UnRegisterAncillaryOffersForBooking_Test), MemberType = typeof(TestDataGenerator))]

        public void UnRegisterAncillaryOffersForBooking_Test(MOBRegisterOfferRequest mOBRegisterOfferRequest, Session session, Reservation reservation, MOBShoppingCart mOBShoppingCart, MOBApplyPromoCodeResponse mOBApplyPromoCodeResponse, MOBCSLContentMessagesResponse mOBCSLContentMessagesResponse, FlightReservationResponse flightReservationResponse, ProfileResponse profileResponse, ProfileFOPCreditCardResponse profileFOPCreditCardResponse, PKDispenserKey pkdispenser)
        {
            _sessionHelperService.Setup(p => p.GetSession<Session>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(session);

            _sessionHelperService.Setup(p => p.GetSession<Reservation>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(reservation);

            _sessionHelperService.Setup(p => p.GetSession<MOBShoppingCart>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBShoppingCart);

            _sessionHelperService.Setup(p => p.GetSession<MOBApplyPromoCodeResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBApplyPromoCodeResponse);

            _sessionHelperService.Setup(p => p.GetSession<MOBCSLContentMessagesResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(mOBCSLContentMessagesResponse);

            _shoppingCartService.Setup(p => p.RegisterOrRemoveCoupon<FlightReservationResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(flightReservationResponse);

            _sessionHelperService.Setup(p => p.GetSession<ProfileResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(profileResponse);


            _sessionHelperService.Setup(p => p.GetSession<ProfileFOPCreditCardResponse>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(profileFOPCreditCardResponse);

            _sessionHelperService.Setup(p => p.GetSession<PKDispenserKey>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(pkdispenser);

            //Act
            var result = _productBusiness.UnRegisterAncillaryOffersForBooking(mOBRegisterOfferRequest);
            //Assert
            Assert.True(result.Exception != null || result.Result != null);
        }
    }
}

