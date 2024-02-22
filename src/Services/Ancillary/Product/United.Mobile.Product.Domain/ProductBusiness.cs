using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Common.Helper.MSCPayment.Interfaces;
using United.Common.Helper.Product;
using United.Definition;
using United.Definition.Booking;
using United.Definition.CFOP;
using United.Definition.FormofPayment;
using United.Definition.SeatCSL30;
using United.Definition.Shopping;
using United.Definition.Shopping.Bundles;
using United.Definition.Shopping.TripInsurance;
using United.Definition.Shopping.UnfinishedBooking;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.DynamoDB;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Mobile.Model.Common;
using United.Mobile.Model.Common.CloudDynamoDB;
using United.Mobile.Model.DynamoDb.Common;
using United.Mobile.Model.MSC;
using United.Mobile.Model.MSC.Corporate;
using United.Persist.Definition.CCE;
using United.Persist.Definition.Merchandizing;
using United.Persist.Definition.Pcu;
using United.Persist.Definition.SeatChange;
using United.Persist.Definition.Shopping;
using United.Reward.Configuration;
using United.Service.Presentation.CommonModel;
using United.Service.Presentation.ProductModel;
using United.Service.Presentation.ReservationResponseModel;
using United.Service.Presentation.SecurityResponseModel;
using United.Service.Presentation.SegmentModel;
using United.Services.FlightShopping.Common.Cart;
using United.Services.FlightShopping.Common.DisplayCart;
using United.Services.FlightShopping.Common.Extensions;
using United.Services.FlightShopping.Common.FlightReservation;
using United.Utility.Helper;
using Characteristic = United.Service.Presentation.CommonModel.Characteristic;
using Charge = United.Service.Presentation.CommonModel.Charge;
using Currency = United.Service.Presentation.CommonModel.Currency;
using FlightProfile = United.Service.Presentation.CommonModel.FlightProfile;
using ProductPrice = United.Service.Presentation.CommonModel.ProductPrice;
using Reservation = United.Persist.Definition.Shopping.Reservation;
using Subscription = United.Service.Presentation.ProductModel.Subscription;
using Task = System.Threading.Tasks.Task;


namespace United.Mobile.Product.Domain
{
    public class ProductBusiness : IProductBusiness
    {
        private readonly ICacheLog<ProductBusiness> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHeaders _headers;
        private readonly IMSCShoppingSessionHelper _shoppingSessionHelper;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly IShoppingUtility _shoppingUtility;
        private readonly ISeatMapCSL30Helper _seatMapCSL30Helper;
        private readonly IProductUtility _productUtility;
        private readonly IShoppingCartUtility _shoppingCartUtility;
        private readonly IMSCPageProductInfoHelper _mSCPageProductInfoHelper;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IDPService _dPService;
        private readonly ILegalDocumentsForTitlesService _legalDocumentsForTitlesService;
        private readonly IPKDispenserService _pKDispenserService;
        private readonly IFlightShoppingProductsService _flightShoppingProductsServices;
        private readonly ISeatMapService _seatMapService;
        private readonly ICachingService _cachingService;
        private readonly IShoppingBuyMilesHelper _shoppingBuyMilesHelper;
        private readonly IValidateHashPinService _validateHashPinService;
        private readonly IDynamoDBService _dynamoDBService;
        private readonly IMSCFormsOfPayment _mSCFormsOfPayment;
        private readonly ILookUpTravelCreditService _lookUpTravelCreditService;
        private readonly IMSCPkDispenserPublicKey _pKDispenserPublicKey;
        private readonly IMoneyPlusMilesUtility _moneyPlusMilesUtility;
        private readonly IFeatureSettings _featureSettings;
        private readonly IFeatureToggles _featureToggles;
        public ProductBusiness(ICacheLog<ProductBusiness> logger
            , IConfiguration configuration
            , IHeaders headers
            , IMSCShoppingSessionHelper shoppingSessionHelper
            , ISessionHelperService sessionHelperService
            , IShoppingUtility shoppingUtility
            , IShoppingCartService shoppingCartService
            , IDPService dPService
            , ILegalDocumentsForTitlesService legalDocumentsForTitlesService
            , IPKDispenserService pKDispenserService
            , IFlightShoppingProductsService flightShoppingProductsServices
            , ISeatMapService seatMapService
            , ISeatMapCSL30Helper seatMapCSL30Helper
            , IProductUtility productUtility
            , ICachingService cachingService
            , IShoppingBuyMilesHelper shoppingBuyMilesHelper
            , IShoppingCartUtility shoppingCartUtility
            , IMSCPageProductInfoHelper mSCPageProductInfoHelper
            , IValidateHashPinService validateHashPinService
            , IDynamoDBService dynamoDBService
            , IMSCFormsOfPayment mSCFormsOfPayment
            , ILookUpTravelCreditService lookUpTravelCreditService
             , IMSCPkDispenserPublicKey pKDispenserPublicKey
            ,IMoneyPlusMilesUtility moneyPlusMilesUtility
            , IFeatureSettings featureSettings
            , IFeatureToggles featureToggles
            )
        {
            _logger = logger;
            _configuration = configuration;
            _headers = headers;
            _shoppingSessionHelper = shoppingSessionHelper;
            _sessionHelperService = sessionHelperService;
            _shoppingUtility = shoppingUtility;
            _shoppingCartService = shoppingCartService;
            _dPService = dPService;
            _dynamoDBService = dynamoDBService;
            _legalDocumentsForTitlesService = legalDocumentsForTitlesService;
            _pKDispenserService = pKDispenserService;
            _flightShoppingProductsServices = flightShoppingProductsServices;
            _seatMapService = seatMapService;
            _seatMapCSL30Helper = seatMapCSL30Helper;
            _productUtility = productUtility;
            _cachingService = cachingService;
            _shoppingBuyMilesHelper = shoppingBuyMilesHelper;
            _shoppingCartUtility = shoppingCartUtility;
            _mSCPageProductInfoHelper = mSCPageProductInfoHelper;
            _validateHashPinService = validateHashPinService;
            _mSCFormsOfPayment = mSCFormsOfPayment;
            _lookUpTravelCreditService = lookUpTravelCreditService;
            _pKDispenserPublicKey = pKDispenserPublicKey;
            _moneyPlusMilesUtility = moneyPlusMilesUtility;
            _featureSettings = featureSettings;
            _featureToggles = featureToggles;
        }

        public async Task<MOBBookingRegisterOfferResponse> RegisterOffersForBooking(MOBRegisterOfferRequest request)
        {
            var response = new MOBBookingRegisterOfferResponse();

            string cartId = string.Empty;
            bool isDefault = false;
            MOBShoppingCart shoppingCart = new MOBShoppingCart();
            response.TransactionId = request.TransactionId;
            response.LanguageCode = request.LanguageCode;

            Session session = null;
            session = await _shoppingSessionHelper.GetValidateSession(request.SessionId, false, true);

            session.Flow = request.Flow;

            if (string.IsNullOrEmpty(request.CartId))
            {
                request.CartId = await CreateCart(request, session);

                MOBShoppingCart persistShoppingCart = new MOBShoppingCart();

                await _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, request.SessionId, new List<string> { request.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName);
            }

            Reservation bookingPathReservation = new Reservation();
            bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(session.SessionId, bookingPathReservation.ObjectName, new List<string> { session.SessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);

            // MOBILE-25395: SAF
            var safCode = _configuration.GetValue<string>("SAFCode");
            if (request.CartKey == "ProductBundles" && !(request.MerchandizingOfferDetails?.Any(md => md.ProductCode == safCode) ?? false))
            {
                if (_shoppingCartUtility.IsEnableOmniCartMVP2Changes(request.Application.Id, request.Application.Version.Major, bookingPathReservation?.ShopReservationInfo2?.IsDisplayCart == true))
                {
                    response = await RegisterBundlesV2(request, session);
                }
                else
                {
                    response = await RegisterBundles(request, session);
                }

                if (response.PromoCodeRemoveAlertForProducts != null)
                    return response;
            }
            else if (request.CartKey == "OTP")
                response = await RegisterOfferforOTP(request, session);
            else if (request.CartKey == "TPI")
                response = await RegisterOfferForTPI(request, session);

            if (request.MerchandizingOfferDetails != null && request.MerchandizingOfferDetails.Count > 0)
            {
                foreach (var merchandizingOfferDetail in request.MerchandizingOfferDetails)
                {
                    if (merchandizingOfferDetail.ProductCode.Equals("FLK") && !merchandizingOfferDetail.IsOfferRegistered)
                    {
                        response = await RegisterFareLock(request, session);
                    }
                    else if (merchandizingOfferDetail.ProductCode.Equals("FLK") && merchandizingOfferDetail.IsOfferRegistered)
                    {
                        response = await UnRegisterFareLock(request, session);
                    }
                    else if (merchandizingOfferDetail.ProductCode.Equals(safCode) &&
                             ConfigUtility.IsEnableSAFFeature(session))
                    {
                        // MOBILE-25395: SAF
                        response =  await RegisterOfferForSAF(request, session);
                    }
                }
            }

            shoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, shoppingCart.ObjectName, new List<string> { request.SessionId, shoppingCart.ObjectName }).ConfigureAwait(false);

            shoppingCart = await _shoppingCartUtility.ReservationToShoppingCart_DataMigration(response.Reservation, shoppingCart, request, false, session);

            if (_productUtility.IsBuyMilesFeatureEnabled(request.Application.Id, request.Application.Version.Major))
                _shoppingBuyMilesHelper.ApplyPriceChangesForBuyMiles(reservation: response.Reservation);

            shoppingCart.Flow = request.Flow;
            //Code to be same as RegisterSeatsForBooking

            if (_configuration.GetValue<bool>("EnableTravelCreditSummary"))
            {
                var travelCreditCount = shoppingCart?.FormofPaymentDetails?.TravelCreditDetails?.TravelCredits?.Count ?? 0;
                if (travelCreditCount > 0)
                {
                    var travelCreditSummary = _configuration.GetValue<string>("TravelCreditSummary");
                    var pluralChar = travelCreditCount > 1 ? "s" : string.Empty;
                    shoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCreditSummary = shoppingCart?.Products?.FirstOrDefault().Code != "FLK" ? string.Format(travelCreditSummary, travelCreditCount, pluralChar) : string.Empty;
                }
            }
            response.ShoppingCart = shoppingCart;
            await _sessionHelperService.SaveSession<MOBShoppingCart>(response.ShoppingCart, request.SessionId, new List<string> { request.SessionId, shoppingCart.ObjectName }, shoppingCart.ObjectName);
            var tupleRespEligibleFormofPayments = await _mSCFormsOfPayment.GetEligibleFormofPayments(request, session, shoppingCart, request.CartId, request.Flow, response.Reservation);
            response.EligibleFormofPayments = tupleRespEligibleFormofPayments.EligibleFormofPayments;
            isDefault = tupleRespEligibleFormofPayments.isDefault;

            response.IsDefaultPaymentOption = isDefault;
            await response.Reservation.UpdateRewards(_configuration, _cachingService);
            //Covid 19 Emergency WHO TPI content
            if (_configuration.GetValue<bool>("ToggleCovidEmergencytextTPI") == true)
            {
                bool return_TPICOVID_19WHOMessage_For_BackwardBuilds = GeneralHelper.IsApplicationVersionGreater2(request.Application.Id, request.Application.Version.Major, "Android_Return_TPICOVID_19WHOMessage__For_BackwardBuilds", "iPhone_Return_TPICOVID_19WHOMessage_For_BackwardBuilds", null, null, _configuration);
                if (!return_TPICOVID_19WHOMessage_For_BackwardBuilds && response.Reservation != null
                    && response.Reservation.TripInsuranceInfoBookingPath != null && response.Reservation.TripInsuranceInfoBookingPath.TPIAIGReturnedMessageContentList != null
                    && response.Reservation.TripInsuranceInfoBookingPath.TPIAIGReturnedMessageContentList.Count > 0)
                {
                    MOBItem tpiCOVID19EmergencyAlert = response.Reservation.TripInsuranceInfoBookingPath.TPIAIGReturnedMessageContentList.Find(p => p.Id.ToUpper().Trim() == "COVID19EmergencyAlert".ToUpper().Trim());
                    if (tpiCOVID19EmergencyAlert != null)
                    {
                        response.Reservation.TripInsuranceInfoBookingPath.Tnc = response.Reservation.TripInsuranceInfoBookingPath.Tnc +
                            "<br><br>" + tpiCOVID19EmergencyAlert.CurrentValue;
                    }

                }
            }


            return response;
        }

        public async Task<MOBRESHOPRegisterOfferResponse> RegisterOffersForReshop(MOBRegisterOfferRequest request)
        {
            var response = new MOBBookingRegisterOfferResponse();
            string cartId = string.Empty;
            bool isDefault = false;
            MOBShoppingCart shoppingCart = new MOBShoppingCart();

            Session session = null;
            session = await _shoppingSessionHelper.GetValidateSession(request.SessionId, false, true);
            session.Flow = request.Flow;

            if (string.IsNullOrEmpty(request.CartId))
            {
                request.CartId = await CreateCart(request, session);

                MOBShoppingCart persistShoppingCart = new MOBShoppingCart();

                await _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, request.SessionId, new List<string> { request.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName);
            }

            if (request.CartKey == "OTP")
                response = await RegisterOfferforOTP(request, session);

            shoppingCart = response.ShoppingCart;
            shoppingCart = await _shoppingCartUtility.ReservationToShoppingCart_DataMigration(response.Reservation, shoppingCart, request).ConfigureAwait(false);
            shoppingCart.Flow = request.Flow;

            if (_configuration.GetValue<bool>("EnableReshopRegOffSCSaveToPersist"))
            {
                await _sessionHelperService.SaveSession<MOBShoppingCart>(shoppingCart, request.SessionId, new List<string> { request.SessionId, shoppingCart.ObjectName }, shoppingCart.ObjectName);
            }

            response.Flow = request.Flow;
            response.ShoppingCart = shoppingCart;
            var tupleRespEligibleFormofPayments  = await _mSCFormsOfPayment.GetEligibleFormofPayments(request, session, shoppingCart, request.CartId, request.Flow);
            response.EligibleFormofPayments = tupleRespEligibleFormofPayments.EligibleFormofPayments;
            isDefault = tupleRespEligibleFormofPayments.isDefault;
            response.IsDefaultPaymentOption = isDefault;

            return await Task.FromResult(ConvertBookingToReshopRegisterOffersResponse(response));
        }

        
        public async Task<MOBBookingRegisterSeatsResponse> RegisterSeatsForBooking(MOBRegisterSeatsRequest request)
        {
            var response = new MOBBookingRegisterSeatsResponse();
            MOBShoppingCart shoppingCart = new MOBShoppingCart();
            bool isDefault = false;

            Session session = null;
            session = await _shoppingSessionHelper.GetValidateSession(request.SessionId, false, true);
            session.Flow = request.Flow;

            if (string.IsNullOrEmpty(request.CartId))
            {
                request.CartId = await CreateCart(request, session);

                MOBShoppingCart persistShoppingCart = new MOBShoppingCart();

                await _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, request.SessionId, new List<string> { request.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName);
            }

            response = await RegisterSeats(request, session);
            if (_configuration.GetValue<bool>("EnableAdvanceSearchCouponBooking") && response.PromoCodeRemoveAlertForProducts != null)
                return response;

            if (_shoppingCartUtility.IsEnableMoneyPlusMilesFeature(request.Application.Id, request.Application.Version.Major, session?.CatalogItems)
             && (!string.IsNullOrEmpty(session.MileagPlusNumber) || session.IsMoneyPlusMilesSelected) && request.IsDone
              )
            {
                try
                {
                    await _moneyPlusMilesUtility.GetMoneyPlusMilesOptionsForFinalRTIScreen(request, response, session, shoppingCart);
                }
                catch (Exception ex)
                {
                    _logger.ILoggerWarning("GetMoneyPlusMilesOptionsForFinalRTIScreen : There is problem getting MoneyMilesOptions RegisterSeatsForBooking exception , {@Request} " + ex.Message, request);
                }
            }
            shoppingCart = response.ShoppingCart;

            if (!_configuration.GetValue<bool>("DisableFireAndForgetTravelCreditCallInGetProfile") && (shoppingCart?.FormofPaymentDetails?.TravelCreditDetails == null ||
                                                                                                           shoppingCart?.FormofPaymentDetails?.TravelCreditDetails.LookUpMessages?.Count == 0))
            {
                var shoppinfCartFireAndForget = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, shoppingCart.ObjectName + "FireAndForget", new List<string> { request.SessionId, shoppingCart.ObjectName + "FireAndForget" }).ConfigureAwait(false);
                if (shoppinfCartFireAndForget != null)
                {
                    if (shoppingCart.FormofPaymentDetails == null)
                        shoppingCart.FormofPaymentDetails = new MOBFormofPaymentDetails();

                    shoppingCart.FormofPaymentDetails.TravelCreditDetails = shoppinfCartFireAndForget.FormofPaymentDetails.TravelCreditDetails;
                    shoppingCart.FormofPaymentDetails.TravelBankDetails = shoppinfCartFireAndForget.FormofPaymentDetails.TravelBankDetails;
                    _logger.LogInformation("RegisterSeatsForReshop - FireAndForget - Load TB and Preload credits in registerseats- {applicationId} {version} {deviceid} and {SessionId}", request.Application.Id, request.Application.Version.Major, request.DeviceId, request.SessionId);
                }
                else
                {
                    if (request.Flow == FlowType.BOOKING.ToString() && !session.IsAward && (ConfigUtility.IsEnableU4BCorporateBookingFeature(request.Application.Id, request.Application.Version.Major) ? true : !session.IsCorporateBooking))
                    {
                        await _productUtility.PreLoadTravelCredit(session.SessionId, shoppingCart, request, true);
                    }

                    shoppingCart.FormofPaymentDetails.TravelBankDetails = await _productUtility.PopulateTravelBankData(session, response.Reservation, request);
                    await _sessionHelperService.SaveSession<MOBShoppingCart>(shoppingCart, session.SessionId, new List<string> { session.SessionId, shoppingCart.ObjectName }, shoppingCart.ObjectName);
                    _logger.LogInformation("RegisterSeatsForReshop - LoadTBAndPreloadInRegisterSeats - Load TB and Preload credits in registerseats- {applicationId} {version} {deviceid} and {SessionId}", request.Application.Id, request.Application.Version.Major, request.DeviceId, request.SessionId);
                }
            }
          
            shoppingCart = await _shoppingCartUtility.ReservationToShoppingCart_DataMigration(response.Reservation, shoppingCart, request, false, session);

            if (_productUtility.IsBuyMilesFeatureEnabled(request.Application.Id, request.Application.Version.Major))
                _shoppingBuyMilesHelper.ApplyPriceChangesForBuyMiles(reservation: response.Reservation);

            shoppingCart.Flow = request.Flow;

            if (_configuration.GetValue<bool>("EnableTravelCreditSummary"))
            {
                var travelCreditCount = shoppingCart?.FormofPaymentDetails?.TravelCreditDetails?.TravelCredits?.Count ?? 0;

                if (travelCreditCount > 0)
                {
                    var travelCreditSummary = _configuration.GetValue<string>("TravelCreditSummary");
                    var pluralChar = travelCreditCount > 1 ? "s" : string.Empty;
                    shoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCreditSummary = shoppingCart?.Products?.FirstOrDefault().Code != "FLK" ? string.Format(travelCreditSummary, travelCreditCount, pluralChar) : string.Empty;
                    if(await _featureSettings.GetFeatureSettingValue("EnableFSRETCTravelCreditsFeature").ConfigureAwait(false))
                    {
                        travelCreditSummary = _configuration.GetValue<string>("PricingTypeTravelCreditSummary");
                        shoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCreditSummary = shoppingCart?.Products?.FirstOrDefault().Code != "FLK" ? string.Format(travelCreditSummary, travelCreditCount, pluralChar) : string.Empty;
                    }
                }
            }
            if (!_configuration.GetValue<bool>("DisableTravelCreditsSummaryCheckForAwardFlow") && response.Reservation.AwardTravel && !string.IsNullOrEmpty(response?.ShoppingCart?.FormofPaymentDetails?.TravelCreditDetails?.TravelCreditSummary))
            {
                response.ShoppingCart.FormofPaymentDetails.TravelCreditDetails.TravelCreditSummary = string.Empty;
            }

            response.ShoppingCart = shoppingCart;
            var tupleRespEligibleFormofPayments = await _mSCFormsOfPayment.GetEligibleFormofPayments(request, session, shoppingCart, request.CartId, request.Flow, response.Reservation);
            response.EligibleFormofPayments = tupleRespEligibleFormofPayments.EligibleFormofPayments;
            isDefault = tupleRespEligibleFormofPayments.isDefault;
            response.IsDefaultPaymentOption = isDefault;
           

            await response.Reservation.Initialise(_configuration, _cachingService);
            await response.Reservation.UpdateRewards(_configuration, _cachingService);
            response.Reservation.CartId = request.CartId;


            if (_configuration.GetValue<bool>("EnableUpliftPayment"))
            {
                //need to set delete order to true, so client will delete uplift order when landing on RTI first time
                response.EligibleFormofPayments?.ForEach(e => e.DeleteOrder = e.Code == "UPLIFT");
            }


            //Covid 19 Emergency WHO TPI content
            if (_configuration.GetValue<bool>("ToggleCovidEmergencytextTPI") == true)
            {
                bool return_TPICOVID_19WHOMessage_For_BackwardBuilds = GeneralHelper.IsApplicationVersionGreater2(request.Application.Id, request.Application.Version.Major, "Android_Return_TPICOVID_19WHOMessage__For_BackwardBuilds", "iPhone_Return_TPICOVID_19WHOMessage_For_BackwardBuilds", null, null, _configuration);
                if (!return_TPICOVID_19WHOMessage_For_BackwardBuilds && response.Reservation != null
                    && response.Reservation.TripInsuranceInfoBookingPath != null && response.Reservation.TripInsuranceInfoBookingPath.TPIAIGReturnedMessageContentList != null
                    && response.Reservation.TripInsuranceInfoBookingPath.TPIAIGReturnedMessageContentList.Count > 0)
                {
                    MOBItem tpiCOVID19EmergencyAlert = response.Reservation.TripInsuranceInfoBookingPath.TPIAIGReturnedMessageContentList.Find(p => p.Id.ToUpper().Trim() == "COVID19EmergencyAlert".ToUpper().Trim());
                    if (tpiCOVID19EmergencyAlert != null)
                    {
                        response.Reservation.TripInsuranceInfoBookingPath.Tnc = response.Reservation.TripInsuranceInfoBookingPath.Tnc +
                            "<br><br>" + tpiCOVID19EmergencyAlert.CurrentValue;
                    }
                }
            }

            ///Add key IOSOASeatMapWorkAroundFlag value as false in config to ByPass
            if (_configuration.GetValue<bool>("IOSOASeatMapWorkAroundFlag"))
            {
                if (response.Reservation != null && response.Reservation.TravelersCSL != null && response.Reservation.TravelersCSL.Count > 0)
                {
                    MapPrimaryTravelerNoOfSeatsToAllTravelers(response.Reservation.TravelersCSL);
                }
            }

            #region Express checkout Flow to hide or show travel options in Final RTI Screen
            if (await _featureToggles.IsEnabledExpressCheckoutFlow(request.Application.Id, request.Application.Version.Major, session?.CatalogItems).ConfigureAwait(false)
                && response != null 
                && response.Reservation != null 
                && response.Reservation.ShopReservationInfo2 != null 
                && response.Reservation.ShopReservationInfo2.IsExpressCheckoutPath)
            {
                try
                {
                    MOBBookingBundlesResponse bundleResponse = new MOBBookingBundlesResponse(_configuration);
                    bundleResponse = await _sessionHelperService.GetSession<MOBBookingBundlesResponse>(request.SessionId, bundleResponse.ObjectName, new List<string>() { request.SessionId, bundleResponse.ObjectName });
                    var hideBundles = true;
                    if (bundleResponse != null && bundleResponse.Products != null && bundleResponse.Products.Count > 0)
                    {
                        hideBundles = false;
                    }
                    // Save hideTravelOptionsOnRTI property value on session
                    United.Persist.Definition.Shopping.Reservation bookingPathReservation = new Persist.Definition.Shopping.Reservation();
                    bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, bookingPathReservation.ObjectName, new List<string> { request.SessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);
                    if (bookingPathReservation != null && bookingPathReservation.ShopReservationInfo2 != null)
                    {
                        bookingPathReservation.ShopReservationInfo2.HideTravelOptionsOnRTI = hideBundles;
                        await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, request.SessionId, new List<string> { request.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);
                    }

                    //Save hideTravelOptionsOnRTI property value in response
                    response.Reservation.ShopReservationInfo2.HideTravelOptionsOnRTI = hideBundles;
                }
                catch (Exception ex)
                {
                    _logger.LogError("RegisterSeatsForBooking - ExpressCheckout Exception {error} and SessionId {sessionId}", ex.Message, response.SessionId);
                }
            }
            #endregion

            return await Task.FromResult(response);
        }

        public async Task<MOBReshopRegisterSeatsResponse> RegisterSeatsForReshop(MOBRegisterSeatsRequest request)
        { 
            var response = new MOBReshopRegisterSeatsResponse();
            var shoppingCart = new MOBShoppingCart();
            bool isDefault = false;

            Session session = null;
            session = await _shoppingSessionHelper.GetValidateSession(request.SessionId, false, true);
            session.Flow = request.Flow;

            if (string.IsNullOrEmpty(request.CartId))
            {
                request.CartId = await CreateCart(request, session);

                MOBShoppingCart persistShoppingCart = new MOBShoppingCart();

                await _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, request.SessionId, new List<string> { request.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName);
            }

            var registerSeats = await RegisterSeats(request, session);
            response = ConvertBookingToReshopRegisterSeatsResponse(registerSeats, request);

            shoppingCart = response.ShoppingCart;
            shoppingCart = await _shoppingCartUtility.ReservationToShoppingCart_DataMigration(response.Reservation, shoppingCart, request);
            shoppingCart.Flow = request.Flow;
            response.ShoppingCart = shoppingCart;
            var tupleRespEligibleFormofPayments = await _mSCFormsOfPayment.GetEligibleFormofPayments(request, session, shoppingCart, request.CartId, request.Flow);
            response.EligibleFormofPayments = tupleRespEligibleFormofPayments.EligibleFormofPayments;
            isDefault = tupleRespEligibleFormofPayments.isDefault;
            response.IsDefaultPaymentOption = isDefault;
            response.Reservation.CartId = request.CartId;
            response.DOTBagrules(_configuration);
            await response.Reservation.Initialise(_configuration, _cachingService);
            return await Task.FromResult(response);
        }

        public async Task<MOBBookingRegisterOfferResponse> RemoveAncillaryOffer(MOBRemoveAncillaryOfferRequest request)
        {
            var response = new MOBBookingRegisterOfferResponse();
            try
            {
                await response.Reservation.UpdateRewards(_configuration, _cachingService);
                MOBShoppingCart shoppingCart = new MOBShoppingCart();
                Session session = null;
                session = await _shoppingSessionHelper.GetValidateSession(request.SessionId, false, true);
                response = await RemoveAncillaryOffer(request, session);
                shoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, shoppingCart.ObjectName, new List<string> { request.SessionId, shoppingCart.ObjectName }).ConfigureAwait(false);
                shoppingCart = await _shoppingCartUtility.ReservationToShoppingCart_DataMigration(response.Reservation, shoppingCart, request, false, session);
                if (_shoppingCartUtility.IsEnableMoneyPlusMilesFeature(request.Application.Id, request.Application.Version.Major, session?.CatalogItems))
                {
                    bool isDefault = false;
                    var tupleRespEligibleFormofPayments = await _mSCFormsOfPayment.GetEligibleFormofPayments(request, session,
                    response.ShoppingCart, request.CartId, request.Flow, response.Reservation);
                    response.EligibleFormofPayments = tupleRespEligibleFormofPayments.EligibleFormofPayments;
                    isDefault = tupleRespEligibleFormofPayments.isDefault;
                }
                response.ShoppingCart = shoppingCart;
            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("RemoveAncillaryOffer UnitedException {@UnitedException}", JsonConvert.SerializeObject(uaex));
                if (uaex != null && !string.IsNullOrEmpty(uaex.Message))
                {
                    response.Exception = new MOBException();
                    response.Exception.Code = (string.IsNullOrEmpty(uaex.Code)) ? "9999" : uaex.Code.ToString();
                    response.Exception.Message = uaex.Message.Trim();
                    response.Exception.ErrMessage = $"{uaex.InnerException?.Message} - {uaex.StackTrace}";
                }
                else
                {
                    response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
            }
            return await Task.FromResult(response);

        }

        public async Task<MOBBookingRegisterOfferResponse> UnRegisterAncillaryOffersForBooking(MOBRegisterOfferRequest request)
        {
            var response = new MOBBookingRegisterOfferResponse();
            MOBShoppingCart shoppingCart = new MOBShoppingCart();

            _logger.LogInformation("UnRegisterAncillaryOffersForBooking Request:{request} with SessionId:{sessionId}", JsonConvert.SerializeObject(request), request.SessionId);

            var session = await _shoppingSessionHelper.GetValidateSession(request.SessionId, false, true);
            await UnRegisterAncillaryOffersForBooking(request, session);

            shoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, shoppingCart.ObjectName, new List<string> { request.SessionId, shoppingCart.ObjectName }).ConfigureAwait(false);

            response.ShoppingCart = shoppingCart;
            return await Task.FromResult(response);
        }



        public async Task<MOBBookingRegisterOfferResponse> ClearCartOnBackNavigation(MOBClearCartOnBackNavigationRequest request)
        {
            var response = new MOBBookingRegisterOfferResponse();
            MOBShoppingCart shoppingCart = new MOBShoppingCart();

            Session session = null;
            session = await _shoppingSessionHelper.GetValidateSession(request.SessionId, false, true);
            response = await ClearCartOnBackNavigation(request, session);
            shoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, shoppingCart.ObjectName, new List<string> { request.SessionId, shoppingCart.ObjectName }).ConfigureAwait(false);

            shoppingCart = await _shoppingCartUtility.ReservationToShoppingCart_DataMigration(response.Reservation, shoppingCart, request, false, session);
            response.ShoppingCart = shoppingCart;
            response.SessionId = request.SessionId;
            response.TransactionId = request.TransactionId;
            response.Reservation.CartId = request.CartId;
            if (!_configuration.GetValue<bool>("DisableTravelerNameDisplayinCartAtSoftRTIFix") && !string.IsNullOrEmpty(request.ClearOption) && string.Equals(request.ClearOption, CartClearOption.ClearBundles.ToString(), StringComparison.OrdinalIgnoreCase) && shoppingCart?.SCTravelers?.Count > 0)
            {
                response.ShoppingCart.SCTravelers = null;
            }
            if (!string.IsNullOrEmpty(request.ClearOption) && string.Equals(request.ClearOption, CartClearOption.ClearBundles.ToString(), StringComparison.OrdinalIgnoreCase) && response?.ShoppingCart?.OmniCart != null)
            {
                response.ShoppingCart.OmniCart.IsUpliftEligible = false;
            }
            return await Task.FromResult(response);
        }

        public async Task<MOBSHOPSelectTripResponse> RegisterOffersForOmniCartSavedTrip(MOBSHOPUnfinishedBookingRequestBase request)
        {
            bool isDefault = false;
            var response = new MOBSHOPSelectTripResponse();
            var shoppingCart = new MOBShoppingCart();

            var session = await _sessionHelperService.GetSession<Session>(request.SessionId, new Session().ObjectName, new List<string> { request.SessionId, new Session().ObjectName }).ConfigureAwait(false);

            if (session == null)
            {
                throw new MOBUnitedException(_configuration.GetValue<string>("OmnicartExceptionMessage"));
            }

            await ValidateVulnerability(request.MileagePlusAccountNumber, request.PasswordHash, request.Application.Id, request.DeviceId
                                 , request.Application.Version.Major, _configuration.GetValue<string>("OmnicartExceptionMessage"), session.SessionId, request.TransactionId);

            response = await RegisterOffersForOmniCartSavedTrips(session, request);
            response.Availability.SessionId = request.SessionId;

            shoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, shoppingCart.ObjectName, new List<string> { request.SessionId, shoppingCart.ObjectName }).ConfigureAwait(false);

            shoppingCart = await _shoppingCartUtility.ReservationToShoppingCart_DataMigration(response.Availability.Reservation, shoppingCart, request,session: session);
            shoppingCart.Flow = FlowType.BOOKING.ToString();
            response.ShoppingCart = shoppingCart;

            await _sessionHelperService.SaveSession<MOBShoppingCart>(response.ShoppingCart, request.SessionId, new List<string> { request.SessionId, shoppingCart.ObjectName }, shoppingCart.ObjectName);

            //Persist.FilePersist.Save<MOBShoppingCart>(request.SessionId, response.ShoppingCart.GetType().ToString(), response.ShoppingCart);

            var tupleRespEligibleFormofPayments  = await _mSCFormsOfPayment.GetEligibleFormofPayments(request, session, shoppingCart, request.CartId, FlowType.BOOKING.ToString(), response.Availability.Reservation);
            response.EligibleFormofPayments = tupleRespEligibleFormofPayments.EligibleFormofPayments;
            isDefault = tupleRespEligibleFormofPayments.isDefault;
            response.IsDefaultPaymentOption = isDefault;
            response.CartId = shoppingCart.CartId;
            //Covid-19 Emergency WHO TPI content
            if (_configuration.GetValue<bool>("ToggleCovidEmergencytextTPI") == true)
            {
                bool return_TPICOVID_19WHOMessage_For_BackwardBuilds = GeneralHelper.IsApplicationVersionGreater2(request.Application.Id, request.Application.Version.Major, "Android_Return_TPICOVID_19WHOMessage__For_BackwardBuilds", "iPhone_Return_TPICOVID_19WHOMessage_For_BackwardBuilds", null, null, _configuration);
                if (!return_TPICOVID_19WHOMessage_For_BackwardBuilds && response.Availability.Reservation != null
                    && response.Availability.Reservation.TripInsuranceInfoBookingPath != null && response.Availability.Reservation.TripInsuranceInfoBookingPath.TPIAIGReturnedMessageContentList != null
                    && response.Availability.Reservation.TripInsuranceInfoBookingPath.TPIAIGReturnedMessageContentList.Count > 0)
                {
                    MOBItem tpiCOVID19EmergencyAlertBookingPath = response.Availability.Reservation.TripInsuranceInfoBookingPath.TPIAIGReturnedMessageContentList.Find(p => p.Id.ToUpper().Trim() == "COVID19EmergencyAlert".ToUpper().Trim());
                    if (tpiCOVID19EmergencyAlertBookingPath != null)
                    {
                        response.Availability.Reservation.TripInsuranceInfoBookingPath.Tnc = response.Availability.Reservation.TripInsuranceInfoBookingPath.Tnc +
                            "<br><br>" + tpiCOVID19EmergencyAlertBookingPath.CurrentValue;
                    }
                }
            }
            return await Task.FromResult(response);
        }

        public MOBSection GetPromoCodeAlertMessage(bool isContinueToRegisterAncillary)
        {
            if (_configuration.GetValue<bool>("EnableCouponsforBooking") && isContinueToRegisterAncillary)
            {
                var changeInTravelerMessage = _configuration.GetValue<string>("PromoCodeRemovalmessage");
                return string.IsNullOrWhiteSpace(changeInTravelerMessage) ? null : new MOBSection
                {
                    Text1 = changeInTravelerMessage,
                    Text2 = "Cancel",
                    Text3 = "Continue"
                };
            }
            return null;
        }

        private async System.Threading.Tasks.Task ValidateVulnerability(string mpNumber, string hashPinCode, int applicationId, string deviceId, string appMajorVersion, string exceptionMessage, string sessionId, string transactionId)
        {
            if (!string.IsNullOrEmpty(mpNumber) &&
               (_configuration.GetValue<bool>("EnableVulnerabilityFixForUnfinishedBooking")
               || GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appMajorVersion, _configuration.GetValue<string>("AndroidVulnerabilityFixVersion"), _configuration.GetValue<string>("iPhoneVulnerabilityFixVersion"))))
            {
                bool validOmniCartRequest = false;
                string authToken = string.Empty;
                try
                {
                    _logger.LogInformation("ValidateVulnerability - ValidateHashPinAndGetAuthTokenNew Call {mpNumber} {hashPinCode} {applicationId} {deviceId} and {appMajorVersion}", mpNumber, hashPinCode, applicationId, deviceId, appMajorVersion);
                    var mpResponse = await new HashPin(_logger, _configuration, _validateHashPinService, _dynamoDBService,_headers, _featureSettings).ValidateHashPinAndGetAuthTokenDynamoDB(mpNumber, hashPinCode, applicationId, deviceId, appMajorVersion).ConfigureAwait(false);
                    if (mpResponse != null && mpResponse.HashPincode == hashPinCode)
                    {
                        validOmniCartRequest = true;
                        authToken = mpResponse.AuthenticatedToken?.ToString();
                    }

                }
                catch (Exception ex) { string msg = ex.Message; }

                if (validOmniCartRequest == false && _configuration.GetValue<string>("ByPassMPByPassCheckForDpMPSignCall2_1_41") != null &&
                    _configuration.GetValue<string>("ByPassMPByPassCheckForDpMPSignCall2_1_41").ToUpper().Trim() == appMajorVersion.ToUpper().Trim())
                {
                    var deviceDynamodb = new DeviceDynamDB(_configuration, _dynamoDBService);
                    validOmniCartRequest = await deviceDynamodb.ValidateDeviceIDAPPID(deviceId, applicationId, mpNumber, appMajorVersion);
                }
                if (!validOmniCartRequest)
                    throw new MOBUnitedException(exceptionMessage);
            }
        }


        private MOBRESHOPRegisterOfferResponse ConvertBookingToReshopRegisterOffersResponse(MOBBookingRegisterOfferResponse bookingRegisterOfferResponse)
        {
            MOBRESHOPRegisterOfferResponse response = new MOBRESHOPRegisterOfferResponse();
            response.Reservation = bookingRegisterOfferResponse.Reservation;
            response.ShoppingCart = bookingRegisterOfferResponse.ShoppingCart;
            response.Exception = bookingRegisterOfferResponse.Exception;
            response.CallDuration = bookingRegisterOfferResponse.CallDuration;
            response.LanguageCode = bookingRegisterOfferResponse.LanguageCode;
            response.MachineName = bookingRegisterOfferResponse.MachineName;
            response.TransactionId = bookingRegisterOfferResponse.TransactionId;
            response.EligibleFormofPayments = bookingRegisterOfferResponse.EligibleFormofPayments;
            response.CheckinSessionId = bookingRegisterOfferResponse.CheckinSessionId;
            response.Disclaimer = bookingRegisterOfferResponse.Disclaimer;
            response.Flow = "RESHOP";
            response.IsDefaultPaymentOption = bookingRegisterOfferResponse.IsDefaultPaymentOption;
            response.PkDispenserPublicKey = bookingRegisterOfferResponse.PkDispenserPublicKey;
            response.SessionId = response.SessionId;
            return response;
        }


        private async Task<MileagePlusDetails> ValidateHashPinAndGetAuthTokenDynamoDB(string accountNumber, string hashPinCode
           , int applicationId, string deviceId, string appVersion, string sessionid = "")
        {
            var mileagePlusDynamoDB = new MileagePlusDynamoDB(_configuration, _dynamoDBService);
            var hashResponse = await mileagePlusDynamoDB.ValidateHashPinAndGetAuthToken<MileagePlusDetails>(accountNumber, hashPinCode, applicationId, deviceId, appVersion, _headers.ContextValues.SessionId).ConfigureAwait(false);

            if (!await _featureSettings.GetFeatureSettingValue("DisableLoggingHashPinCodeEmpty").ConfigureAwait(false)
                && hashResponse != null && string.IsNullOrEmpty(hashResponse.HashPincode))
            {
                _logger.LogWarning("DynamoDB -GetRecords Hashpincode is null/empty.");
            }
            bool isAuthorized = false;
            if (hashResponse?.HashPincode == hashPinCode)
            {
                isAuthorized = true;
                return hashResponse;
            }
            else
            {
                _logger.LogInformation("ValidateHashPinAndGetAuthToken - OnPremSQL Request {accountNumber} {applicationId} {transactionId} and {SessionId}", accountNumber, applicationId, _headers.ContextValues.TransactionId, _headers.ContextValues.SessionId);
                var hashpinResponse = await _validateHashPinService.ValidateHashPin<HashPinValidate>(accountNumber, hashPinCode, applicationId, deviceId, appVersion, _headers.ContextValues.TransactionId, _headers.ContextValues.SessionId).ConfigureAwait(false);
                _logger.LogInformation("ValidateHashPinAndGetAuthToken - OnPremSQL Response {response} {transactionId} and {SessionId}", JsonConvert.SerializeObject(hashpinResponse), _headers.ContextValues.TransactionId, _headers.ContextValues.SessionId);

                var authToken = hashpinResponse?.validAuthToken;
                isAuthorized = true;
                var Data = new MileagePlusDetails()
                {
                    MileagePlusNumber = accountNumber,
                    MPUserName = accountNumber,
                    HashPincode = hashPinCode,
                    PinCode = _configuration.GetValue<bool>("LogMPPinCodeOnlyForTestingAtStage") ? hashPinCode : string.Empty,
                    ApplicationID = Convert.ToString(applicationId),
                    AppVersion = appVersion,
                    DeviceID = deviceId,
                    IsTokenValid = Convert.ToString(isAuthorized),
                    TokenExpireDateTime = string.Empty,
                    TokenExpiryInSeconds = string.Empty,
                    IsTouchIDSignIn = string.Empty,
                    IsTokenAnonymous = string.Empty,
                    CustomerID = string.Empty,
                    DataPowerAccessToken = (hashpinResponse?.IsValidHashPin == true) ? authToken : string.Empty,
                    UpdateDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.FFF")
                };

                string Key = string.Format("{0}::{1}::{2}", accountNumber, applicationId, deviceId);

                _logger.LogInformation("ValidateHashPinAndGetAuthToken - DynamoDB Request", Data);
                bool jsonResponse = await new MPValidationCSSDynamoDB(_configuration, _dynamoDBService).SaveRecords<MileagePlusDetails>(Data, _headers.ContextValues.SessionId, Key, accountNumber, _headers.ContextValues.TransactionId).ConfigureAwait(false);
                _logger.LogInformation("ValidateHashPinAndGetAuthToken - DynamoDB {Response} with {sessionID}", JsonConvert.SerializeObject(jsonResponse), sessionid);

                if (!string.IsNullOrEmpty(authToken))
                {
                    var hashResp = new MileagePlusDetails()
                    {
                        HashPincode = hashPinCode,
                        AuthenticatedToken = authToken
                    };

                    return hashResp;
                }
            }
            if (!isAuthorized)
            {
                _logger.LogError("ValidateHashPinAndGetAuthToken-UnAutorizedm with {sessionID}", isAuthorized, sessionid);
            }
            return default(MileagePlusDetails);
        }

        private bool IsEnableOmniCartReleaseCandidateThreeChanges_Seats(int applicationId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableOmniCartReleaseCandidateThreeChanges_Seats") && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Android_EnableOmniCartReleaseCandidateThreeChanges_Seats_AppVersion"), _configuration.GetValue<string>("iPhone_EnableOmniCartReleaseCandidateThreeChanges_Seats_AppVersion"));
        }

        public async Task UnRegisterAncillaryOffersForBooking(MOBRegisterOfferRequest request, Session session)
        {

            try
            {
                if (request.MerchandizingOfferDetails?.Any() == true)
                {
                    foreach (var merchOfferDetail in request.MerchandizingOfferDetails)
                    {
                        if (merchOfferDetail.ProductCode == "SEATASSIGNMENTS")
                        {
                            var mobRegisterSeatsRequest = new MOBRegisterSeatsRequest();
                            mobRegisterSeatsRequest.CartId = request.CartId;
                            mobRegisterSeatsRequest.Application = request.Application;
                            mobRegisterSeatsRequest.SessionId = request.SessionId;
                            mobRegisterSeatsRequest.Flow = request.Flow;
                            mobRegisterSeatsRequest.IsRemove = true;
                          await CompleteSeatsV2(mobRegisterSeatsRequest, session, updateSeatsAfterBundlesChanged: false, isClearSeats: true);
                        }
                        else
                        {
                          await  RegisterOffers(request, session);
                        }
                    }
                }
            }
            catch (System.Net.WebException wex)
            {
                if (wex.Response != null)
                {
                    //if (levelSwitch.TraceInfo)
                    //{
                    //    var errorResponse = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd();
                    //    shopping.LogEntries.Add(United.Logger.LogEntry.GetLogEntry<string>(request.SessionId, "UnRegisterAncillaryOffersForBooking", "ErrorMessageResponse", request.Application.Id, request.Application.Version.Major, request.DeviceId, errorResponse, true, false));
                    //}
                    _logger.LogError("UnRegisterAncillaryOffersForBooking SessionId:{Id}, Exception:{ex}", request.SessionId, wex.Message);

                }
                throw;
            }
            catch (MOBUnitedException uaex)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(uaex.InnerException ?? uaex).Throw();
            }
            catch (System.Exception ex)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException ?? ex).Throw();
            }
            finally
            {
                //TODO:Check with Dhanush
                //if (shopping.LogEntries != null && shopping.LogEntries.Count > 0)
                //{
                //    LogEntries.AddRange(shopping.LogEntries);
                //}
                //if (formofPayment.LogEntries != null && formofPayment.LogEntries.Count > 0)
                //{
                //    LogEntries.AddRange(shopping.LogEntries);
                //}
            }
        }

        private async Task BuildTravelerSeatDetails(FlightReservationResponse flightReservationResponse, int appId, String appVersion, string sessionId)
        {
            if (IsEnableOmniCartReleaseCandidateThreeChanges_Seats(appId, appVersion))
            {
                if (flightReservationResponse.DisplayCart?.DisplaySeats != null &&
                       flightReservationResponse.DisplayCart.DisplaySeats.Any(displaySeat => !string.IsNullOrEmpty(displaySeat.Seat)))
                {
                    Reservation bookingPathReservation = new Reservation();
                    bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(sessionId, bookingPathReservation.ObjectName, new List<string> { sessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);

                    foreach (var traveler in bookingPathReservation.TravelersCSL)
                    {
                        if (traveler.Value.TravelerTypeCode != "INF")//No need to builds the seat details for Infant on Lap as we dont allow seat for infant
                        {
                            traveler.Value.Seats = new List<MOBSeat>();
                            AssignDefaultSeats(bookingPathReservation, traveler.Value);
                            var travelerSeats = flightReservationResponse.DisplayCart.DisplaySeats.Where(displaySeat => displaySeat.PersonIndex == traveler.Value.TravelerNameIndex);
                            foreach (var travelerSeat in travelerSeats)
                            {

                                var travelerSelectedSeatForSegment = traveler.Value?
                                                                                       .Seats?
                                                                             .FirstOrDefault(seat => seat.Origin == travelerSeat.DepartureAirportCode
                                                                                         && GetFinalDestinationForThruFlight(seat.Origin, seat.Destination, bookingPathReservation) == travelerSeat.ArrivalAirportCode
                                                                                         && seat.FlightNumber == travelerSeat.FlightNumber);
                                if (travelerSelectedSeatForSegment != null)
                                {
                                    travelerSelectedSeatForSegment.SeatAssignment = travelerSeat.Seat;
                                    travelerSelectedSeatForSegment.SeatType = travelerSeat.SeatType;
                                    travelerSelectedSeatForSegment.PriceAfterTravelerCompanionRules = travelerSeat.OriginalPrice;
                                    travelerSelectedSeatForSegment.Currency = travelerSeat.Currency;
                                    travelerSelectedSeatForSegment.Destination = travelerSeat.ArrivalAirportCode;
                                    travelerSelectedSeatForSegment.Origin = travelerSeat.DepartureAirportCode;
                                    travelerSelectedSeatForSegment.ProgramCode = travelerSeat.SeatPromotionCode;
                                    travelerSelectedSeatForSegment.OldSeatProgramCode = travelerSeat.ProductCode;
                                    if (!String.IsNullOrEmpty(travelerSeat.PromotionalCouponCode))
                                    {
                                        travelerSelectedSeatForSegment.PromotionalCouponCode = travelerSeat.PromotionalCouponCode;
                                        travelerSelectedSeatForSegment.PriceAfterCouponApplied = travelerSeat.SeatPrice;
                                    }
                                    else
                                        travelerSelectedSeatForSegment.PriceAfterTravelerCompanionRules = travelerSeat.SeatPrice;
                                    travelerSelectedSeatForSegment.Price = travelerSeat.SeatPrice;
                                    travelerSelectedSeatForSegment.FlightNumber = travelerSeat.FlightNumber;
                                    travelerSelectedSeatForSegment.TravelerSharesIndex = traveler.Value.PaxIndex.ToString();
                                }
                            }
                        }
                    }
                    List<MOBSeatPrice> seatPrices = BuildSeatPricesForPriceBreakDown(bookingPathReservation.TravelersCSL.Values.SelectMany(traveler => traveler.Seats).ToList());
                    bookingPathReservation.SeatPrices = new List<MOBSeatPrice>();
                    bookingPathReservation.SeatPrices = SortAndOrderSeatPrices(seatPrices);

                    await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, sessionId, new List<string> { sessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);

                }
            }
        }

        private void AssignDefaultSeats(United.Persist.Definition.Shopping.Reservation reservation, MOBCPTraveler traveler)
        {
            int key = 0;
            foreach (var trip in reservation.Trips)
            {
                foreach (var flattenedFlight in trip.FlattenedFlights)
                {
                    foreach (var flight in flattenedFlight.Flights)
                    {
                        if (!IsThruFlight(flight))
                        {
                            if (traveler.Seats == null)
                                traveler.Seats = new List<MOBSeat>();
                            traveler.Seats.Add(new MOBSeat
                            {
                                Destination = flight.Destination,
                                Origin = flight.Origin,
                                FlightNumber = flight.FlightNumber,
                                UAOperated = (flight.OperatingCarrier.ToUpper() == "UA"),
                                Key = key,
                                TravelerSharesIndex = traveler.PaxIndex.ToString(),
                                SeatAssignment = string.Empty
                            });
                            key++;
                        }
                    }
                }
            }
        }


        private bool IsThruFlight(MOBSHOPFlight flight)
        {
            if (!_configuration.GetValue<bool>("DisableThruFlightSeatNotAssigningFix"))
            {
                return flight.IsStopOver && !flight.ChangeOfPlane;
            }

            return flight.IsStopOver;
        }

        private bool IsSeatsRegistered(FlightReservationResponse flightReservationResponse)
        {
            return flightReservationResponse.DisplayCart?.DisplaySeats != null &&
                       flightReservationResponse.DisplayCart.DisplaySeats.Any(displaySeat => !string.IsNullOrEmpty(displaySeat.Seat));
        }

        private async Task<MOBSHOPSelectTripResponse> RegisterOffersForOmniCartSavedTrips(Session session, MOBSHOPUnfinishedBookingRequestBase request)
        {
            MOBSHOPSelectTripResponse registerOffersResponse = new MOBSHOPSelectTripResponse();
            registerOffersResponse.Availability = new MOBSHOPAvailability();
            MOBBookingRegisterOfferResponse registerBundleResponse = new MOBBookingRegisterOfferResponse();
            Reservation persistedReservation = new Reservation();

            persistedReservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, persistedReservation.ObjectName, new List<string> { request.SessionId, persistedReservation.ObjectName }).ConfigureAwait(false);

            var flightReservationResponse = await GetFlightReservationResponseByCartId(request.SessionId, request.CartId);
            registerBundleResponse = _shoppingCartUtility.IsEnableOmniCartMVP2Changes(request.Application.Id, request.Application.Version.Major, persistedReservation?.ShopReservationInfo2?.IsDisplayCart == true) ? await RegisterBundlesV2(BuildRegisterOffersRequest(request, flightReservationResponse), session) : await RegisterBundles(BuildRegisterOffersRequest(request, flightReservationResponse), session);
            registerOffersResponse.Availability.Reservation = registerBundleResponse.Reservation;
            registerOffersResponse.ShoppingCart = registerOffersResponse.ShoppingCart;
            if (IsEnableOmniCartReleaseCandidateThreeChanges_Seats(request.Application.Id, request.Application.Version.Major)
                && IsSeatsRegistered(flightReservationResponse))
            {
                #region OmnicartRelease Candidate 1.3 retaining seats
                await BuildTravelerSeatDetails(flightReservationResponse, request.Application.Id, request.Application.Version.Major, request.SessionId);
                MOBBookingRegisterSeatsResponse registerSeatsResponse = new MOBBookingRegisterSeatsResponse();
                registerSeatsResponse = await RegisterSeats(BuildRegisterSeatsRequest(request), session);
                registerOffersResponse.Availability.Reservation = registerSeatsResponse.Reservation;
                registerOffersResponse.ShoppingCart = registerSeatsResponse.ShoppingCart;
                #endregion
            }
            return registerOffersResponse;
        }

        private void MapPrimaryTravelerNoOfSeatsToAllTravelers(List<MOBCPTraveler> travelersCSL)
        {
            var profileOwner = travelersCSL.FindAll(p => p != null && p.Seats != null).OrderByDescending(p => p.Seats.Count).First();
            var travelers = travelersCSL.Where(p => p != null && p != profileOwner);
            if (profileOwner != null && profileOwner.Seats != null && profileOwner.Seats.Count > 0)
            {
                int profileOwnerSeatCount = profileOwner.Seats.Count;
                foreach (var traveler in travelers)
                {
                    int noOfSeatsToAdd = 0;
                    if (traveler.Seats == null)
                    {
                        traveler.Seats = new List<MOBSeat>();
                        noOfSeatsToAdd = profileOwnerSeatCount;
                    }
                    else
                    {
                        noOfSeatsToAdd = profileOwner.Seats.Count - traveler.Seats.Count;
                    }

                    if (traveler != null && noOfSeatsToAdd > 0)
                    {
                        traveler.Seats.AddRange(Enumerable.Repeat(new MOBSeat(), profileOwner.Seats.Count - traveler.Seats.Count).ToList());
                    }
                }
            }
        }

        #region "RemoveAncillaryOffer"

        private async Task<MOBBookingRegisterOfferResponse> ClearCartOnBackNavigation(MOBClearCartOnBackNavigationRequest request, Session session)
        {
            MOBBookingRegisterOfferResponse backNavigationRegisterOfferResponse = new MOBBookingRegisterOfferResponse();

            try
            {
                var nextViewName = string.Empty;
                MOBShoppingCart persistShoppingCart = new MOBShoppingCart();
                if (string.IsNullOrEmpty(request.ClearOption))
                {
                    throw new Exception("ClearOption value is empty in the request");
                }
                persistShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(session.SessionId, persistShoppingCart.ObjectName, new List<string> { session.SessionId, persistShoppingCart.ObjectName }).ConfigureAwait(false);
                var persistedReservation = new United.Persist.Definition.Shopping.Reservation();
                persistedReservation = await _sessionHelperService.GetSession<Reservation>(session.SessionId, persistedReservation.ObjectName, new List<string> { session.SessionId, persistedReservation.ObjectName }).ConfigureAwait(false);

                //if (request.IsBeforeRTI) 
                {
                    if (string.Equals(request.ClearOption, CartClearOption.ClearSeats.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        //check any seat assignment is there or not from shopping cart products
                        nextViewName = "TravelOption";
                        if (persistShoppingCart != null && persistedReservation != null)
                        {
                            backNavigationRegisterOfferResponse.ShoppingCart = persistShoppingCart;
                            var seatProduct = persistShoppingCart.Products?.FirstOrDefault(product => string.Equals(product.Code, "SEATASSIGNMENTS", StringComparison.OrdinalIgnoreCase));
                            if (seatProduct != null)
                            {
                                var mobRegisterSeatsRequest = new MOBRegisterSeatsRequest();
                                mobRegisterSeatsRequest.CartId = request.CartId;
                                mobRegisterSeatsRequest.Application = request.Application;
                                mobRegisterSeatsRequest.SessionId = request.SessionId;
                                mobRegisterSeatsRequest.Flow = request.Flow;
                                mobRegisterSeatsRequest.IsRemove = true;
                                var registerSeatsResponse = await CompleteSeatsV2(mobRegisterSeatsRequest, session, updateSeatsAfterBundlesChanged: false, isClearSeats: true);
                                backNavigationRegisterOfferResponse.Reservation = await _productUtility.GetReservationFromPersist(registerSeatsResponse.Reservation, session);
                                backNavigationRegisterOfferResponse.ShoppingCart = registerSeatsResponse.ShoppingCart;
                            }
                            else
                            {
                                backNavigationRegisterOfferResponse.Reservation = await _productUtility.GetReservationFromPersist(new MOBSHOPReservation(_configuration, _cachingService), session);
                            }

                        }

                    }
                    if (string.Equals(request.ClearOption, CartClearOption.ClearBundles.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        nextViewName = "TravelOption";
                        if (persistShoppingCart != null && persistedReservation != null)
                        {
                            backNavigationRegisterOfferResponse.ShoppingCart = persistShoppingCart;
                            MOBRegisterOfferRequest clearBundleRequest = new MOBRegisterOfferRequest();
                            var bundleProducts = persistShoppingCart.Products?.Where(product => 
                                                                                    string.Equals(product.ProdDescription, "BE", StringComparison.OrdinalIgnoreCase));
                            if (bundleProducts != null && bundleProducts.Any())
                            {
                                clearBundleRequest.Application = request.Application;
                                clearBundleRequest.DeviceId = request.DeviceId;
                                clearBundleRequest.SessionId = request.SessionId;
                                clearBundleRequest.TransactionId = request.TransactionId;
                                clearBundleRequest.CartId = request.CartId;
                                clearBundleRequest.Flow = request.Flow;

                                foreach (var product in bundleProducts)
                                {
                                    BuildClearOfferRequest(clearBundleRequest, product);
                                }
                                clearBundleRequest.IsRemove = true;
                                backNavigationRegisterOfferResponse = await RegisterBundlesV2(clearBundleRequest, session);
                            }
                            else
                            {
                                backNavigationRegisterOfferResponse.Reservation = await _productUtility.GetReservationFromPersist(new MOBSHOPReservation(_configuration, _cachingService), session);
                            }

                            // MOBILE-25395: SAF
                            if (ConfigUtility.IsEnableSAFFeature(session))
                            {
                                var safProduct = persistShoppingCart.Products?.FirstOrDefault(product => string.Equals(product.Code, _configuration.GetValue<string>("SAFCode"), StringComparison.OrdinalIgnoreCase));
                                if (safProduct != null)
                                {
                                    MOBRegisterOfferRequest clearSAFProductRequest = new MOBRegisterOfferRequest();
                                    clearSAFProductRequest.Application = request.Application;
                                    clearSAFProductRequest.DeviceId = request.DeviceId;
                                    clearSAFProductRequest.SessionId = request.SessionId;
                                    clearSAFProductRequest.TransactionId = request.TransactionId;
                                    clearSAFProductRequest.CartId = request.CartId;
                                    clearSAFProductRequest.Flow = request.Flow;
                                    BuildClearOfferRequest(clearSAFProductRequest, safProduct);
                                    clearSAFProductRequest.IsRemove = true;
                                    backNavigationRegisterOfferResponse = await RegisterOfferForSAF(clearSAFProductRequest, session);
                                }
                            }

                        }

                    }
                    if (_configuration.GetValue<bool>("EnableTravelInsuranceOptimization"))
                    {
                        if (string.Equals(request.ClearOption, CartClearOption.ClearTPI.ToString(), StringComparison.OrdinalIgnoreCase)
                           || string.Equals(request.ClearOption, CartClearOption.ClearBundles.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            var tpiProduct = persistShoppingCart.Products?.FirstOrDefault(product => string.Equals(product.Code, "TPI", StringComparison.OrdinalIgnoreCase));
                            if (tpiProduct != null)
                            {
                                MOBRegisterOfferRequest clearBundleRequest = new MOBRegisterOfferRequest();
                                clearBundleRequest.Application = request.Application;
                                clearBundleRequest.DeviceId = request.DeviceId;
                                clearBundleRequest.SessionId = request.SessionId;
                                clearBundleRequest.TransactionId = request.TransactionId;
                                clearBundleRequest.CartId = request.CartId;
                                clearBundleRequest.Flow = request.Flow;
                                BuildClearOfferRequest(clearBundleRequest, tpiProduct);
                                backNavigationRegisterOfferResponse = await RegisterOfferForTPI(clearBundleRequest, session);
                            }
                        }
                    }
                    persistedReservation = persistedReservation ?? new Reservation();
                    persistedReservation = await _sessionHelperService.GetSession<Reservation>(session.SessionId, persistedReservation.ObjectName, new List<String> { session.SessionId, persistedReservation.ObjectName }).ConfigureAwait(false);

                    if (persistedReservation != null && !string.IsNullOrEmpty(nextViewName))
                    {
                        //breaking
                        persistedReservation.ShopReservationInfo2.NextViewName = nextViewName;
                        backNavigationRegisterOfferResponse.Reservation.ShopReservationInfo2 = persistedReservation.ShopReservationInfo2;
                        await _sessionHelperService.SaveSession<Reservation>(persistedReservation, session.SessionId, new List<string> { session.SessionId, persistedReservation.ObjectName }, persistedReservation.ObjectName).ConfigureAwait(false);
                    }
                }


            }
            catch (System.Net.WebException wex)
            {
                if (wex.Response != null)
                {
                    //if (levelSwitch.TraceInfo)
                    //{
                    //    var errorResponse = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd();
                    //    //shopping.LogEntries.Add(United.Logger.LogEntry.GetLogEntry<string>(request.SessionId, "ClearCartOnBackNavigation", "ErrorMessageResponse", request.Application.Id, request.Application.Version.Major, request.DeviceId, errorResponse, true, false));
                    //}
                    _logger.LogError("ClearCartOnBackNavigation Exception:{WebException}", JsonConvert.SerializeObject(wex));
                }
                throw;
            }
            catch (MOBUnitedException uaex)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(uaex.InnerException ?? uaex).Throw();
            }
            catch (System.Exception ex)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException ?? ex).Throw();
            }
            finally
            {
                //if (shopping.LogEntries != null && shopping.LogEntries.Count > 0)
                //{
                //    LogEntries.AddRange(shopping.LogEntries);
                //}
            }
            return backNavigationRegisterOfferResponse;
        }

        private async Task<MOBBookingRegisterSeatsResponse> CompleteSeatsV2(MOBRegisterSeatsRequest request, Session session, bool updateSeatsAfterBundlesChanged = false, bool isClearSeats = false)
        {
            MOBBookingRegisterSeatsResponse response = new MOBBookingRegisterSeatsResponse();
            FlightReservationResponse cslResponse = new FlightReservationResponse();

            Reservation persistedReservation = new Reservation();

            var isCOGPartialSeatAssignmentFixEnabled = await _featureSettings.GetFeatureSettingValue("EnableCOGBookingPartialSeatAssignmentFix").ConfigureAwait(false);
            bool hasCOGFlight = false;

            if (isCOGPartialSeatAssignmentFixEnabled)
            {
                persistedReservation = await _sessionHelperService.GetSession<Reservation>(session.SessionId, persistedReservation.ObjectName, new List<string> { session.SessionId, persistedReservation.ObjectName }).ConfigureAwait(false);
                hasCOGFlight = persistedReservation?.Trips?.Any(trip => trip?.FlattenedFlights?.Any(ff => ff?.Flights?.Any(flight => flight?.ChangeOfGauge == true && flight.ChangeOfPlane == true) == true) == true) == true;
            }

            #region Booking cart change
            MOBShoppingCart shoppingCart = new MOBShoppingCart();
            shoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, shoppingCart.ObjectName, new List<string> { request.SessionId, shoppingCart.ObjectName }).ConfigureAwait(false);

            #endregion Booking cart change
            if (!(IsEnableOmniCartReleaseCandidateThreeChanges_Seats(request.Application.Id, request.Application.Version.Major) && request.IsOmniCartSavedTripFlow)
                && !request.IsRemove // is remove set to be true only when we are removing seats from Cart
                && (!request.IsDone || (isCOGPartialSeatAssignmentFixEnabled && request.IsDone && hasCOGFlight))) // isdont set be true only when user click done button from seat screen
            {
                //need to be executed at least one time

                response.Seats = updateSeatsAfterBundlesChanged ?
                                await GetAllTravelerSeats(request.SessionId) :
                                await FlightTravelerSeats(request.SessionId, request.SeatAssignment, request.Origin, request.Destination, request.PaxIndex, "", "");

                persistedReservation = await _sessionHelperService.GetSession<Reservation>(session.SessionId, persistedReservation.ObjectName, new List<string> { session.SessionId, persistedReservation.ObjectName }).ConfigureAwait(false);

                if (EnableAdvanceSearchCouponBooking(request.Application.Id, request.Application.Version.Major)
                    && request.ContinueToRegisterAncillary)
                {
                    if (!await AddOrRemovePromo(request, session, true, request.Flow))
                    {
                        throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                    }
                }
                int highestEliteLevel = 0;
                bool hasSeatPrice = false;
                bool hasSubscriptions = false;
                bool hasMPSavedTraveler = false;

                List<MOBSeat> allSeats = new List<MOBSeat>();
                List<MOBSeat> lstSeats = new List<MOBSeat>();

                if (persistedReservation != null && persistedReservation.TravelersCSL != null)
                {
                    foreach (MOBCPTraveler traveler in persistedReservation.TravelersCSL.Values)
                    {
                        if (traveler.MileagePlus != null && traveler.MileagePlus.CurrentEliteLevel > highestEliteLevel)
                        {
                            highestEliteLevel = traveler.MileagePlus.CurrentEliteLevel;
                        }
                        if (traveler.Seats != null && traveler.Seats.Count > 0)
                        {
                            foreach (var seat in traveler.Seats)
                            {
                                lstSeats.Add(seat);
                                if (seat.Price > 0)
                                {
                                    if (!hasSeatPrice)
                                    {
                                        hasSeatPrice = true;
                                    }
                                    allSeats.Add(seat);
                                }
                            }
                        }

                    }


                    if (highestEliteLevel <= 2)
                    {
                        if (persistedReservation != null && persistedReservation.TravelersCSL != null)
                        {
                            foreach (MOBCPTraveler traveler in persistedReservation.TravelersCSL.Values)
                            {
                                if (traveler.Subscriptions != null)
                                {
                                    hasSubscriptions = true;
                                }
                            }
                        }
                    }

                    // Kiran - Fix for bug AB-1724 [Saved travelers are not getting subscriptions]
                    // If not have elite level or subscriptions finding any mileage plus member is there in saved traveler to get benifits
                    if (_configuration.GetValue<bool>("EnableSubscriptionsForMPSavedTravelerBooking") && highestEliteLevel == 0 && !hasSubscriptions && !hasMPSavedTraveler
                        && persistedReservation.TravelersCSL.Any() && persistedReservation.TravelersCSL.Count > 1 && !persistedReservation.TravelerKeys.IsNullOrEmpty())
                    {
                        foreach (string travelerKey in persistedReservation.TravelerKeys)
                        {
                            MOBCPTraveler bookingTravelerInfo = persistedReservation.TravelersCSL[travelerKey];
                            if (bookingTravelerInfo.AirRewardPrograms != null && bookingTravelerInfo.AirRewardPrograms.Any())
                            {
                                bookingTravelerInfo.AirRewardPrograms.ForEach(p =>
                                {
                                    if (!p.IsNullOrEmpty() && !p.CarrierCode.IsNullOrEmpty() && p.CarrierCode.Trim().ToUpper() == "UA" && !hasMPSavedTraveler)
                                    {
                                        hasMPSavedTraveler = !p.MemberId.IsNullOrEmpty() ? true : false;
                                    }
                                });
                            }
                        }
                    }
                }

                if (hasSeatPrice && (highestEliteLevel > 0 || hasSubscriptions || hasMPSavedTraveler))
                {

                    var tupleResponse = await GetFinalTravelerSeatPrices(request.LanguageCode, response.Seats, request.SessionId, lstSeats, request.Application.Id, request.Application.Version.Major, request.DeviceId, request.SponsorMPAccountId, request.SponsorEliteLevel);
                    response.SeatPrices = tupleResponse.SeatPrice;
                    lstSeats = tupleResponse.lstSeats;
                    persistedReservation = await _sessionHelperService.GetSession<Reservation>(session.SessionId, persistedReservation.ObjectName, new List<string> { session.SessionId, persistedReservation.ObjectName }).ConfigureAwait(false);

                    persistedReservation.SeatPrices = response.SeatPrices;
                    await _sessionHelperService.SaveSession<Reservation>(persistedReservation, session.SessionId, new List<string> { session.SessionId, persistedReservation.ObjectName }, persistedReservation.ObjectName).ConfigureAwait(false);
                }
                else
                {
                    if (allSeats.Count > 0)
                    {
                        if (EnableAdvanceSearchCouponBooking(request.Application.Id, request.Application.Version.Major))
                        {
                            MOBShoppingCart persistShoppingCart = new MOBShoppingCart();
                            persistShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, persistShoppingCart.ObjectName, new List<string> { request.SessionId, persistShoppingCart.ObjectName }).ConfigureAwait(false);
                            if (IsAFSSeatCouponApplied(persistShoppingCart))
                            {
                                var tupleResponse = await GetFinalTravelerSeatPrices(request, request.LanguageCode, response.Seats, request.SessionId);
                                response.SeatPrices = tupleResponse.response;
                                lstSeats = tupleResponse.lstSeats;
                            }
                            else
                            {
                                List<MOBSeatPrice> seatPrices = BuildSeatPricesForPriceBreakDown(allSeats);
                                response.SeatPrices = SortAndOrderSeatPrices(seatPrices);
                            }
                        }
                        else
                        {
                            List<MOBSeatPrice> seatPrices = BuildSeatPricesForPriceBreakDown(allSeats);
                            response.SeatPrices = SortAndOrderSeatPrices(seatPrices);
                        }

                        persistedReservation = await _sessionHelperService.GetSession<Reservation>(session.SessionId, persistedReservation.ObjectName, new List<string> { session.SessionId, persistedReservation.ObjectName }).ConfigureAwait(false);
                        persistedReservation.SeatPrices = response.SeatPrices;
                        await _sessionHelperService.SaveSession<Reservation>(persistedReservation, session.SessionId, new List<string> { session.SessionId, persistedReservation.ObjectName }, persistedReservation.ObjectName).ConfigureAwait(false);
                    }
                    else
                    {
                        if (_configuration.GetValue<bool>("IsEnableAFSFreeSeatCoupon") && EnableAdvanceSearchCouponBooking(request.Application.Id, request.Application.Version.Major))
                        {
                            MOBShoppingCart persistShoppingCart = new MOBShoppingCart();
                            persistShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, persistShoppingCart.ObjectName, new List<string> { request.SessionId, persistShoppingCart.ObjectName }).ConfigureAwait(false);
                            bool isAFSCouponApplied = IsAFSSeatCouponApplied(persistShoppingCart);
                            if (isAFSCouponApplied)
                            {
                                var tupleResponse = await GetFinalTravelerSeatPrices(request, request.LanguageCode, response.Seats, request.SessionId);
                                response.SeatPrices = tupleResponse.response;
                                lstSeats = tupleResponse.lstSeats;
                            }
                            persistedReservation = await _sessionHelperService.GetSession<Reservation>(session.SessionId, persistedReservation.ObjectName, new List<string> { session.SessionId, persistedReservation.ObjectName }).ConfigureAwait(false);
                            persistedReservation.SeatPrices = isAFSCouponApplied ? response.SeatPrices : null;

                            await _sessionHelperService.SaveSession<Reservation>(persistedReservation, session.SessionId, new List<string> { session.SessionId, persistedReservation.ObjectName }, persistedReservation.ObjectName).ConfigureAwait(false);
                        }
                        else
                        {
                            persistedReservation = await _sessionHelperService.GetSession<Reservation>(session.SessionId, persistedReservation.ObjectName, new List<string> { session.SessionId, persistedReservation.ObjectName }).ConfigureAwait(false);
                            persistedReservation.SeatPrices = null;
                            await _sessionHelperService.SaveSession<Reservation>(persistedReservation, session.SessionId, new List<string> { session.SessionId, persistedReservation.ObjectName }, persistedReservation.ObjectName).ConfigureAwait(false);
                        }
                    }
                }

                SelectTrip persistSelectTripObj = await _sessionHelperService.GetSession<SelectTrip>(request.SessionId, new SelectTrip().ObjectName, new List<string> { request.SessionId, new SelectTrip().ObjectName }).ConfigureAwait(false);

                if (persistSelectTripObj != null)
                {
                    await persistSelectTripObj.Responses[persistSelectTripObj.LastSelectTripKey].Availability.Reservation.Initialise(_configuration, _cachingService);
                    response.Reservation = persistSelectTripObj.Responses[persistSelectTripObj.LastSelectTripKey].Availability.Reservation;

                    response.Reservation.Prices = persistedReservation.Prices;
                }

                if (hasSeatPrice && !CompareAndVerifySeatPrices(response, lstSeats))
                {
                    _logger.LogWarning("CompleteSeatsV2: 'CompleteSeats failed as the total seats price is not equal to sum of Individual Seats Price.'");
                    throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage").ToString());
                }
            }
            else
            {
                persistedReservation = await _sessionHelperService.GetSession<Reservation>(session.SessionId, persistedReservation.ObjectName, new List<string> { session.SessionId, persistedReservation.ObjectName }).ConfigureAwait(false);
                if (request.IsDone)
                {
                    response.Seats = (updateSeatsAfterBundlesChanged || await _featureSettings.GetFeatureSettingValue("EnableEplusWithCouponSCRemovalFix").ConfigureAwait(false)) ?
                                    await GetAllTravelerSeats(request.SessionId) :
                                    await FlightTravelerSeats(request.SessionId, request.SeatAssignment, request.Origin, request.Destination, request.PaxIndex, "", "");
                }
            }

            var hasOaSegment = persistedReservation.Trips.Any(t => t.FlattenedFlights.Any(ff => ff.Flights.Any(f => IsSeatMapSupportedOa(f.OperatingCarrier, f.MarketingCarrier))));
          
            if (!request.IsDone || (request.IsDone && IsAFSSeatCouponApplied(shoppingCart)) || (isCOGPartialSeatAssignmentFixEnabled && request.IsDone && hasCOGFlight)) // Dont call shoppingcart if registerseat is called on done button click .But , Call if there is coupon applied on AFS (To identify whether eligible seat has been selected for coupon we depend on shoppingcart so we need to call shoppingcart in this sceanrio)
            {
                bool isRegister = true;
                if ((persistedReservation.IsSSA && (persistedReservation.IsELF || IsIBE(persistedReservation)) || hasOaSegment))
                {
                    var tupleResponse = await RegisterSeatsHandleElf(request, session, persistedReservation, response, isClearSeats);
                    isRegister = tupleResponse.Item1;
                    response = tupleResponse.registerSeatsResponse;
                }
                else
                {
                    var tupleResponse = await RegisterSeats(request, session, persistedReservation, response, isClearSeats);
                    isRegister = tupleResponse.Item1;
                    response = tupleResponse.registerSeatsResponse;
                }
                if (!isRegister)
                {
                    if (EnableAdvanceSearchCouponBooking(request.Application.Id, request.Application.Version.Major) && response.PromoCodeRemoveAlertForProducts != null)
                    {
                        persistedReservation.ShopReservationInfo2.SeatRemoveCouponPopupCount += 1;
                        await _sessionHelperService.SaveSession<Reservation>(persistedReservation, session.SessionId, new List<string> { session.SessionId, persistedReservation.ObjectName }, persistedReservation.ObjectName).ConfigureAwait(false);

                        return response;
                    }

                    throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage").ToString());
                }

                persistedReservation = await _sessionHelperService.GetSession<Reservation>(session.SessionId, persistedReservation.ObjectName, new List<string> { session.SessionId, persistedReservation.ObjectName }).ConfigureAwait(false);

                if (persistedReservation != null)
                {
                    persistedReservation.Prices = response.Reservation.Prices;
                    if (EnableAdvanceSearchCouponBooking(request.Application.Id, request.Application.Version.Major))
                    {
                        persistedReservation.SeatPrices = response.Reservation.SeatPrices;
                    }
                    if (persistedReservation.TravelersCSL != null)
                    {
                        List<MOBCPTraveler> lstTravelers = new List<MOBCPTraveler>();
                        foreach (string travelerKey in persistedReservation.TravelerKeys)
                        {
                            lstTravelers.Add(persistedReservation.TravelersCSL[travelerKey]);
                        }
                        response.Reservation.TravelersCSL = lstTravelers;
                    }

                    if (persistedReservation.LMXTravelers != null)
                    {
                        response.Reservation.lmxtravelers = persistedReservation.LMXTravelers == null ? persistedReservation.LMXTravelers = new List<MOBLMXTraveler>() : persistedReservation.LMXTravelers;
                    }
                }
            }
            else
            {
                response.ShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, shoppingCart.ObjectName, new List<string> { request.SessionId, shoppingCart.ObjectName }).ConfigureAwait(false);
            }

            //revisit to check
            response.Reservation.CreditCards = persistedReservation.CreditCards;
            response.Reservation.CartId = persistedReservation.CartId;
            response.Reservation.IsSignedInWithMP = persistedReservation.IsSignedInWithMP;
            response.Reservation.TravelersRegistered = persistedReservation.TravelersRegistered;
            response.Reservation.NumberOfTravelers = persistedReservation.NumberOfTravelers;


            if (persistedReservation.ShopReservationInfo2 != null && !request.IsRemove && (request.IsDone || request.ContinueToRegisterAncillary))
            {

                persistedReservation.ShopReservationInfo2.NextViewName = "RTI";
                persistedReservation.ShopReservationInfo2.IsForceSeatMapInRTI = false;
                //The  below code is to show the back button in  Travel option and Seats pages
                persistedReservation.ShopReservationInfo2.ShouldHideBackButton = false;

            }
            #region 1127 - Chase Offer (Booking)
            // Code is moved to utility so that it can be reused across different actions
            if (_shoppingCartUtility.EnableChaseOfferRTI(request.Application.Id, request.Application.Version.Major))
                _shoppingCartUtility.UpdateChaseCreditStatement(persistedReservation);
            #endregion 1127 - Chase Offer (Booking)
            if (_shoppingCartUtility.IsEnableOmniCartMVP2Changes(request.Application.Id, request.Application.Version.Major, persistedReservation?.ShopReservationInfo2?.IsDisplayCart == true))
            {
                PopulateSelectedAncillaries(persistedReservation, request, request.SessionId);
            }

            await  _sessionHelperService.SaveSession<Reservation>(persistedReservation, session.SessionId, new List<string> { session.SessionId, persistedReservation.ObjectName }, persistedReservation.ObjectName).ConfigureAwait(false);

            if (Convert.ToBoolean(_configuration.GetValue<string>("HazMatOn")))
            {
                response.HazMat = GetHazMat();
            }
            else
            {
                response.HazMat = null;
            }

            return response;
        }

        private MOBRegisterSeatsRequest BuildRegisterSeatsRequest(MOBSHOPUnfinishedBookingRequestBase request)
        {
            MOBRegisterSeatsRequest registerSeatsRequest = new MOBRegisterSeatsRequest();
            registerSeatsRequest.Application = request.Application;
            registerSeatsRequest.DeviceId = request.DeviceId;
            registerSeatsRequest.SessionId = request.SessionId;
            registerSeatsRequest.TransactionId = request.TransactionId;
            registerSeatsRequest.CartId = request.CartId;
            registerSeatsRequest.IsOmniCartSavedTripFlow = true;
            registerSeatsRequest.Flow = request.Flow;
            return registerSeatsRequest;
        }

        private void BuildClearOfferRequest(MOBRegisterOfferRequest clearOfferRequest, MOBProdDetail product)
        {
            clearOfferRequest.MerchandizingOfferDetails = clearOfferRequest.MerchandizingOfferDetails ?? new Collection<MerchandizingOfferDetails>();
            clearOfferRequest.MerchandizingOfferDetails.Add(new MerchandizingOfferDetails
            {
                ProductCode = product?.Code,
                TripIds = product?.Segments?.Select(y => y?.TripId)?.SelectMany(y => y?.Split(',').ToList()).ToList(),
                SelectedTripProductIDs = product?.Segments?.Select(y => y?.ProductId)?.ToList(),
                ProductIds = product?.Segments.SelectMany(y => y?.ProductIds).ToList(),
                IsOfferRegistered = true,
            });
        }
        private async Task<MOBBookingRegisterOfferResponse> RemoveAncillaryOffer(MOBRemoveAncillaryOfferRequest request, Session session)
        {
            MOBBookingRegisterOfferResponse removeAncillaryOfferResponse = new MOBBookingRegisterOfferResponse();
            try
            {
                var removeAncillaryOfferRequest = await BuildRemoveAncillaryOfferRequest(request, session);
                switch (removeAncillaryOfferRequest.CartKey.ToUpper())
                {
                    case "SEATASSIGNMENTS":
                        MOBBookingRegisterSeatsResponse registerSeatsResponse = new MOBBookingRegisterSeatsResponse();
                        var registerSeatsRequest = new MOBRegisterSeatsRequest()
                        {
                            Application = request.Application,
                            DeviceId = request.DeviceId,
                            SessionId = request.SessionId,
                            TransactionId = request.TransactionId,
                            CartId = request.CartId,
                            IsRemove = true,
                            Flow = request.Flow
                        };
                        registerSeatsResponse = await RegisterSeats(registerSeatsRequest, session);
                        removeAncillaryOfferResponse.Reservation = registerSeatsResponse.Reservation;
                        removeAncillaryOfferResponse.ShoppingCart = registerSeatsResponse.ShoppingCart;
                        break;
                    case "PRODUCTBUNDLES":
                        removeAncillaryOfferResponse =await RegisterBundlesV2(removeAncillaryOfferRequest, session);
                        break;
                    case "EFS":
                        removeAncillaryOfferResponse = await UnregisterEFS(removeAncillaryOfferRequest, session);
                        break;
                    case "TPI":
                        removeAncillaryOfferResponse = await RegisterOfferForTPI(removeAncillaryOfferRequest, session);
                        break;
                }

                // MOBILE-25395: SAF
                if (ConfigUtility.IsEnableSAFFeature(session) && removeAncillaryOfferRequest.MerchandizingOfferDetails != null && removeAncillaryOfferRequest.MerchandizingOfferDetails.Count > 0)
                {
                    var safCode = _configuration.GetValue<string>("SAFCode");
                    foreach (var merchandizingOfferDetail in removeAncillaryOfferRequest.MerchandizingOfferDetails)
                    {
                        if (merchandizingOfferDetail.ProductCode.Equals(safCode))
                        {
                            // MOBILE-25395: SAF
                            removeAncillaryOfferResponse = await RegisterOfferForSAF(removeAncillaryOfferRequest, session);
                        }
                    }
                }
            }
            catch (System.Net.WebException wex)
            {
                if (wex.Response != null)
                {
                    var errorResponse = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd();
                    _logger.LogError("UnRegisterAncillaryOffersForBooking WebException {exceptionstack} {applicatinId} {appversion} {deviceId} and {sessionId}", JsonConvert.SerializeObject(errorResponse), request.Application.Id, request.Application.Version.Major, request.DeviceId, request.SessionId);
                }
                throw;
            }
            catch (MOBUnitedException uaex)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(uaex.InnerException ?? uaex).Throw();
            }
            catch (System.Exception ex)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException ?? ex).Throw();
            }
            finally
            {

            }

            return removeAncillaryOfferResponse;
        }

        private async Task< MOBRegisterOfferRequest> BuildRemoveAncillaryOfferRequest(MOBRemoveAncillaryOfferRequest request, Session session)
        {
            MOBRegisterOfferRequest unRegisterAncillaryRequest = new MOBRegisterOfferRequest();
            unRegisterAncillaryRequest.Application = request.Application;
            unRegisterAncillaryRequest.DeviceId = request.DeviceId;
            unRegisterAncillaryRequest.SessionId = request.SessionId;
            unRegisterAncillaryRequest.TransactionId = request.TransactionId;
            unRegisterAncillaryRequest.CartId = request.CartId;
            unRegisterAncillaryRequest.Flow = request.Flow;
            if (request?.Products != null)
            {
                foreach (var product in request.Products)
                {
                    AssignCartkey(unRegisterAncillaryRequest, product);
                    if (product.Code == "SEATASSIGNMENTS")
                    {
                        await RemoveSeats(product, session.SessionId);
                    }
                    else
                    {
                        unRegisterAncillaryRequest.MerchandizingOfferDetails = unRegisterAncillaryRequest.MerchandizingOfferDetails ?? new Collection<MerchandizingOfferDetails>();
                        unRegisterAncillaryRequest.MerchandizingOfferDetails.Add(new MerchandizingOfferDetails
                        {
                            ProductCode = product?.Code,
                            TripIds = product?.Segments?.Select(y => y?.TripId)?.SelectMany(y => y?.Split(',').ToList()).ToList(),
                            SelectedTripProductIDs = product?.Segments?.Select(y => y?.ProductId)?.ToList(),
                            ProductIds = product?.Segments.SelectMany(y => y?.ProductIds).ToList(),
                            IsOfferRegistered = true,
                        });
                        unRegisterAncillaryRequest.IsRemove = true;
                    }
                }
            }
            return unRegisterAncillaryRequest;
        }
        private void AssignCartkey(MOBRegisterOfferRequest unRegisterAncillaryRequest, MOBProdDetail product)
        {
            if (product.ProdDescription == "BE")
                unRegisterAncillaryRequest.CartKey = "ProductBundles";
            else
                unRegisterAncillaryRequest.CartKey = product.Code;
        }
        private async Task RemoveSeats(MOBProdDetail product, string sessionId)
        {
            var subSegments = product.Segments.SelectMany(segment => segment.SubSegmentDetails);
            var persistedReservation = new Reservation();
            persistedReservation = await _sessionHelperService.GetSession<Reservation>(sessionId, new Reservation().ObjectName, new List<string> { sessionId, new Reservation().ObjectName }).ConfigureAwait(false);
            var isEnableRemoveEplusSeatsWithCouponAppliedFix = await _featureSettings.GetFeatureSettingValue("EnableRemoveEplusSeatsWithCouponAppliedFix").ConfigureAwait(false);

            foreach (var subsegment in subSegments)
            {
                subsegment.PaxDetails.ForEach(paxdetail =>
                {
                    var traveler = persistedReservation.TravelersCSL.Values.FirstOrDefault(trvlr => trvlr.TravelerNameIndex == paxdetail.Key);
                    if (traveler != null)
                    {
                        var travelerSeat = traveler.Seats.FirstOrDefault(seat => (seat.Origin + " - " + seat.Destination) == subsegment.SegmentInfo);
                        if (_configuration.GetValue<bool>("LiveCartRemoveThroughFlightFix") && travelerSeat == null)
                        {
                            var flight = persistedReservation?.Trips?.SelectMany(t => t.FlattenedFlights)
                                                                    .SelectMany(ff => ff.Flights)
                                                                    .FirstOrDefault(f => (f.IsThroughFlight || f.ChangeOfPlane)
                                                                                        && (string.Equals(f.FlightNumber, subsegment.FlightNumber, StringComparison.OrdinalIgnoreCase))
                                                                                        && subsegment.SegmentInfo.StartsWith(f.Origin));

                            travelerSeat = traveler.Seats.FirstOrDefault(seat => string.Equals(seat.Origin, flight?.Origin, StringComparison.OrdinalIgnoreCase));

                        }
                        if (travelerSeat != null)
                        {
                            travelerSeat.SeatAssignment = String.Empty;
                            travelerSeat.Price = 0;
                            travelerSeat.PriceAfterTravelerCompanionRules = 0;
                            travelerSeat.Currency = "USD";
                            travelerSeat.ProgramCode = String.Empty;
                            travelerSeat.SeatType = String.Empty;
                            travelerSeat.LimitedRecline = false;
                            if (isEnableRemoveEplusSeatsWithCouponAppliedFix)
                            {
                                travelerSeat.PriceAfterCouponApplied = 0;
                                travelerSeat.PromotionalCouponCode = String.Empty;
                            }
                        }
                    }
                });

            }
            List<MOBSeatPrice> seatPrices = BuildSeatPricesForPriceBreakDown(persistedReservation.TravelersCSL.Values.SelectMany(traveler => traveler.Seats).ToList());
            persistedReservation.SeatPrices = new List<MOBSeatPrice>();
            persistedReservation.SeatPrices = SortAndOrderSeatPrices(seatPrices);

            await _sessionHelperService.SaveSession<Reservation>(persistedReservation, sessionId, new List<string> { sessionId, persistedReservation.ObjectName }, persistedReservation.ObjectName).ConfigureAwait(false);
        }
        private async Task<MOBBookingRegisterOfferResponse> RegisterBundlesV2(MOBRegisterOfferRequest request, Session session)
        {
            MOBBookingRegisterOfferResponse response = new MOBBookingRegisterOfferResponse();
            MOBRegisterOfferResponse registerOfferResponse = new MOBRegisterOfferResponse();
            FlightReservationResponse cslResponse = new FlightReservationResponse();
            Reservation reservation = new Reservation();
            MOBShoppingCart persistedShoppingCart = new MOBShoppingCart();
            bool isOmniCartSavedTripFlow = IsEnableOmniCartReleaseCandidateTwoChanges_Bundles(request.Application.Id, request.Application.Version.Major) && request.IsOmniCartSavedTripFlow;
            try
            {
                if (request != null)
                {
                    if (!isOmniCartSavedTripFlow)
                    {
                        if (request.MerchandizingOfferDetails != null && request.MerchandizingOfferDetails.Count > 0)
                        {
                            #region Fixing the Null offers request sent by Client Request
                            MOBRegisterOfferRequest requestClone = request.Clone();
                            request.MerchandizingOfferDetails = new Collection<MerchandizingOfferDetails>();
                            foreach (MerchandizingOfferDetails products in requestClone.MerchandizingOfferDetails)
                            {
                                if (!string.IsNullOrEmpty(products.ProductCode))
                                {
                                    if (!_configuration.GetValue<bool>("DisableFixforiOSRTMultiPaxBundles_MOBILE14646") && request.Application.Id == 1
                                        && products.ProductIds?.Count > 0 && products.ProductIds[0].IndexOf(',') > -1)
                                    {
                                        string joinedProductIds = string.Join(",", products.ProductIds);
                                        products.ProductIds = joinedProductIds.Split(',').ToList();
                                    }
                                    request.MerchandizingOfferDetails.Add(products);
                                }
                            }
                            #endregion
                        }

                        persistedShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, persistedShoppingCart.ObjectName, new List<string> { request.SessionId, persistedShoppingCart.ObjectName }).ConfigureAwait(false);

                        bool isAdvanceSearchCouponApplied = EnableAdvanceSearchCouponBooking(request.Application.Id, request.Application.Version.Major);

                        if (_configuration.GetValue<bool>("EnableCouponsforBooking"))
                        {
                            if (request.ContinueToRegisterAncillary)
                            {
                                if (!await AddOrRemovePromo(request, session, true, request.Flow))
                                {
                                    throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                                }
                                // Loading the updated shopping cart persist for prices
                                persistedShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, persistedShoppingCart.ObjectName, new List<string> { request.SessionId, persistedShoppingCart.ObjectName }).ConfigureAwait(false);
                            }
                            else
                            {
                                var reservationcheck = await _sessionHelperService.GetSession<Reservation>(request.SessionId, reservation.ObjectName, new List<string> { request.SessionId, reservation.ObjectName }).ConfigureAwait(false);
                                if (persistedShoppingCart?.Products != null && persistedShoppingCart.Products.Any(p => p.CouponDetails != null && p.CouponDetails.Count > 0)
                                    || (isAdvanceSearchCouponApplied && persistedShoppingCart?.Products != null
                                                                     && persistedShoppingCart?.PromoCodeDetails?.PromoCodes != null
                                                                     && persistedShoppingCart.PromoCodeDetails.PromoCodes.Count > 0))
                                {
                                    if (isAdvanceSearchCouponApplied)
                                    {
                                        if (reservationcheck?.ShopReservationInfo2?.NextViewName == "RTI")
                                        {
                                            var changeInTravelerMessage = _configuration.GetValue<string>("PromoCodeRemovalmessage");
                                            response.PromoCodeRemoveAlertForProducts = string.IsNullOrWhiteSpace(changeInTravelerMessage) ? null : new MOBSection
                                            {
                                                Text1 = changeInTravelerMessage,
                                                Text2 = "Cancel",
                                                Text3 = "Continue"
                                            };
                                            return response;
                                        }
                                        else if (reservationcheck?.ShopReservationInfo2?.NextViewName != "RTI" && persistedShoppingCart?.PromoCodeDetails != null && persistedShoppingCart.PromoCodeDetails.PromoCodes?.Count > 0)
                                        {
                                            if (await ValidateBundleSelectedWhenCouponIsApplied(request, persistedShoppingCart, reservationcheck))
                                            {
                                                var changeInTravelerMessage = _configuration.GetValue<string>("AdvanceSearchCouponWithRegisterBundlesErrorMessage");
                                                response.PromoCodeRemoveAlertForProducts = string.IsNullOrWhiteSpace(changeInTravelerMessage) ? null : new MOBSection
                                                {
                                                    Text1 = changeInTravelerMessage,
                                                    Text2 = "Remove bundle",
                                                    Text3 = "Continue",
                                                    MessageType = "Booking"
                                                };
                                                return response;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var changeInTravelerMessage = _configuration.GetValue<string>("PromoCodeRemovalmessage");
                                        response.PromoCodeRemoveAlertForProducts = string.IsNullOrWhiteSpace(changeInTravelerMessage) ? null : new MOBSection
                                        {
                                            Text1 = changeInTravelerMessage,
                                            Text2 = "Cancel",
                                            Text3 = "Continue"
                                        };
                                        return response;
                                    }
                                }
                            }
                        }

                        if (_configuration.GetValue<bool>("BundleCart"))
                        {
                            if (persistedShoppingCart != null && !string.IsNullOrEmpty(persistedShoppingCart.BundleCartId))
                            {
                                request.CartId = persistedShoppingCart.BundleCartId;
                            }
                        }
                        reservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, reservation.ObjectName, new List<string> { request.SessionId, reservation.ObjectName }).ConfigureAwait(false);
                    }
                    if ((request.MerchandizingOfferDetails != null && request.MerchandizingOfferDetails.Count > 0))
                    {
                        if (_shoppingCartUtility.IsEnableBundleLiveUpdateChanges(request.Application.Id, request.Application.Version.Major, reservation?.ShopReservationInfo2?.IsDisplayCart == true))
                        {
                            await BuildRegisteredBundleOfferRequest(persistedShoppingCart, request);
                        }
                        var tupleResponse = await RegisterOffers(request, session, false);
                        registerOfferResponse = tupleResponse.response;
                        cslResponse = tupleResponse.flightReservationResponse;

                        response.PkDispenserPublicKey = registerOfferResponse.PkDispenserPublicKey;
                        response.ShoppingCart = registerOfferResponse.ShoppingCart;
                    }
                    else
                    {
                        response.ShoppingCart = persistedShoppingCart;
                        response.PkDispenserPublicKey = await _pKDispenserPublicKey.GetCachedOrNewpkDispenserPublicKey(request.Application.Id, request.Application.Version.Major, request.DeviceId, session.SessionId, session.Token,session?.CatalogItems).ConfigureAwait(false);
                    }
                    response.Flow = request.Flow;
                    response.SessionId = request.SessionId;
                    response.TransactionId = request.TransactionId;
                    response.CheckinSessionId = registerOfferResponse.CheckinSessionId;
                    reservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, reservation.ObjectName, new List<string> { request.SessionId, reservation.ObjectName }).ConfigureAwait(false);

                    if (reservation != null && !request.IsContinue)
                    {
                        response.Reservation = MakeReservationFromPersistReservation(reservation);

                        var enableSFFTravelOption = System.Threading.Tasks.Task.Run(async () =>
                        {
                            return await _featureSettings.GetFeatureSettingValue("EnableFixForSFFTravelOption_MOBILE39209").ConfigureAwait(false);
                        }).Result;

                        if (await _featureToggles.IsEnableETCCreditsInBookingFeature(session?.CatalogItems))
                        {
                            await SetEligibilityforETCTravelCredit(response.Reservation, session, reservation);
                            _shoppingCartUtility.ApplyETCCreditsOnRTIAction(response.ShoppingCart, cslResponse.DisplayCart);
                        }
                        List<MOBSHOPPrice> prices = null;
                        if (response.Reservation.Prices != null && response.Reservation.Prices.Count > 0)
                        {
                            double travelOptionSubTotal = 0.0;
                            foreach (var price in response.Reservation.Prices)
                            {
                                if (price.PriceType.ToUpper().Equals("TRAVEL OPTIONS") || price.PriceType.ToUpper().Equals("ONE-TIME PASS") || (enableSFFTravelOption && price.PriceType.ToUpper().Equals("SFC")))
                                {
                                    travelOptionSubTotal = travelOptionSubTotal + Convert.ToDouble(price.DisplayValue);
                                }
                                else if (!price.PriceType.ToUpper().Equals("GRAND TOTAL"))
                                {
                                    if (prices == null)
                                    {
                                        prices = new List<MOBSHOPPrice>();
                                    }
                                    prices.Add(price);
                                }
                            }

                            foreach (var price in response.Reservation.Prices)
                            {
                                if (price.PriceType.ToUpper().Equals("GRAND TOTAL"))
                                {
                                    double grandTotal = Convert.ToDouble(price.DisplayValue);
                                    price.DisplayValue = string.Format("{0:#,0.00}", grandTotal - travelOptionSubTotal);
                                    price.FormattedDisplayValue = TopHelper.FormatAmountForDisplay(price.DisplayValue, new CultureInfo("en-US"));
                                    double tempDouble1 = 0;
                                    double.TryParse(price.DisplayValue.ToString(), out tempDouble1);
                                    price.Value = Math.Round(tempDouble1, 2, MidpointRounding.AwayFromZero);
                                    if (prices == null)
                                    {
                                        prices = new List<MOBSHOPPrice>();
                                    }
                                    prices.Add(price);
                                }
                            }
                        }

                        response.Reservation.Prices = prices;

                        response.Reservation.SeatPrices = reservation.SeatPrices.Clone();
                        response.Reservation.Taxes = reservation.Taxes.Clone();
                        response.Reservation.Trips = reservation.Trips.Clone();
                        if ((request.MerchandizingOfferDetails != null && request.MerchandizingOfferDetails.Count > 0))
                        {
                            response.Reservation.TravelOptions = GetTravelBundleOptions(cslResponse.DisplayCart, request.Application.Id, request.Application.Version.Major, reservation.IsReshopChange);
                        }


                        MOBBookingBundlesResponse objbundleResponse = new MOBBookingBundlesResponse(_configuration);
                        objbundleResponse = await _sessionHelperService.GetSession<MOBBookingBundlesResponse>(request.SessionId, objbundleResponse.ObjectName, new List<string> { request.SessionId, objbundleResponse.ObjectName }).ConfigureAwait(false);
                        if (objbundleResponse?.Products != null)
                        {
                            if (response.Reservation.TravelOptions != null)
                            {
                                foreach (MOBBundleProduct bundleProduct in objbundleResponse.Products)
                                {
                                    foreach (MOBSHOPTravelOption travelOption in response.Reservation.TravelOptions)
                                    {
                                        if (travelOption.SubItems != null && travelOption.SubItems.Any() &&
                                            travelOption.SubItems[0].ProductId == bundleProduct.ProductCode)
                                        {
                                            if (bundleProduct.Tile != null && bundleProduct.Tile.OfferDescription != null)
                                                travelOption.BundleOfferDescription = bundleProduct.Tile.OfferDescription.Select(desc => "- " + desc).ToList();
                                        }
                                    }
                                }
                            }
                        }

                        if (_shoppingCartUtility.IsEnableBundleLiveUpdateChanges(request.Application.Id, request.Application.Version.Major, reservation?.ShopReservationInfo2?.IsDisplayCart == true))
                        {
                            persistedShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, persistedShoppingCart.ObjectName, new List<string> { request.SessionId, persistedShoppingCart.ObjectName }).ConfigureAwait(false);
                            if (response.Reservation.TravelOptions != null && request.MerchandizingOfferDetails != null)
                            {
                                foreach (var travelOption in response.Reservation.TravelOptions)
                                {

                                    if (travelOption.SubItems != null && travelOption.SubItems.Any())
                                    {
                                        travelOption.TripIds = GetTripIdsByBundle(travelOption.SubItems[0].ProductId, persistedShoppingCart.Products, session);
                                        travelOption.BundleOfferTitle = "Travel Options Bundle";
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (response.Reservation.TravelOptions != null && request.MerchandizingOfferDetails != null)
                            {
                                foreach (var travelOption in response.Reservation.TravelOptions)
                                {
                                    foreach (var requestProduct in request.MerchandizingOfferDetails)
                                    {
                                        if (travelOption.SubItems != null && travelOption.SubItems.Any() &&
                                            travelOption.SubItems[0].ProductId == requestProduct.ProductCode)
                                        {
                                            travelOption.TripIds = requestProduct.TripIds;
                                            travelOption.BundleOfferTitle = "Travel Options Bundle";
                                        }
                                    }
                                }
                            }
                        }

                        if (request.ClubPassPurchaseRequest != null && request.ClubPassPurchaseRequest.NumberOfPasses > 0)
                        {
                            response.Reservation.ClubPassPurchaseRequest = request.ClubPassPurchaseRequest;
                            reservation.ClubPassPurchaseRequest = response.Reservation.ClubPassPurchaseRequest;
                        }
                        else
                        {
                            response.Reservation.ClubPassPurchaseRequest = null;
                            reservation.ClubPassPurchaseRequest = null;
                        }

                        if (_configuration.GetValue<bool>("EnableEplusCodeRefactor") && request != null && request.MerchandizingOfferDetails != null && request.MerchandizingOfferDetails.Count > 0)
                        {
                            response.Reservation.Prices = UpdatePricesForBundles(response.Reservation, request, request.Application.Id, request.Application.Version.Major, reservation.IsReshopChange, "BUNDLES");
                        }
                        else if (!_configuration.GetValue<bool>("EnableEplusCodeRefactor"))
                        {
                            response.Reservation.Prices = UpdatePricesForBundles(response.Reservation, request, request.Application.Id, request.Application.Version.Major, reservation.IsReshopChange);
                        }
                        reservation.TravelOptions = response.Reservation.TravelOptions;
                        reservation.ClubPassPurchaseRequest = response.Reservation.ClubPassPurchaseRequest;
                        if (response.Reservation.Prices != null && response.Reservation.Prices.Count > 0)
                        {
                            reservation.Prices = response.Reservation.Prices;
                        }
                        if (session != null && !string.IsNullOrEmpty(session.EmployeeId))
                        {
                            response.Reservation.IsEmp20 = true;
                        }

                        try
                        {
                            if (!_configuration.GetValue<bool>("DisableAutoAssignSeatBundle"))
                            {
                                var tripIndices = GetTripIndicesOfBundleChangeV2(request.MerchandizingOfferDetails, reservation.ShopReservationInfo2.NextViewName);
                                if (tripIndices != null && tripIndices.Any())
                                {
                                    await _sessionHelperService.SaveSession<Reservation>(reservation, request.SessionId, new List<string> { request.SessionId, reservation.ObjectName }, reservation.ObjectName).ConfigureAwait(false);

                                    await UpdateSeatsAfterBundleChanged(request.SessionId, request.Application.Id, request.Application.Version.Major, request.DeviceId, tripIndices);
                                    reservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, reservation.ObjectName, new List<string> { request.SessionId, reservation.ObjectName }).ConfigureAwait(false);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogInformation("RegisterBundles_CFOP-AutoAssignSeatBundle - {@Exception}", JsonConvert.SerializeObject(ex));
                            // ignore when auto assigning of seats failed
                        }

                        if (reservation.ShopReservationInfo2.NextViewName == "RTI" || reservation.IsELF)
                        {
                            #region TPI in booking path
                            if (EnableTPI(request.Application.Id, request.Application.Version.Major, 3) && !reservation.IsReshopChange)
                            {
                                // call TPI 
                                try
                                {
                                    string token = session.Token;
                                    MOBTPIInfoInBookingPath tPIInfo = await GetBookingPathTPIInfo(request.SessionId, request.LanguageCode, request.Application, request.DeviceId, response.Reservation.CartId, token, true, false, false);
                                    reservation.TripInsuranceFile = new MOBTripInsuranceFile();
                                    reservation.TripInsuranceFile.TripInsuranceBookingInfo = tPIInfo;
                                }
                                catch (Exception ex)
                                {
                                    reservation.TripInsuranceFile = null;
                                }
                            }
                            #endregion
                        }
                        // Code is moved to utility so that it can be reused across different actions
                        if (_shoppingCartUtility.EnableChaseOfferRTI(request.Application.Id, request.Application.Version.Major))
                            _shoppingCartUtility.UpdateChaseCreditStatement(reservation);

                       await _sessionHelperService.SaveSession<Reservation>(reservation, request.SessionId, new List<string> { request.SessionId, reservation.ObjectName }, reservation.ObjectName).ConfigureAwait(false);

                        if (_shoppingCartUtility.IsEnableBundleLiveUpdateChanges(request.Application.Id, request.Application.Version.Major, reservation?.ShopReservationInfo2?.IsDisplayCart == true))
                        {
                            response.BundleResponse = await UpdateBundleResponse(reservation, request.SessionId);
                        }

                    }
                    else
                    {
                        if (reservation != null)
                        {
                            if (!string.Equals(reservation.ShopReservationInfo2?.NextViewName, "RTI", StringComparison.OrdinalIgnoreCase))
                            {
                                reservation.ShopReservationInfo2.NextViewName = "Seats";
                                reservation.ShopReservationInfo2.IsForceSeatMapInRTI = true;

                                await _sessionHelperService.SaveSession<Reservation>(reservation, request.SessionId, new List<string> { request.SessionId, reservation.ObjectName }, reservation.ObjectName).ConfigureAwait(false);
                            }
                        }

                    }
                    if (reservation != null)
                    {
                        response.Reservation = await _productUtility.GetReservationFromPersist(response.Reservation, session);

                        if (response != null && response.Reservation != null &&
                            response.Reservation.TravelersCSL != null && response.Reservation.TravelersCSL.Any() &&
                            response.Reservation.ShopReservationInfo2 != null &&
                            response.Reservation.ShopReservationInfo2.AllEligibleTravelersCSL != null &&
                            response.Reservation.ShopReservationInfo2.AllEligibleTravelersCSL.Count > 0)
                        {
                            response.Reservation.TravelersCSL[0].SelectedSpecialNeeds = reservation.ShopReservationInfo2.AllEligibleTravelersCSL[0].SelectedSpecialNeeds;

                            response.Reservation.TravelersCSL[0].SelectedSpecialNeedMessages = reservation.ShopReservationInfo2.AllEligibleTravelersCSL[0].SelectedSpecialNeedMessages;
                        }
                        //Code by prasad prasad for Emp-20 single pax fix
                        if (reservation.NumberOfTravelers == 1 && response.Reservation.TravelersCSL == null)
                        {
                            if (reservation.ShopReservationInfo2.AllEligibleTravelersCSL != null)
                            {
                                response.Reservation.TravelersCSL = new List<MOBCPTraveler>();
                                foreach (MOBCPTraveler cpTraveler in reservation.ShopReservationInfo2.AllEligibleTravelersCSL)
                                {
                                    if (cpTraveler.IsProfileOwner)
                                    {
                                        response.Reservation.TravelersCSL.Add(cpTraveler);
                                    }
                                }
                            }
                        }
                    }
                }
                response.Reservation.IsBookingCommonFOPEnabled = reservation.IsBookingCommonFOPEnabled;
            }
            catch (MOBUnitedException uaex)
            {
                throw uaex;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }

            return response;
        }

        private List<int> GetTripIndicesOfBundleChangeV2(Collection<MerchandizingOfferDetails> products, string viewName)
        {
            var tripIndices = new List<int>();



            // Get involved tripIndices
            tripIndices = new[] { products }
                                        .Where(listOfBundles => listOfBundles != null).SelectMany(bundle => bundle)
                                        .Where(bundle => bundle != null && bundle.TripIds != null).SelectMany(bundle => bundle.TripIds)
                                        .Where(tripId => !string.IsNullOrEmpty(tripId)).Distinct()
                                        .Select(int.Parse).ToList();

            return tripIndices;
        }

        private async Task<MOBBookingRegisterOfferResponse> UnregisterEFS(MOBRegisterOfferRequest request, Session session)
        {
            MOBBookingRegisterOfferResponse response = new MOBBookingRegisterOfferResponse();
            MOBRegisterOfferResponse registerOfferResponse = new MOBRegisterOfferResponse();
            FlightReservationResponse cslResponse = new FlightReservationResponse();
            Reservation reservation = new Reservation();
            MOBShoppingCart persistedShoppingCart = new MOBShoppingCart();

            try
            {
                if (request != null)
                {

                    var tupleResponse = await RegisterOffers(request, session, false);
                    registerOfferResponse = tupleResponse.response;
                    cslResponse = tupleResponse.flightReservationResponse;
                    reservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, reservation.ObjectName, new List<string> { request.SessionId, reservation.ObjectName }).ConfigureAwait(false);
                    if (reservation != null)
                    {
                        response.Reservation = MakeReservationFromPersistReservation(reservation);

                        if (await _featureToggles.IsEnableETCCreditsInBookingFeature(session?.CatalogItems))
                        {
                            await SetEligibilityforETCTravelCredit(response.Reservation, session, reservation);
                            _shoppingCartUtility.ApplyETCCreditsOnRTIAction(response.ShoppingCart, cslResponse.DisplayCart);
                        }
                        List<MOBSHOPPrice> prices = null;
                        if (response.Reservation.Prices != null && response.Reservation.Prices.Count > 0)
                        {
                            double travelOptionSubTotal = 0.0;
                            foreach (var price in response.Reservation.Prices)
                            {
                                if (price.PriceType.ToUpper().Equals("EFS"))
                                {
                                    travelOptionSubTotal = travelOptionSubTotal + Convert.ToDouble(price.DisplayValue);
                                }
                                else if (!price.PriceType.ToUpper().Equals("GRAND TOTAL"))
                                {
                                    if (prices == null)
                                    {
                                        prices = new List<MOBSHOPPrice>();
                                    }
                                    prices.Add(price);
                                }
                            }

                            foreach (var price in response.Reservation.Prices)
                            {
                                if (price.PriceType.ToUpper().Equals("GRAND TOTAL"))
                                {
                                    double grandTotal = Convert.ToDouble(price.DisplayValue);
                                    price.DisplayValue = string.Format("{0:#,0.00}", grandTotal - travelOptionSubTotal);
                                    price.FormattedDisplayValue = TopHelper.FormatAmountForDisplay(price.DisplayValue, new CultureInfo("en-US")); //string.Format("${0:c}", price.DisplayValue);
                                    double tempDouble1 = 0;
                                    double.TryParse(price.DisplayValue.ToString(), out tempDouble1);
                                    price.Value = Math.Round(tempDouble1, 2, MidpointRounding.AwayFromZero);
                                    if (prices == null)
                                    {
                                        prices = new List<MOBSHOPPrice>();
                                    }
                                    prices.Add(price);
                                }
                            }
                        }

                        response.Reservation.Prices = prices;
                        response.Reservation.TravelOptions = GetTravelBundleOptions(cslResponse.DisplayCart, request.Application.Id, request.Application.Version.Major, reservation.IsReshopChange);
                        response.Reservation.Prices = UpdatePricesForBundles(response.Reservation, request, request.Application.Id, request.Application.Version.Major, reservation.IsReshopChange, "EFS");
                        reservation.TravelOptions = response.Reservation.TravelOptions;
                        reservation.Prices = response.Reservation.Prices;
                        UpdateSeatsAfterEfsRemovalV2(cslResponse, reservation, request.SessionId);
                        PopulateSelectedAncillaries(reservation, request, request.SessionId);
                        //UpdateSeatsAfterEfsRemoval(cslResponse, reservation, request.SessionId);

                        #region TPI in booking path
                        if (EnableTPI(request.Application.Id, request.Application.Version.Major, 3) && !reservation.IsReshopChange)
                        {
                            // call TPI 
                            try
                            {
                                string token = session.Token;
                                MOBTPIInfoInBookingPath tPIInfo = await GetBookingPathTPIInfo(request.SessionId, request.LanguageCode, request.Application, request.DeviceId, response.Reservation.CartId, token, true, false, false);
                                reservation.TripInsuranceFile = new MOBTripInsuranceFile();
                                reservation.TripInsuranceFile.TripInsuranceBookingInfo = tPIInfo;
                            }
                            catch (Exception ex)
                            {
                                reservation.TripInsuranceFile = null;
                            }
                        }
                        #endregion
                        // Code is moved to utility so that it can be reused across different actions
                        if (_shoppingCartUtility.EnableChaseOfferRTI(request.Application.Id, request.Application.Version.Major))
                            _shoppingCartUtility.UpdateChaseCreditStatement(reservation);

                        await _sessionHelperService.SaveSession<Reservation>(reservation, request.SessionId, new List<string> { request.SessionId, reservation.ObjectName }, reservation.ObjectName).ConfigureAwait(false);
                        response.Reservation = await _productUtility.GetReservationFromPersist(response.Reservation, session);
                    }
                }

            }
            catch (MOBUnitedException uaex)
            {
                throw uaex;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }

            return response;
        }

        private void UpdateSeatsAfterEfsRemoval(FlightReservationResponse flightReservationResponse, Reservation bookingPathReservation, string sessionId)
        {
            if (flightReservationResponse.DisplayCart?.DisplaySeats != null &&
                   flightReservationResponse.DisplayCart.DisplaySeats.Any(displaySeat => !string.IsNullOrEmpty(displaySeat.Seat)))
            {
                foreach (var traveler in bookingPathReservation.TravelersCSL)
                {
                    if (traveler.Value.TravelerTypeCode != "INF")//No need to builds the seat details for Infant on Lap as we dont allow seat for infant
                    {
                        var travelerSeats = flightReservationResponse.DisplayCart.DisplaySeats.Where(displaySeat => displaySeat.PersonIndex == traveler.Value.TravelerNameIndex);
                        foreach (var travelerSeat in traveler.Value.Seats)
                        {
                            if (!travelerSeats.Any(seat => seat.DepartureAirportCode == travelerSeat.Origin && seat.ArrivalAirportCode == travelerSeat.Destination))
                            {
                                travelerSeat.SeatAssignment = String.Empty;
                                travelerSeat.Price = 0;
                                travelerSeat.PriceAfterTravelerCompanionRules = 0;
                                travelerSeat.Currency = "USD";
                                travelerSeat.ProgramCode = String.Empty;
                                travelerSeat.SeatType = String.Empty;
                                travelerSeat.LimitedRecline = false;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        private MOBReshopRegisterSeatsResponse ConvertBookingToReshopRegisterSeatsResponse(MOBBookingRegisterSeatsResponse registerSeats, MOBRegisterSeatsRequest request)
        {
            MOBReshopRegisterSeatsResponse response = new MOBReshopRegisterSeatsResponse();
            response.Ancillary = registerSeats.Ancillary;
            response.bookingTravlerInfo = registerSeats.bookingTravlerInfo;
            response.CallDuration = registerSeats.CallDuration;
            response.ContractOfCarriage = registerSeats.ContractOfCarriage;
            response.CreditCards = registerSeats.CreditCards;
            response.Disclaimer = registerSeats.Disclaimer;
            response.DotBaggageInformation = registerSeats.DotBaggageInformation;
            response.DOTBagRules = registerSeats.DOTBagRules;
            response.EDDMessages = registerSeats.EDDMessages;
            response.EligibleFormofPayments = registerSeats.EligibleFormofPayments;
            response.Emails = registerSeats.Emails;
            response.Exception = registerSeats.Exception;
            response.Flow = registerSeats.Flow;
            response.FooterMessage = registerSeats.FooterMessage;
            response.HazMat = registerSeats.HazMat;
            response.IsDefaultPaymentOption = registerSeats.IsDefaultPaymentOption;
            response.LanguageCode = registerSeats.LanguageCode;
            response.MachineName = registerSeats.MachineName;
            response.PkDispenserPublicKey = registerSeats.PkDispenserPublicKey;
            response.PNR = registerSeats.PNR;
            response.PremierAccess = registerSeats.PremierAccess;
            response.PriceBreakDownTitle = registerSeats.PriceBreakDownTitle;
            response.ProfileOwnerAddresses = registerSeats.ProfileOwnerAddresses;
            response.Request = registerSeats.Request;
            response.Reservation = registerSeats.Reservation;
            response.SeatAssignMessages = registerSeats.SeatAssignMessages;
            response.SeatPrices = registerSeats.SeatPrices == null ? registerSeats.SeatPrices = new List<MOBSeatPrice>() : registerSeats.SeatPrices;
            response.Seats = registerSeats.Seats;
            response.SelectedTrips = registerSeats.SelectedTrips;
            response.SessionGuID = registerSeats.SessionGuID;
            response.SessionId = registerSeats.SessionId;
            response.ShoppingCart = registerSeats.ShoppingCart;
            response.ShowPremierAccess = registerSeats.ShowPremierAccess;
            response.ShowSeatChange = registerSeats.ShowSeatChange;
            response.TermsAndConditions = registerSeats.TermsAndConditions;
            response.TransactionId = registerSeats.TransactionId;
            response.TripInsuranceInfo = registerSeats.TripInsuranceInfo;

            _logger.LogInformation("ConvertBookingToReshopRegisterSeatsResponse Response:{Response} with SessionId:{sessionId}", JsonConvert.SerializeObject(response), request.SessionId);

            return response;
        }

        private async Task<MOBBookingRegisterSeatsResponse> RegisterSeats(MOBRegisterSeatsRequest request, Session session)
        {
            MOBBookingRegisterSeatsResponse response = new MOBBookingRegisterSeatsResponse();
            try
            {
                if (!string.IsNullOrEmpty(request.SeatAssignment))
                {
                    request.SeatAssignment = await ValidateSeatPrice(request.SeatAssignment, request.SessionId, request.Origin, request.Destination);
                }

                Reservation bookingPathReservation = new Reservation();
                bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(session.SessionId, bookingPathReservation.ObjectName, new List<string> { session.SessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);
                if (_shoppingCartUtility.IsEnableOmniCartMVP2Changes(request.Application.Id, request.Application.Version.Major, bookingPathReservation?.ShopReservationInfo2?.IsDisplayCart == true))
                {
                    response = await CompleteSeatsV2(request, session, updateSeatsAfterBundlesChanged: false);
                }
                else
                {
                    response = await CompleteSeats(request, session, updateSeatsAfterBundlesChanged: false);
                }
                var isNotReshop = (!_configuration.GetValue<bool>("DisableConvertToFFCExceptionFix")) ? (session.Flow != FlowType.RESHOP.ToString()) : true;
                if (!_configuration.GetValue<bool>("EnableBookWithCredit") && !string.IsNullOrEmpty(session.BWCSessionId) && !session.IsAward && !session.IsCorporateBooking && isNotReshop) // For Award Travel we can't show Book with Credit
                {

                    var res = await _sessionHelperService.GetSession<MOBFOPLookUpTravelCreditRequest>(session.BWCSessionId, new MOBFOPLookUpTravelCreditRequest().ObjectName, new List<string> { session.BWCSessionId, new MOBFOPLookUpTravelCreditRequest().ObjectName }).ConfigureAwait(false);
                    

                    if (res != null)
                    {
                        res.SessionId = session.SessionId;
                        string path = "/Payment/LookUpTravelCredit";
                        var lookupRes = await _lookUpTravelCreditService.LookUpTravelCredit(session.Token, path, res, session.BWCSessionId);
                        response.ShoppingCart.FormofPaymentDetails.TravelCreditDetails = new Definition.FormofPayment.TravelCredit.MOBFOPTravelCreditDetails();
                        response.ShoppingCart.FormofPaymentDetails.TravelCreditDetails = lookupRes.ShoppingCart.FormofPaymentDetails?.TravelCreditDetails;
                    }

                }

                if (EnableAdvanceSearchCouponBooking(request.Application.Id, request.Application.Version.Major) && response.PromoCodeRemoveAlertForProducts != null)
                    return response;
                response.FooterMessage = GetBookingPathDisclaimerList();
                response.LanguageCode = request.LanguageCode;
                response.TransactionId = request.TransactionId;
                response.SessionId = request.SessionId;
                response.Disclaimer = GetDisclaimer();
                #region TPI in booking path
                if (_shoppingCartUtility.IsEnableOmniCartMVP2Changes(request.Application.Id, request.Application.Version.Major, bookingPathReservation?.ShopReservationInfo2?.IsDisplayCart == true)
                    ? request.IsDone || request.ContinueToRegisterAncillary : true)
                {
                    if (EnableTPI(request.Application.Id, request.Application.Version.Major, 3) && !response.Reservation.IsReshopChange)
                    {
                        // call TPI 
                        try
                        {
                            MOBTPIInfoInBookingPath tPIInfo = await GetBookingPathTPIInfo(request.SessionId, request.LanguageCode, request.Application, request.DeviceId, response.Reservation.CartId, session.Token, true, false, false);
                        }
                        catch
                        {
                            //  Reservation persistReservation = new Reservation();
                            bookingPathReservation.TripInsuranceFile = null;
                            await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, request.SessionId, new List<string> { request.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName);
                        }
                    }
                }
                #endregion
                //**// A common place to get all the saved reservation data at persist. 
                response.Reservation = await _productUtility.GetReservationFromPersist(response.Reservation, session);
                //**//

                if (response != null && response.Reservation != null &&
                    response.Reservation.TravelersCSL != null && response.Reservation.TravelersCSL.Any() &&
                    response.Reservation.ShopReservationInfo2 != null &&
                    response.Reservation.ShopReservationInfo2.AllEligibleTravelersCSL != null &&
                    response.Reservation.ShopReservationInfo2.AllEligibleTravelersCSL.Count > 0)
                {
                    response.Reservation.TravelersCSL[0].SelectedSpecialNeeds = response.Reservation.ShopReservationInfo2.AllEligibleTravelersCSL[0].SelectedSpecialNeeds;

                    response.Reservation.TravelersCSL[0].SelectedSpecialNeedMessages = response.Reservation.ShopReservationInfo2.AllEligibleTravelersCSL[0].SelectedSpecialNeedMessages;
                }

            }
            catch (MOBUnitedException uaex)
            {
                _logger.LogWarning("RegisterSeats UnitedException:{Exception} with SessionId:{sessionId}", JsonConvert.SerializeObject(uaex), session.SessionId);
                throw uaex;
            }
            catch (Exception ex)
            {
                _logger.LogError("RegisterSeats Exception:{Exception} with SessionId:{sessionId}", JsonConvert.SerializeObject(ex), session.SessionId);
                throw ex;
            }

            return response;
        }

        private string GetBookingPathDisclaimerList()
        {
            string footerMessage = "*Miles shown are the actual miles flown for this segment.Mileage accrued will vary depending on the terms and conditions of your frequent flyer program.";
            if (_configuration.GetValue<string>("GetAvailabilityAddTripCompleteSeatsAndReservationMessage") != null)
            {
                footerMessage = _configuration.GetValue<string>("GetAvailabilityAddTripCompleteSeatsAndReservationMessage");
            }

            return footerMessage;
        }

        private async Task<string> ValidateSeatPrice(string seatAssignment, string sessionId, string origin, string destination)
        {
            string validAssignnment = string.Empty;
            bool isLoop = true;
            if (!string.IsNullOrEmpty(seatAssignment) && seatAssignment.Trim(',').Trim() != "")
            {
                try
                {
                    SelectSeats persistSelectSeatsResponse = await _sessionHelperService.GetSession<SelectSeats>(sessionId, new SelectSeats().ObjectName, new List<string> { sessionId, new SelectSeats().ObjectName }).ConfigureAwait(false);

                    string[] multipleSeats = seatAssignment.Split(',');

                    for (int i = 0; i < multipleSeats.Count(); i++)
                    {
                        isLoop = true;
                        string[] SeatValues = multipleSeats[i].Split('|');

                        if (SeatValues[0] != null && SeatValues[0].ToUpper().Trim() != "")
                        {
                            #region
                            if (persistSelectSeatsResponse != null)
                            {
                                MOBSHOPSelectSeatsResponse shopSelectSeatsResponse = persistSelectSeatsResponse.Responses[origin + destination];

                                foreach (MOBCabin cabin in shopSelectSeatsResponse.SeatMap[0].Cabins)
                                {
                                    foreach (MOBRow mobRows in cabin.Rows)
                                    {
                                        foreach (MOBSeatB eachSeat in mobRows.Seats)
                                        {
                                            if (eachSeat.ServicesAndFees?.Count > 0 && eachSeat.ServicesAndFees[0]?.SeatNumber?.ToUpper() == SeatValues[0]?.ToUpper())
                                            {
                                                if (eachSeat.ServicesAndFees[0]?.TotalFee != Convert.ToDecimal(SeatValues[1]))
                                                {
                                                    multipleSeats[i] = SeatValues[0] + "|" + eachSeat.ServicesAndFees[0].TotalFee + "|" + SeatValues[2] + "|" + eachSeat.ServicesAndFees[0].Program.ToUpper() + "|" + eachSeat.ServicesAndFees[0].SeatFeature.ToUpper() + "|" + SeatValues[5];
                                                }
                                                isLoop = false;
                                                break;
                                            }
                                        }
                                        if (!isLoop)
                                        {
                                            break;
                                        }
                                    }
                                    if (!isLoop)
                                    {
                                        break;
                                    }
                                }
                            }
                            #endregion
                        }
                        validAssignnment = validAssignnment + multipleSeats[i] + ",";
                    }
                }

                catch (Exception ex)
                {
                    validAssignnment = string.Empty;
                }
            }
            else
            {
                // Scenario for SelectSeatsCall first time for multi pax if  seatAssignment = ',' we need to return the same ',' string back with out Trim(',')
                return seatAssignment;
            }

            return validAssignnment.Trim(',');
        }

        private async Task<double> GetTravelBankBalance(string sessionId)
        {
            MOBCPTraveler mobCPTraveler = await _productUtility.GetProfileOwnerTravelerCSL(sessionId);
            return mobCPTraveler?.MileagePlus?.TravelBankBalance > 0.00 ? mobCPTraveler.MileagePlus.TravelBankBalance : 0.00;
        }

        private async Task<string> CreateCart(MOBRequest request, Session session)
        {
            string response = string.Empty;

            try
            {
                string jsonRequest = JsonConvert.SerializeObject(string.Empty);

                _logger.LogInformation("CreateCart CSL Request:{request} with {sessionId}", JsonConvert.SerializeObject(jsonRequest), session.SessionId);

                string jsonResponse = await _shoppingCartService.CreateCart(session.Token, jsonRequest, session.SessionId).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    response = JsonConvert.DeserializeObject<string>(jsonResponse);
                }

            }
            catch (System.Net.WebException wex)
            {
                response = null;
                var errorResponse = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd();

                _logger.LogError("CreateCart WebException:{exception} with {sessionId}", errorResponse, session.SessionId);

                throw new System.Exception(errorResponse);
            }
            catch (System.Exception ex)
            {
                response = null;
                MOBExceptionWrapper exceptionWrapper = new MOBExceptionWrapper(ex);

                _logger.LogError("CreateCart Exception:{exception} with {sessionId}", exceptionWrapper.Message.ToString(), session.SessionId);

                throw new System.Exception(exceptionWrapper.Message.ToString());
            }

            return response;
        }

        private bool TravelOptionsContainsFareLock(List<MOBSHOPTravelOption> options)
        {
            bool containsFareLock = false;

            if (options != null && options.Count > 0)
            {
                foreach (MOBSHOPTravelOption option in options)
                {
                    if (option != null && !string.IsNullOrEmpty(option.Key) && option.Key.ToUpper() == "FARELOCK")
                    {
                        containsFareLock = true;
                        break;
                    }
                }
            }

            return containsFareLock;
        }

        private async Task<MOBSHOPReservation> BuildResponseforUnregisterFareLock(MOBRegisterOfferRequest fareLockRequest, Session session)
        {
            MOBSHOPReservation fareLockReservationResponse = new MOBSHOPReservation(_configuration, _cachingService);

            Reservation bookingPathReservation = new Reservation();
            try
            {
                bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(fareLockRequest.SessionId, bookingPathReservation.ObjectName, new List<string> { fareLockRequest.SessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);
            }
            catch (System.Exception ex) { throw new MOBUnitedException("Could not find your booking session."); }

            SelectTrip persistShop = new SelectTrip();
            persistShop = await _sessionHelperService.GetSession<SelectTrip>(fareLockRequest.SessionId, persistShop.ObjectName, new List<string> { fareLockRequest.SessionId, persistShop.ObjectName }).ConfigureAwait(false);

            #region Define Booking Path Persist Reservation and Save to session - Venkat 08/13/2014
            bool unregisterFarelock = TravelOptionsContainsFareLock(bookingPathReservation.TravelOptions);
            bookingPathReservation.SessionId = fareLockRequest.SessionId;
            bookingPathReservation.CartId = fareLockRequest.CartId;
            bookingPathReservation.UnregisterFareLock = unregisterFarelock;
            if (EnableEPlusAncillary(fareLockRequest.Application.Id, fareLockRequest.Application.Version.Major, session.IsReshopChange) && bookingPathReservation.TravelOptions != null)
            {
                var travelOption = bookingPathReservation.TravelOptions?.FirstOrDefault(t => t?.Key?.Trim().ToUpper() == "EFS");
                if (travelOption != null && !string.IsNullOrEmpty(travelOption.Key))
                {
                    bookingPathReservation.TravelOptions = new List<MOBSHOPTravelOption>() { travelOption };
                }
                else
                {
                    bookingPathReservation.TravelOptions = null;
                }
            }
            else
            {
                bookingPathReservation.TravelOptions = null;
            }

            await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, bookingPathReservation.SessionId, new List<string> { bookingPathReservation.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);
            #endregion

            fareLockReservationResponse = persistShop.Responses[persistShop.LastSelectTripKey].Availability.Reservation;
            fareLockReservationResponse.TravelersCSL = new List<MOBCPTraveler>();

            if (bookingPathReservation.TravelersCSL != null && bookingPathReservation.TravelersCSL.Count > 0)
            {
                foreach (string travelerKey in bookingPathReservation.TravelerKeys)
                {
                    MOBCPTraveler bookingTravelerInfo = bookingPathReservation.TravelersCSL[travelerKey];
                    fareLockReservationResponse.TravelersCSL.Add(bookingTravelerInfo);
                }
            }
            fareLockReservationResponse.CreditCards = bookingPathReservation.CreditCards;
            fareLockReservationResponse.CreditCardsAddress = bookingPathReservation.CreditCardsAddress;
            if (EnableEPlusAncillary(fareLockRequest.Application.Id, fareLockRequest.Application.Version.Major, session.IsReshopChange))
            {
                fareLockReservationResponse.TravelOptions = bookingPathReservation.TravelOptions;
            }
            else
            {
                fareLockReservationResponse.TravelOptions = null;
            }

            fareLockReservationResponse.UnregisterFareLock = unregisterFarelock;
            fareLockReservationResponse.FareLock = null;

            if (fareLockReservationResponse != null && fareLockReservationResponse.NumberOfTravelers == 1)
            {
                bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(fareLockRequest.SessionId, bookingPathReservation.ObjectName, new List<string> { fareLockRequest.SessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);
                if (bookingPathReservation != null)
                {
                    bookingPathReservation.GetALLSavedTravelers = false;
                    bookingPathReservation.ShopReservationInfo2.NextViewName = "TravelOption";
                    bookingPathReservation.ShopReservationInfo2.IsForceSeatMapInRTI = true;

                    await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, fareLockRequest.SessionId, new List<string> { fareLockRequest.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);

                    fareLockReservationResponse.GetALLSavedTravelers = bookingPathReservation.GetALLSavedTravelers;
                }
                fareLockReservationResponse = await _productUtility.GetReservationFromPersist(fareLockReservationResponse, session);
            }
            else
            {
                bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(fareLockRequest.SessionId, bookingPathReservation.ObjectName, new List<string> { fareLockRequest.SessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);
                //removed the above condition ,both forceEplus and IsSignedWithMP
                if (bookingPathReservation != null)
                {
                    #region
                    MOBCPTraveler mobCPTraveler = await _productUtility.GetProfileOwnerTravelerCSL(fareLockRequest.SessionId);
                    if (mobCPTraveler != null)
                    {
                        fareLockReservationResponse.TravelersCSL = new List<MOBCPTraveler>();
                        bookingPathReservation.TravelersCSL = new SerializableDictionary<string, MOBCPTraveler>();
                        bookingPathReservation.TravelerKeys = new List<string>();
                        if (mobCPTraveler.IsProfileOwner)
                        {
                            bookingPathReservation.TravelersCSL.Add(mobCPTraveler.PaxIndex.ToString(), mobCPTraveler);
                            bookingPathReservation.TravelerKeys.Add(mobCPTraveler.PaxIndex.ToString());
                        }
                    }
                    bookingPathReservation.ShopReservationInfo2.NextViewName = "TravelOption";

                    await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, fareLockRequest.SessionId, new List<string> { fareLockRequest.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);

                    fareLockReservationResponse = await _productUtility.GetReservationFromPersist(fareLockReservationResponse, session);
                    #endregion
                }
            }
            fareLockReservationResponse.IsBookingCommonFOPEnabled = bookingPathReservation.IsBookingCommonFOPEnabled;

            _logger.LogInformation("BuildResponseforUnregisterFareLock FareLockReservationResponse:{fareLockReservationResponse} with SessionId:{sessionId}", JsonConvert.SerializeObject(fareLockReservationResponse), session.SessionId);

            return fareLockReservationResponse;
        }


        private async Task<MOBRegisterOfferResponse> RegisterOffers(MOBRegisterOfferRequest request, Session session)
        {
            MOBRegisterOfferResponse response = new MOBRegisterOfferResponse();
            FlightReservationResponse flightReservationResponse = null;
            var tupleResponse = await RegisterOffers(request, session, false);
            response = tupleResponse.response;
            flightReservationResponse = tupleResponse.flightReservationResponse;
            return response;
        }

        private async Task<MOBBookingRegisterOfferResponse> UnRegisterFareLock(MOBRegisterOfferRequest request, Session session)
        {
            MOBBookingRegisterOfferResponse response = new MOBBookingRegisterOfferResponse();
            MOBRegisterOfferResponse fareLockResponse = new MOBRegisterOfferResponse();
            try
            {
                fareLockResponse = await RegisterOffers(request, session);
                response.Reservation = await BuildResponseforUnregisterFareLock(request, session);
                response.ShoppingCart = fareLockResponse.ShoppingCart;
                response.PkDispenserPublicKey = fareLockResponse.PkDispenserPublicKey;
            }
            catch (MOBUnitedException uaex)
            {
                throw uaex;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }

            response.LanguageCode = request.LanguageCode;
            response.TransactionId = request.TransactionId;
            response.SessionId = request.SessionId;
            response.CheckinSessionId = request.CheckinSessionId;
            response.Flow = request.Flow;

            _logger.LogInformation("UnRegisterFareLock Response:{response} with {sessionId}", JsonConvert.SerializeObject(response), request.SessionId);

            return response;
        }

        private List<MOBSHOPPrice> UpdatePriceForUnRegisterTPI(List<MOBSHOPPrice> persistedPrices)
        {
            List<MOBSHOPPrice> prices = null;
            if (persistedPrices != null && persistedPrices.Count > 0)
            {
                double travelOptionSubTotal = 0.0;
                foreach (var price in persistedPrices)
                {
                    // Add TPI here 
                    if (price.PriceType.ToUpper().Equals("TRAVEL INSURANCE"))
                    {
                        travelOptionSubTotal = travelOptionSubTotal + Convert.ToDouble(price.DisplayValue);
                    }
                    else if (!price.PriceType.ToUpper().Equals("GRAND TOTAL"))
                    {
                        if (prices == null)
                        {
                            prices = new List<MOBSHOPPrice>();
                        }
                        prices.Add(price);
                    }
                }

                foreach (var price in persistedPrices)
                {
                    if (price.PriceType.ToUpper().Equals("GRAND TOTAL"))
                    {
                        double grandTotal = Convert.ToDouble(price.DisplayValue);
                        price.DisplayValue = string.Format("{0:#,0.00}", grandTotal - travelOptionSubTotal);
                        price.FormattedDisplayValue = TopHelper.FormatAmountForDisplay(price.DisplayValue, new CultureInfo("en-US")); //string.Format("${0:c}", price.DisplayValue);
                        double tempDouble1 = 0;
                        double.TryParse(price.DisplayValue.ToString(), out tempDouble1);
                        price.Value = Math.Round(tempDouble1, 2, MidpointRounding.AwayFromZero);
                        if (prices == null)
                        {
                            prices = new List<MOBSHOPPrice>();
                        }
                        prices.Add(price);
                    }
                }
            }

            return prices;
        }

        private MOBSHOPReservation MakeReservationFromPersistReservation(Reservation reservation)
        {
            return new MOBSHOPReservation(_configuration,_cachingService)
            {
                SessionId = reservation.SessionId,
                IsSignedInWithMP = reservation.IsSignedInWithMP,
                NumberOfTravelers = reservation.NumberOfTravelers,
                SearchType = reservation.SearchType,
                TravelersRegistered = reservation.TravelersRegistered,
                CartId = reservation.CartId,
                CreditCards = reservation.CreditCards.Clone(),
                CreditCardsAddress = reservation.CreditCardsAddress.Clone(),
                Prices = reservation.Prices.Clone(),
                PointOfSale = reservation.PointOfSale,
                ClubPassPurchaseRequest = reservation.ClubPassPurchaseRequest,
                FareLock = reservation.FareLock,
                FareRules = reservation.FareRules,
                UnregisterFareLock = reservation.UnregisterFareLock,
                AwardTravel = reservation.AwardTravel,
                LMXFlights = reservation.LMXFlights,
                lmxtravelers = reservation.LMXTravelers == null ? reservation.LMXTravelers = new List<MOBLMXTraveler>() : reservation.LMXTravelers,
                ReservationEmail = reservation.ReservationEmail.Clone(),
                ReservationPhone = reservation.ReservationPhone.Clone(),
                IneligibleToEarnCreditMessage = reservation.IneligibleToEarnCreditMessage,
                OaIneligibleToEarnCreditMessage = reservation.OaIneligibleToEarnCreditMessage,
                IsELF = reservation.IsELF,
                IsSSA = reservation.IsSSA,
                IsMetaSearch = reservation.IsMetaSearch,
                MetaSessionId = reservation.MetaSessionId,
                IsUpgradedFromEntryLevelFare = reservation.IsUpgradedFromEntryLevelFare,
                IsReshopChange = reservation.IsReshopChange,
                ReshopTrips = reservation.ReshopTrips == null ? reservation.ReshopTrips = new List<MOBSHOPReshopTrip>() : reservation.ReshopTrips,
                Reshop = reservation.Reshop,
                ShopReservationInfo = reservation.ShopReservationInfo,
                ShopReservationInfo2 = reservation.ShopReservationInfo2,
                SeatAssignmentMessage = reservation.SeatAssignmentMessage,
                TripInsuranceInfoBookingPath = reservation.TripInsuranceFile == null ? null : reservation.TripInsuranceFile.TripInsuranceBookingInfo,
                AlertMessages = reservation.AlertMessages,
                IsRedirectToSecondaryPayment = reservation.IsRedirectToSecondaryPayment,
                RecordLocator = reservation.RecordLocator,
                Messages = reservation.Messages,
                TCDAdvisoryMessages = reservation.TCDAdvisoryMessages,
                CheckedbagChargebutton = reservation.CheckedbagChargebutton,
                TravelOptions = reservation.TravelOptions,
                Trips = reservation.Trips
            };
        }

        private async Task<MOBBookingRegisterOfferResponse> RegisterOfferForTPI(MOBRegisterOfferRequest request, Session session)
        {
            MOBBookingRegisterOfferResponse response = new MOBBookingRegisterOfferResponse();
            MOBRegisterOfferResponse registerOfferResponse = new MOBRegisterOfferResponse();
            FlightReservationResponse flightReservationResponse = new FlightReservationResponse();
            #region
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.SessionId) && request.Application != null && request.Application.Version != null && EnableTPI(request.Application.Id, request.Application.Version.Major, 3))
                {
                    Reservation reservation = new Reservation();
                    reservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, reservation.ObjectName, new List<string> { request.SessionId, reservation.ObjectName }).ConfigureAwait(false);
                    response.Reservation = MakeReservationFromPersistReservation(reservation);
                    if (await _featureToggles.IsEnableETCCreditsInBookingFeature(session?.CatalogItems))
                    {
                        await SetEligibilityforETCTravelCredit(response.Reservation, session, reservation);
                    }
                    string pointOfSale = !string.IsNullOrEmpty(reservation.PointOfSale) ? reservation.PointOfSale : "US";
                    if (reservation != null && reservation.TripInsuranceFile != null && reservation.TripInsuranceFile.TripInsuranceBookingInfo != null)
                    {
                        if (request.MerchandizingOfferDetails.Where(x => x.ProductCode == "TPI").FirstOrDefault().IsReQuote)
                        {
                            var tupleResponse = await RegisterOffers(request, session, false);
                            registerOfferResponse = tupleResponse.response;
                            flightReservationResponse = tupleResponse.flightReservationResponse;
                            reservation.Prices = UpdatePriceForUnRegisterTPI(reservation.Prices);
                            var travelOptions = BuildTravelOptionsForTPI(flightReservationResponse, "TPI", request.Application.Id, request.Application.Version.Major, session.IsReshopChange);
                            // when press accept and continue, unregister the old TPI, register new one, change prices 
                            if (request.MerchandizingOfferDetails.Where(x => x.ProductCode == "TPI").FirstOrDefault().IsAcceptChanges)
                            {
                                reservation.TripInsuranceFile.TripInsuranceBookingInfo.ButtonTextInProdPage = _configuration.GetValue<string>("TPIinfo_BookingPath_PRODBtnText_AfterRegister");
                                reservation.TripInsuranceFile.TripInsuranceBookingInfo.CoverCostStatus = _configuration.GetValue<string>("TPIinfo_BookingPath_CoverCostStatus");
                                reservation.TripInsuranceFile.TripInsuranceBookingInfo.IsRegistered = true;
                                reservation.Prices = UpdatePricesForTPI(reservation.Prices, reservation.TripInsuranceFile.TripInsuranceBookingInfo);
                            }
                            // when press remove insurance, unregister the old one, remove prices 
                            else
                            {
                                reservation.TripInsuranceFile.TripInsuranceBookingInfo.IsRegistered = false;
                            }
                            // remove the pop up message 
                            reservation.TripInsuranceFile.TripInsuranceBookingInfo.OldQuoteId = string.Empty;
                            reservation.TripInsuranceFile.TripInsuranceBookingInfo.OldAmount = 0;
                            reservation.TripInsuranceFile.TripInsuranceBookingInfo.PopUpMessage = string.Empty;
                        }
                        else
                        {
                            //Logic for unregister
                            if (request.MerchandizingOfferDetails.Where(x => x.ProductCode == "TPI").FirstOrDefault().IsOfferRegistered && reservation.TripInsuranceFile.TripInsuranceBookingInfo.IsRegistered)
                            {
                                MOBTPIInfoInBookingPath tPIInfo = reservation.TripInsuranceFile.TripInsuranceBookingInfo.Clone();
                                var tupleResponse =  await RegisterOffers(request, session, false);
                                registerOfferResponse = tupleResponse.response;
                                flightReservationResponse = tupleResponse.flightReservationResponse;

                                var travelOptions = BuildTravelOptionsForTPI(flightReservationResponse, "TPI", request.Application.Id, request.Application.Version.Major, session.IsReshopChange);
                                List<MOBSHOPPrice> prices = UpdatePriceForUnRegisterTPI(response.Reservation.Prices);

                                try
                                {

                                    string token = session.Token;
                                    tPIInfo = await GetBookingPathTPIInfo(request.SessionId, request.LanguageCode, request.Application, request.DeviceId, response.Reservation.CartId, token, false, false, session.IsReshopChange);
                                }
                                catch
                                {
                                    tPIInfo = null;
                                }
                                reservation.Prices = prices;
                                reservation.TripInsuranceFile.TripInsuranceBookingInfo = tPIInfo;
                            }
                            else if (!request.MerchandizingOfferDetails.Where(x => x.ProductCode == "TPI").FirstOrDefault().IsOfferRegistered && !reservation.TripInsuranceFile.TripInsuranceBookingInfo.IsRegistered)
                            {
                                MOBTPIInfoInBookingPath tPIInfo = reservation.TripInsuranceFile.TripInsuranceBookingInfo.Clone();
                                // request.MerchandizingOfferDetails.Where(x => x.ProductCode == "TPI").FirstOrDefault().ProductId = tPIInfo.QuoteId;
                                //  request.MerchandizingOfferDetails.Where(x => x.ProductCode == "TPI").FirstOrDefault().IsOfferRegistered = !request.MerchandizingOfferDetails.Where(x => x.ProductCode == "TPI").FirstOrDefault().IsOfferRegistered;
                                var tupleResponse = await RegisterOffers(request, session, false);
                                registerOfferResponse = tupleResponse.response;
                                flightReservationResponse = tupleResponse.flightReservationResponse;
                                var travelOptions = BuildTravelOptionsForTPI(flightReservationResponse, "TPI", request.Application.Id, request.Application.Version.Major, session.IsReshopChange);
                                tPIInfo.ButtonTextInProdPage = _configuration.GetValue<string>("TPIinfo_BookingPath_PRODBtnText_AfterRegister");
                                //tPIInfo.ButtonTextInRTIPage = _configuration.GetValue<string>("TPIinfo_BookingPath_RTIBtnText_AfterRegister"];
                                tPIInfo.CoverCostStatus = _configuration.GetValue<string>("TPIinfo_BookingPath_CoverCostStatus");
                                tPIInfo.IsRegistered = true;
                                reservation.TripInsuranceFile.TripInsuranceBookingInfo = tPIInfo;
                                reservation.Prices = UpdatePricesForTPI(reservation.Prices, tPIInfo);
                            }
                            else
                            {
                                throw new MOBUnitedException("Unable to get shopping cart contents.");
                            }
                        }
                        // Code is moved to utility so that it can be reused across different actions
                        if (_shoppingCartUtility.EnableChaseOfferRTI(request.Application.Id, request.Application.Version.Major))
                            _shoppingCartUtility.UpdateChaseCreditStatement(reservation);

                        if (_configuration.GetValue<bool>("EnableUpliftPayment"))
                        {
                            var formOfPaymentType = registerOfferResponse.ShoppingCart?.FormofPaymentDetails?.FormOfPaymentType ?? reservation.FormOfPaymentType.ToString();
                            var showUpliftTpiMessage = ShowUpliftTpiMessage(reservation, formOfPaymentType);
                            reservation.ShopReservationInfo2.InfoWarningMessages = UpdateInfoWarningForUpliftTpi(reservation.ShopReservationInfo2.InfoWarningMessages, showUpliftTpiMessage);
                        }

                         await _sessionHelperService.SaveSession<Reservation>(reservation, request.SessionId, new List<string> { request.SessionId, reservation.ObjectName }, reservation.ObjectName).ConfigureAwait(false);
                        response.Reservation = await _productUtility.GetReservationFromPersist(response.Reservation, session);
                    }
                    else
                    {
                        throw new MOBUnitedException("Unable to get shopping cart contents.");
                    }
                }
                response.ShoppingCart = registerOfferResponse.ShoppingCart;
                response.PkDispenserPublicKey = registerOfferResponse.PkDispenserPublicKey;
            }
            catch (System.Net.WebException wex)
            {
                var errorResponse = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd();


                // LogEntries.Add(United.Logger.LogEntry.GetLogEntry<string>(request.SessionId, "RegisterOffersForBooking", "UnitedException", request.Application.Id, request.Application.Version.Major, request.DeviceId, errorResponse, true, false));
                _logger.LogError("RegisterOffersForBooking UnitedException:{exception} with {sessionId}", errorResponse, session.SessionId);

                response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }
            catch (MOBUnitedException uaex)
            {
                throw uaex;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            #endregion
            response.LanguageCode = request.LanguageCode;
            response.TransactionId = request.TransactionId;
            response.SessionId = request.SessionId;
            response.CheckinSessionId = request.CheckinSessionId;
            response.Flow = request.Flow;

            _logger.LogInformation("RegisterOfferForTPI Response:{response} with {sessionId}", JsonConvert.SerializeObject(response), request.SessionId);

            return response;
        }

        private List<MOBSHOPPrice> UpdatePricesForTPI(List<MOBSHOPPrice> prices, MOBTPIInfoInBookingPath tPIInfo)
        {
            if (tPIInfo != null)
            {
                List<MOBSHOPPrice> totalPrices = new List<MOBSHOPPrice>();
                bool totalExist = false;
                double flightTotal = 0;

                CultureInfo ci = null;

                for (int i = 0; i < prices.Count; ++i)
                {
                    if (ci == null)
                        ci = GetCultureInfo(prices[i].CurrencyCode);

                    if (prices[i].DisplayType.ToUpper() == "GRAND TOTAL")
                    {
                        totalExist = true;
                        prices[i].DisplayValue = string.Format("{0:#,0.00}", (Convert.ToDouble(prices[i].DisplayValue) + tPIInfo.Amount));
                        prices[i].FormattedDisplayValue = TopHelper.FormatAmountForDisplay(prices[i].DisplayValue, new CultureInfo("en-US")); //string.Format("${0:c}", prices[i].DisplayValue); 
                        double tempDouble1 = 0;
                        double.TryParse(prices[i].DisplayValue.ToString(), out tempDouble1);
                        prices[i].Value = Math.Round(tempDouble1, 2, MidpointRounding.AwayFromZero);
                    }
                    if (prices[i].DisplayType.ToUpper() == "TOTAL")
                    {
                        flightTotal = Convert.ToDouble(prices[i].DisplayValue);
                    }
                }
                MOBSHOPPrice travelInsurancePrice = new MOBSHOPPrice();
                travelInsurancePrice.CurrencyCode = "USD";
                travelInsurancePrice.DisplayType = "Travel Insurance";
                travelInsurancePrice.DisplayValue = string.Format("{0:#,0.00}", tPIInfo.Amount.ToString());
                travelInsurancePrice.FormattedDisplayValue = TopHelper.FormatAmountForDisplay(travelInsurancePrice.DisplayValue, new CultureInfo("en-US")); //string.Format("${0:c}", travelInsurancePrice.DisplayValue); //formatAmountForDisplay(travelInsurancePrice.DisplayValue, ci, false); 
                travelInsurancePrice.PriceType = "Travel Insurance";
                travelInsurancePrice.PriceTypeDescription = "Travel Insurance";

                double tmpDouble1 = 0;
                double.TryParse(travelInsurancePrice.DisplayValue.ToString(), out tmpDouble1);
                travelInsurancePrice.Value = Math.Round(tmpDouble1, 2, MidpointRounding.AwayFromZero);

                if (totalExist)
                {
                    prices.Insert(prices.Count - 1, travelInsurancePrice);
                }
                else
                {
                    prices.Add(travelInsurancePrice);
                    MOBSHOPPrice totalPrice = new MOBSHOPPrice();
                    totalPrice.CurrencyCode = "USD";
                    totalPrice.DisplayType = "Grand Total";
                    totalPrice.DisplayValue = (flightTotal + tPIInfo.Amount).ToString("N2", CultureInfo.InvariantCulture);
                    totalPrice.FormattedDisplayValue = TopHelper.FormatAmountForDisplay(totalPrice.DisplayValue, new CultureInfo("en-US")); //string.Format("${0:c}", totalPrice.DisplayValue);
                    double tempDouble1 = 0;
                    double.TryParse(totalPrice.DisplayValue.ToString(), out tempDouble1);
                    totalPrice.Value = Math.Round(tempDouble1, 2, MidpointRounding.AwayFromZero);
                    totalPrice.PriceType = "Grand Total";
                    prices.Add(totalPrice);
                }
            }
            return prices;
        }

        private bool ShowUpliftTpiMessage(Reservation reservation, string formOfPaymentType)
        {
            return (reservation?.TripInsuranceFile?.TripInsuranceBookingInfo?.IsRegistered ?? false) &&
                   (formOfPaymentType == MOBFormofPayment.Uplift.ToString());
        }

        private List<MOBInfoWarningMessages> UpdateInfoWarningForUpliftTpi(List<MOBInfoWarningMessages> infoWarningMessages, bool showUpliftTpiMessage)
        {
            if (!showUpliftTpiMessage)
            {
                if (infoWarningMessages?.Any() ?? false)
                {
                    infoWarningMessages.RemoveAll(i => i.Order == MOBINFOWARNINGMESSAGEORDER.UPLIFTTPISECONDARYPAYMENT.ToString());
                }
                return infoWarningMessages;
            }

            if (infoWarningMessages?.Any() ?? false)
            {
                if (infoWarningMessages?.Any(i => i.Order == MOBINFOWARNINGMESSAGEORDER.UPLIFTTPISECONDARYPAYMENT.ToString()) ?? false)
                {
                    return infoWarningMessages;
                }

                infoWarningMessages.Add(GetTpiUpliftMessage());
                infoWarningMessages.OrderBy(c => (int)(MOBINFOWARNINGMESSAGEORDER)Enum.Parse(typeof(MOBINFOWARNINGMESSAGEORDER), c.Order));
                return infoWarningMessages;
            }

            return new List<MOBInfoWarningMessages> { GetTpiUpliftMessage() };
        }

        private MOBInfoWarningMessages GetTpiUpliftMessage()
        {
            return new MOBInfoWarningMessages
            {
                IconType = MOBINFOWARNINGMESSAGEICON.INFORMATION.ToString(),
                Messages = new List<string> { _configuration.GetValue<string>("UpliftTpiSecondaryPaymentMessage") },
                Order = MOBINFOWARNINGMESSAGEORDER.UPLIFTTPISECONDARYPAYMENT.ToString()
            };
        }

        private List<MOBSHOPTravelOption> BuildTravelOptionsForTPI(FlightReservationResponse response, String productCode, int appID, string appVersion, bool isReshop)
        {
            List<MOBSHOPTravelOption> travelOptions = null;
            if (response != null && response.Status.Equals(United.Services.FlightShopping.Common.StatusType.Success) && response.DisplayCart != null)
            {
                if (productCode != "TPI" && productCode != "PBS")
                {
                    travelOptions = GetTravelOptions(response.DisplayCart, appID, appVersion, isReshop);
                }
            }
            else
            {
                if (response.Errors != null && response.Errors.Count > 0)
                {
                    string errorMessage = string.Empty;
                    foreach (var error in response.Errors)
                    {
                        errorMessage = errorMessage + " " + error.Message;
                    }

                    throw new System.Exception(errorMessage);
                }
                else
                {
                    throw new MOBUnitedException("Unable to get shopping cart contents.");
                }
            }

            return travelOptions;
        }

        // MOBILE-25395: SAF
        private async Task<MOBBookingRegisterOfferResponse> RegisterOfferForSAF(MOBRegisterOfferRequest request, United.Persist.Definition.Shopping.Session session)
        {
            MOBBookingRegisterOfferResponse response = new MOBBookingRegisterOfferResponse();
            MOBRegisterOfferResponse registerOfferResponse = new MOBRegisterOfferResponse();
            FlightReservationResponse flightReservationResponse = new FlightReservationResponse();
            MOBShoppingCart persistedShoppingCart = new MOBShoppingCart();

            #region

            try
            {
                if (request != null && !string.IsNullOrEmpty(request.SessionId) && request.Application != null && request.Application.Version != null)
                {
                    // for SAF, cartkey should be null, revisit to vaidate
                    request.CartKey = "";
                    Reservation reservation = new Reservation();
                    reservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, reservation.ObjectName, new List<string> { request.SessionId, reservation.ObjectName }).ConfigureAwait(false);
                    response.Reservation = MakeReservationFromPersistReservation(reservation);
                    if (await _featureToggles.IsEnableETCCreditsInBookingFeature(session?.CatalogItems))
                    {
                        await SetEligibilityforETCTravelCredit(response.Reservation, session, reservation);
                    }
                    string pointOfSale = !string.IsNullOrEmpty(reservation.PointOfSale) ? reservation.PointOfSale : "US";
                    var safCode = _configuration.GetValue<string>("SAFCode");

                    if (reservation != null)
                    {
                        // Logic for unregister
                        if (request.MerchandizingOfferDetails.Where(x => x.ProductCode == safCode).FirstOrDefault().IsOfferRegistered)
                        {
                            var tupleResponse = await RegisterOffers(request, session, false);
                            registerOfferResponse = tupleResponse.response;
                            flightReservationResponse = tupleResponse.flightReservationResponse;
                            reservation.Prices = UpdatePriceForUnRegisterSAF(response.Reservation.Prices);
                            // Builds the travel options
                            await BuildTravelOptionsForSAF(request, response, flightReservationResponse, reservation, persistedShoppingCart, session);
                        }
                        else if (!request.MerchandizingOfferDetails.Where(x => x.ProductCode == safCode).FirstOrDefault().IsOfferRegistered)
                        {
                            var tupleResponse = await RegisterOffers(request, session, false);
                            registerOfferResponse = tupleResponse.response;
                            flightReservationResponse = tupleResponse.flightReservationResponse;
                            reservation.Prices = UpdatePricesForSAF(reservation.Prices, flightReservationResponse);
                            // Builds the travel options
                            await BuildTravelOptionsForSAF(request, response, flightReservationResponse, reservation, persistedShoppingCart, session);
                        }
                        else
                        {
                            throw new MOBUnitedException("Unable to get shopping cart contents.");
                        }
                        await _sessionHelperService.SaveSession<Reservation>(reservation, request.SessionId, new List<string> { request.SessionId, reservation.ObjectName }, reservation.ObjectName).ConfigureAwait(false);
                        response.Reservation = await _productUtility.GetReservationFromPersist(response.Reservation, session);
                    }
                    else
                    {
                        throw new MOBUnitedException("Unable to get shopping cart contents.");
                    }
                }
                response.ShoppingCart = registerOfferResponse.ShoppingCart;
                response.PkDispenserPublicKey = registerOfferResponse.PkDispenserPublicKey;
            }
            catch (System.Net.WebException wex)
            {
                var errorResponse = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd();


                // LogEntries.Add(United.Logger.LogEntry.GetLogEntry<string>(request.SessionId, "RegisterOffersForBooking", "UnitedException", request.Application.Id, request.Application.Version.Major, request.DeviceId, errorResponse, true, false));
                _logger.LogError("RegisterOffersForBooking UnitedException:{exception} with {sessionId}", errorResponse, session.SessionId);

                response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }
            catch (MOBUnitedException uaex)
            {
                throw uaex;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            #endregion

            response.LanguageCode = request.LanguageCode;
            response.TransactionId = request.TransactionId;
            response.SessionId = request.SessionId;
            response.CheckinSessionId = request.CheckinSessionId;
            response.Flow = request.Flow;

            _logger.LogInformation("RegisterOfferForSAF Response:{response} with {sessionId}", JsonConvert.SerializeObject(response), request.SessionId);

            return response;
        }

        // MOBILE-25395: SAF
        private List<MOBSHOPPrice> UpdatePricesForSAF(List<MOBSHOPPrice> prices, FlightReservationResponse flightReservationResponse)
        {

            List<MOBSHOPPrice> totalPrices = new List<MOBSHOPPrice>();
            bool totalExist = false;
            double flightTotal = 0;
            var safCode = _configuration.GetValue<string>("SAFCode");

            CultureInfo ci = null;

            var safItem = flightReservationResponse.ShoppingCart?.Items?.FirstOrDefault(item => item.Product?.Any(product => string.Equals(product.Code, safCode, StringComparison.OrdinalIgnoreCase)) ?? false);
            if (safItem != null)
            {
                double safAmount = safItem.Product.FirstOrDefault(product => string.Equals(product.Code, safCode, StringComparison.OrdinalIgnoreCase)).Price?.Totals?[0].Amount ?? 0;
                if (safAmount != 0)
                {
                    for (int i = 0; i < prices.Count; ++i)
                    {
                        if (ci == null)
                            ci = GetCultureInfo(prices[i].CurrencyCode);

                        if (prices[i].DisplayType.ToUpper() == "GRAND TOTAL")
                        {
                            totalExist = true;
                            prices[i].DisplayValue = string.Format("{0:#,0.00}", (Convert.ToDouble(prices[i].DisplayValue) + safAmount));
                            prices[i].FormattedDisplayValue = TopHelper.FormatAmountForDisplay(prices[i].DisplayValue, ci, false);
                            double tempDouble1 = 0;
                            double.TryParse(prices[i].DisplayValue.ToString(), out tempDouble1);
                            prices[i].Value = Math.Round(tempDouble1, 2, MidpointRounding.AwayFromZero);
                        }
                        if (prices[i].DisplayType.ToUpper() == "TOTAL")
                        {
                            flightTotal = Convert.ToDouble(prices[i].DisplayValue);
                        }
                    }
                    MOBSHOPPrice safPrice = new MOBSHOPPrice();
                    safPrice.CurrencyCode = "USD";
                    safPrice.DisplayType = "SustainableAviationFuelContribution";
                    safPrice.DisplayValue = string.Format("{0:#,0.00}", safAmount.ToString());
                    safPrice.FormattedDisplayValue = TopHelper.FormatAmountForDisplay(safPrice.DisplayValue, ci, false);
                    safPrice.PriceType = safCode;
                    safPrice.PriceTypeDescription = "United’s Sustainable Aviation Fuel(SAF) contribuition";

                    double tmpDouble1 = 0;
                    double.TryParse(safPrice.DisplayValue.ToString(), out tmpDouble1);
                    safPrice.Value = Math.Round(tmpDouble1, 2, MidpointRounding.AwayFromZero);

                    if (totalExist)
                    {
                        prices.Insert(prices.Count - 1, safPrice);
                    }
                    else
                    {
                        prices.Add(safPrice);
                        MOBSHOPPrice totalPrice = new MOBSHOPPrice();
                        totalPrice.CurrencyCode = "USD";
                        totalPrice.DisplayType = "Grand Total";
                        totalPrice.DisplayValue = (flightTotal + safAmount).ToString("N2", CultureInfo.InvariantCulture);
                        totalPrice.FormattedDisplayValue = TopHelper.FormatAmountForDisplay(totalPrice.DisplayValue, ci, false);
                        double tempDouble1 = 0;
                        double.TryParse(totalPrice.DisplayValue.ToString(), out tempDouble1);
                        totalPrice.Value = Math.Round(tempDouble1, 2, MidpointRounding.AwayFromZero);
                        totalPrice.PriceType = "Grand Total";
                        prices.Add(totalPrice);
                    }
                }
            }

            return prices;
        }

        // MOBILE-25395: SAF
        private List<MOBSHOPPrice> UpdatePriceForUnRegisterSAF(List<MOBSHOPPrice> persistedPrices)
        {
            List<MOBSHOPPrice> prices = null;
            var safCode = _configuration.GetValue<string>("SAFCode");

            if (persistedPrices != null && persistedPrices.Count > 0)
            {
                double travelOptionSubTotal = 0.0;
                foreach (var price in persistedPrices)
                {
                    // Looks for SAF price
                    if (price.PriceType.ToUpper().Equals(safCode))
                    {
                        travelOptionSubTotal = travelOptionSubTotal + Convert.ToDouble(price.DisplayValue);
                    }
                    else if (!price.PriceType.ToUpper().Equals("GRAND TOTAL"))
                    {
                        if (prices == null)
                        {
                            prices = new List<MOBSHOPPrice>();
                        }
                        prices.Add(price);
                    }
                }

                foreach (var price in persistedPrices)
                {
                    if (price.PriceType.ToUpper().Equals("GRAND TOTAL"))
                    {
                        double grandTotal = Convert.ToDouble(price.DisplayValue);
                        price.DisplayValue = string.Format("{0:#,0.00}", grandTotal - travelOptionSubTotal);
                        price.FormattedDisplayValue = string.Format("${0:c}", price.DisplayValue);
                        double tempDouble1 = 0;
                        double.TryParse(price.DisplayValue.ToString(), out tempDouble1);
                        price.Value = Math.Round(tempDouble1, 2, MidpointRounding.AwayFromZero);
                        if (prices == null)
                        {
                            prices = new List<MOBSHOPPrice>();
                        }
                        prices.Add(price);
                    }
                }
            }

            return prices;
        }

        // MOBILE-25395: SAF
        private async Task BuildTravelOptionsForSAF(MOBRegisterOfferRequest request, MOBBookingRegisterOfferResponse response, FlightReservationResponse flightReservationResponse,
                                                    Reservation reservation, MOBShoppingCart persistedShoppingCart, Session session)
        {
            if ((request.MerchandizingOfferDetails != null && request.MerchandizingOfferDetails.Count > 0))
            {
                response.Reservation.TravelOptions = GetTravelBundleOptions(flightReservationResponse.DisplayCart, request.Application.Id, request.Application.Version.Major, reservation.IsReshopChange);
            }
            MOBBookingBundlesResponse objbundleResponse = new MOBBookingBundlesResponse(_configuration);
            objbundleResponse = await _sessionHelperService.GetSession<MOBBookingBundlesResponse>(request.SessionId, objbundleResponse.ObjectName, new List<string> { request.SessionId, objbundleResponse.ObjectName }).ConfigureAwait(false);

            if (objbundleResponse?.Products != null)
            {
                if (response.Reservation.TravelOptions != null)
                {
                    foreach (MOBBundleProduct bundleProduct in objbundleResponse.Products)
                    {
                        foreach (MOBSHOPTravelOption travelOption in response.Reservation.TravelOptions)
                        {
                            if (travelOption.SubItems != null && travelOption.SubItems.Any() &&
                                travelOption.SubItems[0].ProductId == bundleProduct.ProductCode)
                            {
                                if (bundleProduct.Tile != null && bundleProduct.Tile.OfferDescription != null)
                                    travelOption.BundleOfferDescription = bundleProduct.Tile.OfferDescription.Select(desc => "- " + desc).ToList();
                            }
                        }
                    }
                }
            }

            var isEnableBundleLiveUpdateChanges = _shoppingCartUtility.IsEnableBundleLiveUpdateChanges(request.Application.Id, request.Application.Version.Major, reservation?.ShopReservationInfo2?.IsDisplayCart == true);
            if (isEnableBundleLiveUpdateChanges)
            {
                persistedShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, persistedShoppingCart.ObjectName, new List<string> { request.SessionId, persistedShoppingCart.ObjectName }).ConfigureAwait(false);
                if (response.Reservation.TravelOptions != null && request.MerchandizingOfferDetails != null)
                {
                    foreach (var travelOption in response.Reservation.TravelOptions)
                    {

                        if (travelOption.SubItems != null && travelOption.SubItems.Any())
                        {
                            travelOption.TripIds = GetTripIdsByBundle(travelOption.SubItems[0].ProductId, persistedShoppingCart.Products, session);
                            travelOption.BundleOfferTitle = "Travel Options Bundle";
                        }
                    }
                }
            }
            else
            {
                if (response.Reservation.TravelOptions != null && request.MerchandizingOfferDetails != null)
                {
                    foreach (var travelOption in response.Reservation.TravelOptions)
                    {
                        foreach (var requestProduct in request.MerchandizingOfferDetails)
                        {
                            if (travelOption.SubItems != null && travelOption.SubItems.Any() &&
                                travelOption.SubItems[0].ProductId == requestProduct.ProductCode)
                            {
                                travelOption.TripIds = requestProduct.TripIds;
                                travelOption.BundleOfferTitle = "Travel Options Bundle";
                            }
                        }
                    }
                }
            }
            reservation.TravelOptions = response.Reservation.TravelOptions;

            if (isEnableBundleLiveUpdateChanges)
            {
                response.BundleResponse = await UpdateBundleResponse(reservation, request.SessionId);
            }
        }

        private bool IsEnableTaxForAgeDiversification(bool isReShop, int appid, string appversion)
        {
            if (!isReShop && EnableTaxForAgeDiversification(appid, appversion))
            {
                return true;
            }

            return false;
        }

        private bool EnableTaxForAgeDiversification(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableTaxForAgeDiversification")
           && GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidiPhoneTaxForAgeDiversificationVersion", "AndroidiPhoneTaxForAgeDiversificationVersion", "", "", true, _configuration);
        }

        private async Task ValidateAwardMileageBalance(string sessionId, decimal milesNeeded)
        {
            CSLShopRequest shopRequest = new CSLShopRequest();
            shopRequest = await _sessionHelperService.GetSession<CSLShopRequest>(sessionId, shopRequest.ObjectName, new List<string> { sessionId, shopRequest.ObjectName }).ConfigureAwait(false);
            if (shopRequest != null && shopRequest.ShopRequest != null && shopRequest.ShopRequest.AwardTravel && shopRequest.ShopRequest.LoyaltyPerson != null && shopRequest.ShopRequest.LoyaltyPerson.AccountBalances != null)
            {
                if (shopRequest.ShopRequest.LoyaltyPerson.AccountBalances[0] != null && shopRequest.ShopRequest.LoyaltyPerson.AccountBalances[0].Balance < milesNeeded)
                {
                    throw new MOBUnitedException(_configuration.GetValue<string>("NoEnoughMilesForAwardBooking"));
                }
            }            
        }

        private async Task<List<MOBSHOPPrice>> GetPrices(List<United.Services.FlightShopping.Common.DisplayCart.DisplayPrice> prices, bool isAwardBooking, string sessionId, bool isReshopChange = false, string searchType = null, bool isFareLockViewRes = false, bool isCorporateFare = false, int appId = 0, string appVersion = "", FlightReservationResponse shopBookingDetailsResponse = null, Session session = null)
        {
            List<MOBSHOPPrice> bookingPrices = new List<MOBSHOPPrice>();
            CultureInfo ci = null;
            var isEnableOmniCartMVP2Changes = _configuration.GetValue<bool>("EnableOmniCartMVP2Changes");
            foreach (var price in prices)
            {
                if (ci == null)
                {
                    ci = GetCultureInfo(price.Currency);
                }

                MOBSHOPPrice bookingPrice = new MOBSHOPPrice();
                decimal NonDiscountTravelPrice = 0;
                double promoValue = 0;

                if (_configuration.GetValue<bool>("EnableAdvanceSearchCouponBooking"))
                {
                    if (price.Type.Equals("NONDISCOUNTPRICE-TRAVELERPRICE", StringComparison.OrdinalIgnoreCase) || price.Type.Equals("NonDiscountPrice-Total", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    if (price.Type.Equals("TRAVELERPRICE", StringComparison.OrdinalIgnoreCase) || (price.Type.Equals("TOTAL", StringComparison.OrdinalIgnoreCase)))
                    {
                        if (price.Type.Equals("TRAVELERPRICE", StringComparison.OrdinalIgnoreCase))
                        {
                            var nonDiscountedPrice = prices.Find(dp => dp.PaxTypeCode == price.PaxTypeCode && dp.Type.ToUpper().Equals("NONDISCOUNTPRICE-TRAVELERPRICE"));
                            var discountedPrice = prices.Find(dp => dp.PaxTypeCode == price.PaxTypeCode && dp.Type.ToUpper().Equals("TRAVELERPRICE"));
                            if (discountedPrice != null && nonDiscountedPrice != null)
                            {
                                promoValue = Math.Round(Convert.ToDouble(nonDiscountedPrice.Amount)
                                             - Convert.ToDouble(discountedPrice.Amount), 2, MidpointRounding.AwayFromZero);
                                NonDiscountTravelPrice = nonDiscountedPrice.Amount;
                            }
                            else
                            {
                                promoValue = 0;
                            }
                        }
                        if (price.Type.Equals("TOTAL", StringComparison.OrdinalIgnoreCase))
                        {

                            var nonDiscountedTotalPrice = prices.Find(dp => dp.PaxTypeCode == price.PaxTypeCode && dp.Type.ToUpper().Equals("NONDISCOUNTPRICE-TOTAL"));
                            var discountedTotalPrice = prices.Find(dp => dp.PaxTypeCode == price.PaxTypeCode && dp.Type.ToUpper().Equals("TOTAL"));
                            if (discountedTotalPrice != null && nonDiscountedTotalPrice != null)
                            {
                                promoValue = Math.Round(Convert.ToDouble(nonDiscountedTotalPrice.Amount)
                                            - Convert.ToDouble(discountedTotalPrice.Amount), 2, MidpointRounding.AwayFromZero);
                                promoValue = _shoppingUtility.UpdatePromoValueForFSRMoneyMiles(prices, session, promoValue);
                            }
                            else
                            {
                                promoValue = 0;
                            }
                        }
                        bookingPrice.PromoDetails = promoValue > 0 ? new MOBPromoCode
                        {
                            PriceTypeDescription = price.Type.Equals("TOTAL", StringComparison.OrdinalIgnoreCase) ? _configuration.GetValue<string>("PromoSavedText") : _configuration.GetValue<string>("PromoCodeAppliedText"),
                            PromoValue = Math.Round(promoValue, 2, MidpointRounding.AwayFromZero),
                            FormattedPromoDisplayValue = "-" + TopHelper.FormatAmountForDisplay(promoValue.ToString(), ci, false)//promoValue.ToString("C2", CultureInfo.CurrentCulture)
                        } : null;
                    }
                }

                bookingPrice.CurrencyCode = price.Currency;
                bookingPrice.DisplayType = price.Type;
                bookingPrice.Status = price.Status;
                bookingPrice.Waived = price.Waived;
                bookingPrice.DisplayValue = string.Format("{0:#,0.00}", price.Amount);
                if (_configuration.GetValue<bool>("EnableCouponsforBooking") && !string.IsNullOrEmpty(price.PaxTypeCode))
                {
                    bookingPrice.PaxTypeCode = price.PaxTypeCode;
                }
                if (!string.IsNullOrEmpty(searchType))
                {
                    string desc = string.Empty;
                    if (price.Description != null && price.Description.Length > 0)
                    {
                        if (!EnableYADesc(isReshopChange) || (price.PricingPaxType != null && !price.PricingPaxType.Equals("UAY")))
                        {
                            desc = GetFareDescription(price);
                        }
                    }
                    if (EnableYADesc(isReshopChange) && !string.IsNullOrEmpty(price.PricingPaxType) && price.PricingPaxType.ToUpper().Equals("UAY"))
                    {
                        bookingPrice.PriceTypeDescription = BuildYAPriceTypeDescription(searchType);
                        bookingPrice.PaxTypeDescription = $"{price?.Count} {"young adult (18-23)"}".ToLower();
                    }
                    else
                    if (price.Description.ToUpper().Contains("TOTAL"))
                    {
                        bookingPrice.PriceTypeDescription = price?.Description.ToLower(); 
                        if (await _featureToggles.IsEnableETCCreditsInBookingFeature(session?.CatalogItems) && prices.Any(x => x.Type.Equals("TravelCredits", StringComparison.OrdinalIgnoreCase) && x.Amount > 0)
                            && price.StrikeThroughPricing > 0 && (int)price.StrikeThroughPricing >= (int)price.Amount)
                        {
                            bookingPrice.StrikeThroughDisplayValue = ((int)price.StrikeThroughPricing - (int)price.Amount).ToString("C2", CultureInfo.CurrentCulture);
                            // bookingPrice.StrikeThroughDescription = BuildETCCreditsStrikeThroughDescription();
                        }
                    }
                    else
                    {
                        // bookingPrice.PriceTypeDescription = UtilityHelper.BuildPriceTypeDescription(searchType, price.Description, price.Count, desc, isFareLockViewRes, isCorporateFare);
                        if (_configuration.GetValue<bool>("EnableAwardStrikeThroughPricing") && session.IsAward
                                && session != null && session.CatalogItems != null && session.CatalogItems.Count > 0 && session.CatalogItems.FirstOrDefault(a => a.Id == ((int)IOSCatalogEnum.AwardStrikeThroughPricing).ToString() || a.Id == ((int)AndroidCatalogEnum.AwardStrikeThroughPricing).ToString())?.CurrentValue == "1"
                                && price.StrikeThroughPricing > 0 && (int)price.StrikeThroughPricing != (int)price.Amount
                          )
                        {
                            bookingPrice.StrikeThroughDisplayValue = UtilityHelper.FormatAwardAmountForDisplay(price.StrikeThroughPricing.ToString(), true);
                            bookingPrice.StrikeThroughDescription = BuildStrikeThroughDescription();
                        }
                        bookingPrice.PriceTypeDescription = UtilityHelper.BuildPriceTypeDescription(searchType, price.Description, price.Count, desc, isFareLockViewRes, isCorporateFare);
                        if (isEnableOmniCartMVP2Changes)
                        {
                            string description = price?.Description;
                            if (_shoppingUtility.EnableShoppingcartPhase2ChangesWithVersionCheck(appId, appVersion) && !isReshopChange && !string.IsNullOrEmpty(session?.TravelType) && (session.TravelType == TravelType.CB.ToString() || session.TravelType == TravelType.CLB.ToString()))
                            {
                                description = _shoppingUtility.BuildPaxTypeDescription(price?.PaxTypeCode, price?.Description, price.Count);
                            }
                            bookingPrice.PaxTypeDescription = $"{price?.Count} {description}".ToLower();
                        }
                    }
                }

                if (!isReshopChange)
                {
                    if (!string.IsNullOrEmpty(bookingPrice.DisplayType) && bookingPrice.DisplayType.Equals("MILES") && isAwardBooking && !string.IsNullOrEmpty(sessionId))
                    {
                        await ValidateAwardMileageBalance(sessionId, price.Amount);
                    }
                }

                double tempDouble = 0;
                double.TryParse(NonDiscountTravelPrice > 0 ? NonDiscountTravelPrice.ToString() : price.Amount.ToString(), out tempDouble);
                bookingPrice.Value = Math.Round(tempDouble, 2, MidpointRounding.AwayFromZero);

                if (price.Currency.ToUpper() == "MIL")
                {
                    bookingPrice.FormattedDisplayValue = formatAwardAmountForDisplay(price.Amount.ToString(), false);
                }
                else
                {
                    bookingPrice.FormattedDisplayValue = formatAmountForDisplay(NonDiscountTravelPrice > 0 ? NonDiscountTravelPrice : price.Amount, ci, false);

                    if (_configuration.GetValue<bool>("EnableMilesPlusMoney") && string.Equals("MILESANDMONEY", price.Type, StringComparison.OrdinalIgnoreCase))
                    {
                        bookingPrice.FormattedDisplayValue = "-" + formatAmountForDisplay(price.Amount, new CultureInfo("en-US"), false); // Money and Miles have to be displayed in -ve format as per MOBILE-14807
                    }
                }

                _shoppingUtility.UpdatePricesForTravelCredits(bookingPrices, price, bookingPrice, session);
                bookingPrices.Add(bookingPrice);
            }

            return bookingPrices;
        }

        private string GetFormatPriceSearch(string searchType)
        {
            return !searchType.IsNullOrEmpty() ? "Fare " + searchType + " " : string.Empty;
        }

        private string BuildPriceTypeDescription(string searchType, string priceDescription, int price, string desc, bool isFareLockViewRes, bool isCorporateFareLock)
        {
            if (!searchType.IsNullOrEmpty() && !priceDescription.IsNullOrEmpty() && !price.IsNullOrEmpty())
            {
                if (isFareLockViewRes)
                {
                    var description = isCorporateFareLock ? "traveler)" : desc;
                    return GetFormatPriceSearch(searchType) + "(" + Convert.ToString(price) + " " + description;
                }
                else
                {
                    if (!priceDescription.ToUpper().Equals("CORPORATE RATE"))
                    {
                        return GetSearchTypeDesc(searchType) + "(" + Convert.ToString(price) + " " + desc;
                    }
                }
            }

            return string.Empty;
        }

        private string GetSearchTypeDesc(string searchType)
        {
            if (string.IsNullOrEmpty(searchType))
                return string.Empty;

            string searchTypeDesc = string.Empty;
            if (searchType.ToUpper().Equals("OW"))
            {
                searchTypeDesc = "Oneway";
            }
            else if (searchType.ToUpper().Equals("RT"))
            {
                searchTypeDesc = "Roundtrip";
            }
            else if (searchType.ToUpper().Equals("MD"))
            {
                searchTypeDesc = "Multipletrip";
            }

            return (!string.IsNullOrEmpty(searchTypeDesc)) ? "Fare " + searchTypeDesc + " " : string.Empty; //"Fare Oneway "
        }

        private string BuildYAPriceTypeDescription(string searchType)
        {
            if (searchType.ToUpper().Equals("OW"))
                return "Fare Oneway (1 young adult)";
            else
                if (searchType.ToUpper().Equals("RT"))
                return "Fare Roundtrip (1 young adult)";
            else
                if (searchType.ToUpper().Equals("MD"))
                return "Fare Multipletrip (1 young adult)";
            else
                return "Fare Oneway (1 young adult)";
        }

        private string GetFareDescription(DisplayPrice price)
        {
            return (price.Description.ToUpper().IndexOf("ADULT") == 0 ? "adult)" : (!price.Description.ToUpper().Contains("TOTAL")) ? price.Description.Replace("(", "").ToLower() : string.Empty);
        }

        private bool EnableYADesc(bool isReshop = false)
        {
            return _configuration.GetValue<bool>("EnableYoungAdultBooking") && _configuration.GetValue<bool>("EnableYADesc") && !isReshop;
        }

        private List<MOBSHOPPrice> AdjustTotal(List<MOBSHOPPrice> prices)
        {
            CultureInfo ci = null;

            List<MOBSHOPPrice> newPrices = prices;
            double fee = 0;
            foreach (MOBSHOPPrice p in newPrices)
            {
                if (ci == null)
                {
                    ci = GetCultureInfo(p.CurrencyCode);
                }

                if (fee == 0)
                {
                    foreach (MOBSHOPPrice q in newPrices)
                    {
                        if (q.DisplayType.Trim().ToUpper() == "RBF")
                        {
                            fee = q.Value;
                            break;
                        }
                    }
                }
                if (p.DisplayType.Trim().ToUpper() == "REFUNDPRICE" && p.Value < 0)
                {
                    p.Value *= -1;
                }
                if ((fee > 0 && p.DisplayType.Trim().ToUpper() == "TOTAL") || p.DisplayType.Trim().ToUpper() == "REFUNDPRICE")
                {
                    //update total
                    p.Value += fee;
                    p.DisplayValue = string.Format("{0:#,0.00}", p.Value);
                }
            }

            return newPrices;
        }

        private List<List<MOBSHOPTax>> GetTaxAndFeesAfterPriceChange(List<United.Services.FlightShopping.Common.DisplayCart.DisplayPrice> prices, bool isReshopChange = false, int appId = 0, string appVersion = "", string travelType = null)
        {
            List<List<MOBSHOPTax>> taxsAndFees = new List<List<MOBSHOPTax>>();
            CultureInfo ci = null;
            decimal taxTotal = 0.0M;
            decimal subTaxTotal = 0.0M;
            bool isTravelerPriceDirty = false;
            bool isEnableOmniCartMVP2Changes = _configuration.GetValue<bool>("EnableOmniCartMVP2Changes");

            foreach (var price in prices)
            {
                List<MOBSHOPTax> tmpTaxsAndFees = new List<MOBSHOPTax>();

                subTaxTotal = 0;

                if (price.SubItems != null && price.SubItems.Count > 0
                    && (!isReshopChange || (isReshopChange && price.Type.ToUpper() == "TRAVELERPRICE" && !isTravelerPriceDirty))
                    && (_configuration.GetValue<bool>("EnableCouponsforBooking") && price.Type.ToUpper() != "NONDISCOUNTPRICE-TRAVELERPRICE")) // Added by Hasnan - # 167553 - 10/04/2017
                {
                    foreach (var subItem in price.SubItems)
                    {
                        if (ci == null)
                        {
                            ci = GetCultureInfo(subItem.Currency);
                        }
                        MOBSHOPTax taxNfee = new MOBSHOPTax();
                        taxNfee = new MOBSHOPTax();
                        taxNfee.CurrencyCode = subItem.Currency;
                        taxNfee.Amount = subItem.Amount;
                        taxNfee.DisplayAmount = formatAmountForDisplay(taxNfee.Amount, ci, false);
                        taxNfee.TaxCode = subItem.Type;
                        taxNfee.TaxCodeDescription = subItem.Description;
                        isTravelerPriceDirty = true;
                        tmpTaxsAndFees.Add(taxNfee);

                        subTaxTotal += taxNfee.Amount;
                    }
                }

                if (tmpTaxsAndFees != null && tmpTaxsAndFees.Count > 0)
                {
                    //add new label as first item for UI
                    MOBSHOPTax tnf = new MOBSHOPTax();
                    tnf.CurrencyCode = tmpTaxsAndFees[0].CurrencyCode;
                    tnf.Amount = subTaxTotal;
                    tnf.DisplayAmount = formatAmountForDisplay(tnf.Amount, ci, false);
                    tnf.TaxCode = "PERPERSONTAX";
                    string description = price?.Description;
                    if (_shoppingUtility.EnableShoppingcartPhase2ChangesWithVersionCheck(appId, appVersion) && !isReshopChange && !string.IsNullOrEmpty(travelType) && (travelType == TravelType.CB.ToString() || travelType == TravelType.CLB.ToString()))
                    {
                        description = _shoppingUtility.BuildPaxTypeDescription(price?.PaxTypeCode, price?.Description, price.Count);
                    }
                    tnf.TaxCodeDescription = string.Format("{0} {1}: {2}{3}", price.Count, description.ToLower(), tnf.DisplayAmount
                        , isEnableOmniCartMVP2Changes ? "/person" : " per person");
                    tmpTaxsAndFees.Insert(0, tnf);
                }
                taxTotal += subTaxTotal * price.Count;
                if (tmpTaxsAndFees.Count > 0)
                {
                    taxsAndFees.Add(tmpTaxsAndFees);
                }

            }
            if (taxsAndFees != null && taxsAndFees.Count > 0)
            {
                //add grand total for all taxes
                List<MOBSHOPTax> lstTnfTotal = new List<MOBSHOPTax>();
                MOBSHOPTax tnfTotal = new MOBSHOPTax();
                tnfTotal.CurrencyCode = taxsAndFees[0][0].CurrencyCode;
                tnfTotal.Amount += taxTotal;
                tnfTotal.DisplayAmount = formatAmountForDisplay(tnfTotal.Amount, ci, false);
                tnfTotal.TaxCode = "TOTALTAX";
                tnfTotal.TaxCodeDescription = "Taxes and fees total";
                lstTnfTotal.Add(tnfTotal);
                taxsAndFees.Add(lstTnfTotal);
            }

            return taxsAndFees;
        }

        private List<List<MOBSHOPTax>> GetTaxAndFeesAfterPriceChange(List<United.Services.FlightShopping.Common.DisplayCart.DisplayPrice> prices, int numPax, bool isReshopChange = false, int appId = 0, string appVersion = "", string travelType = null)
        {
            List<List<MOBSHOPTax>> taxsAndFees = new List<List<MOBSHOPTax>>();
            CultureInfo ci = null;
            decimal taxTotal = 0.0M;
            decimal subTaxTotal = 0.0M;
            bool isTravelerPriceDirty = false;
            bool isEnableOmniCartMVP2Changes = _configuration.GetValue<bool>("EnableOmniCartMVP2Changes");

            foreach (var price in prices)
            {
                List<MOBSHOPTax> tmpTaxsAndFees = new List<MOBSHOPTax>();

                subTaxTotal = 0;

                if (_configuration.GetValue<bool>("EnableAdvanceSearchCouponBooking") && !string.IsNullOrEmpty(price?.Type) && price.Type.ToUpper() == "NONDISCOUNTPRICE-TRAVELERPRICE")
                    continue;

                if (price.SubItems != null && price.SubItems.Count > 0 && (!isReshopChange || (isReshopChange && price.Type.ToUpper() == "TRAVELERPRICE" && !isTravelerPriceDirty))) // Added by Hasnan - # 167553 - 10/04/2017
                {
                    foreach (var subItem in price.SubItems)
                    {
                        if (ci == null)
                        {
                            ci = GetCultureInfo(subItem.Currency);
                        }
                        MOBSHOPTax taxNfee = new MOBSHOPTax();
                        taxNfee = new MOBSHOPTax();
                        taxNfee.CurrencyCode = subItem.Currency;
                        taxNfee.Amount = subItem.Amount;
                        taxNfee.DisplayAmount = formatAmountForDisplay(taxNfee.Amount, ci, false);
                        taxNfee.TaxCode = subItem.Type;
                        taxNfee.TaxCodeDescription = subItem.Description;
                        isTravelerPriceDirty = true;
                        tmpTaxsAndFees.Add(taxNfee);

                        subTaxTotal += taxNfee.Amount;
                    }
                }

                if (tmpTaxsAndFees != null && tmpTaxsAndFees.Count > 0)
                {
                    //add new label as first item for UI
                    MOBSHOPTax tnf = new MOBSHOPTax();
                    tnf.CurrencyCode = tmpTaxsAndFees[0].CurrencyCode;
                    tnf.Amount = subTaxTotal;
                    tnf.DisplayAmount = formatAmountForDisplay(tnf.Amount, ci, false);
                    tnf.TaxCode = "PERPERSONTAX";
                    string description = price?.Description;
                    if (_shoppingUtility.EnableShoppingcartPhase2ChangesWithVersionCheck(appId, appVersion) && !isReshopChange && !string.IsNullOrEmpty(travelType) && (travelType == TravelType.CB.ToString() || travelType == TravelType.CLB.ToString()))
                    {
                        description = _shoppingUtility.BuildPaxTypeDescription(price?.PaxTypeCode, price?.Description, price.Count);
                    }
                    tnf.TaxCodeDescription = string.Format("{0} {1}: {2}{3}", price.Count, description.ToLower(), tnf.DisplayAmount,
                        isEnableOmniCartMVP2Changes ? "/person" : " per person");
                    tmpTaxsAndFees.Insert(0, tnf);
                }
                taxTotal += subTaxTotal * price.Count;
                if (tmpTaxsAndFees.Count > 0)
                {
                    taxsAndFees.Add(tmpTaxsAndFees);
                }

            }
            if (taxsAndFees != null && taxsAndFees.Count > 0)
            {
                //add grand total for all taxes
                List<MOBSHOPTax> lstTnfTotal = new List<MOBSHOPTax>();
                MOBSHOPTax tnfTotal = new MOBSHOPTax();
                tnfTotal.CurrencyCode = taxsAndFees[0][0].CurrencyCode;
                tnfTotal.Amount += taxTotal;
                tnfTotal.DisplayAmount = formatAmountForDisplay(tnfTotal.Amount, ci, false);
                tnfTotal.TaxCode = "TOTALTAX";
                tnfTotal.TaxCodeDescription = "Taxes and fees total";
                lstTnfTotal.Add(tnfTotal);
                taxsAndFees.Add(lstTnfTotal);
            }

            return taxsAndFees;
        }

        private List<MOBSHOPTax> AddFeesAfterPriceChange(List<United.Services.FlightShopping.Common.DisplayCart.DisplayPrice> prices)
        {

            List<MOBSHOPTax> taxsAndFees = new List<MOBSHOPTax>();
            CultureInfo ci = null;

            foreach (var price in prices)
            {
                if (ci == null)
                {
                    ci = GetCultureInfo(price.Currency);
                }
                MOBSHOPTax taxNfee = new MOBSHOPTax();
                taxNfee.CurrencyCode = price.Currency;
                taxNfee.Amount = price.Amount;
                taxNfee.DisplayAmount = formatAmountForDisplay(taxNfee.Amount, ci, false);
                taxNfee.TaxCode = price.Type;
                taxNfee.TaxCodeDescription = price.Description;
                if (taxNfee.TaxCode != "MPF")
                    taxsAndFees.Add(taxNfee);
            }
            return taxsAndFees;
        }


        private async Task<MOBSHOPReservation> BuildReservationResponseforFareLock(FlightReservationResponse response, MOBRegisterOfferRequest fareLockRequest, Session session)
        {
            MOBSHOPReservation fareLockReservationResponse = new MOBSHOPReservation(_configuration, _cachingService);
            ShoppingResponse shop = new ShoppingResponse();
            try
            {
                shop = await _sessionHelperService.GetSession<ShoppingResponse>(fareLockRequest.SessionId, shop.ObjectName, new List<string> { fareLockRequest.SessionId, shop.ObjectName }).ConfigureAwait(false);
            }
            catch (System.Exception ex) { throw new MOBUnitedException("Could not find your booking session."); }

            Reservation bookingPathReservation = new Reservation();
            try
            {
                bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(fareLockRequest.SessionId, bookingPathReservation.ObjectName, new List<string> { fareLockRequest.SessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);
            }
            catch (System.Exception ex) { throw new MOBUnitedException("Could not find your booking session."); }

            if (response != null)
            {
                // FlightReservationResponse response = JsonSerializer.NewtonSoftDeserialize<FlightReservationResponse>(jsonResponse);
                if (response != null && response.Status.Equals(United.Services.FlightShopping.Common.StatusType.Success) && response.Reservation != null)
                {
                    SelectTrip persistShop = new SelectTrip();
                    persistShop = await _sessionHelperService.GetSession<SelectTrip>(fareLockRequest.SessionId, persistShop.ObjectName, new List<string> { fareLockRequest.SessionId, persistShop.ObjectName }).ConfigureAwait(false);


                    List<MOBSHOPRewardProgram> rewardPrograms = new List<MOBSHOPRewardProgram>();
                    var res = await _cachingService.GetCache<string>(_configuration.GetValue<string>("FrequestFlyerRewardProgramListStaticGUID")+ ObjectNames.BookingFrequentFlyerList, _headers.ContextValues.TransactionId).ConfigureAwait(false);
                    rewardPrograms = JsonConvert.DeserializeObject<List<MOBSHOPRewardProgram>>(res);
                    if (rewardPrograms == null || rewardPrograms.Count == 0)
                    {
                        rewardPrograms = new List<MOBSHOPRewardProgram>();
                        var rewardTypes = _configuration.GetSection("rewardTypes").Get<List<RewardType>>();
                        rewardTypes = rewardTypes ?? new List<RewardType>();
                        foreach (var rt in rewardTypes) {
                            rewardPrograms.Add(new MOBSHOPRewardProgram()
                            {
                                ProgramID = rt.ProductID,
                                Type = rt.Type,
                                Description = rt.Description
                            });

                        }
                    }

                    persistShop.Responses[persistShop.LastSelectTripKey].Availability.Reservation.RewardPrograms = rewardPrograms;

                    if (response.DisplayCart != null && response.DisplayCart.DisplayPrices != null)
                    {
                        persistShop.Responses[persistShop.LastSelectTripKey].Availability.Reservation.PointOfSale = shop.Request.CountryCode;
                        if (IsEnableTaxForAgeDiversification(shop.Request.IsReshopChange, fareLockRequest.Application.Id, fareLockRequest.Application.Version.Major))
                        {
                            persistShop.Responses[persistShop.LastSelectTripKey].Availability.Reservation.Prices = await GetPrices(response.DisplayCart.DisplayPrices, false, string.Empty, searchType: shop.Request.SearchType, session: session);
                        }
                        else
                        {
                            persistShop.Responses[persistShop.LastSelectTripKey].Availability.Reservation.Prices = await GetPrices(response.DisplayCart.DisplayPrices, false, string.Empty);
                        }
                        if (response.DisplayCart.DisplayFees != null && response.DisplayCart.DisplayFees.Count > 0)
                        {

                            persistShop.Responses[persistShop.LastSelectTripKey].Availability.Reservation.Prices.AddRange(await GetPrices(response.DisplayCart.DisplayFees, false, string.Empty));
                        }
                        //need to add close in fee to TOTAL
                        persistShop.Responses[persistShop.LastSelectTripKey].Availability.Reservation.Prices = AdjustTotal(persistShop.Responses[persistShop.LastSelectTripKey].Availability.Reservation.Prices);
                        if (IsEnableTaxForAgeDiversification(shop.Request.IsReshopChange, fareLockRequest.Application.Id, fareLockRequest.Application.Version.Major))
                        {
                            if (persistShop.Responses[persistShop.LastSelectTripKey].Availability.Reservation.ShopReservationInfo2 == null)
                                persistShop.Responses[persistShop.LastSelectTripKey].Availability.Reservation.ShopReservationInfo2 = new MOBSHOPReservationInfo2();

                            if (persistShop.Responses[persistShop.LastSelectTripKey].Availability.Reservation.ShopReservationInfo2.InfoNationalityAndResidence == null)
                                persistShop.Responses[persistShop.LastSelectTripKey].Availability.Reservation.ShopReservationInfo2.InfoNationalityAndResidence = new MOBInfoNationalityAndResidence();

                            persistShop.Responses[persistShop.LastSelectTripKey].Availability.Reservation.ShopReservationInfo2.InfoNationalityAndResidence.ComplianceTaxes = GetTaxAndFeesAfterPriceChange(response.DisplayCart.DisplayPrices, shop.Request.NumberOfAdults, session.IsReshopChange, appId: fareLockRequest.Application.Id, appVersion: fareLockRequest.Application.Version.Major, travelType: session.TravelType);
                            if (response.DisplayCart.DisplayFees != null && response.DisplayCart.DisplayFees.Count > 0)
                            {
                                persistShop.Responses[persistShop.LastSelectTripKey].Availability.Reservation.ShopReservationInfo2.InfoNationalityAndResidence.ComplianceTaxes.Add(AddFeesAfterPriceChange(response.DisplayCart.DisplayFees));
                            }
                        }
                        else
                        {
                            persistShop.Responses[persistShop.LastSelectTripKey].Availability.Reservation.Taxes = GetTaxAndFees(response.DisplayCart.DisplayPrices, shop.Request.NumberOfAdults);
                            if (response.DisplayCart.DisplayFees != null && response.DisplayCart.DisplayFees.Count > 0)
                            {
                                //combine fees into taxes so that totals are correct
                                List<United.Services.FlightShopping.Common.DisplayCart.DisplayPrice> tempList = new List<Services.FlightShopping.Common.DisplayCart.DisplayPrice>();
                                tempList.AddRange(response.DisplayCart.DisplayPrices);
                                tempList.AddRange(response.DisplayCart.DisplayFees);
                                persistShop.Responses[persistShop.LastSelectTripKey].Availability.Reservation.Taxes = GetTaxAndFees(tempList, shop.Request.NumberOfAdults);
                            }
                        }
                        persistShop.Responses[persistShop.LastSelectTripKey].Availability.Reservation.TravelOptions = GetTravelOptions(response.DisplayCart, fareLockRequest.Application.Id, fareLockRequest.Application.Version.Major, shop.Request.IsReshopChange);
                        await _sessionHelperService.SaveSession<SelectTrip>(persistShop, bookingPathReservation.SessionId, new List<string> { bookingPathReservation.SessionId, persistShop.ObjectName }, persistShop.ObjectName).ConfigureAwait(false);

                        bookingPathReservation.PointOfSale = shop.Request.CountryCode;
                        if (IsEnableTaxForAgeDiversification(shop.Request.IsReshopChange, fareLockRequest.Application.Id, fareLockRequest.Application.Version.Major))
                        {
                            bookingPathReservation.Prices =await GetPrices(response.DisplayCart.DisplayPrices, false, string.Empty, session.IsReshopChange, shop.Request.SearchType, appId: fareLockRequest.Application.Id, appVersion: fareLockRequest.Application.Version.Major, session: session);
                        }
                        else
                        {

                            bookingPathReservation.Prices = await GetPrices(response.DisplayCart.DisplayPrices, false, string.Empty);
                        }
                        if (response.DisplayCart.DisplayFees != null && response.DisplayCart.DisplayFees.Count > 0)
                        {
                            //combine fees into taxes so that totals are correct
                            List<United.Services.FlightShopping.Common.DisplayCart.DisplayPrice> tempList = new List<Services.FlightShopping.Common.DisplayCart.DisplayPrice>();
                            tempList.AddRange(response.DisplayCart.DisplayPrices);
                            tempList.AddRange(response.DisplayCart.DisplayFees);
                            bookingPathReservation.Prices.AddRange( await GetPrices(tempList, false, string.Empty));
                        }

                        //need to add close in fee to TOTAL
                        bookingPathReservation.Prices = AdjustTotal(bookingPathReservation.Prices);
                        if (IsEnableTaxForAgeDiversification(shop.Request.IsReshopChange, fareLockRequest.Application.Id, fareLockRequest.Application.Version.Major))
                        {
                            if (bookingPathReservation.ShopReservationInfo2 == null)
                                bookingPathReservation.ShopReservationInfo2 = new MOBSHOPReservationInfo2();

                            if (bookingPathReservation.ShopReservationInfo2.InfoNationalityAndResidence == null)
                                bookingPathReservation.ShopReservationInfo2.InfoNationalityAndResidence = new MOBInfoNationalityAndResidence();

                            bookingPathReservation.ShopReservationInfo2.InfoNationalityAndResidence.ComplianceTaxes = GetTaxAndFeesAfterPriceChange(response.DisplayCart.DisplayPrices, shop.Request.NumberOfAdults, session.IsReshopChange, appId: fareLockRequest.Application.Id, appVersion: fareLockRequest.Application.Version.Major, travelType: session.TravelType);

                            if (response.DisplayCart.DisplayFees != null && response.DisplayCart.DisplayFees.Count > 0)
                            {
                                bookingPathReservation.ShopReservationInfo2.InfoNationalityAndResidence.ComplianceTaxes.Add(AddFeesAfterPriceChange(response.DisplayCart.DisplayFees));
                            }
                        }
                        else
                        {
                            bookingPathReservation.Taxes = GetTaxAndFees(response.DisplayCart.DisplayPrices, shop.Request.NumberOfAdults);
                            if (response.DisplayCart.DisplayFees != null && response.DisplayCart.DisplayFees.Count > 0)
                            {
                                //combine fees into taxes so that totals are correct
                                List<United.Services.FlightShopping.Common.DisplayCart.DisplayPrice> tempList = new List<Services.FlightShopping.Common.DisplayCart.DisplayPrice>();
                                tempList.AddRange(response.DisplayCart.DisplayPrices);
                                tempList.AddRange(response.DisplayCart.DisplayFees);
                                bookingPathReservation.Taxes = GetTaxAndFees(tempList, shop.Request.NumberOfAdults);
                            }
                        }
                        bookingPathReservation.TravelOptions = GetTravelOptions(response.DisplayCart, fareLockRequest.Application.Id, fareLockRequest.Application.Version.Major, shop.Request.IsReshopChange);

                        bookingPathReservation.Prices = UpdatePricesForEFS(persistShop.Responses[persistShop.LastSelectTripKey].Availability.Reservation, fareLockRequest.Application.Id, fareLockRequest.Application.Version.Major, shop.Request.IsReshopChange);

                        bookingPathReservation.UnregisterFareLock = false;
                        //commented in 22I code - onprem
                        if (_productUtility.IsBuyMilesFeatureEnabled(fareLockRequest.Application.Id, fareLockRequest.Application.Version.Major))
                            _shoppingBuyMilesHelper.ApplyPriceChangesForBuyMiles(response, null, bookingPathReservation);

                        await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, bookingPathReservation.SessionId, new List<string> { bookingPathReservation.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);
                    }

                    #region Define Booking Path Persist Reservation and Save to session - Venkat 08/13/2014
                    bookingPathReservation.PointOfSale = shop.Request.CountryCode;
                    bookingPathReservation.SessionId = fareLockRequest.SessionId;
                    bookingPathReservation.CartId = fareLockRequest.CartId;
                    bookingPathReservation.Trips = persistShop.Responses[persistShop.LastSelectTripKey].Availability.Reservation.Trips;
                    #region 
                    if (Convert.ToBoolean(_configuration.GetValue<string>("RemoveEplusSeatsAndOTP") ?? "false"))
                    {
                        bookingPathReservation.SeatPrices = null;// blank out EPlus seats 
                        bookingPathReservation.ClubPassPurchaseRequest = null;//blank out one time pass amount 
                        if (bookingPathReservation.TravelersCSL != null)
                        {
                            foreach (var traveler in bookingPathReservation.TravelersCSL)
                            {
                                if (traveler.Value.Seats != null)
                                {
                                    traveler.Value.Seats = null;
                                }
                            }
                        }
                    }
                    #endregion
                    await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, bookingPathReservation.SessionId, new List<string> { bookingPathReservation.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);
                    #endregion

                    ShopBookingDetailsResponse persistedShopBookingDetailsResponse = new ShopBookingDetailsResponse();
                    persistedShopBookingDetailsResponse = await _sessionHelperService.GetSession<ShopBookingDetailsResponse>(fareLockRequest.SessionId, persistedShopBookingDetailsResponse.ObjectName, new List<string> { fareLockRequest.SessionId, persistedShopBookingDetailsResponse.ObjectName }).ConfigureAwait(false);
                    //updated the necessary objects
                    persistedShopBookingDetailsResponse.SessionId = fareLockRequest.SessionId;
                    persistedShopBookingDetailsResponse.Reservation = response.Reservation;
                    await _sessionHelperService.SaveSession<ShopBookingDetailsResponse>(persistedShopBookingDetailsResponse, persistedShopBookingDetailsResponse.SessionId, new List<string> { persistedShopBookingDetailsResponse.SessionId, persistedShopBookingDetailsResponse.ObjectName }, persistedShopBookingDetailsResponse.ObjectName).ConfigureAwait(false);

                    fareLockReservationResponse = persistShop.Responses[persistShop.LastSelectTripKey].Availability.Reservation;
                    fareLockReservationResponse.TravelersCSL = new List<MOBCPTraveler>();
                    fareLockReservationResponse.UnregisterFareLock = false;

                    if (bookingPathReservation.TravelersCSL != null && bookingPathReservation.TravelersCSL.Count > 0)
                    {
                        foreach (string travelerKey in bookingPathReservation.TravelerKeys)
                        {
                            MOBCPTraveler bookingTravelerInfo = bookingPathReservation.TravelersCSL[travelerKey];
                            fareLockReservationResponse.TravelersCSL.Add(bookingTravelerInfo);
                        }
                    }
                    fareLockReservationResponse.CreditCards = bookingPathReservation.CreditCards;
                    fareLockReservationResponse.CreditCardsAddress = bookingPathReservation.CreditCardsAddress;
                    fareLockReservationResponse.FareLock = bookingPathReservation.FareLock;

                    if (response.Reservation != null)
                    {
                        if (fareLockReservationResponse.NumberOfTravelers == 1)
                        {
                            bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(fareLockRequest.SessionId, bookingPathReservation.ObjectName, new List<string> { fareLockRequest.SessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);
                            if (bookingPathReservation != null)
                            {
                                bookingPathReservation.GetALLSavedTravelers = false;
                                bookingPathReservation.TripInsuranceFile = null;
                                await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, fareLockRequest.SessionId, new List<string> { fareLockRequest.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);
                                fareLockReservationResponse.GetALLSavedTravelers = bookingPathReservation.GetALLSavedTravelers;
                            }
                        }
                        else
                        {
                            bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(fareLockRequest.SessionId, bookingPathReservation.ObjectName, new List<string> { fareLockRequest.SessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);
                            if (bookingPathReservation != null && bookingPathReservation.IsSignedInWithMP)
                            {
                                #region
                                MOBCPTraveler mobCPTraveler = await _productUtility.GetProfileOwnerTravelerCSL(fareLockRequest.SessionId);
                                if (mobCPTraveler != null)
                                {
                                    fareLockReservationResponse.TravelersCSL = new List<MOBCPTraveler>();
                                    bookingPathReservation.TravelersCSL = new SerializableDictionary<string, MOBCPTraveler>();
                                    bookingPathReservation.TravelerKeys = new List<string>();
                                    if (mobCPTraveler.IsProfileOwner)
                                    {

                                        AssignTravelerCslToAllEligibleTravelers(bookingPathReservation);
                                        bookingPathReservation.TripInsuranceFile = null;
                                       await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, fareLockRequest.SessionId, new List<string> { fareLockRequest.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);
                                    }
                                }
                                #endregion
                            }
                        }
                        fareLockReservationResponse = await _productUtility.GetReservationFromPersist(fareLockReservationResponse, session);
                        RemoveChaseAd_CFOP(fareLockReservationResponse);
                    }
                    fareLockReservationResponse.IsBookingCommonFOPEnabled = bookingPathReservation.IsBookingCommonFOPEnabled;
                }
                else
                {
                    if (response.Errors != null && response.Errors.Count > 0)
                    {
                        string errorMessage = string.Empty;
                        foreach (var error in response.Errors)
                        {
                            errorMessage = errorMessage + " " + error.Message;
                        }

                        throw new System.Exception(errorMessage);
                    }
                    else
                    {
                        throw new MOBUnitedException("Failed to retrieve booking details.");
                    }
                }
            }
            else
            {
                throw new MOBUnitedException("Failed to retrieve booking details.");
            }
            return fareLockReservationResponse;
        }

        private void RemoveChaseAd_CFOP(MOBSHOPReservation Reservation)
        {
            if (Reservation != null && Reservation.ShopReservationInfo2 != null && Reservation?.ShopReservationInfo2.ChaseCreditStatement != null)
            {
                Reservation.ShopReservationInfo2.ChaseCreditStatement = null;
            }
        }

        private void AssignTravelerCslToAllEligibleTravelers(Reservation bookingPathReservation)
        {
            if (bookingPathReservation.TravelersCSL != null &&
                                                    bookingPathReservation.TravelersCSL.Count() > 0 &&
                                                    bookingPathReservation.ShopReservationInfo2 != null &&
                                                    bookingPathReservation.ShopReservationInfo2.AllEligibleTravelersCSL != null &&
                                                    bookingPathReservation.ShopReservationInfo2.AllEligibleTravelersCSL.Count() > 0)
            {
                bookingPathReservation.ShopReservationInfo2.AllEligibleTravelersCSL.ForEach(p => p.IsPaxSelected = false);
                foreach (var selectedTraveler in bookingPathReservation.TravelersCSL)
                {
                    var eligibleTraveler = bookingPathReservation.ShopReservationInfo2.AllEligibleTravelersCSL.FirstOrDefault(p => p.Key == selectedTraveler.Value.Key);
                    if (eligibleTraveler != null)
                    {
                        eligibleTraveler.IsPaxSelected = true;
                    }
                }
            }
        }


        private List<MOBSHOPPrice> UpdatePricesForEFS(MOBSHOPReservation reservation, int appID, string appVersion, bool isReshop)
        {
            if (EnableEPlusAncillary(appID, appVersion, false) &&
                         reservation.TravelOptions != null &&
                         reservation.TravelOptions.Exists(t => t?.Key?.Trim()?.ToUpper() == "EFS"))
            {
                return UpdatePricesForBundles(reservation, null, appID, appVersion, isReshop, "EFS");
            }
            return reservation.Prices;
        }

        private List<MOBSHOPTax> GetTaxAndFees(List<United.Services.FlightShopping.Common.DisplayCart.DisplayPrice> prices, int numPax, bool isReshopChange = false)
        {
            List<MOBSHOPTax> taxsAndFees = new List<MOBSHOPTax>();
            CultureInfo ci = null;
            decimal taxTotal = 0.0M;
            bool isTravelerPriceDirty = false;
            string reshopTaxCodeDescription = string.Empty;

            foreach (var price in prices)
            {
                if (price.SubItems != null && price.SubItems.Count > 0 &&
                    (!isReshopChange || (isReshopChange && price.Type.ToUpper() == "TRAVELERPRICE" && !isTravelerPriceDirty))
                    && price.Type.Trim().ToUpper() != "RBF" // Added by Hasnan - # 167553 - 10/04/2017
                   )
                {
                    foreach (var subItem in price.SubItems)
                    {
                        if (ci == null)
                        {
                            ci = GetCultureInfo(subItem.Currency);
                        }

                        MOBSHOPTax taxNfee = new MOBSHOPTax();
                        taxNfee.CurrencyCode = subItem.Currency;
                        taxNfee.Amount = subItem.Amount;
                        taxNfee.DisplayAmount = formatAmountForDisplay(taxNfee.Amount, ci, false);
                        taxNfee.TaxCode = subItem.Type;
                        taxNfee.TaxCodeDescription = subItem.Description;
                        isTravelerPriceDirty = true;
                        taxsAndFees.Add(taxNfee);

                        taxTotal += taxNfee.Amount;
                    }

                    reshopTaxCodeDescription = price.Description;
                }
                else if (price.Type.Trim().ToUpper() == "RBF") //Reward Booking Fee
                {
                    if (ci == null)
                    {
                        ci = GetCultureInfo(price.Currency);
                    }
                    MOBSHOPTax taxNfee = new MOBSHOPTax();
                    taxNfee.CurrencyCode = price.Currency;
                    taxNfee.Amount = price.Amount / numPax;
                    taxNfee.DisplayAmount = formatAmountForDisplay(taxNfee.Amount, ci, false);
                    taxNfee.TaxCode = price.Type;
                    taxNfee.TaxCodeDescription = price.Description;
                    taxsAndFees.Add(taxNfee);

                    taxTotal += taxNfee.Amount;
                }
            }

            if (taxsAndFees != null && taxsAndFees.Count > 0)
            {
                //add new label as first item for UI
                MOBSHOPTax tnf = new MOBSHOPTax();
                tnf.CurrencyCode = taxsAndFees[0].CurrencyCode;
                tnf.Amount = taxTotal;
                tnf.DisplayAmount = formatAmountForDisplay(tnf.Amount, ci, false);
                tnf.TaxCode = "PERPERSONTAX";
                tnf.TaxCodeDescription = string.Format("{0} {1}: {2} per person", numPax,
                    (!string.IsNullOrEmpty(reshopTaxCodeDescription)) ? reshopTaxCodeDescription : (numPax > 1) ? "travelers" : "traveler", tnf.DisplayAmount);
                taxsAndFees.Insert(0, tnf);

                //add grand total for all taxes
                MOBSHOPTax tnfTotal = new MOBSHOPTax();
                tnfTotal.CurrencyCode = taxsAndFees[0].CurrencyCode;
                tnfTotal.Amount = taxTotal * numPax;
                tnfTotal.DisplayAmount = formatAmountForDisplay(tnfTotal.Amount, ci, false);
                tnfTotal.TaxCode = "TOTALTAX";
                tnfTotal.TaxCodeDescription = "Taxes and Fees Total";
                taxsAndFees.Add(tnfTotal);

            }

            return taxsAndFees;
        }


        private async Task<MOBBookingRegisterOfferResponse> RegisterFareLock(MOBRegisterOfferRequest request, Session session)
        {
            MOBBookingRegisterOfferResponse response = new MOBBookingRegisterOfferResponse();
            MOBRegisterOfferResponse registerOfferResponse = new MOBRegisterOfferResponse();
            try
            {
                if (EnableAdvanceSearchCouponBooking(request.Application.Id, request.Application.Version.Major) && request.ContinueToRegisterAncillary)
                {
                    if (!await AddOrRemovePromo(request, session, true, request.Flow))
                    {
                        throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                    }
                }
                FlightReservationResponse flightReservationResponse = new FlightReservationResponse();
                 var tupleResponse = await RegisterOffers(request, session, false);
                registerOfferResponse = tupleResponse.response;
                flightReservationResponse = tupleResponse.flightReservationResponse;
                response.ShoppingCart = registerOfferResponse.ShoppingCart;
                response.PkDispenserPublicKey = registerOfferResponse.PkDispenserPublicKey;
                response.Reservation = await BuildReservationResponseforFareLock(flightReservationResponse, request, session);
            }
            catch (MOBUnitedException uaex)
            {
                throw uaex;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            response.Flow = request.Flow;
            response.SessionId = request.SessionId;
            response.TransactionId = request.TransactionId;
            response.CheckinSessionId = request.CheckinSessionId;
            response.Disclaimer = GetDisclaimer();
            return response;
        }

        private List<MOBTypeOption> GetDisclaimer()
        {
            List<MOBTypeOption> disclaimer = new List<MOBTypeOption>();

            if (_configuration.GetValue<string>("MakeReservationDisclaimer") != null)
            {
                disclaimer.Add(new MOBTypeOption("MakeReservationDisclaimer", _configuration.GetValue<string>("MakeReservationDisclaimer")));
            }
            else
            {
                disclaimer.Add(new MOBTypeOption("MakeReservationDisclaimer", "*Miles shown are the actual miles flown for this segment.Mileage accrued will vary depending on the terms and conditions of your frequent flyer program."));
            }
            return disclaimer;
        }

        private List<MOBSHOPPrice> UpdatePricesForBundles(MOBSHOPReservation reservation, MOBRegisterOfferRequest request, int appID, string appVersion, bool isReshop, string productId = "")
        {
            List<MOBSHOPPrice> prices = reservation.Prices.Clone();
            string productId2 = "SFC";
            bool enableSFFTravelOption = System.Threading.Tasks.Task.Run(async () =>
            {
                return await _featureSettings.GetFeatureSettingValue("EnableFixForSFFTravelOption_MOBILE39209").ConfigureAwait(false);
            }).Result;

            if (reservation.TravelOptions != null && reservation.TravelOptions.Count > 0)
            {
                foreach (var travelOption in reservation.TravelOptions)
                {
                    //below if condition modified by prasad for bundle checking
                    //MOB-4676-Added condition to ignore the trip insurance which is added as traveloption - sandeep/saikiran
                    if (!travelOption.Key.Equals("PAS") && (!string.IsNullOrEmpty(travelOption.Type) && !travelOption.Type.ToUpper().Equals("TRIP INSURANCE"))
                       && (EnableEPlusAncillary(appID, appVersion, isReshop) ? !travelOption.Key.Trim().ToUpper().Equals("FARELOCK") : true)
                       && !(_configuration.GetValue<bool>("EnableEplusCodeRefactor") && !string.IsNullOrEmpty(productId) && productId.Trim().ToUpper() != travelOption.Key.Trim().ToUpper()))
                    {
                        UpdatePricesListForBundlesOrSFF(reservation, travelOption, out prices);
                        reservation.Prices = prices;
                    }
                    else if (enableSFFTravelOption && !travelOption.Key.Equals("PAS") && (!string.IsNullOrEmpty(travelOption.Type) && !travelOption.Type.ToUpper().Equals("TRIP INSURANCE"))
                       && (EnableEPlusAncillary(appID, appVersion, isReshop) ? !travelOption.Key.Trim().ToUpper().Equals("FARELOCK") : true)
                       && !(_configuration.GetValue<bool>("EnableEplusCodeRefactor") && !string.IsNullOrEmpty(productId2) && productId2.Trim().ToUpper() != travelOption.Key.Trim().ToUpper()))
                    {
                        UpdatePricesListForBundlesOrSFF(reservation, travelOption, out prices);
                        reservation.Prices = prices;
                    }
                }
            }
            if (request != null && request.ClubPassPurchaseRequest != null)
            {
                List<MOBSHOPPrice> totalPrices = new List<MOBSHOPPrice>();
                bool totalExist = false;
                double flightTotal = 0;

                CultureInfo ci = null;

                for (int i = 0; i < prices.Count; ++i)
                {
                    if (ci == null)
                        ci = GetCultureInfo(prices[i].CurrencyCode);

                    if (prices[i].DisplayType.ToUpper() == "GRAND TOTAL")
                    {
                        totalExist = true;
                        prices[i].DisplayValue = string.Format("{0:#,0.00}", Convert.ToDouble(prices[i].DisplayValue) + request.ClubPassPurchaseRequest.AmountPaid);
                        prices[i].FormattedDisplayValue = TopHelper.FormatAmountForDisplay(prices[i].DisplayValue, new CultureInfo("en-US")); //Convert.ToDouble(prices[i].DisplayValue).ToString("C2", CultureInfo.CurrentCulture);
                        double tempDouble1 = 0;
                        double.TryParse(prices[i].DisplayValue.ToString(), out tempDouble1);
                        prices[i].Value = Math.Round(tempDouble1, 2, MidpointRounding.AwayFromZero);
                    }
                    if (prices[i].DisplayType.ToUpper() == "TOTAL")
                    {
                        flightTotal = Convert.ToDouble(prices[i].DisplayValue);
                    }
                }
                MOBSHOPPrice otpPrice = new MOBSHOPPrice();
                otpPrice.CurrencyCode = prices[prices.Count - 1].CurrencyCode;
                otpPrice.DisplayType = "One-time Pass";
                otpPrice.DisplayValue = string.Format("{0:#,0.00}", request.ClubPassPurchaseRequest.AmountPaid);
                double tempDouble = 0;
                double.TryParse(otpPrice.DisplayValue.ToString(), out tempDouble);
                otpPrice.Value = Math.Round(tempDouble, 2, MidpointRounding.AwayFromZero);
                otpPrice.FormattedDisplayValue = TopHelper.FormatAmountForDisplay(request.ClubPassPurchaseRequest.AmountPaid.ToString(), new CultureInfo("en-US")); //request.ClubPassPurchaseRequest.AmountPaid.ToString("C2", CultureInfo.CurrentCulture);
                otpPrice.PriceType = "One-time Pass";
                if (totalExist)
                {
                    prices.Insert(prices.Count - 2, otpPrice);
                }
                else
                {
                    prices.Add(otpPrice);
                }

                if (!totalExist)
                {
                    MOBSHOPPrice totalPrice = new MOBSHOPPrice();
                    totalPrice.CurrencyCode = prices[prices.Count - 1].CurrencyCode;
                    totalPrice.DisplayType = "Grand Total";
                    totalPrice.DisplayValue = (flightTotal + request.ClubPassPurchaseRequest.AmountPaid).ToString("N2", CultureInfo.InvariantCulture);
                    //totalPrice.DisplayValue = string.Format("{0:#,0.00}", (flightTotal + request.ClubPassPurchaseRequest.AmountPaid).ToString("{0:#,0.00}", CultureInfo.InvariantCulture);
                    totalPrice.FormattedDisplayValue = TopHelper.FormatAmountForDisplay(totalPrice.DisplayValue, new CultureInfo("en-US")); //string.Format("${0:c}", totalPrice.DisplayValue);
                    double tempDouble1 = 0;
                    double.TryParse(totalPrice.DisplayValue.ToString(), out tempDouble1);
                    totalPrice.Value = Math.Round(tempDouble1, 2, MidpointRounding.AwayFromZero);
                    totalPrice.PriceType = "Grand Total";
                    prices.Add(totalPrice);
                }
            }
            return prices;
        }

        private void UpdatePricesListForBundlesOrSFF(MOBSHOPReservation reservation, MOBSHOPTravelOption travelOption, out List<MOBSHOPPrice> prices)
        {
            prices = reservation.Prices.Clone();
            List<MOBSHOPPrice> totalPrices = new List<MOBSHOPPrice>();
            bool totalExist = false;
            double flightTotal = 0;

            CultureInfo ci = null;

            bool enableSFFTravelOption = System.Threading.Tasks.Task.Run(async () =>
            {
                return await _featureSettings.GetFeatureSettingValue("EnableFixForSFFTravelOption_MOBILE39209").ConfigureAwait(false);
            }).Result;

            for (int i = 0; i < prices.Count; ++i)
            {
                if (ci == null)
                    ci = GetCultureInfo(prices[i].CurrencyCode);

                if (prices[i].DisplayType.ToUpper() == "GRAND TOTAL")
                {
                    totalExist = true;
                    prices[i].DisplayValue = string.Format("{0:#,0.00}", (Convert.ToDouble(prices[i].DisplayValue) + travelOption.Amount));
                    prices[i].FormattedDisplayValue = formatAmountForDisplay(prices[i].DisplayValue, ci, false); // string.Format("{0:c}", prices[i].DisplayValue);
                    double tempDouble1 = 0;
                    double.TryParse(prices[i].DisplayValue.ToString(), out tempDouble1);
                    prices[i].Value = Math.Round(tempDouble1, 2, MidpointRounding.AwayFromZero);
                }
                if (prices[i].DisplayType.ToUpper() == "TOTAL")
                {
                    flightTotal = Convert.ToDouble(prices[i].DisplayValue);
                }
            }
            MOBSHOPPrice travelOptionPrice = new MOBSHOPPrice();
            travelOptionPrice.CurrencyCode = travelOption.CurrencyCode;
            travelOptionPrice.DisplayType = "Travel Options";
            travelOptionPrice.DisplayValue = string.Format("{0:#,0.00}", travelOption.Amount.ToString());
            travelOptionPrice.FormattedDisplayValue = formatAmountForDisplay(travelOptionPrice.DisplayValue, ci, false); //Convert.ToDouble(travelOptionPrice.DisplayValue).ToString("C2", CultureInfo.CurrentCulture);
            travelOptionPrice.PriceTypeDescription = "Travel Options Bundle";

            if (_configuration.GetValue<bool>("EnableEplusCodeRefactor") && travelOption.Key?.Trim().ToUpper() == "EFS")
            {
                travelOptionPrice.PriceType = "EFS";
            }
            else if(enableSFFTravelOption && travelOption?.Code == "SFC")
            {
                travelOptionPrice.DisplayType = travelOption.Description;
                travelOptionPrice.PriceTypeDescription = "United’s Sustainable Aviation Fuel(SAF) contribuition";
                travelOptionPrice.PriceType = travelOption.Code;
            } else
            {
                travelOptionPrice.PriceType = "Travel Options";
            }

            double tmpDouble1 = 0;
            double.TryParse(travelOptionPrice.DisplayValue.ToString(), out tmpDouble1);
            travelOptionPrice.Value = Math.Round(tmpDouble1, 2, MidpointRounding.AwayFromZero);

            prices.Add(travelOptionPrice);

            if (!totalExist)
            {
                MOBSHOPPrice totalPrice = new MOBSHOPPrice();
                totalPrice.CurrencyCode = travelOption.CurrencyCode;
                totalPrice.DisplayType = "Grand Total";
                totalPrice.DisplayValue = (flightTotal + travelOption.Amount).ToString("N2", CultureInfo.InvariantCulture);
                totalPrice.FormattedDisplayValue = TopHelper.FormatAmountForDisplay(totalPrice.DisplayValue, new CultureInfo("en-US")); //string.Format("${0:c}", totalPrice.DisplayValue); 
                double tempDouble1 = 0;
                double.TryParse(totalPrice.DisplayValue.ToString(), out tempDouble1);
                totalPrice.Value = Math.Round(tempDouble1, 2, MidpointRounding.AwayFromZero);
                totalPrice.PriceType = "Grand Total";
                prices.Add(totalPrice);
            }
        }

        private List<MOBSHOPTravelOption> GetTravelBundleOptions(Services.FlightShopping.Common.DisplayCart.DisplayCart displayCart, int appID, string appVersion, bool isReshop)
        {
            List<MOBSHOPTravelOption> travelOptions = null;
            // MOBILE-25395: SAF
            var safCode = _configuration.GetValue<string>("SAFCode");

            if (displayCart != null && displayCart.TravelOptions != null && displayCart.TravelOptions.Count > 0)
            {
                CultureInfo ci = null;
                travelOptions = new List<MOBSHOPTravelOption>();
                bool addTripInsuranceInTravelOption = !_configuration.GetValue<bool>("ShowTripInsuranceBookingSwitch") && Convert.ToBoolean(_configuration.GetValue<string>("ShowTPIatPostBooking_ForAppVer_2.1.36_UpperVersions") ?? "false");
                foreach (var anOption in displayCart.TravelOptions)
                {
                    //Bypass SEATASSIGNMENTS
                    if (!string.IsNullOrEmpty(anOption.Type) && anOption.Type.Equals("SEATASSIGNMENTS", StringComparison.OrdinalIgnoreCase))
                        continue;

                    // below condition modified by prasad for bundle checking
                    //wade - added check for farelock as we were bypassing it
                    //if (!anOption.Type.Equals("Premium Access") && !anOption.Key.Trim().ToUpper().Contains("FARELOCK") && !(addTripInsuranceInTravelOption && anOption.Key.Trim().ToUpper().Contains("TPI")))
                    //{
                    //    continue;
                    //}
                    if (ci == null)
                    {
                        ci = GetCultureInfo(anOption.Currency);
                    }

                    MOBSHOPTravelOption travelOption = new MOBSHOPTravelOption();
                    travelOption.Amount = (double)anOption.Amount;
                    travelOption.DisplayAmount = formatAmountForDisplay(anOption.Amount, ci, false);
                    //??
                    if (anOption.Key.Trim().ToUpper().Contains("FARELOCK") || (addTripInsuranceInTravelOption && anOption.Key.Trim().ToUpper().Contains("TPI")))
                        travelOption.DisplayButtonAmount = formatAmountForDisplay(anOption.Amount, ci, false);
                    else
                        travelOption.DisplayButtonAmount = formatAmountForDisplay(anOption.Amount, ci, true);

                    travelOption.CurrencyCode = anOption.Currency;
                    travelOption.Deleted = anOption.Deleted;
                    travelOption.Description = anOption.Description;
                    if (EnableEPlusAncillary(appID, appVersion, isReshop) && anOption?.Key?.Trim().ToUpper() == "EFS")
                    {
                        travelOption.Key = anOption.Key;
                        travelOption.Code = anOption.Key;
                    }
                    if (anOption.Type == "BE")
                    {
                        //The below key values hardcoded by prasad ...may need to change later
                        travelOption.Key = "BUNDLES";
                        if (_configuration.GetValue<bool>("EnableCouponsforBooking"))
                        {
                            travelOption.Code = anOption.Key;
                        }
                    }
                    // MOBILE-25395: SAF
                    else if (anOption.Key == safCode)
                    {
                        travelOption.Key = safCode;
                        travelOption.Code = safCode;
                    }
                    travelOption.ProductId = anOption.ProductId;
                    travelOption.SubItems = GetTravelOptionSubItems(anOption.SubItems);
                    travelOption.BundleCode = GetTravelOptionEplus(anOption.SubItems);
                    if (EnableEPlusAncillary(appID, appVersion, isReshop) && anOption.SubItems != null && anOption.SubItems.Count > 0)
                    {
                        travelOption.BundleCode = GetTravelOptionEplusAncillary(anOption.SubItems, travelOption.BundleCode);
                        GetTravelOptionAncillaryDescription(anOption.SubItems, travelOption, displayCart);
                    }
                    if (!string.IsNullOrEmpty(anOption.Type))
                    {
                        travelOption.Type = anOption.Type.Equals("Premium Access") ? "Premier Access" : anOption.Type;
                    }


                    travelOptions.Add(travelOption);
                }
            }

            return travelOptions;
        }

        private List<MobShopBundleEplus> GetTravelOptionEplus(Services.FlightShopping.Common.DisplayCart.SubitemsCollection subitemsCollection)
        {
            List<MobShopBundleEplus> bundlecode = null;

            if (subitemsCollection != null && subitemsCollection.Count > 0)
            {
                bundlecode = new List<MobShopBundleEplus>();

                foreach (var item in subitemsCollection)
                {
                    if (item.Product != null)
                    {
                        foreach (var v in item.Product.SubProducts)
                        {
                            if (v.Code == "EPU")
                            {
                                MobShopBundleEplus objeplus = new MobShopBundleEplus();
                                objeplus.ProductKey = item.Product.ProductType;
                                objeplus.SegmentName = item.Product.PromoDescription;
                                objeplus.AssociatedTripIndex = item.Product.TripIndex;
                                bundlecode.Add(objeplus);
                            }
                        }
                    }
                    //subItems.Add(subItem);
                }

            }

            return bundlecode;
        }

        private List<string> GetTripIdsByBundle(string bundleCode, List<MOBProdDetail> prodDetails, Session session)
        {
            if (prodDetails != null)
            {
                var bundleProduct = prodDetails.FirstOrDefault(p => p.Code == bundleCode);
                if (bundleProduct != null)
                {
                    // MOBILE-25395: SAF
                    if (ConfigUtility.IsEnableSAFFeature(session) &&
                        string.Equals(bundleProduct.Code, _configuration.GetValue<string>("SAFCode"), StringComparison.OrdinalIgnoreCase))
                    {
                        return bundleProduct.Segments.Select(seg => seg.TripId.Split(',').Select(tripId => "10" + tripId).First()).ToList();
                    }
                    else
                    {
                        return bundleProduct.Segments.Select(seg => seg.TripId.Split(',').First()).ToList();
                    }
                }
            }
            return null;
        }

        private List<int> GetTripIndicesOfBundleChange(Collection<MerchandizingOfferDetails> products, string viewName)
        {
            var tripIndices = new List<int>();

            // Don't need to do any thing if it's not coming from RTI screen
            if (string.IsNullOrEmpty(viewName) || !viewName.Equals("RTI", StringComparison.OrdinalIgnoreCase))
                return tripIndices;

            // Get involved tripIndices
            tripIndices = new[] { products }
                                        .Where(listOfBundles => listOfBundles != null).SelectMany(bundle => bundle)
                                        .Where(bundle => bundle != null && bundle.TripIds != null).SelectMany(bundle => bundle.TripIds)
                                        .Where(tripId => !string.IsNullOrEmpty(tripId)).Distinct()
                                        .Select(int.Parse).ToList();

            return tripIndices;
        }

        private async Task<MOBBookingRegisterOfferResponse> RegisterBundles(MOBRegisterOfferRequest request, Session session)
        {
            MOBBookingRegisterOfferResponse response = new MOBBookingRegisterOfferResponse();
            MOBRegisterOfferResponse registerOfferResponse = new MOBRegisterOfferResponse();
            FlightReservationResponse cslResponse = new FlightReservationResponse();
            Reservation reservation = new Reservation();
            MOBShoppingCart persistedShoppingCart = new MOBShoppingCart();
            bool isOmniCartSavedTripFlow = IsEnableOmniCartReleaseCandidateTwoChanges_Bundles(request.Application.Id, request.Application.Version.Major) && request.IsOmniCartSavedTripFlow;
            try
            {
                if (request != null)
                {
                    if (!isOmniCartSavedTripFlow)
                    {
                        if (request.MerchandizingOfferDetails != null && request.MerchandizingOfferDetails.Count > 0)
                        {
                            #region Fixing the Null offers request sent by Client Request
                            MOBRegisterOfferRequest requestClone = request.Clone();
                            request.MerchandizingOfferDetails = new Collection<MerchandizingOfferDetails>();
                            foreach (MerchandizingOfferDetails products in requestClone.MerchandizingOfferDetails)
                            {
                                if (!string.IsNullOrEmpty(products.ProductCode))
                                {
                                    if (!_configuration.GetValue<bool>("DisableFixforiOSRTMultiPaxBundles_MOBILE14646") && request.Application.Id == 1
                                        && products.ProductIds?.Count > 0 && products.ProductIds[0].IndexOf(',') > -1)
                                    {
                                        string joinedProductIds = string.Join(",", products.ProductIds);
                                        products.ProductIds = joinedProductIds.Split(',').ToList();
                                    }
                                    request.MerchandizingOfferDetails.Add(products);
                                }
                            }
                            #endregion
                        }

                        persistedShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, persistedShoppingCart.ObjectName, new List<string> { request.SessionId, persistedShoppingCart.ObjectName }).ConfigureAwait(false);

                        bool isAdvanceSearchCouponApplied = EnableAdvanceSearchCouponBooking(request.Application.Id, request.Application.Version.Major);

                        if (_configuration.GetValue<bool>("EnableCouponsforBooking"))
                        {
                            if (request.ContinueToRegisterAncillary)
                            {
                                if (!await AddOrRemovePromo(request, session, true, request.Flow))
                                {
                                    throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                                }
                                // Loading the updated shopping cart persist for prices
                                persistedShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, persistedShoppingCart.ObjectName, new List<string> { request.SessionId, persistedShoppingCart.ObjectName }).ConfigureAwait(false);
                            }
                            else
                            {
                                var reservationcheck = await _sessionHelperService.GetSession<Reservation>(request.SessionId, reservation.ObjectName, new List<string> { request.SessionId, reservation.ObjectName });
                                if (persistedShoppingCart?.Products != null && persistedShoppingCart.Products.Any(p => p.CouponDetails != null && p.CouponDetails.Count > 0)
                                    || (isAdvanceSearchCouponApplied && persistedShoppingCart?.Products != null
                                                                     && persistedShoppingCart?.PromoCodeDetails?.PromoCodes != null
                                                                     && persistedShoppingCart.PromoCodeDetails.PromoCodes.Count > 0))
                                {
                                    if (isAdvanceSearchCouponApplied)
                                    {
                                        if (reservationcheck?.ShopReservationInfo2?.NextViewName == "RTI")
                                        {
                                            var changeInTravelerMessage = _configuration.GetValue<string>("PromoCodeRemovalmessage");
                                            response.PromoCodeRemoveAlertForProducts = string.IsNullOrWhiteSpace(changeInTravelerMessage) ? null : new MOBSection
                                            {
                                                Text1 = changeInTravelerMessage,
                                                Text2 = "Cancel",
                                                Text3 = "Continue"
                                            };
                                            return response;
                                        }
                                        else if (reservationcheck?.ShopReservationInfo2?.NextViewName != "RTI" && persistedShoppingCart?.PromoCodeDetails != null && persistedShoppingCart.PromoCodeDetails.PromoCodes?.Count > 0)
                                        {
                                            if (await ValidateBundleSelectedWhenCouponIsApplied(request, persistedShoppingCart, reservationcheck))
                                            {
                                                var changeInTravelerMessage = _configuration.GetValue<string>("AdvanceSearchCouponWithRegisterBundlesErrorMessage");
                                                response.PromoCodeRemoveAlertForProducts = string.IsNullOrWhiteSpace(changeInTravelerMessage) ? null : new MOBSection
                                                {
                                                    Text1 = changeInTravelerMessage,
                                                    Text2 = "Remove bundle",
                                                    Text3 = "Continue",
                                                    MessageType = "Booking"
                                                };
                                                return response;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var changeInTravelerMessage = _configuration.GetValue<string>("PromoCodeRemovalmessage");
                                        response.PromoCodeRemoveAlertForProducts = string.IsNullOrWhiteSpace(changeInTravelerMessage) ? null : new MOBSection
                                        {
                                            Text1 = changeInTravelerMessage,
                                            Text2 = "Cancel",
                                            Text3 = "Continue"
                                        };
                                        return response;
                                    }
                                }
                            }
                        }

                        if (_configuration.GetValue<bool>("BundleCart"))
                        {
                            if (persistedShoppingCart != null && !string.IsNullOrEmpty(persistedShoppingCart.BundleCartId))
                            {
                                request.CartId = persistedShoppingCart.BundleCartId;
                            }
                        }
                        reservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, reservation.ObjectName, new List<string> { request.SessionId, reservation.ObjectName }).ConfigureAwait(false);
                        if (_shoppingCartUtility.IsEnableOmniCartMVP2Changes(request.Application.Id, request.Application.Version.Major, reservation?.ShopReservationInfo2?.IsDisplayCart == true) ? !_shoppingCartUtility.IsEnableBundleLiveUpdateChanges(request.Application.Id, request.Application.Version.Major, reservation?.ShopReservationInfo2?.IsDisplayCart == true) : true)
                        {
                            if (persistedShoppingCart.Products.Count() > 0 && persistedShoppingCart.Products.Where(x => x.ProdDescription == "BE").Count() > 0)
                            {
                                var previousRegisteredBundlesinShoppingCart = persistedShoppingCart.Products.Where(x => x.ProdDescription == "BE").ToList();
                                var products = previousRegisteredBundlesinShoppingCart.Select(x => new MerchandizingOfferDetails
                                {
                                    ProductCode = x.Code,
                                    TripIds = x.Segments.Select(y => y.TripId).SelectMany(y => y.Split(',').ToList()).ToList(),
                                    ProductIds = x.Segments.SelectMany(y => y.ProductIds).ToList().Except(request.MerchandizingOfferDetails.Where(y => y.ProductCode == x.Code).SelectMany(y => y.ProductIds).ToList()).ToList(),
                                    SelectedTripProductIDs = x.Segments.Select(y => y.ProductId).ToList(),
                                    IsOfferRegistered = true
                                }).ToList();
                                products.AddRange(request.MerchandizingOfferDetails);
                                request.MerchandizingOfferDetails = new Collection<MerchandizingOfferDetails>(products.Where(x => x.ProductIds.Count() > 0).ToList());
                            }
                        }
                    }
                    if ((request.MerchandizingOfferDetails != null && request.MerchandizingOfferDetails.Count > 0))
                    {
                        if (_shoppingCartUtility.IsEnableBundleLiveUpdateChanges(request.Application.Id, request.Application.Version.Major, reservation?.ShopReservationInfo2?.IsDisplayCart == true))
                        {
                            await BuildRegisteredBundleOfferRequest(persistedShoppingCart, request);
                        }
                        var tupleResponse = await RegisterOffers(request, session, false);
                        registerOfferResponse = tupleResponse.response;
                        cslResponse = tupleResponse.flightReservationResponse;
                        response.PkDispenserPublicKey = registerOfferResponse.PkDispenserPublicKey;
                        response.ShoppingCart = registerOfferResponse.ShoppingCart;
                    }
                    else
                    {
                        response.ShoppingCart = persistedShoppingCart;
                        response.PkDispenserPublicKey = await _pKDispenserPublicKey.GetCachedOrNewpkDispenserPublicKey(request.Application.Id, request.Application.Version.Major, request.DeviceId, session.SessionId, session.Token, session?.CatalogItems).ConfigureAwait(false);
                    }
                    response.Flow = request.Flow;
                    response.SessionId = request.SessionId;
                    response.TransactionId = request.TransactionId;
                    response.CheckinSessionId = registerOfferResponse.CheckinSessionId;
                    reservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, reservation.ObjectName, new List<string> { request.SessionId, reservation.ObjectName }).ConfigureAwait(false);
                    if (reservation != null)
                    {
                        response.Reservation = MakeReservationFromPersistReservation(reservation);

                        var enableSFFTravelOption = System.Threading.Tasks.Task.Run(async () =>
                        {
                            return await _featureSettings.GetFeatureSettingValue("EnableFixForSFFTravelOption_MOBILE39209").ConfigureAwait(false);
                        }).Result;

                        if (await _featureToggles.IsEnableETCCreditsInBookingFeature(session?.CatalogItems))
                        {
                            await SetEligibilityforETCTravelCredit(response.Reservation, session, reservation);
                        }

                        List<MOBSHOPPrice> prices = null;
                        if (response.Reservation.Prices != null && response.Reservation.Prices.Count > 0)
                        {
                            double travelOptionSubTotal = 0.0;
                            foreach (var price in response.Reservation.Prices)
                            {
                                if (price.PriceType.ToUpper().Equals("TRAVEL OPTIONS") || price.PriceType.ToUpper().Equals("ONE-TIME PASS") || (enableSFFTravelOption && price.PriceType.ToUpper().Equals("SFC")))
                                {
                                    travelOptionSubTotal = travelOptionSubTotal + Convert.ToDouble(price.DisplayValue);
                                }
                                else if (!price.PriceType.ToUpper().Equals("GRAND TOTAL"))
                                {
                                    if (prices == null)
                                    {
                                        prices = new List<MOBSHOPPrice>();
                                    }
                                    prices.Add(price);
                                }
                            }

                            foreach (var price in response.Reservation.Prices)
                            {
                                if (price.PriceType.ToUpper().Equals("GRAND TOTAL"))
                                {
                                    double grandTotal = Convert.ToDouble(price.DisplayValue);
                                    price.DisplayValue = string.Format("{0:#,0.00}", grandTotal - travelOptionSubTotal);
                                    price.FormattedDisplayValue = TopHelper.FormatAmountForDisplay(price.DisplayValue, new CultureInfo("en-US")); //string.Format("${0:c}", price.DisplayValue);
                                    double tempDouble1 = 0;
                                    double.TryParse(price.DisplayValue.ToString(), out tempDouble1);
                                    price.Value = Math.Round(tempDouble1, 2, MidpointRounding.AwayFromZero);
                                    if (prices == null)
                                    {
                                        prices = new List<MOBSHOPPrice>();
                                    }
                                    prices.Add(price);
                                }
                            }
                        }

                        response.Reservation.Prices = prices;

                        response.Reservation.SeatPrices = reservation.SeatPrices.Clone();
                        response.Reservation.Taxes = reservation.Taxes.Clone();
                        response.Reservation.Trips = reservation.Trips.Clone();
                        if ((request.MerchandizingOfferDetails != null && request.MerchandizingOfferDetails.Count > 0))
                        {
                            response.Reservation.TravelOptions = GetTravelBundleOptions(cslResponse.DisplayCart, request.Application.Id, request.Application.Version.Major, reservation.IsReshopChange);
                        }


                        MOBBookingBundlesResponse objbundleResponse = new MOBBookingBundlesResponse(_configuration);
                        objbundleResponse = await _sessionHelperService.GetSession<MOBBookingBundlesResponse>(request.SessionId, objbundleResponse.ObjectName, new List<string> { request.SessionId, objbundleResponse.ObjectName }).ConfigureAwait(false);

                        if (objbundleResponse?.Products != null)
                        {
                            if (response.Reservation.TravelOptions != null)
                            {
                                foreach (MOBBundleProduct bundleProduct in objbundleResponse.Products)
                                {
                                    foreach (MOBSHOPTravelOption travelOption in response.Reservation.TravelOptions)
                                    {
                                        if (travelOption.SubItems != null && travelOption.SubItems.Any() &&
                                            travelOption.SubItems[0].ProductId == bundleProduct.ProductCode)
                                        {
                                            if (bundleProduct.Tile != null && bundleProduct.Tile.OfferDescription != null)
                                                travelOption.BundleOfferDescription = bundleProduct.Tile.OfferDescription.Select(desc => "- " + desc).ToList();
                                        }
                                    }
                                }
                            }
                        }

                        if (_shoppingCartUtility.IsEnableBundleLiveUpdateChanges(request.Application.Id, request.Application.Version.Major, reservation?.ShopReservationInfo2?.IsDisplayCart == true))
                        {
                            persistedShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, persistedShoppingCart.ObjectName, new List<string> { request.SessionId, persistedShoppingCart.ObjectName }).ConfigureAwait(false);
                            if (response.Reservation.TravelOptions != null && request.MerchandizingOfferDetails != null)
                            {
                                foreach (var travelOption in response.Reservation.TravelOptions)
                                {

                                    if (travelOption.SubItems != null && travelOption.SubItems.Any())
                                    {
                                        travelOption.TripIds = GetTripIdsByBundle(travelOption.SubItems[0].ProductId, persistedShoppingCart.Products, session);
                                        travelOption.BundleOfferTitle = "Travel Options Bundle";
                                    }
                                }
                            }

                        }
                        else
                        {
                            if (response.Reservation.TravelOptions != null && request.MerchandizingOfferDetails != null)
                            {
                                foreach (var travelOption in response.Reservation.TravelOptions)
                                {
                                    foreach (var requestProduct in request.MerchandizingOfferDetails)
                                    {
                                        if (travelOption.SubItems != null && travelOption.SubItems.Any() &&
                                            travelOption.SubItems[0].ProductId == requestProduct.ProductCode)
                                        {
                                            travelOption.TripIds = requestProduct.TripIds;
                                            travelOption.BundleOfferTitle = "Travel Options Bundle";
                                        }
                                    }
                                }
                            }
                        }

                        if (request.ClubPassPurchaseRequest != null && request.ClubPassPurchaseRequest.NumberOfPasses > 0)
                        {
                            response.Reservation.ClubPassPurchaseRequest = request.ClubPassPurchaseRequest;
                            reservation.ClubPassPurchaseRequest = response.Reservation.ClubPassPurchaseRequest;
                        }
                        else
                        {
                            response.Reservation.ClubPassPurchaseRequest = null;
                            reservation.ClubPassPurchaseRequest = null;
                        }

                        if (_configuration.GetValue<bool>("EnableEplusCodeRefactor") && request != null && request.MerchandizingOfferDetails != null && request.MerchandizingOfferDetails.Count > 0)
                        {
                            response.Reservation.Prices = UpdatePricesForBundles(response.Reservation, request, request.Application.Id, request.Application.Version.Major, reservation.IsReshopChange, "BUNDLES");
                        }
                        else if (!_configuration.GetValue<bool>("EnableEplusCodeRefactor"))
                        {
                            response.Reservation.Prices = UpdatePricesForBundles(response.Reservation, request, request.Application.Id, request.Application.Version.Major, reservation.IsReshopChange);
                        }
                        reservation.TravelOptions = response.Reservation.TravelOptions;
                        reservation.ClubPassPurchaseRequest = response.Reservation.ClubPassPurchaseRequest;
                        if (response.Reservation.Prices != null && response.Reservation.Prices.Count > 0)
                        {
                            reservation.Prices = response.Reservation.Prices;
                        }
                        if (session != null && !string.IsNullOrEmpty(session.EmployeeId))
                        {
                            response.Reservation.IsEmp20 = true;
                        }
                        if (!isOmniCartSavedTripFlow && !(_shoppingCartUtility.IsEnableOmniCartMVP2Changes(request.Application.Id, request.Application.Version.Major, true) && request.IsRemove)) // No Need to update the nextview name if we are coming from omnicart flow.For partial bundle selection sceanrio (Bundle selected only for one trip) we will end up calling the register offer one more time which is updating the next view name to "RTI" instead of seats.
                        {
                            if (_shoppingCartUtility.IsEnableBundleLiveUpdateChanges(request.Application.Id, request.Application.Version.Major, reservation?.ShopReservationInfo2?.IsDisplayCart == true))
                            {
                                if ((reservation.ShopReservationInfo2.NextViewName == string.Empty
                                || (reservation.ShopReservationInfo2.IsForceSeatMapInRTI && reservation.ShopReservationInfo2.NextViewName == "TravelOption"))
                                && !(reservation.IsELF && !reservation.IsSSA))
                                {
                                    reservation.ShopReservationInfo2.NextViewName = "Seats";
                                    reservation.ShopReservationInfo2.IsForceSeatMapInRTI = true;
                                }
                            }
                            else
                            {
                                if ((reservation.ShopReservationInfo2.NextViewName == string.Empty
                                ||
                                (reservation.ShopReservationInfo2.IsForceSeatMapInRTI && reservation.ShopReservationInfo2.NextViewName == "TravelOption")
                                )
                                && !(reservation.IsELF && !reservation.IsSSA))
                                {
                                    reservation.ShopReservationInfo2.NextViewName = "Seats";
                                    reservation.ShopReservationInfo2.IsForceSeatMapInRTI = true;
                                }
                                else
                                {
                                    reservation.ShopReservationInfo2.NextViewName = "RTI";
                                }
                            }
                        }
                        try
                        {
                            if (!_configuration.GetValue<bool>("DisableAutoAssignSeatBundle"))
                            {
                                var tripIndices = GetTripIndicesOfBundleChange(request.MerchandizingOfferDetails, reservation.ShopReservationInfo2.NextViewName);
                                if (tripIndices != null && tripIndices.Any())
                                {
                                    await _sessionHelperService.SaveSession<Reservation>(reservation, request.SessionId, new List<string> { request.SessionId, reservation.ObjectName }, reservation.ObjectName).ConfigureAwait(false);

                                    await UpdateSeatsAfterBundleChanged(request.SessionId, request.Application.Id, request.Application.Version.Major, request.DeviceId, tripIndices);
                                    reservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, reservation.ObjectName, new List<string> { request.SessionId, reservation.ObjectName }).ConfigureAwait(false);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("RegisterBundles_CFOP-AutoAssignSeatBundle Exception:{exception} with {sessionId}", JsonConvert.SerializeObject(ex), request.SessionId);
                            // ignore when auto assigning of seats failed
                        }

                        if (reservation.ShopReservationInfo2.NextViewName == "RTI" || reservation.IsELF)
                        {
                            #region TPI in booking path
                            if (EnableTPI(request.Application.Id, request.Application.Version.Major, 3) && !reservation.IsReshopChange)
                            {
                                // call TPI 
                                try
                                {
                                    string token = session.Token;
                                    MOBTPIInfoInBookingPath tPIInfo = await GetBookingPathTPIInfo(request.SessionId, request.LanguageCode, request.Application, request.DeviceId, response.Reservation.CartId, token, true, false, false);
                                    reservation.TripInsuranceFile = new MOBTripInsuranceFile();
                                    reservation.TripInsuranceFile.TripInsuranceBookingInfo = tPIInfo;
                                }
                                catch (Exception ex)
                                {
                                    reservation.TripInsuranceFile = null;
                                }
                            }
                            #endregion
                        }
                        // Code is moved to utility so that it can be reused across different actions
                        if (_shoppingCartUtility.EnableChaseOfferRTI(request.Application.Id, request.Application.Version.Major))
                            _shoppingCartUtility.UpdateChaseCreditStatement(reservation);

                        if (_shoppingCartUtility.IsEnableBundleLiveUpdateChanges(request.Application.Id, request.Application.Version.Major, reservation?.ShopReservationInfo2?.IsDisplayCart == true))
                        {
                            response.BundleResponse = await UpdateBundleResponse(reservation, request.SessionId);
                        }
                        response.Reservation = await _productUtility.GetReservationFromPersist(response.Reservation, session);

                        if (response != null && response.Reservation != null &&
                            response.Reservation.TravelersCSL != null && response.Reservation.TravelersCSL.Any() &&
                            response.Reservation.ShopReservationInfo2 != null &&
                            response.Reservation.ShopReservationInfo2.AllEligibleTravelersCSL != null &&
                            response.Reservation.ShopReservationInfo2.AllEligibleTravelersCSL.Count > 0)
                        {
                            response.Reservation.TravelersCSL[0].SelectedSpecialNeeds = reservation.ShopReservationInfo2.AllEligibleTravelersCSL[0].SelectedSpecialNeeds;

                            response.Reservation.TravelersCSL[0].SelectedSpecialNeedMessages = reservation.ShopReservationInfo2.AllEligibleTravelersCSL[0].SelectedSpecialNeedMessages;
                        }

                        //Code by prasad prasad for Emp-20 single pax fix
                        if (reservation.NumberOfTravelers == 1 && response.Reservation.TravelersCSL == null)
                        {
                            if (reservation.ShopReservationInfo2.AllEligibleTravelersCSL != null)
                            {
                                response.Reservation.TravelersCSL = new List<MOBCPTraveler>();
                                foreach (MOBCPTraveler cpTraveler in reservation.ShopReservationInfo2.AllEligibleTravelersCSL)
                                {
                                    if (cpTraveler.IsProfileOwner)
                                    {
                                        response.Reservation.TravelersCSL.Add(cpTraveler);
                                    }
                                }
                            }
                        }
                    }
                }
                response.Reservation.IsBookingCommonFOPEnabled = reservation.IsBookingCommonFOPEnabled;
            }
            catch (MOBUnitedException uaex)
            {
                throw uaex;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }

            return response;
        }

        private async Task<string> GetCachedOrNewpkDispenserPublicKey(int appId, string appVersion, string deviceId, string transactionId, string token)
        {
            string pkDispenserPublicKey = null;
            if (_configuration.GetValue<bool>("EnablePKDispenserKeyRotationAndOAEPPadding"))
            {
                var key = string.Format(_configuration.GetValue<string>("PKDispenserKeyTokenKeyFormat"), appId);
                var pKDispenserKey = await _cachingService.GetCache<string>(key, "TID1").ConfigureAwait(false);
                try
                {
                    United.Service.Presentation.SecurityResponseModel.PKDispenserKey obj = DataContextJsonSerializer.DeserializeJsonDataContract<United.Service.Presentation.SecurityResponseModel.PKDispenserKey>(pKDispenserKey);
                    pkDispenserPublicKey = obj == null ? null : obj.PublicKey;
                }
                catch { }
            }
            else
                pkDispenserPublicKey = await _cachingService.GetCache<string>(GetCSSPublicKeyPersistSessionStaticGUID(appId) + "pkDispenserPublicKey", "TID1").ConfigureAwait(false);

            return string.IsNullOrEmpty(pkDispenserPublicKey)
                    ? await GetPkDispenserPublicKey(appId, deviceId, appVersion, transactionId, token)
                    : pkDispenserPublicKey;
        }

        private async Task<string> GetPkDispenserPublicKey(int applicationId, string deviceId, string appVersion, string transactionId, string authToken)
        {
            #region
            //**RSA Public Key Implementation**//
            string transId = string.IsNullOrEmpty(transactionId) ? "trans0" : transactionId;
            string key = string.Format(_configuration.GetValue<string>("PKDispenserKeyTokenKeyFormat"), applicationId);
            var cacheResponse = await _cachingService.GetCache<string>(key, transId).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(cacheResponse))
            {
                var obj = JsonConvert.DeserializeObject<PKDispenserKey>(cacheResponse);

                if (!string.IsNullOrEmpty(obj.PublicKey))
                {
                    return obj.PublicKey;
                }
            }

            var response = await _pKDispenserService.GetPkDispenserPublicKey<Service.Presentation.SecurityResponseModel.PKDispenserResponse>(authToken, transactionId, string.Empty).ConfigureAwait(false);
            return GetPublicKeyFromtheResponse(response, transactionId, applicationId);
            #endregion
        }

        private string GetPublicKeyFromtheResponse(Service.Presentation.SecurityResponseModel.PKDispenserResponse response, string transactionId, int applicationId)
        {
            string pkDispenserPublicKey = string.Empty;

            if (response != null && response.Keys != null && response.Keys.Count > 0)
            {
                var obj = (from st in response.Keys
                           where st.CryptoTypeID.Trim().Equals("2")
                           select st).ToList();
                obj[0].PublicKey = obj[0].PublicKey.Replace("\r", "").Replace("\n", "").Replace("-----BEGIN PUBLIC KEY-----", "").Replace("-----END PUBLIC KEY-----", "").Trim();
                pkDispenserPublicKey = obj[0].PublicKey;

                string transId = string.IsNullOrEmpty(transactionId) ? "trans0" : transactionId;
                string key = string.Format(_configuration.GetValue<string>("PKDispenserKeyTokenKeyFormat"), applicationId);
                _cachingService.SaveCache<United.Service.Presentation.SecurityResponseModel.PKDispenserKey>(key, obj[0], transId, new TimeSpan(1, 30, 0));

                string tokenKey = string.Format(_configuration.GetValue<string>("PKDispenserKeyTokenKeyFormat"), "Token::" + applicationId);
                _cachingService.SaveCache<string>(tokenKey, pkDispenserPublicKey, transId, new TimeSpan(1, 30, 0));
            }
            else
            {
                string exceptionMessage = _configuration.GetValue<string>("Booking2OGenericExceptionMessage");
                if (!String.IsNullOrEmpty(_configuration.GetValue<string>("UnableToGetPkDispenserPublicKeyErrorMessage")))
                {
                    exceptionMessage = _configuration.GetValue<string>("UnableToGetPkDispenserPublicKeyErrorMessage");
                }
                throw new MOBUnitedException(exceptionMessage);
            }
            return pkDispenserPublicKey;
        }

        private string GetCSSPublicKeyPersistSessionStaticGUID(int applicationId)
        {
            #region Get Aplication and Profile Ids
            string[] cSSPublicKeyPersistSessionStaticGUIDs = _configuration.GetValue<string>("CSSPublicKeyPersistSessionStaticGUID").Split('|');
            List<string> applicationDeviceTokenSessionIDList = new List<string>();
            foreach (string applicationSessionGUID in cSSPublicKeyPersistSessionStaticGUIDs)
            {
                #region
                if (Convert.ToInt32(applicationSessionGUID.Split('~')[0].ToString().ToUpper().Trim()) == applicationId)
                {
                    return applicationSessionGUID.Split('~')[1].ToString().Trim();
                }
                #endregion
            }
            return "1CSSPublicKeyPersistStatSesion4IphoneApp";
            #endregion
        }


        private bool IsEnableOmniCartReleaseCandidateTwoChanges_Bundles(int applicationId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableOmniCartReleaseCandidateTwoChanges_Bundles") && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Android_EnableOmniCartReleaseCandidateTwoChanges_Bundles_AppVersion"), _configuration.GetValue<string>("iPhone_EnableOmniCartReleaseCandidateTwoChanges_Bundles_AppVersion"));
        }

        private bool IsCheckinFlow(string flowName)
        {
            FlowType flowType;
            if (!Enum.TryParse(flowName, out flowType))
                return false;
            return flowType == FlowType.CHECKIN;
        }

        private async Task<List<MOBMobileCMSContentMessages>> GetTermsAndConditions()
        {
            var cmsContentMessages = new List<MOBMobileCMSContentMessages>();
            var docKeys = "PCU_TnC";
            var docs = await _legalDocumentsForTitlesService.GetNewLegalDocumentsForTitles(docKeys, _headers.ContextValues.TransactionId, true).ConfigureAwait(false); ;
            if (docs != null && docs.Any())
            {
                foreach (var doc in docs)
                {
                    var cmsContentMessage = new MOBMobileCMSContentMessages();
                    cmsContentMessage.ContentFull = doc.LegalDocument;
                    cmsContentMessage.Title = doc.Title;
                    cmsContentMessages.Add(cmsContentMessage);
                }
            }

            return cmsContentMessages;
        }

        private List<MOBTypeOption> GetPATermsAndConditionsList()
        {
            List<MOBTypeOption> tAndCList = new List<MOBTypeOption>();
            if (_configuration.GetValue<string>("PremierAccessTermsAndConditionsList") != null)
            {
                string premierAccessTermsAndConditionsList = _configuration.GetValue<string>("PremierAccessTermsAndConditionsList");
                foreach (string eachItem in premierAccessTermsAndConditionsList.Split('~'))
                {
                    tAndCList.Add(new MOBTypeOption(eachItem.Split('|')[0].ToString(), eachItem.Split('|')[1].ToString()));
                }
            }
            else
            {
                #region
                tAndCList.Add(new MOBTypeOption("paTandC1", "This Premier Access offer is nonrefundable and non-transferable"));
                tAndCList.Add(new MOBTypeOption("paTandC2", "Voluntary changes to your itinerary may forfeit your Premier Access purchase and \n any associated fees."));
                tAndCList.Add(new MOBTypeOption("paTandC3", "In the event of a flight cancellation or involuntary schedule change, we will refund \n the fees paid for the unused Premier Access product upon request."));
                tAndCList.Add(new MOBTypeOption("paTandC4", "Premier Access is offered only on flights operated by United and United Express."));
                tAndCList.Add(new MOBTypeOption("paTandC5", "This Premier Access offer is processed based on availability at time of purchase."));
                tAndCList.Add(new MOBTypeOption("paTandC6", "Premier Access does not guarantee wait time in airport check-in, boarding, or security lines. Premier Access does not exempt passengers from check-in time limits."));
                tAndCList.Add(new MOBTypeOption("paTandC7", "Premier Access benefits apply only to the customer who purchased Premier Access \n unless purchased for all customers on a reservation. Each travel companion must purchase Premier Access in order to receive benefits."));
                tAndCList.Add(new MOBTypeOption("paTandC8", "“Premier Access” must be printed or displayed on your boarding pass in order to \n receive benefits."));
                tAndCList.Add(new MOBTypeOption("paTandC9", "This offer is made at United's discretion and is subject to change or termination \n at any time with or without notice to the customer."));
                tAndCList.Add(new MOBTypeOption("paTandC10", "By clicking “I agree - Continue to purchase” you agree to all terms and conditions."));
                #endregion
            }

            return tAndCList;
        }

        private List<MOBTypeOption> GetPBContentList(string configValue)
        {
            List<MOBTypeOption> contentList = new List<MOBTypeOption>();
            if (_configuration.GetValue<string>(configValue) != null)
            {
                string pBContentList = _configuration.GetValue<string>(configValue);
                foreach (string eachItem in pBContentList.Split('~'))
                {
                    contentList.Add(new MOBTypeOption(eachItem.Split('|')[0].ToString(), eachItem.Split('|')[1].ToString()));
                }
            }

            return contentList;
        }

        private async Task<List<MOBMobileCMSContentMessages>> GetTermsAndConditions(bool hasPremierAccelerator)
        {
            var dbKey = hasPremierAccelerator ? "AAPA_TERMS_AND_CONDITIONS_AA_PA_MP"
                                              : "AAPA_TERMS_AND_CONDITIONS_AA_MP";

            var docs = await _legalDocumentsForTitlesService.GetNewLegalDocumentsForTitles(dbKey, _headers.ContextValues.TransactionId, true).ConfigureAwait(false);
            if (docs == null || !docs.Any())
                return null;

            var tncs = new List<MOBMobileCMSContentMessages>();
            foreach (var doc in docs)
            {
                var tnc = new MOBMobileCMSContentMessages
                {
                    Title = "Terms and conditions",
                    ContentFull = doc.LegalDocument,
                    ContentShort = _configuration.GetValue<string>("PaymentTnCMessage"),
                    HeadLine = doc.Title
                };
                tncs.Add(tnc);
            }

            return tncs;
        }

        private async Task<List<MOBMobileCMSContentMessages>> GetProductBasedTermAndConditions(United.Service.Presentation.ProductResponseModel.ProductOffer productVendorOffer, FlightReservationResponse flightReservationResponse, bool isPost, bool isGetCartInfo = false)
        {
            List<MOBMobileCMSContentMessages> tNClist = new List<MOBMobileCMSContentMessages>();
            MOBMobileCMSContentMessages tNC = null;
            List<MOBTypeOption> typeOption = null;

            var productCodes = isPost ? flightReservationResponse.CheckoutResponse.ShoppingCart.Items.Where(x => x.Product.FirstOrDefault().Code != "RES").Select(x => x.Product.FirstOrDefault().Code).ToList() :
                                       flightReservationResponse.ShoppingCart.Items.Where(x => x.Product.FirstOrDefault().Code != "RES").Select(x => x.Product.FirstOrDefault().Code).ToList();

            if (productCodes == null || !productCodes.Any())
                return null;

            if (isPost == true)
            {
                bool isTPIFailed = flightReservationResponse.CheckoutResponse.ShoppingCartResponse.Items.Any(x => x.Item.Product[0].Code == "TPI") &&
                    flightReservationResponse.CheckoutResponse.ShoppingCartResponse.Items.Where(x => x.Item.Product[0].Code == "TPI").Select(x => x.Item).Any(y => y.Status.Contains("FAILED"));

                isPost = !isTPIFailed; //If TPI failed during checkout then set isPost to false.
            }

            foreach (var productCode in productCodes)
            {
                if (isPost == false)
                {
                    switch (productCode)
                    {
                        case "PCU":
                            tNC = new MOBMobileCMSContentMessages();
                            List<MOBMobileCMSContentMessages> tncPCU = await GetTermsAndConditions();
                            tNC.Title = "Terms and conditions";
                            tNC.ContentShort = _configuration.GetValue<string>("PaymentTnCMessage");
                            tNC.ContentFull = tncPCU[0].ContentFull;
                            tNC.HeadLine = tncPCU[0].Title;
                            tNClist.Add(tNC);
                            break;

                        case "PAS":
                            tNC = new MOBMobileCMSContentMessages();
                            typeOption = new List<MOBTypeOption>();
                            typeOption = GetPATermsAndConditionsList();

                            tNC.Title = "Terms and conditions";
                            tNC.ContentShort = _configuration.GetValue<string>("PaymentTnCMessage");
                            tNC.ContentFull = string.Join("<br><br>", typeOption.Select(x => x.Value));
                            tNC.HeadLine = "Premier Access";
                            tNClist.Add(tNC);
                            break;

                        case "PBS":
                            tNC = new MOBMobileCMSContentMessages();
                            typeOption = new List<MOBTypeOption>();
                            typeOption = GetPBContentList("PriorityBoardingTermsAndConditionsList");

                            tNC.Title = "Terms and conditions";
                            tNC.ContentShort = _configuration.GetValue<string>("PaymentTnCMessage");
                            tNC.ContentFull = "<ul><li>" + string.Join("<br></li><li>", typeOption.Select(x => x.Value)) + "</li></ul>";
                            tNC.HeadLine = "Priority Boarding";
                            tNClist.Add(tNC);
                            break;

                        case "TPI":
                            if (productVendorOffer == null)
                                break;
                            tNC = new MOBMobileCMSContentMessages();
                            var product = productVendorOffer.Offers.FirstOrDefault(a => a.ProductInformation.ProductDetails.Where(b => b.Product != null && b.Product.Code.ToUpper().Trim() == "TPI").ToList().Count > 0).
                                ProductInformation.ProductDetails.FirstOrDefault(c => c.Product != null && c.Product.Code.ToUpper().Trim() == "TPI").Product;

                            string tncTPI = string.Empty;
                            string tncTPIMessage1 = product.Presentation.Contents.Where(x => x.Header.ToUpper() == "MobPaymentTAndCHeader1Message".ToUpper()).Select(x => x.Body).FirstOrDefault().ToString();
                            string tncTPIMessage2 = product.Presentation.Contents.Where(x => x.Header.ToUpper() == "MobPaymentTAndCBodyUrlMessage".ToUpper()).Select(x => x.Body).FirstOrDefault().ToString();
                            string tncTPIMessage3 = product.Presentation.Contents.Where(x => x.Header.ToUpper() == "MobPaymentTAndCUrlHeaderMessage".ToUpper()).Select(x => x.Body).FirstOrDefault().ToString();
                            string tncTPIMessage4 = product.Presentation.Contents.Where(x => x.Header.ToUpper() == "MobPaymentTAndCUrlHeader2Message".ToUpper()).Select(x => x.Body).FirstOrDefault().ToString();
                            string tncTPIMessage5 = product.Presentation.Contents.Any(x => x.Header.ToUpper() == "MobTIDetailsTAndCUrlMessage".ToUpper()) ? product.Presentation.Contents.Where(x => x.Header.ToUpper() == "MobTIDetailsTAndCUrlMessage".ToUpper()).Select(x => x.Body).FirstOrDefault().ToString() : string.Empty;
                            string tncTPIMessage6 = product.Presentation.Contents.Any(x => x.Header.ToUpper() == "MobTIDetailsTAndCUrlHeaderMessage".ToUpper()) ? product.Presentation.Contents.Where(x => x.Header.ToUpper() == "MobTIDetailsTAndCUrlHeaderMessage".ToUpper()).Select(x => x.Body).FirstOrDefault().ToString() : string.Empty;
                            string tncTPIMessage7 = product.Presentation.Contents.Any(x => x.Header.ToUpper() == "MobTGIAndMessage".ToUpper()) ? product.Presentation.Contents.Where(x => x.Header.ToUpper() == "MobTGIAndMessage".ToUpper()).Select(x => x.Body).FirstOrDefault().ToString() : string.Empty;
                            if (string.IsNullOrEmpty(tncTPIMessage5) || string.IsNullOrEmpty(tncTPIMessage6) || string.IsNullOrEmpty(tncTPIMessage7))
                                tncTPI = tncTPIMessage1 + " <a href =\"" + tncTPIMessage2 + "\" target=\"_blank\">" + tncTPIMessage3 + "</a> " + tncTPIMessage4;
                            else
                                tncTPI = tncTPIMessage1 + " " + tncTPIMessage4 + " <a href =\"" + tncTPIMessage2 + "\" target=\"_blank\">" + tncTPIMessage3 + "</a> " + tncTPIMessage7 + " <a href =\"" + tncTPIMessage5 + "\" target=\"_blank\">" + tncTPIMessage6 + "</a> ";
                            tNC.Title = "Terms and conditions";
                            tNC.ContentShort = _configuration.GetValue<string>("PaymentTnCMessage");
                            tNC.ContentFull = tncTPI;
                            tNC.HeadLine = "Terms and conditions";
                            tNClist.Add(tNC);
                            break;
                        case "AAC":
                            var acceleratorTnCs = await GetTermsAndConditions(flightReservationResponse.DisplayCart.TravelOptions.Any(d => d.Key == "PAC"));
                            if (acceleratorTnCs != null && acceleratorTnCs.Any())
                            {
                                tNClist.AddRange(acceleratorTnCs);
                            }
                            break;
                        case "SEATASSIGNMENTS":
                            if (!isGetCartInfo)
                                break;
                            if (string.IsNullOrEmpty(_configuration.GetValue<string>("EnablePCUSelectedSeatPurchaseViewRes")))
                                break;
                            var seatTypes = flightReservationResponse.DisplayCart.DisplaySeats.Where(s => s.SeatPrice > 0).Select(s => GetCommonSeatCode(s.SeatPromotionCode)).ToList();
                            var seatsTnCs = new List<MOBItem>();
                            if (seatTypes.Any() && seatTypes.Contains("ASA"))
                            {
                                var asaTncs = await GetCaptions("CFOP_UnitedTravelOptions_ASA_TnC");
                                if (asaTncs != null && asaTncs.Any())
                                {
                                    seatsTnCs.AddRange(asaTncs);
                                }
                            }
                            if (seatTypes.Any() && (seatTypes.Contains("EPU") || seatTypes.Contains("PSL")))
                            {
                                var eplusTncs = await GetCaptions("CFOP_UnitedTravelOptions_EPU_TnC");
                                if (eplusTncs != null && eplusTncs.Any())
                                {
                                    seatsTnCs.AddRange(eplusTncs);
                                }
                            }
                            if (seatTypes.Any() && seatTypes.Contains("PZA"))
                            {
                                var pzaTncs = await GetCaptions("CFOP_UnitedTravelOptions_PZA_TnC");
                                if (pzaTncs != null && pzaTncs.Any())
                                {
                                    seatsTnCs.AddRange(pzaTncs);
                                }
                            }

                            if (seatsTnCs.Any())
                            {
                                tNC = new MOBMobileCMSContentMessages
                                {
                                    Title = "Terms and conditions",
                                    ContentShort = _configuration.GetValue<string>("PaymentTnCMessage"),
                                    ContentFull = string.Join("<br>", seatsTnCs.Select(a => a.CurrentValue)),
                                    HeadLine = seatsTnCs[0].Id
                                };
                                tNClist.Add(tNC);
                            }
                            break;
                        case "PET":
                            if (!isGetCartInfo)
                                break;
                            tNC = new MOBMobileCMSContentMessages();
                            List<MOBMobileCMSContentMessages> tncPET = await GetTermsAndConditions("PETINCABIN_TNC");
                            tNC.Title = "Terms and conditions";
                            tNC.ContentShort = _configuration.GetValue<string>("PaymentTnCMessage");
                            tNC.ContentFull = tncPET[0].ContentFull;
                            tNC.HeadLine = tncPET[0].Title;
                            tNClist.Add(tNC);
                            break;
                        case "OTP":
                            if (!isGetCartInfo)
                                break;
                            tNC = new MOBMobileCMSContentMessages();
                            string termsAndConditionsConfigurationKey = "UnitedClubDayPassTermsAndConditions" + flightReservationResponse.DisplayCart.CountryCode;
                            typeOption = new List<MOBTypeOption>();
                            typeOption = GetTCList(termsAndConditionsConfigurationKey);

                            tNC.Title = "Terms and conditions";
                            tNC.ContentShort = _configuration.GetValue<string>("PaymentTnCMessage");
                            tNC.ContentFull = "<ul><li>" + string.Join("<br></li><li>", typeOption.Select(x => x.Value)) + "</li></ul>";
                            tNC.HeadLine = "United Club Pass";
                            tNClist.Add(tNC);
                            break;
                    }
                }
                else if (isPost == true)
                {
                    switch (productCode)
                    {
                        case "TPI":
                            string specialCharacter = _configuration.GetValue<string>("TPIinfo-SpecialCharacter") ?? "";
                            tNC = new MOBMobileCMSContentMessages();
                            var product = productVendorOffer.Offers.FirstOrDefault(a => a.ProductInformation.ProductDetails.Where(b => b.Product != null && b.Product.Code.ToUpper().Trim() == "TPI").ToList().Count > 0).
                                ProductInformation.ProductDetails.FirstOrDefault(c => c.Product != null && c.Product.Code.ToUpper().Trim() == "TPI").Product;

                            string tncTPIMessage1 = product.Presentation.Contents.Where(x => x.Header == "MobTIConfirmationBody1Message").Select(x => x.Body).FirstOrDefault().ToString().Replace("(R)", specialCharacter);
                            string tncTPIMessage2 = product.Presentation.Contents.Where(x => x.Header == "MobTIConfirmationBody2Message").Select(x => x.Body).FirstOrDefault().ToString();

                            string tncTPI = tncTPIMessage1 + "\n\n" + tncTPIMessage2;

                            tNC.Title = _configuration.GetValue<string>("TPIPurchaseResposne-ConfirmationResponseMessage") ?? ""; ;
                            tNC.ContentShort = _configuration.GetValue<string>("TPIPurchaseResposne-ConfirmationResponseEmailMessage"); // + ((flightReservationResponse.Reservation.EmailAddress.Count() > 0) ? flightReservationResponse.Reservation.EmailAddress.Where(x => x.Address != null).Select(x => x.Address).FirstOrDefault().ToString() : null) ?? "";
                            tNC.ContentFull = tncTPI;
                            tNClist.Add(tNC);
                            break;
                    }
                }
            }

            return tNClist;
        }

        private List<MOBTypeOption> GetTCList(string configValue)
        {
            List<MOBTypeOption> contentList = new List<MOBTypeOption>();
            if (_configuration.GetValue<string>(configValue) != null)
            {
                string tcContentList = _configuration.GetValue<string>(configValue);
                foreach (string eachItem in tcContentList.Split('|'))
                {
                    contentList.Add(new MOBTypeOption() { Value = eachItem });
                }
            }
            return contentList;
        }

        private async Task<List<MOBMobileCMSContentMessages>> GetTermsAndConditions(string title)
        {
            var cmsContentMessages = new List<MOBMobileCMSContentMessages>();
            var docKeys = title;
            var docs = await _legalDocumentsForTitlesService.GetNewLegalDocumentsForTitles(docKeys, _headers.ContextValues.TransactionId, true).ConfigureAwait(false);
            if (docs != null && docs.Any())
            {
                foreach (var doc in docs)
                {
                    var cmsContentMessage = new MOBMobileCMSContentMessages();
                    cmsContentMessage.ContentFull = doc.LegalDocument;
                    cmsContentMessage.Title = doc.Title;
                    cmsContentMessages.Add(cmsContentMessage);
                }
            }

            return cmsContentMessages;
        }

        private async Task<(MOBRegisterOfferResponse response, FlightReservationResponse flightReservationResponse)> RegisterOffers(MOBRegisterOfferRequest request, Session session, bool temp)
        {
            MOBRegisterOfferResponse response = new MOBRegisterOfferResponse();
            FlightReservationResponse flightReservationResponse = null;
            var productOffer = new GetOffers();
            var productVendorOffer = new GetVendorOffers();
            var reservationDetail = new United.Service.Presentation.ReservationResponseModel.ReservationDetail();
            var productFareLockOffer = new United.Service.Presentation.ProductResponseModel.ProductOffer();
            if (!(IsEnableOmniCartReleaseCandidateTwoChanges_Bundles(request.Application.Id, request.Application.Version.Major) && request.IsOmniCartSavedTripFlow))
            {
                if (request.MerchandizingOfferDetails.Count() == 0)
                {
                    response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                    return (response, flightReservationResponse);
                }
                if (!IsCheckinFlow(request.Flow))
                {
                    ShopBookingDetailsResponse detailResponse = new ShopBookingDetailsResponse();
                    detailResponse = await _sessionHelperService.GetSession<ShopBookingDetailsResponse>(request.SessionId, detailResponse.ObjectName, new List<string> { request.SessionId, detailResponse.ObjectName }).ConfigureAwait(false);
                    productFareLockOffer = detailResponse?.FareLock;
                }
                productOffer = await _sessionHelperService.GetSession<GetOffers>(session.SessionId, productOffer.ObjectName, new List<string> { session.SessionId, productOffer.ObjectName }).ConfigureAwait(false);
                productVendorOffer = await _sessionHelperService.GetSession<GetVendorOffers>(session.SessionId, productVendorOffer.ObjectName, new List<string> { session.SessionId, productVendorOffer.ObjectName }).ConfigureAwait(false);
                reservationDetail = await _sessionHelperService.GetSession<United.Service.Presentation.ReservationResponseModel.ReservationDetail>(request.SessionId, reservationDetail.GetType().FullName, new List<string> { request.SessionId, reservationDetail.GetType().FullName }).ConfigureAwait(false);
            }
            var isCompleteFarelockPurchase = _configuration.GetValue<bool>("EnableFareLockPurchaseViewRes") && request.MerchandizingOfferDetails.Any(o => (o.ProductCode != null && o.ProductCode.Equals("FLK_VIEWRES", StringComparison.OrdinalIgnoreCase)));
            if (IsEnableOmniCartReleaseCandidateTwoChanges_Bundles(request.Application.Id, request.Application.Version.Major) && request.IsOmniCartSavedTripFlow)
            {
                flightReservationResponse = await GetFlightReservationResponseByCartId(session.SessionId, request.CartId);
            }
            else
            {
                flightReservationResponse = isCompleteFarelockPurchase ? await RegisterFareLockReservation(request, reservationDetail, session)
                                                        : await RegisterOffers(request, productOffer, productVendorOffer, reservationDetail, session, productFareLockOffer);
            }
            if (IncludeFFCResidual(request.Application.Id, request.Application.Version.Major) && request.Flow == FlowType.BOOKING.ToString())
            {
                response.ShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(session.SessionId, new MOBShoppingCart().ObjectName, new List<string> { session.SessionId, new MOBShoppingCart().ObjectName }).ConfigureAwait(false);
                if (response.ShoppingCart == null)
                {
                    response.ShoppingCart = new MOBShoppingCart();
                }
            }
            response.ShoppingCart.Products = await _shoppingCartUtility.ConfirmationPageProductInfo(flightReservationResponse, false, false, request.Flow.ToString(), request.Application, null, false, false, null, sessionId: request?.SessionId);
            response.ShoppingCart.TermsAndConditions = await GetProductBasedTermAndConditions(productVendorOffer, flightReservationResponse, false);
            response.ShoppingCart.PaymentTarget = GetPaymentTargetForRegisterFop(flightReservationResponse, request.Flow, isCompleteFarelockPurchase);
            double price = GetGrandTotalPriceForShoppingCart(isCompleteFarelockPurchase, flightReservationResponse, false, request.Flow);
            response.ShoppingCart.TotalPrice = String.Format("{0:0.00}", price);
            response.ShoppingCart.Flow = request.Flow;
            // response.ShoppingCart.DisplayTotalPrice = string.Format("${0:c}", Decimal.Parse(price.ToString())); //Decimal.Parse(price.ToString()).ToString("c");
            response.ShoppingCart.DisplayTotalPrice = Decimal.Parse(price.ToString()).ToString("c", new CultureInfo("en-us")); //Decimal.Parse(price.ToString()).ToString("c");
            response.ShoppingCart.CartId = flightReservationResponse.CartId.ToString();
            response.PkDispenserPublicKey = await _pKDispenserPublicKey.GetCachedOrNewpkDispenserPublicKey(request.Application.Id, request.Application.Version.Major, request.DeviceId, session.SessionId, session.Token, session?.CatalogItems).ConfigureAwait(false);

            if (isCompleteFarelockPurchase)
            {
                response.ShoppingCart.Trips =  _mSCFormsOfPayment.GetTrips(reservationDetail.Detail.FlightSegments, request.Flow).Result;
                response.ShoppingCart.SCTravelers = _mSCFormsOfPayment.GetTravelerCSLDetails(flightReservationResponse.Reservation, response.ShoppingCart.Trips, session.SessionId, request.Flow);
                //Prices & Taxes
                if (!flightReservationResponse.DisplayCart.IsNullOrEmpty() && !flightReservationResponse.DisplayCart.DisplayPrices.IsNullOrEmpty() && flightReservationResponse.DisplayCart.DisplayPrices.Any())
                {
                    // Journey Type will be "OW", "RT", "MD"
                    var JourneyType = GetJourneyTypeDescription(flightReservationResponse.Reservation);
                    bool isCorporateFare = IsCorporateTraveler(flightReservationResponse.Reservation.Characteristic);
                    response.ShoppingCart.Prices = await GetPrices(flightReservationResponse.DisplayCart.DisplayPrices, false, null, false, JourneyType, isCompleteFarelockPurchase, isCorporateFare, session: session);
                    response.ShoppingCart.Taxes = GetTaxAndFeesAfterPriceChange(flightReservationResponse.DisplayCart.DisplayPrices, false);
                    response.ShoppingCart.Captions = GetFareLockCaptions(flightReservationResponse.Reservation, JourneyType, isCorporateFare);
                    response.ShoppingCart.ELFLimitations = await GetELFLimitationsViewRes(response.ShoppingCart.Trips, request.Application.Id);
                }
            }
            if (response.ShoppingCart.Captions.IsNullOrEmpty())
            {
                response.ShoppingCart.Captions = await GetCaptions("PaymentPage_ViewRes_Captions");
            }
            response.ShoppingCart.TravelPolicyWarningAlert = await _shoppingCartUtility.BuildCorporateTravelPolicyWarningAlert(request, session, flightReservationResponse, response.ShoppingCart.IsCorporateBusinessNamePersonalized);
           
            IsHidePromoOption(response.ShoppingCart, request.Flow, request.Application, session.IsReshopChange);
            await AssignProfileSavedETCsFromPersist(request.SessionId, response.ShoppingCart);
            if (await _featureToggles.IsEnableETCCreditsInBookingFeature(session?.CatalogItems))
            {
                _shoppingCartUtility.ApplyETCCreditsOnRTIAction(response.ShoppingCart, flightReservationResponse.DisplayCart);
            }
            await _sessionHelperService.SaveSession<MOBShoppingCart>(response.ShoppingCart, request.SessionId, new List<string> { request.SessionId, response.ShoppingCart.ObjectName }, response.ShoppingCart.ObjectName).ConfigureAwait(false);

            _logger.LogInformation("RegisterOffers Response:{@Response}", JsonConvert.SerializeObject(response));

            return (response, flightReservationResponse);
        }

        private List<MOBSHOPTripBase> GetTripBase(List<MOBSHOPTrip> ShopTrip)
        {
            if (ShopTrip == null || !ShopTrip.Any())
                return null;

            List<MOBSHOPTripBase> trips = new List<MOBSHOPTripBase>();
            foreach (var trip in ShopTrip)
            {
                var tripBase = new MOBSHOPTripBase();
                tripBase = trip;
                tripBase.DepartDate = trip.DepartDate;
                trips.Add(tripBase);
            }
            return trips;
        }

        private List<MOBCPPhone> GetMobCpPhones(United.Service.Presentation.PersonModel.Contact contact)
        {
            if (contact.IsNullOrEmpty() || contact.PhoneNumbers.IsListNullOrEmpty())
                return null;

            var mobCpPhones = new List<MOBCPPhone>();

            foreach (var pnrPhone in contact.PhoneNumbers)
            {
                if (!pnrPhone.IsNullOrEmpty() && pnrPhone.CountryAccessCode.Equals("US"))
                {
                    var mobPhone = new MOBCPPhone();
                    var phoneNum = !pnrPhone.PhoneNumber.IsNullOrEmpty() ? pnrPhone.PhoneNumber : string.Empty;
                    if (phoneNum.Length == 11)
                    {
                        mobPhone.AreaNumber = phoneNum.Substring(1, 3);
                        mobPhone.PhoneNumber = Regex.Replace(phoneNum.Substring(4), @"[^0-9]+", "");
                        mobPhone.CountryCode = pnrPhone.CountryAccessCode;
                        mobPhone.ChannelCodeDescription = pnrPhone.CountryAccessCode;
                        mobCpPhones.Add(mobPhone);
                    }
                }
            }

            return mobCpPhones;
        }

        private string FirstLetterToUpperCase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (value.Length == 1)
                return value[0].ToString().ToUpper();

            return value[0].ToString().ToUpper() + value.Substring(1).ToLower();
        }

        private int GetAgeByDOB(string birthDate, string firstLOFDepDate)
        {
            var travelDate = DateTime.Parse(firstLOFDepDate);

            var birthDate1 = DateTime.Parse(birthDate);
            // Calculate the age.
            var age = travelDate.Year - birthDate1.Year;
            // Go back to the year the person was born in case of a leap year
            if (birthDate1 > travelDate.AddYears(-age)) age--;

            return age;
        }

        private string GetPaxDescriptionByDOB(string date, string deptDateFLOF)
        {
            int age = GetAgeByDOB(date, deptDateFLOF);
            if ((18 <= age) && (age <= 64))
            {
                return "Adult (18-64)";
            }
            else
            if ((2 <= age) && (age < 5))
            {
                return "Child (2-4)";
            }
            else
            if ((5 <= age) && (age <= 11))
            {
                return "Child (5-11)";
            }
            else
            //if((12 <= age) && (age <= 17))
            //{

            //}
            if ((12 <= age) && (age <= 14))
            {
                return "Child (12-14)";
            }
            else
            if ((15 <= age) && (age <= 17))
            {
                return "Child (15-17)";
            }
            else
            if (65 <= age)
            {
                return "Senior (65+)";
            }
            else if (age < 2)
                return "Infant (under 2)";

            return string.Empty;
        }

        private string GetPaymentTargetForRegisterFop(FlightReservationResponse flightReservationResponse, string flow, bool isCompleteFarelockPurchase = false)
        {
            var productCodes = new List<string>();
            if (string.IsNullOrEmpty(_configuration.GetValue<string>("EnablePCUSelectedSeatPurchaseViewRes")))
                return string.Empty;

            if (isCompleteFarelockPurchase)
                return "RES";

            if (flightReservationResponse == null || flightReservationResponse.ShoppingCart == null || flightReservationResponse.ShoppingCart.Items == null)
                return string.Empty;

            switch (flow)
            {
                case "BOOKING":
                case "RESHOP":
                    productCodes = flightReservationResponse.ShoppingCart.Items.Select(x => x.Product.FirstOrDefault().Code).ToList();
                    break;

                case "VIEWRES":
                case "CHECKIN":
                    productCodes = flightReservationResponse.ShoppingCart.Items.Where(x => x.Product.FirstOrDefault().Code != "RES").Select(x => x.Product.FirstOrDefault().Code).ToList();
                    break;

                case "POSTBOOKING":
                    productCodes = flightReservationResponse.ShoppingCart.Items.SelectMany(x => x.Product).Where(x => x.Characteristics != null && (x.Characteristics.Any(y => y.Description == "PostPurchase" && Convert.ToBoolean(y.Value) == true))).Select(x => x.Code).ToList();
                    break;
            }
            if (productCodes == null || !productCodes.Any())
                return string.Empty;

            return string.Join(",", productCodes.Distinct());
        }

        private async Task<List<MOBItem>> GetELFLimitationsViewRes(List<MOBSHOPTrip> shopTrip, int appId = 0)
        {
            if (shopTrip.IsNullOrEmpty())
            { return null; }

            var databaseKey = string.Empty;
            bool isIBE = false;
            bool isELF = false;
            shopTrip.ForEach(p =>
            {
                if (!p.IsNullOrEmpty() && !p.FlattenedFlights.IsNullOrEmpty())
                {
                    isELF = p.FlattenedFlights.Any(k => k.IsElf);
                    isIBE = p.FlattenedFlights.Any(k => k.IsIBE);
                }
            });

            if (isELF)
            {
                databaseKey = "SSA_ELF_MR_Limitations";
            }
            else if (isIBE)
            {
                databaseKey = "IBE_MR_Limitations";
            }

            var messages = !string.IsNullOrEmpty(databaseKey) ?
                    await GetMPPINPWDTitleMessages(databaseKey) : null;

            if (!_configuration.GetValue<bool>("DisableRestrictionsForiOS"))
            {
                if (messages != null && appId == 1)
                {
                    try
                    {
                        var footNote = messages.Where(x => x.Id == _configuration.GetValue<string>("RestrictionsLimitationsFootNotes")).FirstOrDefault();
                        if (footNote != null && footNote?.CurrentValue != null)
                        {
                            if (footNote.CurrentValue.StartsWith("<p>"))
                            {
                                footNote.CurrentValue = footNote.CurrentValue.Replace("<p>", "").Replace("</p>", "").Replace("<br/><br/>", "\n\n");
                            }
                        }
                    }
                    catch (Exception ex) { }
                }
            }

            return messages;
        }

        private async Task<List<MOBItem>> GetMPPINPWDTitleMessages(string titleList)
        {
            List<MOBItem> messages = new List<MOBItem>();
            List<United.Definition.MOBLegalDocument> docs = await _legalDocumentsForTitlesService.GetNewLegalDocumentsForTitles(titleList, _headers.ContextValues.SessionId, true).ConfigureAwait(false);
            if (docs != null && docs.Count > 0)
            {
                foreach (var doc in docs)
                {
                    MOBItem item = new MOBItem();
                    item.Id = doc.Title;
                    item.CurrentValue = doc.LegalDocument;
                    messages.Add(item);
                }
            }
            return messages;
        }

        private async Task AssignProfileSavedETCsFromPersist(string sessionId, MOBShoppingCart shoppingCart)
        {
            if (_configuration.GetValue<bool>("SavedETCToggle"))
            {
                var persistShopingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(sessionId, shoppingCart.ObjectName, new List<string> { sessionId, shoppingCart.ObjectName }).ConfigureAwait(false);
                if (persistShopingCart?.ProfileTravelerCertificates != null)
                {
                    shoppingCart.ProfileTravelerCertificates = persistShopingCart.ProfileTravelerCertificates;
                }
            }
        }

        private async Task<List<MOBItem>> GetCaptions(string key)
        {
            return !string.IsNullOrEmpty(key) ? await GetCaptions(key, true) : null;
        }

        private async Task<List<MOBItem>> GetCaptions(string keyList, bool isTnC)
        {
            var docs = await _legalDocumentsForTitlesService.GetNewLegalDocumentsForTitles(keyList, _headers.ContextValues.TransactionId, isTnC).ConfigureAwait(false);
            if (docs == null || !docs.Any()) return null;

            var captions = new List<MOBItem>();

            captions.AddRange(
                docs.Select(doc => new MOBItem
                {
                    Id = doc.Title,
                    CurrentValue = doc.LegalDocument
                }));
            return captions;
        }

        private void IsHidePromoOption(MOBShoppingCart shoppingCart, string flow, MOBApplication application, bool IsReshopChange)
        {
            string[] productCodes;
            switch (flow)
            {
                case "POSTBOOKING":
                    if (IsPostBookingPromoCodeEnabled(application.Id, application.Version.Major))
                    {
                        productCodes = _configuration.GetValue<string>("PostBookingPromocodeEligibleProducts").Split('|');
                        if (shoppingCart.PromoCodeDetails == null)
                        {
                            shoppingCart.PromoCodeDetails = new MOBPromoCodeDetails();
                        }
                        //To know if it is Postbooking after Reshop                   
                        if (IsReshopChange)
                        {
                            shoppingCart.PromoCodeDetails.IsHidePromoOption = true;
                        }
                        else
                        {
                            if (shoppingCart?.Products != null
                                && !shoppingCart.Products.Any(p => productCodes.Contains(p.Code)))
                            {
                                shoppingCart.PromoCodeDetails.IsHidePromoOption = true;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private bool IsPostBookingPromoCodeEnabled(int applicationId, string appVersion)
        {
            if (_configuration.GetValue<bool>("EnableCouponsInPostBooking") && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Android_EnablePromoCodePostBooking_AppVersion"), _configuration.GetValue<string>("iPhone_EnablePromoCodePostBooking_AppVersion")))
            {
                return true;
            }
            return false;
        }

        private List<MOBItem> GetFareLockCaptions(United.Service.Presentation.ReservationModel.Reservation reservation, string journeyType, bool isCorporateFareLock)
        {
            if (reservation.IsNullOrEmpty() || journeyType.IsNullOrEmpty())
                return null;

            var title = GetPaymentTitleCaptionFareLock(reservation, journeyType);
            var tripType = GetSegmentDescriptionPageSubTitle(reservation);

            List<MOBItem> captions = new List<MOBItem>();
            if (!title.IsNullOrEmpty() && !tripType.IsNullOrEmpty())
            {
                captions.Add(GetFareLockViewResPaymentCaptions("PaymentPage_Title", title));
                captions.Add(GetFareLockViewResPaymentCaptions("PaymentPage_SubTitle", tripType));
                captions.Add(GetFareLockViewResPaymentCaptions("PaymentPage_Book24Hr_Policy", "Book without worry"));
                captions.Add(GetFareLockViewResPaymentCaptions("PaymentPage_ProductCode", "FLK_ViewRes"));
            }

            // To find if PNR is corporate travel
            if (isCorporateFareLock)
            {
                var priceText = _configuration.GetValue<string>("CorporateRateText");
                if (!priceText.IsNullOrEmpty())
                {
                    captions.Add(GetFareLockViewResPaymentCaptions("Corporate_PriceBreakDownText", priceText));
                }
            }

            return captions;
        }

        private string GetPaymentTitleCaptionFareLock(Service.Presentation.ReservationModel.Reservation reservation, string journeyType)
        {
            if (reservation.IsNullOrEmpty() || journeyType.IsNullOrEmpty())
                return string.Empty;
            string fareLockHeaderDate = GetFareLockTitleViewResPaymentRTI(reservation, journeyType);
            string fareLockHeaderTitle = GetFareLockTitle(reservation, journeyType);

            if (!fareLockHeaderDate.IsNullOrEmpty() && !fareLockHeaderTitle.IsNullOrEmpty())
            {
                string title = journeyType.Equals("Multicity") ? "Starting" + " " + fareLockHeaderDate + " " + fareLockHeaderTitle
                                                        : fareLockHeaderDate + " " + fareLockHeaderTitle;
                if (journeyType.Equals("Multicity") && title.Length > 26)
                {
                    int endIndex = title.IndexOf("/") + 1;
                    title = title.Substring(0, endIndex) + "....";
                }
                return title;
            }
            return string.Empty;
        }

        private string GetFareLockTitleViewResPaymentRTI(United.Service.Presentation.ReservationModel.Reservation reservation, string journeyType)
        {
            if (reservation.IsNullOrEmpty() || journeyType.IsNullOrEmpty())
                return null;
            string depatureDate = reservation.FlightSegments.FirstOrDefault(k => !k.IsNullOrEmpty() && !k.FlightSegment.IsNullOrEmpty() && !k.FlightSegment.DepartureDateTime.IsNullOrEmpty()).FlightSegment.DepartureDateTime;
            string arrivalDate = reservation.FlightSegments.LastOrDefault(k => !k.IsNullOrEmpty() && !k.FlightSegment.IsNullOrEmpty() && !k.FlightSegment.ArrivalDateTime.IsNullOrEmpty()).FlightSegment.ArrivalDateTime;
            string getTitle = string.Empty;
            if (!depatureDate.IsNullOrEmpty() && !arrivalDate.IsNullOrEmpty())
            {
                DateTime depatureDateObj;
                DateTime arrivalDateObj;
                DateTimeFormatInfo getMonth = new DateTimeFormatInfo();

                DateTime.TryParse(depatureDate, out depatureDateObj);
                string departMonth = !depatureDateObj.IsNullOrEmpty() ? getMonth.GetMonthName(depatureDateObj.Month).Substring(0, 3) : string.Empty;

                if (journeyType.Equals("Roundtrip"))
                {
                    DateTime.TryParse(arrivalDate, out arrivalDateObj);
                    string arrivalMonth = !arrivalDateObj.IsNullOrEmpty() ? getMonth.GetMonthName(arrivalDateObj.Month).Substring(0, 3) : string.Empty;
                    if (departMonth.Equals(arrivalMonth, StringComparison.InvariantCultureIgnoreCase) && depatureDateObj.Day.Equals(arrivalDateObj.Day))
                    {
                        getTitle = departMonth + " " + depatureDateObj.Day.ToString();
                    }
                    else if (departMonth.Equals(arrivalMonth, StringComparison.InvariantCultureIgnoreCase))
                    {
                        getTitle = departMonth + " " + depatureDateObj.Day.ToString() + " - " + arrivalDateObj.Day.ToString();
                    }
                    else
                    {
                        getTitle = departMonth + " " + depatureDateObj.Day.ToString() + " - " + arrivalMonth + " " + arrivalDateObj.Day.ToString();
                    }
                }
                else
                {
                    getTitle = departMonth + " " + depatureDateObj.Day.ToString();
                }

                return getTitle;
            }
            return getTitle;
        }

        private string GetFareLockTitle(United.Service.Presentation.ReservationModel.Reservation reservation, string journeyType)
        {
            if (reservation.IsNullOrEmpty() || journeyType.IsNullOrEmpty())
                return string.Empty;

            string tripBuild = string.Empty;
            var arrivalAirportCode = reservation.FlightSegments.LastOrDefault(k => !k.IsNullOrEmpty() && !k.FlightSegment.IsNullOrEmpty()).FlightSegment;
            reservation.FlightSegments.ForEach(p =>
            {
                if (p != null && !p.FlightSegment.IsNullOrEmpty() && !p.FlightSegment.DepartureAirport.IsNullOrEmpty() && !p.FlightSegment.ArrivalAirport.IsNullOrEmpty())
                {
                    if (journeyType.Equals("Multicity"))
                    {
                        tripBuild = tripBuild.IsNullOrEmpty() ? p.FlightSegment.DepartureAirport.IATACode + " to " + p.FlightSegment.ArrivalAirport.IATACode
                                                              : tripBuild + "/" + p.FlightSegment.DepartureAirport.IATACode + " to " + p.FlightSegment.ArrivalAirport.IATACode;
                    }
                    else if (journeyType.Equals("Roundtrip"))
                    {
                        if (tripBuild.IsNullOrEmpty())
                        {
                            tripBuild = p.FlightSegment.DepartureAirport.IATACode + " - " + p.FlightSegment.ArrivalAirport.IATACode;
                        }
                    }
                    else
                    {
                        if (tripBuild.IsNullOrEmpty())
                        {
                            string arrivalAirport = !arrivalAirportCode.IsNullOrEmpty() && !arrivalAirportCode.ArrivalAirport.IsNullOrEmpty() ? arrivalAirportCode.ArrivalAirport.IATACode : p.FlightSegment.ArrivalAirport.IATACode;
                            tripBuild = p.FlightSegment.DepartureAirport.IATACode + " - " + arrivalAirport;
                        }
                    }
                }
            });
            return tripBuild;
        }

        private string GetSegmentDescriptionPageSubTitle(United.Service.Presentation.ReservationModel.Reservation reservation)
        {
            if (reservation != null)
            {
                var traveler = reservation.Travelers.Count.ToString() + (reservation.Travelers.Count() > 1 ? " travelers" : " traveler");
                var JourneyType = GetJourneyTypeDescription(reservation);

                return !JourneyType.IsNullOrEmpty() ? JourneyType + ", " + traveler : string.Empty;
            }
            return string.Empty;
        }

        private MOBItem GetFareLockViewResPaymentCaptions(string id, string currentValue)
        {
            if (id.IsNullOrEmpty() || currentValue.IsNullOrEmpty())
                return null;

            var captions = new MOBItem()
            {
                Id = id,
                CurrentValue = currentValue
            };
            return captions;
        }

        private bool IsCorporateTraveler(Collection<Characteristic> characteristics)
        {
            if (!characteristics.IsNullOrEmpty())
            {
                return characteristics.Any(c => !c.Code.IsNullOrEmpty() && c.Code.Equals("IsValidCorporateTravel", StringComparison.OrdinalIgnoreCase) &&
                                          !c.Value.IsNullOrEmpty() && c.Value.Equals("True"));
            }
            return false;
        }


        private string GetJourneyTypeDescription(United.Service.Presentation.ReservationModel.Reservation reservation)
        {
            if (reservation != null)
            {
                var JourneyType = reservation.Type.FirstOrDefault(o => (o.Description != null && o.Key != null && o.Description.Equals("JOURNEY_TYPE", StringComparison.OrdinalIgnoreCase)));
                return JourneyType.IsNullOrEmpty() ? GetTravelType(reservation.FlightSegments) : GetTravelType(JourneyType.Key);
            }
            return string.Empty;
        }

        private string GetTravelType(string JourneyType)
        {
            string Type = string.Empty;

            if (!string.IsNullOrEmpty(JourneyType))
            {
                switch (JourneyType.ToLower())
                {
                    case "one_way":
                        Type = "Oneway";
                        break;
                    case "round_trip":
                        Type = "Roundtrip";
                        break;
                    case "multi_city":
                        Type = "Multicity";
                        break;
                }
            }
            return Type;
        }

        private string GetTravelType(Collection<ReservationFlightSegment> FlightSegments)
        {
            var journeytype = string.Empty;

            if (FlightSegments != null && FlightSegments.Any(p => p != null))
            {

                var maxTripNumber = FlightSegments.Max(tq => tq.TripNumber);
                var minTripNumber = FlightSegments.Min(f => f.TripNumber);

                if (maxTripNumber.ToInteger() == 1)
                {
                    journeytype = "Oneway";
                }

                if (maxTripNumber.ToInteger() == 2)
                {

                    var firstTripDepartureAirportCode = FlightSegments.Where(t => t.TripNumber == minTripNumber.ToString()).Select(t => t.FlightSegment.DepartureAirport.IATACode).FirstOrDefault();
                    var firstTripArrivalAirportCode = FlightSegments.Where(t => t.TripNumber == minTripNumber.ToString()).Select(t => t.FlightSegment.ArrivalAirport.IATACode).LastOrDefault();
                    var lastTripArrivalAirportCode = FlightSegments.Where(f => f.TripNumber == maxTripNumber.ToString()).Select(t => t.FlightSegment.ArrivalAirport.IATACode).LastOrDefault();
                    var lastTripDepartureAirportCode = FlightSegments.Where(f => f.TripNumber == maxTripNumber.ToString()).Select(t => t.FlightSegment.DepartureAirport.IATACode).FirstOrDefault();

                    if (firstTripDepartureAirportCode == lastTripArrivalAirportCode && firstTripArrivalAirportCode == lastTripDepartureAirportCode)
                    {
                        journeytype = "Roundtrip";
                    }
                    else
                    {
                        journeytype = "Multicity";
                    }

                }
                if (maxTripNumber.ToInteger() > 2)
                {
                    journeytype = "Multicity";
                }
            }


            return journeytype;
        }

        private United.Service.Presentation.ProductResponseModel.ProductOffer BuildProductOffersforOTP(MOBRegisterOfferRequest request, United.Service.Presentation.ProductResponseModel.ProductOffer productOffer, MerchandizingOfferDetails product)
        {
            var otpOffer = new Service.Presentation.ProductResponseModel.ProductOffer();
            otpOffer.Offers = new Collection<Service.Presentation.ProductResponseModel.Offer>();
            Service.Presentation.ProductResponseModel.Offer offer = new Service.Presentation.ProductResponseModel.Offer();
            offer.ProductInformation = new Service.Presentation.ProductResponseModel.ProductInformation();
            offer.ProductInformation.ProductDetails = new Collection<Service.Presentation.ProductResponseModel.ProductDetail>();
            offer.ProductInformation.ProductDetails.Add(productOffer.Offers[0].ProductInformation.ProductDetails.Where(x => x.Product.Code.Equals(product.ProductCode)).FirstOrDefault().Clone());
            otpOffer.Offers.Add(offer);
            otpOffer.Travelers = new Collection<United.Service.Presentation.ProductModel.ProductTraveler>();
            otpOffer.Travelers = productOffer.Travelers;
            otpOffer.Solutions = new Collection<United.Service.Presentation.ProductRequestModel.Solution>();
            otpOffer.Solutions = productOffer.Solutions;
            otpOffer.Response = new Service.Presentation.CommonModel.ServiceResponse();
            otpOffer.Response = productOffer.Response;
            for (int i = 1; i < product.NumberOfPasses; i++)
            {
                United.Service.Presentation.ProductModel.SubProduct Item = otpOffer.Offers.FirstOrDefault().ProductInformation.ProductDetails.FirstOrDefault().Product.SubProducts[0].Clone();
                Item.ID = (i + 1).ToString();
                Item.Prices.FirstOrDefault().ID = (i + 1).ToString();
                otpOffer.Offers.FirstOrDefault().ProductInformation.ProductDetails.FirstOrDefault().Product.SubProducts.Add(Item);
            }

            return otpOffer;
        }
        public MOBRegisterOfferRequest BuildRegisterOffersRequest(MOBSHOPUnfinishedBookingRequestBase request, FlightReservationResponse flightReservationResponse)
        {
            MOBRegisterOfferRequest registerOfferRequest = new MOBRegisterOfferRequest();
            registerOfferRequest.Application = request.Application;
            registerOfferRequest.DeviceId = request.DeviceId;
            registerOfferRequest.SessionId = request.SessionId;
            registerOfferRequest.TransactionId = request.TransactionId;
            registerOfferRequest.CartId = request.CartId;
            registerOfferRequest.IsOmniCartSavedTripFlow = true;
            registerOfferRequest.CartKey = "ProductBundles";
            registerOfferRequest.Flow = request.Flow;
            registerOfferRequest.MerchandizingOfferDetails = new Collection<MerchandizingOfferDetails>();
            BuildMerchandizingOfferDetails(registerOfferRequest.SessionId, registerOfferRequest.CartId, registerOfferRequest, flightReservationResponse);
            return registerOfferRequest;
        }

        private void BuildMerchandizingOfferDetails(string sessionId, string cartid, MOBRegisterOfferRequest registerOfferRequest, FlightReservationResponse flightReservationResponse)
        {
            var enableSFFTravelOption = System.Threading.Tasks.Task.Run(async () =>
            {
                return await _featureSettings.GetFeatureSettingValue("EnableFixForSFFTravelOption_MOBILE39209").ConfigureAwait(false);
            }).Result;

            if (flightReservationResponse?.DisplayCart?.TravelOptions != null)
            {
                var travelOptionsBundles = flightReservationResponse?.DisplayCart?.TravelOptions.Where(to => to.Type == "BE" || (enableSFFTravelOption && to.Type == "SustainableAviationFuelContribution"));
                if (travelOptionsBundles != null)
                {
                    foreach (var bundle in travelOptionsBundles)
                    {
                        MerchandizingOfferDetails offerDetails = new MerchandizingOfferDetails();
                        offerDetails.ProductCode = bundle.Key;
                        offerDetails.TripIds = bundle.SubItems.Select(si => si.TripIndex).Distinct().ToList();
                        offerDetails.ProductIds = bundle.SubItems.Select(si => si.Key).ToList();
                        registerOfferRequest.MerchandizingOfferDetails.Add(offerDetails);
                    }
                }
            }

        }

        private Collection<United.Services.FlightShopping.Common.FlightReservation.RegisterOfferRequest> BuildRegisterOffersRequest(MOBRegisterOfferRequest request, United.Service.Presentation.ProductResponseModel.ProductOffer productOffer, United.Service.Presentation.ProductResponseModel.ProductOffer productVendorOffer, United.Service.Presentation.ReservationResponseModel.ReservationDetail reservation, United.Service.Presentation.ProductResponseModel.ProductOffer productFareLockOffer)
        {
            var reservationDetail = new ReservationDetail();
            var selectedOffer = new Service.Presentation.ProductResponseModel.ProductOffer();
            var registerOfferRequests = new Collection<United.Services.FlightShopping.Common.FlightReservation.RegisterOfferRequest>();
            int reservationCtr = 0;
            United.Services.FlightShopping.Common.FlightReservation.RegisterOfferRequest registerOfferRequest = null;
            Service.Presentation.ProductResponseModel.ProductOffer selOffer = null;
            foreach (var merchandizingOfferDetail in request.MerchandizingOfferDetails)
            {
                if (merchandizingOfferDetail.ProductCode.Equals("TPI"))
                    selectedOffer = productVendorOffer;
                else if (merchandizingOfferDetail.ProductCode.Equals("FLK") && !merchandizingOfferDetail.IsOfferRegistered)
                    selectedOffer = productFareLockOffer;
                else if (merchandizingOfferDetail.ProductCode.Equals("OTP"))
                {
                    selectedOffer = BuildProductOffersforOTP(request, productOffer, merchandizingOfferDetail);
                }
                else if (merchandizingOfferDetail.ProductCode.Equals("FLK") && merchandizingOfferDetail.IsOfferRegistered)
                    selectedOffer = null;
                else if (!_configuration.GetValue<bool>("DisableRemoveAncillaryOfferFixForEFS") && merchandizingOfferDetail.ProductCode.Equals("EFS") && merchandizingOfferDetail.IsOfferRegistered)
                    selectedOffer = null;
                else
                    selectedOffer = productOffer;

                if (selectedOffer != null)
                {
                    if (!(selectedOffer.Offers.FirstOrDefault().ProductInformation.ProductDetails.Any(x => x.Product.Code.Equals(merchandizingOfferDetail.ProductCode)))
                    || selectedOffer.Offers.FirstOrDefault().ProductInformation.ProductDetails.Where(x => x.Product.Code.Equals(merchandizingOfferDetail.ProductCode)).Select(c => c.Product.SubProducts.All(x => x.Prices.Count() == 0 || x.Prices == null)).FirstOrDefault())
                        continue;
                    else
                    {
                        selOffer = null;
                        if (!(merchandizingOfferDetail.IsOfferRegistered))
                        {
                            selOffer = new Service.Presentation.ProductResponseModel.ProductOffer();
                            selOffer.Offers = new Collection<Service.Presentation.ProductResponseModel.Offer>();
                            Service.Presentation.ProductResponseModel.Offer offer = new Service.Presentation.ProductResponseModel.Offer();
                            offer.ProductInformation = new Service.Presentation.ProductResponseModel.ProductInformation();
                            offer.ProductInformation.ProductDetails = new Collection<Service.Presentation.ProductResponseModel.ProductDetail>();
                            offer.ProductInformation.ProductDetails.Add(selectedOffer.Offers[0].ProductInformation.ProductDetails.Where(x => x.Product.Code.Equals(merchandizingOfferDetail.ProductCode)).FirstOrDefault());
                            selOffer.Offers.Add(offer);
                            selOffer.Travelers = selectedOffer.Travelers;
                            selOffer.Solutions = selectedOffer.Solutions;
                            selOffer.Response = selectedOffer.Response;
                            selOffer.FlightSegments = selectedOffer.FlightSegments;
                            selOffer.Requester = selectedOffer.Requester;
                            if (IsEnableOmniCartReleaseCandidateTwoChanges_Bundles(request.Application.Id, request.Application.Version.Major))
                            {
                                selOffer.ODOptions = selectedOffer.ODOptions;
                                selOffer.Characteristics = selectedOffer.Characteristics;
                            }
                        }

                        registerOfferRequest = null;
                        if (merchandizingOfferDetail.IsOfferRegistered || reservationCtr != 0)
                            reservationDetail = null;
                        else
                        {
                            reservationDetail = reservation;
                            reservationCtr++;
                        }
                    }
                }

                registerOfferRequest = GetRegisterOffersRequest(request, merchandizingOfferDetail, selOffer, reservationDetail, null);
                registerOfferRequests.Add(registerOfferRequest);
            }

            _logger.LogInformation("BuildRegisterOffersRequest RegisterOfferRequests{registerOfferRequests} SessionId:{sessionId}", JsonConvert.SerializeObject(registerOfferRequests), request.SessionId);

            return registerOfferRequests;
        }

        private United.Services.FlightShopping.Common.FlightReservation.RegisterOfferRequest GetRegisterOffersRequest
         (MOBRegisterOfferRequest request, MerchandizingOfferDetails merchandizingOfferDetail,
         United.Service.Presentation.ProductResponseModel.ProductOffer Offer,
            United.Service.Presentation.ReservationResponseModel.ReservationDetail reservation,
          System.Collections.ObjectModel.Collection<Characteristic> characteristics = null)
        {
            United.Services.FlightShopping.Common.FlightReservation.RegisterOfferRequest registerOfferRequest = new United.Services.FlightShopping.Common.FlightReservation.RegisterOfferRequest();


            registerOfferRequest.AutoTicket = false;
            registerOfferRequest.CartId = request.CartId;
            registerOfferRequest.CartKey = request.CartKey;
            registerOfferRequest.CountryCode = (!string.IsNullOrEmpty(request.PointOfSale)) ? request.PointOfSale : "US";
            registerOfferRequest.Delete = merchandizingOfferDetail.IsOfferRegistered;
            registerOfferRequest.LangCode = (!string.IsNullOrEmpty(request.LanguageCode)) ? request.LanguageCode : "en-US";
            registerOfferRequest.Offer = (Offer == null ? null : Offer);
            registerOfferRequest.Characteristics = characteristics;
            registerOfferRequest.ProductCode = merchandizingOfferDetail.ProductCode;
            registerOfferRequest.ProductIds = merchandizingOfferDetail.ProductIds;
            registerOfferRequest.SubProductCode = merchandizingOfferDetail.SubProductCode;
            registerOfferRequest.Reservation = (request.Flow == "RESHOP" || request.Flow == "POSTBOOKING") ? null : (reservation == null ? null : reservation.Detail);
            registerOfferRequest.Products = new List<ProductRequest>()
            {
                new ProductRequest () { Code = merchandizingOfferDetail.ProductCode, Ids = merchandizingOfferDetail.ProductIds }
            };
            registerOfferRequest.GUID = !IsCheckinFlow(request.Flow) ? null : new UniqueIdentifier() { ID = request.CheckinSessionId, Type = "eToken" };
            registerOfferRequest.WorkFlowType = ConfigUtility.GetWorkFlowType(request.Flow);
            if (_configuration.GetValue<bool>("EnableOmniChannelCartMVP1"))
            {
                registerOfferRequest.DeviceID = request.DeviceId;
            }

            return registerOfferRequest;
        }

        private async Task<FlightReservationResponse> RegisterOffers(MOBRegisterOfferRequest request, United.Service.Presentation.ProductResponseModel.ProductOffer productOffer, United.Service.Presentation.ProductResponseModel.ProductOffer productVendorOffer, United.Service.Presentation.ReservationResponseModel.ReservationDetail reservationDetail, Session session, United.Service.Presentation.ProductResponseModel.ProductOffer productFareLockOffer)
        {
            var response = new FlightReservationResponse();


            if (((request.Flow != "BOOKING" && request.Flow != "POSTBOOKING") && !((productOffer != null || productVendorOffer != null) && reservationDetail != null)) || (request.Flow == "BOOKING" && !(productFareLockOffer != null) && (request.MerchandizingOfferDetails.FirstOrDefault().ProductCode == "FLK")) || (request.Flow == "POSTBOOKING" && productOffer == null))
            {
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }
            var registerOfferRequests = BuildRegisterOffersRequest(request, productOffer, productVendorOffer, reservationDetail, productFareLockOffer);
            if (registerOfferRequests.Count > 0)
            {
                string jsonRequest = JsonConvert.SerializeObject(registerOfferRequests);
                string url = "RegisterOffers";

                if (session == null)
                {
                    throw new MOBUnitedException("Could not find your session.");
                }
                response = await _shoppingCartService.RegisterOffers<FlightReservationResponse>(session.Token, url, jsonRequest, request.SessionId).ConfigureAwait(false);


                if (response != null)
                {
                    if (!(response != null && response.Status.Equals(United.Services.FlightShopping.Common.StatusType.Success) && response.DisplayCart != null))
                    {
                        if (response.Errors != null && response.Errors.Count > 0)
                        {
                            string errorMessage = string.Empty;
                            foreach (var error in response.Errors)
                            {
                                errorMessage = errorMessage + " " + error.Message;
                            }

                            throw new System.Exception(errorMessage);
                        }
                    }
                }
                else
                {
                    throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
            }
            else
            {
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }

            return response;
        }

        private string TicketingCountryCode(Service.Presentation.CommonModel.PointOfSale pointOfSale)
        {
            return pointOfSale != null && pointOfSale.Country != null ? pointOfSale.Country.CountryCode : string.Empty;
        }

        private async Task<FlightReservationResponse> RegisterFareLockReservation(MOBRegisterOfferRequest request, ReservationDetail reservationDetail, Session session)
        {
            var response = new FlightReservationResponse();
            string actionName = "RegisterFareLockReservation";

            if (session == null)
            {
                throw new MOBUnitedException("Could not find your session.");
            }

            if (reservationDetail != null && reservationDetail.Detail != null)
            {
                var registerOfferRequest = new United.Services.FlightShopping.Common.FlightReservation.RegisterOfferRequest();
                registerOfferRequest.CartId = request.CartId;
                registerOfferRequest.CountryCode = TicketingCountryCode(reservationDetail.Detail.PointOfSale);
                registerOfferRequest.LangCode = request.LanguageCode;
                registerOfferRequest.Reservation = reservationDetail.Detail;

                var jsonRequest = JsonConvert.SerializeObject(registerOfferRequest);
                if (!string.IsNullOrEmpty(jsonRequest))
                {
                    string cslResponse = await _shoppingCartService.RegisterFareLockReservation(session.Token, actionName, jsonRequest, session.SessionId).ConfigureAwait(false);

                    if (!string.IsNullOrEmpty(cslResponse))
                    {
                        response = JsonConvert.DeserializeObject<FlightReservationResponse>(cslResponse);
                        if (!(response != null && response.Status.Equals(United.Services.FlightShopping.Common.StatusType.Success) && response.DisplayCart != null))
                        {
                            if (response.Errors != null && response.Errors.Count > 0)
                            {
                                string errorMessage = string.Empty;
                                foreach (var error in response.Errors)
                                {
                                    errorMessage = errorMessage + " " + error.Message;
                                }

                                throw new System.Exception(errorMessage);
                            }
                        }
                    }
                    else
                    {
                        throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                    }
                }
            }
            else
            {
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }
            return response;
        }

        private async Task BuildRegisteredBundleOfferRequest(MOBShoppingCart persistedShoppingCart, MOBRegisterOfferRequest request)
        {
            if (persistedShoppingCart == null)
            {
                persistedShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, persistedShoppingCart.ObjectName, new List<string> { request.SessionId, persistedShoppingCart.ObjectName }).ConfigureAwait(false);
            }
            if (persistedShoppingCart.Products.Count() > 0 && persistedShoppingCart.Products.Where(x => x.ProdDescription == "BE").Count() > 0)
            {
                var previousRegisteredBundlesinShoppingCart = persistedShoppingCart.Products.Where(x => x.ProdDescription == "BE").ToList();
                List<MerchandizingOfferDetails> unRegisteredBundleRequest = new List<MerchandizingOfferDetails>();
                if (request.MerchandizingOfferDetails != null)
                {

                    foreach (var offerRequest in request.MerchandizingOfferDetails.ToList().Where(mo => mo.IsOfferRegistered == false))
                    {
                        foreach (var tripId in offerRequest.TripIds)
                        {
                            previousRegisteredBundlesinShoppingCart.ForEach(product =>
                            {
                                product.Segments.ForEach(segment =>
                                {
                                    if (segment.TripId.Contains(tripId))
                                    {
                                        unRegisteredBundleRequest.Add(new MerchandizingOfferDetails
                                        {
                                            ProductCode = product.Code,
                                            ProductIds = segment.ProductIds.ToList(),
                                            IsOfferRegistered = true,
                                            TripIds = segment.TripId.Split(',').ToList(),
                                        });
                                    }
                                });
                            });
                        }
                    }
                    unRegisteredBundleRequest.AddRange(request.MerchandizingOfferDetails);
                    request.MerchandizingOfferDetails = new Collection<MerchandizingOfferDetails>(unRegisteredBundleRequest);
                }
            }
        }

        private async Task<bool> ValidateBundleSelectedWhenCouponIsApplied(MOBRegisterOfferRequest request, MOBShoppingCart persistedShoppingCart, United.Persist.Definition.Shopping.Reservation reservation)
        {
            if (persistedShoppingCart != null && request != null)
            {
                if (persistedShoppingCart?.Products != null
                    && persistedShoppingCart?.PromoCodeDetails?.PromoCodes != null
                    && persistedShoppingCart.PromoCodeDetails.PromoCodes.Count > 0)
                {
                    var bundleResponse = await _sessionHelperService.GetSession<MOBBookingBundlesResponse>(request.SessionId, typeof(MOBBookingBundlesResponse).FullName, new List<string> { request.SessionId, typeof(MOBBookingBundlesResponse).FullName }).ConfigureAwait(false);
                    if (bundleResponse != null && bundleResponse?.Products != null)
                    {
                        var couponProductCode = persistedShoppingCart.PromoCodeDetails.PromoCodes.First()?.Product;
                        var selectedBundleCodes = request.CartKey == "ProductBundles" ? request.MerchandizingOfferDetails?.Select(x => x.ProductCode) : null;
                        bool IsEnableBundlesAlertMessageFixWhenAFSCouponApplied = EnableBundlesAlertMessageFixWhenAFSCouponApplied(request.Application.Id, request.Application?.Version?.Major);
                        if (IsEnableBundlesAlertMessageFixWhenAFSCouponApplied && request.IsContinue)
                        {
                            if (reservation != null && reservation.TravelOptions != null)
                            {
                                var bundleCodes = new List<string>();
                                var bundleCode = reservation.TravelOptions.Where(x => x != null && !string.IsNullOrEmpty(x.Key) && x.Key == "BUNDLES")?.ToList();
                                if (bundleCode != null && bundleCode.Count > 0)
                                {
                                    bundleCode.ForEach(x =>
                                    {
                                        if (!string.IsNullOrEmpty(x.Code))
                                            bundleCodes.Add(x.Code);
                                    });
                                }
                                selectedBundleCodes = bundleCodes.Count > 0 ? bundleCodes.AsEnumerable() : null;
                            }
                        }
                        else
                        {
                            selectedBundleCodes = IsEnableBundlesAlertMessageFixWhenAFSCouponApplied ? null : selectedBundleCodes;
                        }
                        if (selectedBundleCodes != null)
                        {
                            var selectedBundleProducts = bundleResponse.Products.Where(x => selectedBundleCodes.Contains(x.ProductCode));
                            if (selectedBundleProducts != null)
                            {
                                foreach (var bundle in selectedBundleProducts)
                                {
                                    if (bundle.BundleProductCodes != null && bundle.BundleProductCodes.Count > 0)
                                    {
                                        if (bundle.BundleProductCodes.Contains(couponProductCode))
                                            return true;
                                        else if (couponProductCode == "BAG" && bundle.BundleProductCodes.Contains("EXB"))
                                            return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        private async Task UpdateSeatsAfterBundleChanged(string sessionId, int appId, string appVersion, string deviceId, List<int> tripIndices)
        {
            #region Get Involved Segments

            if (tripIndices == null || !tripIndices.Any())
                return;

            var persistedReservation = new Reservation();
            persistedReservation = await _sessionHelperService.GetSession<Reservation>(sessionId, persistedReservation.ObjectName, new List<string> { sessionId, persistedReservation.ObjectName }).ConfigureAwait(false);
            if (persistedReservation == null || persistedReservation.Trips == null || !persistedReservation.Trips.Any() || persistedReservation.TravelersCSL == null || !persistedReservation.TravelersCSL.Values.Any())
                return;

            // Get the segments that have bundle changes from the reservation object
            var segmentList = persistedReservation.Trips
                                .SelectMany(t => t.FlattenedFlights).Where(flattenedFlight => flattenedFlight != null)
                                .SelectMany(ff => ff.Flights).Where(flight => flight != null)
                                .Where(flight => tripIndices.Contains(flight.TripIndex)).ToList();

            if (!segmentList.Any()) return;

            #endregion

            #region Update Reservation Object

            var needUpdate = false; // only do update if this flag is on 

            var seatMapTasks = segmentList.Select(async segment =>
            {
                #region per segment
                //get the preselected seats
                var preselectedSeats = persistedReservation.TravelersCSL.Values.SelectMany(t => t.Seats).Where(s => !string.IsNullOrEmpty(s.SeatAssignment) && s.Origin == segment.Origin && s.Destination == segment.Destination).ToList();
                if (!preselectedSeats.Any()) return;
                //get seatmap        
                List<MOBSeatMap> seatMap = null;
                try
                {
                    if (_configuration.GetValue<bool>("EnableCSL30BookingReshopSelectSeatMap"))
                    {
                        string flow = (persistedReservation.IsReshopChange)
                        ? Convert.ToString(FlowType.RESHOP) : Convert.ToString(FlowType.BOOKING);
                        seatMap = await _seatMapCSL30Helper.GetCSL30SeatMapDetail
                        (flow, sessionId, segment.Destination, segment.Origin, appId, appVersion, deviceId, true);

                    }
                    else
                    {
                        seatMap = await _seatMapCSL30Helper.GetSeatMapDetail(sessionId, segment.Destination, segment.Origin, appId, appVersion, deviceId, true);
                    }
                }
                catch
                {
                    //ignore 
                }
                //clear out the seats if seatMap failed -- no need to notify as per BA
                if (seatMap == null || !seatMap.Any())
                {
                    needUpdate = true;
                    foreach (var preselectedSeat in preselectedSeats)
                    {
                        preselectedSeat.SeatAssignment = "";
                        preselectedSeat.Price = 0;
                        preselectedSeat.PriceAfterTravelerCompanionRules = 0;
                    }
                    return;
                }

                //rePrice the preSelected seats
                foreach (var cabin in seatMap[0].Cabins.Where(c => c != null))
                {
                    foreach (var row in cabin.Rows.Where(r => r != null))
                    {
                        foreach (var seat in row.Seats.Where(s => s != null))
                        {
                            foreach (var preselectedSeat in preselectedSeats.Where(s => s.SeatAssignment.Equals(seat.Number, StringComparison.OrdinalIgnoreCase)))
                            {
                                needUpdate = true;
                                if (seat.ServicesAndFees == null || !seat.ServicesAndFees.Any()) continue;

                                preselectedSeat.Price = seat.ServicesAndFees[0].TotalFee;
                                preselectedSeat.PriceAfterTravelerCompanionRules = seat.ServicesAndFees[0].TotalFee;
                            }
                        }
                    }
                }
                #endregion
            });
            await Task.WhenAll(seatMapTasks);
            if (!needUpdate)
                return;

            #endregion

            #region Update Seats Info

           await _sessionHelperService.SaveSession<Reservation>(persistedReservation, sessionId, new List<string> { sessionId, persistedReservation.ObjectName }, persistedReservation.ObjectName).ConfigureAwait(false);

            var completeSeatsRequest = new MOBRegisterSeatsRequest
            {
                SessionId = sessionId,
                Application = new MOBApplication
                {
                    Id = appId,
                    Version = new MOBVersion
                    {
                        Major = appVersion,
                        Minor = appVersion
                    }
                },
                DeviceId = deviceId,
                Flow = FlowType.BOOKING.ToString()
            };
            Session session = new Session();
            session = await _sessionHelperService.GetSession<Session>(sessionId, session.ObjectName, new List<string> { sessionId, session.ObjectName }).ConfigureAwait(false); ;
            if (_shoppingCartUtility.IsEnableOmniCartMVP2Changes(appId, appVersion, persistedReservation?.ShopReservationInfo2?.IsDisplayCart == true))
            {
                await CompleteSeatsV2(completeSeatsRequest, session, updateSeatsAfterBundlesChanged: true);
            }
            else
            {
                await CompleteSeats(completeSeatsRequest, session, updateSeatsAfterBundlesChanged: true);
            }

            #endregion
        }

        private bool EnableTravelerTypes(int appId, string appVersion, bool reshop = false)
        {
            if (!string.IsNullOrEmpty(appVersion) && appId != -1)
            {
                return _configuration.GetValue<bool>("EnableTravelerTypes") && !reshop
               && GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidTravelerTypesVersion", "iPhoneTravelerTypesVersion", "", "", true, _configuration);
            }
            return false;
        }

        private bool EnableBundlesAlertMessageFixWhenAFSCouponApplied(int appId, string appVersion)
        {
            if (!string.IsNullOrEmpty(appVersion) && appId != -1)
            {
                return _configuration.GetValue<bool>("IsEnableBundlesAlertMessageFixWhenAFSCouponApplied")
               && GeneralHelper.IsApplicationVersionGreaterorEqual(appId, appVersion, _configuration.GetValue<string>("AndroidEnableBundlesAlertMessageFixWhenAFSCouponApplied"), _configuration.GetValue<string>("iPhoneEnableBundlesAlertMessageFixWhenAFSCouponApplied"));
            }
            return false;
        }

        private async Task<(List<MOBSeatPrice> response, List<MOBSeat> lstSeats)> GetFinalTravelerSeatPrices(MOBRequest mobRequest, string languageCode, List<MOBSeat> seats, string sessionID)
        {
            Reservation persistedReservation = await _sessionHelperService.GetSession<Reservation>(sessionID, new Reservation().ObjectName, new List<string> { sessionID, new Reservation().ObjectName }).ConfigureAwait(false);
            List<MOBSeat> lstSeats = null;

            List<SeatDetail> objSeatEngineSeatInfoList = new List<SeatDetail>();
            List<MOBSeatPrice> seatPrices = new List<MOBSeatPrice>();
            List<MOBSeatPrice> tempSeatPrices = new List<MOBSeatPrice>();
            United.Service.Presentation.FlightRequestModel.SeatMap request = new Service.Presentation.FlightRequestModel.SeatMap();

            foreach (MOBSHOPTrip objTrip in persistedReservation.Trips)
            {
                #region
                foreach (MOBSHOPFlattenedFlight objSegment in objTrip.FlattenedFlights)
                {
                    foreach (MOBSHOPFlight segment in objSegment.Flights)
                    {
                        IEnumerable<MOBSeat> seatsList = from s in seats
                                                         where s.Origin.ToUpper().Trim() == segment.Origin.ToUpper().Trim() &&
                                                         s.Destination.ToUpper().Trim() == segment.Destination.ToUpper().Trim()
                                                         select s;

                        #region
                        if (seatsList.Count() > 0)
                        {

                            List<MOBSeat> _seatsList = new List<MOBSeat>();
                            _seatsList = seatsList.ToList();
                            request.Travelers = new Collection<ProductTraveler>();


                            #region
                            if (persistedReservation.TravelersCSL != null && persistedReservation.TravelersCSL.Count > 0)
                            {
                                int i = 0;
                                //foreach (BookingTravelerInfo bookingTravelerInfo in bookingTravelerInfoList)
                                foreach (string travelerKey in persistedReservation.TravelerKeys)
                                {
                                    MOBCPTraveler bookingTravelerInfo = persistedReservation.TravelersCSL[travelerKey];

                                    if (EnableTravelerTypes(mobRequest.Application.Id, mobRequest.Application.Version.Major, persistedReservation.IsReshopChange) &&
                                        persistedReservation.ShopReservationInfo2 != null && persistedReservation.ShopReservationInfo2.TravelerTypes != null && persistedReservation.ShopReservationInfo2.TravelerTypes.Count > 0)
                                    {
                                        if (bookingTravelerInfo.TravelerTypeCode.ToUpper().Equals("INF"))
                                        {
                                            i++;
                                            continue;
                                        }
                                    }

                                    #region
                                    ProductTraveler travelerInformation = new ProductTraveler();
                                    travelerInformation.GivenName = bookingTravelerInfo.FirstName;
                                    travelerInformation.Surname = bookingTravelerInfo.LastName;
                                    // travelerInformation.Index = Convert.ToUInt16(i);
                                    //travelerInformation.TravelerNumber = i + 1;
                                    travelerInformation.TravelerNameIndex = (i + 1).ToString() + ".1";
                                    travelerInformation.Seats = new Collection<SeatDetail>();
                                    SeatDetail seatDetail = new SeatDetail();
                                    seatDetail.DepartureAirport = new Service.Presentation.CommonModel.AirportModel.Airport();
                                    seatDetail.DepartureAirport.IATACode = segment.Origin;
                                    seatDetail.ArrivalAirport = new Service.Presentation.CommonModel.AirportModel.Airport();
                                    seatDetail.ArrivalAirport.IATACode = segment.Destination;
                                    seatDetail.FlightNumber = segment.FlightNumber;
                                    //seatDetail.DepartureDate = Convert.ToDateTime(segment.DepartDate).ToString("yyyy-MM-dd");
                                    seatDetail.DepartureDate = segment.DepartureDateTime;
                                    //travelerInformation.Seats[0].TravelerSharesNameIndex = (i + 1).ToString() + ".1";
                                    if (_seatsList[i].SeatAssignment.Trim() != string.Empty)
                                    {
                                        seatDetail.Seat = new Service.Presentation.CommonModel.AircraftModel.Seat();
                                        seatDetail.Seat.Identifier = _seatsList[i].SeatAssignment.Trim();
                                        seatDetail.Seat.Characteristics = new Collection<Characteristic>();

                                        Characteristic seatCharacter = new Characteristic();
                                        seatCharacter.Code = "DisplaySeatType";
                                        seatCharacter.Value = _seatsList[i].SeatType;
                                        seatDetail.Seat.Characteristics.Add(seatCharacter);

                                        seatDetail.Seat.Price = new ProductPrice();
                                        seatDetail.Seat.Price.PromotionCode = _seatsList[i].ProgramCode;
                                        seatDetail.Seat.Price.Totals = new Collection<Charge>();
                                        Charge seatPrice = new Charge();
                                        seatPrice.Amount = Convert.ToDouble(_seatsList[i].Price);
                                        seatPrice.Currency = new Currency();
                                        seatPrice.Currency.Code = _seatsList[i].Currency;
                                        seatDetail.Seat.Price.Totals.Add(seatPrice);


                                    }
                                    else
                                    {
                                        seatDetail.Seat = new Service.Presentation.CommonModel.AircraftModel.Seat();
                                        seatDetail.Seat.Identifier = string.Empty;
                                    }
                                    travelerInformation.Seats.Add(seatDetail);
                                    request.Travelers.Add(travelerInformation);
                                    i += 1;
                                    #endregion
                                }


                                #region "CSL30 - Start here"                               

                                if (request?.Travelers?.Count > 0)
                                {
                                    foreach (ProductTraveler travelerInformation in request.Travelers)
                                    {
                                        if (travelerInformation.Seats != null && travelerInformation.Seats.Count > 0)
                                        {
                                            foreach (SeatDetail seatInfo in travelerInformation.Seats)
                                            {
                                                var travelCompanionRulesResult= await ApplyTravelerCompanionRulesCSl30(sessionID, seats, segment, seatInfo,
                                                    travelerInformation, objSeatEngineSeatInfoList, tempSeatPrices);
                                                objSeatEngineSeatInfoList = travelCompanionRulesResult.objSeatEngineSeatInfoList;
                                                tempSeatPrices = travelCompanionRulesResult.tempSeatPrices;                                               
                                            }
                                        }
                                    }
                                }
                                #endregion "CSL30 - Start here"

                            }
                            #endregion
                        }
                        #endregion
                    }
                }
                #endregion
            }

            AddMissingSeatAssignment(tempSeatPrices, persistedReservation);

            if (tempSeatPrices.Count > 0)
            {
                seatPrices = new List<MOBSeatPrice>();
                string origin = tempSeatPrices[0].Origin;
                string destination = tempSeatPrices[0].Destination;

                CultureInfo ci = null;

                foreach (MOBSeatPrice seatPrice in tempSeatPrices)
                {
                    if (ci == null)
                    {
                        ci = GetCultureInfo(seatPrice.CurrencyCode);
                    }

                    if (seatPrice.Origin.Equals(origin) && seatPrice.Destination.Equals(destination))
                    {
                        var item = seatPrices.Find(s => s.Origin == seatPrice.Origin
                                                     && s.Destination == seatPrice.Destination
                                                     && s.SeatMessage == seatPrice.SeatMessage);

                        if (item != null)
                        {
                            item.NumberOftravelers = item.NumberOftravelers + 1;
                            item.TotalPrice = item.TotalPrice + seatPrice.TotalPrice;
                            item.TotalPriceDisplayValue = formatAmountForDisplay(item.TotalPrice, ci, false);
                            item.DiscountedTotalPrice = item.DiscountedTotalPrice + seatPrice.DiscountedTotalPrice;
                            item.DiscountedTotalPriceDisplayValue = formatAmountForDisplay(item.DiscountedTotalPrice, ci, false);
                            if (item.SeatNumbers.IsNullOrEmpty())
                                item.SeatNumbers = new List<string>();
                            if (!seatPrice.SeatNumbers.IsNullOrEmpty())
                                item.SeatNumbers.AddRange(seatPrice.SeatNumbers);
                        }
                        else
                        {
                            MOBSeatPrice sp = BuildSeatPricesWithDiscountedPrice(seatPrice);
                            if (!sp.IsNullOrEmpty())
                                seatPrices.Add(sp);
                        }
                    }
                    else
                    {
                        origin = seatPrice.Origin;
                        destination = seatPrice.Destination;

                        MOBSeatPrice sp = BuildSeatPricesWithDiscountedPrice(seatPrice);
                        if (!sp.IsNullOrEmpty())
                            seatPrices.Add(sp);
                    }
                }
            }

            lstSeats = await UpdateSeatPricesAfterApplyTravelerCompanionCall(sessionID, objSeatEngineSeatInfoList);

            seatPrices = SortAndOrderSeatPrices(seatPrices);
            Reservation reservationAfterUpdatingSeatPrice = await _sessionHelperService.GetSession<Reservation>(sessionID, new Reservation().ObjectName, new List<string> { sessionID, new Reservation().ObjectName }).ConfigureAwait(false);
            reservationAfterUpdatingSeatPrice.SeatPrices = seatPrices;
            await _sessionHelperService.SaveSession<Reservation>(reservationAfterUpdatingSeatPrice, sessionID, new List<string> { sessionID, reservationAfterUpdatingSeatPrice.ObjectName }, reservationAfterUpdatingSeatPrice.ObjectName).ConfigureAwait(false);

            return (seatPrices, lstSeats);
        }

        private async Task<(MOBSeatPrice MobSeatPrice, List<SeatDetail> objSeatEngineSeatInfoList, List<MOBSeatPrice> tempSeatPrices)> ApplyTravelerCompanionRulesCSl30(string sessionID, List<MOBSeat> seats, MOBSHOPFlight segment, SeatDetail seatInfo, ProductTraveler travelerInformation, List<SeatDetail> objSeatEngineSeatInfoList, List<MOBSeatPrice> tempSeatPrices)
        {
            MOBSeatPrice seatPrice = null;
            MOBSeatCSL30 seatDetail = null;
            CultureInfo ci = null;

            string travelerindex = travelerInformation.TravelerNameIndex.Split('.')[0];

            MOBTierPricingCSL30 selectedPrice = null;

            var persistedCSL30SeatMaps
                = await _sessionHelperService.GetSession<List<MOBSeatMapCSL30>>
                (sessionID, new MOBSeatMapCSL30().ObjectName, new List<string> { sessionID, new MOBSeatMapCSL30().ObjectName }).ConfigureAwait(false);

            if (persistedCSL30SeatMaps == null)
            {
                throw new MOBUnitedException("Unable to retrieve information needed for seat change.");
            }
            if (!persistedCSL30SeatMaps.IsNullOrEmpty() && !seatInfo.IsNullOrEmpty())
            {
                foreach (MOBSeatMapCSL30 pCSL30SeatMap in persistedCSL30SeatMaps)
                {
                    foreach (MOBSeatCSL30 csl30seat in pCSL30SeatMap.Seat)
                    {
                        if (seatInfo.DepartureAirport.IATACode == pCSL30SeatMap.DepartureCode
                            && seatInfo.ArrivalAirport.IATACode == pCSL30SeatMap.ArrivalCode)
                        {
                            seatDetail = pCSL30SeatMap.Seat.FirstOrDefault
                                (x => !x.IsNullOrEmpty() && x.Number == seatInfo.Seat.Identifier && x.IsAvailable);

                            selectedPrice = seatDetail?.pricing?.FirstOrDefault
                                (x => string.Equals(travelerindex, Convert.ToString(x.TravelerId),
                                StringComparison.OrdinalIgnoreCase));

                            if (selectedPrice != null)
                            {
                                double.TryParse(Convert.ToString(selectedPrice.TotalPrice), out double totalprice);
                                seatInfo.Seat.Price = new ProductPrice
                                {
                                    PromotionCode = seatDetail.EDoc,
                                    CouponCode = seatDetail.Pricing.FirstOrDefault().CouponCode,
                                    OriginalPrice = !string.IsNullOrEmpty(seatDetail?.Pricing?.FirstOrDefault()?.OriginalPrice) ? Convert.ToDouble(seatDetail?.Pricing?.FirstOrDefault()?.OriginalPrice) : 0,
                                    Totals = new Collection<Charge> { new Charge
                                { Amount = totalprice,
                                        Currency = new Currency{ Code = seatInfo.Seat.Price.Totals[0].Currency.Code } } }
                                };
                                break;
                            }
                        }
                    }
                }
            }

            objSeatEngineSeatInfoList.Add(seatInfo);

            if (seatInfo.Seat != null && seatInfo.Seat.Price != null)
            {
                if (ci == null)
                {
                    ci = GetCultureInfo(seatInfo.Seat.Price.Totals[0].Currency.Code);
                }

                var seat = seats.Where(s => s.Origin.Equals(segment.Origin, StringComparison.OrdinalIgnoreCase) &&
                        s.Destination.Equals(segment.Destination, StringComparison.OrdinalIgnoreCase) &&
                        s.SeatAssignment.Equals(seatInfo.Seat.Identifier, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                if ((!seat.IsNullOrEmpty() && seat.Price > 0)
                    && (!selectedPrice.IsNullOrEmpty() && !selectedPrice.TotalPrice.IsNullOrEmpty()))
                {
                    seatPrice = new MOBSeatPrice
                    {
                        Origin = segment.Origin,
                        Destination = segment.Destination,
                        SeatMessage = GetSeatMessageforSeatPrices
                                (seatInfo.Seat.Characteristics, seatInfo.Seat.Price.PromotionCode, seatDetail?.displaySeatCategory, seat.LimitedRecline),
                        NumberOftravelers = 1,

                        SeatNumbers = new List<string> { seatInfo.Seat.Identifier }
                    };

                    if (selectedPrice.TotalPrice.Equals(0))
                    {
                        seatPrice.TotalPrice = seat.Price;
                        seatPrice.DiscountedTotalPrice = 0;
                    }
                    else
                    {

                        if (_configuration.GetValue<bool>("EnableAdvanceSearchCouponBooking")
                            && !string.IsNullOrEmpty(selectedPrice.OriginalPrice)
                            && seatInfo?.Seat?.Price?.OriginalPrice > 0
                            && !string.IsNullOrEmpty(seatInfo?.Seat?.Price?.CouponCode))
                        {
                            seatPrice.TotalPrice = Convert.ToDecimal(selectedPrice.OriginalPrice);
                            seatPrice.DiscountedTotalPrice = Convert.ToDecimal(selectedPrice.OriginalPrice);
                        }
                        else
                        {
                            seatPrice.TotalPrice = Convert.ToDecimal(selectedPrice.TotalPrice);
                            seatPrice.DiscountedTotalPrice = Convert.ToDecimal(selectedPrice.TotalPrice);
                        }
                    }

                    seatPrice.CurrencyCode = seatInfo.Seat.Price.Totals.FirstOrDefault().Currency.Code;
                    seatPrice.TotalPriceDisplayValue
                        = formatAmountForDisplay(seatPrice.TotalPrice, ci, false);

                    seatPrice.DiscountedTotalPriceDisplayValue
                        = formatAmountForDisplay(seatPrice.DiscountedTotalPrice, ci, false);

                    if (!seatInfo.Seat.IsNullOrEmpty())
                        seatPrice.SeatNumbers = new List<string> { seatInfo.Seat.Identifier };

                    tempSeatPrices.Add(seatPrice);
                }
                else if (_configuration.GetValue<bool>("IsEnableAFSFreeSeatCoupon") && (!seat.IsNullOrEmpty() && seat.Price == 0) && (!selectedPrice.IsNullOrEmpty() && !selectedPrice.CouponCode.IsNullOrEmpty() && !selectedPrice.OriginalPrice.IsNullOrEmpty() && !selectedPrice.TotalPrice.IsNullOrEmpty()))
                {
                    seatPrice = new MOBSeatPrice
                    {
                        Origin = segment.Origin,
                        Destination = segment.Destination,
                        SeatMessage = GetSeatMessageforSeatPrices
                                (seatInfo.Seat.Characteristics, seatInfo.Seat.Price.PromotionCode, seatDetail?.displaySeatCategory, seat.LimitedRecline),
                        NumberOftravelers = 1,

                        SeatNumbers = new List<string> { seatInfo.Seat.Identifier }
                    };
                    if (selectedPrice.TotalPrice.Equals(0))
                    {
                        if (_configuration.GetValue<bool>("EnableAdvanceSearchCouponBooking")
                            && !string.IsNullOrEmpty(selectedPrice.OriginalPrice)
                            && seatInfo?.Seat?.Price?.OriginalPrice > 0
                            && !string.IsNullOrEmpty(seatInfo?.Seat?.Price?.CouponCode))
                        {
                            seatPrice.TotalPrice = Convert.ToDecimal(selectedPrice.OriginalPrice);
                            seatPrice.DiscountedTotalPrice = Convert.ToDecimal(selectedPrice.OriginalPrice);
                        }
                        else
                        {
                            seatPrice.TotalPrice = seat.Price;
                            seatPrice.DiscountedTotalPrice = 0;
                        }
                    }
                    else
                    {

                        if (_configuration.GetValue<bool>("EnableAdvanceSearchCouponBooking")
                            && !string.IsNullOrEmpty(selectedPrice.OriginalPrice)
                            && seatInfo?.Seat?.Price?.OriginalPrice > 0
                            && !string.IsNullOrEmpty(seatInfo?.Seat?.Price?.CouponCode))
                        {
                            seatPrice.TotalPrice = Convert.ToDecimal(selectedPrice.OriginalPrice);
                            seatPrice.DiscountedTotalPrice = Convert.ToDecimal(selectedPrice.OriginalPrice);
                        }
                        else
                        {
                            seatPrice.TotalPrice = Convert.ToDecimal(selectedPrice.TotalPrice);
                            seatPrice.DiscountedTotalPrice = Convert.ToDecimal(selectedPrice.TotalPrice);
                        }
                    }

                    seatPrice.CurrencyCode = seatInfo.Seat.Price.Totals.FirstOrDefault().Currency.Code;
                    seatPrice.TotalPriceDisplayValue
                        = formatAmountForDisplay(seatPrice.TotalPrice, ci, false);

                    seatPrice.DiscountedTotalPriceDisplayValue
                        = formatAmountForDisplay(seatPrice.DiscountedTotalPrice, ci, false);

                    if (!seatInfo.Seat.IsNullOrEmpty())
                        seatPrice.SeatNumbers = new List<string> { seatInfo.Seat.Identifier };

                    tempSeatPrices.Add(seatPrice);
                }
            }
            return (seatPrice, objSeatEngineSeatInfoList,tempSeatPrices);
        }

        private async Task<List<MOBSeat>> GetAllTravelerSeats(string sessionId)
        {
            var persistedReservation = await _sessionHelperService.GetSession<Reservation>(sessionId, new Reservation().ObjectName, new List<string> { sessionId, new Reservation().ObjectName }).ConfigureAwait(false);
            if (persistedReservation == null || persistedReservation.TravelersCSL == null)
                return null;
            var allSeats = persistedReservation.TravelersCSL.Values.SelectMany(t => t.Seats);

            return allSeats.ToList();
        }

        private string formatAwardAmountForDisplay(string amt, bool truncate = true)
        {
            string newAmt = amt;
            decimal amount = 0;
            decimal.TryParse(amt, out amount);

            try
            {
                if (amount > -1)
                {
                    if (truncate)
                    {
                        //int newTempAmt = (int)decimal.Ceiling(amount);
                        try
                        {
                            if (amount > 999)
                            {
                                amount = amount / 1000;
                                if (amount % 1 > 0)
                                    newAmt = string.Format("{0:n1}", amount) + "K miles";
                                else
                                    newAmt = string.Format("{0:n0}", amount) + "K miles";
                            }
                            else
                            {
                                newAmt = string.Format("{0:n0}", amount) + " miles";
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        try
                        {
                            newAmt = string.Format("{0:n0}", amount) + " miles";
                        }
                        catch { }
                    }
                }

            }
            catch { }

            return newAmt;
        }

        private void ResetPopulatedSeats(List<MOBSeat> seats, string origin, string destination)
        {
            int index = 0;
            foreach (var seat in seats)
            {
                if (seat.Destination == destination && seat.Origin == origin)
                {
                    seats.RemoveAt(index);
                    break;
                }
                index++;
            }
        }

        private async Task<List<MOBSeat>> FlightTravelerSeats(string sessionID, string seatAssignment, string origin, string destination, string paxIndex, string nextOrigin, string nextDestination)
        {
            List<MOBSeat> lstSeats = new List<MOBSeat>();
            List<MOBSeat> allSeats = new List<MOBSeat>();

            Reservation persistedReservation = await _sessionHelperService.GetSession<Reservation>(sessionID, new Reservation().ObjectName, new List<string> { sessionID, new Reservation().ObjectName }).ConfigureAwait(false);

            string[] arrSeatAssignments = seatAssignment.Split(',');
            string[] arrPax = paxIndex.Split(',');


            if (persistedReservation != null && persistedReservation.TravelersCSL != null)
            {
                for (int i = 0; i < arrPax.Length; i++)
                {
                    if (persistedReservation.IsReshopChange)
                    {
                        ResetPopulatedSeats(persistedReservation.TravelersCSL[arrPax[i]].Seats, origin, destination);
                    }

                    lstSeats = persistedReservation.TravelersCSL[arrPax[i]]?.Seats;
                    if (lstSeats != null && lstSeats.Count > 0)
                    {
                        IEnumerable<MOBSeat> seat = from s in lstSeats
                                                    where s.Origin == origin.ToUpper().Trim()
                                                    && s.Destination == destination.ToUpper().Trim()
                                                    && s.TravelerSharesIndex == arrPax[i]
                                                    select s;
                        if (seat != null && seat.Count() > 0)
                        {
                            List<MOBSeat> _seat = new List<MOBSeat>();
                            _seat = seat.ToList();
                            if (_seat.Count() == 1)
                            {
                                MOBSeat tmpSeat = new MOBSeat();

                                tmpSeat.Destination = destination;
                                tmpSeat.Origin = origin;
                                tmpSeat.TravelerSharesIndex = _seat[0].TravelerSharesIndex;
                                tmpSeat.Key = _seat[0].Key;

                                string[] assignments = arrSeatAssignments[i].Split('|');

                                if (assignments.Length == 6)
                                {
                                    //Added as part of the changes for the exception 284001:Select seatsFormatException
                                    if (_configuration.GetValue<bool>("BugFixToggleForExceptionAnalysis"))
                                    {
                                        decimal price;
                                        tmpSeat.SeatAssignment = assignments[0];
                                        price = string.IsNullOrEmpty(assignments[1]) ? 0 : Convert.ToDecimal(assignments[1]);
                                        tmpSeat.Price = price;
                                        tmpSeat.PriceAfterTravelerCompanionRules = price;
                                        tmpSeat.Currency = assignments[2];
                                        tmpSeat.ProgramCode = assignments[3];
                                        tmpSeat.SeatType = assignments[4];
                                        tmpSeat.LimitedRecline = string.IsNullOrEmpty(assignments[5]) ? false : Convert.ToBoolean(assignments[5]);
                                        if (ConfigUtility.IsMilesFOPEnabled())
                                        {
                                            tmpSeat.OldSeatMiles = Convert.ToInt32(_configuration.GetValue<string>("milesFOP"));
                                            tmpSeat.DisplayOldSeatMiles = formatAwardAmountForDisplay(_configuration.GetValue<string>("milesFOP"), false);
                                        }
                                    }
                                    else
                                    {
                                        tmpSeat.SeatAssignment = assignments[0];
                                        tmpSeat.Price = Convert.ToDecimal(assignments[1]);
                                        tmpSeat.PriceAfterTravelerCompanionRules = Convert.ToDecimal(assignments[1]);
                                        tmpSeat.Currency = assignments[2];
                                        tmpSeat.ProgramCode = assignments[3];
                                        tmpSeat.SeatType = assignments[4];
                                        tmpSeat.LimitedRecline = Convert.ToBoolean(assignments[5]);
                                        if (ConfigUtility.IsMilesFOPEnabled())
                                        {
                                            tmpSeat.OldSeatMiles = Convert.ToInt32(_configuration.GetValue<string>("milesFOP"));
                                            tmpSeat.DisplayOldSeatMiles = formatAwardAmountForDisplay(_configuration.GetValue<string>("milesFOP"), false);
                                        }
                                    }
                                }
                                else
                                {
                                    ///Bug fix for numbers 77203,203206 by Ranjit
                                    ///If SeatAssignment is null from the request, SeatAssignment value saving as empty so issue reproducible.
                                    ///Now at this time Seat details are fetching from Persist file and assigning to SeatAssignment in tmpSeat.
                                    if (string.IsNullOrEmpty(arrSeatAssignments[i]))
                                    {
                                        tmpSeat.Price = _seat[0].Price;
                                        tmpSeat.PriceAfterTravelerCompanionRules = _seat[0].PriceAfterTravelerCompanionRules;
                                        tmpSeat.Currency = _seat[0].Currency;
                                        tmpSeat.ProgramCode = _seat[0].ProgramCode;
                                        tmpSeat.SeatType = _seat[0].SeatType;
                                        tmpSeat.LimitedRecline = _seat[0].LimitedRecline;
                                        tmpSeat.SeatAssignment = _seat[0].SeatAssignment;
                                        if (ConfigUtility.IsMilesFOPEnabled())
                                        {
                                            tmpSeat.OldSeatMiles = Convert.ToInt32(_configuration.GetValue<string>("milesFOP"));
                                            tmpSeat.DisplayOldSeatMiles = formatAwardAmountForDisplay(_configuration.GetValue<string>("milesFOP"), false);
                                        }
                                    }
                                    else
                                    {
                                        tmpSeat.SeatAssignment = arrSeatAssignments[i];
                                    }
                                }

                                lstSeats[_seat[0].Key] = tmpSeat;
                            }
                        }
                        else
                        {
                            MOBSeat tmpSeat = new MOBSeat();
                            tmpSeat.Destination = destination;
                            tmpSeat.Origin = origin;

                            string[] assignments = arrSeatAssignments[i].Split('|');
                            if (assignments.Length == 6)
                            {
                                tmpSeat.SeatAssignment = assignments[0];
                                tmpSeat.Price = Convert.ToDecimal(assignments[1]);
                                tmpSeat.PriceAfterTravelerCompanionRules = Convert.ToDecimal(assignments[1]);
                                tmpSeat.Currency = assignments[2];
                                tmpSeat.ProgramCode = assignments[3];
                                tmpSeat.SeatType = assignments[4];
                                tmpSeat.LimitedRecline = Convert.ToBoolean(assignments[5]);
                                if (ConfigUtility.IsMilesFOPEnabled())
                                {
                                    tmpSeat.OldSeatMiles = Convert.ToInt32(_configuration.GetValue<string>("milesFOP"));
                                    tmpSeat.DisplayOldSeatMiles = formatAwardAmountForDisplay(_configuration.GetValue<string>("milesFOP"), false);
                                }
                            }
                            else
                            {
                                //if (arrSeatAssignments[i] != null && arrSeatAssignments[i] != string.Empty)
                                //{
                                tmpSeat.SeatAssignment = arrSeatAssignments[i];
                                //}
                            }
                            if (lstSeats == null)
                            {
                                lstSeats = new List<MOBSeat>();
                            }
                            tmpSeat.TravelerSharesIndex = arrPax[i];
                            tmpSeat.Key = lstSeats.Count;
                            lstSeats.Add(tmpSeat);
                        }

                    }
                    else
                    {
                        MOBSeat tmpSeat = new MOBSeat();
                        tmpSeat.Destination = destination;
                        tmpSeat.Origin = origin;

                        string[] assignments = arrSeatAssignments[i].Split('|');
                        if (assignments.Length == 6)
                        {
                            //Added as part of the changes for the exception 284001:Select seatsFormatException
                            if (_configuration.GetValue<bool>("BugFixToggleForExceptionAnalysis"))
                            {
                                decimal price;
                                price = string.IsNullOrEmpty(assignments[1]) ? 0 : Convert.ToDecimal(assignments[1]);
                                tmpSeat.SeatAssignment = assignments[0];
                                tmpSeat.Price = price;
                                tmpSeat.PriceAfterTravelerCompanionRules = price;
                                tmpSeat.Currency = assignments[2];
                                tmpSeat.ProgramCode = assignments[3];
                                tmpSeat.SeatType = assignments[4];
                                tmpSeat.LimitedRecline = string.IsNullOrEmpty(assignments[5]) ? false : Convert.ToBoolean(assignments[5]);
                                if (ConfigUtility.IsMilesFOPEnabled())
                                {
                                    tmpSeat.OldSeatMiles = Convert.ToInt32(_configuration.GetValue<string>("milesFOP"));
                                    tmpSeat.DisplayOldSeatMiles = formatAwardAmountForDisplay(_configuration.GetValue<string>("milesFOP"), false);
                                }
                            }
                            else
                            {
                                tmpSeat.SeatAssignment = assignments[0];
                                tmpSeat.Price = Convert.ToDecimal(assignments[1]);
                                tmpSeat.PriceAfterTravelerCompanionRules = Convert.ToDecimal(assignments[1]);
                                tmpSeat.Currency = assignments[2];
                                tmpSeat.ProgramCode = assignments[3];
                                tmpSeat.SeatType = assignments[4];
                                tmpSeat.LimitedRecline = Convert.ToBoolean(assignments[5]);
                                if (ConfigUtility.IsMilesFOPEnabled())
                                {
                                    tmpSeat.OldSeatMiles = Convert.ToInt32(_configuration.GetValue<string>("milesFOP"));
                                    tmpSeat.DisplayOldSeatMiles = formatAwardAmountForDisplay(_configuration.GetValue<string>("milesFOP"), false);
                                }
                            }
                        }
                        else
                        {
                            tmpSeat.SeatAssignment = arrSeatAssignments[i];
                        }
                        if (lstSeats == null)
                        {
                            lstSeats = new List<MOBSeat>();
                        }
                        tmpSeat.TravelerSharesIndex = arrPax[i];
                        tmpSeat.Key = lstSeats.Count;
                        lstSeats.Add(tmpSeat);

                    }
                    lstSeats = lstSeats?.OrderBy(s => s.Key)?.ToList();

                    AssignSeatUAOperatedFlag(lstSeats, allSeats, persistedReservation);

                    persistedReservation.TravelersCSL[arrPax[i]].Seats = lstSeats;
                }
            }


           await _sessionHelperService.SaveSession<Reservation>(persistedReservation, sessionID, new List<string> { sessionID, persistedReservation.ObjectName }, persistedReservation.ObjectName).ConfigureAwait(false);

            IEnumerable<MOBSeat> retSeat = from s in lstSeats
                                           where s.Origin == nextOrigin.ToUpper().Trim()
                                           && s.Destination == nextDestination.ToUpper().Trim()
                                           select s;
            if (retSeat?.Count() > 0)
            {
                lstSeats = retSeat?.ToList();
            }
            else if (!string.IsNullOrEmpty(nextDestination))
                allSeats = null;

            return allSeats;

        }

        private void AssignSeatUAOperatedFlag(List<MOBSeat> lstSeats, List<MOBSeat> allSeats, Reservation persistedReservation)
        {
            if (lstSeats != null)
            {
                foreach (MOBSeat seat in lstSeats)
                {
                    if (persistedReservation != null)
                    {
                        foreach (MOBSHOPTrip trip in persistedReservation.Trips)
                        {
                            foreach (MOBSHOPFlattenedFlight flight in trip.FlattenedFlights)
                            {
                                foreach (MOBSHOPFlight shopFlight in flight.Flights)
                                {
                                    if (seat.Origin.Trim().ToUpper() == shopFlight.Origin.Trim().ToUpper()
                                        && seat.Destination.Trim().ToUpper() == shopFlight.Destination.Trim().ToUpper()
                                        && shopFlight.OperatingCarrier.Trim().ToUpper() == "UA")
                                    {
                                        seat.UAOperated = true;
                                    }
                                }
                            }
                        }
                    }
                    allSeats.Add(seat);
                }
            }
        }

        private async Task<(bool, MOBBookingRegisterSeatsResponse registerSeatsResponse)> RegisterSeats(MOBRegisterSeatsRequest request, Session session, Reservation persistedReservation, MOBBookingRegisterSeatsResponse registerSeatsResponse, bool isClearSeats)
        {
            FlightReservationResponse response = new FlightReservationResponse();
            request.CartId = session.CartId;
            try
            {
                bool ignoreRegisterSeatsCSL = false;
                RegisterSeatsRequest registerSeatsRequest = (persistedReservation != null ? await GetRegisterSeatsRequest(request, persistedReservation) : null);
                if (registerSeatsRequest.SeatAssignments == null || registerSeatsRequest.SeatAssignments.Count == 0)
                    ignoreRegisterSeatsCSL = true;

                if (_shoppingCartUtility.IsEnableOmniCartMVP2Changes(request.Application.Id, request.Application.Version.Major, persistedReservation?.ShopReservationInfo2?.IsDisplayCart == true) && isClearSeats)
                {

                    try
                    {
                        var tupleResponse = await ClearSeats(request, session, response);
                        registerSeatsResponse.ShoppingCart = tupleResponse.response;
                        response = tupleResponse.flightReservationResponse;

                        await ClearTravelerSeats(session.SessionId);
                    }
                    catch (System.Net.WebException wex)
                    {
                        throw wex;
                    }
                    catch (MOBUnitedException uaex)
                    {
                        throw uaex;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }
                else
                {
                    var tupleResponse = await RegisterSeats(request, session.Token, null, persistedReservation, response, session);
                    registerSeatsResponse.ShoppingCart = tupleResponse.ShoppingCart;
                    response = tupleResponse.flightReservationResponse;
                }


                #region 159514 - Added for Inhibit booking 
                bool inhibitBooking = false;
                if (_configuration.GetValue<bool>("EnableInhibitBooking"))
                {
                    inhibitBooking = IdentifyInhibitWarning(response);
                }
                #endregion

                if (ignoreRegisterSeatsCSL || (response != null && response.Status.Equals(United.Services.FlightShopping.Common.StatusType.Success) && (response.Errors == null || (response.Errors != null && (response.Errors.Capacity == 0 || inhibitBooking)))))
                {

                    persistedReservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, new Reservation().ObjectName, new List<string> { request.SessionId, new Reservation().ObjectName }).ConfigureAwait(false);
                    if (EnableAdvanceSearchCouponBooking(request.Application.Id, request.Application.Version.Major))
                        registerSeatsResponse.Reservation.Prices = persistedReservation.Prices;
                    if (_shoppingCartUtility.IsEnableOmniCartMVP2Changes(request.Application.Id, request.Application.Version.Major, persistedReservation?.ShopReservationInfo2.IsDisplayCart == true)
                        ? request.IsDone : true)
                    {
                        if (EnableAdvanceSearchCouponBooking(request.Application.Id, request.Application.Version.Major)
                      && IsAFSSeatCouponApplied(registerSeatsResponse.ShoppingCart)
                      && persistedReservation.ShopReservationInfo2.SeatRemoveCouponPopupCount == 0)
                        {
                            var seatproduct = response.ShoppingCart.Items.SelectMany(d => d.Product).Where(d => d?.Code == "SEATASSIGNMENTS")?.ToList();
                            if (seatproduct.Any(x => x == null || x.CouponDetails == null || x.CouponDetails.Any(a => a != null && !a.IsCouponEligible.ToUpper().Equals("TRUE"))))
                            {
                                registerSeatsResponse.PromoCodeRemoveAlertForProducts = new MOBSection
                                {
                                    Text1 = _configuration.GetValue<string>("AdvanceSearchCouponWithRegisterSeatsErrorMessage"),
                                    Text2 = "Select a seat",
                                    Text3 = "Continue"
                                };

                                return (false, registerSeatsResponse);
                            }
                        }
                    }
                    decimal totalEplusSeatPrice = 0;
                    decimal totalOriginalEplusSeatPrice = 0;
                    double totalCurrentEplusSeatPrice = 0;
                    bool showLimitedRecline = false;
                    var countNoOfEPlusSeatPriceRows = 0;
                    var countNoOfEPlusPurchased = 0;

                    decimal totalEminusSeatPrice = 0;
                    decimal totalOriginalEminusSeatPrice = 0;
                    double totalCurrentEminusSeatPrice = 0;
                    var countNoOfEminusPurchased = 0;

                    decimal totalPreferedSeatPrice = 0;
                    decimal totalOriginalPreferedSeatPrice = 0;
                    double totalCurrentPreferedSeatPrice = 0;
                    var countNoOfPreferedPurchased = 0;
                    var preferedSeatText = _configuration.GetValue<string>("PreferedSeat_PriceBreakdownTitle") ?? string.Empty;
                    if (registerSeatsResponse.Reservation.Prices.Find(x => x.DisplayType.ToUpper() == "ECONOMYPLUS SEATS") != null)
                    {
                        totalCurrentEplusSeatPrice = registerSeatsResponse.Reservation.Prices.First(x => x.DisplayType.ToUpper() == "ECONOMYPLUS SEATS").Value;
                    }

                    if (registerSeatsResponse.Reservation.Prices.Find(x => x.DisplayType.ToUpper() == preferedSeatText.ToUpper()) != null)
                    {
                        totalCurrentPreferedSeatPrice = registerSeatsResponse.Reservation.Prices.First(x => x.DisplayType.ToUpper() == preferedSeatText.ToUpper()).Value;
                    }

                    if (registerSeatsResponse.Reservation.Prices.Find(x => x.DisplayType.ToUpper() == "ADVANCE SEAT ASSIGNMENT") != null)
                    {
                        totalCurrentEminusSeatPrice = registerSeatsResponse.Reservation.Prices.First(x => x.DisplayType.ToUpper() == "ADVANCE SEAT ASSIGNMENT").Value;
                    }

                    if (persistedReservation.SeatPrices != null)
                    {
                        foreach (var seatPrice in persistedReservation.SeatPrices)
                        {
                            if (seatPrice.SeatMessage.ToUpper().Contains("ECONOMY PLUS"))
                            {
                                if (!seatPrice.SeatNumbers.IsNullOrEmpty())
                                    countNoOfEPlusPurchased = countNoOfEPlusPurchased + seatPrice.SeatNumbers.Count;
                                countNoOfEPlusSeatPriceRows = countNoOfEPlusSeatPriceRows + 1;
                                if (countNoOfEPlusSeatPriceRows == 1)
                                    showLimitedRecline = seatPrice.SeatMessage.ToUpper().Contains("LIMITED RECLINE");
                                totalEplusSeatPrice += seatPrice.DiscountedTotalPrice;
                                totalOriginalEplusSeatPrice += seatPrice.TotalPrice;
                            }

                            if (seatPrice.SeatMessage.ToUpper().Contains(preferedSeatText.ToUpper()))
                            {
                                if (!seatPrice.SeatNumbers.IsNullOrEmpty())
                                    countNoOfPreferedPurchased = countNoOfPreferedPurchased + seatPrice.SeatNumbers.Count;
                                totalPreferedSeatPrice += seatPrice.DiscountedTotalPrice;
                                totalOriginalPreferedSeatPrice += seatPrice.TotalPrice;
                            }

                            if (seatPrice.SeatMessage.ToUpper().Contains("ADVANCE SEAT ASSIGNMENT"))
                            {
                                if (!seatPrice.SeatNumbers.IsNullOrEmpty())
                                    countNoOfEminusPurchased = countNoOfEminusPurchased + seatPrice.SeatNumbers.Count;
                                totalEminusSeatPrice += seatPrice.DiscountedTotalPrice;
                                totalOriginalEminusSeatPrice += seatPrice.TotalPrice;
                            }
                        }
                    }
                    bool totalExist = false;
                    bool eplusSeatPriceExist = false;
                    bool preferedSeatPriceExist = false;
                    bool eminusSeatPriceExist = false;
                    decimal flightTotal = 0;

                    for (int i = 0; i < registerSeatsResponse.Reservation.Prices.Count; i++)
                    {
                        if (registerSeatsResponse.Reservation.Prices[i].DisplayType.ToUpper() == "GRAND TOTAL")
                        {
                            totalExist = true;
                            var discountAmount = (totalCurrentEplusSeatPrice + totalCurrentPreferedSeatPrice + totalCurrentEminusSeatPrice) - Convert.ToDouble(totalEplusSeatPrice + totalPreferedSeatPrice + totalEminusSeatPrice);

                            registerSeatsResponse.Reservation.Prices[i].DisplayValue = string.Format("{0:#,0.00}", registerSeatsResponse.Reservation.Prices[i].Value - discountAmount);
                            registerSeatsResponse.Reservation.Prices[i].FormattedDisplayValue = TopHelper.FormatAmountForDisplay(registerSeatsResponse.Reservation.Prices[i].DisplayValue, new CultureInfo("en-US"), false); //string.Format("${0:c}", registerSeatsResponse.Reservation.Prices[i].DisplayValue); 

                            registerSeatsResponse.Reservation.Prices[i].Value = ParseToDoubleAndRound(registerSeatsResponse.Reservation.Prices[i].DisplayValue);
                        }
                        if (registerSeatsResponse.Reservation.Prices[i].DisplayType.ToUpper() == "ECONOMYPLUS SEATS")
                        {
                            eplusSeatPriceExist = true;
                            registerSeatsResponse.Reservation.Prices[i].DisplayValue = string.Format("{0:#,0.00}", totalEplusSeatPrice);
                            registerSeatsResponse.Reservation.Prices[i].FormattedDisplayValue = TopHelper.FormatAmountForDisplay(totalEplusSeatPrice.ToString(), new CultureInfo("en-US"), false); //totalEplusSeatPrice.ToString("C2", CultureInfo.CurrentCulture); 
                            registerSeatsResponse.Reservation.Prices[i].PriceTypeDescription = showLimitedRecline ? (countNoOfEPlusPurchased > 1 ? "Economy Plus Seats (limited recline)" : "Economy Plus Seat (limited recline)")
                                                                                                                  : (countNoOfEPlusPurchased > 1 ? "Economy Plus Seats" : "Economy Plus Seat");

                            registerSeatsResponse.Reservation.Prices[i].Value = ParseToDoubleAndRound(registerSeatsResponse.Reservation.Prices[i].DisplayValue);
                        }
                        if (registerSeatsResponse.Reservation.Prices[i].DisplayType.ToUpper() == preferedSeatText.ToUpper())
                        {
                            preferedSeatPriceExist = true;
                            registerSeatsResponse.Reservation.Prices[i].DisplayValue = string.Format("{0:#,0.00}", totalPreferedSeatPrice);
                            registerSeatsResponse.Reservation.Prices[i].FormattedDisplayValue = TopHelper.FormatAmountForDisplay(registerSeatsResponse.Reservation.Prices[i].DisplayValue, new CultureInfo("en-US"), false); //string.Format("{0:c2}", registerSeatsResponse.Reservation.Prices[i].DisplayValue); 
                            registerSeatsResponse.Reservation.Prices[i].PriceTypeDescription = countNoOfPreferedPurchased > 1 ? "Preferred seats" : "Preferred seat";
                            registerSeatsResponse.Reservation.Prices[i].Value = ParseToDoubleAndRound(registerSeatsResponse.Reservation.Prices[i].DisplayValue);
                        }
                        if (registerSeatsResponse.Reservation.Prices[i].DisplayType.ToUpper() == "ADVANCE SEAT ASSIGNMENT")
                        {
                            eminusSeatPriceExist = true;
                            registerSeatsResponse.Reservation.Prices[i].DisplayValue = string.Format("{0:#,0.00}", totalEminusSeatPrice);
                            registerSeatsResponse.Reservation.Prices[i].FormattedDisplayValue = TopHelper.FormatAmountForDisplay(totalEminusSeatPrice.ToString(), new CultureInfo("en-US"), false);// totalEminusSeatPrice.ToString("C2", CultureInfo.CurrentCulture); 
                            registerSeatsResponse.Reservation.Prices[i].PriceTypeDescription = countNoOfEminusPurchased > 1 ? "Advance Seat Assignments"
                                                                                                                            : "Advance Seat Assignment";

                            registerSeatsResponse.Reservation.Prices[i].Value = ParseToDoubleAndRound(registerSeatsResponse.Reservation.Prices[i].DisplayValue);
                        }
                        if (registerSeatsResponse.Reservation.Prices[i].DisplayType.ToUpper() == "TOTAL")
                        {
                            flightTotal = Convert.ToDecimal(registerSeatsResponse.Reservation.Prices[i].DisplayValue);
                        }
                    }
                    if (!eplusSeatPriceExist && totalEplusSeatPrice > 0)
                    {
                        MOBSHOPPrice seatTotal = new MOBSHOPPrice
                        {
                            CurrencyCode = persistedReservation.SeatPrices[0].CurrencyCode,
                            DisplayType = "EconomyPlus Seats",
                            DisplayValue = string.Format("{0:#,0.00}", totalEplusSeatPrice),
                            FormattedDisplayValue = TopHelper.FormatAmountForDisplay(totalEplusSeatPrice.ToString(), new CultureInfo("en-US"), false),
                            PriceType = "EconomyPlus Seats",
                            PriceTypeDescription = showLimitedRecline ? (countNoOfEPlusPurchased > 1 ? "Economy Plus Seats (limited recline)" : "Economy Plus Seat (limited recline)")
                                                                      : (countNoOfEPlusPurchased > 1 ? "Economy Plus Seats" : "Economy Plus Seat")
                        };
                        seatTotal.Value = ParseToDoubleAndRound(seatTotal.DisplayValue);
                        registerSeatsResponse.Reservation.Prices.Add(seatTotal);

                        var preferedSeatPrice = registerSeatsResponse.Reservation.Prices.Find(s => s.DisplayType.ToUpper() == preferedSeatText.ToUpper());
                        if (!preferedSeatPrice.IsNullOrEmpty())
                        {
                            registerSeatsResponse.Reservation.Prices.Remove(preferedSeatPrice);
                            registerSeatsResponse.Reservation.Prices.Add(preferedSeatPrice);
                        }

                        var economyMinusPrice = registerSeatsResponse.Reservation.Prices.Find(s => s.DisplayType.ToUpper() == "ADVANCE SEAT ASSIGNMENT");
                        if (!economyMinusPrice.IsNullOrEmpty())
                        {
                            registerSeatsResponse.Reservation.Prices.Remove(economyMinusPrice);
                            registerSeatsResponse.Reservation.Prices.Add(economyMinusPrice);
                        }
                    }
                    if (!preferedSeatPriceExist && totalPreferedSeatPrice > 0)
                    {
                        MOBSHOPPrice seatTotal = new MOBSHOPPrice
                        {
                            CurrencyCode = persistedReservation.SeatPrices[0].CurrencyCode,
                            DisplayType = preferedSeatText,
                            DisplayValue = string.Format("{0:#,0.00}", totalPreferedSeatPrice),
                            FormattedDisplayValue = TopHelper.FormatAmountForDisplay(totalPreferedSeatPrice, new CultureInfo("en-US"),false),//totalPreferedSeatPrice.ToString("C2", CultureInfo.CurrentCulture),
                            PriceType = preferedSeatText,
                            PriceTypeDescription = countNoOfPreferedPurchased > 1 ? "Preferred seats" : "Preferred seat"
                        };

                        seatTotal.Value = ParseToDoubleAndRound(seatTotal.DisplayValue);
                        registerSeatsResponse.Reservation.Prices.Add(seatTotal);

                        var economyMinusPrice = registerSeatsResponse.Reservation.Prices.Find(s => s.DisplayType.ToUpper() == "ADVANCE SEAT ASSIGNMENT");
                        if (!economyMinusPrice.IsNullOrEmpty())
                        {
                            registerSeatsResponse.Reservation.Prices.Remove(economyMinusPrice);
                            registerSeatsResponse.Reservation.Prices.Add(economyMinusPrice);
                        }
                    }

                    if (!eminusSeatPriceExist && totalEminusSeatPrice > 0)
                    {
                        MOBSHOPPrice seatTotal = new MOBSHOPPrice
                        {
                            CurrencyCode = persistedReservation.SeatPrices[0].CurrencyCode,
                            DisplayType = "Advance Seat Assignment",
                            DisplayValue = string.Format("{0:#,0.00}", totalEminusSeatPrice),
                            FormattedDisplayValue = TopHelper.FormatAmountForDisplay(totalPreferedSeatPrice, new CultureInfo("en-US")),//totalEminusSeatPrice.ToString("C2", CultureInfo.CurrentCulture),
                            PriceType = "Advance Seat Assignment",
                            PriceTypeDescription = countNoOfEminusPurchased > 1 ? "Advance Seat Assignments"
                                                                                : "Advance Seat Assignment"
                        };

                        seatTotal.Value = ParseToDoubleAndRound(seatTotal.DisplayValue);
                        registerSeatsResponse.Reservation.Prices.Add(seatTotal);
                    }

                    if (!totalExist)
                    {
                        var totalAmount = flightTotal + totalEplusSeatPrice + totalPreferedSeatPrice + totalEminusSeatPrice;
                        MOBSHOPPrice totalPrice = new MOBSHOPPrice();
                        totalPrice.CurrencyCode = registerSeatsResponse.Reservation.Prices[0].CurrencyCode;
                        totalPrice.DisplayType = "Grand Total";
                        totalPrice.DisplayValue = string.Format("{0:#,0.00}", totalAmount);
                        totalPrice.FormattedDisplayValue = TopHelper.FormatAmountForDisplay(totalAmount, new CultureInfo("en-US"),false);//(totalAmount).ToString("C2", CultureInfo.CurrentCulture);
                        totalPrice.PriceType = "Grand Total";

                        totalPrice.Value = ParseToDoubleAndRound(totalPrice.DisplayValue);
                        registerSeatsResponse.Reservation.Prices.Add(totalPrice);
                    }
                    if (eplusSeatPriceExist && totalOriginalEplusSeatPrice == 0)
                    {
                        registerSeatsResponse.Reservation.Prices.Remove(registerSeatsResponse.Reservation.Prices.Find(item => item.DisplayType.ToUpper() == "ECONOMYPLUS SEATS"));
                    }
                    if (preferedSeatPriceExist && totalOriginalPreferedSeatPrice == 0)
                    {
                        registerSeatsResponse.Reservation.Prices.Remove(registerSeatsResponse.Reservation.Prices.Find(item => item.DisplayType.ToUpper() == preferedSeatText.ToUpper()));
                    }
                    if (eminusSeatPriceExist && totalOriginalEminusSeatPrice == 0)
                    {
                        registerSeatsResponse.Reservation.Prices.Remove(registerSeatsResponse.Reservation.Prices.Find(item => item.DisplayType.ToUpper() == "ADVANCE SEAT ASSIGNMENT"));
                    }
                    if (EnableAdvanceSearchCouponBooking(request.Application.Id, request.Application.Version.Major) && request.Flow == FlowType.BOOKING.ToString())
                    {
                        List<MOBSHOPPrice> priceClone = registerSeatsResponse.Reservation.Prices.Clone();
                        UpdateSeatsPriceWithPromo(response, registerSeatsResponse.Reservation.Prices, persistedReservation.SeatPrices, registerSeatsResponse.ShoppingCart.Products, registerSeatsResponse.ShoppingCart.Products);
                        UpdateGrandTotal(registerSeatsResponse.Reservation.Prices, priceClone);
                        registerSeatsResponse.SeatPrices = persistedReservation.SeatPrices;
                        registerSeatsResponse.Reservation.SeatPrices = persistedReservation.SeatPrices;
                    }
                    #region 159514 - Inhibit booking 
                    if (inhibitBooking)
                    {

                        _logger.LogInformation("RegisterSeats - Response for RegisterSeats with Inhibit warning :{response} with {sessionId}", JsonConvert.SerializeObject(response), request.SessionId);

                        UpdateInhibitMessage(ref persistedReservation);
                        await _sessionHelperService.SaveSession<Reservation>(persistedReservation, persistedReservation.SessionId, new List<string> { persistedReservation.SessionId, persistedReservation.ObjectName }, persistedReservation.ObjectName).ConfigureAwait(false);
                    }
                    #endregion

                }
                else
                {
                    //159514 - Added for inhibit booking error message
                    if (_configuration.GetValue<bool>("EnableInhibitBooking"))
                    {
                        if (response.Errors.Exists(error => error != null && !string.IsNullOrEmpty(error.MinorCode) && error.MinorCode.Trim().Equals("10050")))
                        {
                            var inhibitErrorCsl = response.Errors.FirstOrDefault(error => error != null && !string.IsNullOrEmpty(error.MinorCode) && error.MinorCode.Trim().Equals("10050"));
                            throw new MOBUnitedException(inhibitErrorCsl.Message, new Exception(inhibitErrorCsl.MinorCode));
                        }
                    }
                    return (false, registerSeatsResponse);
                }

            }
            catch (MOBUnitedException uaex)
            {
                throw uaex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return (true, registerSeatsResponse);
        }

        private async Task<bool> AddOrRemovePromo(MOBRequest request, Session session, bool isRemove, string flow)
        {
            try
            {
                MOBShoppingCart persistedShoppingCart = new MOBShoppingCart();
                persistedShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(session.SessionId, persistedShoppingCart.ObjectName, new List<string> { session.SessionId, persistedShoppingCart.ObjectName }).ConfigureAwait(false);
                List<MOBPromoCode> promoCodes = new List<MOBPromoCode>();
                bool isAdvanceSearchCoupon = EnableAdvanceSearchCouponBooking(request.Application.Id, request.Application.Version.Major);

                var persistedApplyPromoCodeResponse = new MOBApplyPromoCodeResponse();
                persistedApplyPromoCodeResponse = await _sessionHelperService.GetSession<MOBApplyPromoCodeResponse>(session.SessionId, persistedApplyPromoCodeResponse.ObjectName, new List<string> { session.SessionId, persistedApplyPromoCodeResponse.ObjectName }).ConfigureAwait(false);
                if (persistedApplyPromoCodeResponse == null
                   || (persistedApplyPromoCodeResponse?.ShoppingCart?.PromoCodeDetails?.PromoCodes != null
                    && persistedApplyPromoCodeResponse?.ShoppingCart?.PromoCodeDetails?.PromoCodes.Count == 0)) //Do nothing if there is no coupon.
                {
                    if (isAdvanceSearchCoupon && persistedShoppingCart?.PromoCodeDetails != null && persistedShoppingCart?.PromoCodeDetails?.PromoCodes != null && persistedShoppingCart?.PromoCodeDetails?.PromoCodes.Count > 0)
                    {
                        promoCodes = persistedShoppingCart?.PromoCodeDetails?.PromoCodes;
                    }
                    else
                        return true;
                }
                MOBApplyPromoCodeRequest promoCodeRequest = new MOBApplyPromoCodeRequest();
                if (isAdvanceSearchCoupon)
                {
                    if (promoCodes == null || promoCodes?.Count == 0)
                        promoCodes = persistedApplyPromoCodeResponse.ShoppingCart?.PromoCodeDetails?.PromoCodes;
                }
                else
                    promoCodes = persistedApplyPromoCodeResponse.ShoppingCart?.PromoCodeDetails?.PromoCodes;
                promoCodeRequest = BuildApplyPromoCodeRequest(request, session, isRemove, promoCodes, flow);
                var tupleResponse = await ApplyPromoCode(promoCodeRequest, session, true).ConfigureAwait(false);
                MOBItem inEligibleReason = tupleResponse.inEligibleReason;
                
                if (inEligibleReason != null && !string.IsNullOrEmpty(inEligibleReason.CurrentValue))
                    return false;
            }
            catch (System.Net.WebException wex)
            {
                throw wex;
            }
            catch (MOBUnitedException uaex)
            {
                throw uaex;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }

            return true;
        }

        private bool IsUpliftFopAdded(MOBShoppingCart shoppingCart)
        {
            if (shoppingCart?.FormofPaymentDetails != null && shoppingCart.FormofPaymentDetails.FormOfPaymentType.ToUpper() == MOBFormofPayment.Uplift.ToString().ToUpper())
            {
                return true;
            }
            return false;
        }

        private RegisterCouponRequest BuildRegisterOrRemoveCouponRequest(MOBApplyPromoCodeRequest request, bool isRemove)
        {
            RegisterCouponRequest cslCouponRequest = new RegisterCouponRequest();
            cslCouponRequest.CartId = request.CartId;
            cslCouponRequest.Channel = _configuration.GetValue<string>("Shopping - ChannelType");
            cslCouponRequest.Requestor = new Service.Presentation.CommonModel.Requestor();
            cslCouponRequest.Requestor.ChannelID = _configuration.GetValue<bool>("IsEnableManageResCoupon") && !string.IsNullOrEmpty(request.Flow) && (request.Flow.Equals(FlowType.VIEWRES.ToString()) || request.Flow.Equals(FlowType.VIEWRES_SEATMAP.ToString())) ? _configuration.GetValue<string>("MerchandizeOffersServiceMRCouponChannelID") : _configuration.GetValue<string>("MerchandizeOffersCSLServiceChannelID");
            cslCouponRequest.Requestor.ChannelName = _configuration.GetValue<bool>("IsEnableManageResCoupon") && !string.IsNullOrEmpty(request.Flow) && (request.Flow.Equals(FlowType.VIEWRES.ToString()) || request.Flow.Equals(FlowType.VIEWRES_SEATMAP.ToString())) ? _configuration.GetValue<string>("MerchandizeOffersServiceMRCouponChannelName") : _configuration.GetValue<string>("MerchandizeOffersCSLServiceChannelName");
            cslCouponRequest.Requestor.ChannelSource = _configuration.GetValue<string>("RegisterCouponServiceChannelSource");
            cslCouponRequest.Coupons = new System.Collections.ObjectModel.Collection<Service.Presentation.ProductModel.ProductPromotion>();
            cslCouponRequest.Requestor.CountryCode = "US";
            cslCouponRequest.Requestor.LanguageCode = request.LanguageCode;
            request.PromoCodes.ForEach(x =>
            {
                if (isRemove)
                {
                    if (x.IsRemove)
                    {
                        cslCouponRequest.Coupons.Add(new ProductPromotion
                        {
                            Code = !_configuration.GetValue<bool>("DisableHandlingCaseSenstivity") ? x.PromoCode.ToUpper().Trim() : x.PromoCode.Trim()
                        });
                    }
                }
                else
                {
                    if (!x.IsRemove)
                    {
                        cslCouponRequest.Coupons.Add(new ProductPromotion
                        {
                            Code = !_configuration.GetValue<bool>("DisableHandlingCaseSenstivity") ? x.PromoCode.ToUpper().Trim() : x.PromoCode.Trim()
                        });
                    }
                }
            });
            cslCouponRequest.Requestor.LanguageCode = request.LanguageCode;
            cslCouponRequest.WorkFlowType = GetWorkFlowType(request.Flow);

            _logger.LogInformation("BuildRegisterOrRemoveCouponRequest CslCouponRequest{cslCouponRequest} SessionId:{sessionId}", JsonConvert.SerializeObject(cslCouponRequest), request.SessionId);

            return cslCouponRequest;
        }

        private async Task<FlightReservationResponse> RegisterOrRemoveCoupon(MOBApplyPromoCodeRequest request, Session session, bool isRemove)
        {
            var registerCouponRequest = BuildRegisterOrRemoveCouponRequest(request, isRemove);
            string jsonRequest = JsonConvert.SerializeObject(registerCouponRequest);
            FlightReservationResponse response = new FlightReservationResponse();
            string cslActionName = isRemove ? "RemoveCoupon" : "RegisterCoupon";


            response = await _shoppingCartService.RegisterOrRemoveCoupon<FlightReservationResponse>(session.Token, cslActionName, jsonRequest, session.SessionId);


            if (!(response == null))
            {
                if (!(response != null && response.Status.Equals(United.Services.FlightShopping.Common.StatusType.Success) && response.DisplayCart != null && response.Reservation != null))
                {
                    if (response.Errors != null && response.Errors.Count > 0)
                    {
                        if (response.Errors.Any(error => (error.MajorCode == "2000" || error.MajorCode == "3000")))
                        {
                            var errors = response.Errors.Where(error => (error.MajorCode == "2000" || error.MajorCode == "3000")).FirstOrDefault();
                            throw new MOBUnitedException(errors.MajorCode, errors.Message);
                        }
                        else
                        {
                            string errorMessage = string.Empty;
                            foreach (var error in response.Errors)
                            {
                                errorMessage = errorMessage + " " + error.Message;
                            }
                            throw new System.Exception(errorMessage);
                        }
                    }
                }
            }
            else
            {
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }

            return response;
        }

        private bool IsMaxPromoCountReached(MOBShoppingCart persistShoppingCart)
        {
            if (persistShoppingCart?.PromoCodeDetails?.PromoCodes != null
                && persistShoppingCart.PromoCodeDetails.PromoCodes.Any()
                && persistShoppingCart.PromoCodeDetails.PromoCodes.Count > 0)
            {
                return true;
            }

            return false;
        }

        private double GetCloseBookingFee(bool isPost, United.Service.Presentation.InteractionModel.ShoppingCart flightReservationResponseShoppingCart, string flow)
        {
            return isPost ? flightReservationResponseShoppingCart.Items.SelectMany(d => d.Product).Where(d => d.Code == "RBF").FirstOrDefault().Price.Totals.FirstOrDefault().Amount :
                                  flightReservationResponseShoppingCart.Items.SelectMany(d => d.Product).Where(d => d.Code == "RBF").SelectMany(x => x.Price.Totals).FirstOrDefault().Amount;
        }

        private double GetGrandTotalPriceForShoppingCart(bool isCompleteFarelockPurchase, FlightReservationResponse flightReservationResponse, bool isPost, string flow = "VIEWRES")
        {
            //Added Null check for price.Since,CSL is not sending the price when we select seat of price zero.
            //Added condition to check whether Total in the price exists(This is for scenario when we register the bundle after registering the seat).
            //return isCompleteFarelockPurchase ? Convert.ToDouble(flightReservationResponse.DisplayCart.DisplayPrices.FirstOrDefault(o => (o.Description != null && o.Description.Equals("GrandTotal", StringComparison.OrdinalIgnoreCase))).Amount)
            //                                  : (Utility.IsCheckinFlow(flow) || flow == FlowType.VIEWRES.ToString()) ? flightReservationResponse.ShoppingCart.Items.Where(x => x.Product.FirstOrDefault().Code != "RES" && (x.Product.FirstOrDefault().Price != null ? (x.Product.FirstOrDefault().Price.Totals.Any()) : false)).Select(x => x.Product.FirstOrDefault().Price.Totals.FirstOrDefault().Amount).ToList().Sum() : flightReservationResponse.ShoppingCart.Items.SelectMany(x => x.Product).Where(x => x.Price != null).SelectMany(x => x.Price.Totals).Where(x => (x.Name != null ? x.Name.ToUpper() == "GrandTotalForCurrency".ToUpper() : true)).Select(x => x.Amount).ToList().Sum();
            double shoppingCartTotalPrice = 0.0;
            double closeBookingFee = 0.0;
            if (isCompleteFarelockPurchase)
                shoppingCartTotalPrice = Convert.ToDouble(flightReservationResponse.DisplayCart.DisplayPrices.FirstOrDefault(o => (o.Description != null && o.Description.Equals("GrandTotal", StringComparison.OrdinalIgnoreCase))).Amount);
            else
            {
                if (isPost ? flightReservationResponse.CheckoutResponse.ShoppingCartResponse.Items.Select(x => x.Item).Any(x => x.Product.FirstOrDefault().Code == "FLK")
                        : flightReservationResponse.ShoppingCart.Items.Any(x => x.Product.FirstOrDefault().Code == "FLK"))
                    flow = FlowType.FARELOCK.ToString();
                //[MB-6519]:Getting Sorry Something went wrong for Award Booking for with Reward booking fee and reservation price is zero
                if (_configuration.GetValue<bool>("CFOP19HBugFixToggle") && (isPost ? flightReservationResponse.CheckoutResponse.ShoppingCartResponse.Items.Select(x => x.Item).Any(x => x.Product.FirstOrDefault().Code == "RBF")
                                                                            : flightReservationResponse.ShoppingCart.Items.Any(x => x.Product.FirstOrDefault().Code == "RBF")))

                {
                    United.Service.Presentation.InteractionModel.ShoppingCart flightReservationResponseShoppingCart = new United.Service.Presentation.InteractionModel.ShoppingCart();
                    flightReservationResponseShoppingCart = isPost ? flightReservationResponse.CheckoutResponse.ShoppingCart : flightReservationResponse.ShoppingCart;
                    closeBookingFee = GetCloseBookingFee(isPost, flightReservationResponseShoppingCart, flow);

                }

                switch (flow)
                {
                    case "BOOKING":
                    case "RESHOP":
                        shoppingCartTotalPrice = isPost ? flightReservationResponse.CheckoutResponse.ShoppingCart.Items.Where(x => !UtilityHelper.CheckFailedShoppingCartItem(flightReservationResponse, x)).SelectMany(x => x.Product).Where(x => x.Price != null).SelectMany(x => x.Price.Totals).Where(x => (x.Name != null ? x.Name.ToUpper() == "GrandTotalForCurrency".ToUpper() /*|| x.Name.ToUpper() == "Close-In Booking Fee".ToUpper()*/ : true)).Select(x => x.Amount).ToList().Sum()
                            : flightReservationResponse.ShoppingCart.Items.SelectMany(x => x.Product).Where(x => x.Price != null).SelectMany(x => x.Price.Totals).Where(x => (x.Name != null ? x.Name.ToUpper() == "GrandTotalForCurrency".ToUpper() /*|| x.Name.ToUpper() == "Close-In Booking Fee".ToUpper()*/ : true)).Select(x => x.Amount).ToList().Sum();
                        break;

                    case "POSTBOOKING":
                        //productCodes = flightReservationResponseShoppingCart.Items.SelectMany(x => x.Product).Where(x => x.Characteristics != null && (x.Characteristics.Any(y => y.Description.ToUpper() == "POSTPURCHASE" && Convert.ToBoolean(y.Value) == true))).Select(x => x.Code).ToList();
                        shoppingCartTotalPrice = isPost ? flightReservationResponse.CheckoutResponse.ShoppingCart.Items.Where(x => !UtilityHelper.CheckFailedShoppingCartItem(flightReservationResponse, x)).Where(x => (x.Product.FirstOrDefault().Price != null ? (x.Product.FirstOrDefault().Price.Totals.Any()) : false) && x.Product.FirstOrDefault().Characteristics != null && (x.Product.FirstOrDefault().Characteristics.Any(y => y.Description == "PostPurchase" && Convert.ToBoolean(y.Value) == true))).Select(x => x.Product.FirstOrDefault().Price.Totals.FirstOrDefault().Amount).ToList().Sum()
                            : flightReservationResponse.ShoppingCart.Items.Where(x => (x.Product.FirstOrDefault().Price != null ? (x.Product.FirstOrDefault().Price.Totals.Any()) : false) && x.Product.FirstOrDefault().Characteristics != null && (x.Product.FirstOrDefault().Characteristics.Any(y => y.Description == "PostPurchase" && Convert.ToBoolean(y.Value) == true))).Select(x => x.Product.FirstOrDefault().Price.Totals.FirstOrDefault().Amount).ToList().Sum(); ;
                        break;

                    case "FARELOCK":
                        shoppingCartTotalPrice = isPost ? flightReservationResponse.CheckoutResponse.ShoppingCart.Items.Where(x => !UtilityHelper.CheckFailedShoppingCartItem(flightReservationResponse, x)).Where(x => x.Product.FirstOrDefault().Code == "FLK" && (x.Product.FirstOrDefault().Price != null ? (x.Product.FirstOrDefault().Price.Totals.Any()) : false)).Select(x => x.Product.FirstOrDefault().Price.Totals.FirstOrDefault().Amount).ToList().Sum()
                            : flightReservationResponse.ShoppingCart.Items.Where(x => x.Product.FirstOrDefault().Code == "FLK" && (x.Product.FirstOrDefault().Price != null ? (x.Product.FirstOrDefault().Price.Totals.Any()) : false)).Select(x => x.Product.FirstOrDefault().Price.Totals.FirstOrDefault().Amount).ToList().Sum();
                        break;

                    case "CHECKIN":
                        shoppingCartTotalPrice = isPost ? flightReservationResponse.CheckoutResponse.ShoppingCart.Items.Where(x => !UtilityHelper.CheckFailedShoppingCartItem(flightReservationResponse, x)).Where(x => x.Product.FirstOrDefault().Code != "RES" && (x.Product.FirstOrDefault().Price != null ? (x.Product.FirstOrDefault().Price.Totals.Any()) : false)).Select(x => x.Product.FirstOrDefault().Price.Totals.FirstOrDefault().Amount).ToList().Sum()
                            : flightReservationResponse.ShoppingCart.Items.Where(x => x.Product.FirstOrDefault().Code != "RES" && (x.Product.FirstOrDefault().Price != null ? (x.Product.FirstOrDefault().Price.Totals.Any()) : false)).Select(x => x.Product.FirstOrDefault().Price.Totals.FirstOrDefault().Amount).ToList().Sum();
                        break;
                    case "VIEWRES":
                    case "VIEWRES_SEATMAP":
                    case "MOBILECHECKOUT":
                        shoppingCartTotalPrice = isPost ? flightReservationResponse.CheckoutResponse.ShoppingCart.Items.Where(x => !UtilityHelper.CheckFailedShoppingCartItem(flightReservationResponse, x)).Where(x => x.Product.FirstOrDefault().Code != "RES" && (x.Product.FirstOrDefault().Price != null ? (x.Product.FirstOrDefault().Price.Totals.Any()) : false)).Select(x => x.Product.FirstOrDefault().Price.Totals.FirstOrDefault().Amount).ToList().Sum()
                            : flightReservationResponse.ShoppingCart.Items.Where(x => x.Product.FirstOrDefault().Code != "RES" && (x.Product.FirstOrDefault().Price != null ? (x.Product.FirstOrDefault().Price.Totals.Any()) : false)).Select(x => x.Product.FirstOrDefault().Price.Totals.Select(t => t != null && t.Amount > 0 ? t.Amount : 0).ToList().Sum()).ToList().Sum();
                        break;
                }
            }
            if (_configuration.GetValue<bool>("CFOP19HBugFixToggle"))
            {
                shoppingCartTotalPrice = shoppingCartTotalPrice + closeBookingFee;
            }
            return shoppingCartTotalPrice;
        }

        private bool BuildMoneyPlusMilesPrice(List<MOBSHOPPrice> prices, List<DisplayPrice> displayPrices, bool isRemove, List<MOBProdDetail> products)
        {
            bool isDirty = false;
            double mmValue = 0;
            if (isRemove)
            {
                var mmPrice = prices.FirstOrDefault(p => p.DisplayType == "MILESANDMONEY");
                if (mmPrice != null)
                {
                    mmValue = mmPrice.Value;
                    prices.Remove(mmPrice);
                    isDirty = true;
                }
            }
            else
            {
                var mmPrice = displayPrices.FirstOrDefault(p => p.Type.Equals("MILESANDMONEY", StringComparison.OrdinalIgnoreCase));
                if (mmPrice != null && mmPrice.Amount > 0)
                {
                    MOBSHOPPrice bookingPrice = new MOBSHOPPrice();
                    bookingPrice.CurrencyCode = mmPrice.Currency;
                    bookingPrice.DisplayType = mmPrice.Type;
                    bookingPrice.Status = mmPrice.Status;
                    bookingPrice.Waived = mmPrice.Waived;
                    bookingPrice.DisplayValue = string.Format("{0:#,0.00}", mmPrice.Amount);
                    bookingPrice.PriceTypeDescription = "Miles applied";

                    double tempDouble = 0;
                    double.TryParse(mmPrice.Amount.ToString(), out tempDouble);
                    bookingPrice.Value = Math.Round(tempDouble, 2, MidpointRounding.AwayFromZero);
                    CultureInfo ci = GetCultureInfo(mmPrice.Currency);
                    bookingPrice.FormattedDisplayValue = "-" + formatAmountForDisplay(mmPrice.Amount, ci, false); // Money and Miles have to be displayed in -ve format as per MOBILE-14807
                    prices.Add(bookingPrice);
                    mmValue = bookingPrice.Value;
                    isDirty = true;
                }
            }

            if (isDirty)
            {
                var price = prices.FirstOrDefault(p => p.DisplayType == "RES");
                UpdateCertificateRedeemAmountFromTotalInReserationPrices(price, mmValue, !isRemove);
                price = prices.FirstOrDefault(p => p.DisplayType == "TOTAL");
                UpdateCertificateRedeemAmountFromTotalInReserationPrices(price, mmValue, !isRemove);
                price = prices.FirstOrDefault(p => p.DisplayType.ToUpper().Equals("GRAND TOTAL"));
                UpdateCertificateRedeemAmountFromTotalInReserationPrices(price, mmValue, !isRemove);
                var scRESProduct = products.Find(p => p.Code == "RES");
                UpdateCertificateRedeemAmountInSCProductPrices(scRESProduct, mmValue, !isRemove);
            }
            return isDirty;

        }

        private void UpdateCertificateRedeemAmountInSCProductPrices(MOBProdDetail scProduct, double value, bool isRemove = true)
        {
            if (scProduct != null)
            {
                double prodValue = Convert.ToDouble(scProduct.ProdTotalPrice);
                prodValue = Math.Round(prodValue, 2, MidpointRounding.AwayFromZero);
                double prodValueAfterUpdate;
                if (isRemove)
                {
                    prodValueAfterUpdate = prodValue - value;
                }
                else
                {
                    prodValueAfterUpdate = prodValue + value;
                }
                prodValueAfterUpdate = Math.Round(prodValueAfterUpdate, 2, MidpointRounding.AwayFromZero);
                scProduct.ProdTotalPrice = (prodValueAfterUpdate).ToString("N2", CultureInfo.CurrentCulture); //(prodValueAfterUpdate).ToString("N2", CultureInfo.CurrentCulture);
                scProduct.ProdDisplayTotalPrice = (prodValueAfterUpdate).ToString("C2", CultureInfo.CurrentCulture);
            }
        }

        private void UpdateCertificateRedeemAmountFromTotalInReserationPrices(MOBSHOPPrice price, double value, bool isRemove = true)
        {
            if (price != null)
            {
                if (isRemove)
                {
                    price.Value -= value;
                }
                else
                {
                    price.Value += value;
                }
                price.Value = Math.Round(price.Value, 2, MidpointRounding.AwayFromZero);
                price.FormattedDisplayValue = (price.Value).ToString("C2", CultureInfo.CurrentCulture);
                price.DisplayValue = string.Format("{0:#,0.00}", price.Value);
            }
        }


        private async Task<(MOBApplyPromoCodeResponse response, MOBItem inEligibleReason)> ApplyPromoCode(MOBApplyPromoCodeRequest request, Session session, bool isByPassValidations = false)
        {
            MOBApplyPromoCodeResponse response = new MOBApplyPromoCodeResponse();
            MOBItem inEligibleReason;
            var persistShoppingCart = new MOBShoppingCart();
            Reservation bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, new Reservation().ObjectName, new List<string> { request.SessionId, new Reservation().ObjectName }).ConfigureAwait(false);
            persistShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, persistShoppingCart.ObjectName, new List<string> { request.SessionId, persistShoppingCart.ObjectName }).ConfigureAwait(false);
            inEligibleReason = new MOBItem();
            FlightReservationResponse cslFlightReservationResponse = new FlightReservationResponse();
            try
            {
                #region coupon Service Call        

                if (request.PromoCodes.Any(p => p.IsRemove))
                {
                    if (IsUpliftFopAdded(persistShoppingCart) && !isByPassValidations)
                    {
                        throw new MOBUnitedException(_configuration.GetValue<string>("RemovePromo_UpliftAddedMessage"));
                    }
                    cslFlightReservationResponse = await RegisterOrRemoveCoupon(request, session, true);
                    if (EnableAdvanceSearchCouponBooking(request.Application.Id, request.Application.Version.Major))
                    {
                        persistShoppingCart.PromoCodeDetails = new MOBPromoCodeDetails
                        {
                            PromoCodes = null,
                            IsHidePromoOption = false,
                            IsDisablePromoOption = false
                        };
                    }
                }
                if (request.PromoCodes.Any(p => !p.IsRemove))
                {
                    if (IsMaxPromoCountReached(persistShoppingCart))
                    {
                        response.MaxCountMessage = _configuration.GetValue<string>("MaxPromoCodeMessage");
                        response.ShoppingCart = persistShoppingCart;
                        response.Reservation = new MOBSHOPReservation(_configuration, _cachingService);
                        response.Reservation = await _productUtility.GetReservationFromPersist(response.Reservation, session);
                        return (response, inEligibleReason);
                    }
                    cslFlightReservationResponse = await RegisterOrRemoveCoupon(request, session, false);
                }
                #endregion coupon Service Call
                #region Update Prices with CouponDetails    
                var persistedSCproducts = persistShoppingCart.Products.Clone();
                persistShoppingCart.Products = await _shoppingCartUtility.ConfirmationPageProductInfo(cslFlightReservationResponse, false, false, request.Flow, request.Application, null, false, false, null, null, sessionId: request?.SessionId);
                UpdatePricesWithPromo(cslFlightReservationResponse, bookingPathReservation.Prices.Clone(), persistedSCproducts, persistShoppingCart.Products, bookingPathReservation, session);
                AddFreeBagDetails(persistShoppingCart, bookingPathReservation);
                #endregion Update Prices with CouponDetails  
                #region TPI in booking path
                if (EnableTPI(request.Application.Id, request.Application.Version.Major, 3) && !bookingPathReservation.IsReshopChange)
                {
                    // call TPI 
                    try
                    {
                        string token = session.Token;
                        MOBTPIInfoInBookingPath tPIInfo = await GetBookingPathTPIInfo(request.SessionId, request.LanguageCode, request.Application, request.DeviceId, request.CartId, token, true, false, false);
                        bookingPathReservation.TripInsuranceFile = new MOBTripInsuranceFile();
                        bookingPathReservation.TripInsuranceFile.TripInsuranceBookingInfo = tPIInfo;
                    }
                    catch (Exception ex)
                    {
                        bookingPathReservation.TripInsuranceFile = null;
                    }
                }
                #endregion
                #region Save Reservation & ShoppingCart              
                double price = GetGrandTotalPriceForShoppingCart(false, cslFlightReservationResponse, false, request.Flow);
                persistShoppingCart.TotalPrice = String.Format("{0:0.00}", price);
                persistShoppingCart.DisplayTotalPrice = Decimal.Parse(price.ToString()).ToString("c", new CultureInfo("en-us"));//string.Format("${0:c}", Decimal.Parse(price.ToString())); //Decimal.Parse(price.ToString()).ToString("c");
                AssignWarningMessage(bookingPathReservation.ShopReservationInfo2, cslFlightReservationResponse.Warnings);
                #region Update the chase banner price total
                // Code is moved to utility so that it can be reused across different actions
                if (_shoppingCartUtility.EnableChaseOfferRTI(request.Application.Id, request.Application.Version.Major))
                    _shoppingCartUtility.UpdateChaseCreditStatement(bookingPathReservation);
                #endregion Update the chase banner price Total
                await Task.WhenAll(
                _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, request.SessionId, new List<string> { request.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName),
                _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, request.SessionId, new List<string> { request.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName));
                #endregion Save Reservation & ShoppingCart          

            }
            catch (System.Net.WebException wex)
            {
                throw wex;
            }
            catch (MOBUnitedException uaex)
            {
                //If it is a soft failure from service it should be thrown as united exception .
                //But, no need to rethrow back to contrller since client shows Soft failures from service as inline instead of popup
                if (!string.IsNullOrEmpty(uaex.Code) && (uaex.Code == "2000" || uaex.Code == "3000"))
                {
                    inEligibleReason.Id = uaex.Code;
                    inEligibleReason.CurrentValue = uaex.Message;
                    MOBExceptionWrapper uaexWrapper = new MOBExceptionWrapper(uaex);
                    _logger.LogError("ApplyPromoCode UnitedException:{exception} with {sessionId}", JsonConvert.SerializeObject(uaexWrapper), request.SessionId);
                }
                else
                {
                    throw uaex;
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            if (_shoppingCartUtility.IncludeMoneyPlusMiles(request.Application.Id, request.Application.Version.Major) && !bookingPathReservation.Prices.Exists(p => p.DisplayType == "MILESANDMONEY"))
            {
                if (cslFlightReservationResponse?.DisplayCart?.DisplayPrices != null && BuildMoneyPlusMilesPrice(bookingPathReservation.Prices, cslFlightReservationResponse.DisplayCart.DisplayPrices, false, persistShoppingCart.Products))
                {
                    await Task.WhenAll(
                    _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, request.SessionId, new List<string> { request.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName),
                    _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, request.SessionId, new List<string> { request.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName));
                }
            }
            await UpdateTravelBankPrice(request, persistShoppingCart, session, bookingPathReservation);
            response.ShoppingCart = persistShoppingCart;
            response.Reservation = new MOBSHOPReservation(_configuration, _cachingService);
            response.Reservation = await _productUtility.GetReservationFromPersist(response.Reservation, session);
            #region Update individual amount in traveler
            await UpdateTravelerIndividualAmount(response.Reservation, response.ShoppingCart, cslFlightReservationResponse, bookingPathReservation, request);

            #endregion Update individual amount in traveler
            return (response, inEligibleReason);
        }

        private void AddFreeBagDetails(MOBShoppingCart shoppingCart, Reservation bookingPathReservation)
        {
            if (_configuration.GetValue<bool>("EnableCouponMVP2Changes"))
            {
                //Fare+Bag Coupon check for display type in coupondetails ,FreeBag Coupon check SC->Product has Bag product
                if (shoppingCart?.Products != null
                    && (shoppingCart.Products.Any(p => p.Code == "BAG")
                    || (shoppingCart.Products.Find(p => p.Code == "RES").CouponDetails != null && shoppingCart.Products.Find(p => p.Code == "RES").CouponDetails.Any()
                        && shoppingCart.Products.Find(p => p.Code == "RES").CouponDetails.First().Product == "BAG")))
                {
                    if (bookingPathReservation?.Prices != null)
                    {
                        bookingPathReservation.Prices.Add(new MOBSHOPPrice
                        {
                            PriceTypeDescription = _configuration.GetValue<string>("FreeBagCouponDescription"),
                            DisplayType = "TRAVELERPRICE",
                            FormattedDisplayValue = "",
                            DisplayValue = "",
                            Value = 0
                        });
                    }
                }
                else
                {
                    if (bookingPathReservation?.Prices != null)
                    {
                        var bagPriceItem = bookingPathReservation.Prices.Find(p => p.DisplayType == "TRAVELERPRICE"
                                                                                   && p.PriceTypeDescription.Equals(_configuration.GetValue<string>("FreeBagCouponDescription")));
                        if (bagPriceItem != null)
                        {
                            bookingPathReservation.Prices.Remove(bagPriceItem);
                        }
                    }
                }
            }
        }

        private void UpdatePricesWithPromo(United.Services.FlightShopping.Common.FlightReservation.FlightReservationResponse cslFlightReservationResponse, List<MOBSHOPPrice> peristedPrices, List<MOBProdDetail> persistedSCproducts, List<MOBProdDetail> updatedSCproducts, Reservation bookingPathReservation, Session session)
        {
            UpdateFlightPriceWithPromo(cslFlightReservationResponse, bookingPathReservation.Prices, persistedSCproducts, updatedSCproducts, bookingPathReservation,session);
            UpdateSeatsPriceWithPromo(cslFlightReservationResponse, bookingPathReservation.Prices, bookingPathReservation.SeatPrices, persistedSCproducts, updatedSCproducts);
            UpdateGrandTotal(bookingPathReservation.Prices, peristedPrices);
        }

        private void UpdateFlightPriceWithPromo(United.Services.FlightShopping.Common.FlightReservation.FlightReservationResponse cslFlightReservationResponse, List<MOBSHOPPrice> prices, List<MOBProdDetail> persistedSCproducts, List<MOBProdDetail> updatedSCproducts, Reservation bookingPathReservation, Session session)
        {
            double promoValue = 0;
            if (prices != null && prices.Any(x => x.DisplayType.ToUpper().Contains("TRAVELERPRICE"))
                                && IsPromoDetailsUpdatedByProduct(persistedSCproducts, updatedSCproducts, "RES"))
            {
                var displayCartPrices = cslFlightReservationResponse.DisplayCart.DisplayPrices;
                if (displayCartPrices != null && displayCartPrices.Any())
                {
                    #region Update Reservation Prices(RTI) for Price breakdown
                    foreach (var price in prices)
                    {
                        if (displayCartPrices.Any(dp => dp.PaxTypeCode == price.PaxTypeCode && (dp.Type.ToUpper().Equals("TRAVELERPRICE") || dp.Type.ToUpper().Equals("TOTAL")))
                            && (price.DisplayType.Equals("TRAVELERPRICE") || (price.DisplayType.Equals("TOTAL"))))
                        {
                            if (price.DisplayType.Equals("TRAVELERPRICE"))
                            {
                                var nonDiscountedPrice = displayCartPrices.Find(dp => dp.PaxTypeCode == price.PaxTypeCode && dp.Type.ToUpper().Equals("NONDISCOUNTPRICE-TRAVELERPRICE"));
                                var discountedPrice = displayCartPrices.Find(dp => dp.PaxTypeCode == price.PaxTypeCode && dp.Type.ToUpper().Equals("TRAVELERPRICE"));
                                if (discountedPrice != null && nonDiscountedPrice != null)
                                {
                                    promoValue = Math.Round(Convert.ToDouble(nonDiscountedPrice.Amount)
                                                 - Convert.ToDouble(discountedPrice.Amount), 2, MidpointRounding.AwayFromZero);
                                }
                                else
                                {
                                    promoValue = 0;
                                }
                            }
                            if (price.DisplayType.Equals("TOTAL"))
                            {
                                var nonDiscountedTotalPrice = displayCartPrices.Find(dp => dp.PaxTypeCode == price.PaxTypeCode && dp.Type.ToUpper().Equals("NONDISCOUNTPRICE-TOTAL"));
                                var discountedTotalPrice = displayCartPrices.Find(dp => dp.PaxTypeCode == price.PaxTypeCode && dp.Type.ToUpper().Equals("TOTAL"));
                                if (discountedTotalPrice != null && nonDiscountedTotalPrice != null)
                                {
                                    promoValue = Math.Round(Convert.ToDouble(nonDiscountedTotalPrice.Amount)
                                                - Convert.ToDouble(discountedTotalPrice.Amount), 2, MidpointRounding.AwayFromZero);
                                    promoValue = _shoppingUtility.UpdatePromoValueForFSRMoneyMiles(displayCartPrices, session, promoValue);
                                }
                                else
                                {
                                    promoValue = 0;
                                }
                            }
                            CultureInfo ci = null;
                            price.PromoDetails = promoValue > 0 ? new MOBPromoCode
                            {
                                PriceTypeDescription = _configuration.GetValue<string>("PromoCodeAppliedText"),
                                PromoValue = Math.Round(promoValue, 2, MidpointRounding.AwayFromZero),
                                FormattedPromoDisplayValue = "-" + TopHelper.FormatAmountForDisplay(promoValue.ToString(), ci, false)// promoValue.ToString("C2", CultureInfo.CurrentCulture)
                            } : null;

                        }
                    }
                    #endregion  Update Reservation Prices(RTI)
                }
                #region update taxes               
                bookingPathReservation.ShopReservationInfo2.InfoNationalityAndResidence.ComplianceTaxes = GetTaxAndFeesAfterPriceChange(cslFlightReservationResponse.DisplayCart.DisplayPrices, bookingPathReservation.IsReshopChange);
                #endregion update taxes
            }
        }

        private void AssignWarningMessage(MOBSHOPReservationInfo2 shopReservationInfo2, List<United.Services.FlightShopping.Common.ErrorInfo> Warnings)
        {
            if (shopReservationInfo2 == null)
            {
                shopReservationInfo2 = new MOBSHOPReservationInfo2();
            }
            if (shopReservationInfo2.InfoWarningMessages == null)
            {
                shopReservationInfo2.InfoWarningMessages = new List<MOBInfoWarningMessages>();
            }
            MOBInfoWarningMessages selectSeatWarningMessage = new MOBInfoWarningMessages();

            if (shopReservationInfo2.InfoWarningMessages.Exists(x => x.Order == MOBINFOWARNINGMESSAGEORDER.RTIPROMOSELECTSEAT.ToString()))
            {
                shopReservationInfo2.InfoWarningMessages.Remove(shopReservationInfo2.InfoWarningMessages.Find(x => x.Order == MOBINFOWARNINGMESSAGEORDER.RTIPROMOSELECTSEAT.ToString()));
            }
            if (Warnings != null && Warnings.Any() && Warnings.Count > 0)
            {
                selectSeatWarningMessage = new MOBInfoWarningMessages();
                selectSeatWarningMessage.Order = MOBINFOWARNINGMESSAGEORDER.RTIPROMOSELECTSEAT.ToString();
                selectSeatWarningMessage.IconType = MOBINFOWARNINGMESSAGEICON.WARNING.ToString();
                selectSeatWarningMessage.Messages = new List<string>();
                selectSeatWarningMessage.Messages.Add(Warnings.FirstOrDefault().Message);
                if (selectSeatWarningMessage != null)
                {
                    shopReservationInfo2.InfoWarningMessages.Add(selectSeatWarningMessage);
                    shopReservationInfo2.InfoWarningMessages = shopReservationInfo2.InfoWarningMessages.OrderBy(c => (int)((MOBINFOWARNINGMESSAGEORDER)Enum.Parse(typeof(MOBINFOWARNINGMESSAGEORDER), c.Order))).ToList();
                }
            }
        }

        private async Task UpdateTravelBankPrice(MOBRequest request, MOBShoppingCart shoppingCart, Session session, Reservation persistShoppingCart)
        {
            if (IncludeTravelBankFOP(request.Application.Id, request.Application.Version.Major))
            {
                var scRESProduct = shoppingCart.Products.Find(p => p.Code == "RES");
                double prodValue = Convert.ToDouble(scRESProduct?.ProdTotalPrice);
                prodValue = Math.Round(prodValue, 2, MidpointRounding.AwayFromZero);
                if (shoppingCart?.FormofPaymentDetails?.TravelBankDetails?.TBApplied > 0 && shoppingCart?.FormofPaymentDetails?.TravelBankDetails?.TBApplied != prodValue)
                {
                    var tbRequest = new MOBFOPTravelerBankRequest();
                    tbRequest.Application = request.Application;
                    tbRequest.SessionId = session.SessionId;

                    tbRequest.AppliedAmount = prodValue < shoppingCart.FormofPaymentDetails.TravelBankDetails.TBApplied ? prodValue : shoppingCart.FormofPaymentDetails.TravelBankDetails.TBApplied;
                    tbRequest.IsRemove = false;
                    var response = await TravelBankCredit(session, tbRequest, false);
                    shoppingCart.FormofPaymentDetails = response.ShoppingCart.FormofPaymentDetails;
                    if (persistShoppingCart != null)
                        persistShoppingCart.Prices = response.Reservation.Prices;
                }
            }
        }

        private async Task<MOBFOPResponse> TravelBankCredit(Session session, MOBFOPTravelerBankRequest request, bool isCustomerCall)
        {
            Reservation bookingPathReservation = new Reservation ();
            var loadBasicFopResult=await _productUtility.LoadBasicFOPResponse(session, bookingPathReservation);
            MOBFOPResponse response = loadBasicFopResult.MobFopResponse;
            bookingPathReservation = loadBasicFopResult.BookingPathReservation;
            response.Flow = request.Flow;

            var travelBankDetails = response.ShoppingCart.FormofPaymentDetails.TravelBankDetails;
            if (travelBankDetails == null)
            {
                travelBankDetails = new MOBFOPTravelBankDetails();
            }
            var scRESProduct = response.ShoppingCart.Products.Find(p => p.Code == "RES");
            double prodValue = Convert.ToDouble(scRESProduct.ProdTotalPrice);
            prodValue = Math.Round(prodValue, 2, MidpointRounding.AwayFromZero);


            travelBankDetails.TBBalance = await GetTravelBankBalance(session.SessionId);
            travelBankDetails.DisplayTBBalance = (travelBankDetails.TBBalance).ToString("C2", CultureInfo.CurrentCulture);
            travelBankDetails.TBApplied = request.AppliedAmount > prodValue ? prodValue : request.AppliedAmount;
            travelBankDetails.TBApplied = request.IsRemove ? 0 : travelBankDetails.TBApplied;
            if (isCustomerCall)
                travelBankDetails.TBAppliedByCustomer = travelBankDetails.TBApplied;
            else if (travelBankDetails.TBAppliedByCustomer > travelBankDetails.TBApplied && travelBankDetails.TBAppliedByCustomer <= prodValue)
                travelBankDetails.TBApplied = travelBankDetails.TBAppliedByCustomer;

            travelBankDetails.DisplaytbApplied = (travelBankDetails.TBApplied).ToString("C2", CultureInfo.CurrentCulture);
            travelBankDetails.RemainingBalance = travelBankDetails.TBBalance > 0 ? travelBankDetails.TBBalance - travelBankDetails.TBApplied : 0;
            travelBankDetails.DisplayRemainingBalance = (travelBankDetails.RemainingBalance).ToString("C2", CultureInfo.CurrentCulture);
            travelBankDetails.DisplayAvailableBalanceAsOfDate = $"{"Balance as of "}{ DateTime.Now.ToString("MM/dd/yyyy") }";
            List<CMSContentMessage> lstMessages = await _productUtility.GetSDLContentByGroupName(request, session.SessionId, session.Token, _configuration.GetValue<string>("CMSContentMessages_GroupName_BookingRTI_Messages"), "BookingPathRTI_CMSContentMessagesCached_StaticGUID");
            travelBankDetails.ApplyTBContentMessage = _productUtility.GetSDLContentMessages(lstMessages, "RTI.TravelBank.Apply");
            //travelBankDetails.ReviewTBContentMessage = GetSDLContentMessages(lstMessages, "RTI.TravelBank.Review");
            travelBankDetails.LearnmoreTermsandConditions = _productUtility.GetSDLContentMessages(lstMessages, "RTI.TravelBank.Learnmore");
            _productUtility.SwapTitleAndLocation(travelBankDetails.ApplyTBContentMessage);
            //SwapTitleAndLocation(travelBankDetails.ReviewTBContentMessage);
            _productUtility.SwapTitleAndLocation(travelBankDetails.LearnmoreTermsandConditions);
            UpdateCertificateAmountInTotalPrices(bookingPathReservation.Prices, (request.IsRemove ? 0 : travelBankDetails.TBApplied), "TB", "TravelBank cash");
            response.Reservation.Prices = bookingPathReservation.Prices;

            _shoppingCartUtility.AssignIsOtherFOPRequired(response.ShoppingCart.FormofPaymentDetails, response.Reservation.Prices);
            AssignFormOfPaymentType(response.ShoppingCart.FormofPaymentDetails, response.Reservation.Prices, response.ShoppingCart.FormofPaymentDetails.IsOtherFOPRequired, travelBankDetails.TBApplied, MOBFormofPayment.TB);
            response.ShoppingCart.FormofPaymentDetails.TravelBankDetails = travelBankDetails;

            if (_shoppingCartUtility.IsEnableBundleLiveUpdateChanges(request.Application.Id, request.Application.Version.Major, response.Reservation?.ShopReservationInfo2?.IsDisplayCart == true))
            {
                _shoppingCartUtility.BuildOmniCart(response.ShoppingCart, response.Reservation, request.Application);
            }
            
            response.PkDispenserPublicKey = await _pKDispenserPublicKey.GetCachedOrNewpkDispenserPublicKey(request.Application.Id, request.Application.Version.Major, request.DeviceId, session.SessionId, session.Token, session?.CatalogItems).ConfigureAwait(false);
            #region OfferCode expansion Changes(This properties are added as part of offercode expansion changes but can be later used in general )
            response.ShoppingCart.FormofPaymentDetails.IsFOPRequired = _shoppingCartUtility.IsFOPRequired(response.ShoppingCart, response.Reservation);
            response.ShoppingCart.FormofPaymentDetails.IsEnableAgreeandPurchaseButton = _shoppingCartUtility.IsEnableAgreeandPurchaseButton(response.ShoppingCart, response.Reservation);
            response.ShoppingCart.FormofPaymentDetails.MaskedPaymentMethod =await _shoppingCartUtility.AssignMaskedPaymentMethod(response.ShoppingCart,request.Application).ConfigureAwait(false);
            #endregion OfferCode expansion Changes
            await Task.WhenAll(
            _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, session.SessionId, new List<string> { session.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName),
            _sessionHelperService.SaveSession<MOBShoppingCart>(response.ShoppingCart, session.SessionId, new List<string> { session.SessionId, response.ShoppingCart.ObjectName }, response.ShoppingCart.ObjectName));

            _logger.LogInformation("TravelBankCredit Response {response} sessionID{sessionId}", JsonConvert.SerializeObject(response), session.SessionId);

            return response;
        }

        private void UpdateCertificateAmountInTotalPrices(List<MOBSHOPPrice> prices, double certificateTotalAmount, string fopType, string fopDescription)
        {
            var ffcPrice = prices.FirstOrDefault(p => p.DisplayType.ToUpper() == fopType);
            var grandtotal = prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "GRAND TOTAL");

            if (ffcPrice == null && certificateTotalAmount > 0)
            {
                ffcPrice = new MOBSHOPPrice();
                prices.Add(ffcPrice);
            }
            else if (ffcPrice != null)
            {
                UpdateCertificateRedeemAmountFromTotalInReserationPrices(grandtotal, ffcPrice.Value, false);
            }

            if (certificateTotalAmount > 0)
            {
                UpdateCertificatePrice(ffcPrice, certificateTotalAmount, fopType, fopDescription, isAddNegative: true);
                UpdateCertificateRedeemAmountFromTotalInReserationPrices(grandtotal, certificateTotalAmount);
            }
            else
            {
                prices.Remove(ffcPrice);
            }
        }

        private MOBSHOPPrice UpdateCertificatePrice(MOBSHOPPrice ffc, double totalAmount, string priceType, string priceTypeDescription, string status = "", bool isAddNegative = false)
        {
            ffc.CurrencyCode = "USD";
            ffc.DisplayType = priceType;
            ffc.PriceType = priceType;
            ffc.Status = status;
            ffc.PriceTypeDescription = priceTypeDescription;
            ffc.Value = totalAmount;
            ffc.Value = Math.Round(ffc.Value, 2, MidpointRounding.AwayFromZero);
            ffc.FormattedDisplayValue = TopHelper.FormatAmountForDisplay((isAddNegative ? "-" : "") + (ffc.Value).ToString(), new CultureInfo("en-US"));// (isAddNegative ? "-" : "") + (ffc.Value).ToString("C2", CultureInfo.CurrentCulture);
            ffc.DisplayValue = string.Format("{0:#,0.00}", ffc.Value);

            return ffc;
        }

        private void AssignFormOfPaymentType(MOBFormofPaymentDetails formofPaymentDetails, List<MOBSHOPPrice> prices, bool isOtherFOPRequired, double fopAmount, MOBFormofPayment fopPayment)
        {
            //need to update only when TravelFutureFlightCredit is added as formofpayment.          
            if (fopAmount > 0)
            {
                if (!isOtherFOPRequired)
                {
                    formofPaymentDetails.FormOfPaymentType = fopPayment.ToString();
                    if (!string.IsNullOrEmpty(formofPaymentDetails.CreditCard?.Message) &&
                        _configuration.GetValue<string>("CreditCardDateExpiredMessage").IndexOf(formofPaymentDetails.CreditCard?.Message, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        formofPaymentDetails.CreditCard = null;
                    }
                }
                else
                {
                    formofPaymentDetails.FormOfPaymentType = MOBFormofPayment.CreditCard.ToString();
                }
            }
            else
            {
                formofPaymentDetails.FormOfPaymentType = MOBFormofPayment.CreditCard.ToString();
            }
        }

        private bool IncludeTravelBankFOP(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableTravelBankFOP")
                && GeneralHelper.isApplicationVersionGreater
                (appId, appVersion, "AndroidTravelBankFOPVersion", "iPhoneTravelBankFOPVersion", "", "", true, _configuration);
        }

        private async Task UpdateTravelerIndividualAmount(MOBSHOPReservation reservation, MOBShoppingCart ShoppingCart, United.Services.FlightShopping.Common.FlightReservation.FlightReservationResponse cslFlightReservationResponse, Reservation bookingPathReservation, MOBRequest request)
        {
            if ((reservation != null
                && ShoppingCart != null
                && cslFlightReservationResponse != null
                && ShoppingCart?.Products != null
                && ShoppingCart.Products.Any(p => p.Code == "RES" && p.CouponDetails != null)
                && cslFlightReservationResponse?.DisplayCart?.DisplayPrices != null) ||
                (IncludeFFCResidual(request.Application.Id, request.Application.Version.Major)
                 && cslFlightReservationResponse?.DisplayCart?.DisplayPrices != null))

            {
                AssignTravelerIndividualTotalAmount(reservation.TravelersCSL, cslFlightReservationResponse.DisplayCart.DisplayPrices, cslFlightReservationResponse.Reservation?.Travelers.ToList(), cslFlightReservationResponse.Reservation?.Prices?.ToList());


                if (reservation.TravelersCSL != null && reservation.TravelersCSL.Count > 0)
                {
                    bookingPathReservation.TravelersCSL = new SerializableDictionary<string, MOBCPTraveler>();
                    foreach (var travelersCSL in reservation.TravelersCSL)
                    {
                        bookingPathReservation.TravelersCSL.Add(travelersCSL.PaxIndex.ToString(), travelersCSL);
                    }
                }

                await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, reservation.SessionId, new List<string> { reservation.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);
            }
        }

        private bool IncludeFFCResidual(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableFFCResidual") && GeneralHelper.isApplicationVersionGreater(appId, appVersion, "AndroidFFCResidualVersion", "iPhoneFFCResidualVersion", "", "", true, _configuration);
        }

        private void AssignTravelerIndividualTotalAmount(List<MOBCPTraveler> travelers, List<DisplayPrice> displayPrices, List<Service.Presentation.ReservationModel.Traveler> cslReservationTravelers, List<Service.Presentation.PriceModel.Price> cslReservationPrices)
        {

            if (travelers?.Count > 0 && displayPrices?.Count > 0)
            {
                foreach (var traveler in travelers)
                {
                    var cslReservationTraveler = cslReservationTravelers.Find(crt => crt.Person.Key == traveler.TravelerNameIndex);
                    if (cslReservationTraveler == null && traveler.TravelerTypeCode == "INF")
                    {
                        cslReservationTraveler = cslReservationTravelers.Find(crt => crt.Person.Type == "INF");
                    }
                    DisplayPrice dPrice = null;
                    if (cslReservationTraveler == null)
                    {
                        dPrice = displayPrices.Find(dp => dp.PaxTypeCode == traveler.TravelerTypeCode);
                    }
                    else
                    {
                        var MultiplePriceTypeExist = displayPrices.Where(dp => (dp.PaxTypeCode == cslReservationTraveler.Person.Type) && (_configuration.GetValue<bool>("EnableCouponsforBooking")
                        ? !string.IsNullOrEmpty(dp.Type) && !dp.Type.ToUpper().Contains("NONDISCOUNTPRICE")
                        : true));
                        if (MultiplePriceTypeExist.Count() > 1)
                        {
                            var cslReservationPrice = cslReservationPrices.Find(crp => crp.PassengerIDs?.Key.IndexOf(traveler.TravelerNameIndex) > -1);
                            traveler.CslReservationPaxTypeCode = cslReservationPrice.PassengerTypeCode;
                            traveler.IndividualTotalAmount = cslReservationPrice.Totals.ToList().Find(t => t.Name.ToUpper() == "GRANDTOTALFORCURRENCY" && t.Currency.Code == "USD").Amount;
                        }
                        else
                        {
                            dPrice = displayPrices.Find(dp => (dp.PaxTypeCode == cslReservationTraveler.Person.Type));
                        }
                        traveler.CslReservationPaxTypeCode = cslReservationTraveler.Person.Type;
                    }
                    if (dPrice != null && dPrice.Amount > 0 && (_configuration.GetValue<bool>("EnableCouponsforBooking") ? true : traveler.IndividualTotalAmount == 0))
                    {
                        var amount = Math.Round((dPrice.Amount / Convert.ToDecimal(dPrice.Count)), 2, MidpointRounding.AwayFromZero);
                        traveler.IndividualTotalAmount = Convert.ToDouble(amount);
                        if (dPrice.SubItems != null)
                        {
                            foreach (var sp in dPrice.SubItems)
                            {
                                traveler.IndividualTotalAmount += Math.Round(Convert.ToDouble(sp.Amount), 2, MidpointRounding.AwayFromZero);
                            }
                        }
                    }
                }
            }
        }

        private MOBApplyPromoCodeRequest BuildApplyPromoCodeRequest(MOBRequest request, Session session, bool isRemove, List<MOBPromoCode> promoCodes, string flow)
        {

            MOBApplyPromoCodeRequest applyPromoCodeRequest = new MOBApplyPromoCodeRequest();
            applyPromoCodeRequest.CartId = session.CartId;
            applyPromoCodeRequest.SessionId = session.SessionId;
            applyPromoCodeRequest.TransactionId = request.TransactionId;
            applyPromoCodeRequest.DeviceId = request.DeviceId;
            applyPromoCodeRequest.Application = request.Application;
            applyPromoCodeRequest.LanguageCode = request.LanguageCode;
            applyPromoCodeRequest.Flow = flow;
            applyPromoCodeRequest.PromoCodes = new List<MOBPromoCode>();

            promoCodes.ForEach(x =>
            {
                if (isRemove)
                {
                    applyPromoCodeRequest.PromoCodes.Add(new MOBPromoCode
                    {
                        PromoCode = !_configuration.GetValue<bool>("DisableHandlingCaseSenstivity") ? x.PromoCode.ToUpper().Trim() : x.PromoCode.Trim(),
                        IsRemove = isRemove
                    });
                }
            });

            return applyPromoCodeRequest;
        }

        private async Task<MOBBookingRegisterSeatsResponse> CompleteSeats(MOBRegisterSeatsRequest request, Session session, bool updateSeatsAfterBundlesChanged = false, bool isClearSeats = false)
        {
            MOBBookingRegisterSeatsResponse response = new MOBBookingRegisterSeatsResponse();


            FlightReservationResponse cslResponse = new FlightReservationResponse();
            Reservation persistedReservation = new Reservation();
            if (!(IsEnableOmniCartReleaseCandidateThreeChanges_Seats(request.Application.Id, request.Application.Version.Major) && request.IsOmniCartSavedTripFlow)
                && !(_shoppingCartUtility.IsEnableOmniCartMVP2Changes(request.Application.Id, request.Application.Version.Major, true) && request.IsRemove))
            {


                response.Seats = updateSeatsAfterBundlesChanged ?
                                await GetAllTravelerSeats(request.SessionId) :
                                await FlightTravelerSeats(request.SessionId, request.SeatAssignment, request.Origin, request.Destination, request.PaxIndex, "", "");

                persistedReservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, persistedReservation.ObjectName, new List<string> { request.SessionId, persistedReservation.ObjectName }).ConfigureAwait(false);
                if (persistedReservation.LMXTravelers != null && persistedReservation.LMXTravelers.Any())
                {
                    response.Reservation.lmxtravelers = persistedReservation.LMXTravelers;
                }
                if (EnableAdvanceSearchCouponBooking(request.Application.Id, request.Application.Version.Major)
                    && request.ContinueToRegisterAncillary)
                {
                    if (!await AddOrRemovePromo(request, session, true, request.Flow))
                    {
                        throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                    }
                }
                int highestEliteLevel = 0;
                bool hasSeatPrice = false;
                bool hasSubscriptions = false;
                bool hasMPSavedTraveler = false;

                List<MOBSeat> allSeats = new List<MOBSeat>();
                List<MOBSeat> lstSeats = new List<MOBSeat>();

                if (persistedReservation != null && persistedReservation.TravelersCSL != null)
                {
                    foreach (MOBCPTraveler traveler in persistedReservation.TravelersCSL.Values)
                    {
                        if (traveler.MileagePlus != null && traveler.MileagePlus.CurrentEliteLevel > highestEliteLevel)
                        {
                            highestEliteLevel = traveler.MileagePlus.CurrentEliteLevel;
                        }
                        if (traveler.Seats != null && traveler.Seats.Count > 0)
                        {
                            foreach (var seat in traveler.Seats)
                            {
                                lstSeats.Add(seat);
                                if (seat.Price > 0)
                                {
                                    if (!hasSeatPrice)
                                    {
                                        hasSeatPrice = true;
                                    }
                                    allSeats.Add(seat);
                                }
                            }
                        }


                        //traveler.MileagePlus.TravelBankAccountNumber = traveler.MileagePlus.TravelBankAccountNumber == string.Empty ? null : traveler.MileagePlus.TravelBankAccountNumber;
                        //traveler.MileagePlus.VendorCode = traveler.MileagePlus.VendorCode == string.Empty ? null : traveler.MileagePlus.VendorCode;

                        //traveler.MileagePlus.TravelBankCurrencyCode = traveler.MileagePlus.TravelBankCurrencyCode == string.Empty ? null : traveler.MileagePlus.TravelBankCurrencyCode;
                        //traveler.MileagePlus.TravelBankExpirationDate = traveler.MileagePlus.TravelBankExpirationDate == string.Empty ? null : traveler.MileagePlus.TravelBankExpirationDate;

                    }


                    if (highestEliteLevel <= 2)
                    {
                        foreach (MOBCPTraveler traveler in persistedReservation.TravelersCSL.Values)
                        {
                            if (traveler.Subscriptions != null)
                            {
                                hasSubscriptions = true;
                            }
                        }
                    }

                    // Kiran - Fix for bug AB-1724 [Saved travelers are not getting subscriptions]
                    // If not have elite level or subscriptions finding any mileage plus member is there in saved traveler to get benifits
                    if (_configuration.GetValue<bool>("EnableSubscriptionsForMPSavedTravelerBooking") && highestEliteLevel == 0 && !hasSubscriptions && !hasMPSavedTraveler
                        && persistedReservation.TravelersCSL.Any() && persistedReservation.TravelersCSL.Count > 1 && !persistedReservation.TravelerKeys.IsNullOrEmpty())
                    {
                        foreach (string travelerKey in persistedReservation.TravelerKeys)
                        {
                            MOBCPTraveler bookingTravelerInfo = persistedReservation.TravelersCSL[travelerKey];
                            if (bookingTravelerInfo.AirRewardPrograms != null && bookingTravelerInfo.AirRewardPrograms.Any())
                            {
                                bookingTravelerInfo.AirRewardPrograms.ForEach(p =>
                                {
                                    if (!p.IsNullOrEmpty() && !p.CarrierCode.IsNullOrEmpty() && p.CarrierCode.Trim().ToUpper() == "UA" && !hasMPSavedTraveler)
                                    {
                                        hasMPSavedTraveler = !p.MemberId.IsNullOrEmpty() ? true : false;
                                    }
                                });
                            }
                        }
                    }
                }


                if (hasSeatPrice && (highestEliteLevel > 0 || hasSubscriptions || hasMPSavedTraveler))
                {

                    var tupleResponse = await GetFinalTravelerSeatPrices(request.LanguageCode, response.Seats, request.SessionId, lstSeats, request.Application.Id, request.Application.Version.Major, request.DeviceId, request.SponsorMPAccountId, request.SponsorEliteLevel);
                    response.SeatPrices = tupleResponse.SeatPrice;
                    lstSeats = tupleResponse.lstSeats;
                    persistedReservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, persistedReservation.ObjectName, new List<string> { request.SessionId, persistedReservation.ObjectName }).ConfigureAwait(false);
                    persistedReservation.SeatPrices = response.SeatPrices;
                   await _sessionHelperService.SaveSession<Reservation>(persistedReservation, request.SessionId, new List<string> { request.SessionId, persistedReservation.ObjectName }, persistedReservation.ObjectName).ConfigureAwait(false);
                }
                else
                {
                    if (allSeats.Count > 0)
                    {
                        if (EnableAdvanceSearchCouponBooking(request.Application.Id, request.Application.Version.Major))
                        {
                            MOBShoppingCart persistShoppingCart = new MOBShoppingCart();
                            persistShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, persistShoppingCart.ObjectName, new List<string> { request.SessionId, persistShoppingCart.ObjectName }).ConfigureAwait(false);
                            if (IsAFSSeatCouponApplied(persistShoppingCart))
                            {
                                var tupleResponse = await GetFinalTravelerSeatPrices(request, request.LanguageCode, response.Seats, request.SessionId);
                                response.SeatPrices = tupleResponse.response;
                                lstSeats = tupleResponse.lstSeats;
                            }
                            else
                            {
                                List<MOBSeatPrice> seatPrices = BuildSeatPricesForPriceBreakDown(allSeats);
                                response.SeatPrices = SortAndOrderSeatPrices(seatPrices);
                            }
                        }
                        else
                        {
                            List<MOBSeatPrice> seatPrices = BuildSeatPricesForPriceBreakDown(allSeats);
                            response.SeatPrices = SortAndOrderSeatPrices(seatPrices);
                        }
                        persistedReservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, persistedReservation.ObjectName, new List<string> { request.SessionId, persistedReservation.ObjectName }).ConfigureAwait(false);
                        persistedReservation.SeatPrices = response.SeatPrices;
                        await _sessionHelperService.SaveSession<Reservation>(persistedReservation, request.SessionId, new List<string> { request.SessionId, persistedReservation.ObjectName }, persistedReservation.ObjectName).ConfigureAwait(false);
                    }
                    else
                    {
                        if (_configuration.GetValue<bool>("IsEnableAFSFreeSeatCoupon") && EnableAdvanceSearchCouponBooking(request.Application.Id, request.Application.Version.Major))
                        {
                            MOBShoppingCart persistShoppingCart = new MOBShoppingCart();
                            persistShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, persistShoppingCart.ObjectName, new List<string> { request.SessionId, persistShoppingCart.ObjectName }).ConfigureAwait(false);
                            bool isAFSCouponApplied = IsAFSSeatCouponApplied(persistShoppingCart);
                            if (isAFSCouponApplied)
                            {
                                var tupleResponse = await GetFinalTravelerSeatPrices(request, request.LanguageCode, response.Seats, request.SessionId);
                                response.SeatPrices = tupleResponse.response;
                                lstSeats = tupleResponse.lstSeats;
                            }
                            persistedReservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, persistedReservation.ObjectName, new List<string> { request.SessionId, persistedReservation.ObjectName }).ConfigureAwait(false);
                            persistedReservation.SeatPrices = isAFSCouponApplied ? response.SeatPrices : null;
                           await _sessionHelperService.SaveSession<Reservation>(persistedReservation, request.SessionId, new List<string> { request.SessionId, persistedReservation.ObjectName }, persistedReservation.ObjectName).ConfigureAwait(false);
                        }
                        else
                        {
                            persistedReservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, persistedReservation.ObjectName, new List<string> { request.SessionId, persistedReservation.ObjectName }).ConfigureAwait(false);
                            persistedReservation.SeatPrices = new List<MOBSeatPrice>();
                           await _sessionHelperService.SaveSession<Reservation>(persistedReservation, request.SessionId, new List<string> { request.SessionId, persistedReservation.ObjectName }, persistedReservation.ObjectName).ConfigureAwait(false);
                        }
                    }
                }

                var persistSelectTripObj = await _sessionHelperService.GetSession<SelectTrip>(request.SessionId, new SelectTrip().ObjectName, new List<string> { request.SessionId, new SelectTrip().ObjectName }).ConfigureAwait(false);
                if (persistSelectTripObj != null)
                {
                    response.Reservation = persistSelectTripObj.Responses[persistSelectTripObj.LastSelectTripKey].Availability.Reservation;
                    response.Reservation.Prices = persistedReservation.Prices;
                }

                if (hasSeatPrice && !CompareAndVerifySeatPrices(response, lstSeats))
                {
                    // logEntries.Add(LogEntry.GetLogEntry<string>(request.SessionId, "CompleteSeats", "Trace", request.Application.Id, request.Application.Version.Major, request.DeviceId, "CompleteSeats failed as the total seats price is not equal to sum of Individual Seats Price."));
                    throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
            }
            else
            {
                persistedReservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, persistedReservation.ObjectName, new List<string> { request.SessionId, persistedReservation.ObjectName }).ConfigureAwait(false);
            }
            var hasOaSegment = persistedReservation.Trips.Any(t => t.FlattenedFlights.Any(ff => ff.Flights.Any(f => IsSeatMapSupportedOa(f.OperatingCarrier, f.MarketingCarrier))));

            bool isRegister = true;
            
            if (persistedReservation.IsSSA && (persistedReservation.IsELF || IsIBE(persistedReservation)) || hasOaSegment)
            {
                var tupleResponse = await RegisterSeatsHandleElf(request, session, persistedReservation, response, isClearSeats);
                isRegister = tupleResponse.Item1;
                response = tupleResponse.registerSeatsResponse;
            }
            else
            {
                var tupleResponse = await RegisterSeats(request, session, persistedReservation, response, isClearSeats);
                isRegister = tupleResponse.Item1;
                response = tupleResponse.registerSeatsResponse;
            }

            if (!isRegister)
            {
                if (EnableAdvanceSearchCouponBooking(request.Application.Id, request.Application.Version.Major) && response.PromoCodeRemoveAlertForProducts != null)
                {
                    persistedReservation.ShopReservationInfo2.SeatRemoveCouponPopupCount += 1;
                   await _sessionHelperService.SaveSession<Reservation>(persistedReservation, request.SessionId, new List<string> { request.SessionId, persistedReservation.ObjectName }, persistedReservation.ObjectName).ConfigureAwait(false);

                    return response;
                }

                // logEntries.Add(LogEntry.GetLogEntry<string>(request.SessionId, "RegisterSeats", "Trace", request.Application.Id, request.Application.Version.Major, request.DeviceId, "RegisterSeats failed."));
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }



            persistedReservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, persistedReservation.ObjectName, new List<string> { request.SessionId, persistedReservation.ObjectName }).ConfigureAwait(false);

            if (persistedReservation != null)
            {
                persistedReservation.Prices = response.Reservation.Prices;
                if (EnableAdvanceSearchCouponBooking(request.Application.Id, request.Application.Version.Major))
                {
                    persistedReservation.SeatPrices = response.Reservation.SeatPrices;
                }
                if (persistedReservation.TravelersCSL != null)
                {
                    List<MOBCPTraveler> lstTravelers = new List<MOBCPTraveler>();
                    foreach (string travelerKey in persistedReservation.TravelerKeys)
                    {
                        lstTravelers.Add(persistedReservation.TravelersCSL[travelerKey]);
                    }
                    response.Reservation.TravelersCSL = lstTravelers;
                }
                response.Reservation.CreditCards = persistedReservation.CreditCards;
                response.Reservation.CartId = persistedReservation.CartId;
                response.Reservation.IsSignedInWithMP = persistedReservation.IsSignedInWithMP;
                response.Reservation.TravelersRegistered = persistedReservation.TravelersRegistered;
                response.Reservation.NumberOfTravelers = persistedReservation.NumberOfTravelers;
                if (persistedReservation.LMXTravelers != null)
                {
                    response.Reservation.lmxtravelers = persistedReservation.LMXTravelers == null ? persistedReservation.LMXTravelers = new List<MOBLMXTraveler>() : persistedReservation.LMXTravelers;
                }
            }
            if (persistedReservation.ShopReservationInfo2 != null && !(_shoppingCartUtility.IsEnableOmniCartMVP2Changes(request.Application.Id, request.Application.Version.Major, true) && request.IsRemove))
            {
                persistedReservation.ShopReservationInfo2.NextViewName = "RTI";
                persistedReservation.ShopReservationInfo2.IsForceSeatMapInRTI = false;
                //The  below code is to show the back button in  Travel option and Seats pages
                persistedReservation.ShopReservationInfo2.ShouldHideBackButton = false;

            }


            #region 1127 - Chase Offer (Booking)
            // Code is moved to utility so that it can be reused across different actions
            if (_shoppingCartUtility.EnableChaseOfferRTI(request.Application.Id, request.Application.Version.Major))
                _shoppingCartUtility.UpdateChaseCreditStatement(persistedReservation);
            #endregion 1127 - Chase Offer (Booking)
            if (_shoppingCartUtility.IsEnableOmniCartMVP2Changes(request.Application.Id, request.Application.Version.Major, persistedReservation?.ShopReservationInfo2?.IsDisplayCart == true))
            {
                PopulateSelectedAncillaries(persistedReservation, request, request.SessionId);
            }

            await _sessionHelperService.SaveSession<Reservation>(persistedReservation, request.SessionId, new List<string> { request.SessionId, persistedReservation.ObjectName }, persistedReservation.ObjectName).ConfigureAwait(false);

            if (Convert.ToBoolean(_configuration.GetValue<string>("HazMatOn")))
            {
                response.HazMat = GetHazMat();
            }
            else
            {
                response.HazMat = null;
            }

            return response;
        }

        private bool IsIBE(Reservation persistedReservation)
        {
            if (_configuration.GetValue<bool>("EnablePBE") && (persistedReservation.ShopReservationInfo2 != null))
            {
                return persistedReservation.ShopReservationInfo2.IsIBE;
            }
            return false;
        }

        private string GetFareBasicCodeFromBundles(List<MOBSHOPTravelOption> travelOptions, int tripId, string defaultFareBasisCode, string destination, string origin)
        {
            // string strSegmentname = origin + " - " + destination;
            if (travelOptions == null || travelOptions.Count <= 0)
                return defaultFareBasisCode;

            foreach (var travelOption in travelOptions)
            {
                foreach (var bundleCode in travelOption.BundleCode)
                {
                    if (bundleCode.AssociatedTripIndex == tripId && !string.IsNullOrEmpty(bundleCode.ProductKey))
                    {
                        return bundleCode.ProductKey;
                    }

                }
            }

            return defaultFareBasisCode;
        }

        private Collection<Characteristic> GetTourCode(MOBSHOPReservationInfo2 shopReservationInfo2)
        {
            if (!_configuration.GetValue<bool>("SendTourCodeToSeatEngine"))
                return null;

            if (shopReservationInfo2 == null || shopReservationInfo2.Characteristics == null || !shopReservationInfo2.Characteristics.Any())
                return null;

            var tourCode = shopReservationInfo2.Characteristics.Where(c => c != null && c.Id != null && c.Id.Equals("tourboxcode", StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            if (tourCode == null || string.IsNullOrEmpty(tourCode.CurrentValue))
                return null;

            return new Collection<Characteristic> { new Characteristic { Code = "tourboxcode", Value = tourCode.CurrentValue.Trim() } };
        }

        private async Task<(List<MOBSeatPrice> SeatPrice, List<MOBSeat> lstSeats)> GetFinalTravelerSeatPrices(string languageCode, List<MOBSeat> seats, string sessionID, List<MOBSeat> lstSeats, int applicationId, string appVersion, string deviceId, string sponsorMPAccountId, string sponsorEliteLevel)
        {
            Reservation persistedReservation = await _sessionHelperService.GetSession<Reservation>(sessionID, new Reservation().ObjectName, new List<string> { sessionID, new Reservation().ObjectName }).ConfigureAwait(false);

            #region
            List<SeatDetail> objSeatEngineSeatInfoList = new List<SeatDetail>();
            List<MOBSeatPrice> seatPrices = new List<MOBSeatPrice>();
            List<MOBSeatPrice> tempSeatPrices = new List<MOBSeatPrice>();
            United.Service.Presentation.FlightRequestModel.SeatMap request = new Service.Presentation.FlightRequestModel.SeatMap();
            request.Characteristics = GetTourCode(persistedReservation.ShopReservationInfo2);

            #endregion
            int loggingIndex = 1;
            foreach (MOBSHOPTrip objTrip in persistedReservation.Trips)
            {
                #region
                foreach (MOBSHOPFlattenedFlight objSegment in objTrip.FlattenedFlights)
                {
                    foreach (MOBSHOPFlight segment in objSegment.Flights)
                    {
                        IEnumerable<MOBSeat> seatsList = from s in seats
                                                         where s.Origin.ToUpper().Trim() == segment.Origin.ToUpper().Trim() &&
                                                         s.Destination.ToUpper().Trim() == segment.Destination.ToUpper().Trim()
                                                         select s;

                        #region
                        if (seatsList.Count() > 0)
                        {

                            List<MOBSeat> _seatsList = new List<MOBSeat>();
                            _seatsList = seatsList.ToList();
                            request.Travelers = new Collection<ProductTraveler>();


                            #region
                            if (persistedReservation.TravelersCSL != null && persistedReservation.TravelersCSL.Count > 0)
                            {
                                int i = 0;
                                //foreach (BookingTravelerInfo bookingTravelerInfo in bookingTravelerInfoList)
                                foreach (string travelerKey in persistedReservation.TravelerKeys)
                                {
                                    MOBCPTraveler bookingTravelerInfo = persistedReservation.TravelersCSL[travelerKey];

                                    if (EnableTravelerTypes(applicationId, appVersion, persistedReservation.IsReshopChange) &&
                                        persistedReservation.ShopReservationInfo2 != null && persistedReservation.ShopReservationInfo2.TravelerTypes != null && persistedReservation.ShopReservationInfo2.TravelerTypes.Count > 0)
                                    {
                                        if (bookingTravelerInfo.TravelerTypeCode.ToUpper().Equals("INF"))
                                        {
                                            i++;
                                            continue;
                                        }
                                    }

                                    #region
                                    ProductTraveler travelerInformation = new ProductTraveler();
                                    travelerInformation.GivenName = bookingTravelerInfo.FirstName;
                                    travelerInformation.Surname = bookingTravelerInfo.LastName;
                                    // travelerInformation.Index = Convert.ToUInt16(i);
                                    //travelerInformation.TravelerNumber = i + 1;
                                    travelerInformation.TravelerNameIndex = (i + 1).ToString() + ".1";
                                    if (bookingTravelerInfo.AirRewardPrograms != null && bookingTravelerInfo.AirRewardPrograms.Count > 0)
                                    {
                                        travelerInformation.LoyaltyProgramProfile = new LoyaltyProgramProfile();
                                        travelerInformation.LoyaltyProgramProfile.LoyaltyProgramMemberID = string.Empty;
                                        travelerInformation.LoyaltyProgramProfile.LoyaltyProgramCarrierCode = string.Empty;
                                        foreach (MOBBKLoyaltyProgramProfile loyaltyProfile in bookingTravelerInfo.AirRewardPrograms)
                                        {
                                            if (loyaltyProfile.CarrierCode.Trim().ToUpper() == "UA")
                                            {
                                                travelerInformation.LoyaltyProgramProfile.LoyaltyProgramMemberID = loyaltyProfile.MemberId;
                                                travelerInformation.LoyaltyProgramProfile.LoyaltyProgramCarrierCode = loyaltyProfile.CarrierCode.Trim().ToUpper();
                                            }
                                        }
                                    }
                                    if (bookingTravelerInfo.Subscriptions != null)
                                    {
                                        travelerInformation.Subscriptions = new Collection<Subscription>();
                                        Subscription subscription = new Subscription();
                                        if (bookingTravelerInfo.Subscriptions.SubscriptionTypes != null && bookingTravelerInfo.Subscriptions.SubscriptionTypes.Count() > 0)
                                        {

                                            foreach (MOBUASubscription objUASubscription in bookingTravelerInfo.Subscriptions.SubscriptionTypes)
                                            {
                                                foreach (MOBItem item in objUASubscription.Items)
                                                {

                                                    if (item.Id.Trim().ToUpper() == "EPlusSubscribeCompanionCount".ToUpper())
                                                    {
                                                        subscription.CompanionCount = Convert.ToInt32(item.CurrentValue.Trim());
                                                        subscription.Destination = segment.Destination;
                                                        subscription.Origin = segment.Origin;
                                                        subscription.FlightNumber = segment.FlightNumber;
                                                    }

                                                }
                                            }
                                        }
                                    }
                                    travelerInformation.Seats = new Collection<SeatDetail>();
                                    SeatDetail seatDetail = new SeatDetail();
                                    seatDetail.DepartureAirport = new Service.Presentation.CommonModel.AirportModel.Airport();
                                    seatDetail.DepartureAirport.IATACode = segment.Origin;
                                    seatDetail.ArrivalAirport = new Service.Presentation.CommonModel.AirportModel.Airport();
                                    seatDetail.ArrivalAirport.IATACode = segment.Destination;
                                    seatDetail.FlightNumber = segment.FlightNumber;
                                    seatDetail.DepartureDate = segment.DepartureDateTime;
                                    if (_seatsList[i].SeatAssignment.Trim() != string.Empty)
                                    {
                                        seatDetail.Seat = new Service.Presentation.CommonModel.AircraftModel.Seat();
                                        seatDetail.Seat.Identifier = _seatsList[i].SeatAssignment.Trim();
                                        seatDetail.Seat.Characteristics = new Collection<Characteristic>();

                                        Characteristic seatCharacter = new Characteristic();
                                        seatCharacter.Code = "DisplaySeatType";
                                        seatCharacter.Value = _seatsList[i].SeatType;
                                        seatDetail.Seat.Characteristics.Add(seatCharacter);

                                        seatDetail.Seat.Price = new ProductPrice();
                                        seatDetail.Seat.Price.PromotionCode = _seatsList[i].ProgramCode;
                                        seatDetail.Seat.Price.Totals = new Collection<Charge>();
                                        Charge seatPrice = new Charge();
                                        seatPrice.Amount = Convert.ToDouble(_seatsList[i].Price);
                                        seatPrice.Currency = new Currency();
                                        seatPrice.Currency.Code = _seatsList[i].Currency;
                                        seatDetail.Seat.Price.Totals.Add(seatPrice);


                                    }
                                    else
                                    {
                                        seatDetail.Seat = new Service.Presentation.CommonModel.AircraftModel.Seat();
                                        seatDetail.Seat.Identifier = string.Empty;
                                    }
                                    travelerInformation.Seats.Add(seatDetail);
                                    request.Travelers.Add(travelerInformation);
                                    i += 1;
                                    #endregion
                                }

                                if (_configuration.GetValue<bool>("EnableCSL30BookingReshopSelectSeatMap") == false)
                                {
                                    #region

                                    //DateTime departureDateTime = DateTime.ParseExact(segment.DepartDate + " " + segment.DepartTime, "ddd., MMM. dd, yyyy h:m tt", CultureInfo.InvariantCulture);
                                    DateTime departureDateTime = DateTime.ParseExact(Convert.ToDateTime(segment.DepartDate).ToShortDateString() + " " + segment.DepartTime, "M/d/yyyy h:mtt", CultureInfo.InvariantCulture);

                                    request.Rules = new Collection<SeatRuleParameter>();
                                    SeatRuleParameter seatRule = new SeatRuleParameter();
                                    seatRule.ClassOfService = segment.ServiceClass;

                                    if (ConfigUtility.EnableIBEFull())
                                    {
                                        if (segment.ShoppingProducts != null && segment.ShoppingProducts.Any())
                                            seatRule.ProductCode = segment.ShoppingProducts[0].ProductCode;

                                        seatRule.FareBasisCode = segment.FareBasisCode;
                                        seatRule.BundleCode = GetFareBasicCodeFromBundles(persistedReservation.TravelOptions, segment.TripIndex, null, segment.Destination, segment.Origin);
                                    }
                                    else
                                    {
                                        seatRule.FareBasisCode = segment.FareBasisCode;
                                    }

                                    seatRule.IsInCabinPet = "false";
                                    seatRule.IsLapChild = "false";
                                    seatRule.IsOxygen = "false";
                                    seatRule.IsServiceAnimal = "false";
                                    seatRule.IsUnaccompaniedMinor = "false";
                                    seatRule.hasSSR = "false";
                                    seatRule.IsAwardTravel = "false";
                                    seatRule.LanguageCode = string.Empty;
                                    if (EnableTravelerTypes(applicationId, appVersion, persistedReservation.IsReshopChange) &&
                                        persistedReservation != null && persistedReservation.ShopReservationInfo2 != null && persistedReservation.ShopReservationInfo2.TravelerTypes != null
                                        && persistedReservation.ShopReservationInfo2.TravelerTypes.Count > 0)
                                    {
                                        if (persistedReservation.ShopReservationInfo2.DisplayTravelTypes.Any(t => t.TravelerType.ToString().Equals(MOBPAXTYPE.InfantLap.ToString()) || t.TravelerType.ToString().Equals(MOBPAXTYPE.InfantSeat.ToString())))
                                            seatRule.IsLapChild = "true";

                                        seatRule.PassengerCount = persistedReservation.ShopReservationInfo2.DisplayTravelTypes.Where(t => !t.PaxType.ToUpper().Equals("INF")).Count();

                                    }
                                    else
                                    {
                                        seatRule.PassengerCount = persistedReservation.TravelersCSL.Count;
                                    }
                                    //seatRule.PassengerCount = persistedReservation.TravelersCSL.Count();
                                    seatRule.PassengerClass = string.Empty;
                                    seatRule.Segment = segment.Origin + segment.Destination;

                                    seatRule.Flight = new FlightProfile();
                                    seatRule.Flight.ArrivalAirport = segment.Destination;
                                    seatRule.Flight.ArrivalDate = Convert.ToDateTime(segment.DestinationDate).ToString("yyyy-MM-dd");
                                    seatRule.Flight.DepartureAirport = segment.Origin;
                                    seatRule.Flight.DepartureDate = segment.DepartureDateTime; // Convert.ToDateTime(segment.DepartDate).ToString("yyyy-MM-dd"); We need to pass exact date time
                                    seatRule.Flight.FlightNumber = segment.FlightNumber.Trim();
                                    request.Rules.Add(seatRule);
                                    //As per kent for his Aug 23 2013 push using ME to get Seat Prices
                                    //•	Need to set the same rules for call to get seat map with rules and apply traveler companion rules
                                    #endregion


                                    #region Award Travel Companion EPlus Seats Fix

                                    ShoppingResponse shop = new ShoppingResponse();
                                    shop =await _sessionHelperService.GetSession<ShoppingResponse>(sessionID, shop.ObjectName, new List<string> { sessionID, shop.ObjectName }).ConfigureAwait(false);
                                    Reservation reservation = new Reservation();
                                    reservation = await _sessionHelperService.GetSession<Reservation>(sessionID, reservation.ObjectName, new List<string> { sessionID, reservation.ObjectName }).ConfigureAwait(false);
                                    if (shop != null)
                                    {
                                        if (shop.Request.AwardTravel && !string.IsNullOrEmpty(shop.Request.MileagePlusAccountNumber))
                                        {
                                            bool profileOwnerFound = false;
                                            if (reservation != null && reservation.TravelersCSL != null && reservation.TravelersCSL.Count > 0)
                                            {
                                                foreach (var traveler in reservation.TravelersCSL)
                                                {
                                                    if (traveler.Value.IsProfileOwner || (traveler.Value.MileagePlus != null && !string.IsNullOrEmpty(traveler.Value.MileagePlus.MileagePlusId) && traveler.Value.MileagePlus.MileagePlusId.ToUpper().Equals(shop.Request.MileagePlusAccountNumber.ToUpper())))
                                                    {
                                                        profileOwnerFound = true;
                                                        break;
                                                    }
                                                }
                                            }
                                            if (!reservation.IsReshopChange)
                                            {
                                                if (!profileOwnerFound)
                                                {
                                                    ProductTraveler travelerInformation = new ProductTraveler();
                                                    travelerInformation.LoyaltyProgramProfile = new LoyaltyProgramProfile();
                                                    travelerInformation.LoyaltyProgramProfile.LoyaltyProgramMemberID = shop.Request.MileagePlusAccountNumber;
                                                    travelerInformation.LoyaltyProgramProfile.LoyaltyProgramCarrierCode = "UA";
                                                    travelerInformation.Type = "Sponsor";
                                                    travelerInformation.GivenName = "Test";
                                                    travelerInformation.Surname = "Test";

                                                    request.Travelers.Add(travelerInformation);
                                                }
                                            }
                                        }
                                    }
                                    #endregion

                                    #region "GetFinalTravelerSeatPricesWithAllFares"

                                    string jsonRequest = JsonConvert.SerializeObject(request);
                                    string xmlRequest = XmlSerializerHelper.Serialize(request);

                                    Session session = new Session();
                                    session = await _sessionHelperService.GetSession<Session>(sessionID, session.ObjectName, new List<string> { sessionID, session.ObjectName }).ConfigureAwait(false);
                                    if (session == null)
                                    {
                                        throw new MOBUnitedException("Could not find your booking session.");
                                    }
                                    string url = persistedReservation.IsSSA ?
                                                 string.Format("GetFinalTravelerSeatPricesWithAllFares") :
                                                 string.Format("GetFinalTravelerSeatPrices");


                                    #endregion//****Get Call Duration Code - Venkat 03/17/2015******* 
                                    loggingIndex++;
                                    var response = await _seatMapService.SeatEngine<United.Service.Presentation.ProductModel.FlightSeatMapDetail>(session.Token, url, jsonRequest, sessionID);

                                    if (response != null && response.Travelers != null && response.Travelers.Count > 0)
                                    {
                                        CultureInfo ci = null;

                                        foreach (ProductTraveler travelerInformation in response.Travelers)
                                        {
                                            if (travelerInformation.Seats != null && travelerInformation.Seats.Count > 0)
                                            {
                                                foreach (SeatDetail seatInfo in travelerInformation.Seats)
                                                {
                                                    objSeatEngineSeatInfoList.Add(seatInfo);
                                                    if (seatInfo.Seat != null && seatInfo.Seat.Price != null)
                                                    {
                                                        if (ci == null)
                                                        {
                                                            ci = GetCultureInfo(seatInfo.Seat.Price.Totals[0].Currency.Code);
                                                        }

                                                        List<MOBSeat> seatlist = seats.Where(s => s.Origin.Equals(segment.Origin, StringComparison.OrdinalIgnoreCase) &&
                                                                                                  s.Destination.Equals(segment.Destination, StringComparison.OrdinalIgnoreCase) &&
                                                                                                  s.SeatAssignment.Equals(seatInfo.Seat.Identifier, StringComparison.OrdinalIgnoreCase)).ToList();

                                                        if (!seatlist.IsNullOrEmpty() && seatlist.FirstOrDefault().Price > 0 && !seatInfo.Seat.Price.Totals.IsNullOrEmpty())
                                                        {
                                                            MOBSeatPrice seatPrice = new MOBSeatPrice();
                                                            seatPrice.Origin = segment.Origin;
                                                            seatPrice.Destination = segment.Destination;
                                                            seatPrice.SeatMessage = GetSeatMessageforSeatPrices(seatInfo.Seat.Characteristics, seatInfo.Seat.Price.PromotionCode);
                                                            seatPrice.NumberOftravelers = 1;
                                                            if (seatInfo.Seat.Price.Totals[0].Amount.Equals(0))
                                                            {
                                                                seatPrice.TotalPrice = seatlist[0].Price;
                                                                seatPrice.DiscountedTotalPrice = 0;
                                                            }
                                                            else
                                                            {
                                                                seatPrice.TotalPrice = Convert.ToDecimal(seatInfo.Seat.Price.Totals[0].Amount);
                                                                seatPrice.DiscountedTotalPrice = Convert.ToDecimal(seatInfo.Seat.Price.Totals[0].Amount);
                                                            }
                                                            seatPrice.CurrencyCode = seatlist[0].Currency;
                                                            seatPrice.TotalPriceDisplayValue = formatAmountForDisplay(seatPrice.TotalPrice, ci, false);
                                                            seatPrice.DiscountedTotalPriceDisplayValue = formatAmountForDisplay(seatPrice.DiscountedTotalPrice, ci, false);
                                                            if (!seatInfo.Seat.IsNullOrEmpty())
                                                                seatPrice.SeatNumbers = new List<string> { seatInfo.Seat.Identifier };

                                                            tempSeatPrices.Add(seatPrice);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region "CSL30 - Start here"                               

                                    if (request?.Travelers?.Count > 0)
                                    {
                                        foreach (ProductTraveler travelerInformation in request.Travelers)
                                        {
                                            if (travelerInformation.Seats != null && travelerInformation.Seats.Count > 0)
                                            {
                                                foreach (SeatDetail seatInfo in travelerInformation.Seats)
                                                {
                                                    var travelCompanionRulesResult = await ApplyTravelerCompanionRulesCSl30(sessionID, seats, segment, seatInfo,
                                                    travelerInformation, objSeatEngineSeatInfoList, tempSeatPrices);
                                                    objSeatEngineSeatInfoList = travelCompanionRulesResult.objSeatEngineSeatInfoList;
                                                    tempSeatPrices = travelCompanionRulesResult.tempSeatPrices;
                                                }
                                            }
                                        }
                                    }
                                    #endregion "CSL30 - Start here"
                                }
                            }
                            #endregion
                        }
                        #endregion
                    }

                }
            }

            AddMissingSeatAssignment(tempSeatPrices, persistedReservation);
            #region
            if (tempSeatPrices.Count > 0)
            {
                seatPrices = new List<MOBSeatPrice>();
                string origin = tempSeatPrices[0].Origin;
                string destination = tempSeatPrices[0].Destination;

                CultureInfo ci = null;

                foreach (MOBSeatPrice seatPrice in tempSeatPrices)
                {
                    if (ci == null)
                    {
                        ci = GetCultureInfo(seatPrice.CurrencyCode);
                    }

                    if (seatPrice.Origin.Equals(origin) && seatPrice.Destination.Equals(destination))
                    {
                        var item = seatPrices.Find(s => s.Origin == seatPrice.Origin
                                                     && s.Destination == seatPrice.Destination
                                                     && s.SeatMessage == seatPrice.SeatMessage);

                        if (item != null)
                        {
                            item.NumberOftravelers = item.NumberOftravelers + 1;
                            item.TotalPrice = item.TotalPrice + seatPrice.TotalPrice;
                            item.TotalPriceDisplayValue = formatAmountForDisplay(item.TotalPrice, ci, false);
                            item.DiscountedTotalPrice = item.DiscountedTotalPrice + seatPrice.DiscountedTotalPrice;
                            item.DiscountedTotalPriceDisplayValue = formatAmountForDisplay(item.DiscountedTotalPrice, ci, false);
                            if (item.SeatNumbers.IsNullOrEmpty())
                                item.SeatNumbers = new List<string>();
                            if (!seatPrice.SeatNumbers.IsNullOrEmpty())
                                item.SeatNumbers.AddRange(seatPrice.SeatNumbers);
                        }
                        else
                        {
                            MOBSeatPrice sp = BuildSeatPricesWithDiscountedPrice(seatPrice);
                            if (!sp.IsNullOrEmpty())
                                seatPrices.Add(sp);
                        }
                    }
                    else
                    {
                        origin = seatPrice.Origin;
                        destination = seatPrice.Destination;

                        MOBSeatPrice sp = BuildSeatPricesWithDiscountedPrice(seatPrice);
                        if (!sp.IsNullOrEmpty())
                            seatPrices.Add(sp);
                    }
                }
            }
            #endregion
            lstSeats = await UpdateSeatPricesAfterApplyTravelerCompanionCall(sessionID, objSeatEngineSeatInfoList);

            seatPrices = SortAndOrderSeatPrices(seatPrices);
            Reservation reservationAfterUpdatingSeatPrice = await _sessionHelperService.GetSession<Reservation>(sessionID, new Reservation().ObjectName, new List<string> { sessionID, new Reservation().ObjectName }).ConfigureAwait(false);
            reservationAfterUpdatingSeatPrice.SeatPrices = seatPrices;
           await _sessionHelperService.SaveSession<Reservation>(reservationAfterUpdatingSeatPrice, sessionID, new List<string> { sessionID, reservationAfterUpdatingSeatPrice.ObjectName }, reservationAfterUpdatingSeatPrice.ObjectName).ConfigureAwait(false);

            return (seatPrices, lstSeats);
        }

        private bool IsPreferredSeat(Collection<Characteristic> characteristics, string program)
        {
            return IsPreferredSeatProgramCode(program) && IsPreferredSeatCharacteristics(characteristics);
        }

        private string GetSeatMessageforSeatPricesWithLimitedRecline(Collection<Characteristic> characteristics, string programCode, string displaySeatCategory = "", bool limitedRecline = false)
        {
            string seatMsg = "Economy Plus Seat";

            if (IsEMinusSeat(programCode))
                seatMsg = "Advance Seat Assignment";

            if (_configuration.GetValue<bool>("isEnablePreferredZone") && IsPreferredSeat(characteristics, programCode))
            {
                seatMsg = _configuration.GetValue<string>("PreferedSeat_PriceBreakdownTitle");
            }
            if (limitedRecline)
                seatMsg = seatMsg + _configuration.GetValue<string>("NoOrLimitedReclineMessage");

            return seatMsg;
        }

        private bool IsPreferredSeatCharacteristics(Collection<Characteristic> characteristics)
        {
            if (characteristics != null && characteristics.Count > 0)
            {
                string preferredSeatChar = _configuration.GetValue<string>("PreferredSeatBooleanCharacteristic") ?? string.Empty;
                var preferredSeatCharList = preferredSeatChar.Split('|');
                foreach (Characteristic characteristic in characteristics)
                {
                    if (characteristic != null && !string.IsNullOrEmpty(characteristic.Code) && !string.IsNullOrEmpty(characteristic.Value))
                    {
                        if (preferredSeatCharList.Any(s => s.Trim().Equals(characteristic.Code, StringComparison.OrdinalIgnoreCase)) && characteristic.Value.ToUpper().Trim() == "TRUE" ||
                            (characteristic.Code.Equals("DisplaySeatType") && IsPreferredSeatDisplayType(characteristic.Value)))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private string GetSeatMessageforSeatPrices(Collection<Characteristic> characteristics, string programCode, string displaySeatCategory = "", bool limitedRecline = false)
        {
            if (_configuration.GetValue<bool>("EnableLimitedReclineAllProducts"))
            {
                return GetSeatMessageforSeatPricesWithLimitedRecline(characteristics, programCode, displaySeatCategory, limitedRecline);
            }
            if (IsEMinusSeat(programCode))
                return "Advance Seat Assignment";

            if (_configuration.GetValue<bool>("isEnablePreferredZone") && IsPreferredSeat(characteristics, programCode))
            {
                return _configuration.GetValue<string>("PreferedSeat_PriceBreakdownTitle");
            }

            if (_configuration.GetValue<bool>("EnableCSL30BookingReshopSelectSeatMap") && !displaySeatCategory.IsNullOrEmpty())
            {
                return (isLimitedRecline(displaySeatCategory)) ? "Economy Plus Seat (limited recline)" : "Economy Plus Seat";
            }
            else if (!characteristics.IsNullOrEmpty())
            {
                if (CheckSeatRRowType(characteristics, "IsEconomyPlusPrimeSeat") || CheckSeatRRowType(characteristics, "IsPrimeSeat") || CheckSeatRRowType(characteristics, "IsBulkheadPrime")) // As Kent replied email about Limited Reclined codes
                {
                    return "Economy Plus Seat (limited recline)";
                }
                return "Economy Plus Seat";
            }
            return null;
        }

        private bool CheckSeatRRowType(Collection<United.Service.Presentation.CommonModel.Characteristic> seatCharacteristics, string code)
        {
            bool seatRRowType = false;
            if (seatCharacteristics != null && seatCharacteristics.Count > 0)
            {
                foreach (United.Service.Presentation.CommonModel.Characteristic objChar in seatCharacteristics)
                {
                    if (objChar.Code != null && objChar.Code.ToUpper().Trim() == code.ToUpper().Trim())
                    {
                        seatRRowType = true;
                        break;
                    }
                }
            }
            return seatRRowType;
        }

        private bool isLimitedRecline(string displaySeatCategory)
        {
            if (!string.IsNullOrEmpty(displaySeatCategory))
            {
                var limitedReclineCategory = _configuration.GetValue<string>("SelectSeatsLimitedReclineForCSL30").Split('|');
                if (!limitedReclineCategory.IsNullOrEmpty() && limitedReclineCategory.Any())
                {
                    return limitedReclineCategory.Any(x => !x.IsNullOrEmpty() && x.Trim().Equals(displaySeatCategory, StringComparison.OrdinalIgnoreCase));
                }
            }
            return false;
        }

        private async Task<List<MOBSeat>> UpdateSeatPricesAfterApplyTravelerCompanionCall(string sessionID, List<SeatDetail> objSeatEngineSeatInfoList)
        {
            List<MOBSeat> lstSeats = null;
            List<MOBSeat> allSeats = new List<MOBSeat>();

            Reservation persistedReservation = await _sessionHelperService.GetSession<Reservation>(sessionID, new Reservation().ObjectName, new List<string> { sessionID, new Reservation().ObjectName }).ConfigureAwait(false);
            if (objSeatEngineSeatInfoList != null)
            {
                if (persistedReservation != null && persistedReservation.TravelersCSL != null && persistedReservation.TravelersCSL.Count > 0)
                {
                    foreach (string travelerKey in persistedReservation.TravelerKeys)
                    {
                        MOBCPTraveler bookingTravelerInfo = persistedReservation.TravelersCSL[travelerKey];
                        lstSeats = bookingTravelerInfo.Seats;
                        for (int i = 0; i < lstSeats.Count(); i++)
                        {
                            #region

                            IEnumerable<SeatDetail> seatsList = from s in objSeatEngineSeatInfoList
                                                                where s.DepartureAirport.IATACode.ToUpper().Trim() == lstSeats[i].Origin.ToUpper().Trim() &&
                                                                s.ArrivalAirport.IATACode.ToUpper().Trim() == lstSeats[i].Destination.ToUpper().Trim() &&
                                                                s.Seat.Identifier.ToUpper().Trim() == lstSeats[i].SeatAssignment.ToUpper().Trim()
                                                                select s;
                            List<SeatDetail> _objSeatList = new List<SeatDetail>();
                            _objSeatList = seatsList.ToList();
                            if (_objSeatList != null && _objSeatList.Count > 0 && _objSeatList[0].Seat != null && _objSeatList[0].Seat.Price != null && _objSeatList[0].Seat.Price.Totals != null && _objSeatList[0].Seat.Price.Totals.Count > 0)
                            {

                                if (_configuration.GetValue<bool>("EnableAdvanceSearchCouponBooking") && !String.IsNullOrEmpty(_objSeatList[0]?.Seat?.Price.CouponCode))
                                {
                                    lstSeats[i].PriceAfterCouponApplied = Convert.ToDecimal(_objSeatList[0].Seat.Price.Totals[0].Amount);
                                    lstSeats[i].PromotionalCouponCode = _objSeatList[0]?.Seat?.Price?.CouponCode;
                                    lstSeats[i].PriceAfterTravelerCompanionRules = Convert.ToDecimal(_objSeatList[0]?.Seat?.Price?.OriginalPrice);
                                }
                                else
                                {
                                    lstSeats[i].PriceAfterTravelerCompanionRules = Convert.ToDecimal(_objSeatList[0].Seat.Price.Totals[0].Amount);

                                }
                            }

                            #endregion
                        }
                        //Continental.Common.State.InsertState(0, sessionID, "mySeats", 0, Continental.Common.Xml.Serialization.Serialize(lstSeats, typeof(List<Seat>)), "", "");
                        persistedReservation.TravelersCSL[travelerKey].Seats = lstSeats;
                        foreach (MOBSeat seat in lstSeats)
                        {
                            allSeats.Add(seat);
                        }
                    }
                }
            }
            
            await _sessionHelperService.SaveSession<Reservation>(persistedReservation, sessionID, new List<string> { sessionID, persistedReservation.ObjectName }, persistedReservation.ObjectName).ConfigureAwait(false);

            return allSeats;
        }

        private MOBSeatPrice BuildSeatPricesWithDiscountedPrice(MOBSeatPrice seatPrice)
        {
            if (seatPrice.IsNullOrEmpty())
                return null;

            CultureInfo ci = GetCultureInfo(seatPrice.CurrencyCode);
            return new MOBSeatPrice
            {
                Origin = seatPrice.Origin,
                Destination = seatPrice.Destination,
                SeatMessage = seatPrice.SeatMessage,
                NumberOftravelers = 1,
                TotalPrice = seatPrice.TotalPrice,
                TotalPriceDisplayValue = formatAmountForDisplay(seatPrice.TotalPrice, ci, false),
                DiscountedTotalPrice = seatPrice.DiscountedTotalPrice,
                DiscountedTotalPriceDisplayValue = formatAmountForDisplay(seatPrice.DiscountedTotalPrice, ci, false),
                CurrencyCode = seatPrice.CurrencyCode,
                SeatNumbers = seatPrice.SeatNumbers,
                SeatPromoDetails = _configuration.GetValue<bool>("EnableAdvanceSearchCouponBooking") ? seatPrice.SeatPromoDetails : null
            };
        }

        private List<MOBSeatPrice> BuildSeatPricesForPriceBreakDown(List<MOBSeat> seats)
        {
            if (seats.IsNullOrEmpty()) return null;

            seats.RemoveWhere(s => s.Price == 0 && s.PriceAfterTravelerCompanionRules == 0);
            if (seats.IsNullOrEmpty()) return null;

            seats = seats.GroupBy(s => s.Origin + s.Destination).SelectMany(s => s).ToList();
            List<MOBSeatPrice> seatPrices = new List<MOBSeatPrice>();
            var origin = "";
            var destination = "";
            var preferredSeat = _configuration.GetValue<string>("PreferedSeat_PriceBreakdownTitle") ?? string.Empty;
            var limitedReclineText = _configuration.GetValue<string>("NoOrLimitedReclineMessage").ToUpper() ?? string.Empty;
            foreach (var seat in seats)
            {
                if (seat.Origin == origin && seat.Destination == destination)
                {
                    MOBSeatPrice item = null;
                    if (_configuration.GetValue<bool>("EnableLimitedReclineAllProducts"))
                    {

                        item = seatPrices.Find(s => s.Origin == seat.Origin
                                                        && s.Destination == seat.Destination
                                                        && s.SeatMessage.ToUpper().Contains(limitedReclineText) == seat.LimitedRecline
                                                        && s.SeatMessage.ToUpper().Contains("ADVANCE SEAT ASSIGNMENT") == IsEMinusSeat(seat.ProgramCode)
                                                        && s.SeatMessage.ToUpper().Contains(preferredSeat.ToUpper()) == IsPreferredSeat(seat.SeatType, seat.ProgramCode));
                    }
                    else
                    {

                        item = seatPrices.Find(s => s.Origin == seat.Origin
                                                       && s.Destination == seat.Destination
                                                       && s.SeatMessage.ToUpper().Contains("LIMITED RECLINE") == seat.LimitedRecline
                                                       && s.SeatMessage.ToUpper().Contains("ADVANCE SEAT ASSIGNMENT") == IsEMinusSeat(seat.ProgramCode)
                                                       && s.SeatMessage.ToUpper().Contains(preferredSeat.ToUpper()) == IsPreferredSeat(seat.SeatType, seat.ProgramCode));
                    }

                    if (item != null)
                    {
                        UpdateSeatPrices(ref item, seat);
                    }
                    else
                    {
                        MOBSeatPrice sp = BuildSeatPrice(seat);
                        if (!sp.IsNullOrEmpty())
                            seatPrices.Add(sp);
                    }
                }
                else
                {
                    origin = seat.Origin;
                    destination = seat.Destination;
                    MOBSeatPrice sp = BuildSeatPrice(seat);
                    if (!sp.IsNullOrEmpty())
                        seatPrices.Add(sp);
                }
            }

            return seatPrices;
        }

        private bool IsPreferredSeat(string DisplaySeatType, string program)
        {
            return IsPreferredSeatProgramCode(program) && IsPreferredSeatDisplayType(DisplaySeatType);
        }

        private bool IsPreferredSeatDisplayType(string displaySeatType)
        {
            string seatTypes = _configuration.GetValue<string>("PreferredSeatSharesSeatTypes") ?? string.Empty;
            var seatTypesList = seatTypes.Split('|');
            if (!string.IsNullOrEmpty(seatTypes) && !string.IsNullOrEmpty(displaySeatType))
            {
                if (seatTypesList.Any(s => s.Trim().Equals(displaySeatType, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsPreferredSeatProgramCode(string program)
        {
            string preferredProgramCodes = _configuration.GetValue<string>("PreferredSeatProgramCodes");

            if (!string.IsNullOrEmpty(preferredProgramCodes) && !string.IsNullOrEmpty(program))
            {
                string[] codes = preferredProgramCodes.Split('|');

                foreach (string code in codes)
                {
                    if (code.Equals(program))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private string GetSeatMessage(string seatType, string seatFeature, string programCode, bool limitedRecline)
        {
            if (_configuration.GetValue<bool>("EnableLimitedReclineAllProducts"))
                return GetSeatMessageWithLimitRecline(seatType, seatFeature, programCode, limitedRecline);

            if (IsEMinusSeat(programCode))
                return "Advance Seat Assignment";

            if (_configuration.GetValue<bool>("isEnablePreferredZone") && (IsPreferredSeat(seatType, programCode) || IsPreferredSeat(seatFeature, programCode)))
                return _configuration.GetValue<string>("PreferedSeat_PriceBreakdownTitle");

            return limitedRecline ? "Economy Plus Seat (limited recline)"
                                  : "Economy Plus Seat";
        }

        private string GetSeatMessageWithLimitRecline(string seatType, string seatFeature, string programCode, bool limitedRecline)
        {
            string seatMsg = "Economy Plus Seat";

            if (IsEMinusSeat(programCode))
                seatMsg = "Advance Seat Assignment";

            if (_configuration.GetValue<bool>("isEnablePreferredZone") && (IsPreferredSeat(seatType, programCode) || IsPreferredSeat(seatFeature, programCode)))
                seatMsg = _configuration.GetValue<string>("PreferedSeat_PriceBreakdownTitle");

            if (limitedRecline)
                seatMsg = seatMsg + _configuration.GetValue<string>("NoOrLimitedReclineMessage");
            return seatMsg;
        }

        private bool IsEMinusSeat(string programCode)
        {
            if (!_configuration.GetValue<bool>("EnableSSA") || programCode.IsNullOrEmpty())
                return false;

            programCode = programCode.ToUpper().Trim();
            return programCode.Equals("ASA") || programCode.Equals("BSA");
        }

        private MOBSeatPrice BuildSeatPrice(MOBSeat seat)
        {
            if (seat.IsNullOrEmpty())
                return null;
            var ci = GetCultureInfo(seat.Currency);
            var seatMessage = GetSeatMessage(seat.SeatType, seat.SeatFeature, seat.ProgramCode, seat.LimitedRecline);
            return new MOBSeatPrice
            {
                Origin = seat.Origin,
                Destination = seat.Destination,
                SeatMessage = seatMessage,
                NumberOftravelers = 1,
                TotalPrice = seat.Price,
                TotalPriceDisplayValue = formatAmountForDisplay(seat.Price, ci, false),
                DiscountedTotalPrice = seat.PriceAfterTravelerCompanionRules,
                DiscountedTotalPriceDisplayValue = formatAmountForDisplay(seat.PriceAfterTravelerCompanionRules, ci, false),
                CurrencyCode = seat.Currency,
                SeatNumbers = new List<string> { seat.SeatAssignment }
            };
        }


        private void UpdateSeatPrices(ref MOBSeatPrice item, MOBSeat seat)
        {
            if (seat.IsNullOrEmpty())
                return;

            var ci = GetCultureInfo(item.CurrencyCode);

            item.NumberOftravelers = item.NumberOftravelers + 1;
            item.TotalPrice = item.TotalPrice + seat.Price;
            item.DiscountedTotalPrice = item.DiscountedTotalPrice + seat.PriceAfterTravelerCompanionRules;
            item.TotalPriceDisplayValue = formatAmountForDisplay(item.TotalPrice, ci, false);
            item.DiscountedTotalPriceDisplayValue = formatAmountForDisplay(item.DiscountedTotalPrice, ci, false);
            if (item.SeatNumbers.IsNullOrEmpty())
                item.SeatNumbers = new List<string>();
            if (!seat.SeatAssignment.IsNullOrEmpty())
                item.SeatNumbers.Add(seat.SeatAssignment);
        }

        private List<MOBSeatPrice> SortAndOrderSeatPrices(List<MOBSeatPrice> seatPrices)
        {
            List<MOBSeatPrice> economySeatPrices, preferedSeatPrices = null;
            if (seatPrices.IsNullOrEmpty())
                return seatPrices;

            var preferedSeatTitle = _configuration.GetValue<string>("PreferedSeat_PriceBreakdownTitle") ?? "";
            var ci = GetCultureInfo(seatPrices[0].CurrencyCode == null ? seatPrices[0].CurrencyCode : "USD");
            if (_configuration.GetValue<bool>("EnableLimitedReclineAllProducts"))
            {
                economySeatPrices = seatPrices.FindAll(sp => sp.SeatMessage.ToUpper().Contains("ADVANCE SEAT ASSIGNMENT")).Clone();
                preferedSeatPrices = seatPrices.FindAll(sp => sp.SeatMessage.ToUpper().Contains(preferedSeatTitle.ToUpper())).Clone();
                seatPrices.RemoveAll(sp => sp.SeatMessage.ToUpper().Contains("ADVANCE SEAT ASSIGNMENT") || sp.SeatMessage.ToUpper().Contains(preferedSeatTitle.ToUpper()));
            }
            else
            {
                economySeatPrices = seatPrices.FindAll(sp => sp.SeatMessage.Equals("Advance Seat Assignment")).Clone();
                preferedSeatPrices = seatPrices.FindAll(sp => sp.SeatMessage.Equals(preferedSeatTitle, StringComparison.InvariantCultureIgnoreCase)).Clone();
                seatPrices.RemoveAll(sp => sp.SeatMessage.Equals("Advance Seat Assignment") || sp.SeatMessage.Equals(preferedSeatTitle, StringComparison.InvariantCultureIgnoreCase));
            }

            if (!seatPrices.IsNullOrEmpty() && !seatPrices[0].IsNullOrEmpty() && seatPrices[0].NumberOftravelers > 0)
            {
                decimal total = seatPrices.Aggregate<MOBSeatPrice, decimal>(0, (current, sp) => current + sp.DiscountedTotalPrice);
                int noOfTotalSeats = seatPrices.Aggregate(0, (current, sp) => current + sp.NumberOftravelers);
                seatPrices[0].SeatPricesHeaderandTotal = new MOBItem
                {
                    Id = noOfTotalSeats > 1 ? "Economy Plus® Seats" : "Economy Plus® Seat",
                    CurrentValue = formatAmountForDisplay(total, ci, false)
                };
            }

            if (!preferedSeatPrices.IsNullOrEmpty() && !preferedSeatPrices[0].IsNullOrEmpty() && preferedSeatPrices[0].NumberOftravelers > 0)
            {
                decimal total = preferedSeatPrices.Aggregate<MOBSeatPrice, decimal>(0, (current, sp) => current + sp.DiscountedTotalPrice);
                int noOfTotalSeats = preferedSeatPrices.Aggregate(0, (current, sp) => current + sp.NumberOftravelers);
                preferedSeatPrices[0].SeatPricesHeaderandTotal = new MOBItem
                {
                    Id = noOfTotalSeats > 1 ? "Preferred seats" : "Preferred seat",
                    CurrentValue = formatAmountForDisplay(total, ci, false)
                };
                seatPrices.AddRange(preferedSeatPrices);
            }

            if (!economySeatPrices.IsNullOrEmpty() && !economySeatPrices[0].IsNullOrEmpty() && economySeatPrices[0].NumberOftravelers > 0)
            {
                decimal total = economySeatPrices.Aggregate<MOBSeatPrice, decimal>(0, (current, sp) => current + sp.DiscountedTotalPrice);
                int noOfTotalSeats = economySeatPrices.Aggregate(0, (current, sp) => current + sp.NumberOftravelers);
                economySeatPrices[0].SeatPricesHeaderandTotal = new MOBItem
                {
                    Id = noOfTotalSeats > 1 ? "Advance Seat Assignments" : "Advance Seat Assignment",
                    CurrentValue = formatAmountForDisplay(total, ci, false)
                };
                seatPrices.AddRange(economySeatPrices);
            }

            return seatPrices;
        }

        private bool CompareAndVerifySeatPrices(MOBBookingRegisterSeatsResponse reponse, List<MOBSeat> seats)
        {
            if (reponse.Reservation != null && reponse.Reservation.Prices != null && reponse.Seats != null)
            {
                #region
                if (reponse.SeatPrices != null && seats != null)
                {
                    decimal totalDiscountedPrice = 0, totalSeatPrice = 0;
                    foreach (MOBSeatPrice objSeatPrice in reponse.SeatPrices)
                    {
                        totalDiscountedPrice = totalDiscountedPrice + objSeatPrice.DiscountedTotalPrice;
                    }
                    foreach (MOBSeat objSeat in seats)
                    {
                        totalSeatPrice = totalSeatPrice + objSeat.PriceAfterTravelerCompanionRules;
                    }
                    if (totalDiscountedPrice == totalSeatPrice)
                    {
                        return true;
                    }
                }
                #endregion
            }
            return false;
        }

        private async Task<(MOBShoppingCart response, FlightReservationResponse flightReservationResponse)> ClearSeats(MOBRegisterSeatsRequest request, Session session, FlightReservationResponse flightReservationResponse)
        {
            flightReservationResponse = flightReservationResponse ?? new FlightReservationResponse();

            try
            {
                Services.FlightShopping.Common.FlightReservation.RegisterOfferRequest clearSeatsRequest = new Services.FlightShopping.Common.FlightReservation.RegisterOfferRequest();
                clearSeatsRequest.CartId = request.CartId;
                string url = "/cleanupseats";

                string jsonRequest = JsonConvert.SerializeObject(clearSeatsRequest);
                string jsonResponse = await _shoppingCartService.ClearSeats(session.Token, url, jsonRequest, request.SessionId);
                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    flightReservationResponse = JsonConvert.DeserializeObject<FlightReservationResponse>(jsonResponse);

                    if (flightReservationResponse != null && flightReservationResponse.Errors.Count() == 0)
                    {
                        var ret = await _shoppingCartUtility.BuildShoppingCart(request, flightReservationResponse, request.Flow, request.CartId, request.SessionId, session);
                        return (ret, flightReservationResponse);
                    }
                }
            }
            catch (System.Net.WebException wex)
            {
                if (wex.Response != null)
                {
                    _logger.LogError("ClearSeats {@ErrorMessageResponse}", JsonConvert.SerializeObject(wex));
                }
                throw;
            }
            catch (MOBUnitedException uaex)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(uaex.InnerException ?? uaex).Throw();
            }
            catch (System.Exception ex)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException ?? ex).Throw();
            }

            return (null, null);
        }

        private string GetFinalDestinationForThruFlight(string origin, string destination, Reservation reservation)
        {
            string lastDestination = "";
            //Note:If we have same origin and destination in multitrip sceanrio we would still have the issue but that issue would be still there for normal markest as well.
            if (!_configuration.GetValue<bool>("DisableThruFlightSeatNotAssigningFix"))
            {
                if (IsThruFlight(origin, destination, reservation))
                {
                    var stopOverFlight = GetCOGorThruFlight(origin, destination, reservation);

                    lastDestination = GetCOGorThruFlightTrip(origin, destination, reservation)?
                        .FlattenedFlights?.FirstOrDefault()?
                        .Flights?.LastOrDefault(flight => flight?.FlightNumber == stopOverFlight?.FlightNumber
                                                && flight?.EquipmentDisclosures?.EquipmentType == stopOverFlight?.EquipmentDisclosures?.EquipmentType)?.Destination;
                }
            }
            return !string.IsNullOrEmpty(lastDestination) ? lastDestination : destination;
        }

        private bool IsThruFlight(string origin, string destination, Reservation reservation)
        {
            return GetCOGorThruFlightTrip(origin, destination, reservation)?.FlattenedFlights?.FirstOrDefault().Flights?.Any(flight => flight.IsStopOver && !flight.ChangeOfPlane) == true;
        }

        private bool IsCOGFlight(string origin, string destination, Reservation reservation)
        {
            return GetCOGorThruFlightTrip(origin, destination, reservation)?.FlattenedFlights?.FirstOrDefault().Flights?.Any(flight => flight.ChangeOfGauge && flight.ChangeOfPlane) == true;
        }

        private MOBSHOPTrip GetCOGorThruFlightTrip(string origin, string destination, Reservation reservation)
        {
            return reservation?.Trips?.FirstOrDefault(trip => trip?.FlattenedFlights?.FirstOrDefault().Flights?.Any(flight => flight?.Origin == origin && flight.Destination == destination) == true);
        }

        private MOBSHOPFlight GetCOGorThruFlight(string origin, string destination, Reservation reservation)
        {
            return GetCOGorThruFlightTrip(origin, destination, reservation).FlattenedFlights?.FirstOrDefault()?.Flights?.First(flight => flight?.Origin == origin && flight.Destination == destination);
        }

        public async Task<RegisterSeatsRequest> GetRegisterSeatsRequest(MOBRegisterSeatsRequest request, Reservation persistedReservation)
        {
            RegisterSeatsRequest seatRequest = new RegisterSeatsRequest();

            List<United.Service.Presentation.SegmentModel.ReservationFlightSegment>
                segments = JsonConvert.DeserializeObject<List<United.Service.Presentation.SegmentModel.ReservationFlightSegment>>
                (persistedReservation.CSLReservationJSONFormat);

            List<SeatAssignment> seatAssignments = new List<SeatAssignment>();
            SeatAssignment seatAssignment;

            string promoCode = string.Empty;
            bool isAdvanceSearchCoupon = EnableAdvanceSearchCouponBooking(request.Application.Id, request.Application.Version.Major);
            
            seatRequest.CartId = request.CartId;
            if (persistedReservation.IsReshopChange && _configuration.GetValue<bool>("EnableInfantOnLapRegisterSeatChangeRequest"))
            {
                seatRequest.SeatAssignments = GetInfantOnLapRegisterSeatsRequest(segments, persistedReservation, seatAssignments);
                if (_configuration.GetValue<bool>("EnableCSLCloudMigrationToggle"))
                {
                    seatRequest.WorkFlowType = GetWorkFlowType(request.Flow);
                }
                return seatRequest;
            }

            foreach (var traveler in persistedReservation.TravelersCSL.Values)
            {
                if ((!_configuration.GetValue<bool>("TurnOffInfantOnlapRegisterSeatChangeRequestFix") && !traveler.IsEligibleForSeatSelection))
                {
                    continue;
                }
                foreach (var seat in traveler.Seats)
                {
                    string destination = GetFinalDestinationForThruFlight(seat.Origin, seat.Destination, persistedReservation);
                    var segment = segments.FirstOrDefault(s => s.FlightSegment.DepartureAirport.IATACode.ToUpper() == seat.Origin.ToUpper() &&
                                                               s.FlightSegment.ArrivalAirport.IATACode.ToUpper() == destination.ToUpper());
                    if (segment == null) continue;

                    var isOaSegment = IsSeatMapSupportedOa(segment.FlightSegment.OperatingAirlineCode, segment.FlightSegment.MarketedFlightSegment[0] != null ? segment.FlightSegment.MarketedFlightSegment[0].MarketingAirlineCode : string.Empty);
                    var disableFixforBERemovePaidSeat = await _featureSettings.GetFeatureSettingValue("DisableFixforBERemovePaidSeat_MOBILE22711").ConfigureAwait(false);
                    if (!(
                             (
                           (persistedReservation.IsSSA
                             && disableFixforBERemovePaidSeat && (persistedReservation.IsELF || IsIBE(persistedReservation))
                            )
                            || isOaSegment)
                         && string.IsNullOrEmpty(seat.SeatAssignment)
                         )
                        )
                    { 
                        seatAssignment = new SeatAssignment();
                        // Added as part of Bug - 179725 - mApp- Booking_Travel Options_Choose seats_‘Sorry something went wrong’ message is displayed for ARNK Multi Trip
                        //seatAssignment.FlattenedSeatIndex = segmentNumber.SingleOrDefault() - 1;
                        seatAssignment.FlattenedSeatIndex = segment.FlightSegment.SegmentNumber - 1;
                        seatAssignment.TravelerIndex = traveler.PaxIndex;
                        seatAssignment.Seat = seat.SeatAssignment;
                        seatAssignment.SeatType = seat.SeatType;
                        // seatAssignment.SeatPrice = seat.PriceAfterTravelerCompanionRules;
                        seatAssignment.OriginalPrice = seat.PriceAfterTravelerCompanionRules;
                        seatAssignment.Currency = seat.Currency;
                        seatAssignment.OriginalSegmentIndex = segment.FlightSegment.SegmentNumber;
                        seatAssignment.FlightNumber = segment.FlightSegment.FlightNumber;
                        seatAssignment.ArrivalAirportCode = destination;
                        seatAssignment.DepartureAirportCode = seat.Origin;
                        seatAssignment.PersonIndex = traveler.TravelerNameIndex.ToString();
                        seatAssignment.SeatPromotionCode = seat.ProgramCode;
                        seatAssignment.ProductCode = seat.OldSeatProgramCode;
                        if (isAdvanceSearchCoupon && !String.IsNullOrEmpty(seat.PromotionalCouponCode))
                        {
                            seatAssignment.PromotionalCouponCode = seat.PromotionalCouponCode;
                            seatAssignment.SeatPrice = seat.PriceAfterCouponApplied;
                        }
                        else
                            seatAssignment.SeatPrice = seat.PriceAfterTravelerCompanionRules;

                        seatAssignments.Add(seatAssignment);
                    }
                }
            }

            seatRequest.SeatAssignments = seatAssignments;
            seatRequest.WorkFlowType = GetWorkFlowType(request.Flow);
            if (_configuration.GetValue<bool>("EnableOmniChannelCartMVP1"))
            {
                seatRequest.DeviceID = request.DeviceId;
            }
            return seatRequest;
        }

        private List<SeatAssignment> GetInfantOnLapRegisterSeatsRequest(
        List<United.Service.Presentation.SegmentModel.ReservationFlightSegment> segments,
        Reservation persistedReservation, List<SeatAssignment> seatAssignments)
        {
            SeatAssignment seatAssignment;
            int TravelerIndex = 0;
            foreach (var traveler in persistedReservation.TravelersCSL.Values)
            {
                //Execute only for RESHOP
                if (persistedReservation.IsReshopChange && !traveler.IsEligibleForSeatSelection)
                {
                    continue;
                } // Ends here

                foreach (var seat in traveler.Seats)
                {
                    var segment = segments.FirstOrDefault(s => s.FlightSegment.DepartureAirport.IATACode.ToUpper() == seat.Origin.ToUpper() &&
                                                               s.FlightSegment.ArrivalAirport.IATACode.ToUpper() == seat.Destination.ToUpper());
                    if (segment == null) continue;

                    var isOaSegment = IsSeatMapSupportedOa(segment.FlightSegment.OperatingAirlineCode, segment.FlightSegment.MarketedFlightSegment[0] != null ? segment.FlightSegment.MarketedFlightSegment[0].MarketingAirlineCode : string.Empty);
                    if (!(((persistedReservation.IsSSA && (persistedReservation.IsELF
                        || IsIBE(persistedReservation))) || isOaSegment) && string.IsNullOrEmpty(seat.SeatAssignment)))
                    {
                        seatAssignment = new SeatAssignment();
                        seatAssignment.FlattenedSeatIndex = segment.FlightSegment.SegmentNumber - 1;

                        if (persistedReservation.IsReshopChange)
                            seatAssignment.TravelerIndex = TravelerIndex;
                        else
                            seatAssignment.TravelerIndex = traveler.PaxIndex;

                        seatAssignment.Seat = seat.SeatAssignment;
                        seatAssignment.SeatType = seat.SeatType;
                        seatAssignment.SeatPrice = seat.PriceAfterTravelerCompanionRules;
                        seatAssignment.OriginalPrice = seat.PriceAfterTravelerCompanionRules;
                        seatAssignment.Currency = seat.Currency;
                        seatAssignment.OriginalSegmentIndex = segment.FlightSegment.SegmentNumber;
                        seatAssignment.FlightNumber = segment.FlightSegment.FlightNumber;
                        seatAssignment.ArrivalAirportCode = seat.Destination;
                        seatAssignment.DepartureAirportCode = seat.Origin;
                        seatAssignment.PersonIndex = traveler.TravelerNameIndex.ToString();
                        seatAssignment.SeatPromotionCode = seat.ProgramCode;
                        seatAssignment.ProductCode = seat.OldSeatProgramCode;

                        seatAssignments.Add(seatAssignment);
                    }
                }
                TravelerIndex++;
            }
            return seatAssignments;
        }

        private WorkFlowType GetWorkFlowType(string flow)
        {
            switch (flow)
            {
                case "CHECKIN":
                    return WorkFlowType.CheckInProductsPurchase;

                case "BOOKING":
                    return WorkFlowType.InitialBooking;

                case "VIEWRES":
                case "POSTBOOKING":
                    return WorkFlowType.PostPurchase;

                case "RESHOP":
                    return WorkFlowType.Reshop;

                case "FARELOCK":
                    return WorkFlowType.FareLockPurchase;
            }
            return WorkFlowType.UnKnown;
        }

        private async Task<FlightReservationResponse> GetFlightReservationResponseByCartId(string sessionId, string cartId)
        {
            #region
            //string testData = "{\"CartId\":\"C32D2A66-2BE3-4AB6-B21D-AF28D08FF8CC\",\"SessionId\":\"7e3294f9-3abf-4c33-a1c3-8b8082339a4e\",\"LastCallDateTime\":\"20211108 131044\",\"ServerName\":\"VCLD25ZQCSLAP02\",\"Status\":1,\"DisplayCart\":{\"CartId\":\"C32D2A66-2BE3-4AB6-B21D-AF28D08FF8CC\",\"CountryCode\":\"US\",\"LangCode\":\"en-US\",\"BookingCutOffMinutes\":60,\"BookingCutOffWarningMinutes\":30,\"GrandTotal\":348.4,\"SearchType\":1,\"PetTravel\":{\"PetCount\":0,\"PetsAreTraveling\":false},\"MilitaryTravel\":0,\"DisplaySeats\":[],\"DisplayTravelers\":[{\"DateOfBirth\":\"11/8/1996\",\"PaxType\":1,\"PaxTypeCode\":\"ADT\",\"PricingPaxType\":\"ADT\",\"PaxTypeDescription\":\"Adult (18-64)\",\"TravelerCount\":1,\"UMNROptIn\":false,\"Age\":0}],\"DisplayTrips\":[{\"ColumnInformation\":{\"Columns\":[{\"DataSourceLabel\":\"Economy \",\"Description\":\"\",\"Type\":\"ECONOMY\",\"SubType\":\"Revenue\",\"FareContentDescription\":\"<p>Our standard Economy fare:</p><ul class=\\\"fare-details-list\\\"><li>Seat selection at booking, if available</li><li>Enjoy other options for customizing your travel</li></ul>\",\"MarketingText\":\"<span class=bundles-content>Book with our <br>Low Fare Guarantee</span>Find the lowest United fare online at united.com.<br><a target=_blank href=\\\"http://www.united.com/CMS/en-US/products/travelproducts/Pages/Lowfareguarantee.aspx\\\">Learn more</a>\",\"DataSourceLabelStyle\":\"Economy\",\"FareFamilies\":[\"ECONOMY\",\"MIN-ECONOMY-SURP-OR-DISP\",\"ECONOMY\"],\"Value\":\"0-3\",\"FareFamily\":\"ECONOMY\",\"DescriptionId\":\"3\",\"MatrixId\":506,\"SortIndex\":10},{\"DataSourceLabel\":\"EPB\",\"Description\":\"Premier Access and Club Trip Pass\",\"Type\":\"B07\",\"FareContentDescription\":\"Premier Access and Club Trip Pass\",\"DataSourceLabelStyle\":\"EPB\"},{\"DataSourceLabel\":\"EPB\",\"Description\":\"Economy Plus\",\"Type\":\"B01\",\"FareContentDescription\":\"Economy Plus\",\"DataSourceLabelStyle\":\"EPB\"},{\"DataSourceLabel\":\"EPB\",\"Description\":\"Economy Plus, Standard checked bag\",\"Type\":\"B14\",\"FareContentDescription\":\"Economy Plus, Standard checked bag\",\"DataSourceLabelStyle\":\"EPB\"},{\"DataSourceLabel\":\"EPB\",\"Description\":\"Standard checked bag and Club Trip Pass\",\"Type\":\"B06\",\"FareContentDescription\":\"Standard checked bag and Club Trip Pass\",\"DataSourceLabelStyle\":\"EPB\"}]},\"BBXSession\":\"kOq0aRVTrKJk8BXDqWOZlhnUc\",\"BBXSolutionSetId\":\"0A1wLLM3e3aQTKqCCSY4M5H\",\"Origin\":\"IAH\",\"Destination\":\"LAX\",\"DepartDate\":\"2021-12-29 12:17\",\"InternationalCitiesExist\":false,\"Flights\":[{\"DepartDateTime\":\"2021-12-29 12:17\",\"BBXHash\":\"GGZmWdLpLuQlzMj0\",\"BBXSolutionSetId\":\"0A1wLLM3e3aQTKqCCSY4M5H\",\"BookingClassAvailability\":\"J6|JN6|C4|D0|Z0|ZN0|P0|PN0|PZ0|IN0|I0|Y9|YN9|B9|M9|E9|U9|H9|HN7|Q9|V9|W6|S0|T0|L0|K0|G0|N9|XN0|X0\",\"CabinCount\":2,\"CabinType\":\"Coach\",\"Destination\":\"LAX\",\"DestinationCountryCode\":\"US\",\"DestinationDateTime\":\"2021-12-29 14:02\",\"DestinationDescription\":\"Los Angeles, CA, US (LAX)\",\"DestinationStateCode\":\"CA\",\"DestinationTimezoneOffset\":-8,\"DestTimezoneOffset\":-8.0,\"FareBasisCode\":\"WAA1AKEN\",\"FlightNumber\":\"1230\",\"MarketingCarrier\":\"UA\",\"MarketingCarrierDescription\":\"United Airlines\",\"MileageActual\":1375,\"OperatingCarrier\":\"UA\",\"OperatingCarrierDescription\":\"United Airlines\",\"OperatingCarrierDescSource\":\"Domain-GetCarrierInfo\",\"PreSortIndex\":2,\"Origin\":\"IAH\",\"OriginCountryCode\":\"US\",\"OriginDescription\":\"Houston, TX, US (IAH)\",\"OriginStateCode\":\"TX\",\"OriginTimezoneOffset\":-6,\"OrgTimezoneOffset\":-6.0,\"OriginalFlightNumber\":\"1230\",\"PageIndex\":1,\"ParentFlightNumber\":\"\",\"Selected\":true,\"ServiceClass\":\"W\",\"ServiceClassCount\":6,\"ServiceClassCountLowest\":-1,\"ServiceClassAlternate\":\"W\",\"TravelMinutesTotal\":225,\"TravelMinutes\":225,\"TripIndex\":1,\"Connections\":[],\"Messages\":[],\"Products\":[{\"BookingCode\":\"W\",\"BookingClassAvailability\":\"J6|JN6|C4|D0|Z0|ZN0|P0|PN0|PZ0|IN0|I0|Y9|YN9|B9|M9|E9|U9|H9|HN7|Q9|V9|W6|S0|T0|L0|K0|G0|N9|XN0|X0\",\"BookingCount\":6,\"CabinTypeText\":\"\",\"CabinType\":\"Coach\",\"Description\":\"United Economy\",\"MerchIndex\":\"O1|1|1\",\"Mileage\":1375,\"ProductId\":\"NBKkkoUuov1T1rSrzNOBkD001\",\"Fares\":[{\"FareBasisCode\":\"WAA1AKEN\",\"SegmentRefs\":[\"1230\"],\"Amount\":213.02,\"Currency\":\"USD\",\"Ptc\":\"ADT\"}],\"ProductPath\":\"Revenue\",\"ProductSubtype\":\"BASE\",\"ProductCode\":\"\",\"ProductType\":\"ECONOMY\",\"SolutionId\":\"NBKkkoUuov1T1rSrzNOBkD001\",\"Prices\":[{\"Currency\":\"USD\",\"CurrencyAllPax\":\"USD\",\"Amount\":244.0,\"AmountAllPax\":244.0,\"AmountBase\":243.4,\"PricingType\":\"Fare\",\"AmountBySlice\":244.0,\"AmountBySliceAllPax\":244.0},{\"Currency\":\"USD\",\"Amount\":31.0,\"AmountAllPax\":0.0,\"AmountBase\":30.38,\"PricingType\":\"Taxes\"}],\"MealDescription\":\"Snacks for Purchase\",\"CabinTypeCode\":\"UE\",\"DisplayOrder\":0,\"ColumnId\":3,\"NumberOfPassengers\":0,\"TripIndex\":0,\"SegmentNumber\":0,\"IsOverBooked\":0,\"MarriedSegmentIndex\":0,\"IsProductUpdatedForOverBooking\":false,\"UserSelection\":\"ECONOMY\",\"FareFamily\":\"ECONOMY\",\"IsDynamicallyPriced\":0,\"SortIndex\":10},{\"BookingCode\":\"\",\"CabinTypeText\":\"Travel Options bundle\",\"CabinType\":\"Coach\",\"Description\":\"Premier Access and Club Trip Pass\",\"MerchIndex\":\"9\",\"ProductId\":\"B7-SOL1_OD1_S1_0\",\"ProductPath\":\"BE\",\"ProductSubtype\":\"B7\",\"ProductType\":\"B07\",\"PromoDescription\":\"IAH - LAX\",\"SolutionId\":\"0A1wLLM3e3aQTKqCCSY4M5H\",\"Prices\":[{\"Currency\":\"USD\",\"Amount\":105.0,\"AmountAllPax\":0.0,\"AmountBase\":105.0,\"PricingType\":\"Money\",\"ID\":\"B7-SOL1_OD1_S1_0\"}],\"LmxLoyaltyTiers\":[],\"CabinTypeCode\":\"W\",\"SubProducts\":[{\"Code\":\"PAS\",\"Description\":\"Premier Access®\",\"ReferencedSegments\":[{\"Origin\":\"IAH\",\"Destination\":\"LAX\",\"FlightHash\":\"363-1230-UA\",\"IsEligible\":true}],\"ActualPrice\":{\"Currency\":\"USD\",\"Amount\":56.0,\"AmountAllPax\":0.0,\"AmountBase\":56.0,\"PricingType\":\"Money\"},\"Price\":{\"Currency\":\"USD\",\"Amount\":51.0,\"AmountAllPax\":0.0,\"AmountBase\":51.0,\"PricingType\":\"Money\"},\"EddCode\":\"PA1\",\"DisplayOrder\":1,\"SubDescription\":\"Enjoy dedicated check-in, exclusive security lanes (where available) and priority boarding\",\"AlternateDescription\":\"Premier Access®\"},{\"Code\":\"CTP\",\"Description\":\"United Club&#8480 trip pass\",\"ReferencedAirports\":[{\"Airport\":\"IAH\",\"IsEligible\":true},{\"Airport\":\"LAX\",\"IsEligible\":true}],\"ActualPrice\":{\"Currency\":\"USD\",\"Amount\":60.0,\"AmountAllPax\":0.0,\"AmountBase\":60.0,\"PricingType\":\"Money\"},\"Price\":{\"Currency\":\"USD\",\"Amount\":54.0,\"AmountAllPax\":0.0,\"AmountBase\":54.0,\"PricingType\":\"Money\"},\"EddCode\":\"CP1\",\"DisplayOrder\":2,\"SubDescription\":\"Access available club locations throughout your trip\",\"AlternateDescription\":\"United Club&#8480 access\"}],\"IsBundleProduct\":true,\"DisplayOrder\":1,\"NumberOfPassengers\":1,\"MerchandisingSessionSaveName\":\"ProductBundles\",\"MerchandisingSessionKeyName\":\"_POS_US-C32D2A66-2BE3-4AB6-B21D-AF28D08FF8CC\",\"TripIndex\":1,\"SegmentNumber\":1,\"IsOverBooked\":0,\"MarriedSegmentIndex\":0,\"IsProductUpdatedForOverBooking\":false,\"IsDynamicallyPriced\":0},{\"BookingCode\":\"\",\"CabinTypeText\":\"Travel Options bundle\",\"CabinType\":\"Coach\",\"Description\":\"Economy Plus\",\"MerchIndex\":\"1\",\"ProductId\":\"B1-SOL1_OD1_S1_0\",\"ProductPath\":\"BE\",\"ProductSubtype\":\"B1\",\"ProductType\":\"B01\",\"PromoDescription\":\"IAH - LAX\",\"SolutionId\":\"0A1wLLM3e3aQTKqCCSY4M5H\",\"Prices\":[{\"Currency\":\"USD\",\"Amount\":74.0,\"AmountAllPax\":0.0,\"AmountBase\":74.0,\"PricingType\":\"Money\",\"ID\":\"B1-SOL1_OD1_S1_0\"}],\"LmxLoyaltyTiers\":[],\"CabinTypeCode\":\"W\",\"SubProducts\":[{\"Code\":\"EPU\",\"Description\":\"Economy Plus® seating\",\"ReferencedSegments\":[{\"Origin\":\"IAH\",\"Destination\":\"LAX\",\"FlightHash\":\"363-1230-UA\",\"IsEligible\":true}],\"ActualPrice\":{\"Currency\":\"USD\",\"Amount\":82.0,\"AmountAllPax\":0.0,\"AmountBase\":82.0,\"PricingType\":\"Money\"},\"Price\":{\"Currency\":\"USD\",\"Amount\":74.0,\"AmountAllPax\":0.0,\"AmountBase\":74.0,\"PricingType\":\"Money\"},\"EddCode\":\"EP1\",\"DisplayOrder\":1,\"SubDescription\":\"Choose any available Economy Plus seat\",\"AlternateDescription\":\"Economy Plus®\"}],\"IsBundleProduct\":true,\"DisplayOrder\":2,\"NumberOfPassengers\":1,\"MerchandisingSessionSaveName\":\"ProductBundles\",\"MerchandisingSessionKeyName\":\"_POS_US-C32D2A66-2BE3-4AB6-B21D-AF28D08FF8CC\",\"TripIndex\":1,\"SegmentNumber\":1,\"IsOverBooked\":0,\"MarriedSegmentIndex\":0,\"IsProductUpdatedForOverBooking\":false,\"IsDynamicallyPriced\":0},{\"BookingCode\":\"\",\"CabinTypeText\":\"Travel Options bundle\",\"CabinType\":\"Coach\",\"Description\":\"Economy Plus, Standard checked bag\",\"MerchIndex\":\"5\",\"ProductId\":\"B14-SOL1_OD1_S1_0\",\"ProductPath\":\"BE\",\"ProductSubtype\":\"B14\",\"ProductType\":\"B14\",\"PromoDescription\":\"IAH - LAX\",\"SolutionId\":\"0A1wLLM3e3aQTKqCCSY4M5H\",\"Prices\":[{\"Currency\":\"USD\",\"Amount\":90.0,\"AmountAllPax\":0.0,\"AmountBase\":90.0,\"PricingType\":\"Money\",\"ID\":\"B14-SOL1_OD1_S1_0\"}],\"LmxLoyaltyTiers\":[],\"CabinTypeCode\":\"W\",\"SubProducts\":[{\"Code\":\"EPU\",\"Description\":\"Economy Plus® seating\",\"ReferencedSegments\":[{\"Origin\":\"IAH\",\"Destination\":\"LAX\",\"FlightHash\":\"363-1230-UA\",\"IsEligible\":true}],\"ActualPrice\":{\"Currency\":\"USD\",\"Amount\":82.0,\"AmountAllPax\":0.0,\"AmountBase\":82.0,\"PricingType\":\"Money\"},\"Price\":{\"Currency\":\"USD\",\"Amount\":66.0,\"AmountAllPax\":0.0,\"AmountBase\":66.0,\"PricingType\":\"Money\"},\"EddCode\":\"EP1\",\"DisplayOrder\":1,\"SubDescription\":\"Choose any available Economy Plus seat\",\"AlternateDescription\":\"Economy Plus®\"},{\"Code\":\"EXB\",\"Description\":\"Standard checked bag\",\"ReferencedSegments\":[{\"Origin\":\"IAH\",\"Destination\":\"LAX\",\"FlightHash\":\"363-1230-UA\",\"IsEligible\":true}],\"ActualPrice\":{\"Currency\":\"USD\",\"Amount\":30.0,\"AmountAllPax\":0.0,\"AmountBase\":30.0,\"PricingType\":\"Money\"},\"Price\":{\"Currency\":\"USD\",\"Amount\":24.0,\"AmountAllPax\":0.0,\"AmountBase\":24.0,\"PricingType\":\"Money\"},\"EddCode\":\"XB1\",\"DisplayOrder\":2,\"SubDescription\":\"One checked bag in addition to the baggage allowance that you otherwise qualify for\",\"AlternateDescription\":\"Standard checked bag\"}],\"IsBundleProduct\":true,\"DisplayOrder\":3,\"NumberOfPassengers\":1,\"MerchandisingSessionSaveName\":\"ProductBundles\",\"MerchandisingSessionKeyName\":\"_POS_US-C32D2A66-2BE3-4AB6-B21D-AF28D08FF8CC\",\"TripIndex\":1,\"SegmentNumber\":1,\"IsOverBooked\":0,\"MarriedSegmentIndex\":0,\"IsProductUpdatedForOverBooking\":false,\"IsDynamicallyPriced\":0},{\"BookingCode\":\"\",\"CabinTypeText\":\"Travel Options bundle\",\"CabinType\":\"Coach\",\"Description\":\"Standard checked bag and Club Trip Pass\",\"MerchIndex\":\"8\",\"ProductId\":\"B6-SOL1_OD1_S1_0\",\"ProductPath\":\"BE\",\"ProductSubtype\":\"B6\",\"ProductType\":\"B06\",\"PromoDescription\":\"IAH - LAX\",\"SolutionId\":\"0A1wLLM3e3aQTKqCCSY4M5H\",\"Prices\":[{\"Currency\":\"USD\",\"Amount\":81.0,\"AmountAllPax\":0.0,\"AmountBase\":81.0,\"PricingType\":\"Money\",\"ID\":\"B6-SOL1_OD1_S1_0\"}],\"LmxLoyaltyTiers\":[],\"CabinTypeCode\":\"W\",\"SubProducts\":[{\"Code\":\"EXB\",\"Description\":\"Standard checked bag\",\"ReferencedSegments\":[{\"Origin\":\"IAH\",\"Destination\":\"LAX\",\"FlightHash\":\"363-1230-UA\",\"IsEligible\":true}],\"ActualPrice\":{\"Currency\":\"USD\",\"Amount\":30.0,\"AmountAllPax\":0.0,\"AmountBase\":30.0,\"PricingType\":\"Money\"},\"Price\":{\"Currency\":\"USD\",\"Amount\":27.0,\"AmountAllPax\":0.0,\"AmountBase\":27.0,\"PricingType\":\"Money\"},\"EddCode\":\"XB1\",\"DisplayOrder\":1,\"SubDescription\":\"One checked bag in addition to the baggage allowance that you otherwise qualify for\",\"AlternateDescription\":\"Standard checked bag\"},{\"Code\":\"CTP\",\"Description\":\"United Club&#8480 trip pass\",\"ReferencedAirports\":[{\"Airport\":\"IAH\",\"IsEligible\":true},{\"Airport\":\"LAX\",\"IsEligible\":true}],\"ActualPrice\":{\"Currency\":\"USD\",\"Amount\":60.0,\"AmountAllPax\":0.0,\"AmountBase\":60.0,\"PricingType\":\"Money\"},\"Price\":{\"Currency\":\"USD\",\"Amount\":54.0,\"AmountAllPax\":0.0,\"AmountBase\":54.0,\"PricingType\":\"Money\"},\"EddCode\":\"CP1\",\"DisplayOrder\":2,\"SubDescription\":\"Access available club locations throughout your trip\",\"AlternateDescription\":\"United Club&#8480 access\"}],\"IsBundleProduct\":true,\"DisplayOrder\":4,\"NumberOfPassengers\":1,\"MerchandisingSessionSaveName\":\"ProductBundles\",\"MerchandisingSessionKeyName\":\"_POS_US-C32D2A66-2BE3-4AB6-B21D-AF28D08FF8CC\",\"TripIndex\":1,\"SegmentNumber\":1,\"IsOverBooked\":0,\"MarriedSegmentIndex\":0,\"IsProductUpdatedForOverBooking\":false,\"IsDynamicallyPriced\":0}],\"StopInfos\":[],\"ProductsNotSelected\":[{\"BookingCode\":\"C\",\"BookingClassAvailability\":\"J6|JN6|C4|D0|Z0|ZN0|P0|PN0|PZ0|IN0|I0|Y9|YN9|B9|M9|E9|U9|H9|HN7|Q9|V9|W6|S0|T0|L0|K0|G0|N9|XN0|X0\",\"BookingCount\":4,\"CabinTypeText\":\"(2-cabin, fully refundable)\",\"CabinType\":\"First\",\"Description\":\"United First\",\"Mileage\":1375,\"ProductId\":\"NBKkkoUuov1T1rSrzNOBkD00c\",\"Fares\":[{\"FareBasisCode\":\"EAA2ODFM\",\"SegmentRefs\":[\"1230\"],\"Amount\":858.6,\"Currency\":\"USD\",\"Ptc\":\"ADT\"}],\"ProductPath\":\"Revenue\",\"ProductSubtype\":\"BASE\",\"ProductType\":\"MIN-BUSINESS-OR-FIRST-UNRESTRICTED\",\"SolutionId\":\"NBKkkoUuov1T1rSrzNOBkD00c\",\"Prices\":[{\"Currency\":\"USD\",\"CurrencyAllPax\":\"USD\",\"Amount\":938.0,\"AmountAllPax\":938.0,\"AmountBase\":937.4,\"PricingType\":\"Fare\",\"AmountBySlice\":938.0,\"AmountBySliceAllPax\":938.0},{\"Currency\":\"USD\",\"Amount\":79.0,\"AmountAllPax\":0.0,\"AmountBase\":78.8,\"PricingType\":\"Taxes\"}],\"MealDescription\":\"Lunch\",\"CabinTypeCode\":\"UF\",\"DisplayOrder\":0,\"ColumnId\":81,\"NumberOfPassengers\":0,\"TripIndex\":0,\"SegmentNumber\":0,\"IsOverBooked\":0,\"MarriedSegmentIndex\":0,\"IsProductUpdatedForOverBooking\":false,\"UserSelection\":\"MIN-BUSINESS-OR-FIRST-UNRESTRICTED\",\"FareFamily\":\"MIN-BUSINESS-OR-FIRST-UNRESTRICTED\",\"IsDynamicallyPriced\":0,\"SortIndex\":79},{\"BookingCode\":\"C\",\"BookingClassAvailability\":\"J6|JN6|C4|D0|Z0|ZN0|P0|PN0|PZ0|IN0|I0|Y9|YN9|B9|M9|E9|U9|H9|HN7|Q9|V9|W6|S0|T0|L0|K0|G0|N9|XN0|X0\",\"BookingCount\":4,\"CabinTypeText\":\"(2-cabin)\",\"CabinType\":\"First\",\"Description\":\"United First\",\"Mileage\":1375,\"ProductId\":\"NBKkkoUuov1T1rSrzNOBkD00S\",\"Fares\":[{\"FareBasisCode\":\"EAA0OKFN\",\"SegmentRefs\":[\"1230\"],\"Amount\":765.58,\"Currency\":\"USD\",\"Ptc\":\"ADT\"}],\"ProductPath\":\"Revenue\",\"ProductSubtype\":\"BASE\",\"ProductType\":\"MIN-BUSINESS-OR-FIRST\",\"SolutionId\":\"NBKkkoUuov1T1rSrzNOBkD00S\",\"Prices\":[{\"Currency\":\"USD\",\"CurrencyAllPax\":\"USD\",\"Amount\":838.0,\"AmountAllPax\":838.0,\"AmountBase\":837.4,\"PricingType\":\"Fare\",\"AmountBySlice\":838.0,\"AmountBySliceAllPax\":838.0},{\"Currency\":\"USD\",\"Amount\":72.0,\"AmountAllPax\":0.0,\"AmountBase\":71.82,\"PricingType\":\"Taxes\"}],\"MealDescription\":\"Lunch\",\"CabinTypeCode\":\"UF\",\"DisplayOrder\":0,\"ColumnId\":79,\"NumberOfPassengers\":0,\"TripIndex\":0,\"SegmentNumber\":0,\"IsOverBooked\":0,\"MarriedSegmentIndex\":0,\"IsProductUpdatedForOverBooking\":false,\"UserSelection\":\"MIN-BUSINESS-OR-FIRST\",\"FareFamily\":\"MIN-BUSINESS-OR-FIRST\",\"IsDynamicallyPriced\":0,\"SortIndex\":77},{\"BookingCode\":\"W\",\"BookingClassAvailability\":\"J6|JN6|C4|D0|Z0|ZN0|P0|PN0|PZ0|IN0|I0|Y9|YN9|B9|M9|E9|U9|H9|HN7|Q9|V9|W6|S0|T0|L0|K0|G0|N9|XN0|X0\",\"BookingCount\":6,\"CabinTypeText\":\"(fully refundable)\",\"CabinType\":\"Coach\",\"Description\":\"United Economy\",\"Mileage\":1375,\"ProductId\":\"NBKkkoUuov1T1rSrzNOBkD00B\",\"Fares\":[{\"FareBasisCode\":\"WAA1AKEM\",\"SegmentRefs\":[\"1230\"],\"Amount\":259.53,\"Currency\":\"USD\",\"Ptc\":\"ADT\"}],\"ProductPath\":\"Revenue\",\"ProductSubtype\":\"BASE\",\"ProductType\":\"ECONOMY-UNRESTRICTED\",\"SolutionId\":\"NBKkkoUuov1T1rSrzNOBkD00B\",\"Prices\":[{\"Currency\":\"USD\",\"CurrencyAllPax\":\"USD\",\"Amount\":294.0,\"AmountAllPax\":294.0,\"AmountBase\":293.4,\"PricingType\":\"Fare\",\"AmountBySlice\":294.0,\"AmountBySliceAllPax\":294.0},{\"Currency\":\"USD\",\"Amount\":34.0,\"AmountAllPax\":0.0,\"AmountBase\":33.87,\"PricingType\":\"Taxes\"}],\"MealDescription\":\"Snacks for Purchase\",\"CabinTypeCode\":\"UE\",\"DisplayOrder\":0,\"ColumnId\":9,\"NumberOfPassengers\":0,\"TripIndex\":0,\"SegmentNumber\":0,\"IsOverBooked\":0,\"MarriedSegmentIndex\":0,\"IsProductUpdatedForOverBooking\":false,\"UserSelection\":\"ECONOMY-UNRESTRICTED\",\"FareFamily\":\"ECONOMY-UNRESTRICTED\",\"IsDynamicallyPriced\":0,\"SortIndex\":16},{\"BookingCode\":\"\",\"BookingCount\":6,\"CabinType\":\"Coach\",\"Mileage\":1375,\"ProductId\":\"NBKkkoUuov1T1rSrzNOBkD001_ME\",\"Fares\":[{\"FareBasisCode\":\"WAA1AKEN\",\"SegmentRefs\":[\"1230\"],\"Amount\":213.02,\"Currency\":\"USD\",\"Ptc\":\"ADT\"}],\"ProductPath\":\"Revenue\",\"ProductSubtype\":\"NonExistingProductPlaceHolder\",\"ProductType\":\"ECONOMY-MERCH-EPLUS\",\"SolutionId\":\"NBKkkoUuov1T1rSrzNOBkD001\",\"Prices\":[],\"MealDescription\":\"Snacks for Purchase\",\"CabinTypeCode\":\"UE\",\"DisplayOrder\":0,\"ColumnId\":3,\"NumberOfPassengers\":0,\"TripIndex\":0,\"SegmentNumber\":0,\"IsOverBooked\":0,\"MarriedSegmentIndex\":0,\"IsProductUpdatedForOverBooking\":false,\"UserSelection\":\"ECONOMY\",\"FareFamily\":\"ECONOMY-MERCH-EPLUS\",\"IsDynamicallyPriced\":0,\"SortIndex\":10},{\"BookingCode\":\"\",\"BookingCount\":6,\"CabinType\":\"Coach\",\"Mileage\":1375,\"ProductId\":\"NBKkkoUuov1T1rSrzNOBkD00B_MEU\",\"Fares\":[{\"FareBasisCode\":\"WAA1AKEM\",\"SegmentRefs\":[\"1230\"],\"Amount\":259.53,\"Currency\":\"USD\",\"Ptc\":\"ADT\"}],\"ProductPath\":\"Revenue\",\"ProductSubtype\":\"NonExistingProductPlaceHolder\",\"ProductType\":\"ECONOMY-UNRESTRICTED-MERCH-EPLUS\",\"SolutionId\":\"NBKkkoUuov1T1rSrzNOBkD00B\",\"Prices\":[],\"MealDescription\":\"Snacks for Purchase\",\"CabinTypeCode\":\"UE\",\"DisplayOrder\":0,\"NumberOfPassengers\":0,\"TripIndex\":0,\"SegmentNumber\":0,\"IsOverBooked\":0,\"MarriedSegmentIndex\":0,\"IsProductUpdatedForOverBooking\":false,\"UserSelection\":\"ECONOMY-UNRESTRICTED\",\"FareFamily\":\"ECONOMY-UNRESTRICTED-MERCH-EPLUS\",\"IsDynamicallyPriced\":0}],\"Warnings\":[],\"EquipmentDisclosures\":{\"EquipmentType\":\"739\",\"EquipmentDescription\":\"Boeing 737-900\",\"IsSingleCabin\":false,\"NoBoardingAssistance\":false,\"NonJetEquipment\":false,\"WheelchairsNotAllowed\":false},\"FlightInfo\":{\"EstimatedArrivalDateTime\":\"2021-12-29 14:02\",\"EstimatedDepartureDateTime\":\"2021-12-29 12:17\",\"ScheduledArrivalDateTime\":\"2021-12-29 14:02\",\"ScheduledDepartureDateTime\":\"2021-12-29 12:17\"},\"Aircraft\":{},\"Amenities\":[],\"Hash\":\"363-1230-UA\",\"MerchIndex\":\"S1\",\"MerchTripIndex\":\"O11\",\"Index\":1}],\"PetCount\":0,\"Index\":1,\"OriginalMileage\":0,\"OriginalMileageTotal\":0,\"OriginalTax\":0.0,\"ChangeType\":0,\"OriginalChangeType\":0}],\"DisplayPrices\":[{\"Amount\":213.02,\"Count\":1,\"Currency\":\"USD\",\"Description\":\"Adult (18-64)\",\"Type\":\"TravelerPrice\",\"SubItems\":[{\"Type\":\"US\",\"Amount\":15.98,\"Currency\":\"USD\",\"Description\":\"U.S. Transportation Tax\",\"Key\":\"0\",\"Value\":\"Tax\",\"Count\":0,\"OriginalPrice\":0.0},{\"Type\":\"XF\",\"Amount\":4.5,\"Currency\":\"USD\",\"Description\":\"U.S. Passenger Facility Charge\",\"Key\":\"1\",\"Value\":\"Tax\",\"Count\":0,\"OriginalPrice\":0.0},{\"Type\":\"ZP\",\"Amount\":4.3,\"Currency\":\"USD\",\"Description\":\"U.S. Flight Segment Tax\",\"Key\":\"2\",\"Value\":\"Tax\",\"Count\":0,\"OriginalPrice\":0.0},{\"Type\":\"AY\",\"Amount\":5.6,\"Currency\":\"USD\",\"Description\":\"September 11th Security Fee\",\"Key\":\"3\",\"Value\":\"Tax\",\"Count\":0,\"OriginalPrice\":0.0}],\"PaxTypeCode\":\"ADT\",\"Waived\":false,\"PricingPaxType\":\"ADT\",\"ResidualAmount\":0.0},{\"Amount\":243.4,\"Count\":0,\"Currency\":\"USD\",\"Description\":\"Total\",\"Type\":\"Total\",\"Waived\":false,\"ResidualAmount\":0.0},{\"Amount\":30.38,\"Count\":0,\"Currency\":\"USD\",\"Description\":\"TAXTotal\",\"Type\":\"TAXTotal\",\"Waived\":false,\"ResidualAmount\":0.0}],\"DisplayDetails\":[],\"bookingDetails\":{\"solution\":[{\"ext\":{\"bookingCodeCount\":0,\"fareFamily\":\"bookingDetails\",\"fareFlavor\":\"vanilla\",\"isOverbooked\":0},\"passenger\":[{\"ptc\":[\"ADT\",\"AUTOAC\",\"AUTOSP\"],\"id\":\"0\",\"age\":\"25\"}],\"pricing\":[{\"tax\":[{\"salePrice\":{\"amount\":15.98,\"currency\":\"USD\"},\"code\":\"US\",\"status\":\"APPLIED\"},{\"salePrice\":{\"amount\":4.5,\"currency\":\"USD\"},\"code\":\"XF\",\"status\":\"APPLIED\"},{\"salePrice\":{\"amount\":4.3,\"currency\":\"USD\"},\"code\":\"ZP\",\"status\":\"APPLIED\"},{\"salePrice\":{\"amount\":5.6,\"currency\":\"USD\"},\"code\":\"AY\",\"status\":\"APPLIED\"}],\"PaxPricingIndex\":0,\"ext\":{\"bookingCodeCount\":0,\"price\":{\"amount\":213.02,\"currency\":\"USD\"},\"taxTotalNoYQYR\":{\"totalSalePrice\":{\"amount\":30.38,\"currency\":\"USD\"}},\"referencePrice\":{\"amount\":213.02,\"currency\":\"USD\"},\"allPassengerPrice\":{\"amount\":243.4,\"currency\":\"USD\"},\"miles\":\"214\",\"endorsements\":[\"NONREF/0VALUAFTDPT\"],\"taxTotal\":{\"totalSalePrice\":{\"amount\":30.38,\"currency\":\"USD\"}},\"isOverbooked\":0},\"saleFareTotal\":{\"amount\":213.02,\"currency\":\"USD\"},\"salePrice\":{\"amount\":243.4,\"currency\":\"USD\"},\"saleTaxTotal\":{\"amount\":30.38,\"currency\":\"USD\"},\"cocFareTotal\":{\"amount\":213.02,\"currency\":\"USD\"},\"additionalCollectionAwardPoints\":0,\"previousAwardPointsFareTotal\":0,\"refundAwardPoints\":0,\"fareCalculation\":[{\"line\":[\"HOU UA LAX 213.02WAA1AKEN USD 213.02 END ZP IAH XT 15.98US 4.30ZP 5.60AY 4.50XF IAH4.5\"]}],\"fare\":[{\"destinationCity\":\"LAX\",\"originCity\":\"HOU\",\"tag\":\"ONE-WAY\",\"extendedFareCode\":\"WAA1AKEN\",\"carrier\":\"UA\",\"endorsement\":\"NONREF/0VALUAFTDPT\",\"ptc\":\"ADT\",\"isMerchEnginePriced\":false,\"rkey\":\"A2rldcoDXUBFLeO+cVa5gcIT+lOsVsZkvTwO08pXj0Kc\",\"basePrice\":\"USD213.02\",\"bookingInfo\":[{\"segment\":{\"duration\":0,\"bookingInfo\":[{\"bookingCode\":\"W\",\"PaxPriceIndex\":0,\"marriedSegmentIndex\":0}],\"isConnection\":false},\"PaxPriceIndex\":0,\"marriedSegmentIndex\":0}],\"ruleSummary\":{},\"characteristics\":[]}],\"note\":[{}],\"paxCount\":1,\"passenger\":[{\"ptc\":[\"ADT\",\"AUTOAC\",\"AUTOSP\"],\"id\":\"0\",\"age\":\"25\"}]}],\"id\":\"F6GidMjVWKKTpB9eC8QkB8001\",\"slice\":[{\"duration\":\"225\",\"segment\":[{\"duration\":0,\"hash\":\"GGZmWdLpLuQlzMj0\",\"leg\":[{\"arrival\":\"2021-12-29T14:02-08:00\",\"departure\":\"2021-12-29T12:17-06:00\",\"destination\":\"LAX\",\"origin\":\"IAH\",\"destinationTerminal\":\"7\",\"originTerminal\":\"C\",\"flight\":\"UA1230\",\"hash\":\"LZY-a43wtiQv3C2g\",\"cabinCount\":2}],\"availability\":{\"bookingCodeCount\":[{\"code\":\"J\",\"count\":\"6\"},{\"code\":\"JN\",\"count\":\"6\"},{\"code\":\"C\",\"count\":\"4\"},{\"code\":\"D\",\"count\":\"0\"},{\"code\":\"Z\",\"count\":\"0\"},{\"code\":\"ZN\",\"count\":\"0\"},{\"code\":\"P\",\"count\":\"0\"},{\"code\":\"PN\",\"count\":\"0\"},{\"code\":\"PZ\",\"count\":\"0\"},{\"code\":\"IN\",\"count\":\"0\"},{\"code\":\"I\",\"count\":\"0\"},{\"code\":\"Y\",\"count\":\"9\"},{\"code\":\"YN\",\"count\":\"9\"},{\"code\":\"B\",\"count\":\"9\"},{\"code\":\"M\",\"count\":\"9\"},{\"code\":\"E\",\"count\":\"9\"},{\"code\":\"U\",\"count\":\"9\"},{\"code\":\"H\",\"count\":\"9\"},{\"code\":\"HN\",\"count\":\"7\"},{\"code\":\"Q\",\"count\":\"9\"},{\"code\":\"V\",\"count\":\"9\"},{\"code\":\"W\",\"count\":\"6\"},{\"code\":\"S\",\"count\":\"0\"},{\"code\":\"T\",\"count\":\"0\"},{\"code\":\"L\",\"count\":\"0\"},{\"code\":\"K\",\"count\":\"0\"},{\"code\":\"G\",\"count\":\"0\"},{\"code\":\"N\",\"count\":\"9\"},{\"code\":\"XN\",\"count\":\"0\"},{\"code\":\"X\",\"count\":\"0\"}]},\"isConnection\":false}],\"index\":\"0\"}]}]},\"TravelOptions\":[{\"Type\":\"BE\",\"Amount\":105.0,\"Description\":\"Premier Access and Club Trip Pass\",\"Currency\":\"USD\",\"Key\":\"B07\",\"Deleted\":false,\"SubItems\":[{\"Type\":\"B07\",\"Amount\":105.0,\"Currency\":\"USD\",\"Description\":\"Premier Access and Club Trip Pass\",\"Key\":\"B7-SOL1_OD1_S1_0\",\"Value\":\"IAH - LAX\",\"Product\":{\"BookingCode\":\"\",\"CabinTypeText\":\"Travel Options bundle\",\"CabinType\":\"Coach\",\"Description\":\"Premier Access and Club Trip Pass\",\"MerchIndex\":\"9\",\"ProductId\":\"B7-SOL1_OD1_S1_0\",\"ProductPath\":\"BE\",\"ProductSubtype\":\"B7\",\"ProductType\":\"B07\",\"PromoDescription\":\"IAH - LAX\",\"SolutionId\":\"0A1wLLM3e3aQTKqCCSY4M5H\",\"Prices\":[{\"Currency\":\"USD\",\"Amount\":105.0,\"AmountAllPax\":0.0,\"AmountBase\":105.0,\"PricingType\":\"Money\",\"ID\":\"B7-SOL1_OD1_S1_0\"}],\"CabinTypeCode\":\"W\",\"SubProducts\":[{\"Code\":\"PAS\",\"Description\":\"Premier Access®\",\"ReferencedSegments\":[{\"Origin\":\"IAH\",\"Destination\":\"LAX\",\"FlightHash\":\"363-1230-UA\",\"IsEligible\":true}],\"ActualPrice\":{\"Currency\":\"USD\",\"Amount\":56.0,\"AmountAllPax\":0.0,\"AmountBase\":56.0,\"PricingType\":\"Money\"},\"Price\":{\"Currency\":\"USD\",\"Amount\":51.0,\"AmountAllPax\":0.0,\"AmountBase\":51.0,\"PricingType\":\"Money\"},\"EddCode\":\"PA1\",\"DisplayOrder\":1,\"SubDescription\":\"Enjoy dedicated check-in, exclusive security lanes (where available) and priority boarding\",\"AlternateDescription\":\"Premier Access®\"},{\"Code\":\"CTP\",\"Description\":\"United Club&#8480 trip pass\",\"ReferencedAirports\":[{\"Airport\":\"IAH\",\"IsEligible\":true},{\"Airport\":\"LAX\",\"IsEligible\":true}],\"ActualPrice\":{\"Currency\":\"USD\",\"Amount\":60.0,\"AmountAllPax\":0.0,\"AmountBase\":60.0,\"PricingType\":\"Money\"},\"Price\":{\"Currency\":\"USD\",\"Amount\":54.0,\"AmountAllPax\":0.0,\"AmountBase\":54.0,\"PricingType\":\"Money\"},\"EddCode\":\"CP1\",\"DisplayOrder\":2,\"SubDescription\":\"Access available club locations throughout your trip\",\"AlternateDescription\":\"United Club&#8480 access\"}],\"IsBundleProduct\":true,\"DisplayOrder\":1,\"NumberOfPassengers\":1,\"MerchandisingSessionSaveName\":\"ProductBundles\",\"MerchandisingSessionKeyName\":\"_POS_US-C32D2A66-2BE3-4AB6-B21D-AF28D08FF8CC\",\"TripIndex\":1,\"SegmentNumber\":1,\"IsOverBooked\":0,\"MarriedSegmentIndex\":0,\"IsProductUpdatedForOverBooking\":false,\"IsDynamicallyPriced\":0},\"TripIndex\":\"1\",\"TravelerRefID\":\"0\",\"TravelerRefIDColl\":[\"1.1\"],\"Reason\":\"ADT\",\"SegmentNumber\":\"1\",\"Count\":0,\"OriginalPrice\":0.0}],\"ItemCount\":0,\"OriginalPrice\":0.0}],\"DisplayFees\":[],\"ProductsDirty\":true,\"ProductCode\":\"\",\"Characteristics\":[{\"Code\":\"SHOPREQPETTRAVEL\",\"Value\":\"False\"},{\"Code\":\"SHOPREQPETCOUNT\",\"Value\":\"0\"},{\"Code\":\"RECENTSEARCHKEY\",\"Value\":\"IAHLAX12/29/2021\"},{\"Code\":\"DEVICEID\",\"Value\":\"DFFBE04D-2AAF-40E9-B00D-2E22F47A28D6\"},{\"Code\":\"IS_FROM_FSCSL\",\"Value\":\"true\"},{\"Code\":\"WorkFlowType\",\"Value\":\"1\"}],\"ProductCodeBeforeUpgrade\":\"\",\"BookingDetailsRespSolutionSetId\":\"0elIfg2VfmHVMTn1grH5qF6\",\"FareEndorsements\":[\"NONREF/0VALUAFTDPT\"]},\"Errors\":[],\"Reservation\":{\"Travelers\":[{\"Person\":{\"Key\":\"1.1\",\"Surname\":\"chanda\",\"GivenName\":\"Sai kiran\",\"MiddleName\":\"\",\"Suffix\":\"\",\"DateOfBirth\":\"11/07/1981\",\"Sex\":\"M\",\"Documents\":[{\"DateOfBirth\":\"11/07/1981\",\"GivenName\":\"Sai kiran\",\"Surname\":\"chanda\",\"Suffix\":\"\",\"Sex\":\"M\",\"Type\":0,\"KnownTravelerNumber\":\"\",\"MiddleName\":\"\"}],\"Type\":\"ADT\",\"InfantIndicator\":\"false\",\"ReservationIndex\":\"0\",\"Contact\":{\"Emails\":[],\"PhoneNumbers\":[]},\"PricingPaxType\":\"ADT\"},\"LoyaltyProgramProfile\":{\"LoyaltyProgramMemberTierDescription\":9}}],\"TelephoneNumbers\":[],\"FlightSegments\":[{\"FlightSegment\":{\"DepartureAirport\":{\"IATACode\":\"IAH\",\"Name\":\"Houston, TX, US (IAH)\",\"IATACountryCode\":{\"CountryCode\":\"US\"},\"StateProvince\":{\"StateProvinceCode\":\"TX\"}},\"ArrivalAirport\":{\"IATACode\":\"LAX\",\"Name\":\"Los Angeles, CA, US (LAX)\",\"IATACountryCode\":{\"CountryCode\":\"US\"},\"StateProvince\":{\"StateProvinceCode\":\"CA\"}},\"DepartureDateTime\":\"2021-12-29 12:17\",\"ArrivalDateTime\":\"2021-12-29 14:02\",\"FlightNumber\":\"1230\",\"OperatingAirlineCode\":\"UA\",\"Equipment\":{\"Model\":{\"Description\":\"Boeing 737-900\",\"Fleet\":\"739\"},\"CabinCount\":2},\"MarketedFlightSegment\":[{\"MarketingAirlineCode\":\"UA\",\"FlightNumber\":\"1230\"}],\"MarriageGroup\":\"N\",\"Distance\":1375,\"SegmentNumber\":1,\"BookingClasses\":[{\"Code\":\"W\",\"Cabin\":{\"Key\":\"UE\",\"Name\":\"United Economy\",\"Layout\":\"2\"}}],\"FlightSegmentType\":\"NN\",\"TravelerCounts\":[{\"Counts\":[{\"Value\":\"1\"}]}],\"Message\":[{\"Code\":\"ARRDAYDIFF\"},{\"Text\":\"WAA1AKEN\",\"Code\":\"BASIS_CODE\"},{\"Text\":\"False\",\"Code\":\"PETS_DISALLOWED\"},{\"Text\":\"United Airlines\",\"Code\":\"OPERATINGCARRIERDESC\"},{\"Text\":\"OBY\",\"Code\":\"OB\"}],\"Characteristic\":[{\"Code\":\"PET_COUNT\",\"Value\":\"0\"}],\"IsInternational\":\"False\",\"AvailBookingClasses\":[{\"Code\":\"J\",\"Counts\":{\"Value\":\"6\"}},{\"Code\":\"JN\",\"Counts\":{\"Value\":\"6\"}},{\"Code\":\"C\",\"Counts\":{\"Value\":\"4\"}},{\"Code\":\"D\",\"Counts\":{\"Value\":\"0\"}},{\"Code\":\"Z\",\"Counts\":{\"Value\":\"0\"}},{\"Code\":\"ZN\",\"Counts\":{\"Value\":\"0\"}},{\"Code\":\"P\",\"Counts\":{\"Value\":\"0\"}},{\"Code\":\"PN\",\"Counts\":{\"Value\":\"0\"}},{\"Code\":\"PZ\",\"Counts\":{\"Value\":\"0\"}},{\"Code\":\"IN\",\"Counts\":{\"Value\":\"0\"}},{\"Code\":\"I\",\"Counts\":{\"Value\":\"0\"}},{\"Code\":\"Y\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"YN\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"B\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"M\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"E\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"U\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"H\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"HN\",\"Counts\":{\"Value\":\"7\"}},{\"Code\":\"Q\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"V\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"W\",\"Counts\":{\"Value\":\"6\"}},{\"Code\":\"S\",\"Counts\":{\"Value\":\"0\"}},{\"Code\":\"T\",\"Counts\":{\"Value\":\"0\"}},{\"Code\":\"L\",\"Counts\":{\"Value\":\"0\"}},{\"Code\":\"K\",\"Counts\":{\"Value\":\"0\"}},{\"Code\":\"G\",\"Counts\":{\"Value\":\"0\"}},{\"Code\":\"N\",\"Counts\":{\"Value\":\"9\"}},{\"Code\":\"XN\",\"Counts\":{\"Value\":\"0\"}},{\"Code\":\"X\",\"Counts\":{\"Value\":\"0\"}}],\"UpgradeEligibilityStatus\":0,\"UpgradeVisibilityType\":0,\"InstantUpgradable\":false},\"IsConnection\":\"False\",\"Characteristic\":[{\"Code\":\"SELECTED_FARE_TYPE\",\"Value\":\"ECONOMY\"}],\"SegmentNumber\":1,\"TripNumber\":\"1\"}],\"Prices\":[{\"BasePrice\":[{\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":213.02}],\"Taxes\":[{\"Code\":\"US\",\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":15.98,\"Description\":\"U.S. Transportation Tax\",\"Status\":\"APPLIED\"},{\"Code\":\"XF\",\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":4.5,\"Description\":\"U.S. Passenger Facility Charge\",\"Status\":\"APPLIED\"},{\"Code\":\"ZP\",\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":4.3,\"Description\":\"U.S. Flight Segment Tax\",\"Status\":\"APPLIED\"},{\"Code\":\"AY\",\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":5.6,\"Description\":\"September 11th Security Fee\",\"Status\":\"APPLIED\"}],\"Surcharges\":[],\"Fees\":[],\"Totals\":[{\"Name\":\"TaxTotal\",\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":30.38},{\"Name\":\"GrandTotalForMileage\",\"Currency\":{\"Code\":\"MIL\"},\"Amount\":0.0},{\"Name\":\"GrandTotalForCurrency\",\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":243.4}],\"BasePriceEquivalent\":{\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":213.02},\"Type\":{\"Key\":\"Revenue\",\"Description\":\"fareFlavor\",\"Value\":\"ADT\"},\"Rules\":[{\"Description\":\"NONREF/0VALUAFTDPT\"}],\"FareCalculation\":\"HOU UA LAX 213.02WAA1AKEN USD 213.02 END ZP IAH XT 15.98US 4.30ZP 5.60AY 4.50XF IAH4.5\",\"FareComponents\":[{\"AirlineCode\":\"UA\",\"OriginCity\":\"HOU\",\"DestinationCity\":\"LAX\",\"BasisCodes\":[\"WAA1AKEN\"],\"TripType\":\"ONE-WAY\",\"BasePrice\":[{\"Currency\":{\"Code\":\"USD\"},\"Amount\":213.02}],\"DestinationAirportCode\":\"LAX\",\"OriginAirportCode\":\"HOU\",\"SequenceNumber\":1,\"PassengerTypeCode\":\"ADT\",\"Characteristic\":[]}],\"Count\":{\"CountType\":\"Pax Count\",\"Value\":\"1\"},\"FareType\":0,\"PriceFlightSegments\":[{\"FareBasisCode\":\"WAA1AKEN\",\"TicketDesignator\":\"\",\"LOFNumber\":1,\"FareComponentSequenceNumber\":1,\"GlobalIndicator\":\"\",\"DepartureAirport\":{\"IATACode\":\"IAH\",\"ShortName\":\"IAH\"},\"ArrivalAirport\":{\"IATACode\":\"LAX\",\"ShortName\":\"LAX\"},\"DepartureDateTime\":\"2021-12-29 12:17\",\"ArrivalDateTime\":\"2021-12-29 14:02\",\"FlightNumber\":\"1230\",\"MarketedFlightSegment\":[{\"MarketingAirlineCode\":\"UA\"}],\"MarriageGroup\":\"N\",\"SegmentNumber\":1,\"BookingClasses\":[{\"Code\":\"W\"}],\"Legs\":[{\"Equipment\":{\"Model\":{\"Fleet\":\"739\"}},\"CabinCount\":2}],\"UpgradeEligibilityStatus\":0,\"UpgradeVisibilityType\":0,\"InstantUpgradable\":false,\"AuxillaryText\":\"False\"}],\"PassengerTypeCode\":\"ADT\",\"Characteristics\":[],\"PassengerIDs\":{\"Key\":\"1.1\",\"Description\":\"ADT\"}}],\"Type\":[{\"Key\":\"NEW\"},{\"Key\":\"REVENUE\"}],\"Characteristic\":[{\"Code\":\"Refundable\",\"Value\":\"False\"},{\"Code\":\"ContentLookupID\",\"Value\":\"Messages:63\"},{\"Code\":\"BBXID\",\"Value\":\"WBUQ6Y7wayRFpq5LQ0aRVT\"},{\"Code\":\"TRIP_TYPE\",\"Value\":\"OW\",\"Description\":\"TRIP_TYPE\"},{\"Code\":\"BBXID\",\"Value\":\"oT32QtqwiDFJZIb9W0aRVV\"},{\"Code\":\"StationCode\",\"Value\":\"HOU\"}],\"NumberInParty\":1,\"Pets\":[],\"GUID\":{\"ID\":\"C32D2A66-2BE3-4AB6-B21D-AF28D08FF8CC\"},\"PointOfSale\":{\"Country\":{\"CountryCode\":\"US\"},\"CurrencyCode\":\"USD\"},\"CartId\":\"C32D2A66-2BE3-4AB6-B21D-AF28D08FF8CC\"},\"Timings\":[],\"ShoppingCart\":{\"TimeStamps\":[{\"TransactionId\":\"7e3294f9-3abf-4c33-a1c3-8b8082339a4e\",\"FunctionName\":\"RegisterOfferAsync\",\"StartTime\":\"/Date(1636398644951)/\",\"Message\":\"LastUpdateUTCTimeStampToOmniChannelCart\"}],\"Items\":[{\"Product\":[{\"Description\":\"Reservation\",\"Code\":\"RES\",\"Price\":{\"BasePrice\":[{\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":213.02}],\"Taxes\":[{\"Code\":\"US\",\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":15.98,\"Description\":\"U.S. Transportation Tax\",\"Status\":\"APPLIED\"},{\"Code\":\"XF\",\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":4.5,\"Description\":\"U.S. Passenger Facility Charge\",\"Status\":\"APPLIED\"},{\"Code\":\"ZP\",\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":4.3,\"Description\":\"U.S. Flight Segment Tax\",\"Status\":\"APPLIED\"},{\"Code\":\"AY\",\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":5.6,\"Description\":\"September 11th Security Fee\",\"Status\":\"APPLIED\"}],\"Totals\":[{\"Name\":\"TaxTotal\",\"Currency\":{\"Code\":\"USD\"},\"Amount\":30.38},{\"Name\":\"GrandTotalForMileage\",\"Currency\":{\"Code\":\"MIL\"},\"Amount\":0.0},{\"Name\":\"GrandTotalForCurrency\",\"Currency\":{\"Code\":\"USD\"},\"Amount\":243.4}],\"FareType\":0}}],\"Quantity\":1,\"ID\":{\"ID\":\"2fb39319-99af-4aca-ba06-04fd85535366\"},\"ProductContext\":[\"{\\\"Travelers\\\":[{\\\"Person\\\":{\\\"Key\\\":\\\"1.1\\\",\\\"Surname\\\":\\\"chanda\\\",\\\"GivenName\\\":\\\"Sai kiran\\\",\\\"MiddleName\\\":\\\"\\\",\\\"Suffix\\\":\\\"\\\",\\\"DateOfBirth\\\":\\\"11/07/1981\\\",\\\"Sex\\\":\\\"M\\\",\\\"Documents\\\":[{\\\"DateOfBirth\\\":\\\"11/07/1981\\\",\\\"GivenName\\\":\\\"Sai kiran\\\",\\\"Surname\\\":\\\"chanda\\\",\\\"Suffix\\\":\\\"\\\",\\\"Sex\\\":\\\"M\\\",\\\"Type\\\":0,\\\"KnownTravelerNumber\\\":\\\"\\\",\\\"MiddleName\\\":\\\"\\\"}],\\\"Type\\\":\\\"ADT\\\",\\\"InfantIndicator\\\":\\\"false\\\",\\\"ReservationIndex\\\":\\\"0\\\",\\\"Contact\\\":{\\\"Emails\\\":[],\\\"PhoneNumbers\\\":[]},\\\"PricingPaxType\\\":\\\"ADT\\\"},\\\"LoyaltyProgramProfile\\\":{\\\"LoyaltyProgramMemberTierDescription\\\":9}}],\\\"TelephoneNumbers\\\":[],\\\"FlightSegments\\\":[{\\\"FlightSegment\\\":{\\\"DepartureAirport\\\":{\\\"IATACode\\\":\\\"IAH\\\",\\\"Name\\\":\\\"Houston, TX, US (IAH)\\\",\\\"IATACountryCode\\\":{\\\"CountryCode\\\":\\\"US\\\"},\\\"StateProvince\\\":{\\\"StateProvinceCode\\\":\\\"TX\\\"}},\\\"ArrivalAirport\\\":{\\\"IATACode\\\":\\\"LAX\\\",\\\"Name\\\":\\\"Los Angeles, CA, US (LAX)\\\",\\\"IATACountryCode\\\":{\\\"CountryCode\\\":\\\"US\\\"},\\\"StateProvince\\\":{\\\"StateProvinceCode\\\":\\\"CA\\\"}},\\\"DepartureDateTime\\\":\\\"2021-12-29 12:17\\\",\\\"ArrivalDateTime\\\":\\\"2021-12-29 14:02\\\",\\\"FlightNumber\\\":\\\"1230\\\",\\\"OperatingAirlineCode\\\":\\\"UA\\\",\\\"Equipment\\\":{\\\"Model\\\":{\\\"Description\\\":\\\"Boeing 737-900\\\",\\\"Fleet\\\":\\\"739\\\"},\\\"CabinCount\\\":2,\\\"SeatmapLegendList\\\":null},\\\"MarketedFlightSegment\\\":[{\\\"MarketingAirlineCode\\\":\\\"UA\\\",\\\"FlightNumber\\\":\\\"1230\\\"}],\\\"MarriageGroup\\\":\\\"N\\\",\\\"Distance\\\":1375,\\\"SegmentNumber\\\":1,\\\"BookingClasses\\\":[{\\\"Code\\\":\\\"W\\\",\\\"Cabin\\\":{\\\"Key\\\":\\\"UE\\\",\\\"Name\\\":\\\"United Economy\\\",\\\"Layout\\\":\\\"2\\\"}}],\\\"FlightSegmentType\\\":\\\"NN\\\",\\\"TravelerCounts\\\":[{\\\"Counts\\\":[{\\\"Value\\\":\\\"1\\\"}]}],\\\"Message\\\":[{\\\"Code\\\":\\\"ARRDAYDIFF\\\"},{\\\"Text\\\":\\\"WAA1AKEN\\\",\\\"Code\\\":\\\"BASIS_CODE\\\"},{\\\"Text\\\":\\\"False\\\",\\\"Code\\\":\\\"PETS_DISALLOWED\\\"},{\\\"Text\\\":\\\"United Airlines\\\",\\\"Code\\\":\\\"OPERATINGCARRIERDESC\\\"},{\\\"Text\\\":\\\"OBY\\\",\\\"Code\\\":\\\"OB\\\"}],\\\"Characteristic\\\":[{\\\"Code\\\":\\\"PET_COUNT\\\",\\\"Value\\\":\\\"0\\\"}],\\\"IsInternational\\\":\\\"False\\\",\\\"AvailBookingClasses\\\":[{\\\"Code\\\":\\\"J\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"6\\\"}},{\\\"Code\\\":\\\"JN\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"6\\\"}},{\\\"Code\\\":\\\"C\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"4\\\"}},{\\\"Code\\\":\\\"D\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}},{\\\"Code\\\":\\\"Z\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}},{\\\"Code\\\":\\\"ZN\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}},{\\\"Code\\\":\\\"P\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}},{\\\"Code\\\":\\\"PN\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}},{\\\"Code\\\":\\\"PZ\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}},{\\\"Code\\\":\\\"IN\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}},{\\\"Code\\\":\\\"I\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}},{\\\"Code\\\":\\\"Y\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"9\\\"}},{\\\"Code\\\":\\\"YN\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"9\\\"}},{\\\"Code\\\":\\\"B\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"9\\\"}},{\\\"Code\\\":\\\"M\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"9\\\"}},{\\\"Code\\\":\\\"E\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"9\\\"}},{\\\"Code\\\":\\\"U\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"9\\\"}},{\\\"Code\\\":\\\"H\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"9\\\"}},{\\\"Code\\\":\\\"HN\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"7\\\"}},{\\\"Code\\\":\\\"Q\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"9\\\"}},{\\\"Code\\\":\\\"V\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"9\\\"}},{\\\"Code\\\":\\\"W\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"6\\\"}},{\\\"Code\\\":\\\"S\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}},{\\\"Code\\\":\\\"T\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}},{\\\"Code\\\":\\\"L\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}},{\\\"Code\\\":\\\"K\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}},{\\\"Code\\\":\\\"G\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}},{\\\"Code\\\":\\\"N\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"9\\\"}},{\\\"Code\\\":\\\"XN\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}},{\\\"Code\\\":\\\"X\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}}],\\\"UpgradeEligibilityStatus\\\":0,\\\"UpgradeVisibilityType\\\":0,\\\"InstantUpgradable\\\":false},\\\"IsConnection\\\":\\\"False\\\",\\\"Characteristic\\\":[{\\\"Code\\\":\\\"SELECTED_FARE_TYPE\\\",\\\"Value\\\":\\\"ECONOMY\\\"}],\\\"SegmentNumber\\\":1,\\\"TripNumber\\\":\\\"1\\\"}],\\\"Prices\\\":[{\\\"BasePrice\\\":[{\\\"Currency\\\":{\\\"Code\\\":\\\"USD\\\",\\\"DecimalPlace\\\":2},\\\"Amount\\\":213.02}],\\\"Taxes\\\":[{\\\"Code\\\":\\\"US\\\",\\\"Currency\\\":{\\\"Code\\\":\\\"USD\\\",\\\"DecimalPlace\\\":2},\\\"Amount\\\":15.98,\\\"Description\\\":\\\"U.S. Transportation Tax\\\",\\\"Status\\\":\\\"APPLIED\\\"},{\\\"Code\\\":\\\"XF\\\",\\\"Currency\\\":{\\\"Code\\\":\\\"USD\\\",\\\"DecimalPlace\\\":2},\\\"Amount\\\":4.5,\\\"Description\\\":\\\"U.S. Passenger Facility Charge\\\",\\\"Status\\\":\\\"APPLIED\\\"},{\\\"Code\\\":\\\"ZP\\\",\\\"Currency\\\":{\\\"Code\\\":\\\"USD\\\",\\\"DecimalPlace\\\":2},\\\"Amount\\\":4.3,\\\"Description\\\":\\\"U.S. Flight Segment Tax\\\",\\\"Status\\\":\\\"APPLIED\\\"},{\\\"Code\\\":\\\"AY\\\",\\\"Currency\\\":{\\\"Code\\\":\\\"USD\\\",\\\"DecimalPlace\\\":2},\\\"Amount\\\":5.6,\\\"Description\\\":\\\"September 11th Security Fee\\\",\\\"Status\\\":\\\"APPLIED\\\"}],\\\"Surcharges\\\":[],\\\"Fees\\\":[],\\\"Totals\\\":[{\\\"Name\\\":\\\"TaxTotal\\\",\\\"Currency\\\":{\\\"Code\\\":\\\"USD\\\",\\\"DecimalPlace\\\":2},\\\"Amount\\\":30.38},{\\\"Name\\\":\\\"GrandTotalForMileage\\\",\\\"Currency\\\":{\\\"Code\\\":\\\"MIL\\\"},\\\"Amount\\\":0.0},{\\\"Name\\\":\\\"GrandTotalForCurrency\\\",\\\"Currency\\\":{\\\"Code\\\":\\\"USD\\\",\\\"DecimalPlace\\\":2},\\\"Amount\\\":243.4}],\\\"BasePriceEquivalent\\\":{\\\"Currency\\\":{\\\"Code\\\":\\\"USD\\\",\\\"DecimalPlace\\\":2},\\\"Amount\\\":213.02},\\\"Type\\\":{\\\"Key\\\":\\\"Revenue\\\",\\\"Description\\\":\\\"fareFlavor\\\",\\\"Value\\\":\\\"ADT\\\"},\\\"Rules\\\":[{\\\"Description\\\":\\\"NONREF/0VALUAFTDPT\\\"}],\\\"FareCalculation\\\":\\\"HOU UA LAX 213.02WAA1AKEN USD 213.02 END ZP IAH XT 15.98US 4.30ZP 5.60AY 4.50XF IAH4.5\\\",\\\"FareComponents\\\":[{\\\"AirlineCode\\\":\\\"UA\\\",\\\"OriginCity\\\":\\\"HOU\\\",\\\"DestinationCity\\\":\\\"LAX\\\",\\\"BasisCodes\\\":[\\\"WAA1AKEN\\\"],\\\"TripType\\\":\\\"ONE-WAY\\\",\\\"BasePrice\\\":[{\\\"Currency\\\":{\\\"Code\\\":\\\"USD\\\"},\\\"Amount\\\":213.02}],\\\"DestinationAirportCode\\\":\\\"LAX\\\",\\\"OriginAirportCode\\\":\\\"HOU\\\",\\\"SequenceNumber\\\":1,\\\"PassengerTypeCode\\\":\\\"ADT\\\",\\\"Characteristic\\\":[]}],\\\"Count\\\":{\\\"CountType\\\":\\\"Pax Count\\\",\\\"Value\\\":\\\"1\\\"},\\\"FareType\\\":0,\\\"PriceFlightSegments\\\":[{\\\"FareBasisCode\\\":\\\"WAA1AKEN\\\",\\\"TicketDesignator\\\":\\\"\\\",\\\"LOFNumber\\\":1,\\\"FareComponentSequenceNumber\\\":1,\\\"GlobalIndicator\\\":\\\"\\\",\\\"DepartureAirport\\\":{\\\"IATACode\\\":\\\"IAH\\\",\\\"ShortName\\\":\\\"IAH\\\"},\\\"ArrivalAirport\\\":{\\\"IATACode\\\":\\\"LAX\\\",\\\"ShortName\\\":\\\"LAX\\\"},\\\"DepartureDateTime\\\":\\\"2021-12-29 12:17\\\",\\\"ArrivalDateTime\\\":\\\"2021-12-29 14:02\\\",\\\"FlightNumber\\\":\\\"1230\\\",\\\"MarketedFlightSegment\\\":[{\\\"MarketingAirlineCode\\\":\\\"UA\\\"}],\\\"MarriageGroup\\\":\\\"N\\\",\\\"SegmentNumber\\\":1,\\\"BookingClasses\\\":[{\\\"Code\\\":\\\"W\\\"}],\\\"Legs\\\":[{\\\"Equipment\\\":{\\\"Model\\\":{\\\"Fleet\\\":\\\"739\\\"},\\\"SeatmapLegendList\\\":null},\\\"CabinCount\\\":2}],\\\"UpgradeEligibilityStatus\\\":0,\\\"UpgradeVisibilityType\\\":0,\\\"InstantUpgradable\\\":false,\\\"AuxillaryText\\\":\\\"False\\\"}],\\\"PassengerTypeCode\\\":\\\"ADT\\\",\\\"Characteristics\\\":[],\\\"PassengerIDs\\\":{\\\"Key\\\":\\\"1.1\\\",\\\"Description\\\":\\\"ADT\\\"}}],\\\"Type\\\":[{\\\"Key\\\":\\\"NEW\\\"},{\\\"Key\\\":\\\"REVENUE\\\"}],\\\"Characteristic\\\":[{\\\"Code\\\":\\\"Refundable\\\",\\\"Value\\\":\\\"False\\\"},{\\\"Code\\\":\\\"ContentLookupID\\\",\\\"Value\\\":\\\"Messages:63\\\"},{\\\"Code\\\":\\\"BBXID\\\",\\\"Value\\\":\\\"WBUQ6Y7wayRFpq5LQ0aRVT\\\"},{\\\"Code\\\":\\\"TRIP_TYPE\\\",\\\"Value\\\":\\\"OW\\\",\\\"Description\\\":\\\"TRIP_TYPE\\\"},{\\\"Code\\\":\\\"BBXID\\\",\\\"Value\\\":\\\"oT32QtqwiDFJZIb9W0aRVV\\\"},{\\\"Code\\\":\\\"StationCode\\\",\\\"Value\\\":\\\"HOU\\\"}],\\\"NumberInParty\\\":1,\\\"Pets\\\":[],\\\"GUID\\\":{\\\"ID\\\":\\\"C32D2A66-2BE3-4AB6-B21D-AF28D08FF8CC\\\",\\\"LanguageCode\\\":null},\\\"PointOfSale\\\":{\\\"Country\\\":{\\\"CountryCode\\\":\\\"US\\\"},\\\"CurrencyCode\\\":\\\"USD\\\"},\\\"CartId\\\":\\\"C32D2A66-2BE3-4AB6-B21D-AF28D08FF8CC\\\"}\"]},{\"Product\":[{\"Description\":\"Premier Access and Club Trip Pass\",\"Code\":\"B07\",\"Price\":{\"Totals\":[{\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":105.0}],\"FareType\":0},\"SubProducts\":[{\"Descriptions\":[\"Premier Access and Club Trip Pass\"],\"Code\":\"B07\",\"Prices\":[{\"ID\":\"B7-SOL1_OD1_S1_0\",\"Association\":{\"SegmentRefIDs\":[\"S1\"],\"TravelerRefIDs\":[\"0\"],\"ODMappings\":[{\"SegmentRefIDs\":[\"S1\"],\"RefID\":\"OD1\"}]},\"PaymentOptions\":[{\"Type\":\"Money\",\"EDDCode\":\"B07\",\"PriceComponents\":[{\"Price\":{\"BasePrice\":[{\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":105.0,\"Type\":\"Money\"}],\"Totals\":[{\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":105.0,\"Type\":\"Money\"}],\"FareType\":0},\"Characteristics\":[{\"Code\":\"RFIC\",\"Value\":\"A\"}]}],\"PriceDesignator\":\"70\"}]}],\"Name\":\"Premier Access and Club Trip Pass\",\"ID\":\"9\",\"IsDefault\":\"False\",\"GroupCode\":\"BE\",\"SubGroupCode\":\"B7\",\"Extension\":{\"Bundle\":{\"Products\":[{\"Code\":\"PAS\",\"SubProducts\":[{\"Descriptions\":[\"Premier Access\"],\"Code\":\"PAS\",\"Prices\":[{\"ID\":\"S1_0\",\"Association\":{\"SegmentRefIDs\":[\"S1\"],\"TravelerRefIDs\":[\"0\"]},\"PaymentOptions\":[{\"Type\":\"Money\",\"EDDCode\":\"PA1\",\"PriceComponents\":[{\"Price\":{\"BasePrice\":[{\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":51.0,\"Type\":\"Money\"}],\"Totals\":[{\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":51.0,\"Type\":\"Money\"}],\"FareType\":0,\"Adjustments\":[{\"Type\":\"Discount\",\"Input\":56.0,\"Result\":50.4,\"Percentage\":10.0,\"Value\":-5.6,\"AppliedTo\":56.0,\"Designator\":\"BUNDLEDISC\"},{\"Type\":\"Rounding\",\"Input\":50.4,\"Result\":51.0}]},\"Characteristics\":[{\"Code\":\"RFIC\",\"Value\":\"J\"}],\"ActualPrice\":{\"BasePrice\":[{\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":56.0,\"Type\":\"Money\"}],\"Totals\":[{\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":56.0,\"Type\":\"Money\"}],\"FareType\":0,\"Adjustments\":[{\"Type\":\"Premium\",\"Input\":51.0,\"Result\":56.0,\"Value\":5.0,\"Designator\":\"TBAPRM\"},{\"Type\":\"Rounding\",\"Input\":56.0,\"Result\":56.0}]}}]}]}],\"Name\":\"Premier Access\",\"ID\":\"1\",\"IsDefault\":\"False\",\"GroupCode\":\"PA\",\"SubGroupCode\":\"CSB\",\"IsEntitled\":\"False\",\"Features\":[{\"Type\":0,\"Value\":\"PRIORITY_CHECKIN\"},{\"Type\":2,\"Value\":\"PRIORITY_SECURITY\"},{\"Type\":1,\"Value\":\"PRIORITY_BOARDING\"}]}],\"Rank\":\"1\"},{\"Code\":\"CTP\",\"SubProducts\":[{\"Descriptions\":[\"Trip Pass\"],\"Code\":\"CTP\",\"Prices\":[{\"ID\":\"S1_0\",\"Association\":{\"SegmentRefIDs\":[\"S1\"],\"TravelerRefIDs\":[\"0\"]},\"PaymentOptions\":[{\"Type\":\"Money\",\"EDDCode\":\"CP1\",\"PriceComponents\":[{\"Price\":{\"BasePrice\":[{\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":54.0,\"Type\":\"Money\"}],\"Totals\":[{\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":54.0,\"Type\":\"Money\"}],\"FareType\":0,\"Adjustments\":[{\"Type\":\"Discount\",\"Input\":60.0,\"Result\":54.0,\"Percentage\":10.0,\"Value\":-6.0,\"AppliedTo\":60.0,\"Designator\":\"BUNDLEDISC\"}]},\"Characteristics\":[{\"Code\":\"RFIC\",\"Value\":\"J\"}],\"ActualPrice\":{\"BasePrice\":[{\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":60.0,\"Type\":\"Money\"}],\"Totals\":[{\"Currency\":{\"Code\":\"USD\",\"DecimalPlace\":2},\"Amount\":60.0,\"Type\":\"Money\"}],\"FareType\":0,\"Adjustments\":[{\"Type\":\"Discount\",\"Input\":60.0,\"Result\":60.0,\"AppliedTo\":60.0,\"Designator\":\"NODEPCBPCLUB\"},{\"Type\":\"Rounding\",\"Input\":60.0,\"Result\":60.0}]}}]}]}],\"Name\":\"Trip Pass\",\"ID\":\"1\",\"IsDefault\":\"False\",\"GroupCode\":\"TP\",\"SubGroupCode\":\"CTP\",\"Extension\":{\"AdditionalExtensions\":[{\"Characteristics\":[{\"Code\":\"IAH\",\"Value\":\"true\"},{\"Code\":\"LAX\",\"Value\":\"true\"}]}]},\"IsEntitled\":\"False\"}],\"Rank\":\"2\"}]}},\"IsEntitled\":\"False\",\"Association\":{\"SegmentRefIDs\":[\"S1\"],\"TravelerRefIDs\":[\"0\"],\"ODMappings\":[{\"SegmentRefIDs\":[\"S1\"],\"RefID\":\"OD1\"}]}}],\"Characteristics\":[{\"Code\":\"TripIndex\",\"Value\":\"1\",\"Description\":\"OD Index\"},{\"Code\":\"IsBundle\",\"Value\":\"True\"}]}],\"Quantity\":1,\"ID\":{\"ID\":\"eea8a2a5-5a7f-47f7-96ca-0e71bef72da4\"},\"ProductContext\":[\"{\\\"FlightSegments\\\":[{\\\"ArrivalAirport\\\":{\\\"IATACode\\\":\\\"LAX\\\"},\\\"ArrivalDateTime\\\":\\\"2021-12-29 14:02\\\",\\\"AvailBookingClasses\\\":[{\\\"Code\\\":\\\"J\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"6\\\"}},{\\\"Code\\\":\\\"JN\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"6\\\"}},{\\\"Code\\\":\\\"C\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"4\\\"}},{\\\"Code\\\":\\\"D\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}},{\\\"Code\\\":\\\"Z\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}},{\\\"Code\\\":\\\"ZN\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}},{\\\"Code\\\":\\\"P\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}},{\\\"Code\\\":\\\"PN\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}},{\\\"Code\\\":\\\"PZ\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}},{\\\"Code\\\":\\\"IN\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}},{\\\"Code\\\":\\\"I\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}},{\\\"Code\\\":\\\"Y\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"9\\\"}},{\\\"Code\\\":\\\"YN\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"9\\\"}},{\\\"Code\\\":\\\"B\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"9\\\"}},{\\\"Code\\\":\\\"M\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"9\\\"}},{\\\"Code\\\":\\\"E\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"9\\\"}},{\\\"Code\\\":\\\"U\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"9\\\"}},{\\\"Code\\\":\\\"H\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"9\\\"}},{\\\"Code\\\":\\\"HN\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"7\\\"}},{\\\"Code\\\":\\\"Q\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"9\\\"}},{\\\"Code\\\":\\\"V\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"9\\\"}},{\\\"Code\\\":\\\"W\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"6\\\"}},{\\\"Code\\\":\\\"S\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}},{\\\"Code\\\":\\\"T\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}},{\\\"Code\\\":\\\"L\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}},{\\\"Code\\\":\\\"K\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}},{\\\"Code\\\":\\\"G\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}},{\\\"Code\\\":\\\"N\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"9\\\"}},{\\\"Code\\\":\\\"XN\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}},{\\\"Code\\\":\\\"X\\\",\\\"Counts\\\":{\\\"Value\\\":\\\"0\\\"}}],\\\"BookingClasses\\\":[{\\\"Cabin\\\":{\\\"Description\\\":\\\"United Economy\\\",\\\"Key\\\":\\\"UE\\\",\\\"Layout\\\":\\\"2\\\",\\\"Name\\\":\\\"United Economy\\\"},\\\"Code\\\":\\\"W\\\"}],\\\"DepartureAirport\\\":{\\\"IATACode\\\":\\\"IAH\\\"},\\\"DepartureDateTime\\\":\\\"2021-12-29 12:17\\\",\\\"DirectionIndicator\\\":\\\"\\\",\\\"Distance\\\":1375,\\\"Equipment\\\":{\\\"CabinCount\\\":2,\\\"Model\\\":{\\\"Fleet\\\":\\\"739\\\"},\\\"SeatmapLegendList\\\":null},\\\"FlightNumber\\\":\\\"1230\\\",\\\"InstantUpgradable\\\":false,\\\"IsInternational\\\":\\\"FALSE\\\",\\\"MarketedFlightSegment\\\":[{\\\"Description\\\":\\\"United Airlines\\\",\\\"FlightNumber\\\":\\\"1230\\\",\\\"MarketingAirlineCode\\\":\\\"UA\\\"}],\\\"OperatingAirline\\\":{\\\"MemberName\\\":\\\"United Airlines\\\",\\\"IATACode\\\":\\\"UA\\\"},\\\"OperatingAirlineCode\\\":\\\"UA\\\",\\\"SegmentNumber\\\":1,\\\"UpgradeEligibilityStatus\\\":0,\\\"UpgradeVisibilityType\\\":0,\\\"ClassOfService\\\":\\\"W\\\",\\\"ID\\\":\\\"S1\\\",\\\"IsActive\\\":\\\"True\\\",\\\"TripIndicator\\\":\\\"1\\\"}],\\\"ODOptions\\\":[{\\\"Characteristics\\\":[{\\\"Code\\\":\\\"SELECTED_FARE_TYPE\\\",\\\"Value\\\":\\\"ECONOMY\\\"}],\\\"FlightSegments\\\":[{\\\"ArrivalAirport\\\":{\\\"IATACode\\\":\\\"LAX\\\",\\\"IATACountryCode\\\":{\\\"CountryCode\\\":\\\"US\\\"},\\\"Name\\\":\\\"Los Angeles, CA, US (LAX)\\\"},\\\"ArrivalDateTime\\\":\\\"2021-12-29 14:02\\\",\\\"DepartureAirport\\\":{\\\"IATACode\\\":\\\"IAH\\\",\\\"IATACountryCode\\\":{\\\"CountryCode\\\":\\\"US\\\"},\\\"Name\\\":\\\"Houston, TX, US (IAH)\\\"},\\\"DepartureDateTime\\\":\\\"2021-12-29 12:17\\\",\\\"InstantUpgradable\\\":false,\\\"UpgradeEligibilityStatus\\\":0,\\\"UpgradeVisibilityType\\\":0,\\\"ID\\\":\\\"S1\\\",\\\"RefID\\\":\\\"S1\\\"}],\\\"ID\\\":\\\"OD1\\\"}],\\\"Offers\\\":[{\\\"ProductInformation\\\":{\\\"ProductDetails\\\":[{\\\"Parameters\\\":[{\\\"Name\\\":\\\"SERVICEDATA\\\",\\\"Value\\\":\\\"H4sIAAAAAAAEAO1d\\\\/XPaOBPWn5I3P7e9AAk0N52b4SNpuSMfbyC99m4ynTSQHG8TyEHSu0yn\\\\/\\\\/u7Wkm2bGywQZDVupMBgr1arfbZXT2SwbwRXTEQE\\\\/FVDMUV\\\\/NcSl+IBHr+IN6ItRnB8AM8PIDEWT3j0p8TjW3DmFN\\\\/1xSNoekDZ\\\\/4g\\\\/RRN01kUPHn+Kb2Ib3g3EFCQm0OM9yA2hzQiO\\\\/wwPqWEg7uCYtGkL2lyhVVP42wKrRqB9CzTeQh+f4b+e1rIFLS9Ralu8gEcT7RhorQ2xI2r6TBdbRi2dotw864yEGwsvFth4im2U1tCyNtimZLviRHRESXyC1xa+dvF5R7evYz9j0DBENEMPf8PWA3EDI1D4ncHrNeoOxyi1GSul\\\\/ZeI9i2OOEl+J5CW9hyB\\\\/D38DUH\\\\/TWQE+XoOz6tRq7Fui+9w\\\\/rv20yXEntF3koDXNxzBE5wxfj7C8wM4pnx1AFpbmbCQMnegSbUfxeImlDN+biDeg9gZJd2E6JhgJI3gzFPQJmrHOfijpe2QcSnxvAOtt6DzFl6NzrL2Rx3OjkGztE0eL8E49sQreH4x1w\\\\/fA6zHmPu3kZHRtPUQdEqbbMkdrbsp\\\\/sKzl5jdA\\\\/T\\\\/FGPjKjYyewRnoLMNx9QY3qPtj8HZehB5F7G4UBVjCJE9wmwbwxnVpoaZYbc6Bom7QKerahfmyH5wZIp2XYPcI7R\\\\/0OcPNboDLfcW6+Aj6IxnwEGkXqbLmTw5EP+ir0foC7veNBDlPtYPcyy9\\\\/kb1n4JPuhuo3Mk12e59cU1+\\\\/gq83qoovVFiUBX3YBR+1MR8liZXRKW3L\\\\/4HemUNvEtAKaq7hdXyyrJEVZQRyDwGllUDy84wJ8IqswfHX4ldHScyYq90n5cQ9aqXEsrsJFTalzgLVLXdkkfcYsb2EYl43+m1twHeP4bzHYzlNiDRRF+9SBjxmR5rX3OW5BGHo5odscIpZCbrmIF+tbCuo95HHUsUc6zqTY7lsdRVjoVz0SM8J8dbaU6GVVOyZy9DZvQgPupgwRmMcLWMmFcDqlZGGAaUnwXF+U1JuOE3p8jnFvObJpxvWH0eIK5DeNxiTUru9RD+u8T8nMT4wmzcxLNc4tKGWVs+98RH4BRN8Q76bYrf4MhxCmLljLq6qOk8OJKsrZRRWwPe1eF9Cy17K6Jc9wwZ7BcLOdNX3Mc90O2G42Vdbdt9\\\\/mB2TdDMg9ntejPr5LF0k8yuajGz2aoeWr0Mswvni2RmZ\\\\/edn9n94F6zCNLOgnyWUskC+9wqcXyMO5oH4KsmRI187oBHG2IVTpbF7iycbPFOkysmFp\\\\/705lYKJm+1yQjoY\\\\/8TB2XNsxKRyN9mWrRhp7epdQKeU1EvUvmOx1o+yFD2wurpmXjnsnMqxxoy6Pr+biOlN7cdQTjM1UV7rFuPa1pnzaJEV\\\\/An7qqNnv1LO6zaeI1NVPjorJmBG20bgA5YHp9FeRuD6P8C5x9AHm5Ev2MbZVkPOMNAzZjGiAuUt7UqWic16FlL6Enhae6zjgIrCqBB17Dowz9lOHdFjwUm5BVfgd9eWRViglmd4jRFeaxHOtlUHGG+iplFM0uvBuCvi\\\\/wPME2ZgZTFp1AtDTw3APotO2rwaMk9sFOZV9Z22YsrFsWhh43x5p6hhqjLZOYZ1UkmFhIRj56pVbV93td0Qap11vfwqxiZ0d0dfMSRlHFsdREBV534d12wowg0XyPbEutAM2830EP3iAfuonFwAhnhNuglpnRLR5HHf3f1tEyQG9M9PXnpDGeQJ9p6yGT\\\\/\\\\/2ITYpR3OLM\\\\/Be0i9aUbO0VE1RRpvJM+eqj9k0DbBljnEkPNbG2qMyJzizmeDR\\\\/fg\\\\/ixjAV1WonqFlp+utgkYxx+f4zvspK\\\\/5Shz19T+qxac1lay+Ol2zZTWu4ubNma46P5Lf9YoWXaSBe3PV2h5Sq9Lj\\\\/a9gr9tpdu+TGl5X6Glmn2Lm7bWLrl0dItD5Zueb50y3crtEzzbm1h2\\\\/8u3ev7pVum1dDFNamb0nJx7PaWbtlZuuVvS7d8u3TL5fPswwo15cOcluaTGXF+as\\\\/zHb2+k\\\\/P1tV6v23O6YiOKn4RrnumMnJx7JScxq\\\\/Fw3RruS8Tn51lmJKWmEeYR3Tey5Zp6Nh\\\\/N1IERzu8DXIUc4M7GCFnmU+CrS9zNUeuXNDuamv\\\\/e4B7FU8J5g4hkyFEOF822FrK3gWZH0pq23re4iu2BhPJT3IUaWdpKwEdrYk+YNb\\\\/CcAg+kJz4HDx9g7ywr9lXEpoH4m94J9dixr+GcVQgKszaRHGltBWQ2XdSO3vDmYg0UmGMtXB9El0xh+snW3K2T8nEK4GsXKsfWegphnkdrNONjLyeFfeQlDT7CeG6f1ZmVpvMgb+x3VTHVVxXB+PEsFiVd8pOe42nWPyt5vAmWs2VtxO9ByGRUbvJ1xgZd5E4sNE8wVWm8eziHk7h6EQz+2N9pdVc3Yry\\\\/S5G3wOu0OK9mtV8WgzbO2FSqzyr9i5MLpv9uqgPo7It9JuKRGXDNxFfg9lRG1aD6Lpuipmh\\\\/KT2ZL8koNzBXY5xsG8o1zINrB5hVoR7j3u6Mo7FP1bdDc\\\\/IY7fo31HieTPSfPtu0doT9pDsmxDzMWapnXsla+crujZKyhM1znhmlvSKL3llFY+fUmTcxpe2xfbo0j8RbUsZbUk1I7yaHd0Zu9c7GOFK\\\\/MVavN+Y4\\\\/0ye+83ntn7Byt5P1oZXPi\\\\/smH\\\\/Hzyz\\\\/w\\\\/n+H9x7fHf\\\\/4fP7P911\\\\/4y4epTZl\\\\/7aXvf79rvu\\\\/f9rvy+e9\\\\/vup9l3q0Qnncrnld+\\\\/\\\\/3Pvfbn8\\\\/5mq0\\\\/F89rvf\\\\/T7Xf0XR\\\\/8u4ejf9bz2++597pWftvf9rvy+e5973d8j7P099nWftve5133a3ude92l7P3vdvwgQWO16dFNs4Tdkkq5J71p+jF5zruiVUPpV6VpkrOu8Km0+n7q+HZJarpjZWxgz8q415pN6q8ZMLRg\\\\/xXq5VwD\\\\/c6+Y+by\\\\/uGK69T53pvzauferzrz\\\\/mjRTXlx7fPf+czPl5hzvu2DKtL3ffGbvr8Z7nsP78nO3753532\\\\/e47\\\\/\\\\/ufMe6v7nznz2Sft\\\\/nz33oe5\\\\/7uyHuv+58x\\\\/q\\\\/ufOf6j7nzv\\\\/oe5\\\\/7vynFNz\\\\/iiYC0j7uHMgHDPzmQdm+mZMPhcWf0nOPAnc25EMmcGdEPmDAnRX5gAF\\\\/ZuT6u8quMaD9bXE3tYg+Bn4zIx4Y8OdF9DHgz4voY8CfF9HHgD8vcv2tBuW3P5xhQPs7VW5qEX0M+PMi+hjw50X0MeDPi+hj4DcvyrZ7nQ+FbLvXblHgz4xcf+\\\\/HNQa0v3XophrRx4A\\\\/M6KPAX9mRB8D\\\\/syIPgZ+MyMeGHDnReW1XM3sOUKgXIBPGNFHgDsnoo8Ad0ZEHwHufIg+AtzZEH0E+HMh91cwyw4R4P+ZIvoI8OdC1BHwmwtluV6QF4Ms1wvcYuA3G+KBAX8+RL0S8edDrn8Zwe3+XJn477K4YUTuMSg7RYA\\\\/I6KfBUXgRPlQ2PRnKKR9RWBF7lFwW438ZkVcMoE\\\\/M3L9qy2uMaD9u0WuqlE+FLLcF9ZlNXr+3y5a\\\\/zfz14GB60zwmx3xqEZ+c6PnwcBtLfKbGfHIAv68iPb3DcoF+P7ZOjBwWYn4f\\\\/vMhyzwmxNlY6a0v3VTLsD3z+jXIv6siH4t4s+KaH\\\\/Ovez9d8+yzQf5UNj0bpHv3z3jgYHfvIhHLeLPitxj4DYL+LMi+lnAnxVViWNQ9ZwVPQ8GLitR1XNOxCML+HMi+hjw50TUKxF\\\\/TkQ\\\\/C\\\\/zmRNlWyPl+bXfTn7CrFYATuf+9Y7cI8OdE1BHwmxHxqEN+MyIeGPDnRNQrUREYUb7f4d10FrwuACNy\\\\/0vIbhHwmxHxyIIicCLqGBSBE1HHwG9OxAODIrCifL\\\\/Ou2kM9gvAitz\\\\/PrJbBIrAiqhnQRFYEXUMisCKqGNQBFZEHQO\\\\/WdHiGbmyhjtoukOgUoB7WdNHgD8nyovBZutQxfu7WfPAgD8noo+B35yIw2zgNyPKlgXruI+pOwz439M6LwKbzgL+97Smj4DfjIgDAn7zIQ4I8GdD1BEoAhuifP9SaR1\\\\/NkT5ftbSOv5siDoC\\\\/NkQdQT4syHqCPBnQ9QRKAIbcn+HNJcY+H6vRh4Y+M2HeGDgNyPigQF\\\\/TkT5Xo2VAtyrkT4CfnOiLAhQvjNXxfv7NHJAwG8+xAEBv9kQBwT4cyHqCPDnQtQR4M+FKN+Rq1KAuzPSR4A\\\\/F6KOAH8uRB0B\\\\/lyIOgL8uRB1BPhzIcr3gaoU4K6M9BHgz4WoI8CfC1FHgD8Xoo4Afy5EHQH+XIjy3QArBbgfI30E+HMh6gjw50LUEeDPhagjwJ8LUUcgOxe60A\\\\/jk6Y4EW+hvRzLDUg\\\\/oHz0k4RKTlo1giNXERujkklSLeh7graHrdT9Ck5wzDfwGIGtA\\\\/zPSGxHvPOAMqGONOk6aJQ6v6JVTYybz2D\\\\/SPRnsJaa7zEOH9DCwQJ56YUOyEtrOth\\\\/XKIOrfpW\\\\/EkbpK5reGci9xSypSc+od\\\\/PAc2etn9Hj+B3aHEIj0+IyQD+HrREDZDeh9cQuzaiLCPB9o3s1eRFHSztBvnZ1HE7QpwesbWaJwx2j+ARqSMe0XYknoPMDSLah75NdM9K9UDjrI9bqFe2G1rZsIN\\\\/JfHSeu7hsZ+t521LcxTp1bVOEdEjzMQQ\\\\/x3tGVmvRnA+Ol4TxS1o0Z\\\\/xgESoBy0n6FeZW\\\\/Oz7BT98zWWw\\\\/OQsPXZchLJfgImDTh2ivb04ajMUFvS5FAP6\\\\/AXjDyVfbIPFWFjXXVsrVEkogiUwUdl9H0JXl\\\\/i3dF68P8ueq8cQWA2VrLpKmPFrc3omq0aSk8HLP4wVy7MjgmgbftGRsZXjcw4OGt7w65pqmUb+nun29tn03sJ67as6KUge8P8jM8COzMS7UgE9zBfRzDWy0hlHmDUDDEKTYW6g9pzBEf\\\\/xXpxjy3UPKdqhUSvBo\\\\/XeEViV+8TJMmW9PUUqbGL4xxiFr3HSHmMxZ2UukLkJ9rDKnsGWGnUWL7j7JW11srK14FHEzQdwGg\\\\/YUU8g\\\\/8\\\\/wZGPWIsNh5P1+BgeR3Dc1Fg7s9U7g8W8mM9TdRbFfF5dyTFvx2fWiI9WsrxRn3Zufj923Nve\\\\/hH5JvLl40L8It6InyAGhyDT1lgOcJzSq+psF498xVpuZt1LOPd\\\\/fASdfCIzAQA=\\\"}],\\\"Product\\\":{\\\"Characteristics\\\":[{\\\"Code\\\":\\\"INITIAL_BOOKING\\\",\\\"Description\\\":\\\"InitialBooking\\\",\\\"Value\\\":\\\"true\\\"}],\\\"Code\\\":\\\"B07\\\",\\\"Description\\\":\\\"Premier Access and Club Trip Pass\\\",\\\"DisplayName\\\":\\\"Premier Access and Club Trip Pass\\\",\\\"Rank\\\":\\\"1\\\",\\\"SubProducts\\\":[{\\\"Association\\\":{\\\"ODMappings\\\":[{\\\"RefID\\\":\\\"OD1\\\",\\\"SegmentRefIDs\\\":[\\\"S1\\\"]}],\\\"SegmentRefIDs\\\":[\\\"S1\\\"],\\\"TravelerRefIDs\\\":[\\\"0\\\"]},\\\"Code\\\":\\\"B07\\\",\\\"Descriptions\\\":[\\\"Premier Access and Club Trip Pass\\\"],\\\"Extension\\\":{\\\"Bundle\\\":{\\\"Products\\\":[{\\\"Code\\\":\\\"PAS\\\",\\\"Rank\\\":\\\"1\\\",\\\"SubProducts\\\":[{\\\"Code\\\":\\\"PAS\\\",\\\"Descriptions\\\":[\\\"Premier Access\\\"],\\\"Features\\\":[{\\\"Type\\\":0,\\\"Value\\\":\\\"PRIORITY_CHECKIN\\\"},{\\\"Type\\\":2,\\\"Value\\\":\\\"PRIORITY_SECURITY\\\"},{\\\"Type\\\":1,\\\"Value\\\":\\\"PRIORITY_BOARDING\\\"}],\\\"GroupCode\\\":\\\"PA\\\",\\\"ID\\\":\\\"1\\\",\\\"IsDefault\\\":\\\"False\\\",\\\"IsEntitled\\\":\\\"False\\\",\\\"Name\\\":\\\"Premier Access\\\",\\\"Prices\\\":[{\\\"Association\\\":{\\\"SegmentRefIDs\\\":[\\\"S1\\\"],\\\"TravelerRefIDs\\\":[\\\"0\\\"]},\\\"ID\\\":\\\"S1_0\\\",\\\"PaymentOptions\\\":[{\\\"EDDCode\\\":\\\"PA1\\\",\\\"PriceComponents\\\":[{\\\"ActualPrice\\\":{\\\"Adjustments\\\":[{\\\"Designator\\\":\\\"TBAPRM\\\",\\\"Input\\\":51,\\\"Result\\\":56,\\\"Type\\\":\\\"Premium\\\",\\\"Value\\\":5},{\\\"Input\\\":56,\\\"Result\\\":56,\\\"Type\\\":\\\"Rounding\\\"}],\\\"BasePrice\\\":[{\\\"Amount\\\":56,\\\"Currency\\\":{\\\"Code\\\":\\\"USD\\\",\\\"DecimalPlace\\\":2},\\\"Type\\\":\\\"Money\\\"}],\\\"FareType\\\":0,\\\"Totals\\\":[{\\\"Amount\\\":56,\\\"Currency\\\":{\\\"Code\\\":\\\"USD\\\",\\\"DecimalPlace\\\":2},\\\"Type\\\":\\\"Money\\\"}]},\\\"Characteristics\\\":[{\\\"Code\\\":\\\"RFIC\\\",\\\"Value\\\":\\\"J\\\"}],\\\"Price\\\":{\\\"Adjustments\\\":[{\\\"AppliedTo\\\":56,\\\"Designator\\\":\\\"BUNDLEDISC\\\",\\\"Input\\\":56,\\\"Percentage\\\":10,\\\"Result\\\":50.4,\\\"Type\\\":\\\"Discount\\\",\\\"Value\\\":-5.6},{\\\"Input\\\":50.4,\\\"Result\\\":51,\\\"Type\\\":\\\"Rounding\\\"}],\\\"BasePrice\\\":[{\\\"Amount\\\":51,\\\"Currency\\\":{\\\"Code\\\":\\\"USD\\\",\\\"DecimalPlace\\\":2},\\\"Type\\\":\\\"Money\\\"}],\\\"FareType\\\":0,\\\"Totals\\\":[{\\\"Amount\\\":51,\\\"Currency\\\":{\\\"Code\\\":\\\"USD\\\",\\\"DecimalPlace\\\":2},\\\"Type\\\":\\\"Money\\\"}]}}],\\\"Type\\\":\\\"Money\\\"}]}],\\\"SubGroupCode\\\":\\\"CSB\\\"}]},{\\\"Code\\\":\\\"CTP\\\",\\\"Rank\\\":\\\"2\\\",\\\"SubProducts\\\":[{\\\"Code\\\":\\\"CTP\\\",\\\"Descriptions\\\":[\\\"Trip Pass\\\"],\\\"Extension\\\":{\\\"AdditionalExtensions\\\":[{\\\"Characteristics\\\":[{\\\"Code\\\":\\\"IAH\\\",\\\"Value\\\":\\\"true\\\"},{\\\"Code\\\":\\\"LAX\\\",\\\"Value\\\":\\\"true\\\"}]}]},\\\"GroupCode\\\":\\\"TP\\\",\\\"ID\\\":\\\"1\\\",\\\"IsDefault\\\":\\\"False\\\",\\\"IsEntitled\\\":\\\"False\\\",\\\"Name\\\":\\\"Trip Pass\\\",\\\"Prices\\\":[{\\\"Association\\\":{\\\"SegmentRefIDs\\\":[\\\"S1\\\"],\\\"TravelerRefIDs\\\":[\\\"0\\\"]},\\\"ID\\\":\\\"S1_0\\\",\\\"PaymentOptions\\\":[{\\\"EDDCode\\\":\\\"CP1\\\",\\\"PriceComponents\\\":[{\\\"ActualPrice\\\":{\\\"Adjustments\\\":[{\\\"AppliedTo\\\":60,\\\"Designator\\\":\\\"NODEPCBPCLUB\\\",\\\"Input\\\":60,\\\"Result\\\":60,\\\"Type\\\":\\\"Discount\\\"},{\\\"Input\\\":60,\\\"Result\\\":60,\\\"Type\\\":\\\"Rounding\\\"}],\\\"BasePrice\\\":[{\\\"Amount\\\":60,\\\"Currency\\\":{\\\"Code\\\":\\\"USD\\\",\\\"DecimalPlace\\\":2},\\\"Type\\\":\\\"Money\\\"}],\\\"FareType\\\":0,\\\"Totals\\\":[{\\\"Amount\\\":60,\\\"Currency\\\":{\\\"Code\\\":\\\"USD\\\",\\\"DecimalPlace\\\":2},\\\"Type\\\":\\\"Money\\\"}]},\\\"Characteristics\\\":[{\\\"Code\\\":\\\"RFIC\\\",\\\"Value\\\":\\\"J\\\"}],\\\"Price\\\":{\\\"Adjustments\\\":[{\\\"AppliedTo\\\":60,\\\"Designator\\\":\\\"BUNDLEDISC\\\",\\\"Input\\\":60,\\\"Percentage\\\":10,\\\"Result\\\":54,\\\"Type\\\":\\\"Discount\\\",\\\"Value\\\":-6}],\\\"BasePrice\\\":[{\\\"Amount\\\":54,\\\"Currency\\\":{\\\"Code\\\":\\\"USD\\\",\\\"DecimalPlace\\\":2},\\\"Type\\\":\\\"Money\\\"}],\\\"FareType\\\":0,\\\"Totals\\\":[{\\\"Amount\\\":54,\\\"Currency\\\":{\\\"Code\\\":\\\"USD\\\",\\\"DecimalPlace\\\":2},\\\"Type\\\":\\\"Money\\\"}]}}],\\\"Type\\\":\\\"Money\\\"}]}],\\\"SubGroupCode\\\":\\\"CTP\\\"}]}]}},\\\"GroupCode\\\":\\\"BE\\\",\\\"ID\\\":\\\"9\\\",\\\"IsDefault\\\":\\\"False\\\",\\\"IsEntitled\\\":\\\"False\\\",\\\"Name\\\":\\\"Premier Access and Club Trip Pass\\\",\\\"Prices\\\":[{\\\"Association\\\":{\\\"ODMappings\\\":[{\\\"RefID\\\":\\\"OD1\\\",\\\"SegmentRefIDs\\\":[\\\"S1\\\"]}],\\\"SegmentRefIDs\\\":[\\\"S1\\\"],\\\"TravelerRefIDs\\\":[\\\"0\\\"]},\\\"ID\\\":\\\"B7-SOL1_OD1_S1_0\\\",\\\"PaymentOptions\\\":[{\\\"EDDCode\\\":\\\"B07\\\",\\\"PriceComponents\\\":[{\\\"Characteristics\\\":[{\\\"Code\\\":\\\"RFIC\\\",\\\"Value\\\":\\\"A\\\"}],\\\"Price\\\":{\\\"BasePrice\\\":[{\\\"Amount\\\":105,\\\"Currency\\\":{\\\"Code\\\":\\\"USD\\\",\\\"DecimalPlace\\\":2},\\\"Type\\\":\\\"Money\\\"}],\\\"FareType\\\":0,\\\"Totals\\\":[{\\\"Amount\\\":105,\\\"Currency\\\":{\\\"Code\\\":\\\"USD\\\",\\\"DecimalPlace\\\":2},\\\"Type\\\":\\\"Money\\\"}]}}],\\\"PriceDesignator\\\":\\\"70\\\",\\\"Type\\\":\\\"Money\\\"}]}],\\\"SubGroupCode\\\":\\\"B7\\\"}]}}]}}],\\\"Requester\\\":{\\\"GUIDs\\\":[{\\\"ID\\\":\\\"7E3294F9-3ABF-4C33-A1C3-8B8082339A4E\\\",\\\"LanguageCode\\\":null,\\\"Name\\\":\\\"AuthorizationToken\\\"},{\\\"ID\\\":\\\"C32D2A66-2BE3-4AB6-B21D-AF28D08FF8CC\\\",\\\"LanguageCode\\\":null,\\\"Name\\\":\\\"CartId\\\"},{\\\"ID\\\":\\\"1636398617480010232233162569831\\\",\\\"LanguageCode\\\":null,\\\"Name\\\":\\\"CORRELATIONID\\\"}],\\\"Requestor\\\":{\\\"ChannelID\\\":\\\"401\\\",\\\"ChannelName\\\":\\\"MBE\\\",\\\"Characteristic\\\":[{\\\"Code\\\":\\\"TransactionLog\\\",\\\"Value\\\":\\\"true\\\"}],\\\"ID\\\":\\\"1636398617480010232233162569831\\\",\\\"LanguageCode\\\":\\\"en-US\\\"}},\\\"Response\\\":{\\\"GUIDs\\\":[{\\\"ID\\\":\\\"1636398617480010232233162569831\\\",\\\"LanguageCode\\\":null,\\\"Name\\\":\\\"CORRELATIONID\\\"},{\\\"ID\\\":\\\"1636398617480010232233162569831\\\",\\\"LanguageCode\\\":null,\\\"Name\\\":\\\"TRANSACTIONID\\\"},{\\\"ID\\\":\\\"-261273146\\\",\\\"LanguageCode\\\":null,\\\"Name\\\":\\\"ADVISORID\\\"}],\\\"ProcessTime\\\":591,\\\"RecordLocator\\\":\\\"\\\",\\\"TimeStamps\\\":[{\\\"Time\\\":\\\"11\\\\/8\\\\/2021 2:10:18 PM\\\",\\\"Type\\\":\\\"Eastern Standard Time\\\"},{\\\"Time\\\":\\\"11\\\\/8\\\\/2021 2:10:17 PM\\\",\\\"Type\\\":\\\"Fms Service Invoke Date Time\\\"},{\\\"Time\\\":\\\"44\\\",\\\"Type\\\":\\\"RTD_GetOptimizedOffers\\\"},{\\\"Time\\\":\\\"1\\\\/1\\\\/0001 00:00:00\\\",\\\"Type\\\":\\\"PnrCreateDate\\\"}],\\\"TransactionID\\\":\\\"1636398617480010232233162569831\\\"},\\\"Solutions\\\":[{\\\"ID\\\":\\\"SOL1\\\",\\\"ODOptions\\\":[{\\\"FlightSegments\\\":[{\\\"ArrivalAirport\\\":{\\\"IATACode\\\":\\\"LAX\\\",\\\"IATACountryCode\\\":{\\\"CountryCode\\\":\\\"US\\\"},\\\"Name\\\":\\\"Los Angeles, CA, US (LAX)\\\"},\\\"ArrivalDateTime\\\":\\\"2021-12-29 14:02\\\",\\\"DepartureAirport\\\":{\\\"IATACode\\\":\\\"IAH\\\",\\\"IATACountryCode\\\":{\\\"CountryCode\\\":\\\"US\\\"},\\\"Name\\\":\\\"Houston, TX, US (IAH)\\\"},\\\"DepartureDateTime\\\":\\\"2021-12-29 12:17\\\",\\\"InstantUpgradable\\\":false,\\\"UpgradeEligibilityStatus\\\":0,\\\"UpgradeVisibilityType\\\":0,\\\"ID\\\":\\\"S1\\\",\\\"RefID\\\":\\\"S1\\\"}],\\\"RefID\\\":\\\"OD1\\\"}]}],\\\"Travelers\\\":[{\\\"CustomerID\\\":\\\"0\\\",\\\"DateOfBirth\\\":\\\"11\\\\/7\\\\/1981 12:00:00 AM\\\",\\\"GivenName\\\":\\\"Sai kiran\\\",\\\"Sex\\\":\\\"M\\\",\\\"Surname\\\":\\\"chanda\\\",\\\"ID\\\":\\\"0\\\",\\\"PassengerTypeCode\\\":\\\"ADT\\\",\\\"TicketNumber\\\":\\\"\\\",\\\"TicketingDate\\\":\\\"11\\\\/8\\\\/2021 1:10:00 PM\\\",\\\"TravelerNameIndex\\\":\\\"1.1\\\"}]}\"]}],\"ID\":{\"ID\":\"C32D2A66-2BE3-4AB6-B21D-AF28D08FF8CC\",\"LanguageCode\":\"en-US\"},\"PointOfSale\":{\"Country\":{\"CountryCode\":\"US\"},\"CurrencyCode\":\"USD\"},\"GrandTotal\":243.4,\"WorkFlowType\":1,\"DeviceID\":\"DFFBE04D-2AAF-40E9-B00D-2E22F47A28D6\",\"ShoppingFlowState\":[{\"Channel\":\"MOBILE\",\"CreateDateTime\":\"2021-11-08 07:07 PM\",\"RegisterOperationType\":1},{\"Channel\":\"MOBILE\",\"CreateDateTime\":\"2021-11-08 07:09 PM\",\"RegisterOperationType\":1},{\"Channel\":\"MOBILE\",\"CreateDateTime\":\"2021-11-08 07:10 PM\",\"RegisterOperationType\":2},{\"Channel\":\"MOBILE\",\"CreateDateTime\":\"2021-11-08 07:10 PM\",\"RegisterOperationType\":4}]},\"Warnings\":[]}";
            //return United.Json.Serialization.JsonSerializer.NewtonSoftDeserialize<FlightReservationResponse>(testData);
            #endregion
            // var persistData = United.Persist.FilePersist.Load<List<CCEFlightReservationResponseByCartId>>
            //   (sessionId, new CCEFlightReservationResponseByCartId().ObjectName);
            // List<CCEFlightReservationResponseByCartId> persistCCEFlightReservation = new List<CCEFlightReservationResponseByCartId>();
            var persistData = await _sessionHelperService.GetSession<List<CCEFlightReservationResponseByCartId>>
                (sessionId, new CCEFlightReservationResponseByCartId().ObjectName,
                    new List<string> { sessionId, new CCEFlightReservationResponseByCartId().ObjectName }).ConfigureAwait(false);
            var flightReservationResponse = persistData?.FirstOrDefault(p => string.Equals(p.CartId, cartId, StringComparison.OrdinalIgnoreCase));

            var response = default(FlightReservationResponse);
            if (flightReservationResponse != null)
            {
                response = flightReservationResponse.CslFlightReservationResponse;
            }

            return response;
        }

        private async Task<(MOBShoppingCart ShoppingCart, FlightReservationResponse flightReservationResponse)> RegisterSeats(MOBRegisterSeatsRequest request, string token, SeatChangeState state, Reservation persistedReservation, FlightReservationResponse flightReservationResponse, Session session = null)
        {
            MOBShoppingCart persistShoppingCart = new MOBShoppingCart();
            persistShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, persistShoppingCart.ObjectName, new List<string> { request.SessionId, persistShoppingCart.ObjectName }).ConfigureAwait(false);
            try
            {
                flightReservationResponse = request.IsOmniCartSavedTripFlow ? await GetFlightReservationResponseByCartId(request.SessionId, request.CartId) : await RegisterSeats_CFOP(request, token, state, persistedReservation);
                if (EnableAdvanceSearchCouponBooking(request.Application.Id, request.Application.Version.Major) && IsAFSSeatCouponApplied(persistShoppingCart) && request.Flow == FlowType.BOOKING.ToString())
                {
                    if (flightReservationResponse != null)
                    {
                        var seatproduct = flightReservationResponse.ShoppingCart.Items.SelectMany(d => d.Product).Where(d => d?.Code == "SEATASSIGNMENTS")?.ToList();
                        if (seatproduct.Any(x => x == null || x.CouponDetails == null || x.CouponDetails.Any(a => a != null && !a.IsCouponEligible.ToUpper().Equals("TRUE"))))
                        {
                            if (persistedReservation.ShopReservationInfo2.SeatRemoveCouponPopupCount >= 1)
                            {
                                if (!await AddOrRemovePromo(request, session, true, request.Flow))
                                {
                                    throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                                }
                                persistShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, persistShoppingCart.ObjectName, new List<string> { request.SessionId, persistShoppingCart.ObjectName }).ConfigureAwait(false);
                                return (persistShoppingCart, flightReservationResponse);
                            }
                            else if (!await _featureSettings.GetFeatureSettingValue("EnableEplusWithCouponSCRemovalFix").ConfigureAwait(false))
                            {
                                return (persistShoppingCart, flightReservationResponse);
                            }
                        }
                    }
                }
                if (flightReservationResponse != null && flightReservationResponse.Errors.Count() == 0)
                {
                    if (persistShoppingCart == null)
                        persistShoppingCart = new MOBShoppingCart();
                    var persistedSCProducts = persistShoppingCart.Products;
                    persistShoppingCart.Products = await _shoppingCartUtility.ConfirmationPageProductInfo(flightReservationResponse, false, false, request.Flow.ToString(), request.Application, state, false, false, null, sessionId: request?.SessionId);
                    persistShoppingCart.CartId = request.CartId;
                    double price = GetGrandTotalPriceForShoppingCart(false, flightReservationResponse, false, request.Flow);
                    persistShoppingCart.TotalPrice = String.Format("{0:0.00}", price);
                    //persistShoppingCart.DisplayTotalPrice = string.Format("{0:c}", Decimal.Parse(price.ToString())); //Decimal.Parse(price.ToString()).ToString("c");
                    persistShoppingCart.DisplayTotalPrice = Decimal.Parse(price.ToString()).ToString("c", new CultureInfo("en-us"));
                    persistShoppingCart.TermsAndConditions = await GetProductBasedTermAndConditions(null, flightReservationResponse, false);
                    persistShoppingCart.PaymentTarget = (request.Flow == FlowType.BOOKING.ToString()) ? GetBookingPaymentTargetForRegisterFop(flightReservationResponse) : GetPaymentTargetForRegisterFop(flightReservationResponse.DisplayCart.TravelOptions);
                    persistShoppingCart.TravelPolicyWarningAlert = await _shoppingCartUtility.BuildCorporateTravelPolicyWarningAlert(request, session, flightReservationResponse, persistShoppingCart.IsCorporateBusinessNamePersonalized);
                    await _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, request.SessionId, new List<string> { request.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName).ConfigureAwait(false);

                }
            }
            catch (MOBUnitedException uaex)
            {
                throw uaex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return (persistShoppingCart, flightReservationResponse);
        }

        private async Task<Services.FlightShopping.Common.FlightReservation.RegisterOfferRequest> BuildRegisterPcuSeatOffer(SeatChangeState seatState, string cartId)
        {
            if (seatState == null || seatState.BookingTravelerInfo == null)
                return null;

            var seats = seatState.BookingTravelerInfo.SelectMany(x => x.Seats);
            if (seats == null || !seats.Any())
                return null;

            var optionIds = seats.Where(s => s != null && !string.IsNullOrEmpty(s.PcuOfferOptionId)).Select(s => s.PcuOfferOptionId);

            if (optionIds == null || !optionIds.Any())
                return null;

            var pcuState = new PcuState();
            pcuState = await _sessionHelperService.GetSession<PcuState>(seatState.SessionId, pcuState.ObjectName, new List<string> { seatState.SessionId, pcuState.ObjectName }).ConfigureAwait(false);
            var productIds = pcuState.EligibleSegments.SelectMany(e => e.UpgradeOptions.Where(s => optionIds.Contains(s.OptionId))).SelectMany(u => u.ProductIds).ToList();

            var productOffer = new GetOffers();
            productOffer = await _sessionHelperService.GetSession<GetOffers>(seatState.SessionId, productOffer.ObjectName, new List<string> { seatState.SessionId, productOffer.ObjectName }).ConfigureAwait(false);
            productOffer.Offers[0].ProductInformation.ProductDetails.RemoveWhere(p => p.Product.Code != "PCU");

            return GetRegisterOffersRequest(cartId, null, null, null, "PCU", null, productIds, null, false, productOffer, null, new Collection<Characteristic>() { new Characteristic() { Code = "ManageRes", Value = "true" } });
        }

        private United.Services.FlightShopping.Common.FlightReservation.RegisterOfferRequest GetRegisterOffersRequest
            (string cartId, string cartKey, string languageCode, string pointOfSale, string productCode,
            string productId, List<string> productIds, string subProductCode, bool delete,
            United.Service.Presentation.ProductResponseModel.ProductOffer Offer,
            United.Service.Presentation.ReservationResponseModel.ReservationDetail reservation,
            System.Collections.ObjectModel.Collection<Characteristic> characteristics = null)
        {
            United.Services.FlightShopping.Common.FlightReservation.RegisterOfferRequest registerOfferRequest = new United.Services.FlightShopping.Common.FlightReservation.RegisterOfferRequest();


            registerOfferRequest.AutoTicket = false;
            registerOfferRequest.CartId = cartId;
            registerOfferRequest.CartKey = cartKey;
            registerOfferRequest.CountryCode = (pointOfSale != null) ? pointOfSale : "US";
            registerOfferRequest.Delete = delete;
            registerOfferRequest.LangCode = (languageCode != null) ? languageCode : "en-US";
            registerOfferRequest.Offer = (Offer == null ? null : Offer);

            registerOfferRequest.Characteristics = characteristics;

            registerOfferRequest.ProductCode = productCode;
            registerOfferRequest.ProductId = productId;
            registerOfferRequest.ProductIds = productIds;
            registerOfferRequest.SubProductCode = subProductCode;
            registerOfferRequest.Reservation = (reservation == null ? null : reservation.Detail);
            registerOfferRequest.Products = new List<ProductRequest>()
            {
                new ProductRequest () { Code = productCode, Ids = productIds }
            };

            return registerOfferRequest;
        }

        private async Task<RegisterSeatsRequest> BuildRegisterSeatRequest(MOBRegisterSeatsRequest request, SeatChangeState state)
        {
            RegisterSeatsRequest registerSeatsRequest = new RegisterSeatsRequest();

            var reservationDetail = new United.Service.Presentation.ReservationResponseModel.ReservationDetail();
            reservationDetail = await _sessionHelperService.GetSession<United.Service.Presentation.ReservationResponseModel.ReservationDetail>(request.SessionId, reservationDetail.GetType().FullName, new List<string> { request.SessionId, reservationDetail.GetType().FullName }).ConfigureAwait(false);

            registerSeatsRequest.CartId = request.CartId;
            registerSeatsRequest.Characteristics = new System.Collections.ObjectModel.Collection<Characteristic>() {
                            new Characteristic() { Code = "ManageRes", Value = "true" }};
            registerSeatsRequest.Reservation = reservationDetail.Detail;
            registerSeatsRequest.IsPNRFirst = false;
            registerSeatsRequest.RecordLocator = reservationDetail.Detail.ConfirmationID;
            registerSeatsRequest.OfferRequest = await BuildRegisterPcuSeatOffer(state, request.CartId);
            List<SeatAssignment> seatAssignments = new List<SeatAssignment>();
            registerSeatsRequest.SeatAssignments = state.BookingTravelerInfo.SelectMany(x => x.Seats).Where(x => x.SeatAssignment != null && x.SeatAssignment != x.OldSeatAssignment).Select(x => new SeatAssignment
            {
                Currency = x.Currency,
                FlattenedSeatIndex = Convert.ToInt32(state.Segments.Where(y => y.Arrival.Code == x.Destination && y.Departure.Code == x.Origin && y.FlightNumber == x.FlightNumber).Select(y => y.SegmentIndex).FirstOrDefault()) - 1, // C
                OptOut = false, // C
                OriginalPrice = x.OldSeatPrice,
                ProductCode = x.OldSeatProgramCode,
                PCUSeat = !string.IsNullOrEmpty(x.PcuOfferOptionId),
                Seat = x.SeatAssignment,
                SeatPrice = x.PriceAfterTravelerCompanionRules,
                SeatPromotionCode = x.ProgramCode,
                SeatType = x.SeatType,
                TravelerIndex = Convert.ToInt32(x.TravelerSharesIndex.Split('.')[0]) - 1,
                OriginalSegmentIndex = Convert.ToInt32(state.Segments.Where(y => y.Arrival.Code == x.Destination && y.Departure.Code == x.Origin && y.FlightNumber == x.FlightNumber).Select(y => y.OriginalSegmentNumber).FirstOrDefault()),
                LegIndex = Convert.ToInt32(state.Segments.Where(y => y.Arrival.Code == x.Destination && y.Departure.Code == x.Origin && y.FlightNumber == x.FlightNumber).Select(y => y.LegIndex).FirstOrDefault()),
                PersonIndex = x.TravelerSharesIndex.ToString(),
                ArrivalAirportCode = x.Destination.ToString(),
                DepartureAirportCode = x.Origin.ToString(),
                FlightNumber = x.FlightNumber.ToString()
            }).ToList();
            registerSeatsRequest.WorkFlowType = GetWorkFlowType(request.Flow);
            return registerSeatsRequest;
        }

        private async Task<FlightReservationResponse> RegisterSeats_CFOP(MOBRegisterSeatsRequest request, string token, SeatChangeState state, Reservation persistedReservation)
        {
            FlightReservationResponse flightReservationResponse = null;
            RegisterSeatsRequest registerSeatsRequest = state != null ? await BuildRegisterSeatRequest(request, state) : (persistedReservation != null ? await GetRegisterSeatsRequest(request, persistedReservation) : null);

            if (registerSeatsRequest.SeatAssignments == null || registerSeatsRequest.SeatAssignments.Count == 0)
                return null;

            string jsonRequest = JsonConvert.SerializeObject(registerSeatsRequest);

            string url = "registerseats";

            string jsonResponse = await _shoppingCartService.RegisterSeats_CFOP(token, url, jsonRequest, request.SessionId);

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                flightReservationResponse = JsonConvert.DeserializeObject<FlightReservationResponse>(jsonResponse);
            }
            return flightReservationResponse;
        }

        private async Task<(bool, MOBBookingRegisterSeatsResponse registerSeatsResponse)> RegisterSeatsHandleElf(MOBRegisterSeatsRequest request, Session session, Reservation persistedReservation, MOBBookingRegisterSeatsResponse registerSeatsResponse, bool isClearSeats)
        {
            FlightReservationResponse response = null;
            var ignoreRegisterSeatsCSL = false;
            bool inhibitBooking = false;
            try
            {
                if (!_configuration.GetValue<bool>("DisableBESeatsBundlesChangeFlowFix") && string.IsNullOrEmpty(request?.CartId))
                {
                    request.CartId = session?.CartId;
                }

                RegisterSeatsRequest registerSeatsRequest = (persistedReservation != null ? await GetRegisterSeatsRequest(request, persistedReservation) : null);

                if (registerSeatsRequest.SeatAssignments == null || registerSeatsRequest.SeatAssignments.Count == 0)
                    ignoreRegisterSeatsCSL = true;

                if (_shoppingCartUtility.IsEnableOmniCartMVP2Changes(request.Application.Id, request.Application.Version.Major, persistedReservation?.ShopReservationInfo2?.IsDisplayCart == true) && isClearSeats)
                {

                    try
                    {
                        //var mobRegisterOfferRequest = new MOBRegisterOfferRequest();
                        //mobRegisterOfferRequest.CartId = request.CartId;
                        //mobRegisterOfferRequest.Application = request.Application;
                        //mobRegisterOfferRequest.SessionId = request.SessionId;
                        //mobRegisterOfferRequest.Flow = request.Flow;

                        var tupleResponse = await ClearSeats(request, session, response);
                        registerSeatsResponse.ShoppingCart = tupleResponse.response;
                        response = tupleResponse.flightReservationResponse;

                        await ClearTravelerSeats(session.SessionId);
                    }
                    catch (System.Net.WebException wex)
                    {
                        throw wex;
                    }
                    catch (MOBUnitedException uaex)
                    {
                        throw uaex;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }
                else
                {
                    var tupleResponse =  await RegisterSeats(request, session.Token, null, persistedReservation, response, session);
                    registerSeatsResponse.ShoppingCart = tupleResponse.ShoppingCart;
                    response = tupleResponse.flightReservationResponse;
                }


                if (_configuration.GetValue<bool>("EnableInhibitBooking"))
                {
                    inhibitBooking = IdentifyInhibitWarning(response);
                }
                if (ignoreRegisterSeatsCSL || (response != null && response.Status.Equals(United.Services.FlightShopping.Common.StatusType.Success) && (response.Errors == null || (response.Errors != null && (response.Errors.Capacity == 0 || inhibitBooking)))))
                {
                    persistedReservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, new Reservation().ObjectName, new List<string> { request.SessionId, new Reservation().ObjectName }).ConfigureAwait(false);

                    registerSeatsResponse.Reservation.Prices = persistedReservation.Prices;
                    if (_shoppingCartUtility.IsEnableOmniCartMVP2Changes(request.Application.Id, request.Application.Version.Major, persistedReservation?.ShopReservationInfo2.IsDisplayCart == true)
                        ? request.IsDone : true)
                    {
                        if (EnableAdvanceSearchCouponBooking(request.Application.Id, request.Application.Version.Major)
                     && IsAFSSeatCouponApplied(registerSeatsResponse.ShoppingCart)
                     && persistedReservation.ShopReservationInfo2.SeatRemoveCouponPopupCount == 0 && response != null)
                        {
                            var seatproduct = response.ShoppingCart.Items.SelectMany(d => d.Product).Where(d => d?.Code == "SEATASSIGNMENTS")?.ToList();
                            if (seatproduct.Any(x => x == null || x.CouponDetails == null || x.CouponDetails.Any(a => a != null && !a.IsCouponEligible.ToUpper().Equals("TRUE"))))
                            {
                                registerSeatsResponse.PromoCodeRemoveAlertForProducts = new MOBSection
                                {
                                    Text1 = _configuration.GetValue<string>("AdvanceSearchCouponWithRegisterSeatsErrorMessage"),
                                    Text2 = "Select a seat",
                                    Text3 = "Continue"
                                };

                                return (false, registerSeatsResponse) ;
                            }
                        }
                    }
                    decimal totalEplusSeatPrice = 0;
                    decimal totalOriginalEplusSeatPrice = 0;
                    double totalCurrentEplusSeatPrice = 0;
                    bool showLimitedRecline = false;
                    var countNoOfEPlusSeatPriceRows = 0;
                    var countNoOfEPlusPurchased = 0;

                    decimal totalEminusSeatPrice = 0;
                    decimal totalOriginalEminusSeatPrice = 0;
                    double totalCurrentEminusSeatPrice = 0;
                    var countNoOfEminusPurchased = 0;

                    decimal totalPreferedSeatPrice = 0;
                    decimal totalOriginalPreferedSeatPrice = 0;
                    double totalCurrentPreferedSeatPrice = 0;
                    var countNoOfPreferedPurchased = 0;
                    var preferedSeatText = _configuration.GetValue<string>("PreferedSeat_PriceBreakdownTitle") ?? string.Empty;

                    if (registerSeatsResponse.Reservation.Prices.Find(x => x.DisplayType.ToUpper() == "ECONOMYPLUS SEATS") != null)
                    {
                        totalCurrentEplusSeatPrice = registerSeatsResponse.Reservation.Prices.First(x => x.DisplayType.ToUpper() == "ECONOMYPLUS SEATS").Value;
                    }

                    if (registerSeatsResponse.Reservation.Prices.Find(x => x.DisplayType.ToUpper() == preferedSeatText.ToUpper()) != null)
                    {
                        totalCurrentPreferedSeatPrice = registerSeatsResponse.Reservation.Prices.First(x => x.DisplayType.ToUpper() == preferedSeatText.ToUpper()).Value;
                    }

                    if (registerSeatsResponse.Reservation.Prices.Find(x => x.DisplayType.ToUpper() == "ADVANCE SEAT ASSIGNMENT") != null)
                    {
                        totalCurrentEminusSeatPrice = registerSeatsResponse.Reservation.Prices.First(x => x.DisplayType.ToUpper() == "ADVANCE SEAT ASSIGNMENT").Value;
                    }
                    if (persistedReservation != null && persistedReservation.SeatPrices != null)
                    {
                        foreach (var seatPrice in persistedReservation.SeatPrices)
                        {
                            if (seatPrice.SeatMessage.ToUpper().Contains("ECONOMY PLUS"))
                            {
                                countNoOfEPlusSeatPriceRows = countNoOfEPlusSeatPriceRows + 1;
                                if (countNoOfEPlusSeatPriceRows == 1)
                                    showLimitedRecline = seatPrice.SeatMessage.ToUpper().Contains("LIMITED RECLINE");
                                totalEplusSeatPrice += seatPrice.DiscountedTotalPrice;
                                totalOriginalEplusSeatPrice += seatPrice.TotalPrice;
                            }

                            if (seatPrice.SeatMessage.ToUpper().Contains(preferedSeatText.ToUpper()))
                            {
                                if (!seatPrice.SeatNumbers.IsNullOrEmpty())
                                    countNoOfPreferedPurchased = countNoOfPreferedPurchased + seatPrice.SeatNumbers.Count;
                                totalPreferedSeatPrice += seatPrice.DiscountedTotalPrice;
                                totalOriginalPreferedSeatPrice += seatPrice.TotalPrice;
                            }

                            if (seatPrice.SeatMessage.ToUpper().Contains("ADVANCE SEAT ASSIGNMENT"))
                            {
                                if (!seatPrice.SeatNumbers.IsNullOrEmpty())
                                    countNoOfEminusPurchased = countNoOfEminusPurchased + seatPrice.SeatNumbers.Count;
                                totalEminusSeatPrice += seatPrice.DiscountedTotalPrice;
                                totalOriginalEminusSeatPrice += seatPrice.TotalPrice;
                            }
                        }
                    }

                    bool totalExist = false;
                    bool eplusSeatPriceExist = false;
                    bool eminusSeatPriceExist = false;
                    bool preferedSeatPriceExist = false;
                    decimal flightTotal = 0;

                    for (int i = 0; i < registerSeatsResponse.Reservation.Prices?.Count; i++)
                    {
                        if (registerSeatsResponse.Reservation.Prices[i].DisplayType.ToUpper() == "GRAND TOTAL")
                        {
                            totalExist = true;
                            var discountAmount = (totalCurrentEplusSeatPrice + totalCurrentPreferedSeatPrice + totalCurrentEminusSeatPrice) - Convert.ToDouble(totalEplusSeatPrice + totalPreferedSeatPrice + totalEminusSeatPrice);

                            registerSeatsResponse.Reservation.Prices[i].DisplayValue = string.Format("{0:#,0.00}", registerSeatsResponse.Reservation.Prices[i].Value - discountAmount);
                            registerSeatsResponse.Reservation.Prices[i].FormattedDisplayValue = TopHelper.FormatAmountForDisplay(registerSeatsResponse.Reservation.Prices[i].DisplayValue, new CultureInfo("en-US"), false); //(Convert.ToDecimal(registerSeatsResponse.Reservation.Prices[i].DisplayValue)).ToString("C2", CultureInfo.CurrentCulture); //string.Format("{0:c2}", registerSeatsResponse.Reservation.Prices[i].DisplayValue);

                            registerSeatsResponse.Reservation.Prices[i].Value = ParseToDoubleAndRound(registerSeatsResponse.Reservation.Prices[i].DisplayValue);
                        }
                        if (registerSeatsResponse.Reservation.Prices[i].DisplayType.ToUpper() == "ECONOMYPLUS SEATS")
                        {
                            eplusSeatPriceExist = true;
                            registerSeatsResponse.Reservation.Prices[i].DisplayValue = string.Format("{0:#,0.00}", totalEplusSeatPrice);
                            registerSeatsResponse.Reservation.Prices[i].FormattedDisplayValue = TopHelper.FormatAmountForDisplay(totalEplusSeatPrice, new CultureInfo("en-US"), false); //totalEplusSeatPrice.ToString("C2", CultureInfo.CurrentCulture); //string.Format("{0:c2}", registerSeatsResponse.Reservation.Prices[i].DisplayValue);
                            registerSeatsResponse.Reservation.Prices[i].PriceTypeDescription = showLimitedRecline ? "Economy Plus Seat (limited recline)"
                                                                                                                    : "Economy Plus Seat";

                            registerSeatsResponse.Reservation.Prices[i].Value = ParseToDoubleAndRound(registerSeatsResponse.Reservation.Prices[i].DisplayValue);
                        }

                        if (registerSeatsResponse.Reservation.Prices[i].DisplayType.ToUpper() == preferedSeatText.ToUpper())
                        {
                            preferedSeatPriceExist = true;
                            registerSeatsResponse.Reservation.Prices[i].DisplayValue = string.Format("{0:#,0.00}", totalPreferedSeatPrice);
                            registerSeatsResponse.Reservation.Prices[i].FormattedDisplayValue = TopHelper.FormatAmountForDisplay(totalPreferedSeatPrice, new CultureInfo("en-US"), false); //totalPreferedSeatPrice.ToString("C2", CultureInfo.CurrentCulture); //string.Format("{0:c2}", registerSeatsResponse.Reservation.Prices[i].DisplayValue);
                            registerSeatsResponse.Reservation.Prices[i].PriceTypeDescription = countNoOfPreferedPurchased > 1 ? "Preferred seats" : "Preferred seat";
                            registerSeatsResponse.Reservation.Prices[i].Value = ParseToDoubleAndRound(registerSeatsResponse.Reservation.Prices[i].DisplayValue);
                        }

                        if (registerSeatsResponse.Reservation.Prices[i].DisplayType.ToUpper() == "ADVANCE SEAT ASSIGNMENT")
                        {
                            eminusSeatPriceExist = true;
                            registerSeatsResponse.Reservation.Prices[i].DisplayValue = string.Format("{0:#,0.00}", totalEminusSeatPrice);
                            registerSeatsResponse.Reservation.Prices[i].FormattedDisplayValue = TopHelper.FormatAmountForDisplay(totalEminusSeatPrice, new CultureInfo("en-US"), false);// totalEminusSeatPrice.ToString("C2", CultureInfo.CurrentCulture); //string.Format("{0:c2}", registerSeatsResponse.Reservation.Prices[i].DisplayValue);
                            registerSeatsResponse.Reservation.Prices[i].PriceTypeDescription = countNoOfEminusPurchased > 1 ? "Advance Seat Assignments"
                                                                                                                            : "Advance Seat Assignment";

                            registerSeatsResponse.Reservation.Prices[i].Value = ParseToDoubleAndRound(registerSeatsResponse.Reservation.Prices[i].DisplayValue);
                        }
                        if (registerSeatsResponse.Reservation.Prices[i].DisplayType.ToUpper() == "TOTAL")
                        {
                            flightTotal = Convert.ToDecimal(registerSeatsResponse.Reservation.Prices[i].DisplayValue);
                        }
                    }
                    if (!eplusSeatPriceExist && totalEplusSeatPrice > 0)
                    {

                        MOBSHOPPrice seatTotal = new MOBSHOPPrice
                        {
                            CurrencyCode = persistedReservation.SeatPrices[0].CurrencyCode,
                            DisplayType = "EconomyPlus Seats",
                            DisplayValue = string.Format("{0:#,0.00}", totalEplusSeatPrice),
                            FormattedDisplayValue = TopHelper.FormatAmountForDisplay(totalEplusSeatPrice, new CultureInfo("en-US"), false),//totalEplusSeatPrice.ToString("C2", CultureInfo.CurrentCulture),
                            PriceType = "EconomyPlus Seats",
                            PriceTypeDescription = showLimitedRecline ? (countNoOfEPlusPurchased > 1 ? "Economy Plus Seats (limited recline)" : "Economy Plus Seat (limited recline)")
                                                                          : (countNoOfEPlusPurchased > 1 ? "Economy Plus Seats" : "Economy Plus Seat")
                        };
                        seatTotal.Value = ParseToDoubleAndRound(seatTotal.DisplayValue);
                        registerSeatsResponse.Reservation.Prices.Add(seatTotal);

                        var preferedSeatPrice = registerSeatsResponse.Reservation.Prices.Find(s => s.DisplayType.ToUpper() == preferedSeatText.ToUpper());
                        if (!preferedSeatPrice.IsNullOrEmpty())
                        {
                            registerSeatsResponse.Reservation.Prices.Remove(preferedSeatPrice);
                            registerSeatsResponse.Reservation.Prices.Add(preferedSeatPrice);
                        }

                        var economyMinusPrice = registerSeatsResponse.Reservation.Prices.Find(s => s.DisplayType.ToUpper() == "ADVANCE SEAT ASSIGNMENT");

                        if (!economyMinusPrice.IsNullOrEmpty())
                        {
                            registerSeatsResponse.Reservation.Prices.Remove(economyMinusPrice);
                            registerSeatsResponse.Reservation.Prices.Add(economyMinusPrice);
                        }
                    }
                    if (!preferedSeatPriceExist && totalPreferedSeatPrice > 0)
                    {
                        MOBSHOPPrice seatTotal = new MOBSHOPPrice
                        {
                            CurrencyCode = persistedReservation.SeatPrices[0].CurrencyCode,
                            DisplayType = preferedSeatText,
                            DisplayValue = string.Format("{0:#,0.00}", totalPreferedSeatPrice),
                            FormattedDisplayValue = TopHelper.FormatAmountForDisplay(totalPreferedSeatPrice, new CultureInfo("en-US"), false),//totalPreferedSeatPrice.ToString("C2", CultureInfo.CurrentCulture),
                            PriceType = preferedSeatText,
                            PriceTypeDescription = countNoOfPreferedPurchased > 1 ? "Preferred seats" : "Preferred seat"
                        };

                        seatTotal.Value = ParseToDoubleAndRound(seatTotal.DisplayValue);
                        registerSeatsResponse.Reservation.Prices.Add(seatTotal);

                        var economyMinusPrice = registerSeatsResponse.Reservation.Prices.Find(s => s.DisplayType.ToUpper() == "ADVANCE SEAT ASSIGNMENT");
                        if (!economyMinusPrice.IsNullOrEmpty())
                        {
                            registerSeatsResponse.Reservation.Prices.Remove(economyMinusPrice);
                            registerSeatsResponse.Reservation.Prices.Add(economyMinusPrice);
                        }
                    }
                    if (!eminusSeatPriceExist && totalEminusSeatPrice > 0)
                    {
                        MOBSHOPPrice seatTotal = new MOBSHOPPrice
                        {
                            CurrencyCode = persistedReservation.SeatPrices[0].CurrencyCode,
                            DisplayType = "Advance Seat Assignment",
                            DisplayValue = string.Format("{0:#,0.00}", totalEminusSeatPrice),
                            FormattedDisplayValue = TopHelper.FormatAmountForDisplay(totalEminusSeatPrice, new CultureInfo("en-US")),//totalEminusSeatPrice.ToString("C2", CultureInfo.CurrentCulture),
                            PriceType = "Advance Seat Assignment",
                            PriceTypeDescription = countNoOfEminusPurchased > 1 ? "Advance Seat Assignments"
                                                                                : "Advance Seat Assignment"
                        };

                        seatTotal.Value = ParseToDoubleAndRound(seatTotal.DisplayValue);
                        registerSeatsResponse.Reservation.Prices.Add(seatTotal);
                    }
                    if (!totalExist)
                    {
                        MOBSHOPPrice totalPrice = new MOBSHOPPrice();
                        totalPrice.CurrencyCode = registerSeatsResponse.Reservation.Prices[0].CurrencyCode;
                        totalPrice.DisplayType = "Grand Total";
                        totalPrice.DisplayValue = string.Format("{0:#,0.00}", flightTotal + totalEplusSeatPrice + totalPreferedSeatPrice + totalEminusSeatPrice);//(flightTotal + item.Item.Payment[0].CreditCard.Amount).ToString("N", CultureInfo.InvariantCulture);
                        totalPrice.FormattedDisplayValue = (flightTotal + totalEplusSeatPrice + totalPreferedSeatPrice + totalEminusSeatPrice).ToString("C2", CultureInfo.CurrentCulture); //value.ToString("C3", CultureInfo.CreateSpecificCulture("da-DK")); 
                        totalPrice.PriceType = "Grand Total";

                        totalPrice.Value = ParseToDoubleAndRound(totalPrice.DisplayValue);
                        registerSeatsResponse.Reservation.Prices.Add(totalPrice);
                    }
                    if (eplusSeatPriceExist && totalOriginalEplusSeatPrice == 0)
                    {
                        registerSeatsResponse.Reservation.Prices.Remove(registerSeatsResponse.Reservation.Prices.Find(item => item.DisplayType.ToUpper() == "ECONOMYPLUS SEATS"));
                    }
                    if (preferedSeatPriceExist && totalOriginalPreferedSeatPrice == 0)
                    {
                        registerSeatsResponse.Reservation.Prices.Remove(registerSeatsResponse.Reservation.Prices.Find(item => item.DisplayType.ToUpper() == preferedSeatText.ToUpper()));
                    }
                    if (eminusSeatPriceExist && totalOriginalEminusSeatPrice == 0)
                    {
                        registerSeatsResponse.Reservation.Prices.Remove(registerSeatsResponse.Reservation.Prices.Find(item => item.DisplayType.ToUpper() == "ADVANCE SEAT ASSIGNMENT"));
                    }
                    if (EnableAdvanceSearchCouponBooking(request.Application.Id, request.Application.Version.Major) && request.Flow == FlowType.BOOKING.ToString() && response != null)
                    {
                        List<MOBSHOPPrice> priceClone = registerSeatsResponse.Reservation.Prices.Clone();
                        UpdateSeatsPriceWithPromo(response, registerSeatsResponse.Reservation.Prices, persistedReservation.SeatPrices, registerSeatsResponse.ShoppingCart.Products, registerSeatsResponse.ShoppingCart.Products);
                        UpdateGrandTotal(registerSeatsResponse.Reservation.Prices, priceClone);
                        registerSeatsResponse.SeatPrices = persistedReservation.SeatPrices;
                        registerSeatsResponse.Reservation.SeatPrices = persistedReservation.SeatPrices;
                    }
                    #region 159514 - Inhibit booking 
                    if (inhibitBooking)
                    {
                        // logEntries.Add(LogEntry.GetLogEntry<FlightReservationResponse>(request.SessionId, "RegisterSeats - Response for RegisterSeats with Inhibit warning", "Trace", request.Application.Id, request.Application.Version.Major, request.DeviceId, response, true, false));
                        UpdateInhibitMessage(ref persistedReservation);
                       await _sessionHelperService.SaveSession<Reservation>(persistedReservation, persistedReservation.SessionId, new List<string> { persistedReservation.SessionId, persistedReservation.ObjectName }, persistedReservation.ObjectName).ConfigureAwait(false);
                    }
                    #endregion
                }
                else
                {
                    //159514 - Added for inhibit booking error message
                    if (_configuration.GetValue<bool>("EnableInhibitBooking"))
                    {
                        if (response.Errors.Exists(error => error != null && !string.IsNullOrEmpty(error.MinorCode) && error.MinorCode.Trim().Equals("10050")))
                        {
                            var inhibitErrorCsl = response.Errors.FirstOrDefault(error => error != null && !string.IsNullOrEmpty(error.MinorCode) && error.MinorCode.Trim().Equals("10050"));
                            //throw new MOBUnitedException(inhibitErrorCsl.Message);
                            throw new MOBUnitedException(inhibitErrorCsl.Message, new Exception(inhibitErrorCsl.MinorCode));
                        }
                    }
                    return (false, registerSeatsResponse);
                }

            }
            catch (MOBUnitedException uaex)
            {
                throw uaex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return (true, registerSeatsResponse);
        }

        private async Task ClearTravelerSeats(string sessionId)
        {
            var persistedReservation = new Reservation();
            persistedReservation = await _sessionHelperService.GetSession<Reservation>(sessionId, persistedReservation.ObjectName, new List<string> { sessionId, persistedReservation.ObjectName }).ConfigureAwait(false);
            var travelerSeats = persistedReservation?.TravelersCSL?.SelectMany(traveler => traveler.Value?.Seats);
            if (travelerSeats != null)
            {
                travelerSeats.ForEach(travelerSeat =>
                {

                    if (travelerSeat != null)
                    {
                        travelerSeat.SeatAssignment = String.Empty;
                        travelerSeat.Price = 0;
                        travelerSeat.PriceAfterTravelerCompanionRules = 0;
                        travelerSeat.Currency = "USD";
                        travelerSeat.ProgramCode = String.Empty;
                        travelerSeat.SeatType = String.Empty;
                        travelerSeat.LimitedRecline = false;
                    }

                });
            }

            List<MOBSeatPrice> seatPrices = BuildSeatPricesForPriceBreakDown(persistedReservation.TravelersCSL.Values.SelectMany(traveler => traveler.Seats).ToList());
            persistedReservation.SeatPrices = new List<MOBSeatPrice>();
            persistedReservation.SeatPrices = SortAndOrderSeatPrices(seatPrices);

           await _sessionHelperService.SaveSession<Reservation>(persistedReservation, sessionId, new List<string> { sessionId, persistedReservation.ObjectName }, persistedReservation.ObjectName).ConfigureAwait(false);
        }

        private bool IdentifyInhibitWarning(FlightReservationResponse response)
        {
            if (response != null && response.Errors != null && response.Errors.Count > 0)
            {
                foreach (var err in response.Errors)
                {
                    if (err != null && !string.IsNullOrEmpty(err.MinorCode) && (err.MinorCode.Trim().Equals("10049")))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsAFSSeatCouponApplied(MOBShoppingCart persistShoppingCart)
        {
            var coupon = persistShoppingCart.PromoCodeDetails?.PromoCodes;
            if (coupon != null && coupon.Count > 0 && coupon.Any(x => !string.IsNullOrEmpty(x?.Product) && x.Product.Contains("EPU")))
            {
                return true;
            }
            return false;
        }

        private string GetCommonSeatCode(string seatCode)
        {
            if (string.IsNullOrEmpty(seatCode))
                return string.Empty;

            string commonSeatCode = string.Empty;

            switch (seatCode.ToUpper().Trim())
            {
                case "SXZ": //StandardPreferredExitPlus
                case "SZX": //StandardPreferredExit
                case "SBZ": //StandardPreferredBlukheadPlus
                case "SZB": //StandardPreferredBlukhead
                case "SPZ": //StandardPreferredZone
                case "PZA":
                    commonSeatCode = "PZA";
                    break;
                case "SXP": //StandardPrimeExitPlus
                case "SPX": //StandardPrimeExit
                case "SBP": //StandardPrimeBlukheadPlus
                case "SPB": //StandardPrimeBlukhead
                case "SPP": //StandardPrimePlus
                case "PPE": //StandardPrime
                case "BSA":
                case "ASA":
                    commonSeatCode = "ASA";
                    break;
                case "EPL": //EplusPrime
                case "EPU": //EplusPrimePlus
                case "BHS": //BulkheadPrime
                case "BHP": //BulkheadPrimePlus  
                case "PSF": //PrimePlus 
                    commonSeatCode = "EPU";
                    break;
                case "PSL": //Prime                           
                    commonSeatCode = "PSL";
                    break;
                default:
                    return seatCode;
            }
            return commonSeatCode;
        }

        private bool IsPromoDetailsUpdatedByProduct(List<MOBProdDetail> persistedSCproducts, List<MOBProdDetail> updatedSCproducts, string productCode)
        {
            if (persistedSCproducts.Any(p => p.Code == productCode && p.CouponDetails != null && p.CouponDetails.Count > 0)
                || updatedSCproducts.Any(p => p.Code == productCode && p.CouponDetails != null && p.CouponDetails.Count > 0))
            {
                return true;
            }
            return false;
        }

        private void UpdateSeatsPriceWithPromo(FlightReservationResponse cslFlightReservationResponse, List<MOBSHOPPrice> prices, List<MOBSeatPrice> seatPrices, List<MOBProdDetail> persistedSCproducts, List<MOBProdDetail> updatedSCproducts)
        {
            double totalEplusPromoValue = 0,
                totalAdvanceSeatpromoValue = 0,
                totalPreferredSeatPromoValue = 0, promoValue = 0;

            if (prices != null
                && prices.Any(x => x.DisplayType.ToUpper().Contains("SEAT"))
                && IsPromoDetailsUpdatedByProduct(persistedSCproducts, updatedSCproducts, "SEATASSIGNMENTS"))
            {
                if (cslFlightReservationResponse.DisplayCart.DisplaySeats != null
                    && cslFlightReservationResponse.DisplayCart.DisplaySeats.Any())
                {
                    #region Get Promotion Value applied for Seat Category
                    foreach (var seat in cslFlightReservationResponse.DisplayCart.DisplaySeats)
                    {
                        promoValue = Convert.ToDouble(seat.OriginalPrice - seat.SeatPrice);
                        switch (GetCommonSeatCode(seat.SeatPromotionCode))
                        {
                            case "EPU":
                            case "PSL":
                                totalEplusPromoValue += promoValue;
                                break;
                            case "PZA":
                                totalPreferredSeatPromoValue += promoValue;
                                break;
                            case "ASA":
                                totalAdvanceSeatpromoValue += promoValue;
                                break;
                        }

                    }
                    #endregion Get Promotion Value applied for Seat Category
                    #region Update Reservation Prices(RTI)/SeatPrice for Price Breakdown
                    foreach (var price in prices)
                    {
                        if (price.DisplayType.ToUpper() == "ECONOMYPLUS SEATS")
                        {
                            CultureInfo ci = null;
                            price.PromoDetails = totalEplusPromoValue > 0 ? new MOBPromoCode
                            {
                                PromoValue = totalEplusPromoValue,
                                FormattedPromoDisplayValue = "-" + TopHelper.FormatAmountForDisplay(totalEplusPromoValue.ToString("C2"), ci, false),
                                PriceTypeDescription = _configuration.GetValue<string>("PromoCodeAppliedText")
                            } : null;
                            if (seatPrices != null && seatPrices
                                .Exists(p => p.SeatMessage.ToUpper().Contains("ECONOMY PLUS") || p.SeatMessage.ToUpper().Contains("LIMITED RECLINE")))
                            {
                                seatPrices
                                .Where(p => p.SeatMessage.ToUpper().Contains("ECONOMY PLUS") || p.SeatMessage.ToUpper().Contains("LIMITED RECLINE"))
                                .First().SeatPromoDetails = price.PromoDetails;
                            }
                        }
                        if (price.DisplayType.ToUpper() == _configuration.GetValue<string>("PreferedSeat_PriceBreakdownTitle").ToUpper())
                        {
                            CultureInfo ci = null;
                            price.PromoDetails = totalPreferredSeatPromoValue > 0 ? new MOBPromoCode
                            {
                                PromoValue = totalPreferredSeatPromoValue,
                                FormattedPromoDisplayValue = "-" + TopHelper.FormatAmountForDisplay(totalPreferredSeatPromoValue.ToString("C2"), ci, false),
                                PriceTypeDescription = _configuration.GetValue<string>("PromoCodeAppliedText")
                            } : null;

                            if (seatPrices != null && seatPrices
                                    .Exists(p => p.SeatMessage.ToUpper().Contains(_configuration.GetValue<string>("PreferedSeat_PriceBreakdownTitle").ToUpper())))
                            {
                                seatPrices
                                    .Where(p => p.SeatMessage.ToUpper().Contains(_configuration.GetValue<string>("PreferedSeat_PriceBreakdownTitle").ToUpper()))
                                    .First()
                                    .SeatPromoDetails = price.PromoDetails;
                            }
                        }
                        if (price.DisplayType.ToUpper() == "ADVANCE SEAT ASSIGNMENT")
                        {
                            CultureInfo ci = null;
                            price.PromoDetails = totalAdvanceSeatpromoValue > 0 ? new MOBPromoCode
                            {
                                PromoValue = totalAdvanceSeatpromoValue,
                                FormattedPromoDisplayValue = "-" + TopHelper.FormatAmountForDisplay(totalAdvanceSeatpromoValue.ToString("C2"), ci, false),
                                PriceTypeDescription = _configuration.GetValue<string>("PromoCodeAppliedText")
                            } : null;
                            if (seatPrices != null && seatPrices.Exists(p => p.SeatMessage.ToUpper().Contains("ADVANCE SEAT ASSIGNMENT")))
                            {
                                seatPrices.Where(p => p.SeatMessage.ToUpper().Contains("ADVANCE SEAT ASSIGNMENT")).First().SeatPromoDetails = price.PromoDetails;
                            }
                        }
                    }
                    #endregion  Update Reservation Prices(RTI)/SeatPrice for Price Breakdown
                }
            }
        }

        private void UpdateInhibitMessage(ref Reservation reservation)
        {
            if (reservation == null) return;

            if (reservation.ShopReservationInfo2 == null)
                reservation.ShopReservationInfo2 = new MOBSHOPReservationInfo2();

            if (reservation.ShopReservationInfo2.InfoWarningMessages == null)
                reservation.ShopReservationInfo2.InfoWarningMessages = new List<MOBInfoWarningMessages>();


            if (!reservation.ShopReservationInfo2.InfoWarningMessages.Exists(m => m.Order == MOBINFOWARNINGMESSAGEORDER.INHIBITBOOKING.ToString()))
            {
                List<MOBInfoWarningMessages> lst = reservation.ShopReservationInfo2.InfoWarningMessages.Clone();
                reservation.ShopReservationInfo2.InfoWarningMessages = new List<MOBInfoWarningMessages>();
                //reservation.ShopReservationInfo2.MOBInfoWarningMessages.Add(Utility.GetInhibitMessage());
                if (!_configuration.GetValue<bool>("TurnOffBookingCutoffMinsFromCSL"))
                {
                    reservation.ShopReservationInfo2.InfoWarningMessages.Add(GetInhibitMessage(reservation.ShopReservationInfo2.BookingCutOffMinutes));
                }
                else
                {
                    reservation.ShopReservationInfo2.InfoWarningMessages.Add(GetInhibitMessage(string.Empty));
                }

                reservation.ShopReservationInfo2.InfoWarningMessages.AddRange(lst);
            }

        }

        private MOBInfoWarningMessages GetInhibitMessage(string bookingCutOffMinutes)
        {
            MOBInfoWarningMessages inhibitMessage = new MOBInfoWarningMessages();

            inhibitMessage.Order = MOBINFOWARNINGMESSAGEORDER.INHIBITBOOKING.ToString();
            inhibitMessage.IconType = MOBINFOWARNINGMESSAGEICON.WARNING.ToString();

            inhibitMessage.Messages = new List<string>();
            if (!_configuration.GetValue<bool>("TurnOffBookingCutoffMinsFromCSL") && !string.IsNullOrEmpty(bookingCutOffMinutes))
            {
                inhibitMessage.Messages.Add(string.Format(_configuration.GetValue<string>("InhibitMessageV2"), bookingCutOffMinutes));
            }
            else
            {
                inhibitMessage.Messages.Add((_configuration.GetValue<string>("InhibitMessage") as string) ?? string.Empty);
            }
            return inhibitMessage;
        }

        private double ParseToDoubleAndRound(string displayValue)
        {
            double tempDouble = 0;
            double.TryParse(displayValue, out tempDouble);
            return Math.Round(tempDouble, 2, MidpointRounding.AwayFromZero);
        }

        private void UpdateGrandTotal(List<MOBSHOPPrice> prices, List<MOBSHOPPrice> persistedPrices)
        {
            var grandTotalPrice = prices.FirstOrDefault(p => p.DisplayType.ToUpper().Equals("GRAND TOTAL"));
            if (_configuration.GetValue<bool>("EnableAdvanceSearchCouponBooking") && grandTotalPrice == null || string.IsNullOrEmpty(grandTotalPrice.DisplayType))
                return;
            #region Find previous total promo and add it to GrandTotal
            double persistedPromoValue = 0;
            persistedPrices.ForEach(p =>
            {
                if (p.PromoDetails != null && (!(p.DisplayType.ToUpper().Equals("GRAND TOTAL") || p.DisplayType.ToUpper().Equals("TRAVELERPRICE"))))
                {
                    persistedPromoValue += p.PromoDetails.PromoValue;
                }
            });
            //Update grand total by adding the previous promovalue 
            grandTotalPrice.DisplayValue = string.Format("{0:#,0.00}", grandTotalPrice.Value + persistedPromoValue);
            grandTotalPrice.FormattedDisplayValue = string.Format("${0:c}", grandTotalPrice.DisplayValue);
            grandTotalPrice.Value = Math.Round(grandTotalPrice.Value + persistedPromoValue, 2, MidpointRounding.AwayFromZero);
            #endregion
            #region Update GrandTotal with new promo value
            double totalPromoVlaue = 0;
            prices.ForEach(p =>
            {
                if (p.PromoDetails != null && (!(p.DisplayType.ToUpper().Equals("GRAND TOTAL") || p.DisplayType.ToUpper().Equals("TRAVELERPRICE"))))
                {
                    totalPromoVlaue += p.PromoDetails != null ? p.PromoDetails.PromoValue : 0;
                }
            });
            grandTotalPrice.DisplayValue = string.Format("{0:#,0.00}", grandTotalPrice.Value - totalPromoVlaue);
            grandTotalPrice.FormattedDisplayValue = string.Format("${0:c}", grandTotalPrice.DisplayValue);
            grandTotalPrice.Value = Math.Round(grandTotalPrice.Value - totalPromoVlaue, 2, MidpointRounding.AwayFromZero);
            CultureInfo ci = null;
            grandTotalPrice.PromoDetails = totalPromoVlaue > 0 ? new MOBPromoCode
            {
                PriceTypeDescription = _configuration.GetValue<string>("PromoSavedText"),
                PromoValue = totalPromoVlaue,
                FormattedPromoDisplayValue = TopHelper.FormatAmountForDisplay(totalPromoVlaue.ToString("C2"), ci, false)
            } : null;
            #endregion update GrandTotal with new promo value
        }
        private bool IsSeatMapSupportedOa(string operatingCarrier, string MarketingCarrier)
        {
            if (string.IsNullOrEmpty(operatingCarrier)) return false;
            var seatMapSupportedOa = _configuration.GetValue<string>("SeatMapSupportedOtherAirlines");
            if (string.IsNullOrEmpty(seatMapSupportedOa)) return false;

            var seatMapEnabledOa = seatMapSupportedOa.Split(',');
            if (seatMapEnabledOa.Any(s => s == operatingCarrier.ToUpper().Trim()))
                return true;
            else if (_configuration.GetValue<string>("SeatMapSupportedOtherAirlinesMarketedBy") != null)
            {
                return _configuration.GetValue<string>("SeatMapSupportedOtherAirlinesMarketedBy").Split(',').ToList().Any(m => m == MarketingCarrier + "-" + operatingCarrier);
            }
            return false;

        }

        private bool EnableAdvanceSearchCouponBooking(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableAdvanceSearchCouponBooking") && GeneralHelper.IsApplicationVersionGreaterorEqual(appId, appVersion, _configuration.GetValue<string>("AndroidAdvanceSearchCouponBookingVersion"), _configuration.GetValue<string>("iPhoneAdvanceSearchCouponBookingVersion"));
        }

        private List<MOBTypeOption> GetHazMat()
        {
            List<MOBTypeOption> hazMat = new List<MOBTypeOption>();

            hazMat.Add(new MOBTypeOption("federallaw", _configuration.GetValue<string>("federallaw")));
            hazMat.Add(new MOBTypeOption("byselect", _configuration.GetValue<string>("byselect")));
            hazMat.Add(new MOBTypeOption("prohibited", _configuration.GetValue<string>("prohibited")));
            hazMat.Add(new MOBTypeOption("acknowledge", _configuration.GetValue<string>("acknowledge")));
            hazMat.Add(new MOBTypeOption("hazmatitems", _configuration.GetValue<string>("hazmatitems")));
            hazMat.Add(new MOBTypeOption("hazmatitems1", _configuration.GetValue<string>("hazmatitems1")));
            hazMat.Add(new MOBTypeOption("hazmatitems2", _configuration.GetValue<string>("hazmatitems2")));
            hazMat.Add(new MOBTypeOption("notice", _configuration.GetValue<string>("notice")));

            return hazMat;
        }

        private void PopulateSelectedAncillaries(Reservation reservation, MOBRequest request, string sessionId)
        {
            try
            {
                var allFlightSegments = reservation?.Trips?.SelectMany(trip => trip?.FlattenedFlights?.SelectMany(flattendFlghts => flattendFlghts?.Flights));
                if (allFlightSegments != null)
                {
                    int flightSegmentNumber = 0;

                    foreach (var flight in allFlightSegments)
                    {
                        var selectedSeats = new List<string>();
                        reservation?.TravelersCSL.ForEach(traveler =>
                        {
                            var selectedSeat = traveler.Value.Seats?.FirstOrDefault(seat => seat.Origin == flight.Origin && seat.Destination == flight.Destination)?.SeatAssignment;
                            if (!string.IsNullOrEmpty(selectedSeat))
                            {
                                selectedSeats.Add(selectedSeat);
                            }
                        });
                        flight.SelectedAncillaries = AssignSelectedSeats(selectedSeats, flight.SelectedAncillaries);
                        flightSegmentNumber++;
                    }
                }
            }
            catch (Exception ex)
            {
                MOBExceptionWrapper exceptionWrapper = new MOBExceptionWrapper(ex);
                _logger.LogError("PopulateSelectedAncillaries-UnitedException {message} {StackTrace}", ex.Message, ex.StackTrace);
                // logEntries.Add(LogEntry.GetLogEntry<MOBExceptionWrapper>(sessionId, "PopulateSelectedAncillaries", "UnitedException", request.Application.Id, request.Application.Version.Major, request.DeviceId, exceptionWrapper));
            }
        }

        private List<MOBItem> AssignSelectedSeats(IEnumerable<string> selectedSeats, List<MOBItem> selectedAncillaries)
        {
            selectedAncillaries = selectedAncillaries ?? new List<MOBItem>();

            var seatsAncillary = !_configuration.GetValue<bool>("DisableOminiCartAssignSelectedSeatsFix")
                ? selectedAncillaries.FirstOrDefault()
                : selectedAncillaries.FirstOrDefault(selectedAncillary => selectedAncillary.Id.Equals(_configuration.GetValue<string>("BookingShoppingCartPriceBreakdownSeatsLabelText")));
            string seats = String.Join(",", selectedSeats).TrimEnd(new[] { ',' });
            if (seatsAncillary != null)
            {
                seatsAncillary.CurrentValue = seats;
                //Assign empty value in id when seats empty 
                if (!_configuration.GetValue<bool>("DisableOminiCartAssignSelectedSeatsFix"))
                {
                    if (string.IsNullOrWhiteSpace(seats))
                    {
                        seatsAncillary.Id = string.Empty;
                    }
                    else
                    {
                        seatsAncillary.Id = _configuration.GetValue<string>("BookingShoppingCartPriceBreakdownSeatsLabelText");
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(seats))
                {
                    selectedAncillaries.Add(new MOBItem
                    {
                        Id = _configuration.GetValue<string>("BookingShoppingCartPriceBreakdownSeatsLabelText"),
                        CurrentValue = seats
                    });
                }
            }
            return selectedAncillaries;
        }

        private async Task<MOBBookingBundlesResponse> UpdateBundleResponse(Reservation persistedReservation, string sessionId)
        {
            MOBBookingBundlesResponse bundleResponse = new MOBBookingBundlesResponse(_configuration);
            bundleResponse = await _sessionHelperService.GetSession<MOBBookingBundlesResponse>(sessionId, bundleResponse.ObjectName, new List<string> { sessionId, bundleResponse.ObjectName }).ConfigureAwait(false);

            if (persistedReservation?.TravelOptions != null && bundleResponse.Products !=null && bundleResponse.Products.Count > 0)
            {
                for (int i = 0; i < bundleResponse.Products?.Count; i++)
                {
                    for (int j = 0; j < persistedReservation.TravelOptions.Count; j++)
                    {
                        if (persistedReservation.TravelOptions[j].SubItems[0].ProductId == bundleResponse.Products[i].ProductCode)
                        {
                            bundleResponse.Products[i].Tile.IsSelected = true;

                            for (int k = 0; k < persistedReservation.TravelOptions[j].TripIds.Count; k++)
                            {
                                for (int l = 0; l < bundleResponse.Products[i].Detail.OfferTrips.Count; l++)
                                {
                                    if (persistedReservation.TravelOptions[j].TripIds[k] == bundleResponse.Products[i].Detail.OfferTrips[l].TripId)
                                    {
                                        bundleResponse.Products[i].Detail.OfferTrips[l].IsChecked = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return bundleResponse;
        }

        private async Task<MOBBookingRegisterOfferResponse> RegisterOfferforOTP(MOBRegisterOfferRequest request, Session session)
        {
            MOBBookingRegisterOfferResponse response = new MOBBookingRegisterOfferResponse();
            MOBRegisterOfferResponse registerOfferResponse = new MOBRegisterOfferResponse();
            FlightReservationResponse cslResponse = new FlightReservationResponse();
            MOBShoppingCart persistedShoppingCart = new MOBShoppingCart();
            response.TransactionId = request.TransactionId;
            response.LanguageCode = request.LanguageCode;
            #region
            try
            {
                Reservation reservation = new Reservation();
                reservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, reservation.ObjectName, new List<string> { request.SessionId, reservation.ObjectName }).ConfigureAwait(false);

                if (reservation != null)
                {
                    response.Reservation = new MOBSHOPReservation(_configuration, _cachingService);
                    response.Reservation = await _productUtility.GetReservationFromPersist(response.Reservation, session);

                    List<MOBSHOPPrice> prices = null;
                    if (response.Reservation.Prices != null && response.Reservation.Prices.Count > 0)
                    {
                        double travelOptionSubTotal = 0.0;
                        foreach (var price in response.Reservation.Prices)
                        {
                            if (price.PriceType.ToUpper().Equals("TRAVEL OPTIONS") || price.PriceType.ToUpper().Equals("ONE-TIME PASS"))
                            {
                                travelOptionSubTotal = travelOptionSubTotal + Convert.ToDouble(price.DisplayValue);
                            }
                            else if (!price.PriceType.ToUpper().Equals("GRAND TOTAL"))
                            {
                                if (prices == null)
                                {
                                    prices = new List<MOBSHOPPrice>();
                                }
                                prices.Add(price);
                            }
                        }

                        foreach (var price in response.Reservation.Prices)
                        {
                            if (price.PriceType.ToUpper().Equals("GRAND TOTAL"))
                            {
                                double grandTotal = Convert.ToDouble(price.DisplayValue);
                                price.DisplayValue = string.Format("{0:#,0.00}", grandTotal - travelOptionSubTotal);
                                price.FormattedDisplayValue = string.Format("${0:c}", price.DisplayValue);
                                double tempDouble1 = 0;
                                double.TryParse(price.DisplayValue.ToString(), out tempDouble1);
                                price.Value = Math.Round(tempDouble1, 2, MidpointRounding.AwayFromZero);
                                if (prices == null)
                                {
                                    prices = new List<MOBSHOPPrice>();
                                }
                                prices.Add(price);
                            }
                        }
                    }

                    response.Reservation.Prices = prices;

                    response.Reservation.SeatPrices = reservation.SeatPrices.Clone();
                    response.Reservation.Taxes = reservation.Taxes.Clone();
                    if (reservation.TravelersCSL != null)
                    {
                        response.Reservation.TravelersCSL = new List<MOBCPTraveler>();
                        foreach (var traveler in reservation.TravelersCSL)
                        {
                            response.Reservation.TravelersCSL.Add(traveler.Value.Clone());
                        }
                    }
                    //Code by prasad prasad for Emp-20 single pax fix
                    if (reservation.NumberOfTravelers == 1 && response.Reservation.TravelersCSL == null)
                    {
                        if (reservation.ShopReservationInfo2.AllEligibleTravelersCSL != null)
                        {
                            response.Reservation.TravelersCSL = new List<MOBCPTraveler>();
                            foreach (MOBCPTraveler cpTraveler in reservation.ShopReservationInfo2.AllEligibleTravelersCSL)
                            {
                                if (cpTraveler.IsProfileOwner)
                                {
                                    response.Reservation.TravelersCSL.Add(cpTraveler);
                                }
                            }
                        }
                    }

                    response.Reservation.Trips = reservation.Trips.Clone();


                    //Load the shopping cart to find whether OTP is already registered.
                    persistedShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, persistedShoppingCart.ObjectName, new List<string> { request.SessionId, persistedShoppingCart.ObjectName }).ConfigureAwait(false);

                    //Unregistering all the previously Registered OTP's
                    if (persistedShoppingCart != null && persistedShoppingCart.Products != null && persistedShoppingCart.Products.Count > 0)
                    {
                        if (persistedShoppingCart.Products.Exists(x => x.Code == "OTP"))
                        {
                            var persistedShoppingCartOTPProduct = persistedShoppingCart.Products.Where(x => x.Code == "OTP").ToList();
                            //Loading Offer to get the price of the OTP
                            var productOffer = new GetOffers();
                            productOffer = await _sessionHelperService.GetSession<GetOffers>(session.SessionId, productOffer.ObjectName.ToString(), new List<string> { session.SessionId, productOffer.ObjectName.ToString() }).ConfigureAwait(false);

                            string productId = productOffer.Offers.FirstOrDefault().ProductInformation.ProductDetails.FirstOrDefault().Product.SubProducts.FirstOrDefault().Prices.FirstOrDefault().ID;
                            double otpPrice = (productOffer.Offers.FirstOrDefault().ProductInformation.ProductDetails.FirstOrDefault().Product.SubProducts.FirstOrDefault().Prices.FirstOrDefault().PaymentOptions.FirstOrDefault().PriceComponents.FirstOrDefault().Price.BasePrice.FirstOrDefault().Amount);
                            //Building the previousRequest for OTP
                            var products = persistedShoppingCartOTPProduct.Select(x => new MerchandizingOfferDetails
                            {
                                ProductCode = x.Code,
                                ProductIds = x.Segments.SelectMany(y => y.ProductIds).ToList(),
                                NumberOfPasses = (int)((Convert.ToDouble(x.ProdTotalPrice)) / otpPrice),
                                IsOfferRegistered = true
                            }).ToList();

                            if (request.MerchandizingOfferDetails != null && request.MerchandizingOfferDetails.Count > 0)
                            {
                                //Unregister OTP only if the previously registered number of OTP's are not equal to currently requested number of OTP's
                                if (products.FirstOrDefault().NumberOfPasses != request.MerchandizingOfferDetails.FirstOrDefault().NumberOfPasses)
                                {
                                    products.AddRange(request.MerchandizingOfferDetails);
                                    request.MerchandizingOfferDetails = new Collection<MerchandizingOfferDetails>(products);
                                }
                                else//sending the shoppingcart in persist when Previous Registered Number of OTP's or equal to requested number of OTP's.Since we will not registeroffers at all.
                                {
                                    response.ShoppingCart = persistedShoppingCart;
                                }

                            }
                            else//This is when you register multiple OTP's and unregistering all the OTP's(RTI)
                            {

                                request.MerchandizingOfferDetails = new Collection<MerchandizingOfferDetails>(products);
                            }

                        }
                    }
                    if (request != null)
                    {

                        if (request.MerchandizingOfferDetails != null && request.MerchandizingOfferDetails.Count > 0)
                        {
                            if (request.MerchandizingOfferDetails.Any(x => x.ProductCode == "OTP" && x.IsOfferRegistered == false))
                            {
                                request.MerchandizingOfferDetails.Where(x => x.ProductCode == "OTP" && x.IsOfferRegistered == false).FirstOrDefault().ProductIds = BuildProductIdsForOTP(request.MerchandizingOfferDetails.Where(x => x.ProductCode == "OTP" && x.IsOfferRegistered == false).FirstOrDefault().NumberOfPasses);
                            }
                            var tupleResponse = await RegisterOffers(request, session, false);
                            registerOfferResponse = tupleResponse.response;
                            cslResponse = tupleResponse.flightReservationResponse;
                            response.Reservation.TravelOptions = GetReShopTravelOptions(cslResponse.DisplayCart, request.Application.Id, request.Application.Version.Major, session.IsReshopChange);
                            response.ShoppingCart = registerOfferResponse.ShoppingCart;
                            if (session.CatalogItems != null && session.CatalogItems.Count > 0 && _shoppingUtility.IsEnablePartnerProvision(session?.CatalogItems, request.Flow, request.Application.Id, request.Application.Version.Major))
                            {
                                if (persistedShoppingCart != null && persistedShoppingCart.PartnerProvisionDetails != null)
                                {
                                    response.ShoppingCart.PartnerProvisionDetails = new PartnerProvisionDetails();
                                    response.ShoppingCart.PartnerProvisionDetails = persistedShoppingCart.PartnerProvisionDetails;
                                }
                            }
                        }
                        else
                        {
                            response.ShoppingCart = persistedShoppingCart;
                        }


                        if (request.ClubPassPurchaseRequest != null && request.ClubPassPurchaseRequest.NumberOfPasses > 0)
                        {
                            response.Reservation.ClubPassPurchaseRequest = request.ClubPassPurchaseRequest;
                            reservation.ClubPassPurchaseRequest = response.Reservation.ClubPassPurchaseRequest;
                        }
                        else
                        {
                            response.Reservation.ClubPassPurchaseRequest = null;
                            reservation.ClubPassPurchaseRequest = null;
                        }
                        response.Reservation.Prices = UpdatePricesforOTP(response.Reservation, request);
                        reservation.TravelOptions = response.Reservation.TravelOptions;
                        reservation.ClubPassPurchaseRequest = response.Reservation.ClubPassPurchaseRequest;
                        if (response.Reservation.Prices != null && response.Reservation.Prices.Count > 0)
                        {
                            reservation.Prices = response.Reservation.Prices;
                        }

                        if (session != null && !string.IsNullOrEmpty(session.EmployeeId))
                        {
                            response.Reservation.IsEmp20 = true;
                        }

                        if ((reservation.ShopReservationInfo2.NextViewName == string.Empty || (reservation.ShopReservationInfo2.IsForceSeatMapInRTI && reservation.ShopReservationInfo2.NextViewName == "TravelOption")) && !(reservation.IsELF && !reservation.IsSSA))
                        {
                            reservation.ShopReservationInfo2.NextViewName = "Seats";
                            reservation.ShopReservationInfo2.IsForceSeatMapInRTI = true;
                        }
                        else
                        {
                            reservation.ShopReservationInfo2.NextViewName = "RTI";
                        }

                        if (reservation.ShopReservationInfo2.NextViewName == "RTI" || reservation.IsELF)
                        {
                            #region TPI in booking path
                            if (EnableTPI(request.Application.Id, request.Application.Version.Major, 3) && !reservation.IsReshopChange)
                            {
                                // call TPI 
                                try
                                {
                                    string token = session.Token;
                                    MOBTPIInfoInBookingPath tPIInfo = await GetBookingPathTPIInfo(request.SessionId, request.LanguageCode, request.Application, request.DeviceId, response.Reservation.CartId, token, true, false, false);
                                    reservation.TripInsuranceFile = new MOBTripInsuranceFile();
                                    reservation.TripInsuranceFile.TripInsuranceBookingInfo = tPIInfo;
                                }
                                catch
                                {
                                    reservation.TripInsuranceFile = null;
                                }
                            }
                            #endregion
                        }

                        // Code is moved to utility so that it can be reused across different actions
                        if (_shoppingCartUtility.EnableChaseOfferRTI(request.Application.Id, request.Application.Version.Major))
                            _shoppingCartUtility.UpdateChaseCreditStatement(reservation);

                        await _sessionHelperService.SaveSession<Reservation>(reservation, request.SessionId, new List<string> { request.SessionId, reservation.ObjectName }, reservation.ObjectName).ConfigureAwait(false);

                        response.Reservation = await _productUtility.GetReservationFromPersist(response.Reservation, session);
                    }
                }
                response.PkDispenserPublicKey = registerOfferResponse.PkDispenserPublicKey;
            }
            catch (MOBUnitedException uaex)
            {
                throw uaex;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            response.SessionId = request.SessionId;
            response.CheckinSessionId = request.CheckinSessionId;
            response.Flow = request.Flow;
            #endregion
            var bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, new Reservation().ObjectName, new List<string> { request.SessionId, new Reservation().ObjectName }).ConfigureAwait(false);

           await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, bookingPathReservation.SessionId, new List<string> { bookingPathReservation.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);

            return response;
        }


        private CultureInfo GetCultureInfo(string currencyCode)
        {
            CultureInfo culture = new CultureInfo("en-US");

            if (!string.IsNullOrEmpty(currencyCode))
            {
                var cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);

                foreach (var ci in cultures)
                {
                    try
                    {
                        var ri = new RegionInfo(ci.Name);
                        if (ri.ISOCurrencySymbol.ToUpper() == currencyCode.ToUpper())
                        {
                            culture = ci;
                            break;
                        }
                    }
                    catch { culture = new CultureInfo("en-US"); }
                }
            }

            return culture;
        }

        private List<MOBSHOPPrice> UpdatePricesforOTP(MOBSHOPReservation reservation, MOBRegisterOfferRequest request)
        {
            List<MOBSHOPPrice> prices = reservation.Prices.Clone();

            if (reservation.TravelOptions != null && reservation.TravelOptions.Count > 0)
            {
                foreach (var travelOption in reservation.TravelOptions)
                {
                    //below if condition modified by prasad for bundle checking
                    if (!travelOption.Key.Equals("PAS"))
                    {
                        List<MOBSHOPPrice> totalPrices = new List<MOBSHOPPrice>();
                        bool totalExist = false;
                        double flightTotal = 0;

                        CultureInfo ci = null;

                        for (int i = 0; i < prices.Count; ++i)
                        {
                            if (ci == null)
                                ci = GetCultureInfo(prices[i].CurrencyCode);

                            if (prices[i].DisplayType.ToUpper() == "GRAND TOTAL")
                            {
                                totalExist = true;
                                prices[i].DisplayValue = string.Format("{0:#,0.00}", (Convert.ToDouble(prices[i].DisplayValue) + travelOption.Amount));
                                prices[i].FormattedDisplayValue = string.Format("${0:c}", prices[i].DisplayValue); //formatAmountForDisplay(prices[i].DisplayValue, ci, false); 
                                double tempDouble1 = 0;
                                double.TryParse(prices[i].DisplayValue.ToString(), out tempDouble1);
                                prices[i].Value = Math.Round(tempDouble1, 2, MidpointRounding.AwayFromZero);
                            }
                            if (prices[i].DisplayType.ToUpper() == "TOTAL")
                            {
                                flightTotal = Convert.ToDouble(prices[i].DisplayValue);
                            }
                        }
                        MOBSHOPPrice travelOptionPrice = new MOBSHOPPrice();
                        travelOptionPrice.CurrencyCode = travelOption.CurrencyCode;
                        travelOptionPrice.DisplayType = "One-time Pass";
                        travelOptionPrice.DisplayValue = string.Format("{0:#,0.00}", travelOption.Amount.ToString());
                        travelOptionPrice.FormattedDisplayValue = TopHelper.FormatAmountForDisplay(travelOptionPrice.DisplayValue, new CultureInfo("en-US"), false); //formatAmountForDisplay(travelOptionPrice.DisplayValue, ci, false);TopHelper.FormatAmountForDisplay(travelOptionPrice.DisplayValue, new CultureInfo("en-US"), false);
                        travelOptionPrice.PriceType = "One-time Pass";

                        double tmpDouble1 = 0;
                        double.TryParse(travelOptionPrice.DisplayValue.ToString(), out tmpDouble1);
                        travelOptionPrice.Value = Math.Round(tmpDouble1, 2, MidpointRounding.AwayFromZero);

                        prices.Add(travelOptionPrice);

                        if (!totalExist)
                        {
                            MOBSHOPPrice totalPrice = new MOBSHOPPrice();
                            totalPrice.CurrencyCode = travelOption.CurrencyCode;
                            totalPrice.DisplayType = "Grand Total";
                            totalPrice.DisplayValue = (flightTotal + travelOption.Amount).ToString("N2", CultureInfo.InvariantCulture);
                            totalPrice.FormattedDisplayValue = string.Format("${0:c}", totalPrice.DisplayValue); //formatAmountForDisplay(totalPrice.DisplayValue, ci, false); 
                            double tempDouble1 = 0;
                            double.TryParse(totalPrice.DisplayValue.ToString(), out tempDouble1);
                            totalPrice.Value = Math.Round(tempDouble1, 2, MidpointRounding.AwayFromZero);
                            totalPrice.PriceType = "Grand Total";
                            prices.Add(totalPrice);
                        }
                    }
                }
            }
            return prices;
        }

        private List<MOBSHOPTravelOption> GetTravelOptions(Services.FlightShopping.Common.DisplayCart.DisplayCart displayCart, int appID, string appVersion, bool isReshopChange)
        {
            List<MOBSHOPTravelOption> travelOptions = null;
            if (displayCart != null && displayCart.TravelOptions != null && displayCart.TravelOptions.Count > 0)
            {
                CultureInfo ci = null;
                travelOptions = new List<MOBSHOPTravelOption>();
                bool addTripInsuranceInTravelOption =
                    !_configuration.GetValue<bool>("ShowTripInsuranceBookingSwitch")
                    && Convert.ToBoolean(_configuration.GetValue<string>("ShowTripInsuranceSwitch") ?? "false");
                foreach (var anOption in displayCart.TravelOptions)
                {
                    //wade - added check for farelock as we were bypassing it
                    if (!anOption.Type.Equals("Premium Access") && !anOption.Key.Trim().ToUpper().Contains("FARELOCK") && !(addTripInsuranceInTravelOption && anOption.Key.Trim().ToUpper().Contains("TPI")) &&
                        !(EnableEPlusAncillary(appID, appVersion, isReshopChange) && anOption.Key.Trim().ToUpper().Contains("EFS")))
                    {
                        continue;
                    }
                    if (ci == null)
                    {
                        ci = GetCultureInfo(anOption.Currency);
                    }

                    MOBSHOPTravelOption travelOption = new MOBSHOPTravelOption();
                    travelOption.Amount = (double)anOption.Amount;

                    travelOption.DisplayAmount = formatAmountForDisplay(anOption.Amount, ci, false);

                    //??
                    if (anOption.Key.Trim().ToUpper().Contains("FARELOCK") || (addTripInsuranceInTravelOption && anOption.Key.Trim().ToUpper().Contains("TPI")))
                        travelOption.DisplayButtonAmount = formatAmountForDisplay(anOption.Amount, ci, false);
                    else
                        travelOption.DisplayButtonAmount = formatAmountForDisplay(anOption.Amount, ci, true);

                    travelOption.CurrencyCode = anOption.Currency;
                    travelOption.Deleted = anOption.Deleted;
                    travelOption.Description = anOption.Description;
                    travelOption.Key = anOption.Key;
                    travelOption.ProductId = anOption.ProductId;
                    travelOption.SubItems = GetTravelOptionSubItems(anOption.SubItems);
                    if (EnableEPlusAncillary(appID, appVersion, isReshopChange) && anOption.SubItems != null && anOption.SubItems.Count > 0)
                    {
                        travelOption.BundleCode = GetTravelOptionEplusAncillary(anOption.SubItems, travelOption.BundleCode);
                        GetTravelOptionAncillaryDescription(anOption.SubItems, travelOption, displayCart);
                    }
                    if (!string.IsNullOrEmpty(anOption.Type))
                    {
                        travelOption.Type = anOption.Type.Equals("Premium Access") ? "Premier Access" : anOption.Type;
                    }
                    travelOptions.Add(travelOption);
                }
            }

            return travelOptions;
        }
        private List<MOBSHOPTravelOption> GetReShopTravelOptions(Services.FlightShopping.Common.DisplayCart.DisplayCart displayCart, int appID, string appVersion, bool isReshopChange)
        {
            List<MOBSHOPTravelOption> travelOptions = null;
            if (displayCart != null && displayCart.TravelOptions != null && displayCart.TravelOptions.Count > 0)
            {
                CultureInfo ci = null;
                travelOptions = new List<MOBSHOPTravelOption>();
                bool addTripInsuranceInTravelOption =
                    !_configuration.GetValue<bool>("ShowTripInsuranceBookingSwitch")
                    && Convert.ToBoolean(_configuration.GetValue<string>("ShowTripInsuranceSwitch") ?? "false");
                foreach (var anOption in displayCart.TravelOptions)
                {
                    //wade - added check for farelock as we were bypassing it
                    if (!anOption.Type.Equals("Premium Access") && !anOption.Key.Trim().ToUpper().Contains("FARELOCK") && !anOption.Key.Trim().ToUpper().Contains("OTP") && !(addTripInsuranceInTravelOption && anOption.Key.Trim().ToUpper().Contains("TPI")) &&
                        !(EnableEPlusAncillary(appID, appVersion, isReshopChange) && anOption.Key.Trim().ToUpper().Contains("EFS")))

                    {
                        continue;
                    }
                    if (ci == null)
                    {
                        ci = GetCultureInfo(anOption.Currency);
                    }

                    MOBSHOPTravelOption travelOption = new MOBSHOPTravelOption();
                    travelOption.Amount = (double)anOption.Amount;

                    travelOption.DisplayAmount = formatAmountForDisplay(anOption.Amount, ci, false);

                    //??
                    if (anOption.Key.Trim().ToUpper().Contains("FARELOCK") || (addTripInsuranceInTravelOption && anOption.Key.Trim().ToUpper().Contains("TPI")))
                        travelOption.DisplayButtonAmount = formatAmountForDisplay(anOption.Amount, ci, false);
                    else
                        travelOption.DisplayButtonAmount = formatAmountForDisplay(anOption.Amount, ci, true);

                    travelOption.CurrencyCode = anOption.Currency;
                    travelOption.Deleted = anOption.Deleted;
                    travelOption.Description = anOption.Description;
                    travelOption.Key = anOption.Key;
                    travelOption.ProductId = anOption.ProductId;
                    travelOption.SubItems = GetTravelOptionSubItems(anOption.SubItems);
                    if (EnableEPlusAncillary(appID, appVersion, isReshopChange) && anOption.SubItems != null && anOption.SubItems.Count > 0)
                    {
                        travelOption.BundleCode = GetTravelOptionEplusAncillary(anOption.SubItems, travelOption.BundleCode);
                        GetTravelOptionAncillaryDescription(anOption.SubItems, travelOption, displayCart);
                    }
                    if (!string.IsNullOrEmpty(anOption.Type))
                    {
                        travelOption.Type = anOption.Type.Equals("Premium Access") ? "Premier Access" : anOption.Type;
                    }
                    travelOptions.Add(travelOption);
                }
            }

            return travelOptions;
        }

        private void GetTravelOptionAncillaryDescription(Services.FlightShopping.Common.DisplayCart.SubitemsCollection subitemsCollection, MOBSHOPTravelOption travelOption, Services.FlightShopping.Common.DisplayCart.DisplayCart displayCart)
        {
            List<MOBAncillaryDescriptionItem> ancillaryDesciptionItems = new List<MOBAncillaryDescriptionItem>();
            CultureInfo ci = null;

            if (subitemsCollection.Any(t => t?.Type?.Trim().ToUpper() == "EFS"))
            {
                var trips = subitemsCollection.GroupBy(x => x.TripIndex);
                foreach (var trip in trips)
                {
                    if (trip != null)
                    {
                        decimal ancillaryAmount = 0;
                        foreach (var item in trip)
                        {
                            ancillaryAmount += item.Amount;
                            if (ci == null)
                            {
                                ci = GetCultureInfo(item.Currency);
                            }
                        }

                        MOBAncillaryDescriptionItem objeplus = new MOBAncillaryDescriptionItem();
                        objeplus.DisplayValue = formatAmountForDisplay(ancillaryAmount, ci, false);
                        objeplus.SubTitle = displayCart.DisplayTravelers?.Count.ToString() + (displayCart.DisplayTravelers?.Count > 1 ? " travelers" : " traveler");
                        var displayTrip = displayCart.DisplayTrips?.FirstOrDefault(s => s.Index == Convert.ToInt32(trip.FirstOrDefault().TripIndex));
                        if (displayTrip != null)
                        {
                            objeplus.Title = displayTrip.Origin + " - " + displayTrip.Destination;
                        }
                        ancillaryDesciptionItems.Add(objeplus);
                    }
                }

                travelOption.BundleOfferTitle = "Economy Plus®";
                travelOption.BundleOfferSubtitle = "Included with your fare";
                travelOption.AncillaryDescriptionItems = ancillaryDesciptionItems;
            }
        }

        private List<MobShopBundleEplus> GetTravelOptionEplusAncillary(Services.FlightShopping.Common.DisplayCart.SubitemsCollection subitemsCollection, List<MobShopBundleEplus> bundlecode)
        {
            if (bundlecode == null || bundlecode.Count == 0)
            {
                bundlecode = new List<MobShopBundleEplus>();
            }

            foreach (var item in subitemsCollection)
            {
                if (item?.Type?.Trim().ToUpper() == "EFS")
                {
                    MobShopBundleEplus objeplus = new MobShopBundleEplus();
                    objeplus.ProductKey = item.Type;
                    objeplus.AssociatedTripIndex = Convert.ToInt32(item.TripIndex);
                    bundlecode.Add(objeplus);
                }
            }

            return bundlecode;
        }

        private List<MOBSHOPTravelOptionSubItem> GetTravelOptionSubItems(Services.FlightShopping.Common.DisplayCart.SubitemsCollection subitemsCollection)
        {
            List<MOBSHOPTravelOptionSubItem> subItems = null;

            if (subitemsCollection != null && subitemsCollection.Count > 0)
            {
                CultureInfo ci = null;
                subItems = new List<MOBSHOPTravelOptionSubItem>();

                foreach (var item in subitemsCollection)
                {
                    if (ci == null)
                    {
                        ci = GetCultureInfo(item.Currency);
                    }

                    MOBSHOPTravelOptionSubItem subItem = new MOBSHOPTravelOptionSubItem();
                    subItem.Amount = (double)item.Amount;
                    subItem.DisplayAmount = formatAmountForDisplay(item.Amount, ci, false);
                    subItem.CurrencyCode = item.Currency;
                    subItem.Description = item.Description;
                    subItem.Key = item.Key;
                    subItem.ProductId = item.Type;
                    subItem.Value = item.Value;

                    subItems.Add(subItem);
                }

            }

            return subItems;
        }

        private bool EnableEPlusAncillary(int appID, string appVersion, bool isReshop = false)
        {
            return _configuration.GetValue<bool>("EnableEPlusAncillaryChanges") && !isReshop && GeneralHelper.IsApplicationVersionGreaterorEqual(appID, appVersion, _configuration.GetValue<string>("EplusAncillaryAndroidversion"), _configuration.GetValue<string>("EplusAncillaryiOSversion"));
        }

        private string formatAmountForDisplay(decimal amt, CultureInfo ci, /*string currency,*/ bool roundup = true, bool isAward = false)
        {
            return formatAmountForDisplay(amt.ToString(), ci, roundup, isAward);
        }

        private bool EnableTPI(int appId, string appVersion, int path)
        {
            // path 1 means confirmation flow, path 2 means view reservation flow, path 3 means booking flow 
            if (path == 1)
            {
                // ==>> Venkat and Elise chagne code to offer TPI postbooking when inline TPI is off for new clients 2.1.36 and above
                // App Version 2.1.36 && ShowTripInsuranceSwitch = true
                bool offerTPIAtPostBooking = _configuration.GetValue<bool>("ShowTripInsuranceSwitch") &&
                    GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidTPIConfirmationVersion", "iPhoneTPIConfirmationVersion", "", "", true, _configuration);
                if (offerTPIAtPostBooking)
                {
                    offerTPIAtPostBooking = !GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidTPIBookingVersion", "iPhoneTPIBookingVersion", "", "", true, _configuration);                    // When the Flag is true, we offer for old versions, when the flag is off, we offer for all versions. 
                    if (!offerTPIAtPostBooking && _configuration.GetValue<bool>("ShowTPIatPostBooking_ForAppVer_2.1.36_UpperVersions"))
                    {
                        offerTPIAtPostBooking = true;
                    }
                }
                return offerTPIAtPostBooking;
            }
            else if (path == 2)
            {
                return _configuration.GetValue<bool>("ShowTripInsuranceViewResSwitch") &&
                    GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidTPIViewResVersion", "iPhoneTPIViewResVersion", "", "", true, _configuration);
            }
            else if (path == 3)
            {
                return _configuration.GetValue<bool>("ShowTripInsuranceBookingSwitch") &&
                    GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidTPIBookingVersion", "iPhoneTPIBookingVersion", "", "", true, _configuration);
            }
            else
            {
                return false;
            }
        }

        private List<string> BuildProductIdsForOTP(int numberOfPasses)
        {
            List<string> productIds = new List<string>();
            for (int i = 1; i <= numberOfPasses; i++)
            {
                productIds.Add(i.ToString());
            }
            return productIds;
        }

        private string formatAmountForDisplay(string amt, CultureInfo ci, bool roundup = true, bool isAward = false)
        {
            string newAmt = amt;
            decimal amount = 0;
            decimal.TryParse(amt, out amount);

            try
            {
                string currencySymbol = "";

                RegionInfo ri = new RegionInfo(ci.Name);

                switch (ri.ISOCurrencySymbol.ToUpper())
                {
                    case "JPY":
                    case "EUR":
                    case "CAD":
                    case "GBP":
                    case "CNY":
                    case "USD":
                    case "AUD":
                    default:
                        newAmt = GetCurrencySymbol(ci, amount, roundup);
                        break;
                }

            }
            catch { }

            return isAward ? "+ " + newAmt : newAmt;
        }

        private string GetCurrencySymbol(CultureInfo ci, decimal amount, bool roundup)
        {
            string result = string.Empty;

            try
            {
                if (amount > -1)
                {
                    if (roundup)
                    {
                        int newTempAmt = (int)decimal.Ceiling(amount);
                        try
                        {
                            var ri = new RegionInfo(ci.Name);
                            CultureInfo tempCi = Thread.CurrentThread.CurrentCulture;
                            Thread.CurrentThread.CurrentCulture = ci;
                            result = newTempAmt.ToString("c0");
                            Thread.CurrentThread.CurrentCulture = tempCi;
                        }
                        catch { }
                    }
                    else
                    {
                        try
                        {
                            var ri = new RegionInfo(ci.Name);
                            CultureInfo tempCi = Thread.CurrentThread.CurrentCulture;
                            Thread.CurrentThread.CurrentCulture = ci;
                            result = amount.ToString("c");
                            Thread.CurrentThread.CurrentCulture = tempCi;
                        }
                        catch { }
                    }
                }

            }
            catch { }

            return result;
        }

        private bool IsValidForTPI(Reservation bookingPathReservation)
        {
            bool isValid = true;
            if (bookingPathReservation != null && bookingPathReservation.ShopReservationInfo != null && bookingPathReservation.ShopReservationInfo.IsCorporateBooking && !bookingPathReservation.ShopReservationInfo.IsGhostCardValidForTPIPurchase)
            {
                isValid = false;
            }
            return isValid;
        }
        private T ObjectToObjectCasting<T, R>(R request)
        {
            var typeInstance = Activator.CreateInstance(typeof(T));

            foreach (var propReq in request.GetType().GetProperties())
            {
                var propRes = typeInstance.GetType().GetProperty(propReq.Name);
                if (propRes != null)
                {
                    propRes.SetValue(typeInstance, propReq.GetValue(request));
                }
            }

            return (T)typeInstance;
        }

        private async Task<DisplayCartRequest> GetDisplayCartRequest(MOBSHOPProductSearchRequest request, Session session = null)
        {
            DisplayCartRequest displayCartRequest = null;
            if (request != null && !string.IsNullOrEmpty(request.CartId) && request.ProductCodes != null && request.ProductCodes.Count > 0)
            {
                displayCartRequest = new DisplayCartRequest();
                displayCartRequest.CartId = request.CartId;
                displayCartRequest.CountryCode = request.PointOfSale;
                displayCartRequest.LangCode = request.LanguageCode;
                displayCartRequest.ProductCodes = new List<string>();
                displayCartRequest.CartKey = request.CartKey;
                foreach (var productCode in request.ProductCodes)
                {
                    displayCartRequest.ProductCodes.Add(productCode);
                }

                #region TripInsuranceV2
                if (session != null &&
                    await _featureToggles.IsEnabledTripInsuranceV2(request.Application.Id, request.Application.Version.Major, session.CatalogItems).ConfigureAwait(false) &&
                    !session.IsReshopChange &&
                    displayCartRequest.ProductCodes != null &&
                    displayCartRequest.ProductCodes.Count > 0 &&
                    displayCartRequest.ProductCodes.Contains("TPI"))
                {
                    displayCartRequest.Characteristics = new Collection<Service.Presentation.CommonModel.Characteristic>();
                    // new characteristic to get the right information depending on the version
                    var newTPIITem = new Service.Presentation.CommonModel.Characteristic
                    {
                        Code = _configuration.GetValue<string>("TripInsuranceV2_UAMBM02_Code"),
                        Value = _configuration.GetValue<string>("TripInsuranceV2_UAMBM02_Value")
                    };
                    displayCartRequest.Characteristics.Add(newTPIITem);
                }
                #endregion
            }

            return displayCartRequest;
        }

        private async Task<MOBTPIInfo> GetTripInsuranceInfo(MOBSHOPProductSearchRequest request, string token, bool isBookingPath = false, Session session = null)
        {
            MOBTPIInfo tripInsuranceInfo = new MOBTPIInfo();
            bool isPostBookingCall = _configuration.GetValue<bool>("ShowTripInsuranceSwitch");
            // ==>> Venkat and Elise change only one below line of code to offer TPI postbooking when inline TPI is off for new clients 2.1.36 and above
            isPostBookingCall = EnableTPI(request.Application.Id, request.Application.Version.Major, 1);
            if (isPostBookingCall ||
                (Convert.ToBoolean(_configuration.GetValue<string>("ShowTripInsuranceBookingSwitch") ?? "false")
                && isBookingPath)
                )
            {
                DisplayCartRequest displayCartRequest = await GetDisplayCartRequest(request, session).ConfigureAwait(false);
                string jsonRequest = JsonConvert.SerializeObject(displayCartRequest);
                string cslresponse = await _flightShoppingProductsServices.GetProducts(token, request.SessionId, jsonRequest, request.TransactionId);

                if (!(cslresponse == null))
                {
                    DisplayCartResponse response = JsonConvert.DeserializeObject<DisplayCartResponse>(cslresponse);

                    if (response != null && response.MerchProductOffer != null)
                    {
                        var productVendorOffer = new GetVendorOffers();
                        if (!_configuration.GetValue<bool>("DisableMerchProductOfferNullCheck"))
                            productVendorOffer = response.MerchProductOffer != null ? ObjectToObjectCasting<GetVendorOffers, United.Service.Presentation.ProductResponseModel.ProductOffer>(response.MerchProductOffer) : productVendorOffer;
                        else
                            productVendorOffer = ObjectToObjectCasting<GetVendorOffers, United.Service.Presentation.ProductResponseModel.ProductOffer>(response.MerchProductOffer);

                        await _sessionHelperService.SaveSession<United.Persist.Definition.Merchandizing.GetVendorOffers>(productVendorOffer, request.SessionId, new List<string>() { request.SessionId, productVendorOffer.ObjectName }, productVendorOffer.ObjectName).ConfigureAwait(false);

                    }

                    if (response != null && (response.Errors == null || response.Errors.Count == 0) && response.Status.Equals(United.Services.FlightShopping.Common.StatusType.Success) && response.MerchProductOffer != null)
                    {
                        tripInsuranceInfo = await GetTripInsuranceDetails(response.MerchProductOffer, request.Application.Id, request.Application.Version.Major, request.DeviceId, request.SessionId, isBookingPath, session);
                    }
                    else
                    {
                        tripInsuranceInfo = null;
                    }
                }

                if (!isBookingPath)
                {
                    // add presist file 
                    Reservation bookingPathReservation = new Reservation();
                    bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, bookingPathReservation.ObjectName, new List<string> { request.SessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);

                    if (bookingPathReservation.TripInsuranceFile == null)
                    {
                        bookingPathReservation.TripInsuranceFile = new MOBTripInsuranceFile() { };
                    }
                    bookingPathReservation.TripInsuranceFile.TripInsuranceInfo = tripInsuranceInfo;

                   await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, _headers.ContextValues.SessionId, new List<string>() { _headers.ContextValues.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);
                }

            }

            _logger.LogInformation("GetTripInsuranceInfo TripInsuranceInfo {@TripInsuranceInfo}", JsonConvert.SerializeObject(tripInsuranceInfo));

            return tripInsuranceInfo;
        }
        
        private async Task<MOBTPIInfo> GetTPIDetails(Service.Presentation.ProductResponseModel.ProductOffer productOffer, string sessionId, bool isShoppingCall, bool isBookingPath = false, int appid = -1, string appVersion = "", Session session = null)
        {
            if (productOffer?.Offers == null || !(productOffer?.Offers?.Any() ?? false))
            {
                return null;
            }
            MOBTPIInfo tripInsuranceInfo = new MOBTPIInfo();
            var product = productOffer.Offers.FirstOrDefault(a => a.ProductInformation.ProductDetails.Where(b => b.Product != null && b.Product.Code.ToUpper().Trim() == "TPI").ToList().Count > 0).
                ProductInformation.ProductDetails.FirstOrDefault(c => c.Product != null && c.Product.Code.ToUpper().Trim() == "TPI").Product;
            #region // sample code If AIG Dont Offer TPI, the contents and Prices should be null. 
            if (product.SubProducts[0].Prices == null || product.Presentation == null || product.Presentation.Contents == null)
            {
                tripInsuranceInfo = null;
                return tripInsuranceInfo;
            }
            #endregion
            #region Trip Insurance V2
            if (session != null &&
                await _featureToggles.IsEnabledTripInsuranceV2(appid, appVersion, session.CatalogItems).ConfigureAwait(false) &&
                isBookingPath)
            {
                if (product.Presentation.HTML == null)
                {
                    tripInsuranceInfo = null;
                    return tripInsuranceInfo;
                }
                tripInsuranceInfo = await _shoppingCartUtility.GetTPIInfoFromContentV2(product.Presentation, tripInsuranceInfo, sessionId, isShoppingCall, isBookingPath);
            }
            #endregion
            else
            {
                #region mapping content
                tripInsuranceInfo = await GetTPIInfoFromContent(product.Presentation.Contents, tripInsuranceInfo, sessionId, isShoppingCall, isBookingPath, appid);
                #endregion
            }

            if (tripInsuranceInfo != null)
            {
                tripInsuranceInfo.ProductId = GetTPIQuoteId(product.Characteristics);
                // if quoteId is null, we should keep reponse null
                if (!string.IsNullOrEmpty(tripInsuranceInfo.ProductId))
                {
                    tripInsuranceInfo = GetTPIAmountAndFormattedAmount(tripInsuranceInfo, product.SubProducts);
                }
                tripInsuranceInfo.ProductCode = product.Code;
                tripInsuranceInfo.ProductName = product.DisplayName;
            }

            return tripInsuranceInfo;
        }

        private MOBTPIInfo GetTPIAmountAndFormattedAmount(MOBTPIInfo tripInsuranceInfo, System.Collections.ObjectModel.Collection<Service.Presentation.ProductModel.SubProduct> subProducts)
        {
            string currencyCode = string.Empty;
            var prices = subProducts.Where(a => a.Prices != null && a.Prices.Count > 0).FirstOrDefault().Prices;
            foreach (var price in prices)
            {
                if (price != null && price.PaymentOptions != null && price.PaymentOptions.Count > 0)
                {
                    foreach (var option in price.PaymentOptions)
                    {
                        if (option != null && option.Type != null && option.Type.ToUpper().Contains("TOTALPRICE"))
                        {
                            foreach (var PriceComponent in option.PriceComponents)
                            {
                                foreach (var total in PriceComponent.Price.Totals)
                                {
                                    tripInsuranceInfo.Amount = total.Amount;
                                    currencyCode = total.Currency.Code.ToUpper().Trim();
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            // concate currency sign and round up amount //removed
            //tripInsuranceInfo.FormattedDisplayAmount = AttachCurrencySymbol(tripInsuranceInfo.Amount, currencyCode, true);
            // real amount concat with currency sign
            tripInsuranceInfo.DisplayAmount = AttachCurrencySymbol(tripInsuranceInfo.Amount, currencyCode, false);
            if (tripInsuranceInfo.Amount <= 0)
            {
                tripInsuranceInfo = null;
            }

            return tripInsuranceInfo;
        }

        private string AttachCurrencySymbol(double amount, string currencyCode, bool isRoundUp)
        {
            string formattedDisplayAmount = string.Empty;
            CultureInfo ci = GetCultureInfo(currencyCode);
            formattedDisplayAmount = formatAmountForDisplay(string.Format("{0:#,0.00}", amount), ci, isRoundUp);

            return formattedDisplayAmount;
        }

        private async Task<MOBTPIInfo> GetTPIInfoFromContent(System.Collections.ObjectModel.Collection<United.Service.Presentation.ProductModel.PresentationContent> contents, MOBTPIInfo tripInsuranceInfo, string sessionId, bool isShoppingCall, bool isBookingPath = false, int appId = -1)
        {
            string tncPaymentText1 = string.Empty;
            string tncPaymentText2 = string.Empty;
            string tncPaymentText3 = string.Empty;
            string tncPaymentLinkMessage = string.Empty;
            string tncProductPageText1 = string.Empty;
            string tncProductPageText2 = string.Empty;
            string tncProductPageLinkMessage = string.Empty;
            string confirmationResponseDetailMessage1 = string.Empty;
            string confirmationResponseDetailMessage2 = string.Empty;
            string contentInBooking1 = string.Empty;
            string contentInBooking2 = string.Empty;
            string contentInBooking3 = string.Empty;
            string header1 = string.Empty;
            string header2 = string.Empty;
            string legalInfo = string.Empty;
            string legalInfoText = string.Empty;
            string bookingImg = string.Empty;
            string bookingTncContentMsg = string.Empty;
            string bookingTncLinkMsg = string.Empty;
            string bookingLegalInfoContentMsg = string.Empty;
            string mobTgiLimitationMessage = string.Empty;
            string mobTgiReadMessage = string.Empty;
            string mobTgiAndMessage = string.Empty;
            // Covid 19 Emergency WHO TPI content
            string mobTIMBemergencyMessage = string.Empty;
            string mobTIMBemergencyMessageUrltext = string.Empty;
            string mobTIMBemergencyMessagelinkUrl = string.Empty;
            string mobTIMREmergencyMessage = string.Empty;
            string mobTIMREmergencyMessageUrltext = string.Empty;
            string mobTIMREmergencyMessagelinkUrl = string.Empty;
            string mobTIMBWashingtonMessage = string.Empty;


            foreach (var content in contents)
            {
                switch (content.Header.ToUpper().Trim())
                {
                    case "MOBOFFERHEADERMESSAGE":
                        tripInsuranceInfo.Title1 = content.Body.Trim();
                        break;
                    case "MOBOFFERTITLEMESSAGE":
                        tripInsuranceInfo.QuoteTitle = content.Body.Trim();
                        break;
                    case "MOBTRIPCOVEREDPRICEMESSAGE":
                        tripInsuranceInfo.CoverCost = content.Body.Trim();
                        break;
                    case "MOBOFFERFROMTMESSAGE":
                        tripInsuranceInfo.Title2 = content.Body.Trim();
                        break;
                    case "MOBOFFERIMAGE":
                        tripInsuranceInfo.Image = content.Body.Trim();
                        //tripInsuranceInfo.TileImage = tripInsuranceInfo.Image;
                        break;
                    case "PREBOOKINGMOBOFFERHEADERMESSAGE":
                        tripInsuranceInfo.TileTitle1 = content.Body.Trim();
                        break;
                    case "PREBOOKINGMOBOFFERTITLEMESSAGE":
                        tripInsuranceInfo.TileQuoteTitle = content.Body.Trim();
                        break;
                    case "PREBOOKINGMOBOFFERFROMTMESSAGE":
                        tripInsuranceInfo.TileTitle2 = content.Body.Trim();
                        break;
                    case "PREBOOKINGMOBOFFERCTAMESSAGE":
                        tripInsuranceInfo.TileLinkText = content.Body.Trim();
                        break;
                    case "MOBTGIVIEWHEADERMESSAGE":
                        tripInsuranceInfo.Headline1 = content.Body.Trim();
                        break;
                    case "MOBTIDETAILSTOTALCOVERCOSTMESSAGE":
                        tripInsuranceInfo.LineItemText = content.Body.Trim();
                        break;
                    case "MOBPAYMENTTANDCHEADER1MESSAGE": //By clicking on purchase I acknowledge that I have read and understand the
                        tncPaymentText1 = content.Body.Trim();
                        break;
                    case "MOBPAYMENTTANDCURLHEADERMESSAGE": //Certificate of Insurance
                        tncPaymentText2 = content.Body.Trim();
                        break;
                    case "MOBPAYMENTTANDCURLHEADER2MESSAGE": //, and agree to the terms and conditions of the insurance coverage provided.
                        tncPaymentText3 = content.Body.Trim();
                        break;
                    case "MOBPAYMENTTANDCBODYURLMESSAGE":
                        tncPaymentLinkMessage = content.Body.Trim();
                        break;
                    case "MOBTIDETAILSTANDCHEADER1MESSAGE": // Coverage is offered by Travel Guard Group, Inc. and limitations will apply;
                        tncProductPageText1 = content.Body.Trim();
                        break;
                    case "MOBTIDETAILSTANDCURLHEADERMESSAGE": // view details.
                        tncProductPageText2 = content.Body.Trim();
                        break;
                    case "MOBTIDETAILSTANDCURLMESSAGE":
                        tncProductPageLinkMessage = content.Body.Trim();
                        break;
                    case "MOBTGIVIEWBODY1MESSAGE": // Are you prepared?
                        tripInsuranceInfo.Body1 = content.Body.Trim();
                        break;
                    case "MOBTGIVIEWBODY2MESSAGE": // For millions of travelers every year...
                        tripInsuranceInfo.Body2 = content.Body.Trim();
                        break;
                    // used in payment confirmation page 
                    case "MOBTICONFIRMATIONBODY1MESSAGE":
                        confirmationResponseDetailMessage1 = content.Body.Trim();
                        break;
                    case "MOBTICONFIRMATIONBODY2MESSAGE":
                        confirmationResponseDetailMessage2 = content.Body.Trim();
                        break;
                    // used in booking path 
                    case "PREBOOKINGMOBOFFERIMAGE":
                        bookingImg = content.Body.Trim();
                        tripInsuranceInfo.TileImage = bookingImg;
                        break;
                    case "PREBOOKINGMOBTIDETAILSTANDCURLHEADERMESSAGE":
                        bookingTncContentMsg = content.Body.Trim();
                        break;
                    case "PREBOOKINGMOBTIDETAILSTANDCURLMESSAGE":
                        bookingTncLinkMsg = content.Body.Trim();
                        break;
                    case "PREBOOKINGMOBPAYMENTTANDCHEADER1MESSAGE":
                        bookingLegalInfoContentMsg = content.Body.Trim();
                        break;
                    case "PREBOOKINGMOBBOOKINGPRODPAGEHEADLINE1":
                        header1 = content.Body.Trim();
                        break;
                    case "PREBOOKINGMOBBOOKINGPRODPAGEHEADLINE2":
                        header2 = content.Body.Trim();
                        break;
                    case "PREBOOKINGMOBBOOKINGPRODPAGECONTENT1":
                        contentInBooking1 = content.Body.Trim();
                        break;
                    case "PREBOOKINGMOBBOOKINGPRODPAGECONTENT2":
                        contentInBooking2 = content.Body.Trim();
                        break;
                    case "PREBOOKINGMOBBOOKINGPRODPAGECONTENT3":
                        contentInBooking3 = content.Body.Trim();
                        break;
                    case "PREBOOKINGMOBBOOKINGPRODPAGEBOTTOMLINE":
                        legalInfo = content.Body.Trim();
                        break;
                    case "MOBTGILIMITATIONMESSAGE":
                        mobTgiLimitationMessage = content.Body.Trim();
                        break;
                    case "MOBTGIREADMESSAGE":
                        mobTgiReadMessage = content.Body.Trim();
                        break;
                    case "MOBTGIANDMESSAGE":
                        mobTgiAndMessage = content.Body.Trim();
                        break;
                    // Covid 19 Emergency WHO TPI content
                    case "MOBTIMBEMERGENCYMESSAGETEXT":
                        mobTIMBemergencyMessage = content != null ? content.Body.Trim() : string.Empty;
                        break;
                    case "MOBTIMBEMERGENCYMESSAGELINKTEXT":
                        mobTIMBemergencyMessageUrltext = content != null ? content.Body.Trim() : string.Empty;
                        break;
                    case "MOBTIMBEMERGENCYMESSAGELINKURL":
                        mobTIMBemergencyMessagelinkUrl = content != null ? content.Body.Trim() : string.Empty;
                        break;
                    case "MOBTIMREMERGENCYMESSAGETEXT":
                        mobTIMREmergencyMessage = content != null ? content.Body.Trim() : string.Empty;
                        break;
                    case "MOBTIMREMERGENCYMESSAGELINKTEXT":
                        mobTIMREmergencyMessageUrltext = content != null ? content.Body.Trim() : string.Empty;
                        break;
                    case "MOBTIMREMERGENCYMESSAGELINKURL":
                        mobTIMREmergencyMessagelinkUrl = content != null ? content.Body.Trim() : string.Empty;
                        break;
                    case "MOBTIMBWASHINGTONMESSAGE":
                        mobTIMBWashingtonMessage = content != null && !string.IsNullOrEmpty(content.Body) ? content.Body.Trim() : string.Empty;
                        break;
                    default:
                        break;
                }
            }
            //Covid 19 Emergency WHO TPI content
            if (_configuration.GetValue<bool>("ToggleCovidEmergencytextTPI") == true)
            {
                if (tripInsuranceInfo != null &&
                    !string.IsNullOrEmpty(mobTIMBemergencyMessage) && !string.IsNullOrEmpty(mobTIMBemergencyMessageUrltext)
                    && !string.IsNullOrEmpty(mobTIMBemergencyMessagelinkUrl))
                {
                    MOBItem tpiContentMessage = new MOBItem();
                    tpiContentMessage.Id = "COVID19EmergencyAlert";
                    tpiContentMessage.CurrentValue = mobTIMBemergencyMessage +
                        " <a href =\"" + mobTIMBemergencyMessagelinkUrl + "\" target=\"_blank\">" + mobTIMBemergencyMessageUrltext + "</a> ";
                    tripInsuranceInfo.TPIAIGReturnedMessageContentList = new List<MOBItem>();
                    tripInsuranceInfo.TPIAIGReturnedMessageContentList.Add(tpiContentMessage);
                }
                if (tripInsuranceInfo != null &&
                    !string.IsNullOrEmpty(mobTIMREmergencyMessage) && !string.IsNullOrEmpty(mobTIMREmergencyMessageUrltext)
                    && !string.IsNullOrEmpty(mobTIMREmergencyMessagelinkUrl))
                {
                    MOBItem tpiContentMessage = new MOBItem();
                    tpiContentMessage.Id = "COVID19EmergencyAlertManageRes";
                    tpiContentMessage.CurrentValue = mobTIMREmergencyMessage +
                        " <a href =\"" + mobTIMREmergencyMessagelinkUrl + "\" target=\"_blank\">" + mobTIMREmergencyMessageUrltext + "</a> ";
                    tripInsuranceInfo.TPIAIGReturnedMessageContentList = new List<MOBItem>();
                    tripInsuranceInfo.TPIAIGReturnedMessageContentList.Add(tpiContentMessage);
                }
            }

            string isNewTPIMessageHTML = appId == 2 ? "<br/><br/>" : "<br/>";
            string specialCharacter = _configuration.GetValue<string>("TPIinfo-SpecialCharacter") ?? "";
            if (tripInsuranceInfo != null && !string.IsNullOrEmpty(tripInsuranceInfo.Image) && !string.IsNullOrEmpty(tncProductPageLinkMessage) &&
                !string.IsNullOrEmpty(tncPaymentLinkMessage) &&
                tripInsuranceInfo.QuoteTitle != null && tripInsuranceInfo.QuoteTitle.Contains("(R)") && !isBookingPath)
            {

                tripInsuranceInfo.Body3 = (mobTgiLimitationMessage.IsNullOrEmpty() && mobTgiReadMessage.IsNullOrEmpty()) ?
                                          (tncProductPageText1 + " <a href =\"" + tncProductPageLinkMessage + "\" target=\"_blank\">" + tncProductPageText2 + "</a>")
                                          : (tncProductPageText1 + " <a href =\"" + tncPaymentLinkMessage + "\" target=\"_blank\">" + mobTgiLimitationMessage + "</a> " + mobTgiReadMessage + " <a href =\"" + tncProductPageLinkMessage + "\" target=\"_blank\">" + tncProductPageText2 + "</a>");
                tripInsuranceInfo.Body3 = !_configuration.GetValue<bool>("IsDisableTPIForWashington") && !string.IsNullOrEmpty(tripInsuranceInfo.Body3) && !string.IsNullOrEmpty(mobTIMBWashingtonMessage) ? tripInsuranceInfo.Body3 + isNewTPIMessageHTML + mobTIMBWashingtonMessage : tripInsuranceInfo.Body3;
                tripInsuranceInfo.TNC = tncPaymentText1 + tncPaymentText3 + " <a href =\"" + tncPaymentLinkMessage + "\" target=\"_blank\">" + tncPaymentText2 + "</a> " + mobTgiAndMessage +
                    " <a href =\"" + tncProductPageLinkMessage + "\" target=\"_blank\">" + tncProductPageText2 + "</a> ";
                tripInsuranceInfo.QuoteTitle = @tripInsuranceInfo.QuoteTitle.Replace("(R)", specialCharacter);
                tripInsuranceInfo.PageTitle = _configuration.GetValue<string>("TPIinfo-PageTitle") ?? "";
                tripInsuranceInfo.Headline2 = _configuration.GetValue<string>("TPIinfo-Headline2") ?? "";
                tripInsuranceInfo.PaymentContent = _configuration.GetValue<string>("TPIinfo-PaymentContent") ?? "";
                // confirmation page use
                if (isShoppingCall)
                {
                    Reservation bookingPathReservation = new Reservation();
                    bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(sessionId, bookingPathReservation.ObjectName, new List<string> { sessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);
                    if (bookingPathReservation == null)
                    {
                        bookingPathReservation = new Reservation();
                    }
                    if (bookingPathReservation.TripInsuranceFile == null)
                    {
                        bookingPathReservation.TripInsuranceFile = new MOBTripInsuranceFile() { };
                    }
                    bookingPathReservation.TripInsuranceFile.ConfirmationResponseDetailMessage1 = @confirmationResponseDetailMessage1.Replace("(R)", specialCharacter);
                    bookingPathReservation.TripInsuranceFile.ConfirmationResponseDetailMessage2 = confirmationResponseDetailMessage2;

                    await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, sessionId, new List<string> { sessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);
                }
                else
                {
                    tripInsuranceInfo.Confirmation1 = @confirmationResponseDetailMessage1.Replace("(R)", specialCharacter);
                    tripInsuranceInfo.Confirmation2 = confirmationResponseDetailMessage2;
                }
            }
            else if (isShoppingCall && isBookingPath && !string.IsNullOrEmpty(contentInBooking1) && !string.IsNullOrEmpty(contentInBooking2) && !string.IsNullOrEmpty(contentInBooking3)
                        && !string.IsNullOrEmpty(header1) && !string.IsNullOrEmpty(header2) && !string.IsNullOrEmpty(tncProductPageText1)
                        && !string.IsNullOrEmpty(bookingTncLinkMsg) && !string.IsNullOrEmpty(bookingTncContentMsg) && !string.IsNullOrEmpty(bookingLegalInfoContentMsg)
                        && !string.IsNullOrEmpty(tncPaymentLinkMessage) && !string.IsNullOrEmpty(tncPaymentText2) && !string.IsNullOrEmpty(tncPaymentText3) && !string.IsNullOrEmpty(bookingImg)
                        && !string.IsNullOrEmpty(tripInsuranceInfo.CoverCost) && !string.IsNullOrEmpty(tncPaymentText1)
                        && !string.IsNullOrEmpty(confirmationResponseDetailMessage1)
                        && !string.IsNullOrEmpty(confirmationResponseDetailMessage2))
            {
                tripInsuranceInfo.Body3 = (mobTgiLimitationMessage.IsNullOrEmpty() && mobTgiReadMessage.IsNullOrEmpty()) ?
                                          tncProductPageText1 + " <a href =\"" + bookingTncLinkMsg + "\" target=\"_blank\">" + bookingTncContentMsg + "</a>"
                                        : (tncProductPageText1 + " <a href =\"" + tncPaymentLinkMessage + "\" target=\"_blank\">" + mobTgiLimitationMessage + "</a> " + mobTgiReadMessage + " <a href =\"" + bookingTncLinkMsg + "\" target=\"_blank\">" + bookingTncContentMsg + "</a>");
                tripInsuranceInfo.Body3 = !_configuration.GetValue<bool>("IsDisableTPIForWashington") && !string.IsNullOrEmpty(tripInsuranceInfo.Body3) && !string.IsNullOrEmpty(mobTIMBWashingtonMessage) ? tripInsuranceInfo.Body3 + isNewTPIMessageHTML + mobTIMBWashingtonMessage : tripInsuranceInfo.Body3;
                tripInsuranceInfo.TNC = (mobTgiLimitationMessage.IsNullOrEmpty() && mobTgiReadMessage.IsNullOrEmpty()) ? bookingLegalInfoContentMsg + " <a href =\"" + tncPaymentLinkMessage + "\" target=\"_blank\">" + tncPaymentText2 + "</a> " + tncPaymentText3
                                        : bookingLegalInfoContentMsg + " " + tncPaymentText3 + " <a href =\"" + tncPaymentLinkMessage + "\" target=\"_blank\">" + tncPaymentText2 + "</a> " + mobTgiAndMessage + " <a href =\"" + bookingTncLinkMsg + "\" target=\"_blank\">" + bookingTncContentMsg + "</a>";

                tripInsuranceInfo.PageTitle = _configuration.GetValue<string>("TPIinfo-PageTitle") ?? "";
                tripInsuranceInfo.Image = bookingImg;
                Reservation bookingPathReservation = new Reservation();

                bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(sessionId, bookingPathReservation.ObjectName, new List<string> { sessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);

                if (bookingPathReservation == null)
                {
                    bookingPathReservation = new Reservation();
                }
                if (bookingPathReservation.TripInsuranceFile == null)
                {
                    bookingPathReservation.TripInsuranceFile = new MOBTripInsuranceFile() { };
                }
                if (bookingPathReservation.TripInsuranceFile.TripInsuranceBookingInfo == null)
                {
                    bookingPathReservation.TripInsuranceFile.TripInsuranceBookingInfo = new MOBTPIInfoInBookingPath() { };
                }
                List<string> contentInBooking = new List<string>();
                contentInBooking.Add(contentInBooking1);
                contentInBooking.Add(contentInBooking2);
                contentInBooking.Add(contentInBooking3);
                bookingPathReservation.TripInsuranceFile.TripInsuranceBookingInfo.Content = contentInBooking;
                bookingPathReservation.TripInsuranceFile.TripInsuranceBookingInfo.Header = header1 + " <b>" + header2 + "</b>";
                bookingPathReservation.TripInsuranceFile.TripInsuranceBookingInfo.LegalInformation = legalInfo;
                bookingPathReservation.TripInsuranceFile.TripInsuranceBookingInfo.TncSecondaryFOPPage = (mobTgiLimitationMessage.IsNullOrEmpty() && mobTgiReadMessage.IsNullOrEmpty()) ? tncPaymentText1 + " <a href =\"" + tncPaymentLinkMessage + "\" target=\"_blank\">" + tncPaymentText2 + "</a> " + tncPaymentText3
                : tncPaymentText1 + " " + tncPaymentText3 + " <a href =\"" + tncPaymentLinkMessage + "\" target=\"_blank\">" + tncPaymentText2 + "</a> " + mobTgiAndMessage + " <a href =\"" + tncProductPageLinkMessage + "\" target=\"_blank\">" + tncProductPageText2 + "</a> ";
                bookingPathReservation.TripInsuranceFile.TripInsuranceBookingInfo.ConfirmationMsg = @confirmationResponseDetailMessage1.Replace("(R)", specialCharacter) + "\n\n" + confirmationResponseDetailMessage2;

                await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, sessionId, new List<string> { sessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);
            }
            else
            {
                tripInsuranceInfo = null;
            }

            return tripInsuranceInfo;
        }

        private string GetTPIQuoteId(System.Collections.ObjectModel.Collection<Service.Presentation.CommonModel.Characteristic> characteristics)
        {
            string productId = string.Empty;
            productId = characteristics.FirstOrDefault(a => !string.IsNullOrEmpty(a.Code) && a.Code.ToUpper().Trim() == "QUOTEPACKID").Value.Trim();

            return productId;
        }

        private async Task<MOBTPIInfo> GetTripInsuranceDetails(Service.Presentation.ProductResponseModel.ProductOffer offer, int applicationId, string applicationVersion, string deviceID, string sessionId, bool isBookingPath = false, Session session = null)
        {
            MOBTPIInfo tripInsuranceInfo = new MOBTPIInfo();
            try
            {
                tripInsuranceInfo = await GetTPIDetails(offer, sessionId, true, isBookingPath, applicationId, applicationVersion, session);
            }
            catch (Exception ex)
            {
                tripInsuranceInfo = null;
                _logger.LogError("GetTripInsuranceDetails Exception {@Exception}",JsonConvert.SerializeObject(ex));
            }
            if (tripInsuranceInfo == null && Convert.ToBoolean(_configuration.GetValue<string>("Log_TI_Offer_If_AIG_NotOffered_At_BookingPath") ?? "false"))
            {
                _logger.LogWarning("GetTripInsuranceDetails UnitedException AIG Not Offered Trip Insuracne in Booking Path");
            }

            _logger.LogInformation("GetTripInsuranceDetails TripInsuranceInfo {@TripInsuranceInfo}", JsonConvert.SerializeObject(tripInsuranceInfo));

            return tripInsuranceInfo;
        }

        private MOBTPIInfoInBookingPath AssignBookingPathTPIInfo(MOBTPIInfo tripInsuranceInfo, MOBTPIInfoInBookingPath tripInsuranceBookingInfo)
        {
            MOBTPIInfoInBookingPath tPIInfo = new MOBTPIInfoInBookingPath() { };
            tPIInfo.Amount = tripInsuranceInfo.Amount;
            tPIInfo.ButtonTextInProdPage = Convert.ToString(_configuration.GetValue<string>("TPIinfo_BookingPath_PRODBtnText_BeforeRegister") ?? "") + tripInsuranceInfo.DisplayAmount;
            tPIInfo.Title = tripInsuranceInfo.PageTitle;
            tPIInfo.CoverCostText = _configuration.GetValue<string>("TPIinfo_BookingPath_CoverCostTest") ?? "";
            tPIInfo.CoverCost = Convert.ToString(_configuration.GetValue<string>("TPIinfo_BookingPath_CoverCost") ?? "") + "<b>" + tripInsuranceInfo.CoverCost + "</b>";
            tPIInfo.Img = tripInsuranceInfo.Image;
            tPIInfo.IsRegistered = false;
            tPIInfo.QuoteId = tripInsuranceInfo.ProductId;
            tPIInfo.Tnc = tripInsuranceInfo.Body3;
            tPIInfo.Content = tripInsuranceBookingInfo?.Content;
            tPIInfo.Header = tripInsuranceBookingInfo?.Header;
            tPIInfo.LegalInformation = tripInsuranceBookingInfo?.LegalInformation;
            tPIInfo.LegalInformationText = tripInsuranceInfo.TNC;
            tPIInfo.TncSecondaryFOPPage = tripInsuranceBookingInfo?.TncSecondaryFOPPage;
            tPIInfo.DisplayAmount = tripInsuranceInfo.DisplayAmount;
            tPIInfo.ConfirmationMsg = tripInsuranceBookingInfo?.ConfirmationMsg;
            if (_configuration.GetValue<bool>("EnableTravelInsuranceOptimization"))
            {
                tPIInfo.TileTitle1 = tripInsuranceInfo.TileTitle1;
                tPIInfo.TileTitle2 = tripInsuranceInfo.TileTitle2;
                tPIInfo.TileImage = tripInsuranceInfo.TileImage;
                tPIInfo.TileLinkText = _configuration.GetValue<string>("TPITileLinkText");
            }
            //Covid 19 Emergency WHO TPI content
            if (_configuration.GetValue<bool>("ToggleCovidEmergencytextTPI") == true)
            {
                tPIInfo.TPIAIGReturnedMessageContentList = tripInsuranceInfo.TPIAIGReturnedMessageContentList;
            }

            if (!string.IsNullOrEmpty(tripInsuranceInfo.HtmlContentV2))
            {
                tPIInfo.HtmlContentV2 = tripInsuranceInfo.HtmlContentV2;
            }
            return tPIInfo;
        }

        private string GetBookingPaymentTargetForRegisterFop(FlightReservationResponse flightReservationResponse)
        {
            if (flightReservationResponse.ShoppingCart == null || !flightReservationResponse.ShoppingCart.Items.Any())
                return string.Empty;

            return string.Join(",", flightReservationResponse.ShoppingCart.Items.SelectMany(x => x.Product).Select(x => x.Code).Distinct());
        }

        private async Task<FlightReservationResponse> RegisterOffers(MOBRegisterOfferRequest request, United.Service.Presentation.ProductResponseModel.ProductOffer productOffer, United.Service.Presentation.ProductResponseModel.ProductOffer productVendorOffer, United.Service.Presentation.ReservationResponseModel.ReservationDetail reservationDetail, Session session)
        {
            var response = new FlightReservationResponse();
            response =await RegisterOffers(request, productOffer, productVendorOffer, reservationDetail, session, null);
            return response;
        }

        private string GetPaymentTargetForRegisterFop(TravelOptionsCollection travelOptions, bool isCompleteFarelockPurchase = false)
        {
            if (string.IsNullOrEmpty(_configuration.GetValue<string>("EnablePCUSelectedSeatPurchaseViewRes")))
                return string.Empty;

            if (isCompleteFarelockPurchase)
                return "RES";

            if (travelOptions == null || !travelOptions.Any())
                return string.Empty;

            return string.Join(",", travelOptions.Select(x => x.Type == "SEATASSIGNMENTS" ? x.Type : x.Key).Distinct());
        }

        private async Task<MOBTPIInfoInBookingPath> GetBookingPathTPIInfo(string sessionId, string languageCode, MOBApplication application, string deviceId, string cartId, string token, bool isRequote, bool isRegisterTraveler, bool isReshop)
        {
            MOBTPIInfoInBookingPath tPIInfo = new MOBTPIInfoInBookingPath();
            MOBTPIInfo tripInsuranceInfo = new MOBTPIInfo();
            MOBSHOPProductSearchRequest getTripInsuranceRequest = new MOBSHOPProductSearchRequest();
            getTripInsuranceRequest.SessionId = sessionId;
            getTripInsuranceRequest.LanguageCode = languageCode;
            getTripInsuranceRequest.Application = application;
            getTripInsuranceRequest.DeviceId = deviceId;
            getTripInsuranceRequest.CartId = cartId;
            getTripInsuranceRequest.PointOfSale = "US";
            getTripInsuranceRequest.CartKey = "TPI";
            getTripInsuranceRequest.ProductCodes = new List<string>() { "TPI" };

            MOBRegisterOfferRequest registerOffersRequest = new MOBRegisterOfferRequest()
            {
                SessionId = getTripInsuranceRequest.SessionId,
                CartKey = getTripInsuranceRequest.CartKey,
                CartId = getTripInsuranceRequest.CartId,
                Flow = FlowType.BOOKING.ToString(),
                Application = getTripInsuranceRequest.Application,
                PointOfSale = getTripInsuranceRequest.PointOfSale,
                MerchandizingOfferDetails = new Collection<MerchandizingOfferDetails>() { new MerchandizingOfferDetails() { ProductCode = getTripInsuranceRequest.ProductCodes[0].ToString() } }
            };

            Session session = new Session();
            if (!string.IsNullOrEmpty(sessionId))
            {
                session = await _sessionHelperService.GetSession<Session>(sessionId, session.ObjectName, new List<string> { sessionId, session.ObjectName }).ConfigureAwait(false);
            }

            Reservation bookingPathReservation = new Reservation();
            bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(sessionId, bookingPathReservation.ObjectName, new List<string> { sessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);
            // if it is concur account with non valid fop for ghost card, we dont show TPI to them
            if (IsValidForTPI(bookingPathReservation))
            {
                MOBTPIInfoInBookingPath persistregisteredTPIInfo = new MOBTPIInfoInBookingPath() { };
                if (bookingPathReservation != null && bookingPathReservation.TripInsuranceFile != null && bookingPathReservation.TripInsuranceFile.TripInsuranceBookingInfo != null && bookingPathReservation.TripInsuranceFile.TripInsuranceBookingInfo.IsRegistered)
                {
                    persistregisteredTPIInfo = bookingPathReservation.TripInsuranceFile.TripInsuranceBookingInfo;

                }
                else
                {
                    persistregisteredTPIInfo = null;
                }
                getTripInsuranceRequest.PointOfSale = !string.IsNullOrEmpty(bookingPathReservation.PointOfSale) ? bookingPathReservation.PointOfSale : "US";
                tripInsuranceInfo = await GetTripInsuranceInfo(getTripInsuranceRequest, token, true, session);

                bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(sessionId, bookingPathReservation.ObjectName, new List<string> { sessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);
                if (bookingPathReservation.TripInsuranceFile == null || tripInsuranceInfo == null)
                {
                    bookingPathReservation.TripInsuranceFile = new MOBTripInsuranceFile() { };
                    tPIInfo = null;
                }
                else
                {
                    tPIInfo = AssignBookingPathTPIInfo(tripInsuranceInfo, bookingPathReservation.TripInsuranceFile.TripInsuranceBookingInfo);
                }
                string cartKey = "TPI";
                string productCode = "TPI";
                string productId = string.Empty;
                List<string> productIds = new List<string>() { };
                string subProductCode = string.Empty;
                bool delete = true;
                if (persistregisteredTPIInfo != null && persistregisteredTPIInfo.IsRegistered && isRequote)
                {
                    //requote
                    if (isRegisterTraveler || (tPIInfo == null || (tPIInfo != null && tPIInfo.Amount == 0)))
                    {
                        // unregister old TPI, update price
                        registerOffersRequest.MerchandizingOfferDetails[0].IsOfferRegistered = true;
                        registerOffersRequest.MerchandizingOfferDetails[0].ProductIds.Add(persistregisteredTPIInfo.QuoteId);
                        var flightReservationResponse = await RegisterOffers(registerOffersRequest, null, null, null, session);
                        var travelOptions = GetTravelBundleOptions(flightReservationResponse.DisplayCart, application.Id, application.Version.Major, isReshop);
                        if (flightReservationResponse != null && flightReservationResponse.Errors.Count() == 0)
                        {
                            MOBShoppingCart persistShoppingCart = new MOBShoppingCart();
                            persistShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(registerOffersRequest.SessionId, persistShoppingCart.ObjectName, new List<string> { registerOffersRequest.SessionId, persistShoppingCart.ObjectName }).ConfigureAwait(false);

                            if (persistShoppingCart == null)
                                persistShoppingCart = new MOBShoppingCart();

                            persistShoppingCart.Products = await _shoppingCartUtility.ConfirmationPageProductInfo(flightReservationResponse, false, false, null, application);
                            persistShoppingCart.CartId = registerOffersRequest.CartId;
                            double price = GetGrandTotalPriceForShoppingCart(false, flightReservationResponse, false, registerOffersRequest.Flow);
                            persistShoppingCart.TotalPrice = String.Format("{0:0.00}", price);
                            persistShoppingCart.DisplayTotalPrice = Decimal.Parse(price.ToString()).ToString("c", new CultureInfo("en-us"));// string.Format("${0:c}", Decimal.Parse(price.ToString())); //Decimal.Parse(price.ToString()).ToString("c");
                            persistShoppingCart.TermsAndConditions = await GetProductBasedTermAndConditions(null, flightReservationResponse, false);
                            persistShoppingCart.PaymentTarget = (registerOffersRequest.Flow == FlowType.BOOKING.ToString()) ? GetBookingPaymentTargetForRegisterFop(flightReservationResponse) : GetPaymentTargetForRegisterFop(flightReservationResponse.DisplayCart.TravelOptions);
                            await _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, registerOffersRequest.SessionId, new List<string> { registerOffersRequest.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        // ancillary change
                        if (tPIInfo != null)
                        {
                            // unregister the old one and register new one
                            if (persistregisteredTPIInfo.Amount == tPIInfo.Amount)
                            {
                                registerOffersRequest.MerchandizingOfferDetails[0].IsOfferRegistered = true;
                                registerOffersRequest.MerchandizingOfferDetails[0].ProductIds.Add(persistregisteredTPIInfo.QuoteId);
                                var flightReservationResponse = await RegisterOffers(registerOffersRequest, null, null, null, session);
                                var travelOptions = GetTravelBundleOptions(flightReservationResponse.DisplayCart, application.Id, application.Version.Major, isReshop);

                                registerOffersRequest.MerchandizingOfferDetails[0].IsOfferRegistered = false;
                                registerOffersRequest.MerchandizingOfferDetails[0].ProductIds = new List<string>();
                                registerOffersRequest.MerchandizingOfferDetails[0].ProductIds.Add(tPIInfo.QuoteId);
                                flightReservationResponse = await RegisterOffers(registerOffersRequest, null, null, null, session);
                                travelOptions = GetTravelBundleOptions(flightReservationResponse.DisplayCart, application.Id, application.Version.Major, isReshop);
                                tPIInfo.ButtonTextInProdPage = _configuration.GetValue<string>("TPIinfo_BookingPath_PRODBtnText_AfterRegister");
                                tPIInfo.CoverCostStatus = _configuration.GetValue<string>("TPIinfo_BookingPath_CoverCostStatus");
                                tPIInfo.IsRegistered = true;
                                if (flightReservationResponse != null && flightReservationResponse.Errors.Count() == 0)
                                {
                                    MOBShoppingCart persistShoppingCart = new MOBShoppingCart();
                                    persistShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(registerOffersRequest.SessionId, persistShoppingCart.ObjectName, new List<string> { registerOffersRequest.SessionId, persistShoppingCart.ObjectName }).ConfigureAwait(false);

                                    if (persistShoppingCart == null)
                                        persistShoppingCart = new MOBShoppingCart();
                                    persistShoppingCart.Products = await _shoppingCartUtility.ConfirmationPageProductInfo(flightReservationResponse, false, false, FlowType.BOOKING.ToString(), application);
                                    persistShoppingCart.CartId = registerOffersRequest.CartId;
                                    double price = GetGrandTotalPriceForShoppingCart(false, flightReservationResponse, false, registerOffersRequest.Flow);
                                    persistShoppingCart.TotalPrice = String.Format("{0:0.00}", price);
                                    persistShoppingCart.DisplayTotalPrice = Decimal.Parse(price.ToString()).ToString("c", new CultureInfo("en-us")); //string.Format("${0:c}", Decimal.Parse(price.ToString())); //Decimal.Parse(price.ToString()).ToString("c");
                                    persistShoppingCart.TermsAndConditions = await GetProductBasedTermAndConditions(null, flightReservationResponse, false);
                                    persistShoppingCart.PaymentTarget = (registerOffersRequest.Flow == FlowType.BOOKING.ToString()) ? GetBookingPaymentTargetForRegisterFop(flightReservationResponse) : GetPaymentTargetForRegisterFop(flightReservationResponse.DisplayCart.TravelOptions);
                                    await _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, registerOffersRequest.SessionId, new List<string> { registerOffersRequest.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName).ConfigureAwait(false);
                                }
                            }
                            // send pop up message
                            else
                            {
                                tPIInfo.OldAmount = persistregisteredTPIInfo.Amount;
                                tPIInfo.OldQuoteId = persistregisteredTPIInfo.QuoteId;
                                CultureInfo ci = GetCultureInfo("en-US");
                                string oldPrice = formatAmountForDisplay(string.Format("{0:#,0.00}", tPIInfo.OldAmount), ci, false);
                                string newPrice = formatAmountForDisplay(string.Format("{0:#,0.00}", tPIInfo.Amount), ci, false);
                                tPIInfo.PopUpMessage = string.Format(_configuration.GetValue<string>("TPIinfo_BookingPath_PopUpMessage"), oldPrice, newPrice);
                                // in the background of RTI page, trip insurance considered as added. Dont remove TPI from prices list and keep the isRegistered equal to true until user make any choices. 
                                tPIInfo.IsRegistered = true;
                            }
                        }
                        else
                        {
                            // if trip insurance not shown after requote, remove price from prices list
                            bookingPathReservation.Prices = UpdatePriceForUnRegisterTPI(bookingPathReservation.Prices);
                        }
                    }
                }
                bookingPathReservation.TripInsuranceFile.TripInsuranceBookingInfo = tPIInfo;
            }
            else
            {
                bookingPathReservation.TripInsuranceFile = null;
            }

           await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, bookingPathReservation.SessionId, new List<string> { bookingPathReservation.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);

            return tPIInfo;
        }


        public void UpdateSeatsAfterEfsRemovalV2(FlightReservationResponse flightReservationResponse, Reservation bookingPathReservation, string sessionId)
        {
            if (flightReservationResponse != null)
            {
                if (bookingPathReservation?.TravelersCSL != null)
                {
                    foreach (var traveler in bookingPathReservation.TravelersCSL)
                    {
                        if (traveler.Value.TravelerTypeCode != "INF")//No need to builds the seat details for Infant on Lap as we dont allow seat for infant
                        {
                            var travelerSeats = flightReservationResponse.DisplayCart != null ?
                                                flightReservationResponse.DisplayCart.DisplaySeats.Where(displaySeat => displaySeat.PersonIndex == traveler.Value.TravelerNameIndex)
                                                : null;
                            foreach (var travelerSeat in traveler.Value.Seats)
                            {
                                if (travelerSeats == null)
                                {
                                    travelerSeat.SeatAssignment = String.Empty;
                                    travelerSeat.Price = 0;
                                    travelerSeat.PriceAfterTravelerCompanionRules = 0;
                                    travelerSeat.Currency = "USD";
                                    travelerSeat.ProgramCode = String.Empty;
                                    travelerSeat.SeatType = String.Empty;
                                    travelerSeat.LimitedRecline = false;
                                }
                                else
                                {
                                    if (!travelerSeats.Any(seat => seat.DepartureAirportCode == travelerSeat.Origin && seat.ArrivalAirportCode == travelerSeat.Destination))
                                    {
                                        travelerSeat.SeatAssignment = String.Empty;
                                        travelerSeat.Price = 0;
                                        travelerSeat.PriceAfterTravelerCompanionRules = 0;
                                        travelerSeat.Currency = "USD";
                                        travelerSeat.ProgramCode = String.Empty;
                                        travelerSeat.SeatType = String.Empty;
                                        travelerSeat.LimitedRecline = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void AddMissingSeatAssignment(List<MOBSeatPrice> tempSeatPrices, United.Persist.Definition.Shopping.Reservation persistedReservation)
        {
            if (_configuration.GetValue<bool>("OmnicartPartialSeatAssignmentFix")
                        && persistedReservation?.ShopReservationInfo2?.IsOmniCartSavedTripFlow == true)
            {
                if (persistedReservation.SeatPrices != null && persistedReservation.SeatPrices.Count > 0)
                {
                    foreach (var seatPrice in persistedReservation.SeatPrices)
                    {

                        if (tempSeatPrices != null && !tempSeatPrices.Any(s => string.Equals($"{s.Origin}-{s.Destination}",
                                                                            $"{seatPrice.Origin}-{seatPrice.Destination}", StringComparison.OrdinalIgnoreCase)))
                        {
                            tempSeatPrices.AddRange(persistedReservation.SeatPrices.Where(s => string.Equals($"{s.Origin}-{s.Destination}",
                                                                            $"{seatPrice.Origin}-{seatPrice.Destination}", StringComparison.OrdinalIgnoreCase)));
                        }
                    }
                }
            }
        }

        public string BuildStrikeThroughDescription()
        {
            return _configuration.GetValue<string>("StrikeThroughPriceTypeDescription");
        }

        public async Task<MOBCreateShoppingSessionResponse> CreateShoppingSession(MOBCreateShoppingSessionRequest request)
        {
            MOBCreateShoppingSessionResponse response = new MOBCreateShoppingSessionResponse();
            response.TransactionId = request.TransactionId;
            response.LanguageCode = request.LanguageCode;

            if (!GeneralHelper.ValidateAccessCode(request.AccessCode))
                throw new MOBUnitedException("The access code you provided is not valid.");
            if (request.Reservation == null)
                throw new MOBUnitedException("The Reservation is null.");

            Session session = await _shoppingSessionHelper.CreateShoppingSession(request.Application.Id, request.DeviceId, request.Application.Version.Major, request.TransactionId, request.MPNumber, string.Empty, false, true,false, request.ShoppingSessionId).ConfigureAwait(false);

            ReservationDetail reservationDetail = new ReservationDetail() { Detail = request.Reservation };
            await _sessionHelperService.SaveSession(reservationDetail, session.SessionId, new List<string> { session.SessionId, reservationDetail.GetType().FullName }, reservationDetail.GetType().FullName).ConfigureAwait(false);

            MOBShoppingCart persistShoppingCart = new MOBShoppingCart();
            await _sessionHelperService.SaveSession(persistShoppingCart, session.SessionId, new List<string> { session.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName).ConfigureAwait(false);

            Persist.Definition.Merchandizing.GetOffers productOffer = new Persist.Definition.Merchandizing.GetOffers();
            await _sessionHelperService.SaveSession(productOffer, session.SessionId, new List<string> { session.SessionId, productOffer.ObjectName }, productOffer.ObjectName).ConfigureAwait(false);

            response.TransactionId = request.TransactionId;
            response.ShoppingSessionId = session.SessionId;
            return response;
        }

        public async Task<MOBResponse> PopulateMerchOffers(MOBPopulateMerchOffersRequest request)
        {

            MOBResponse response = new MOBResponse();
            response.TransactionId = request.TransactionId;
            response.LanguageCode = request.LanguageCode;

            if (!GeneralHelper.ValidateAccessCode(request.AccessCode))
                throw new MOBUnitedException("The access code you provided is not valid.");
            if (String.IsNullOrEmpty(request.ProductOfferStr))
                throw new MOBUnitedException("The ProductOffer string is null or empty.");

            Session session = await _shoppingSessionHelper.GetValidateSession(request.ShoppingSessionId, false, false).ConfigureAwait(false);
            United.Service.Presentation.ProductResponseModel.ProductOffer productOffer = JsonConvert.DeserializeObject<United.Service.Presentation.ProductResponseModel.ProductOffer>(request.ProductOfferStr);
            var productOfferPersist = new Persist.Definition.Merchandizing.GetOffers()
            {
                Characteristics = productOffer.Characteristics,
                ODOptions = productOffer.ODOptions,
                FlightSegments = productOffer.FlightSegments,
                Offers = productOffer.Offers,
                Response = productOffer.Response,
                Solutions = productOffer.Solutions,
                Travelers = productOffer.Travelers
            };
            await _sessionHelperService.SaveSession(productOfferPersist, session.SessionId, new List<string> { session.SessionId, productOfferPersist.ObjectName }, productOfferPersist.ObjectName).ConfigureAwait(false);
            return response;
        }

        public async Task<MOBResponse> UpdateReservation(MOBUpdateReservationRequest request)
        {
            MOBResponse response = new MOBResponse();
            response.TransactionId = request.TransactionId;
            response.LanguageCode = request.LanguageCode;

            if (!GeneralHelper.ValidateAccessCode(request.AccessCode))
                throw new MOBUnitedException("The access code you provided is not valid.");
            if (request.Reservation == null)
                throw new MOBUnitedException("The Reservation is null.");
            if (String.IsNullOrEmpty(request.ShoppingSessionId))
                throw new MOBUnitedException("The Shopping Session Id is null or empty.");

            Session session = await _shoppingSessionHelper.GetValidateSession(request.ShoppingSessionId, false, false).ConfigureAwait(false);
            ReservationDetail reservationDetail = new ReservationDetail() { Detail = request.Reservation };
            await _sessionHelperService.SaveSession(reservationDetail, session.SessionId, new List<string> { session.SessionId, reservationDetail.GetType().FullName }, reservationDetail.GetType().FullName).ConfigureAwait(false);

            return response;

        }
        private async Task SetEligibilityforETCTravelCredit(MOBSHOPReservation reservation, Session session, Reservation bookingPathReservation)
        {
            //if (await _featureToggles.IsEnableETCCreditsInBookingFeature(session.CatalogItems))
            //{
                if (session == null)
                {
                    session = new Session();
                    session = await _sessionHelperService.GetSession<Session>(bookingPathReservation.SessionId, session.ObjectName, new List<string> { bookingPathReservation.SessionId, session.ObjectName }).ConfigureAwait(false);
                }
                reservation.EligibleForETCPricingType = false;
                if (session.PricingType == Mobile.Model.MSC.FormofPayment.PricingType.ETC.ToString())
                {
                    reservation.EligibleForETCPricingType = true;
                    reservation.PricingType = session.PricingType;
                }
                else
                {
                    if (bookingPathReservation.FormOfPaymentType == MOBFormofPayment.ETC || bookingPathReservation.FormOfPaymentType == MOBFormofPayment.TC)
                    {
                        reservation.EligibleForETCPricingType = true;
                    }
                    reservation.PricingType = session.PricingType;
                }
            //}
        }
    }
}
