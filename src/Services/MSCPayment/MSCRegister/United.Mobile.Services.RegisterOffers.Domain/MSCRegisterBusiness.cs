using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Common.Helper.MSCPayment.Interfaces;
using United.Definition;
using United.Definition.Booking;
using United.Definition.Common;
using United.Definition.FormofPayment;
using United.Definition.PreOrderMeals;
using United.Definition.SeatCSL30;
using United.Definition.Shopping;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.MSCPayment.Interfaces;
using United.Mobile.DataAccess.MSCPayment.Services;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Mobile.Model;
using United.Mobile.Model.Common;
using United.Mobile.Model.MSC;
using United.Persist.Definition.CCE;
using United.Persist.Definition.Merchandizing;
using United.Persist.Definition.Pcu;
using United.Persist.Definition.SeatChange;
using United.Persist.Definition.Shopping;
using United.Service.Presentation.CommonModel;
using United.Service.Presentation.FlightRequestModel;
using United.Service.Presentation.PersonalizationResponseModel;
using United.Service.Presentation.ProductModel;
using United.Service.Presentation.ProductResponseModel;
using United.Service.Presentation.ReservationResponseModel;
using United.Service.Presentation.SegmentModel;
using United.Services.FlightShopping.Common.DisplayCart;
using United.Services.FlightShopping.Common.Extensions;
using United.Services.FlightShopping.Common.FlightReservation;
using United.Utility.Helper;
using Characteristic = United.Service.Presentation.CommonModel.Characteristic;
using RegisterOfferRequest = United.Services.FlightShopping.Common.FlightReservation.RegisterOfferRequest;

namespace United.Mobile.Services.RegisterOffers.Domain
{
    public class MSCRegisterBusiness : IMSCRegisterBusiness
    {
        private readonly ICacheLog<MSCRegisterBusiness> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMSCShoppingSessionHelper _mscshoppingSessionHelper;
        private readonly IETCUtility _etcUtility;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IShoppingUtility _shoppingUtility;
        private readonly IPaymentService _paymentService;
        private readonly ICheckoutUtiliy _checkoutUtiliy;
        private readonly IMSCFormsOfPayment _mscFormsOfPayment;
        private readonly IMSCPageProductInfoHelper _mscPageProductInfoHelper;
        private readonly ILegalDocumentsForTitlesService _legalDocumentsForTitlesService;
        private readonly IMSCPkDispenserPublicKey _mscPkDispenserPublicKey;
        private readonly IShoppingCartUtility _shoppingCartUtility;
        private readonly ICachingService _cachingService;
        private readonly ISeatEnginePostService _seatEnginePostService;
        private readonly string _UPGRADEMALL = "UPGRADEMALL";
        private readonly IGetPNRByRecordLocatorService _getPNRByRecordLocatorService;
        private readonly IMemberInformationService _memberInformationService;
        private readonly IVerifyMileagePlusHashpinService _verifyMileagePlusHashpinService;
        private readonly IHeaders _headers;
        private readonly IDynamoDBService _dynamoDBService;
        private readonly IValidateHashPinService _validateHashPinService;
        private readonly IFeatureSettings _featureSettings;
        private readonly IFeatureToggles _featureToggles;


        public MSCRegisterBusiness(ICacheLog<MSCRegisterBusiness> logger
            , IConfiguration configuration
            , IMSCShoppingSessionHelper mscshoppingSessionHelper
            , IETCUtility etcUtility
            , ISessionHelperService sessionHelperService
            , IShoppingCartService shoppingCartService
            , IShoppingUtility shoppingUtility
            , IPaymentService paymentService
            , ICheckoutUtiliy checkoutUtiliy
            , IMSCFormsOfPayment mscFormsOfPayment
            , IMSCPageProductInfoHelper mscPageProductInfoHelper
            , ILegalDocumentsForTitlesService legalDocumentsForTitlesService
            , IMSCPkDispenserPublicKey mscPkDispenserPublicKey
            , IShoppingCartUtility shoppingCartUtility
            , ICachingService cachingService
            , ISeatEnginePostService seatEnginePostService
            , IGetPNRByRecordLocatorService getPNRByRecordLocatorService
            , IMemberInformationService memberInformationService
            , IVerifyMileagePlusHashpinService verifyMileagePlusHashpinService
            , IHeaders headers
            , IDynamoDBService dynamoDBService
            , IValidateHashPinService validateHashPinService
            , IFeatureSettings featureSettings
            , IFeatureToggles featureToggles)
        {
            _logger = logger;
            _configuration = configuration;
            _mscshoppingSessionHelper = mscshoppingSessionHelper;
            _etcUtility = etcUtility;
            _sessionHelperService = sessionHelperService;
            _shoppingCartService = shoppingCartService;
            _shoppingUtility = shoppingUtility;
            _paymentService = paymentService;
            _checkoutUtiliy = checkoutUtiliy;
            _mscFormsOfPayment = mscFormsOfPayment;
            _mscPageProductInfoHelper = mscPageProductInfoHelper;
            _legalDocumentsForTitlesService = legalDocumentsForTitlesService;
            _mscPkDispenserPublicKey = mscPkDispenserPublicKey;
            _shoppingCartUtility = shoppingCartUtility;
            _cachingService = cachingService;
            _seatEnginePostService = seatEnginePostService;
            _getPNRByRecordLocatorService = getPNRByRecordLocatorService;
            _memberInformationService = memberInformationService;
            _verifyMileagePlusHashpinService = verifyMileagePlusHashpinService;
            _headers = headers;
            _dynamoDBService = dynamoDBService;
            _validateHashPinService = validateHashPinService;
            _featureSettings = featureSettings;
            _featureToggles = featureToggles;
        }

        public async Task<MOBRegisterOfferResponse> RegisterOffers(MOBRegisterOfferRequest request)
        {
            var response = new MOBRegisterOfferResponse();
            string cartId = string.Empty;

            Session session = null;
            if (!string.IsNullOrEmpty(request.SessionId))
            {
                session = await _mscshoppingSessionHelper.GetValidateSession(request.SessionId, false, true);
                session.Flow = request.Flow;
            }
            else
            {
                _logger.LogError("RegisterOffers {Error}", "SessionId not available");
                throw new MOBUnitedException(((int)MOBErrorCodes.ViewResCFOPSessionExpire).ToString(), _configuration.GetValue<string>("Booking2OGenericExceptionMessage").ToString());
            }
            if (string.IsNullOrEmpty(request.CartId))
            {
                request.CartId = await CreateCart(request, session).ConfigureAwait(false);

                MOBShoppingCart persistShoppingCart = new MOBShoppingCart();


                await _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, request.SessionId, new List<string> { request.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName);
            }

            if (request.Flow == FlowType.POSTBOOKING.ToString() || request.Flow == FlowType.RESHOP.ToString())
                response = await RegisterOffersForPostBooking(request, session);
            else
                response = await RegisterOffers(request, session);

            if (_configuration.GetValue<bool>("GetFoPOptionsAlongRegisterOffers"))
            {

                bool IsMilesFOPEnabled = false;
                IsMilesFOPEnabled = GetIsMilesFOPEnabled(response.ShoppingCart);
                bool isDefault = false;

                if (ConfigUtility.IsManageResETCEnabled(request.Application.Id, request.Application.Version.Major))
                {
                    var tupleRespEligibleFormofPayments = await _mscFormsOfPayment.GetEligibleFormofPayments(request, session, response.ShoppingCart, request.CartId, request.Flow, null, IsMilesFOPEnabled);
                    response.EligibleFormofPayments = tupleRespEligibleFormofPayments.EligibleFormofPayments;
                    isDefault = tupleRespEligibleFormofPayments.isDefault;
                }
                else
                {
                    MOBFOPEligibilityRequest eligiblefopRequest = new MOBFOPEligibilityRequest()
                    {
                        TransactionId = request.TransactionId,
                        DeviceId = request.DeviceId,
                        AccessCode = request.AccessCode,
                        LanguageCode = request.LanguageCode,
                        Application = request.Application,
                        CartId = request.CartId,
                        SessionId = request.SessionId,
                        Flow = request.Flow,
                        Products = await GetProductsForEligibleFopRequest(response.ShoppingCart)
                    };
                    var tupleRespEligibleFormofPayments = await _mscFormsOfPayment.EligibleFormOfPayments(eligiblefopRequest, session, IsMilesFOPEnabled);
                    response.EligibleFormofPayments = tupleRespEligibleFormofPayments.EligibleFormofPayments;
                    isDefault = tupleRespEligibleFormofPayments.isDefault;
                    var upliftFop = _mscFormsOfPayment.UpliftAsFormOfPayment(null, response.ShoppingCart);
                    if (upliftFop != null && response.EligibleFormofPayments != null)
                    {
                        response.EligibleFormofPayments.Add(upliftFop);
                    }
                }
                //controllerUtility.StopWatchAndWriteLogs(request.SessionId, request.DeviceId, "RegisterOffers_CFOP", response, request.Application, "Response");
                if (await _featureSettings.GetFeatureSettingValue("EnableCheckInCloudMigrationMSC_23X") && _checkoutUtiliy.IsInternationalBillingAddress_CheckinFlowEnabled(request.Application)
                  && response.ShoppingCart?.FormofPaymentDetails?.InternationalBilling?.BillingAddressProperties == null
                  && request?.Flow.ToLower() == FlowType.CHECKIN.ToString().ToLower())
                {
                    response.ShoppingCart.FormofPaymentDetails = await GetBillingAddressProperties(response.ShoppingCart.FormofPaymentDetails).ConfigureAwait(false);
                }
               
                response.CheckinSessionId = request.CheckinSessionId;
                response.SessionId = session.SessionId;
                response.IsDefaultPaymentOption = isDefault;
                await _sessionHelperService.SaveSession<List<FormofPaymentOption>>(response.EligibleFormofPayments, session.SessionId, new List<string> { session.SessionId, "United.Definition.FormofPaymentOption" }, "United.Definition.FormofPaymentOption").ConfigureAwait(false);
            }
            return await System.Threading.Tasks.Task.FromResult(response);
        }

        public async Task<MOBRegisterOfferResponse> RegisterUsingMiles(MOBRegisterUsingMilesRequest request)
        {
            var response = new MOBRegisterOfferResponse();
            string cartId = string.Empty;

            //Validating Sessions from client request and throws exception if its invalid
            Session session = null;
            if (!string.IsNullOrEmpty(request.SessionId))
            {
                session = await _mscshoppingSessionHelper.GetValidateSession(request.SessionId, false, true);
                session.Flow = request.Flow;
            }
            else
            {
                if (ConfigUtility.VersionCheck_NullSession_AfterAppUpgradation(request))
                    throw new MOBUnitedException(((int)MOBErrorCodes.ViewResCFOP_NullSession_AfterAppUpgradation).ToString(), _configuration.GetValue<string>("Booking2OGenericExceptionMessage").ToString());
                else
                    throw new MOBUnitedException(((int)MOBErrorCodes.ViewResCFOPSessionExpire).ToString(), _configuration.GetValue<string>("Booking2OGenericExceptionMessage").ToString());
            }

            //save session 
            await _sessionHelperService.SaveSession<Session>(session, session.SessionId, new List<string>() { session.SessionId, session.ObjectName }, session.ObjectName).ConfigureAwait(false);
            //Read shopping cart from redis cache
            response.ShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(session.SessionId, new MOBShoppingCart().ObjectName, new List<string> { session.SessionId, new MOBShoppingCart().ObjectName }).ConfigureAwait(false);
            //throw an exception if shopping cart is null
            if (response.ShoppingCart == null)
            {
                if (ConfigUtility.VersionCheck_NullSession_AfterAppUpgradation(request))
                    throw new MOBUnitedException(((int)MOBErrorCodes.ViewResCFOP_NullSession_AfterAppUpgradation).ToString(), _configuration.GetValue<string>("Booking2OGenericExceptionMessage").ToString());
                else
                    throw new MOBUnitedException(((int)MOBErrorCodes.ViewResCFOPSessionExpire).ToString(), _configuration.GetValue<string>("Booking2OGenericExceptionMessage").ToString());
            }

            response.ShoppingCart.Flow = request.Flow;

            if (_configuration.GetValue<bool>("GetFoPOptionsAlongRegisterOffers"))
            {
                bool IsMilesFOPEnabled = false;
                IsMilesFOPEnabled = GetIsMilesFOPEnabled(response.ShoppingCart);
                bool isDefault = false;

                if (ConfigUtility.IsManageResETCEnabled(request.Application.Id, request.Application.Version.Major))
                {
                    await BuildMilesFOP(request, session, response, IsMilesFOPEnabled).ConfigureAwait(false);
                    var tupleRespEligibleFormofPayments = await _mscFormsOfPayment.GetEligibleFormofPayments(request, session, response.ShoppingCart, request.CartId, request.Flow, null, IsMilesFOPEnabled);
                    response.EligibleFormofPayments = tupleRespEligibleFormofPayments.EligibleFormofPayments;
                    isDefault = tupleRespEligibleFormofPayments.isDefault;
                }
                else
                {
                    MOBFOPEligibilityRequest eligiblefopRequest = new MOBFOPEligibilityRequest()
                    {
                        TransactionId = request.TransactionId,
                        DeviceId = request.DeviceId,
                        AccessCode = request.AccessCode,
                        LanguageCode = request.LanguageCode,
                        Application = request.Application,
                        CartId = request.CartId,
                        SessionId = request.SessionId,
                        Flow = request.Flow,
                        Products = await GetProductsForEligibleFopRequest(response.ShoppingCart)
                    };
                    var tupleRespEligibleFormofPayments = await _mscFormsOfPayment.EligibleFormOfPayments(eligiblefopRequest, session, IsMilesFOPEnabled);
                    response.EligibleFormofPayments = tupleRespEligibleFormofPayments.EligibleFormofPayments;
                    isDefault = tupleRespEligibleFormofPayments.isDefault;
                    var upliftFop = _mscFormsOfPayment.UpliftAsFormOfPayment(null, response.ShoppingCart);
                    if (upliftFop != null && response.EligibleFormofPayments != null)
                    {
                        response.EligibleFormofPayments.Add(upliftFop);
                    }
                }

                await _sessionHelperService.SaveSession<MOBShoppingCart>(response.ShoppingCart, request.SessionId, new List<string> { request.SessionId, response.ShoppingCart.ObjectName }, response.ShoppingCart.ObjectName).ConfigureAwait(false);
                response.SessionId = request.SessionId;
                response.TransactionId = request.TransactionId;
                response.Flow = request.Flow;
                response.CheckinSessionId = request.CheckinSessionId;
                response.IsDefaultPaymentOption = isDefault;
                await _sessionHelperService.SaveSession<List<FormofPaymentOption>>(response.EligibleFormofPayments, session.SessionId, new List<string> { session.SessionId, "United.Definition.FormofPaymentOption" }, "United.Definition.FormofPaymentOption").ConfigureAwait(false);
            }
            return await System.Threading.Tasks.Task.FromResult(response);
        }

        public async Task<MOBRegisterOfferResponse> RegisterBags(MOBRegisterBagsRequest request)
        {
            var response = CreateMOBRegisterOfferResponse(request);

            Session session = null;
            if (!string.IsNullOrEmpty(request.SessionId))
            {
                session = await _mscshoppingSessionHelper.GetValidateSession(request.SessionId, false, true);
                session.Flow = request.Flow;
            }
            else
            {
                _logger.LogError("RegisterBags {Error}", "SessionId not available");
                throw new MOBUnitedException(((int)MOBErrorCodes.ViewResCFOPSessionExpire).ToString(), _configuration.GetValue<string>("Booking2OGenericExceptionMessage").ToString());
            }

            if (request.Bags.Any(x => x.TotalBags > 0))
            {
                if (string.IsNullOrWhiteSpace(request.CartId))
                    request.CartId = await CreateCart(request, session).ConfigureAwait(false);

                MOBShoppingCart persistShoppingCart = new MOBShoppingCart();
                await _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, session.SessionId, new List<string> { session.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName).ConfigureAwait(false);

                bool didPurchase = false;
                var tupleRegisterOfferResponse = await RegisterBags(request, session, response, persistShoppingCart.IsCorporateBusinessNamePersonalized);
                response = tupleRegisterOfferResponse.RegisterOfferResponse;
                didPurchase = tupleRegisterOfferResponse.didPurchase;
                if (!didPurchase) 
                {
                    var isEnableMFOPCheckinBags = await _featureToggles.IsEnableMFOPCheckinBags(request.Application.Id, request.Application.Version.Major).ConfigureAwait(false);
                    bool IsMilesFOPEnabled = false;
                    if(isEnableMFOPCheckinBags) 
                        IsMilesFOPEnabled = GetIsMilesFOPEnabled(response.ShoppingCart);

                    await GetAndSetEligibleFormOfPayments(request, session, response, IsMilesFOPEnabled); 
                }
            }
            else
            {
                response.ShoppingCart = new MOBShoppingCart
                {
                    TotalPrice = "0.00",
                    Products = new List<MOBProdDetail>
                        {
                            new MOBProdDetail()
                            {
                                Code = "BAG"
                            }
                        }
                };
            }
            if (_checkoutUtiliy.IsInternationalBillingAddress_CheckinFlowEnabled(request.Application)
                 && response.ShoppingCart?.FormofPaymentDetails?.InternationalBilling?.BillingAddressProperties == null
                 && request?.Flow.ToLower() == FlowType.CHECKIN.ToString().ToLower())
            {
                response.ShoppingCart.FormofPaymentDetails = await GetBillingAddressProperties(response.ShoppingCart.FormofPaymentDetails);
            }
            return await System.Threading.Tasks.Task.FromResult(response);
        }

        public async Task<MOBRegisterOfferResponse> RegisterSameDayChange(MOBRegisterSameDayChangeRequest request)
        {
            var response = CreateMOBRegisterOfferResponse(request);

            Session session = null;
            if (!string.IsNullOrEmpty(request.SessionId))
            {
                session = await _mscshoppingSessionHelper.GetValidateSession(request.SessionId, false, true);
                session.Flow = request.Flow;
            }
            else
            {
                _logger.LogError("RegisterSameDayChange {Error}", "SessionId not available");
                throw new MOBUnitedException(((int)MOBErrorCodes.ViewResCFOPSessionExpire).ToString(), _configuration.GetValue<string>("Booking2OGenericExceptionMessage").ToString());
            }

            if (string.IsNullOrWhiteSpace(request.CartId))
                request.CartId = await CreateCart(request, session).ConfigureAwait(false);
            bool didPurchase = false;
            var test = await RegisterSameDayChange(request, session, response);
            response = test.response;
            didPurchase = test.didPurchase;
            if (!didPurchase) { await GetAndSetEligibleFormOfPayments(request, session, response); }
            return await System.Threading.Tasks.Task.FromResult(response);
        }

        public async Task<MOBRegisterOfferResponse> RegisterCheckinSeats(MOBRegisterCheckinSeatsRequest request)
        {
            var response = CreateMOBRegisterOfferResponse(request);

            if (request.IsMiles && !string.IsNullOrEmpty(request.MileagePlusNumber))
            {
                string authToken = string.Empty;
                var validWalletRequest = await new HashPin(_logger, _configuration, _validateHashPinService, _dynamoDBService, _headers, _featureSettings).
                    ValidateHashPinAndGetAuthToken(request.MileagePlusNumber, request.HashPinCode, request.Application.Id, request.DeviceId, request.Application.Version.Major, authToken).ConfigureAwait(false);

                if (!validWalletRequest)
                {
                    throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
            }
            Session session = null;
            if (!string.IsNullOrEmpty(request.SessionId))
            {
                session = await _mscshoppingSessionHelper.GetValidateSession(request.SessionId, false, true);
                session.Flow = request.Flow;
            }
            else
            {
                _logger.LogError("RegisterCheckinSeats {Error}", "SessionId is null");
                throw new MOBUnitedException(((int)MOBErrorCodes.ViewResCFOPSessionExpire).ToString(), _configuration.GetValue<string>("Booking2OGenericExceptionMessage").ToString());
            }
            if (string.IsNullOrWhiteSpace(request.CartId))
                request.CartId = await CreateCart(request, session).ConfigureAwait(false);
            bool didPurchase = false;

            var tupleResponse = await RegisterCheckinSeats(request, session, response);
            response = tupleResponse.response;
            didPurchase = tupleResponse.didPurchase;

            //Adding Billing address for International billing
            if (_checkoutUtiliy.IsInternationalBillingAddress_CheckinFlowEnabled(request.Application)
                && response.ShoppingCart.FormofPaymentDetails?.InternationalBilling?.BillingAddressProperties == null && request?.Flow?.ToLower() == FlowType.CHECKIN.ToString().ToLower())
            {
                response.ShoppingCart.FormofPaymentDetails = await GetBillingAddressProperties(response.ShoppingCart.FormofPaymentDetails);
                await _sessionHelperService.SaveSession<MOBShoppingCart>(response.ShoppingCart, request.SessionId, new List<string> { request.SessionId, new MOBShoppingCart().ObjectName }, new MOBShoppingCart().ObjectName).ConfigureAwait(false);
            }

            bool isMFOPEnabled = false;
            if (response.ShoppingCart.FormofPaymentDetails?.MilesFOP != null)
            {
                isMFOPEnabled = response.ShoppingCart.FormofPaymentDetails.MilesFOP.IsMilesFOPEligible;
            }
            if (!didPurchase) { 
                await GetAndSetEligibleFormOfPayments(request, session, response, isMFOPEnabled); }

            return await System.Threading.Tasks.Task.FromResult(response);
        }

        private async Task<(MOBRegisterOfferResponse response, bool didPurchase)> RegisterSameDayChange(MOBRegisterSameDayChangeRequest request, Session session, MOBRegisterOfferResponse response)
        {
            var shopping = new Shopping();
            var reservationDetail = new ReservationDetail();
            reservationDetail = await _sessionHelperService.GetSession<ReservationDetail>(request.SessionId, reservationDetail.GetType().FullName, new List<string> { request.SessionId, reservationDetail.GetType().FullName }).ConfigureAwait(false);
            FlightReservationResponse flightReservationResponse = await RegisterSameDayChange(request, reservationDetail, session).ConfigureAwait(false);
            return await PopulateRegisterResponse(request, response, session, flightReservationResponse, shopping);
        }

        private async Task<FlightReservationResponse> RegisterSameDayChange(MOBRegisterSameDayChangeRequest request, ReservationDetail reservationDetail, Session session)
        {
            var response = new FlightReservationResponse();
            var actionName = "RegisterCheckInProductOffer";
            RegisterCheckInProductOfferRequest registerOfferRequest = BuildRegisterSameDayChangeRequest(request, reservationDetail);

            string jsonRequest = JsonConvert.SerializeObject(registerOfferRequest);
            string cslResponse = await _shoppingCartService.RegisterSameDayChange(session.Token, actionName, jsonRequest, session.SessionId).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(cslResponse))
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));

            response = JsonConvert.DeserializeObject<FlightReservationResponse>(cslResponse);

            if (response == null || response.Status.Equals(United.Services.FlightShopping.Common.StatusType.Failure))
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));

            if (response.Errors != null && response.Errors.Count > 0)
            {
                string errorMessage = string.Empty;
                response.Errors.ForEach(errorInfo => errorMessage += (errorInfo.Message + " "));
                throw new System.Exception(errorMessage);
            }

            return response;
        }

        private RegisterCheckInProductOfferRequest BuildRegisterSameDayChangeRequest(MOBRegisterSameDayChangeRequest request, ReservationDetail reservationDetail)
        {
            RegisterCheckInProductOfferRequest registerSameDayChangeRequest = CreateRegisterCheckInProductOfferRequest(request, reservationDetail);
            registerSameDayChangeRequest.ProductCode = "SDC";
            registerSameDayChangeRequest.ProductDescription = "Same Day Change";
            registerSameDayChangeRequest.ChangeFlight = new Service.Presentation.EServiceCheckInModel.ChangeFlight() { AlternateFlightSegmentNumber = request.TripID.ToInteger() };

            return registerSameDayChangeRequest;
        }

        private async Task<(MOBRegisterOfferResponse RegisterOfferResponse, bool didPurchase)> RegisterBags(MOBRegisterBagsRequest request, Session session, MOBRegisterOfferResponse response, bool isCorporateNamePersonalized)
        {
            var shopping = new Shopping();
            bool didPurchase = false;

            var reservationDetail = new ReservationDetail();
            reservationDetail = await _sessionHelperService.GetSession<ReservationDetail>(request.SessionId, reservationDetail.GetType().FullName, new List<string> { request.SessionId, reservationDetail.GetType().FullName }).ConfigureAwait(false);
            var corporateCheck = reservationDetail?.Detail?.BookingIndicators?.IndicatorSpecifications.Where(c => string.Equals(c?.Code, "CorporateTraveler", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (_configuration.GetValue<bool>("EnableU4BCorporateCheckin"))
            {
                var isCorpTraveler = reservationDetail?.Detail?.BookingIndicators?.IndicatorSpecifications.Where(c => string.Equals(c?.Code, "CorporateTraveler", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (isCorpTraveler != null)
                {
                    if (await _shoppingCartUtility.IsEnableCorporateNameChange().ConfigureAwait(false))
                    {
                        bool isEnableSuppressingCompanyNameForBusiness = await _shoppingCartUtility.IsEnableSuppressingCompanyNameForBusiness(isCorporateNamePersonalized).ConfigureAwait(false);
                        if (await _shoppingCartUtility.IsEnableSuppressingCompanyNameForBusiness(isCorporateNamePersonalized).ConfigureAwait(false))
                        {
                            var u4bCorporateBookingDisclaimerText = !String.IsNullOrEmpty(_configuration.GetValue<string>("U4BCorporateBookingDisclaimerText")) ? _configuration.GetValue<string>("U4BCorporateBookingDisclaimerText") : String.Empty;
                            response.ShoppingCart.CorporateDisclaimerText = $"{u4bCorporateBookingDisclaimerText}{isCorpTraveler.Value}.";
                        }
                        else
                            response.ShoppingCart.CorporateDisclaimerText = _configuration.GetValue<string>("CorporateDisclaimerTextForBusinessTravel");
                    }
                    else
                    {
                        var u4bCorporateBookingDisclaimerText = !String.IsNullOrEmpty(_configuration.GetValue<string>("U4BCorporateBookingDisclaimerText")) ? _configuration.GetValue<string>("U4BCorporateBookingDisclaimerText") : String.Empty;
                        response.ShoppingCart.CorporateDisclaimerText = $"{u4bCorporateBookingDisclaimerText}{isCorpTraveler.Value}.";
                    }
                }
            };

            var isEnableMFOPCheckinBags = await _featureToggles.IsEnableMFOPCheckinBags(request.Application.Id, request.Application.Version.Major).ConfigureAwait(false);
            FlightReservationResponse flightReservationResponse = await RegisterBags(request, reservationDetail, session, isEnableMFOPCheckinBags).ConfigureAwait(false);

            TravelOptionsCollection travelOptions = flightReservationResponse.DisplayCart?.TravelOptions;
            TravelOption bagTravelOption = travelOptions?.FirstOrDefault(x => x.Type == "Bags");

            if (bagTravelOption == null) { return (response, didPurchase); }

            decimal totalPrice = bagTravelOption.Amount;
            response.ShoppingCart.TotalPrice = String.Format("{0:0.00}", totalPrice);
            response.ShoppingCart.PaymentTarget = GetPaymentTargetForRegisterFop(travelOptions, false);
            response.ShoppingCart.Products = await _shoppingCartUtility.ConfirmationPageProductInfo(flightReservationResponse, false, false, request.Flow, request.Application, sessionId: request.SessionId).ConfigureAwait(false);
            
            MOBBag bagCounts = request.Bags.Aggregate(new MOBBag(), (acc, bags) => BagCountFolder(acc, bags));
            response.ShoppingCart.Products.FirstOrDefault(x => x.Code == "BAG").LineItems = GetBagsLineItems(bagCounts);

            response.ShoppingCart.TermsAndConditions = new List<MOBMobileCMSContentMessages>();

            string[] currency = bagTravelOption.Currency.Split('-');
            string currencyCode = currency[0];
            string currencySymbol = currency[1];

            response.ShoppingCart.PointofSale = GetPointOfSale(currencyCode);
            response.ShoppingCart.CurrencyCode = currencyCode;

            response.ShoppingCart.DisplayTotalPrice = $"{currencyCode} {currencySymbol}{String.Format("{0:0.00}", Decimal.Parse(totalPrice.ToString()))}";
            if (isEnableMFOPCheckinBags)
            {
                response.ShoppingCart.TotalMiles = Convert.ToString(bagTravelOption.MileageAmount);
                response.ShoppingCart.DisplayTotalMiles = UtilityHelper.FormatAwardAmountForDisplay(Decimal.Parse((response.ShoppingCart?.TotalMiles != null ? response.ShoppingCart.TotalMiles : "0")).ToString(), false);

                response.ShoppingCart.DisplaySubTotalPrice = $"{currencySymbol}" + String.Format("{0:0.00}", bagTravelOption.SubItems?.Where(x => x.Key.ToUpper().Contains("BAGFEE"))?.Sum(p => p.Amount));
                response.ShoppingCart.DisplayTaxesAndFees = $"{currencySymbol}" + String.Format("{0:0.00}", bagTravelOption.SubItems?.Where(x => x.Key.ToUpper().Contains("BAGTAXFEE"))?.Sum(p => p.Amount));
            }
            else
            {
                SubItem bagSubItem = bagTravelOption.SubItems.FirstOrDefault(x => x.Key == "BagsTotal");
                response.ShoppingCart.DisplaySubTotalPrice = $"{currencySymbol}" + String.Format("{0:0.00}", bagSubItem.Amount);

                SubItem taxSubItem = bagTravelOption.SubItems.FirstOrDefault(x => x.Key == "TAXTotal");
                response.ShoppingCart.DisplayTaxesAndFees = $"{currencySymbol}" + String.Format("{0:0.00}", taxSubItem.Amount);
            }

            response.ShoppingCart.CartId = flightReservationResponse.CartId.ToString();

            response.PkDispenserPublicKey = null;
            response.ShoppingCart.Flow = request.Flow;

            

            await _sessionHelperService.SaveSession<MOBShoppingCart>(response.ShoppingCart, request.SessionId, new List<string> { request.SessionId, response.ShoppingCart.ObjectName }, response.ShoppingCart.ObjectName).ConfigureAwait(false);

            if (totalPrice == 0.0m)
            {
                MOBCheckOutRequest checkOutRequest = new MOBCheckOutRequest()
                {
                    SessionId = request.SessionId,
                    CartId = request.CartId,
                    Application = request.Application,
                    AccessCode = request.AccessCode,
                    LanguageCode = request.LanguageCode,
                    Flow = request.Flow,
                    FormofPaymentDetails = new MOBFormofPaymentDetails
                    {
                        FormOfPaymentType = null
                    }
                };

                MOBCheckOutResponse checkOutResponse = await _checkoutUtiliy.CheckOut(checkOutRequest);
                if (checkOutResponse.Exception != null) { response.Exception = checkOutResponse.Exception; }
                didPurchase = true;
            }

            return (response, didPurchase);
        }

        private async Task<FlightReservationResponse> RegisterBags(MOBRegisterBagsRequest request, ReservationDetail reservationDetail, Session session, bool isEnableMFOPCheckinBags = false)
        {
            var response = new FlightReservationResponse();
            string actionName = "RegisterCheckInProductOffer";
            RegisterCheckInProductOfferRequest registerOfferRequest = BuildRegisterBagsRequest(request, reservationDetail, isEnableMFOPCheckinBags);

            string jsonRequest = JsonConvert.SerializeObject(registerOfferRequest);
            string cslResponse = await _shoppingCartService.RegisterFareLockReservation(session.Token, actionName, jsonRequest, session.SessionId).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(cslResponse))
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));

            response = JsonConvert.DeserializeObject<FlightReservationResponse>(cslResponse);

            if (response == null || response.Status.Equals(United.Services.FlightShopping.Common.StatusType.Failure))
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            if (response.Errors != null && response.Errors.Count > 0)
            {
                string errorMessage = string.Empty;
                response.Errors.ForEach(errorInfo => errorMessage += (errorInfo.Message + " "));
                throw new System.Exception(errorMessage);
            }

            return response;
        }

        public async Task<MOBRegisterOfferResponse> RegisterBagDetails(MOBRegisterBagDetailsRequest request)
        {
            var response = CreateMOBRegisterOfferResponse(request);

            Session session = null;
            if (!string.IsNullOrEmpty(request.SessionId))
            {
                session = await _mscshoppingSessionHelper.GetValidateSession(request.SessionId, false, true);
                session.Flow = request.Flow;
            }
            else
            {
                _logger.LogError("RegisterBagDetails {Error}", "SessionId not available");
                throw new MOBUnitedException(((int)MOBErrorCodes.ViewResCFOPSessionExpire).ToString(), _configuration.GetValue<string>("Booking2OGenericExceptionMessage").ToString());
            }

            if (request.BagDetails?.Count > 0 && request.BagDetails.Any(x => x.BagItems?.Count > 0))
            {

                if (string.IsNullOrWhiteSpace(request.CartId))
                    request.CartId = await CreateCart(request, session).ConfigureAwait(false);

                MOBShoppingCart persistShoppingCart = new MOBShoppingCart();
                await _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, session.SessionId, new List<string> { session.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName).ConfigureAwait(false);

                bool didPurchase = false;
                var tupleRegisterOfferResponse = await ProcessRegisterBagDetails(request, session, response, persistShoppingCart.IsCorporateBusinessNamePersonalized);
                response = tupleRegisterOfferResponse.RegisterOfferResponse;
                didPurchase = tupleRegisterOfferResponse.didPurchase;
                if (!didPurchase)
                {
                    var isEnableMFOPCheckinBags = await _featureToggles.IsEnableMFOPCheckinBags(request.Application.Id, request.Application.Version.Major).ConfigureAwait(false);
                    bool IsMilesFOPEnabled = false;
                    if (isEnableMFOPCheckinBags)
                        IsMilesFOPEnabled = GetIsMilesFOPEnabled(response.ShoppingCart);

                    await GetAndSetEligibleFormOfPayments(request, session, response, IsMilesFOPEnabled); 
                }
            }
            else
            {
                response.ShoppingCart = new MOBShoppingCart
                {
                    TotalPrice = "0.00",
                    Products = new List<MOBProdDetail>
                        {
                            new MOBProdDetail()
                            {
                                Code = "BAG"
                            }
                        }
                };
            }
            if (_checkoutUtiliy.IsInternationalBillingAddress_CheckinFlowEnabled(request.Application)
                 && response.ShoppingCart?.FormofPaymentDetails?.InternationalBilling?.BillingAddressProperties == null
                 && request?.Flow.ToLower() == FlowType.CHECKIN.ToString().ToLower())
            {
                response.ShoppingCart.FormofPaymentDetails = await GetBillingAddressProperties(response.ShoppingCart.FormofPaymentDetails);
            }
            return await System.Threading.Tasks.Task.FromResult(response);
        }

        private async Task<(MOBRegisterOfferResponse RegisterOfferResponse, bool didPurchase)> ProcessRegisterBagDetails(MOBRegisterBagDetailsRequest request, Session session, MOBRegisterOfferResponse response, bool isCorporateNamePersonalized)
        {
            var shopping = new Shopping();
            bool didPurchase = false;

            var reservationDetail = new ReservationDetail();
            reservationDetail = await _sessionHelperService.GetSession<ReservationDetail>(request.SessionId, reservationDetail.GetType().FullName, new List<string> { request.SessionId, reservationDetail.GetType().FullName }).ConfigureAwait(false);
            var corporateCheck = reservationDetail?.Detail?.BookingIndicators?.IndicatorSpecifications.Where(c => string.Equals(c?.Code, "CorporateTraveler", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (_configuration.GetValue<bool>("EnableU4BCorporateCheckin"))
            {
                var isCorpTraveler = reservationDetail?.Detail?.BookingIndicators?.IndicatorSpecifications.Where(c => string.Equals(c?.Code, "CorporateTraveler", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (isCorpTraveler != null)
                {
                    bool isEnableSuppressingCompanyNameForBusiness = await _shoppingCartUtility.IsEnableSuppressingCompanyNameForBusiness(isCorporateNamePersonalized).ConfigureAwait(false);
                    if (await _shoppingCartUtility.IsEnableSuppressingCompanyNameForBusiness(isCorporateNamePersonalized).ConfigureAwait(false))
                    {
                        var u4bCorporateBookingDisclaimerText = !String.IsNullOrEmpty(_configuration.GetValue<string>("U4BCorporateBookingDisclaimerText")) ? _configuration.GetValue<string>("U4BCorporateBookingDisclaimerText") : String.Empty;
                        response.ShoppingCart.CorporateDisclaimerText = $"{u4bCorporateBookingDisclaimerText}{isCorpTraveler.Value}.";
                    }
                    else
                        response.ShoppingCart.CorporateDisclaimerText = _configuration.GetValue<string>("CorporateDisclaimerTextForBusinessTravel");
                }
            };

            var isEnableMFOPCheckinBags = await _featureToggles.IsEnableMFOPCheckinBags(request.Application.Id, request.Application.Version.Major).ConfigureAwait(false);

            var flightReservationResponse = new FlightReservationResponse();
            string actionName = "RegisterCheckInProductOffer";
            RegisterCheckInProductOfferRequest registerOfferRequest = BuildRegisterBagDetailsRequest(request, reservationDetail);

            string jsonRequest = JsonConvert.SerializeObject(registerOfferRequest);
            string cslResponse = await _shoppingCartService.RegisterFareLockReservation(session.Token, actionName, jsonRequest, session.SessionId).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(cslResponse))
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));

            flightReservationResponse = JsonConvert.DeserializeObject<FlightReservationResponse>(cslResponse);

            if (flightReservationResponse == null || flightReservationResponse.Status.Equals(United.Services.FlightShopping.Common.StatusType.Failure))
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            if (flightReservationResponse.Errors != null && flightReservationResponse.Errors.Count > 0)
            {
                string errorMessage = string.Empty;
                flightReservationResponse.Errors.ForEach(errorInfo => errorMessage += (errorInfo.Message + " "));
                throw new System.Exception(errorMessage);
            }

            TravelOptionsCollection travelOptions = flightReservationResponse.DisplayCart?.TravelOptions;
            TravelOption bagTravelOption = travelOptions?.FirstOrDefault(x => x.Type == "Bags");

            if (bagTravelOption == null) { return (response, didPurchase); }

            decimal totalPrice = bagTravelOption.Amount;
            response.ShoppingCart.TotalPrice = String.Format("{0:0.00}", totalPrice);
            response.ShoppingCart.PaymentTarget = GetPaymentTargetForRegisterFop(travelOptions, false);
            response.ShoppingCart.Products = await _shoppingCartUtility.ConfirmationPageProductInfo(flightReservationResponse, false, false, request.Flow, request.Application, sessionId: request.SessionId).ConfigureAwait(false);
            BagDetail bagCounts = request.BagDetails.Aggregate(new BagDetail(), (acc, bags) => BagDetailsCountFolder(acc, bags));
            response.ShoppingCart.Products.FirstOrDefault(x => x.Code == "BAG").LineItems = GetBagDetailsLineItems(bagCounts);

            response.ShoppingCart.TermsAndConditions = new List<MOBMobileCMSContentMessages>();

            string[] currency = bagTravelOption.Currency.Split('-');
            string currencyCode = currency[0];
            string currencySymbol = currency[1];

            response.ShoppingCart.PointofSale = GetPointOfSale(currencyCode);
            response.ShoppingCart.CurrencyCode = currencyCode;

            response.ShoppingCart.DisplayTotalPrice = $"{currencyCode} {currencySymbol}{String.Format("{0:0.00}", Decimal.Parse(totalPrice.ToString()))}";

            if (isEnableMFOPCheckinBags)
            {
                response.ShoppingCart.TotalMiles = Convert.ToString(bagTravelOption.MileageAmount);
                response.ShoppingCart.DisplayTotalMiles = UtilityHelper.FormatAwardAmountForDisplay(Decimal.Parse((response.ShoppingCart?.TotalMiles != null ? response.ShoppingCart.TotalMiles : "0")).ToString(), false);

                response.ShoppingCart.DisplaySubTotalPrice = $"{currencySymbol}" + String.Format("{0:0.00}", bagTravelOption.SubItems?.Where(x => x.Key.ToUpper().Contains("BAGFEE"))?.Sum(p => p.Amount));
                response.ShoppingCart.DisplayTaxesAndFees = $"{currencySymbol}" + String.Format("{0:0.00}", bagTravelOption.SubItems?.Where(x => x.Key.ToUpper().Contains("BAGTAXFEE"))?.Sum(p => p.Amount));
            }
            else
            {
                SubItem bagSubItem = bagTravelOption.SubItems.FirstOrDefault(x => x.Key == "BagsTotal");
                response.ShoppingCart.DisplaySubTotalPrice = $"{currencySymbol}" + String.Format("{0:0.00}", bagSubItem.Amount);

                SubItem taxSubItem = bagTravelOption.SubItems.FirstOrDefault(x => x.Key == "TAXTotal");
                response.ShoppingCart.DisplayTaxesAndFees = $"{currencySymbol}" + String.Format("{0:0.00}", taxSubItem.Amount);
            }

            response.ShoppingCart.CartId = flightReservationResponse.CartId.ToString();

            response.PkDispenserPublicKey = null;
            response.ShoppingCart.Flow = request.Flow;

            await _sessionHelperService.SaveSession<MOBShoppingCart>(response.ShoppingCart, request.SessionId, new List<string> { request.SessionId, response.ShoppingCart.ObjectName }, response.ShoppingCart.ObjectName).ConfigureAwait(false);

            if (totalPrice == 0.0m)
            {
                MOBCheckOutRequest checkOutRequest = new MOBCheckOutRequest()
                {
                    SessionId = request.SessionId,
                    CartId = request.CartId,
                    Application = request.Application,
                    AccessCode = request.AccessCode,
                    LanguageCode = request.LanguageCode,
                    Flow = request.Flow,
                    FormofPaymentDetails = new MOBFormofPaymentDetails
                    {
                        FormOfPaymentType = null
                    }
                };

                MOBCheckOutResponse checkOutResponse = await _checkoutUtiliy.CheckOut(checkOutRequest);
                if (checkOutResponse.Exception != null) { response.Exception = checkOutResponse.Exception; }
                didPurchase = true;
            }

            return (response, didPurchase);
        }


        private RegisterCheckInProductOfferRequest BuildRegisterBagsRequest(MOBRegisterBagsRequest request, ReservationDetail reservationDetail, bool isEnableMFOPCheckinBags = false)
        {
            RegisterCheckInProductOfferRequest registerBagsRequest = CreateRegisterCheckInProductOfferRequest(request, reservationDetail, isEnableMFOPCheckinBags);
            registerBagsRequest.ProductCode = "BAG";
            registerBagsRequest.ProductDescription = "Bags";

            if (!string.IsNullOrWhiteSpace(request.MilitaryTravelType))
            {
                Service.Presentation.EServiceCheckInModel.BagOverrideType bagOverrideType = Service.Presentation.EServiceCheckInModel.BagOverrideType.None;
                switch (request.MilitaryTravelType)
                {
                    case "MilitaryOfficialOrders":
                        bagOverrideType = Service.Presentation.EServiceCheckInModel.BagOverrideType.MilitaryOfficialOrders;
                        break;
                    case "MilitaryPersonalTravel":
                        bagOverrideType = Service.Presentation.EServiceCheckInModel.BagOverrideType.MilitaryPersonalTravel;
                        break;
                }

                if (bagOverrideType != Service.Presentation.EServiceCheckInModel.BagOverrideType.None)
                {
                    registerBagsRequest.BagOverRideRequest = new Collection<Service.Presentation.EServiceCheckInModel.BagOverride>() { new Service.Presentation.EServiceCheckInModel.BagOverride() { Type = bagOverrideType } };
                }
            }

            registerBagsRequest.Travelers = request.Bags.Select(bag => new Service.Presentation.EServiceCheckInModel.Traveler()
            {
                FirstNameNumber = Convert.ToInt32(bag.CustomerId.Substring(4, 3)),
                SurnameNumber = Convert.ToInt32(bag.CustomerId.Substring(1, 3)),
                OperationType = Service.Presentation.EServiceCheckInModel.CustomerOperationType.SetBag,
                BagState = new Service.Presentation.EServiceCheckInModel.BagState()
                {
                    BagCount = new Service.Presentation.EServiceCheckInModel.BagCount()
                    {
                        Counts = PopulateBagCounts(bag)
                    },
                    BagAttributesInfo = PopulateBagAttributesInfo(bag)
                }
            }).ToCollection();
           
            return registerBagsRequest;
        }

        private Collection<Service.Presentation.EServiceCheckInModel.BagAttributeInfo> PopulateBagAttributesInfo(MOBBag bag)
        {
            var bagAttributes = new Collection<Service.Presentation.EServiceCheckInModel.BagAttributeInfo>();

            var bagId = 1;

            if (bag.SpecialtyItems?.Any() ?? false)
            {
                bagAttributes = bag.SpecialtyItems.Aggregate(bagAttributes, (acc, item) =>
                {
                    GetItemStatus(item, out bool isFreeCarryon, out bool interactedWithFreeCarryon, out bool isOtherItem);

                    if (!(isFreeCarryon || isOtherItem)) { return acc; }

                    var attribute = new Service.Presentation.EServiceCheckInModel.BagAttributeInfo() { BagTransactionType = Service.Presentation.EServiceCheckInModel.BagTransactionType.Additional };
                    switch (item.Code)
                    {
                        case "Exception1":
                            attribute.IsExceptionItem1 = "true";
                            break;
                        case "Exception2":
                            attribute.IsExceptionItem2 = "true";
                            break;
                        case "Exception3":
                            attribute.IsExceptionItem3 = "true";
                            break;
                        case "Exception4":
                            attribute.IsExceptionInfantSeat = "true";
                            break;
                        case "OverSize":
                            attribute.IsOverSized = "true";
                            break;
                        case "OverWeight1":
                            attribute.IsOverWeight1 = "true";
                            break;
                        case "OverWeight2":
                            attribute.IsOverWeight2 = "true";
                            break;
                        case "ExceptionHockeySticks":
                            attribute.IsExceptionHockeySticks = "true";
                            break;
                        case "ExceptionCarryOn":
                            attribute.IsExceptionCarryOn = "true";
                            break;
                        case "Ski":
                            attribute.MiscBagID = item.MiscCode;
                            break;
                        default:
                            return acc;
                    }

                    if (isFreeCarryon)
                    {
                        attribute.BagID = bagId++;
                        acc.Add(attribute);
                    }
                    else
                    {
                        Enumerable.Range(1, item.Count).ToList().ForEach(x =>
                        {
                            var newAttribute = attribute.Clone();
                            newAttribute.BagID = bagId++;
                            acc.Add(newAttribute);
                        });
                    }

                    return acc;
                });
            }
            else
            {

                if (bag.ExceptionItemInfantSeat > 0)
                    Enumerable.Range(1, bag.ExceptionItemInfantSeat).ToList().ForEach(x => bagAttributes.Add(new Service.Presentation.EServiceCheckInModel.BagAttributeInfo() { BagID = bagId++, BagTransactionType = Service.Presentation.EServiceCheckInModel.BagTransactionType.Additional, IsExceptionItem1 = "true" }));

                if (bag.ExceptionItemInfantStroller > 0)
                    Enumerable.Range(1, bag.ExceptionItemInfantStroller).ToList().ForEach(x => bagAttributes.Add(new Service.Presentation.EServiceCheckInModel.BagAttributeInfo() { BagID = bagId++, BagTransactionType = Service.Presentation.EServiceCheckInModel.BagTransactionType.Additional, IsExceptionItem1 = "true" }));

                if (bag.ExceptionItemWheelChair > 0)
                    Enumerable.Range(1, bag.ExceptionItemWheelChair).ToList().ForEach(x => bagAttributes.Add(new Service.Presentation.EServiceCheckInModel.BagAttributeInfo() { BagID = bagId++, BagTransactionType = Service.Presentation.EServiceCheckInModel.BagTransactionType.Additional, IsExceptionItem2 = "true" }));

                if (bag.ExceptionItemSkiBoots > 0)
                    Enumerable.Range(1, bag.ExceptionItemSkiBoots).ToList().ForEach(x => bagAttributes.Add(new Service.Presentation.EServiceCheckInModel.BagAttributeInfo() { BagID = bagId++, BagTransactionType = Service.Presentation.EServiceCheckInModel.BagTransactionType.Additional, IsExceptionItem3 = "true" }));

                if (bag.ExceptionItemHockeySticks > 0)
                    Enumerable.Range(1, bag.ExceptionItemHockeySticks).ToList().ForEach(x => bagAttributes.Add(new Service.Presentation.EServiceCheckInModel.BagAttributeInfo() { BagID = bagId++, BagTransactionType = Service.Presentation.EServiceCheckInModel.BagTransactionType.Additional, IsExceptionHockeySticks = "true" }));

                if (bag.OverWeightBags > 0)
                    Enumerable.Range(1, bag.OverWeightBags).ToList().ForEach(x => bagAttributes.Add(new Service.Presentation.EServiceCheckInModel.BagAttributeInfo() { BagID = bagId++, BagTransactionType = Service.Presentation.EServiceCheckInModel.BagTransactionType.Additional, IsOverWeight1 = "true" }));

                if (bag.OverWeight2Bags > 0)
                    Enumerable.Range(1, bag.OverWeight2Bags).ToList().ForEach(x => bagAttributes.Add(new Service.Presentation.EServiceCheckInModel.BagAttributeInfo() { BagID = bagId++, BagTransactionType = Service.Presentation.EServiceCheckInModel.BagTransactionType.Additional, IsOverWeight2 = "true" }));

                if (bag.OverSizeBags > 0)
                    Enumerable.Range(1, bag.OverSizeBags).ToList().ForEach(x => bagAttributes.Add(new Service.Presentation.EServiceCheckInModel.BagAttributeInfo() { BagID = bagId++, BagTransactionType = Service.Presentation.EServiceCheckInModel.BagTransactionType.Additional, IsOverSized = "true" }));

                if (bag.ExceptionFreeCheckedCarryon == 1)
                    bagAttributes.Add(new Service.Presentation.EServiceCheckInModel.BagAttributeInfo() { BagID = bagId++, BagTransactionType = Service.Presentation.EServiceCheckInModel.BagTransactionType.Additional, IsExceptionCarryOn = "true" });
            }
            var normalBagCount = bag.TotalBags - bagAttributes.Count;
            if (normalBagCount > 0)
                Enumerable.Range(1, normalBagCount).ToList().ForEach(x => bagAttributes.Add(new Service.Presentation.EServiceCheckInModel.BagAttributeInfo() { BagID = bagId++, BagTransactionType = Service.Presentation.EServiceCheckInModel.BagTransactionType.Additional }));

            return bagAttributes;
        }

        private RegisterCheckInProductOfferRequest BuildRegisterBagDetailsRequest(MOBRegisterBagDetailsRequest request, ReservationDetail reservationDetail, bool isEnableMFOPCheckinBags = false)
        {
            RegisterCheckInProductOfferRequest registerBagsRequest = CreateRegisterCheckInProductOfferRequest(request, reservationDetail, isEnableMFOPCheckinBags);
            registerBagsRequest.ProductCode = "BAG";
            registerBagsRequest.ProductDescription = "Bags";

            if (!string.IsNullOrWhiteSpace(request.MilitaryTravelType))
            {
                Service.Presentation.EServiceCheckInModel.BagOverrideType bagOverrideType = Service.Presentation.EServiceCheckInModel.BagOverrideType.None;
                switch (request.MilitaryTravelType)
                {
                    case "MilitaryOfficialOrders":
                        bagOverrideType = Service.Presentation.EServiceCheckInModel.BagOverrideType.MilitaryOfficialOrders;
                        break;
                    case "MilitaryPersonalTravel":
                        bagOverrideType = Service.Presentation.EServiceCheckInModel.BagOverrideType.MilitaryPersonalTravel;
                        break;
                }

                if (bagOverrideType != Service.Presentation.EServiceCheckInModel.BagOverrideType.None)
                {
                    registerBagsRequest.BagOverRideRequest = new Collection<Service.Presentation.EServiceCheckInModel.BagOverride>() { new Service.Presentation.EServiceCheckInModel.BagOverride() { Type = bagOverrideType } };
                }
            }

            registerBagsRequest.Travelers = request.BagDetails.Select(bag => new Service.Presentation.EServiceCheckInModel.Traveler()
            {
                FirstNameNumber = Convert.ToInt32(bag.CustomerId.Substring(4, 3)),
                SurnameNumber = Convert.ToInt32(bag.CustomerId.Substring(1, 3)),
                OperationType = Service.Presentation.EServiceCheckInModel.CustomerOperationType.SetBag,
                BagState = new Service.Presentation.EServiceCheckInModel.BagState()
                {
                    BagCount = new Service.Presentation.EServiceCheckInModel.BagCount()
                    {
                        Counts = new Collection<Count> { new Count() { CountType = "BagCount", Value = bag.TotalBags.ToString() } }
                    },
                    BagAttributesInfo = PopulateBagDetailAttributesInfo(bag)
                }
            }).ToCollection();

            return registerBagsRequest;
        }

        private Collection<Service.Presentation.EServiceCheckInModel.BagAttributeInfo> PopulateBagDetailAttributesInfo(BagDetail bag)
        {
            var bagAttributes = new Collection<Service.Presentation.EServiceCheckInModel.BagAttributeInfo>();

            var bagId = 1;

            if (bag.BagItems?.Any() ?? false)
            {
                bagAttributes = bag.BagItems.Aggregate(bagAttributes, (acc, item) =>
                {
                    GetBagItemStatus(item, out bool isFreeCarryon, out bool interactedWithFreeCarryon, out bool isOtherItem);

                    if (!(isFreeCarryon || isOtherItem)) { return acc; }

                    var attribute = new Service.Presentation.EServiceCheckInModel.BagAttributeInfo() { BagTransactionType = Service.Presentation.EServiceCheckInModel.BagTransactionType.Additional };

                    if (item.Code.ToLower() == "standard" && item.Count > 0)
                    {
                        Enumerable.Range(1, item.Count).ToList().ForEach(x => bagAttributes.Add(new Service.Presentation.EServiceCheckInModel.BagAttributeInfo() { BagID = bagId++, BagTransactionType = Service.Presentation.EServiceCheckInModel.BagTransactionType.Additional }));
                    }

                    if (isFreeCarryon)
                    {
                        attribute.BagID = bagId++;
                        attribute.MiscBagID = item.MiscCode;
                        acc.Add(attribute);
                    }
                    else
                    {
                        if (item.Count > 0 && item.BagAttributes != null && item.BagAttributes.Count > 0)
                        {
                            Enumerable.Range(0, item.Count).ToList().ForEach(x =>
                            {
                                var newAttribute = attribute.Clone();
                                newAttribute.BagID = bagId++;
                                newAttribute.MiscBagID = item.MiscCode;
                                newAttribute.IsOverWeight1 = item.BagAttributes[x].isOverweight1 ? "true" : "false";
                                newAttribute.IsOverWeight2 = item.BagAttributes[x].isOverweight2 ? "true" : "false";
                                newAttribute.IsOverSized = item.BagAttributes[x].isOversize ? "true" : "false";
                                newAttribute.IsExceptionItem3 = item.BagAttributes[x].hasBootItem ? "true" : "false";
                                acc.Add(newAttribute);
                            });
                        }
                    }

                    return acc;
                });
            }

            return bagAttributes;
        }

        private List<MOBTypeOption> GetBagsLineItems(MOBBag bagCount)
        {
            List<MOBTypeOption> lineItems = new List<MOBTypeOption>();

            lineItems.Add(new MOBTypeOption() { Key = "Total bags:", Value = $"{bagCount.TotalBags} bag{(bagCount.TotalBags > 1 ? "s" : String.Empty)}" });

            var useSpecialityItems = bagCount.SpecialtyItems?.Any() ?? false;

            Func<string, int, int> getItemCount = (code, propertyCount) =>
            {
                var specialityItemCount = bagCount.SpecialtyItems?.FirstOrDefault(item => String.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase))?.Count ?? 0;
                var count = useSpecialityItems ? specialityItemCount : propertyCount;
                return count;
            };

            var owCount = getItemCount("OverWeight1", bagCount.OverWeightBags);
            if (owCount > 0)
                lineItems.Add(new MOBTypeOption() { Key = "Bags over 50 lbs:", Value = $"{owCount} bag{(owCount > 1 ? "s" : String.Empty)}" });

            var ow2Count = getItemCount("OverWeight2", bagCount.OverWeight2Bags);
            if (ow2Count > 0)
                lineItems.Add(new MOBTypeOption() { Key = "Bags over 70 lbs:", Value = $"{ow2Count} bag{(ow2Count > 1 ? "s" : String.Empty)}" });

            var osCount = getItemCount("OverSize", bagCount.OverSizeBags);
            if (osCount > 0)
                lineItems.Add(new MOBTypeOption() { Key = "Bags over 60 in:", Value = $"{osCount} bag{(osCount > 1 ? "s" : String.Empty)}" });


            int totalExceptionBags = bagCount.ExceptionItemHockeySticks + bagCount.ExceptionItemInfantSeat + bagCount.ExceptionItemInfantStroller + bagCount.ExceptionItemSkiBoots + bagCount.ExceptionItemWheelChair;
            int specialityItemExceptions = (new List<string>() { "Exception1", "Exception2", "Exception3", "Exception4", "ExceptionHockeySticks" }).Sum(exType => getItemCount(exType, 0));
            int exceptionCount = useSpecialityItems ? specialityItemExceptions : totalExceptionBags;

            if (exceptionCount > 0)
                lineItems.Add(new MOBTypeOption() { Key = "Exception items:", Value = $"{exceptionCount} bag{(exceptionCount > 1 ? "s" : String.Empty)}" });

            return lineItems;
        }

        private List<MOBTypeOption> GetBagDetailsLineItems(BagDetail bagCount)
        {
            List<MOBTypeOption> lineItems = new List<MOBTypeOption>();

            lineItems.Add(new MOBTypeOption() { Key = "Total bags:", Value = $"{bagCount.TotalBags} bag{(bagCount.TotalBags > 1 ? "s" : String.Empty)}" });

            var useSpecialityItems = bagCount.BagItems?.Any() ?? false;

            Func<string, int, int> getItemCount = (code, propertyCount) =>
            {
                var specialityItemCount = bagCount.BagItems?.FirstOrDefault(item => String.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase))?.Count ?? 0;
                var count = useSpecialityItems ? specialityItemCount : propertyCount;
                return count;
            };

            return lineItems;
        }

        private void GetBagItemStatus(BagItem item, out bool isFreeCarryon, out bool interactedWithFreeCarryon, out bool isOtherItem)
        {
            isFreeCarryon = String.Equals(item.Code, "ExceptionCarryOn", StringComparison.OrdinalIgnoreCase);
            interactedWithFreeCarryon = isFreeCarryon && (item.Count == 1 || item.Count == -1);
            isOtherItem = !isFreeCarryon && item.Count > 0;
        }

        private BagDetail BagDetailsCountFolder(BagDetail acc, BagDetail customerBags)
        {
            acc.TotalBags += customerBags.TotalBags;
            if (customerBags.BagItems?.Any() ?? false)
            {
                acc.BagItems = acc.BagItems == null ? new List<BagItem>() : acc.BagItems;

                acc.BagItems = customerBags.BagItems.Aggregate(acc.BagItems, (accc, item) => { accc.Add(item); return accc; })
                                                                .GroupBy(item => item.Code)
                                                                .Select(grouping => new BagItem() { Code = grouping.Key, Count = grouping.Sum(g => g.Count), MiscCode = grouping.First().MiscCode })
                                                                .ToList();
            }
            return acc;
        }

        private bool GetIsMilesFOPEnabled(MOBShoppingCart shoppingCart)
        {
            Int32 j;
            Int32.TryParse(shoppingCart.TotalMiles, out j);
            return j > 0;
        }

        public bool IsValidFOPForTPIpayment(string cardType)
        {
            return !string.IsNullOrEmpty(cardType) &&
                (cardType.ToUpper().Trim() == "VI" || cardType.ToUpper().Trim() == "MC" || cardType.ToUpper().Trim() == "AX" || cardType.ToUpper().Trim() == "DS");
        }

        private void GetItemStatus(SpecialtyItem item, out bool isFreeCarryon, out bool interactedWithFreeCarryon, out bool isOtherItem)
        {
            isFreeCarryon = String.Equals(item.Code, "ExceptionCarryOn", StringComparison.OrdinalIgnoreCase);
            interactedWithFreeCarryon = isFreeCarryon && (item.Count == 1 || item.Count == -1);
            isOtherItem = !isFreeCarryon && item.Count > 0;
        }

        private Collection<Count> PopulateBagCounts(MOBBag bag)
        {
            var counts = new Collection<Count>();

            counts.Add(new Count() { CountType = "BagCount", Value = bag.TotalBags.ToString() });

            if (bag.SpecialtyItems?.Any() ?? false)
            {
                counts = bag.SpecialtyItems.Aggregate(counts, (acc, item) =>
                {
                    if (item.MiscCode != -1 && item.Count > 0)
                    {
                        var miscCount = acc.FirstOrNew(count => String.Equals(count.CountType, "MiscBag", StringComparison.OrdinalIgnoreCase), new Count() { CountType = "MiscBag", Value = "" });
                        miscCount.Value = String.IsNullOrEmpty(miscCount.Value) ? $"{item.MiscCode}" : $"{miscCount.Value},{item.MiscCode}";
                        acc.RemoveWhere(count => String.Equals(count.CountType, "MiscBag", StringComparison.OrdinalIgnoreCase));
                        acc.Add(miscCount);
                        return acc;
                    }

                    GetItemStatus(item, out bool isFreeCarryon, out bool interactedWithFreeCarryon, out bool isOtherItem);

                    if (interactedWithFreeCarryon || isOtherItem) { acc.Add(new Count() { CountType = item.Code, Value = $"{item.Count}" }); }
                    return acc;
                });
            }
            else
            {

                if (bag.ExceptionItemInfantStroller > 0 || bag.ExceptionItemInfantSeat > 0)
                    counts.Add(new Count() { CountType = "Exception1", Value = (bag.ExceptionItemInfantStroller + bag.ExceptionItemInfantSeat).ToString() });

                if (bag.ExceptionItemWheelChair > 0)
                    counts.Add(new Count() { CountType = "Exception2", Value = bag.ExceptionItemWheelChair.ToString() });

                if (bag.ExceptionItemSkiBoots > 0)
                    counts.Add(new Count() { CountType = "Exception3", Value = bag.ExceptionItemSkiBoots.ToString() });

                if (bag.ExceptionItemHockeySticks > 0)
                    counts.Add(new Count() { CountType = "ExceptionHockeySticks", Value = bag.ExceptionItemHockeySticks.ToString() });

                if (bag.OverWeightBags > 0)
                    counts.Add(new Count() { CountType = "OverWeight1", Value = bag.OverWeightBags.ToString() });

                if (bag.OverWeight2Bags > 0)
                    counts.Add(new Count() { CountType = "OverWeight2", Value = bag.OverWeight2Bags.ToString() });

                if (bag.OverSizeBags > 0)
                    counts.Add(new Count() { CountType = "OverSize", Value = bag.OverSizeBags.ToString() });

                if (bag.ExceptionFreeCheckedCarryon == 1 || bag.ExceptionFreeCheckedCarryon == -1)
                    counts.Add(new Count() { CountType = "ExceptionCarryOn", Value = bag.ExceptionFreeCheckedCarryon.ToString() });
            }

            return counts;
        }

        private Service.Presentation.EServiceCheckInModel.CustomerOperationType GetBagsOperationType(Collection<SpecialtyItem> specialtyItems)
        {
            if (specialtyItems != null && specialtyItems.Count() > 0) { return Service.Presentation.EServiceCheckInModel.CustomerOperationType.SetBag2; }
            return Service.Presentation.EServiceCheckInModel.CustomerOperationType.SetBag;
        }

        private RegisterCheckInProductOfferRequest CreateRegisterCheckInProductOfferRequest(MOBShoppingRequest request, ReservationDetail reservationDetail, bool includeCalculateBagFee = false)
        {
            RegisterCheckInProductOfferRequest registerCheckInProductOfferRequest = new RegisterCheckInProductOfferRequest();
            registerCheckInProductOfferRequest.Reservation = reservationDetail.Detail;
            registerCheckInProductOfferRequest.CartId = request.CartId;
            registerCheckInProductOfferRequest.LangCode = request.LanguageCode;
            registerCheckInProductOfferRequest.CurrencyCode = "USD";
            registerCheckInProductOfferRequest.GUID = new UniqueIdentifier() { ID = request.CheckinSessionId, Type = "eToken" };
            registerCheckInProductOfferRequest.WorkFlowType = WorkFlowType.CheckInProductsPurchase;
            if (includeCalculateBagFee) registerCheckInProductOfferRequest.Characteristics = new Collection<Characteristic> { new Characteristic { Code = "CALCULATE_BAG_FEE", Value = "true" } };

            return registerCheckInProductOfferRequest;
        }

        private MOBBag BagCountFolder(MOBBag acc, MOBBag customerBags)
        {
            acc.TotalBags += customerBags.TotalBags;
            acc.OverWeightBags += customerBags.OverWeightBags;
            acc.OverWeight2Bags += customerBags.OverWeight2Bags;
            acc.OverSizeBags += customerBags.OverSizeBags;
            acc.ExceptionItemInfantSeat += customerBags.ExceptionItemInfantSeat;
            acc.ExceptionItemHockeySticks += customerBags.ExceptionItemHockeySticks;
            acc.ExceptionItemInfantStroller += customerBags.ExceptionItemInfantStroller;
            acc.ExceptionItemSkiBoots += customerBags.ExceptionItemSkiBoots;
            acc.ExceptionItemWheelChair += customerBags.ExceptionItemWheelChair;

            if (customerBags.SpecialtyItems?.Any() ?? false)
            {
                acc.SpecialtyItems = acc.SpecialtyItems == null ? new Collection<SpecialtyItem>() : acc.SpecialtyItems;

                acc.SpecialtyItems = customerBags.SpecialtyItems.Aggregate(acc.SpecialtyItems, (accc, item) => { accc.Add(item); return accc; })
                                                                .GroupBy(item => item.Code)
                                                                .Select(grouping => new SpecialtyItem() { Code = grouping.Key, Count = grouping.Sum(g => g.Count), MiscCode = grouping.First().MiscCode })
                                                                .ToCollection();
            }
            return acc;
        }

        private async Task<(MOBRegisterOfferResponse response, bool didPurchase)> RegisterCheckinSeats(MOBRegisterCheckinSeatsRequest request, Session session, MOBRegisterOfferResponse response)
        {
            var shopping = new Shopping();
            bool didPurchase;
            bool isMFOPEnabled = isMilesFOPEnabledForCheckIn(request.Application.Id, request.Application.Version.Major);

            FlightReservationResponse flightReservationResponse = await RegisterCheckinSeats(request, session, isMFOPEnabled);
            var tupleres = await PopulateRegisterResponse(request, response, session, flightReservationResponse, shopping, isMFOPEnabled, "Seats", request);
            didPurchase = tupleres.didPurchase;
            response = tupleres.response;

            return (response, didPurchase);
        }
        public bool isMilesFOPEnabledForCheckIn(int appId, string appVersion)
        {
            return !_configuration.GetValue<bool>("DisableMilesFOP") &&
                   GeneralHelper.isApplicationVersionGreater(appId, appVersion, "AndroidMilesFOPVersion", "iPhoneMilesFOPVersion", "", "", true, _configuration);
        }
        private async Task<(MOBRegisterOfferResponse response, bool didPurchase)> PopulateRegisterResponse(MOBShoppingRequest request, MOBRegisterOfferResponse response, Session session, FlightReservationResponse flightReservationResponse, Shopping shopping, bool isMFOPEnabled = false, string product = null, MOBRegisterCheckinSeatsRequest registerCheckInRequest = null)
        {
            bool didPurchase = false;
            decimal price = 0;
            string currencySymbol = "$";

            TravelOption travelOption = flightReservationResponse.DisplayCart.TravelOptions?.FirstOrDefault();

            if (travelOption != null)
            {
                price = flightReservationResponse.DisplayCart.TravelOptions.Sum(x => x.Amount);
                string[] currencies = travelOption.Currency.Split('-');
                currencySymbol = currencies.Length == 2 ? currencies[1] : currencySymbol;
            }
            bool milesFOPSelected = false;
            decimal totalSeatPrice = 0;
            int totalSeatMiles = 0;

            if (isMFOPEnabled && flightReservationResponse != null && flightReservationResponse.DisplayCart != null
                && flightReservationResponse.DisplayCart.DisplaySeats != null)
            {
                foreach (var seatAssignment in flightReservationResponse.DisplayCart.DisplaySeats)
                {
                    if (registerCheckInRequest.IsMiles)
                    {
                        if (seatAssignment.Currency == "ML1")
                        {
                            milesFOPSelected = true;
                            totalSeatPrice = totalSeatPrice + (seatAssignment.SeatPrice != 0 ? seatAssignment.MoneyAmount : 0);
                            totalSeatMiles = totalSeatMiles + Convert.ToInt32(seatAssignment.SeatPrice);
                        }
                    }
                    else
                    {
                        if (seatAssignment.Currency == "ML1")
                        {
                            milesFOPSelected = true;
                            totalSeatMiles = totalSeatMiles + Convert.ToInt32(seatAssignment.SeatPrice);
                        }
                        else
                        {
                            totalSeatPrice = totalSeatPrice + seatAssignment.SeatPrice;
                        }
                    }
                }
            }

            string strPrice = isMFOPEnabled && milesFOPSelected ? String.Format("{0:0.00}", totalSeatPrice) : String.Format("{0:0.00}", price);
            response.ShoppingCart.TotalPrice = strPrice;
            response.ShoppingCart.DisplayTotalPrice = $"{currencySymbol}{strPrice}";
            response.ShoppingCart.Products = await _shoppingCartUtility.ConfirmationPageProductInfo(flightReservationResponse, false, false, request.Flow, request.Application, sessionId: request.SessionId).ConfigureAwait(false);
            response.ShoppingCart.TermsAndConditions = new List<MOBMobileCMSContentMessages>();
            response.ShoppingCart.PaymentTarget = GetPaymentTargetForRegisterFop(flightReservationResponse.DisplayCart.TravelOptions, false);
            response.ShoppingCart.CartId = flightReservationResponse.CartId.ToString();
            response.PkDispenserPublicKey = null;
            response.ShoppingCart.Flow = request.Flow;

            if (isMFOPEnabled && product == "Seats")
            {
                await BuildMilesFOP(response.ShoppingCart, registerCheckInRequest, session.Token);
            }
            await _sessionHelperService.SaveSession<MOBShoppingCart>(response.ShoppingCart, request.SessionId, new List<string> { request.SessionId, response.ShoppingCart.ObjectName }, response.ShoppingCart.ObjectName).ConfigureAwait(false);

            if ((isMFOPEnabled && !registerCheckInRequest.IsMiles && milesFOPSelected && totalSeatMiles == 0 && totalSeatPrice == 0)
          || price == 0)
            {
                MOBCheckOutRequest checkOutRequest = new MOBCheckOutRequest()
                {
                    SessionId = request.SessionId,
                    CartId = request.CartId,
                    Application = request.Application,
                    AccessCode = request.AccessCode,
                    LanguageCode = request.LanguageCode,
                    Flow = request.Flow,
                    FormofPaymentDetails = new MOBFormofPaymentDetails()
                    {
                        FormOfPaymentType = null
                    }
                };

                MOBCheckOutResponse checkOutResponse = await _checkoutUtiliy.CheckOut(checkOutRequest);
                if (checkOutResponse.Exception != null) { response.Exception = checkOutResponse.Exception; }
                didPurchase = true;
            }
            return (response, didPurchase);
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

        private async Task<FlightReservationResponse> RegisterCheckinSeats(MOBRegisterCheckinSeatsRequest request, Session session, bool isMFOPEnabled = false)
        {
            var response = new FlightReservationResponse();
            string actionName = "registerseats";
            var reservationDetail = new ReservationDetail();
            reservationDetail = await _sessionHelperService.GetSession<ReservationDetail>(request.SessionId, reservationDetail.GetType().FullName, new List<string> { request.SessionId, reservationDetail.GetType().FullName }).ConfigureAwait(false);

            RegisterSeatsRequest registerSeatsRequest = BuildCheckinSeatRequest(request.CheckinSessionId, reservationDetail, request.CartId, request.LastSeatSelected, request.SelectedSeats, isMFOPEnabled, request.IsMiles);
            string jsonRequest = JsonConvert.SerializeObject(registerSeatsRequest);
            string cslResponse = await _shoppingCartService.RegisterCheckinSeats(session.Token, actionName, jsonRequest, session.SessionId).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(cslResponse))
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));

            response = JsonConvert.DeserializeObject<FlightReservationResponse>(cslResponse);

            if (response == null || response.Status.Equals(United.Services.FlightShopping.Common.StatusType.Failure))
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));

            if (response.Errors != null && response.Errors.Count > 0)
            {
                string errorMessage = string.Empty;
                response.Errors.ForEach(errorInfo => errorMessage += (errorInfo.Message + " "));
                throw new Exception(errorMessage);
            }

            return response;
        }

        private RegisterSeatsRequest BuildCheckinSeatRequest(string checkinGuid, ReservationDetail reservationDetail, string cartId, List<MOBCheckinSeatDetail> lastSelectedSeats, List<MOBCheckinSeatDetail> selectedSeats, bool isMilesFOPEnabled = false, bool isMilesFOPSelected = false)
        {
            RegisterSeatsRequest request = new RegisterSeatsRequest();
            request.CartId = cartId;
            request.Reservation = reservationDetail.Detail;
            request?.Reservation?.Travelers?.ForEach(pax =>
            {
                if (pax != null)
                {
                    pax.TravelPolicies = null;
                }
            });
            request.RecordLocator = reservationDetail.Detail.ConfirmationID;
            request.WorkFlowType = WorkFlowType.CheckInProductsPurchase;
            request.SeatAssignments = MergeSeatDetailLists(lastSelectedSeats, selectedSeats).Select(x => GenerateSeatAssignment(x, isMilesFOPEnabled: isMilesFOPEnabled, isMilesFOPSelected: isMilesFOPSelected)).OrderBy(s => s.OriginalSegmentIndex).ThenBy(s => s.LegIndex).ToList();
            request.Characteristics = new Collection<Characteristic>() { new Characteristic() { Code = "etoken", Value = checkinGuid },
                                                                         new Characteristic() { Code = "ManageRes", Value = "true" },
                                                                         new Characteristic() { Code = "CALCULATE_SEAT_FEE", Value = "true" }
                                                                       };

            return request;
        }

        private SeatAssignment GenerateSeatAssignment(MOBCheckinSeatDetail seatDetail, bool isMilesFOPEnabled = false, bool isMilesFOPSelected = false)
        {
            IEnumerable<char> indexes = seatDetail.LegId.Where(x => x != '0');
            decimal seatPrice = seatDetail.SeatPrice;
            if (_configuration.GetValue<bool>("CheckinSeatPriceStrikeOffIssueFixToggle"))
                seatPrice = Convert.ToDecimal(String.Format("{0:0.00}", seatPrice));

            SeatAssignment seatAssignment = new SeatAssignment();
            seatAssignment.OriginalSegmentIndex = Convert.ToInt32(indexes.First().ToString());
            seatAssignment.LegIndex = Convert.ToInt32(indexes.Last().ToString());
            seatAssignment.ArrivalAirportCode = seatDetail.Destination;
            seatAssignment.Currency = "USD";
            seatAssignment.DepartureAirportCode = seatDetail.Origin;
            seatAssignment.FlightNumber = seatDetail.FlightNumber;
            seatAssignment.OriginalPrice = seatPrice;
            seatAssignment.SeatPrice = seatPrice;
            seatAssignment.PersonIndex = seatDetail.CustomerId;
            seatAssignment.Seat = seatDetail.SeatNumber;
            seatAssignment.SeatPromotionCode = seatDetail.SeatPromotionCode;
            seatAssignment.SeatType = seatDetail.SeatType;
            if (isMilesFOPEnabled)
            {
                seatAssignment.MoneyAmount = seatPrice;
                seatAssignment.MilesAmount = seatDetail.MilesFee;
                seatAssignment.Currency = string.Empty;

                if (isMilesFOPSelected)
                {
                    seatAssignment.Currency = "ML1";
                    var temp = seatAssignment.MilesAmount;
                    seatAssignment.SeatPrice = (decimal)temp;
                }
            }
            return seatAssignment;
        }

        private List<MOBCheckinSeatDetail> MergeSeatDetailLists(List<MOBCheckinSeatDetail> lastSelectedSeats, List<MOBCheckinSeatDetail> selectedSeats)
        {
            Func<MOBCheckinSeatDetail, MOBCheckinSeatDetail, bool> doesMatch = (x, y) => x.Origin == y.Origin
                                                                                         && x.Destination == y.Destination
                                                                                         && x.CustomerId == y.CustomerId;
            if (selectedSeats.IsNullOrEmpty()) { return lastSelectedSeats; }
            if (lastSelectedSeats.IsNullOrEmpty()) { return selectedSeats; }

            return selectedSeats.Where(x => lastSelectedSeats.All(y => !doesMatch(x, y)))
                                .Concat(lastSelectedSeats)
                                .ToList();
        }

        public async System.Threading.Tasks.Task GetAndSetEligibleFormOfPayments(MOBShoppingRequest request, Session session, MOBRegisterOfferResponse response, bool isMFOPEnabled = false)
        {
            if (_configuration.GetValue<bool>("GetFoPOptionsAlongRegisterOffers"))
            {
                bool isDefault = false;
                MOBFOPEligibilityRequest eligiblefopRequest = new MOBFOPEligibilityRequest()
                {
                    TransactionId = request.TransactionId,
                    DeviceId = request.DeviceId,
                    AccessCode = request.AccessCode,
                    LanguageCode = request.LanguageCode,
                    Application = request.Application,
                    CartId = request.CartId,
                    SessionId = request.SessionId,
                    Flow = request.Flow,
                    Products = await GetProductsForEligibleFopRequest(response.ShoppingCart).ConfigureAwait(false),
                    PointOfSale = GetPointOfSale(response.ShoppingCart)
                };

                var tupleEligibleFormofPayments = await _mscFormsOfPayment.EligibleFormOfPayments(eligiblefopRequest, session, isMFOPEnabled).ConfigureAwait(false);
                response.EligibleFormofPayments = tupleEligibleFormofPayments.EligibleFormofPayments;
                isDefault = tupleEligibleFormofPayments.isDefault;

                response.IsDefaultPaymentOption = isDefault;
                await _sessionHelperService.SaveSession<List<FormofPaymentOption>>(response.EligibleFormofPayments, request.SessionId, new List<string> { request.SessionId, new FormofPaymentOption().GetType().ToString() }, new FormofPaymentOption().GetType().ToString()).ConfigureAwait(false);
            }
        }

        private string GetPointOfSale(MOBShoppingCart shoppingCart)
        {
            if (shoppingCart == null || string.IsNullOrEmpty(shoppingCart.PointofSale))
                return "US";
            else
                return shoppingCart.PointofSale;
        }

        private string GetPointOfSale(string currencyCode)
        {
            return (currencyCode == "CAD") ? "CA" : (currencyCode == "JPY") ? "JP" : "US";
        }

        private async Task<Collection<FOPProduct>> GetProductsForEligibleFopRequest(MOBShoppingCart shoppingCart, SeatChangeState state = null)
        {
            if (shoppingCart == null || shoppingCart.Products == null || !shoppingCart.Products.Any())
                return null;

            var products = shoppingCart.Products.GroupBy(k => new { k.Code, k.ProdDescription }).Select(x => new FOPProduct { Code = x.Key.Code, ProductDescription = x.Key.ProdDescription }).ToCollection();
            if (!string.IsNullOrEmpty(_configuration.GetValue<string>("EnablePCUSelectedSeatPurchaseViewRes")))
            {
                if (!_configuration.GetValue<bool>("ByPassAddingPCUProductToEligibleFopRequest"))
                {
                    products = await AddPCUToRequestWhenPCUSeatIsSelected(state, products, shoppingCart.Flow).ConfigureAwait(false);
                }
            }

            return products;
        }

        private async Task<Collection<FOPProduct>> AddPCUToRequestWhenPCUSeatIsSelected(SeatChangeState state, Collection<FOPProduct> products, string flow)
        {
            if (!await _shoppingCartUtility.IsCheckInFlow(flow))
            {
                products = new Collection<FOPProduct>();
            }
            if (state != null && state.BookingTravelerInfo.Any(t => t.Seats.Any(s => !string.IsNullOrEmpty(s.PcuOfferOptionId) && s.PriceAfterTravelerCompanionRules > 0)))
            {
                if (!products.Any(p => p.Code == "PCU"))
                {
                    products.Add(new FOPProduct { Code = "PCU", ProductDescription = "Premium Cabin Upsell" });
                }
            }
            return products;
        }

        private async Task<MOBRegisterOfferResponse> RegisterOffersForPostBooking(MOBRegisterOfferRequest request, Session session)
        {
            MOBRegisterOfferResponse response = new MOBRegisterOfferResponse();
            FlightReservationResponse flightReservationResponse = null;

            MOBShoppingCart persistedShoppingCart = new MOBShoppingCart();

            //persistedShoppingCart = FilePersist.Load<MOBShoppingCart>(request.SessionId, persistedShoppingCart.GetType().ToString());
            persistedShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, persistedShoppingCart.ObjectName, new List<string> { request.SessionId, persistedShoppingCart.ObjectName }).ConfigureAwait(false);
            var persistedProducts = new List<MerchandizingOfferDetails>();
            if (persistedShoppingCart != null && persistedShoppingCart.Products?.Count() > 0 && persistedShoppingCart.Products?.Where(x => Constants.PostBookingFlow_ProductMapping.Split(',').ToList().Contains(x.Code)).Count() > 0)
            {
                foreach (var product in persistedShoppingCart.Products)
                {
                    if (product != null && !(request.MerchandizingOfferDetails.Any(x => x.ProductCode == product.Code && x.ProductIds.Any(k => ((product.Segments != null) && product.Segments.Select(y => y.ProductIds).ToList().Equals(k)))))) // x.ProductIds.ToList().Except(product.Segments.Select(y=>y.ProductIds)).Any())))
                    {
                        var products = new MerchandizingOfferDetails
                        {
                            ProductCode = product.Code,
                            TripIds = product.Segments?.Select(y => y.TripId).SelectMany(y => y.Split(',').ToList()).ToList(),
                            ProductIds = product.Segments?.SelectMany(y => y.ProductIds).ToList(), //.Except(request.MerchandizingOfferDetails.Where(y => y.ProductCode == x.Code).SelectMany(y => y.ProductIds).ToList()).ToList(),  //GetProductIds(x.Segments.SelectMany(y=>y.ProductIds).ToList(), request.MerchandizingOfferDetails.Where(y=>y.ProductCode == x.Code).SelectMany(y=>y.ProductIds).ToList()), // x.Segments.Select(y=>y.ProductIds).FirstOrDefault(),
                            SelectedTripProductIDs = product.Segments?.Select(y => y.ProductId).ToList(),
                            IsOfferRegistered = true
                        };
                        persistedProducts.Add(products);

                    }
                }
                persistedProducts.AddRange(request.MerchandizingOfferDetails);
                request.MerchandizingOfferDetails = new Collection<MerchandizingOfferDetails>(persistedProducts);
            }

            var tupleResponse = await RegisterOffers(request, session, false);
            response = tupleResponse.registerOfferResponse;
            flightReservationResponse = tupleResponse.flightReservationResponse;

            if (session != null && !string.IsNullOrEmpty(session.EmployeeId))
            {
                response.IsEmp20 = true;
            }

            return response;
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

        private async Task<MOBRegisterOfferResponse> RegisterOffers(MOBRegisterOfferRequest request, Session session)
        {
            MOBRegisterOfferResponse response = new MOBRegisterOfferResponse();
            FlightReservationResponse flightReservationResponse = null;
            var tupleRegisterOfferResponse = await RegisterOffers(request, session, false);
            response = tupleRegisterOfferResponse.registerOfferResponse;
            flightReservationResponse = tupleRegisterOfferResponse.flightReservationResponse;
            return response;
        }

        private async Task<(MOBRegisterOfferResponse registerOfferResponse, FlightReservationResponse flightReservationResponse)> RegisterOffers(MOBRegisterOfferRequest request, Session session, bool temp)
        {
            MOBRegisterOfferResponse response = new MOBRegisterOfferResponse();
            FlightReservationResponse flightReservationResponse = null;
            var productOffer = new GetOffers();
            var productVendorOffer = new GetVendorOffers();
            var reservationDetail = new ReservationDetail();
            var productFareLockOffer = new ProductOffer();
            var persistedBagOfferFromCce = new GetCceBagOffers();

            if (!(IsEnableOmniCartReleaseCandidateTwoChanges_Bundles(request.Application.Id, request.Application.Version.Major) && request.IsOmniCartSavedTripFlow)
                || ConfigUtility.IsViewResFlowPaymentSvcEnabled(request.Flow))
            {
                response.SessionId = request.SessionId;
                response.TransactionId = request.TransactionId;
                response.Flow = request.Flow;

                if (request.MerchandizingOfferDetails.Count() == 0)
                {
                    response.Exception = new MOBException("9999", _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                    return (response, flightReservationResponse);
                }

                if (!UtilityHelper.IsCheckinFlow(request.Flow) && !ConfigUtility.IsViewResFlowPaymentSvcEnabled(request.Flow))
                {
                    ShopBookingDetailsResponse detailResponse = new ShopBookingDetailsResponse();
                    detailResponse = await _sessionHelperService.GetSession<ShopBookingDetailsResponse>(request.SessionId, detailResponse.ObjectName, new List<string> { request.SessionId, detailResponse.ObjectName }).ConfigureAwait(false);
                    productFareLockOffer = detailResponse?.FareLock;
                }

                productOffer = await _sessionHelperService.GetSession<GetOffers>(session.SessionId, productOffer.ObjectName, new List<string> { session.SessionId, productOffer.ObjectName }).ConfigureAwait(false);
                persistedBagOfferFromCce = await _sessionHelperService.GetSession<GetCceBagOffers>(session.SessionId, persistedBagOfferFromCce.ObjectName, new List<string> { session.SessionId, persistedBagOfferFromCce.ObjectName }).ConfigureAwait(false);
                productVendorOffer = await _sessionHelperService.GetSession<GetVendorOffers>(session.SessionId, productVendorOffer.ObjectName, new List<string> { session.SessionId, productVendorOffer.ObjectName }).ConfigureAwait(false);
                reservationDetail = await _sessionHelperService.GetSession<ReservationDetail>(request.SessionId, reservationDetail.GetType().FullName, new List<string> { request.SessionId, reservationDetail.GetType().FullName }).ConfigureAwait(false);
            }

            var pomOffer = new DynamicOfferDetailResponse();
            var cceDODOfferResponse = new ProductOffer();
            if (ConfigUtility.IsViewResFlowPaymentSvcEnabled(request.Flow))
            {
                pomOffer = await GetCCEOffersFromPersist(request.SessionId);

                var DODOfferResponse = new GetOffersCce();
                if (IsEnableTravelOptionsInViewRes(request.Application.Id, request.Application.Version.Major, session?.CatalogItems))
                {
                    DODOfferResponse = await _sessionHelperService.GetSession<GetOffersCce>(request.SessionId, ObjectNames.CCEDynamicOfferDetailResponse, new List<string> { request.SessionId, ObjectNames.CCEDynamicOfferDetailResponse }).ConfigureAwait(false);
                }
                cceDODOfferResponse = string.IsNullOrEmpty(DODOfferResponse?.OfferResponseJson)
                                            ? null
                                            : JsonConvert.DeserializeObject<ProductOffer>(DODOfferResponse.OfferResponseJson);

            }

            var isCompleteFarelockPurchase = _configuration.GetValue<bool>("EnableFareLockPurchaseViewRes") && request.MerchandizingOfferDetails.Any(o => (o.ProductCode != null && o.ProductCode.Equals("FLK_VIEWRES", StringComparison.OrdinalIgnoreCase)));
            if (IsEnableOmniCartReleaseCandidateTwoChanges_Bundles(request.Application.Id, request.Application.Version.Major) && request.IsOmniCartSavedTripFlow)
            {
                flightReservationResponse = await GetFlightReservationResponseByCartId(session.SessionId, request.CartId);
            }
            else
            {
                flightReservationResponse = isCompleteFarelockPurchase ? await RegisterFareLockReservation(request, reservationDetail, session)
                                                        : await RegisterOffers(request, productOffer, productVendorOffer, persistedBagOfferFromCce, reservationDetail, session, productFareLockOffer, pomOffer, null, cceDODOfferResponse);
            }
            if (ConfigUtility.IncludeFFCResidual(request.Application.Id, request.Application.Version.Major) && request.Flow == FlowType.BOOKING.ToString())
            {
                response.ShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(session.SessionId, new MOBShoppingCart().ObjectName, new List<string> { session.SessionId, new MOBShoppingCart().ObjectName }).ConfigureAwait(false);
                if (response.ShoppingCart == null)
                {
                    response.ShoppingCart = new MOBShoppingCart();
                }
            }

            double price;
            if (ConfigUtility.IsViewResFlowPaymentSvcEnabled(request.Flow))
            {
                var flow = GetFlow(request.Flow, flightReservationResponse?.ShoppingCart?.Items);
                response.Flow = flow;
                response.ShoppingCart.Flow = flow;
                price = GetGrandTotalPriceForShoppingCart_ForViewRes(isCompleteFarelockPurchase, flightReservationResponse);
                response.ShoppingCart.DisplayMessage = await GetPaymentMessagesForWLPNRViewRes(flightReservationResponse, reservationDetail.Detail.FlightSegments, request.Flow);
            }
            else
            {
                price = GetGrandTotalPriceForShoppingCart(isCompleteFarelockPurchase, flightReservationResponse, false, request.Flow);
                response.ShoppingCart.Flow = request.Flow;
            }
            bool isEnableMFOPBags = await _featureSettings.GetFeatureSettingValue("EnableMfopForBags").ConfigureAwait(false)
                                        && (request.MerchandizingOfferDetails?.Any(m => m.ProductCode == "BAG") ?? false)
                                        && GeneralHelper.IsApplicationVersionGreaterorEqual(request.Application.Id, request.Application.Version.Major, _configuration.GetValue<string>("AndroidMilesFopBagsVersion"), _configuration.GetValue<string>("iPhoneMilesFopBagsVersion"));
            response.ShoppingCart.Products = await _shoppingCartUtility.ConfirmationPageProductInfo(flightReservationResponse, false, false, request.Flow.ToString(), request.Application, null, false, false, null, null, false, session.SessionId).ConfigureAwait(false);
            response.ShoppingCart.TermsAndConditions = await _shoppingCartUtility.GetProductBasedTermAndConditions(productVendorOffer, flightReservationResponse, false, session.SessionId, request.Flow);
            response.ShoppingCart.PaymentTarget = _shoppingCartUtility.GetPaymentTargetForRegisterFop(flightReservationResponse, request.Flow, isCompleteFarelockPurchase);
            if (flightReservationResponse.DisplayCart?.DisplaySeats != null && flightReservationResponse.DisplayCart.DisplaySeats.Exists(ds => ds.Currency == "ML1") && await _featureSettings.GetFeatureSettingValue("EnableCheckInCloudMigrationMSC_23X"))
            {
                price = Convert.ToDouble(flightReservationResponse.DisplayCart.DisplaySeats.Select(s => s.MoneyAmount).ToList().Sum());
            }
            response.ShoppingCart.TotalPrice = String.Format("{0:0.00}", price);
            response.ShoppingCart.DisplayTotalPrice = Decimal.Parse(price.ToString()).ToString("c");
            response.ShoppingCart.CartId = flightReservationResponse.CartId.ToString();
            if (isEnableMFOPBags)
            {
                response.ShoppingCart.TotalMiles = response.ShoppingCart?.Products?.Sum(p => int.Parse(p.ProdTotalRequiredMiles)).ToString();
                response.ShoppingCart.DisplayTotalMiles = UtilityHelper.FormatAwardAmountForDisplay(Decimal.Parse((response.ShoppingCart?.TotalMiles != null ? response.ShoppingCart.TotalMiles : "0")).ToString(), false);
            }
            if (ConfigUtility.IsViewResFlowPaymentSvcEnabled(request.Flow))
            {
                response.ShoppingCart.TripInfoForUplift = await _mscFormsOfPayment.GetUpliftTripInfo(reservationDetail.Detail, response.ShoppingCart.TotalPrice, response.ShoppingCart.Products);
            }
            else if (!_configuration.GetValue<bool>("DisableManageResChanges23C") && request.Flow == FlowType.POSTBOOKING.ToString()) { }
            else
            {
                response.ShoppingCart.TripInfoForUplift = await _mscFormsOfPayment.GetUpliftTripInfo(flightReservationResponse, response.ShoppingCart.TotalPrice, response.ShoppingCart.Products);
            }

            if (await _shoppingCartUtility.IsCheckInFlow(request.Flow))
            {
                response.PkDispenserPublicKey = null;
            }
            else if (!_configuration.GetValue<bool>("DisableManageResChanges23C"))
            {
                response.PkDispenserPublicKey = await _mscPkDispenserPublicKey.GetCachedOrNewpkDispenserPublicKey(request.Application.Id, request.Application.Version.Major, request.DeviceId, session.SessionId, session.Token, request.Flow, session?.CatalogItems).ConfigureAwait(false);
            }
            else
            {
                response.PkDispenserPublicKey = await _mscPkDispenserPublicKey.GetCachedOrNewpkDispenserPublicKey(request.Application.Id, request.Application.Version.Major, request.DeviceId, session.SessionId, session.Token, session?.CatalogItems).ConfigureAwait(false);
            }

            response.ShoppingCart.PromoCodeDetails = EnablePromoCodeForAncillaryProductsManageRes() && IsEligibleAncillaryProductForPromoCode(request.SessionId, flightReservationResponse, false) ? new Definition.FormofPayment.MOBPromoCodeDetails() : null;

            if (isCompleteFarelockPurchase)
            {
                response.ShoppingCart.Trips = _mscFormsOfPayment.GetTrips(reservationDetail.Detail.FlightSegments, request.Flow).Result;
                response.ShoppingCart.SCTravelers = _mscFormsOfPayment.GetTravelerCSLDetails(flightReservationResponse.Reservation, response.ShoppingCart.Trips, session.SessionId, request.Flow);
                //Prices & Taxes
                if (!flightReservationResponse.DisplayCart.IsNullOrEmpty() && !flightReservationResponse.DisplayCart.DisplayPrices.IsNullOrEmpty() && flightReservationResponse.DisplayCart.DisplayPrices.Any())
                {
                    // Journey Type will be "OW", "RT", "MD"
                    var JourneyType = UtilityHelper.GetJourneyTypeDescription(flightReservationResponse.Reservation);
                    bool isCorporateFare = IsCorporateTraveler(flightReservationResponse.Reservation.Characteristic);
                    response.ShoppingCart.Prices = await GetPrices(flightReservationResponse.DisplayCart.DisplayPrices, false, null, request.Flow, false, JourneyType, isCompleteFarelockPurchase, isCorporateFare, session: session);
                    response.ShoppingCart.Taxes = GetTaxAndFeesAfterPriceChange(flightReservationResponse.DisplayCart.DisplayPrices, request.Flow, false);
                    response.ShoppingCart.Captions = GetFareLockCaptions(flightReservationResponse.Reservation, JourneyType, isCorporateFare);
                    response.ShoppingCart.ELFLimitations = await GetELFLimitationsViewRes(response.ShoppingCart.Trips, request.Flow, reservationDetail.Detail.FlightSegments, request.Application.Id);
                }
            }
            if (response.ShoppingCart.Captions.IsNullOrEmpty())
            {
                response.ShoppingCart.Captions = await _etcUtility.GetCaptions("PaymentPage_ViewRes_Captions");

                if (ConfigUtility.IsViewResFlowPaymentSvcEnabled(request.Flow))
                {
                    // This code is added as part of bundle implementation in MR path to let client know the product custometer selected is bundle
                    MOBItem isBundleProduct = AddBundleCaptionForQMEvent(flightReservationResponse, response.ShoppingCart.Products);

                    if (isBundleProduct != null && !string.IsNullOrEmpty(isBundleProduct.CurrentValue))
                    {
                        if (response.ShoppingCart.Captions != null && response.ShoppingCart.Captions.Count > 0)
                            response.ShoppingCart.Captions.Add(isBundleProduct);
                        else
                        {
                            response.ShoppingCart.Captions = new List<MOBItem>();
                            response.ShoppingCart.Captions.Add(isBundleProduct);
                        }
                    }
                }
            }
            
            if (ConfigUtility.IsViewResFlowPaymentSvcEnabled(request.Flow) && ConfigUtility.IsManageResETCEnabled(request.Application.Id, request.Application.Version.Major))
            {
                response.ShoppingCart.Prices = AddGrandTotalIfNotExist(response.ShoppingCart.Prices, Convert.ToDouble(response.ShoppingCart.TotalPrice), response.ShoppingCart.Flow);
            }
            IsHidePromoOption(response.ShoppingCart, request.Flow, request.Application, session.IsReshopChange);
            await AssignProfileSavedETCsFromPersist(request.SessionId, response.ShoppingCart);

            await _sessionHelperService.SaveSession<MOBShoppingCart>(response.ShoppingCart, request.SessionId, new List<string> { request.SessionId, response.ShoppingCart.ObjectName }, response.ShoppingCart.ObjectName).ConfigureAwait(false);

            return (response, flightReservationResponse);
        }
        public MOBItem AddBundleCaptionForQMEvent(FlightReservationResponse flightReservationResponse, List<MOBProdDetail> products)
        {
            MOBItem mobItem = new MOBItem();
            if (_shoppingCartUtility.IsBundleProductSelected(flightReservationResponse, false))
            {
                mobItem.Id = "IsBundleProduct";
                mobItem.CurrentValue = products?.FirstOrDefault()?.Code;
            }
            return mobItem;
        }

        public List<MOBSHOPPrice> AddGrandTotalIfNotExist(List<MOBSHOPPrice> prices, double amount, string flow)
        {
            if (prices == null)
            {
                prices = new List<MOBSHOPPrice>();
            }
            if (!prices.Exists(p => p.DisplayType == "GRAND TOTAL"))
            {
                prices.Add(ShoppingCartUtility.BuildGrandTotalPriceForReservation(amount));
            }
            return prices;
        }
        private async Task<DynamicOfferDetailResponse> GetCCEOffersFromPersist(string sessionId)
        {
            if (!_configuration.GetValue<bool>("EnablePOMTermsAndConditions"))
                return await _sessionHelperService.GetSession<DynamicOfferDetailResponse>(sessionId, new DynamicOfferDetailResponse().GetType().FullName, new List<string> { sessionId, new DynamicOfferDetailResponse().GetType().FullName }).ConfigureAwait(false);

            var productOfferCce = new GetOffersCce();
            var productOfferCcePOMSessionValue = productOfferCce.ObjectName + _configuration.GetValue<string>("InflightMealProductCode").ToString();
            productOfferCce = await _sessionHelperService.GetSession<GetOffersCce>(sessionId, productOfferCcePOMSessionValue, new List<string> { sessionId, productOfferCcePOMSessionValue }).ConfigureAwait(false);

            var persistDynamicOfferDetailResponse = string.IsNullOrEmpty(productOfferCce?.OfferResponseJson)
                                    ? null
                                    : Newtonsoft.Json.JsonConvert.DeserializeObject<DynamicOfferDetailResponse>(productOfferCce?.OfferResponseJson);
            return persistDynamicOfferDetailResponse;
        }

        private string GetFlow(string flow, Collection<United.Service.Presentation.InteractionModel.ShoppingCartItem> items)
        {
            if (!_configuration.GetValue<bool>("EnableTravelOptionsInViewRes"))
            {
                return flow;
            }

            if (!FlowType.VIEWRES.ToString().Equals(flow) || items.IsNullOrEmpty())
            {
                return flow;
            }

            var hasSeatBundleProduct = items.Any(i => i?.Product?.Any(p => p?.SubProducts?.Any(sp => "BE".Equals(sp?.GroupCode) && HasEPUSubproduct(sp?.Extension?.Bundle)) ?? false) ?? false);
            return hasSeatBundleProduct ? FlowType.VIEWRES_BUNDLES_SEATMAP.ToString() : flow;
        }
        private string GetFlow(MOBShoppingCart persistShoppingCart, string flow)
        {
            if (persistShoppingCart?.Flow == FlowType.VIEWRES_BUNDLES_SEATMAP.ToString() || flow == FlowType.VIEWRES_BUNDLES_SEATMAP.ToString())
            {
                return FlowType.VIEWRES.ToString();
            }
            return string.IsNullOrEmpty(persistShoppingCart?.Flow) ? flow : persistShoppingCart.Flow;
        }
        private bool HasEPUSubproduct(United.Service.Presentation.ProductModel.BundleExtension bundle)
        {
            return bundle?.Products?.Any(p => "EPU".Equals(p.Code)) ?? false;
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

        private List<MOBItem> GetFareLockCaptions(United.Service.Presentation.ReservationModel.Reservation reservation, string journeyType, bool isCorporateFareLock)
        {
            if (reservation.IsNullOrEmpty() || journeyType.IsNullOrEmpty())
                return null;

            var title = GetPaymentTitleCaptionFareLock(reservation, journeyType);
            var tripType = GetSegmentDescriptionPageSubTitle(reservation);

            List<MOBItem> Captions = new List<MOBItem>();
            if (!title.IsNullOrEmpty() && !tripType.IsNullOrEmpty())
            {
                Captions.Add(GetFareLockViewResPaymentCaptions("PaymentPage_Title", title));
                Captions.Add(GetFareLockViewResPaymentCaptions("PaymentPage_SubTitle", tripType));
                Captions.Add(GetFareLockViewResPaymentCaptions("PaymentPage_Book24Hr_Policy", "Book without worry"));
                Captions.Add(GetFareLockViewResPaymentCaptions("PaymentPage_ProductCode", "FLK_ViewRes"));
            }

            // To find if PNR is corporate travel
            if (isCorporateFareLock)
            {
                var priceText = _configuration.GetValue<string>("CorporateRateText");
                if (!priceText.IsNullOrEmpty())
                {
                    Captions.Add(GetFareLockViewResPaymentCaptions("Corporate_PriceBreakDownText", priceText));
                }
            }

            return Captions;
        }

        private string GetSegmentDescriptionPageSubTitle(United.Service.Presentation.ReservationModel.Reservation reservation)
        {
            if (reservation != null)
            {
                var traveler = reservation.Travelers.Count.ToString() + (reservation.Travelers.Count() > 1 ? " travelers" : " traveler");
                var JourneyType = UtilityHelper.GetJourneyTypeDescription(reservation);

                return !JourneyType.IsNullOrEmpty() ? JourneyType + ", " + traveler : string.Empty;
            }
            return string.Empty;
        }

        private MOBItem GetFareLockViewResPaymentCaptions(string id, string currentValue)
        {
            if (id.IsNullOrEmpty() || currentValue.IsNullOrEmpty())
                return null;

            var Captions = new MOBItem()
            {
                Id = id,
                CurrentValue = currentValue
            };
            return Captions;
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

        private async System.Threading.Tasks.Task AssignProfileSavedETCsFromPersist(string sessionId, MOBShoppingCart shoppingCart)
        {
            if (_configuration.GetValue<bool>("SavedETCToggle") && !ConfigUtility.IsViewResFlowPaymentSvcEnabled(shoppingCart.Flow))
            {
                //var persistShopingCart = //FilePersist.Load<MOBShoppingCart>(sessionId, shoppingCart.GetType().ToString());
                var persistShopingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(sessionId, shoppingCart.ObjectName, new List<string> { sessionId, shoppingCart.ObjectName }).ConfigureAwait(false);
                if (persistShopingCart?.ProfileTravelerCertificates != null)
                {
                    shoppingCart.ProfileTravelerCertificates = persistShopingCart.ProfileTravelerCertificates;
                }
            }
        }

        public List<List<MOBSHOPTax>> GetTaxAndFeesAfterPriceChange(List<United.Services.FlightShopping.Common.DisplayCart.DisplayPrice> prices, string flow, bool isReshopChange = false, int appId = 0, string appVersion = "", string travelType = null)
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

                if (!ConfigUtility.IsViewResFlowPaymentSvcEnabled(flow)
                    && _configuration.GetValue<bool>("EnableAdvanceSearchCouponBooking")
                    && !string.IsNullOrEmpty(price?.Type) && price.Type.ToUpper() == "NONDISCOUNTPRICE-TRAVELERPRICE")
                    continue;

                if (price.SubItems != null && price.SubItems.Count > 0 && (!isReshopChange || (isReshopChange && price.Type.ToUpper() == "TRAVELERPRICE" && !isTravelerPriceDirty))) // Added by Hasnan - # 167553 - 10/04/2017
                {
                    foreach (var subItem in price.SubItems)
                    {
                        if (ci == null)
                        {
                            ci = TopHelper.GetCultureInfo(subItem.Currency);
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

                    if (ConfigUtility.IsViewResFlowPaymentSvcEnabled(flow) && ConfigUtility.EnableYADesc(isReshopChange)
                        && price.PricingPaxType != null && price.PricingPaxType.ToUpper().Equals("UAY"))
                    {
                        tnf.TaxCodeDescription = string.Format("{0} {1}: {2} per person", price.Count, "young adult (18-23)", tnf.DisplayAmount);
                    }
                    else
                    {
                        string description = price?.Description;
                        if (_shoppingUtility.EnableShoppingcartPhase2ChangesWithVersionCheck(appId, appVersion) && !isReshopChange && !string.IsNullOrEmpty(travelType) && (travelType == TravelType.CB.ToString() || travelType == TravelType.CLB.ToString()))
                        {
                            description = _shoppingUtility.BuildPaxTypeDescription(price?.PaxTypeCode, price?.Description, price.Count);
                        }
                        tnf.TaxCodeDescription = string.Format("{0} {1}: {2} per person", price.Count, description.ToLower(), tnf.DisplayAmount);
                    }
                    if (isEnableOmniCartMVP2Changes)
                    {
                        tnf.TaxCodeDescription = tnf.TaxCodeDescription.Replace(" per ", "/");
                    }
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

        private string formatAmountForDisplay(decimal amt, CultureInfo ci, /*string currency,*/ bool roundup = true, bool isAward = false)
        {
            return formatAmountForDisplay(amt.ToString(), ci, roundup, isAward);
        }

        private string formatAmountForDisplay(string amt, CultureInfo ci, /*string currency,*/ bool roundup = true, bool isAward = false)
        {
            string newAmt = amt;
            decimal amount = 0;
            decimal.TryParse(amt, out amount);

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
                    //currencySymbol = GetCurrencySymbol(currency.ToUpper());
                    //newAmt = currencySymbol + newAmt;
                    newAmt = GetCurrencySymbol(ci, amount, roundup);
                    break;
            }

            return isAward ? "+ " + newAmt : newAmt;
        }

        private string GetCurrencySymbol(CultureInfo ci, /*string currencyCode,*/ decimal amount, bool roundup)
        {
            string result = string.Empty;

            try
            {

                //decimal tempAmt = -1;
                //decimal.TryParse(amount, out tempAmt);

                if (amount > -1)
                {
                    if (roundup)
                    {
                        int newTempAmt = (int)decimal.Ceiling(amount);
                        //var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

                        //foreach (var ci in CultureInfo.GetCultures(CultureTypes.AllCultures))
                        //{
                        try
                        {
                            var ri = new RegionInfo(ci.Name);
                            //if (ri.ISOCurrencySymbol.ToUpper() == currencyCode.ToUpper())
                            //{
                            //result = ri.CurrencySymbol;
                            CultureInfo tempCi = Thread.CurrentThread.CurrentCulture;
                            Thread.CurrentThread.CurrentCulture = ci;
                            result = newTempAmt.ToString("c0");
                            Thread.CurrentThread.CurrentCulture = tempCi;
                            //break;
                            //}
                        }
                        catch { }
                        //}
                        //newAmt = newTempAmt.ToString();
                    }
                    else
                    {
                        //var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

                        //foreach (var ci in CultureInfo.GetCultures(CultureTypes.AllCultures))
                        //{
                        try
                        {
                            var ri = new RegionInfo(ci.Name);
                            //if (ri.ISOCurrencySymbol.ToUpper() == currencyCode.ToUpper())
                            //{
                            //result = ri.CurrencySymbol;
                            CultureInfo tempCi = Thread.CurrentThread.CurrentCulture;
                            Thread.CurrentThread.CurrentCulture = ci;
                            result = amount.ToString("c");
                            Thread.CurrentThread.CurrentCulture = tempCi;
                            //break;
                            //}
                        }
                        catch { }
                        //newAmt = tempAmt.ToString("F");
                        //}
                    }
                }

            }
            catch { }

            return result;
        }

        private async Task<List<MOBItem>> GetELFLimitationsViewRes(List<MOBSHOPTrip> shopTrip, string flow, Collection<United.Service.Presentation.SegmentModel.ReservationFlightSegment> FlightSegments = null, int appId = 0)
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
                if (_configuration.GetValue<bool>("EnablePBE") && ConfigUtility.IsViewResFlowPaymentSvcEnabled(flow))
                {
                    string productCode = string.Empty;
                    var flightSegment = FlightSegments?.FirstOrDefault(p => p != null && p.TripNumber != null)?.FlightSegment;
                    productCode = flightSegment?.Characteristic?.FirstOrDefault(c => c != null && !string.IsNullOrEmpty(c.Code) && c.Code.Equals("PRODUCTCODE", StringComparison.OrdinalIgnoreCase))?.Value;
                    databaseKey = productCode + "_MR_Limitations";
                }
                else
                {
                    databaseKey = "IBE_MR_Limitations";
                }
            }

            var messages = !string.IsNullOrEmpty(databaseKey) ? await GetMPPINPWDTitleMessages(databaseKey) : null;

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
            List<United.Definition.MOBLegalDocument> docs = await _legalDocumentsForTitlesService.GetNewLegalDocumentsForTitles(titleList, "trans0", true).ConfigureAwait(false);
            if (docs != null && docs.Count > 0)
            {
                foreach (var doc in docs)
                {
                    MOBItem item = new MOBItem();
                    item.Id = doc.Title;
                    item.CurrentValue = doc.Document;
                    messages.Add(item);
                }
            }
            return messages;
        }


        private bool EnablePromoCodeForAncillaryProductsManageRes()
        {
            return _configuration.GetValue<bool>("EnablePromoCodeForAncillaryOffersManageRes");
        }
        private bool IsEligibleAncillaryProductForPromoCode(string sessionId, FlightReservationResponse flightReservationResponse, bool isPost)
        {
            var productCodes = isPost ? flightReservationResponse.CheckoutResponse.ShoppingCart.Items.Where(x => x.Product.FirstOrDefault().Code != "RES").Select(x => x.Product.FirstOrDefault().Code).ToList() :
                                      flightReservationResponse.ShoppingCart.Items.Where(x => x.Product.FirstOrDefault().Code != "RES").Select(x => x.Product.FirstOrDefault().Code).ToList();

            if (productCodes == null || !productCodes.Any())
                return false;

            var ancillaryProductCodesForPromoCode = _configuration.GetValue<string>("EnablePromoCodeForAncillaryProductsManageRes").Split('|');
            return ancillaryProductCodesForPromoCode != null && ancillaryProductCodesForPromoCode.Count() > 0 ? ancillaryProductCodesForPromoCode.Contains(productCodes.FirstOrDefault()) : false;
        }

        private double GetGrandTotalPriceForShoppingCart(bool isCompleteFarelockPurchase, FlightReservationResponse flightReservationResponse, bool isPost, string flow = "VIEWRES")
        {
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
                    closeBookingFee = UtilityHelper.GetCloseBookingFee(isPost, flightReservationResponseShoppingCart, flow);

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
                    case "BAGGAGECALCULATOR":
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

        private double GetGrandTotalPriceForShoppingCart_ForViewRes(bool isCompleteFarelockPurchase, FlightReservationResponse flightReservationResponse)
        {
            return isCompleteFarelockPurchase ? UtilityHelper.GetGrandTotalPriceFareLockShoppingCart(flightReservationResponse)
                                             : flightReservationResponse.ShoppingCart.Items.Where(x => x.Product.FirstOrDefault().Code != "RES").Select(x => x.Product?.FirstOrDefault().Price?.Totals?.FirstOrDefault()?.Amount ?? 0).ToList().Sum();
        }

        private async Task<FlightReservationResponse> RegisterOffers(MOBRegisterOfferRequest request, ProductOffer productOffer,
            United.Service.Presentation.ProductResponseModel.ProductOffer productVendorOffer,
             United.Service.Presentation.ProductResponseModel.ProductOffer cceBagOffers,
            United.Service.Presentation.ReservationResponseModel.ReservationDetail reservationDetail, Session session,
            United.Service.Presentation.ProductResponseModel.ProductOffer productFareLockOffer, DynamicOfferDetailResponse pomOffer = null,
            Collection<RegisterOfferRequest> upgradeCabinRegisterOfferRequest = null, ProductOffer cceDODOfferResponse = null)
        {
            var response = new FlightReservationResponse();
            string actionName = "RegisterOffers";

            bool isUpgradeMallRequest
                = (string.Equals(request.Flow, _UPGRADEMALL, StringComparison.OrdinalIgnoreCase) && ConfigUtility.IsViewResFlowPaymentSvcEnabled(request.Flow));

            if (((request.Flow != "BOOKING" && request.Flow != "POSTBOOKING" && request.Flow != "BAGGAGECALCULATOR") && !((productOffer != null || productVendorOffer != null || pomOffer != null || cceDODOfferResponse != null) && reservationDetail != null))
                || (request.Flow == "BOOKING" && !(productFareLockOffer != null) && (request.MerchandizingOfferDetails.FirstOrDefault().ProductCode == "FLK"))
                || (request.Flow == "POSTBOOKING" && productOffer == null) || (request.Flow == "BAGGAGECALCULATOR" && cceBagOffers == null)
                || isUpgradeMallRequest)
            {
                throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }

            var registerOfferRequests = (isUpgradeMallRequest) ? upgradeCabinRegisterOfferRequest :
               await BuildRegisterOffersRequest(request, productOffer, productVendorOffer, cceBagOffers, reservationDetail, productFareLockOffer, pomOffer, cceDODOfferResponse);

            if (registerOfferRequests != null && registerOfferRequests.Count > 0)
            {
                if (session == null)
                {
                    throw new MOBUnitedException("Could not find your session.");
                }

                string jsonRequest = JsonConvert.SerializeObject(registerOfferRequests);
                response = await _shoppingCartService.RegisterOffers<FlightReservationResponse>(session.Token, actionName, jsonRequest, session.SessionId).ConfigureAwait(false);

                if (!(response == null))
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

                            throw new Exception(errorMessage);
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
        private bool IsIBE(Collection<Characteristic> characteristics)
        {
            if (!characteristics.IsNullOrEmpty())
            {
                return characteristics.Any(c => !c.Code.IsNullOrEmpty() && c.Code.Equals("ProductCode", StringComparison.OrdinalIgnoreCase) &&
                                          !c.Value.IsNullOrEmpty() && c.Value.Equals("IsIBE", StringComparison.OrdinalIgnoreCase));
            }
            return false;
        }

        public async Task<Collection<RegisterOfferRequest>> BuildRegisterOffersRequest(MOBRegisterOfferRequest request, United.Service.Presentation.ProductResponseModel.ProductOffer productOffer, United.Service.Presentation.ProductResponseModel.ProductOffer productVendorOffer,
            United.Service.Presentation.ProductResponseModel.ProductOffer cceBagOffers, United.Service.Presentation.ReservationResponseModel.ReservationDetail reservation, United.Service.Presentation.ProductResponseModel.ProductOffer productFareLockOffer, DynamicOfferDetailResponse pomOffer, ProductOffer cceDODOfferResponse)
        {
            var reservationDetail = new ReservationDetail();
            var selectedOffer = new ProductOffer();
            var registerOfferRequests = new Collection<RegisterOfferRequest>();
            int reservationCtr = 0;
            List<MOBItem> selectedProductsAndCount = new List<MOBItem>();

            RegisterOfferRequest registerOfferRequest = null;
            ProductOffer selOffer = null;
            var flightreservationResponse = new FlightReservationResponse();
            var isIBE = false;
            if (request.Flow == "POSTBOOKING" && await _featureSettings.GetFeatureSettingValue("EnableReservationPostBooking").ConfigureAwait(false))
            {
                var flightSegments = reservation?.Detail?.FlightSegments;
                if (flightSegments == null)
                {
                    flightreservationResponse = _sessionHelperService.GetSession<FlightReservationResponse>(request.SessionId, "United.Services.FlightShopping.Common.FlightReservation.FlightReservationResponse", new List<string> { request.SessionId, "United.Services.FlightShopping.Common.FlightReservation.FlightReservationResponse" }).Result;
                    flightSegments = flightreservationResponse?.Reservation?.FlightSegments;
                }
                if (flightSegments != null)
                    foreach (var segments in flightSegments)
                    {
                        isIBE = IsIBE(segments.Characteristic);
                        if (isIBE)
                            break;
                    }
            }
            if (request.MerchandizingOfferDetails.Any(a => a.ProductCode == _configuration.GetValue<string>("InflightMealProductCode")) && ConfigUtility.IsViewResFlowPaymentSvcEnabled(request.Flow))
            {
                var selectedOffers = await _sessionHelperService.GetSession<List<MOBInFlightMealsRefreshmentsResponse>>(request.SessionId, new MOBInFlightMealsRefreshmentsResponse().ObjectName, new List<string> { request.SessionId, new MOBInFlightMealsRefreshmentsResponse().ObjectName }).ConfigureAwait(false); //change session

                registerOfferRequest = GetInflightMealsRegisterOfferRequest(pomOffer, request, reservation);
                var pastSelections = GetPastRefreshmentSelections(request, pomOffer, selectedOffers);

                var toggleCheck = _shoppingUtility.EnableEditForAllCabinPOM(request.Application.Id, request.Application.Version?.Major);
                if ((!toggleCheck && selectedOffers == null) || (toggleCheck && selectedOffers == null && pastSelections.Count == 0))
                    throw new MOBUnitedException(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                registerOfferRequest.Products[0].SelectedProductRequests = GetInFlightMealSelectedOffersRequest(selectedOffers, pastSelections, request);
                if (registerOfferRequest.Products[0].SelectedProductRequests != null)
                    registerOfferRequest.Products[0].Ids = registerOfferRequest.Products[0].SelectedProductRequests.Select(a => a.ProductId).ToList();
                registerOfferRequest.WorkFlowType = ConfigUtility.GetWorkFlowType(request.Flow, _configuration.GetValue<string>("InflightMealProductCode"));// todo check for other products thrugh debug till checkout                
                registerOfferRequests.Add(registerOfferRequest);
            }
            else
            {
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
                    else if (merchandizingOfferDetail.ProductCode.Equals("BAG"))
                        selectedOffer = cceBagOffers;
                    else if (ConfigUtility.IsViewResFlowPaymentSvcEnabled(request.Flow))
                    {
                        ProductOffer productOfferFromCce = await LoadProductOfferFromPersist(request);
                        selectedOffer = GetSelectedOffer(productOffer, productVendorOffer, productOfferFromCce, merchandizingOfferDetail.ProductCode, cceDODOfferResponse);
                    }
                    else
                        selectedOffer = productOffer;

                    if (selectedOffer != null)
                    {
                        if ((!_configuration.GetValue<bool>("DisableManageResChanges23C")) && !(selectedOffer.Offers.FirstOrDefault().ProductInformation.ProductDetails.Any(x => x.Product.Code.Equals(merchandizingOfferDetail.ProductCode)))
                        || selectedOffer.Offers.FirstOrDefault().ProductInformation.ProductDetails.Where(x => x.Product.Code.Equals(merchandizingOfferDetail.ProductCode))
                        .Select(c => c.Product.SubProducts.All(x => x.Prices == null || x.Prices.Count() == 0)).FirstOrDefault())
                            continue;

                        else if (_configuration.GetValue<bool>("DisableManageResChanges23C"))
                        {
                            if (!(selectedOffer.Offers.FirstOrDefault().ProductInformation.ProductDetails.Any(x => x.Product.Code.Equals(merchandizingOfferDetail.ProductCode)))
                            || selectedOffer.Offers.FirstOrDefault().ProductInformation.ProductDetails.Where(x => x.Product.Code.Equals(merchandizingOfferDetail.ProductCode))
                            .Select(c => c.Product.SubProducts.All(x => x.Prices.Count() == 0 || x.Prices == null)).FirstOrDefault())
                                continue;
                        }
                        else
                        {
                            selOffer = null;
                            if (!(merchandizingOfferDetail.IsOfferRegistered))
                            {
                                selOffer = new ProductOffer();
                                selOffer.Offers = new Collection<Offer>();
                                Offer offer = new Offer();
                                offer.ProductInformation = new ProductInformation();
                                offer.ProductInformation.ProductDetails = new Collection<ProductDetail>();
                                offer.ProductInformation.ProductDetails.Add(selectedOffer.Offers[0].ProductInformation.ProductDetails.Where(x => x.Product.Code.Equals(merchandizingOfferDetail.ProductCode)).FirstOrDefault());
                                selOffer.Offers.Add(offer);
                                selOffer.Travelers = selectedOffer.Travelers;
                                selOffer.Solutions = selectedOffer.Solutions;
                                selOffer.Response = selectedOffer.Response;
                                selOffer.FlightSegments = selectedOffer.FlightSegments;
                                selOffer.Requester = selectedOffer.Requester;
                                if (IsEnableOmniCartReleaseCandidateTwoChanges_Bundles(request.Application.Id, request.Application.Version.Major) && !ConfigUtility.IsViewResFlowPaymentSvcEnabled(request.Flow))
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

                    if (isIBE && request.Flow == "POSTBOOKING" && await _featureSettings.GetFeatureSettingValue("EnableReservationPostBooking").ConfigureAwait(false))
                    {
                        registerOfferRequest.Reservation = flightreservationResponse?.Reservation;
                    }
                    if (ConfigUtility.IsViewResFlowPaymentSvcEnabled(request.Flow))
                    {
                        if (_configuration.GetValue<bool>("EnableCSLCloudMigrationToggle"))
                        {
                            registerOfferRequest.WorkFlowType = ConfigUtility.GetWorkFlowType(request.Flow);
                        }
                    }
                    registerOfferRequests.Add(registerOfferRequest);
                }
            }

            return registerOfferRequests;
        }
        private ProductOffer GetSelectedOffer(ProductOffer productOffer, ProductOffer productVendorOffer, ProductOffer productOfferFromCce, string code, ProductOffer cceDODOfferResponse)
        {
            if ((cceDODOfferResponse?.Offers?.Any(o => o.ProductInformation?.ProductDetails?.Any(p => p?.Product?.Code == code) ?? false) ?? false))
                return cceDODOfferResponse;
            else if (productOfferFromCce?.Offers?.Any(o => o.ProductInformation?.ProductDetails?.Any(p => p?.Product?.Code == code) ?? false) ?? false)
            {
                if (code == "BEB")
                {
                    var bebProduct = productOfferFromCce?.Offers?.FirstOrDefault()?.ProductInformation?.ProductDetails?.FirstOrDefault(p => p.Product.Code == "BEB").Product;
                    bebProduct.DisplayName = !_configuration.GetValue<bool>("EnableNewBEBContentChange") ? "Switch to Economy" : _configuration.GetValue<string>("BEBuyOutPaymentInformationMessage");
                }

                return productOfferFromCce;
            }
            else if (code.Equals("TPI"))
                return productVendorOffer;
            else
                return productOffer;
        }
        private async Task<ProductOffer> LoadProductOfferFromPersist(MOBRegisterOfferRequest request)
        {
            var persistedProductOfferFromCce = new GetOffersCce();
            persistedProductOfferFromCce = await _sessionHelperService.GetSession<GetOffersCce>(request.SessionId, persistedProductOfferFromCce.ObjectName, new List<string> { request.SessionId, persistedProductOfferFromCce.ObjectName }).ConfigureAwait(false);
            var productOfferFromCce = string.IsNullOrEmpty(persistedProductOfferFromCce.OfferResponseJson)
                                            ? null
                                            : JsonConvert.DeserializeObject<ProductOffer>(persistedProductOfferFromCce.OfferResponseJson);

            return productOfferFromCce;
        }
        private List<United.Service.Presentation.ProductModel.Product> GetPastRefreshmentSelections(MOBRegisterOfferRequest request, Service.Presentation.PersonalizationResponseModel.DynamicOfferDetailResponse pomOffer, List<MOBInFlightMealsRefreshmentsResponse> responses)
        {
            var pastSelections = new List<United.Service.Presentation.ProductModel.Product>();

            if (_shoppingUtility.EnableEditForAllCabinPOM(request.Application.Id, request.Application.Version.Major))
            {
                var segmentId = responses.FirstOrDefault()?.SegmentId;
                foreach (var offer in pomOffer.Offers)
                {
                    foreach (var productDetail in offer.ProductInformation.ProductDetails.Where(p => p.Product.Code == "PAST_SELECTION"))
                    {
                        if (productDetail.Product != null && productDetail.Product.SubProducts != null && productDetail.Product.SubProducts.Any(x => x.Association != null && x.Association.SegmentRefIDs != null && x.Association.SegmentRefIDs.Contains(segmentId)))
                        {
                            pastSelections.Add(productDetail.Product);
                        }
                    }
                }
            }
            return pastSelections;
        }
        private List<ProductSelectedRequest> GetInFlightMealSelectedOffersRequest(List<MOBInFlightMealsRefreshmentsResponse> selectedOffers, List<United.Service.Presentation.ProductModel.Product> list, MOBRegisterOfferRequest request)
        {
            List<ProductSelectedRequest> selectedProductRequests = new List<ProductSelectedRequest>();
            var ids = new HashSet<String>();

            foreach (var soffer in selectedOffers)
            {
                ids.Add(soffer.Passenger.PassengerId);

                if (soffer.Snacks != null)
                {
                    foreach (var snacks in soffer?.Snacks?.Where(q => q.Quantity > 0))
                    {
                        GetInFlightMealAddSelectedProduct(selectedProductRequests, snacks);
                    }
                }

                if (soffer.Beverages != null)
                {
                    foreach (var beverage in soffer?.Beverages?.Where(q => q.Quantity > 0))
                    {
                        GetInFlightMealAddSelectedProduct(selectedProductRequests, beverage);
                    }
                }

                if (soffer.FreeMeals != null)
                {
                    foreach (var freeMeal in soffer?.FreeMeals?.Where(q => q.Quantity > 0))
                    {
                        GetInFlightMealAddSelectedProduct(selectedProductRequests, freeMeal);
                    }
                }

                if (_configuration.GetValue<bool>("EnableDynamicPOM"))
                {
                    if (soffer.DifferentPlanOptions != null)
                    {
                        foreach (var displayOption in soffer?.DifferentPlanOptions?.Where(q => q.Quantity > 0))
                        {
                            GetInFlightMealAddSelectedProduct(selectedProductRequests, displayOption);
                        }
                    }

                    if (soffer.SpecialMeals != null)
                    {
                        foreach (var specialMeal in soffer?.SpecialMeals?.Where(q => q.Quantity > 0))
                        {
                            GetInFlightMealAddSelectedProduct(selectedProductRequests, specialMeal);
                        }
                    }
                }

                if (_shoppingUtility.EnablePOMPreArrival(request.Application.Id, request.Application.Version.Major))
                {
                    if (soffer.PreArrivalFreeMeals != null)
                    {
                        foreach (var preArrivalMeal in soffer?.PreArrivalFreeMeals?.Where(q => q.Quantity > 0))
                        {
                            GetInFlightMealAddSelectedProduct(selectedProductRequests, preArrivalMeal);
                        }
                    }

                    if (soffer.PreArrivalDifferentPlanOptions != null)
                    {
                        foreach (var preArrivalDiffOption in soffer?.PreArrivalDifferentPlanOptions?.Where(q => q.Quantity > 0))
                        {
                            GetInFlightMealAddSelectedProduct(selectedProductRequests, preArrivalDiffOption);
                        }
                    }
                }
            }

            if (_shoppingUtility.EnableEditForAllCabinPOM(request.Application.Id, request.Application.Version.Major))
            {
                var pastSelections = list.SelectMany(prod => prod.SubProducts).SelectMany(subProd => subProd.Association.TravelerRefIDs.Where(id => !ids.Contains(id)).Select(tid => CreatePastSelectionRequest(subProd))).ToList();

                if (pastSelections != null && pastSelections.Count > 0)
                {
                    selectedProductRequests.AddRange(pastSelections);
                }
            }
            return selectedProductRequests;
        }
        private ProductSelectedRequest CreatePastSelectionRequest(United.Service.Presentation.ProductModel.SubProduct subProd)
        {
            int quantity = 0;
            if (Int32.TryParse(subProd.Extension?.MealCatalog?.Characteristics?.Where(x => x.Code == "Quantity").FirstOrDefault().Value, out int q))
            {
                quantity = q;
            }

            return new ProductSelectedRequest
            {
                EddCode = subProd.Code,
                ProductId = subProd.Extension?.MealCatalog?.Characteristics?.Where(x => x.Code == "PriceId").FirstOrDefault().Value,
                Quantity = quantity
            };
        }

        private void GetInFlightMealAddSelectedProduct(List<ProductSelectedRequest> selectedProductRequests, MOBInFlightMealRefreshment refreshment)
        {
            if (selectedProductRequests.Any(x => x.EddCode == refreshment.MealCode && x.ProductId == refreshment.ProductId))
            {
                selectedProductRequests.Find(x => x.EddCode == refreshment.MealCode && x.ProductId == refreshment.ProductId).Quantity += refreshment.Quantity;
            }
            else
            {
                selectedProductRequests.Add(new ProductSelectedRequest
                {
                    EddCode = refreshment.MealCode,
                    ProductId = refreshment.ProductId,
                    Quantity = refreshment.Quantity
                });
            }
        }

        private RegisterOfferRequest GetInflightMealsRegisterOfferRequest(DynamicOfferDetailResponse pomOffer, MOBRegisterOfferRequest request, ReservationDetail reservation)
        {
            RegisterOfferRequest registerOfferRequest = new RegisterOfferRequest();
            registerOfferRequest.Offer = new ProductOffer();
            registerOfferRequest.Offer.Offers = pomOffer.Offers;
            registerOfferRequest.Offer.FlightSegments = pomOffer.FlightSegments;
            registerOfferRequest.Offer.ODOptions = pomOffer.ODOptions;
            registerOfferRequest.Offer.Requester = pomOffer.Requester;
            registerOfferRequest.Offer.Solutions = pomOffer.Solutions;
            registerOfferRequest.Offer.Travelers = pomOffer.Travelers;

            registerOfferRequest.AutoTicket = false;
            registerOfferRequest.CartId = request.CartId;
            registerOfferRequest.CartKey = null;
            registerOfferRequest.CountryCode = null;
            registerOfferRequest.Delete = false;
            registerOfferRequest.LangCode = null;
            registerOfferRequest.ProductCode = request.MerchandizingOfferDetails[0].ProductCode;
            registerOfferRequest.SubProductCode = null;
            registerOfferRequest.Reservation = (reservation == null ? null : reservation.Detail);
            registerOfferRequest.SubProductCode = _configuration.GetValue<string>("InflightMealProductCode");
            registerOfferRequest.WorkFlowType = WorkFlowType.PreOrderMeals;

            registerOfferRequest.Products = new List<ProductRequest>
                {
                    new ProductRequest
                    {
                        Code =  _configuration.GetValue<string>("InflightMealProductCode"),
                        Ids = new List<string>(),
                        SelectedProductRequests = new List<ProductSelectedRequest>()
                    }
                };

            if (_shoppingUtility.EnableEditForAllCabinPOM(request.Application.Id, request.Application.Version.Major))
            {
                var list = new Collection<Characteristic>();
                list.Add(new Characteristic() { Code = "ManageRes", Value = "true" });

                var mealType = pomOffer.Offers?.FirstOrDefault()?.ProductInformation?.ProductDetails?.FirstOrDefault()?.Product?.SubProducts?.FirstOrDefault()?.Extension?.MealCatalog?.Characteristics?.Where(a => a.Code == "MealType")?.FirstOrDefault()?.Value;
                var merchandizingOfferDetail = request.MerchandizingOfferDetails.FirstOrDefault();
                var segmentNumber = merchandizingOfferDetail?.ProductIds?.FirstOrDefault();
                var flightSegment = pomOffer.FlightSegments?.FirstOrDefault(x => x.SegmentNumber.ToString() == segmentNumber);

                if (mealType == InflightMealType.Refreshment.ToString() && flightSegment != null)
                {
                    registerOfferRequest.Delete = true;
                    list.Add(new Characteristic() { Code = "SegmentNumber", Value = segmentNumber });
                    list.Add(new Characteristic() { Code = "Pnr", Value = pomOffer.Response.RecordLocator });
                    list.Add(new Characteristic() { Code = "FlightNumber", Value = flightSegment?.FlightNumber });
                    list.Add(new Characteristic() { Code = "DepartureAirport", Value = flightSegment.DepartureAirport.IATACode });
                    list.Add(new Characteristic() { Code = "ArrivalAirport", Value = flightSegment.ArrivalAirport.IATACode });
                    list.Add(new Characteristic() { Code = "DepartureDateTime", Value = flightSegment.DepartureDateTime });
                    list.Add(new Characteristic() { Code = "ClearAll", Value = "true" });
                }
                merchandizingOfferDetail.ProductIds = null;
                registerOfferRequest.Characteristics = list;
            }
            return registerOfferRequest;
        }

        private RegisterOfferRequest GetRegisterOffersRequest(MOBRegisterOfferRequest request, MerchandizingOfferDetails merchandizingOfferDetail, ProductOffer Offer, ReservationDetail reservation, Collection<Characteristic> characteristics = null)
        {
            RegisterOfferRequest registerOfferRequest = new RegisterOfferRequest();

            registerOfferRequest.AutoTicket = false;
            registerOfferRequest.CartId = request.CartId;
            registerOfferRequest.CartKey = request.CartKey;
            registerOfferRequest.CountryCode = (request.PointOfSale != null) ? request.PointOfSale : "US";
            registerOfferRequest.Delete = merchandizingOfferDetail.IsOfferRegistered;
            registerOfferRequest.LangCode = (request.LanguageCode != null) ? request.LanguageCode : "en-US";
            registerOfferRequest.Offer = (Offer == null ? null : GetFilteredOfferByProductId(Offer, merchandizingOfferDetail, request.Flow));
            registerOfferRequest.Characteristics = !ConfigUtility.IsViewResFlowPaymentSvcEnabled(request.Flow) ? characteristics : new System.Collections.ObjectModel.Collection<United.Service.Presentation.CommonModel.Characteristic>() {
                    new Characteristic() { Code = "ManageRes", Value = "true" }};

            registerOfferRequest.ProductCode = merchandizingOfferDetail.ProductCode;
            registerOfferRequest.ProductIds = merchandizingOfferDetail.ProductIds;
            registerOfferRequest.SubProductCode = merchandizingOfferDetail.SubProductCode;
            registerOfferRequest.Reservation = (request.Flow == "RESHOP" || request.Flow == "POSTBOOKING") ? null : (reservation == null ? null : reservation.Detail);
            registerOfferRequest.Products = GetSelectedProductOffersRequest(Offer, merchandizingOfferDetail);

            registerOfferRequest.GUID = !UtilityHelper.IsCheckinFlow(request.Flow) ? null : new Service.Presentation.CommonModel.UniqueIdentifier() { ID = request.CheckinSessionId, Type = "eToken" };
            registerOfferRequest.WorkFlowType = ConfigUtility.GetWorkFlowType(request.Flow);
            if (_configuration.GetValue<bool>("EnableOmniChannelCartMVP1"))
            {
                registerOfferRequest.DeviceID = request.DeviceId;
            }

            return registerOfferRequest;
        }
        //This method is to filter subproduct which matches the productids sent in request.
        //(under productOffer.Offers[0].ProductInformation.ProductDetails[0].Product.SubProducts we get all the prices ..
        //but with new req to shopingcart they want only product which is selected)
        private ProductOffer GetFilteredOfferByProductId(ProductOffer productOffer, MerchandizingOfferDetails offerDetailRequest, string flow)
        {

            if (IsAllowedProductToFilterOffer(flow, offerDetailRequest?.ProductCode)
                && productOffer.Offers[0].ProductInformation.ProductDetails[0].Product != null
                && productOffer.Offers[0].ProductInformation.ProductDetails[0].Product.SubProducts != null
                && productOffer.Offers[0].ProductInformation.ProductDetails[0].Product.SubProducts.Any())
            {
                productOffer.Offers[0].ProductInformation.ProductDetails[0].Product.SubProducts.RemoveWhere(subproduct =>
                                                                                                               !isSelectedProduct(subproduct, offerDetailRequest));
            }
            return productOffer;
        }
        private bool IsAllowedProductToFilterOffer(string flow, string productCode)
        {
            string products = _configuration.GetValue<string>("ProductsToApplyOfferFilterLogic");
            if (flow == FlowType.VIEWRES.ToString() && !string.IsNullOrEmpty(products)
                && products.Split(",").Contains(productCode))
            {
                return true;
            }
            return false;
        }
        private bool isSelectedProduct(SubProduct subProduct, MerchandizingOfferDetails offerDetailRequest)
        {
            if (subProduct != null
                && subProduct.Prices != null
                && subProduct.Prices.Any()
                && !String.IsNullOrEmpty(subProduct.Prices[0].ID))
            {
                return offerDetailRequest.ProductIds.Contains(subProduct.Prices[0].ID) && isSubProductCodeMatched(offerDetailRequest.SubProductCode, subProduct.SubGroupCode);

            }
            return false;
        }
        private bool isSubProductCodeMatched(string selectedOfferSubProductCode, string subProductCode)
        {
            if (!string.IsNullOrEmpty(selectedOfferSubProductCode) && !string.IsNullOrEmpty(subProductCode))
            {
                return selectedOfferSubProductCode.Equals(subProductCode, StringComparison.OrdinalIgnoreCase);
            }
            return true;
        }

        private ProductOffer BuildProductOffersforOTP(MOBRegisterOfferRequest request, ProductOffer productOffer, MerchandizingOfferDetails product)
        {
            var otpOffer = new ProductOffer();
            otpOffer.Offers = new Collection<Offer>();
            Offer offer = new Offer();
            offer.ProductInformation = new ProductInformation();
            offer.ProductInformation.ProductDetails = new Collection<ProductDetail>();
            offer.ProductInformation.ProductDetails.Add(productOffer.Offers[0].ProductInformation.ProductDetails.Where(x => x.Product.Code.Equals(product.ProductCode)).FirstOrDefault().Clone());
            otpOffer.Offers.Add(offer);
            otpOffer.Travelers = new Collection<Service.Presentation.ProductModel.ProductTraveler>();
            otpOffer.Travelers = productOffer.Travelers;
            otpOffer.Solutions = new Collection<Service.Presentation.ProductRequestModel.Solution>();
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
        private List<ProductRequest> GetSelectedProductOffersRequest(ProductOffer selectedCceBagOffers, MerchandizingOfferDetails merchandizingOfferDetail)
        {
            if (merchandizingOfferDetail.ProductCode == "BAG" && !merchandizingOfferDetail.IsOfferRegistered)
            {
                var subProducts = selectedCceBagOffers?.Offers[0]?.ProductInformation?.ProductDetails[0]?.Product?.SubProducts;
                if ((subProducts == null || !subProducts.Any()) || (merchandizingOfferDetail.ProductIds == null || !merchandizingOfferDetail.ProductIds.Any()))
                    return null;

                var productIds = merchandizingOfferDetail.ProductIds.Select(x => subProducts.FirstOrDefault(sp => sp.ID == x).Prices.FirstOrDefault().ID).ToList();
                var selectedProductRequests = subProducts.Select(sp => GetSelectedBagsRequest(sp, merchandizingOfferDetail.ProductIds)).Where(x => x != null).ToList();
                return new List<ProductRequest>
                {
                new ProductRequest { Code = merchandizingOfferDetail.ProductCode, Ids = productIds, SelectedProductRequests = selectedProductRequests }
                };
            }
            return new List<ProductRequest>()
            {
               new ProductRequest () { Code = merchandizingOfferDetail.ProductCode, Ids = merchandizingOfferDetail.ProductIds }
            };
        }
        private ProductSelectedRequest GetSelectedBagsRequest(SubProduct subProduct, List<string> ids)
        {
            var selectedSubProductCount = ids.Count(x => x.Equals(subProduct.ID, StringComparison.InvariantCultureIgnoreCase));

            if (selectedSubProductCount <= 0 || (subProduct.Prices == null || !subProduct.Prices.Any()
                || subProduct.Prices[0].PaymentOptions == null || !subProduct.Prices[0].PaymentOptions.Any())) return null;

            return new ProductSelectedRequest
            {
                ProductId = subProduct.Prices[0]?.ID,
                EddCode = subProduct.Prices[0]?.PaymentOptions[0]?.EDDCode,
                Quantity = selectedSubProductCount
            };
        }
        private async Task<FlightReservationResponse> GetFlightReservationResponseByCartId(string sessionId, string cartId)
        {
            var persistData = await _sessionHelperService.GetSession<List<CCEFlightReservationResponseByCartId>>(sessionId, new CCEFlightReservationResponseByCartId().ObjectName, new List<string> { sessionId, new CCEFlightReservationResponseByCartId().ObjectName }).ConfigureAwait(false);
            var flightReservationResponse = persistData?.FirstOrDefault(p => string.Equals(p.CartId, cartId, StringComparison.OrdinalIgnoreCase));
            var response = default(FlightReservationResponse);
            if (flightReservationResponse != null)
            {
                response = flightReservationResponse.CslFlightReservationResponse;
            }

            return response;
        }

        private bool IsEnableOmniCartReleaseCandidateTwoChanges_Bundles(int applicationId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableOmniCartReleaseCandidateTwoChanges_Bundles") && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, "Android_EnableOmniCartReleaseCandidateTwoChanges_Bundles_AppVersion", "iPhone_EnableOmniCartReleaseCandidateTwoChanges_Bundles_AppVersion");
        }

        private async Task<string> CreateCart(MOBRequest request, Session session)
        {
            string response = string.Empty;

            try
            {
                string jsonRequest = JsonConvert.SerializeObject(string.Empty);
                string cslResponse = await _shoppingCartService.CreateCart(session.Token, jsonRequest, session.SessionId).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(cslResponse))
                {
                    response = (cslResponse);
                }
            }
            catch (System.Exception ex)
            {
                response = null;
                MOBExceptionWrapper exceptionWrapper = new MOBExceptionWrapper(ex);
                throw new System.Exception(exceptionWrapper.Message.ToString());
            }

            return response;
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
                var registerOfferRequest = new RegisterOfferRequest();
                registerOfferRequest.CartId = request.CartId;
                registerOfferRequest.CountryCode = TicketingCountryCode(reservationDetail.Detail.PointOfSale);
                registerOfferRequest.LangCode = request.LanguageCode;
                registerOfferRequest.Reservation = reservationDetail.Detail;

                var jsonRequest = JsonConvert.SerializeObject(registerOfferRequest);
                string cslResponse = await _shoppingCartService.RegisterFareLockReservation(session.Token, actionName, jsonRequest, session.SessionId).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(jsonRequest))
                {
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

        private string TicketingCountryCode(Service.Presentation.CommonModel.PointOfSale pointOfSale)
        {
            return pointOfSale != null && pointOfSale.Country != null ? pointOfSale.Country.CountryCode : string.Empty;
        }

        private async Task<List<MOBSHOPPrice>> GetPrices(List<United.Services.FlightShopping.Common.DisplayCart.DisplayPrice> prices, bool isAwardBooking, string sessionId, string flow, bool isReshopChange = false, string searchType = null, bool isFareLockViewRes = false, bool isCorporateFare = false, int appId = 0, string appVersion = "", FlightReservationResponse shopBookingDetailsResponse = null, Session session = null)
        {
            List<MOBSHOPPrice> bookingPrices = new List<MOBSHOPPrice>();
            CultureInfo ci = null;
            bool isEnableOmniCartMVP2Changes = _configuration.GetValue<bool>("EnableOmniCartMVP2Changes");
            foreach (var price in prices)
            {
                if (ci == null)
                {
                    ci = _etcUtility.GetCultureInfo(price.Currency);
                }

                MOBSHOPPrice bookingPrice = new MOBSHOPPrice();
                decimal NonDiscountTravelPrice = 0;
                if (!ConfigUtility.IsViewResFlowPaymentSvcEnabled(flow))
                {
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
                                FormattedPromoDisplayValue = "-" + promoValue.ToString("C2", CultureInfo.CurrentCulture)
                            } : null;
                        }
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
                        if (!ConfigUtility.EnableYADesc(isReshopChange) || (price.PricingPaxType != null && !price.PricingPaxType.Equals("UAY")))
                        {
                            desc = UtilityHelper.GetFareDescription(price);
                        }
                    }
                    if (ConfigUtility.EnableYADesc(isReshopChange) && !string.IsNullOrEmpty(price.PricingPaxType) && price.PricingPaxType.ToUpper().Equals("UAY"))
                    {
                        bookingPrice.PriceTypeDescription = UtilityHelper.BuildYAPriceTypeDescription(searchType);
                        bookingPrice.PaxTypeDescription = $"{price?.Count} {"young adult (18-23)"}".ToLower();
                    }
                    else
                    if (price.Description.ToUpper().Contains("TOTAL"))
                    {
                        bookingPrice.PriceTypeDescription = price?.Description.ToLower(); 
                        if (prices.Any(x => x.Type.Equals("TravelCredits", StringComparison.OrdinalIgnoreCase) && x.Amount > 0)
                            && price.StrikeThroughPricing > 0 && (int)price.StrikeThroughPricing >= (int)price.Amount)
                        {
                            bookingPrice.StrikeThroughDisplayValue = ((int)price.StrikeThroughPricing - (int)price.Amount).ToString("C2", CultureInfo.CurrentCulture);
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
                            bookingPrice.StrikeThroughDisplayValue = FormatAwardAmountForDisplay(price.StrikeThroughPricing.ToString(), true);
                            bookingPrice.StrikeThroughDescription = BuildStrikeThroughDescription();
                        }
                        bookingPrice.PriceTypeDescription = UtilityHelper.BuildPriceTypeDescription(searchType, price.Description, price.Count, desc, isFareLockViewRes, isCorporateFare);
                        if (isEnableOmniCartMVP2Changes)
                        {
                            string description = price?.Description;
                            if (!ConfigUtility.IsViewResFlowPaymentSvcEnabled(flow)
                                && _shoppingUtility.EnableShoppingcartPhase2ChangesWithVersionCheck(appId, appVersion) && !isReshopChange && !string.IsNullOrEmpty(session?.TravelType)
                                && (session.TravelType == TravelType.CB.ToString() || session.TravelType == TravelType.CLB.ToString()))
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
                        await _shoppingUtility.ValidateAwardMileageBalance(sessionId, price.Amount);
                    }
                }

                double tempDouble = 0;
                double.TryParse(NonDiscountTravelPrice > 0 ? NonDiscountTravelPrice.ToString() : price.Amount.ToString(), out tempDouble);
                bookingPrice.Value = Math.Round(tempDouble, 2, MidpointRounding.AwayFromZero);

                if (price.Currency.ToUpper() == "MIL")
                {
                    bookingPrice.FormattedDisplayValue = FormatAwardAmountForDisplay(price.Amount.ToString(), false);
                }
                else
                {
                    bookingPrice.FormattedDisplayValue = FormatAmountForDisplay((NonDiscountTravelPrice > 0 ? NonDiscountTravelPrice : price.Amount).ToString(), ci, false);

                    if (!ConfigUtility.IsViewResFlowPaymentSvcEnabled(flow) && _configuration.GetValue<bool>("EnableMilesPlusMoney")
                        && string.Equals("MILESANDMONEY", price.Type, StringComparison.OrdinalIgnoreCase))
                    {
                        bookingPrice.FormattedDisplayValue = "-" + FormatAmountForDisplay((price.Amount).ToString(), ci, false); // Money and Miles have to be displayed in -ve format as per MOBILE-14807
                    }
                }

                _shoppingUtility.UpdatePricesForTravelCredits(bookingPrices, price, bookingPrice, session);
                bookingPrices.Add(bookingPrice);
            }

            return bookingPrices;
        }

        private string FormatAwardAmountForDisplay(string amt, bool truncate = true)
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

        private string FormatAmountForDisplay(string amt, CultureInfo ci, bool roundup = true, bool isAward = false)
        {
            string newAmt = amt;
            decimal amount = 0;
            decimal.TryParse(amt, out amount);

            try
            {
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
                        newAmt = TopHelper.GetCurrencySymbol(ci, amount, roundup);
                        break;
                }

            }
            catch { }

            return isAward ? "+ " + newAmt : newAmt;
        }

        private string BuildStrikeThroughDescription()
        {
            return _configuration.GetValue<string>("StrikeThroughPriceTypeDescription");
        }
        public async Task<List<MOBSection>> GetPaymentMessagesForWLPNRViewRes(FlightReservationResponse flightReservationResponse, Collection<ReservationFlightSegment> FlightSegments, string Flow)
        {
            var pcuWaitListManageResMessage = new List<MOBSection>();
            bool isPCUPurchase = Flow.Equals(FlowType.VIEWRES.ToString()) && !flightReservationResponse.IsNull() && !flightReservationResponse.ShoppingCart.IsNull() ? flightReservationResponse.ShoppingCart.Items.Where(x => x.Product.FirstOrDefault().Code != "RES").Select(x => x.Product.FirstOrDefault().Code).Distinct().Any(x => x.Equals("PCU")) : false;
            if (_configuration.GetValue<bool>("EnablePCUWaitListPNRManageRes") && isPCUPurchase && IsWaitListPNRFromCharacteristics(FlightSegments))
            {
                var getPCUSegmentFromTravelOptions = getPCUSegments(flightReservationResponse, Flow);
                var segments = getPCUSegmentFromTravelOptions != null ? getPCUSegmentFromTravelOptions.Select(x => x.SegmentNumber).Distinct().ToList() : null;
                var getPCUFlightSegments = segments != null ? FlightSegments.Where(y => y != null && y.FlightSegment != null && HasPCUSelectedSegments(segments, y.FlightSegment.SegmentNumber)).ToCollection() : null;
                var getAllPCUSegmentsIncludingWL = IsSelectedPCUWaitListSegment(getPCUFlightSegments, FlightSegments);
                var isWaitListPCU = IsWaitListPNRFromCharacteristics(getAllPCUSegmentsIncludingWL);
                if (isWaitListPCU)
                {
                    var isUPP = isPCUUPPWaitListSegment(getPCUSegmentFromTravelOptions, FlightSegments);
                    var refundMessages = isUPP ? await _shoppingCartUtility.GetCaptions("PI_PCUWaitList_RefundMessage_UPPMessage") : await _shoppingCartUtility.GetCaptions("PI_PCUWaitList_RefundMessage_GenericMessage");
                    pcuWaitListManageResMessage = _checkoutUtiliy.AssignRefundMessage(refundMessages);
                }
            }
            return pcuWaitListManageResMessage.Any() ? pcuWaitListManageResMessage : null;
        }
        private static bool HasPCUSelectedSegments(List<string> segments, int segmentNumber)
        {
            return segments.Any(x => x != null && Convert.ToInt32(x) == segmentNumber);
        }
        private bool IsWaitListPNRFromCharacteristics(Collection<ReservationFlightSegment> FlightSegments)
        {
            var flightSegmentCharacteristics = FlightSegments.Where(t => t != null).SelectMany(t => t.Characteristic).ToCollection();
            return flightSegmentCharacteristics.Any(p => p != null && !p.Code.IsNullOrEmpty() && !p.Value.IsNullOrEmpty() && p.Code.Equals("Waitlisted", StringComparison.OrdinalIgnoreCase) && p.Value.Equals("True", StringComparison.OrdinalIgnoreCase));
        }
        public List<SubItem> getPCUSegments(FlightReservationResponse flightReservationResponse, string Flow)
        {
            var segments = flightReservationResponse.DisplayCart.TravelOptions.Where(d => d != null && d.Key == "PCU").SelectMany(x => x.SubItems).Where(x => _shoppingCartUtility.ShouldIgnoreAmount(x) ? true : x.Amount != 0).ToList();
            if (segments != null && segments.Any())
                segments.OrderBy(x => x.SegmentNumber).GroupBy(x => x.SegmentNumber);
            return segments.Any() ? segments : null;
        }
        public static Collection<ReservationFlightSegment> IsSelectedPCUWaitListSegment(Collection<ReservationFlightSegment> PCUFlightSegments, Collection<ReservationFlightSegment> FlightSegments)
        {
            return FlightSegments.Where(x => x != null && x.FlightSegment != null && isFlightMatching(x.FlightSegment.DepartureAirport.IATACode, x.FlightSegment.ArrivalAirport.IATACode, x.FlightSegment.FlightNumber, x.EstimatedDepartureTime, PCUFlightSegments)).ToCollection();
        }
        private static bool isFlightMatching(string origin, string destination, string flightNumber, string flightDate, Collection<ReservationFlightSegment> PCUFlightSegments, BookingClass BookingClass = null, bool isUPP = false)
        {
            return PCUFlightSegments.Any(x => x != null && x.FlightSegment != null && x.FlightSegment.DepartureAirport.IATACode == origin &&
                                     x.FlightSegment.ArrivalAirport.IATACode == destination &&
                                     x.FlightSegment.FlightNumber == flightNumber);
        }
        public bool isPCUUPPWaitListSegment(List<SubItem> PCUTravelOptions, Collection<ReservationFlightSegment> FlightSegments)
        {
            var hasPCUUPPSegment = PCUTravelOptions.Where(t => t != null && t.Description != null && t.Description.ToUpper().Equals("UNITED PREMIUM PLUS")).Select(t => t.SegmentNumber).Distinct().ToCollection();
            bool isBusiness = false;
            if (!hasPCUUPPSegment.IsNull() && hasPCUUPPSegment.Any())
            {
                foreach (var segment in hasPCUUPPSegment)
                {
                    var selectedSegment = FlightSegments.Where(y => y != null && y.FlightSegment != null && y.FlightSegment.SegmentNumber == Convert.ToInt32(segment)).FirstOrDefault().TripNumber;
                    var flightSegmentCharacteristics = selectedSegment != null ? FlightSegments.Where(x => !x.TripNumber.IsNullOrEmpty() && !selectedSegment.IsNullOrEmpty() && x.TripNumber == selectedSegment && x.BookingClass != null && x.BookingClass.Code != null && (x.BookingClass.Code.Equals("PZ") || x.BookingClass.Code.Equals("PN"))).Select(t => t.Characteristic).FirstOrDefault() : null;
                    isBusiness = flightSegmentCharacteristics != null && flightSegmentCharacteristics.Any() ? flightSegmentCharacteristics.Any(p => p != null && !p.Code.IsNullOrEmpty() && !p.Value.IsNullOrEmpty() && p.Code.Equals("Waitlisted", StringComparison.OrdinalIgnoreCase) && p.Value.Equals("True", StringComparison.OrdinalIgnoreCase)) : false;
                    if (isBusiness)
                        return isBusiness;
                }
            }
            return isBusiness;
        }
        #region REGISTER SEATS
        public async Task<MOBRegisterSeatsResponse> RegisterSeats(MOBRegisterSeatsRequest request)
        {
            Session session = null;
            MOBRegisterSeatsResponse response = new MOBRegisterSeatsResponse();

            _logger.LogInformation("RegisterSeats_CFOP {Request} {SessionId} {ApplicationId} {ApplicationVersion} {DeviceId}", JsonConvert.SerializeObject(request), request.SessionId, request.Application.Id, request.Application.Version.Major, request.DeviceId);


            if (!string.IsNullOrEmpty(request.SessionId))
            {
                session = await _mscshoppingSessionHelper.GetValidateSession(request.SessionId, false, true);
                session.Flow = request.Flow;
            }
            else
            {
                _logger.LogInformation("RegisterSeats_CFOP {CFOP - Session Null} {Request} {SessionId} {ApplicationId} {ApplicationVersion} {DeviceId}", JsonConvert.SerializeObject(request), request.SessionId, request.Application.Id, request.Application.Version.Major, request.DeviceId);
                if (ConfigUtility.VersionCheck_NullSession_AfterAppUpgradation(request))
                    throw new MOBUnitedException(((int)MOBErrorCodes.ViewResCFOP_NullSession_AfterAppUpgradation).ToString(), _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                else
                    throw new MOBUnitedException(((int)MOBErrorCodes.ViewResCFOPSessionExpire).ToString(), _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }

            response.SessionId = request.SessionId;
            response.Flow = request.Flow;
            response.Request = request;
            response.PriceBreakDownTitle = "Price details";


            if (GeneralHelper.ValidateAccessCode(request.AccessCode))
            {
                SeatChangeState state = new SeatChangeState();
                state = _sessionHelperService.GetSession<SeatChangeState>(request.SessionId, state.ObjectName, new List<string> { request.SessionId, state.ObjectName }).Result;
                if (state == null)
                {
                    throw new MOBUnitedException("Unable to retrieve information needed for seat change.");
                }

                response.SelectedTrips = state.Trips;

                bool isMFOPEnabled = ConfigUtility.IsMFOPCatalogEnabled(request.CatalogValues);
                bool isVerticalSeatMapEnabled = await _featureSettings.GetFeatureSettingValue("EnableMultiSegmentSeatChange");

                if (!request.IsMiles)
                {
                    if (isVerticalSeatMapEnabled && request.SelectedSeats != null && request.SelectedSeats.Count > 0)
                    {
                        AddSeats(ref state, request, isMFOPEnabled);
                    }
                    else
                    {
                        AddSeats(ref state, request.Origin, request.Destination, request.FlightNumber, request.FlightDate, request.PaxIndex, request.SeatAssignment);

                        try
                        {
                            #region 
                            List<MOBSeatMap> MOBSeatMapList = _sessionHelperService.GetSession<List<MOBSeatMap>>(request.SessionId, ObjectNames.MOBSeatMapListFullName, new List<string> { request.SessionId, ObjectNames.MOBSeatMapListFullName }).Result;
                            if (MOBSeatMapList != null && MOBSeatMapList.Count > 0 && state != null && state.Seats != null)
                            {
                                _logger.LogInformation("RegisterSeats_CFOP {SeatMPCache2ValidatPriceManuplate} {SessionId} {ApplicationId} {ApplicationVersion} and {Transactionid}", MOBSeatMapList, request.SessionId, request.Application.Id, request.Application.Version.DisplayText, request.TransactionId);

                                List<MOBSeat> unavailableSeats = new List<MOBSeat>();
                                bool seatChangeToggle = _configuration.GetValue<bool>("SeatUpgradeForUnavailableSeatCheck");

                                if (_configuration.GetValue<bool>("FixSeatNotFoundManageResObjRefExceptionInRegisterSeatsAction"))
                                {
                                    #region Fix for RegisterSeats_CFOP action for SeatNotFound ManageRes ObjRefException(JIRA MOBILE - 6584) - by Shashank
                                    foreach (MOBSeat seat in state.Seats)
                                    {
                                        if (!string.IsNullOrEmpty(seat.SeatAssignment) && seat.Origin == request.Origin && seat.Destination == request.Destination && seat.FlightNumber == request.FlightNumber && Convert.ToDateTime(seat.DepartureDate).ToString("MMM., dd, yyyy") == Convert.ToDateTime(request.FlightDate).ToString("MMM., dd, yyyy"))
                                        {
                                            List<MOBSeatB> mobSeatB = (from list in MOBSeatMapList
                                                                       from cabin in list.Cabins
                                                                       from row in cabin.Rows
                                                                       from se in row.Seats
                                                                       where se.Number.ToUpper().Trim() == seat.SeatAssignment.ToUpper().Trim()
                                                                       select se).ToList();

                                            if (mobSeatB[0].ServicesAndFees != null && mobSeatB[0].ServicesAndFees.Count > 0)
                                            {
                                                seat.Price = Convert.ToDecimal(mobSeatB[0].ServicesAndFees[0].TotalFee);
                                                seat.PcuOfferOptionId = mobSeatB[0].PcuOfferOptionId;

                                                if (isMFOPEnabled)
                                                {
                                                    seat.Miles = mobSeatB[0].ServicesAndFees[0].MilesFee;
                                                    seat.DisplayMiles = mobSeatB[0].ServicesAndFees[0].DisplayMilesFee;
                                                    seat.MilesAfterTravelerCompanionRules = mobSeatB[0].ServicesAndFees[0].MilesFee;
                                                }
                                            }
                                            if (seatChangeToggle && mobSeatB[0].seatvalue == "X")
                                            {
                                                unavailableSeats.Add(seat);
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    foreach (MOBSeat seat in state.Seats)
                                    {
                                        if (seat.SeatAssignment != string.Empty && seat.Origin == request.Origin && seat.Destination == request.Destination)
                                        {
                                            List<MOBSeatB> mobSeatB = (from list in MOBSeatMapList
                                                                       from cabin in list.Cabins
                                                                       from row in cabin.Rows
                                                                       from se in row.Seats
                                                                       where se.Number.ToUpper().Trim() == seat.SeatAssignment.ToUpper().Trim()
                                                                       select se).ToList();
                                            if (mobSeatB[0].ServicesAndFees != null && mobSeatB[0].ServicesAndFees.Count > 0)
                                            {
                                                seat.Price = Convert.ToDecimal(mobSeatB[0].ServicesAndFees[0].TotalFee);
                                                seat.PcuOfferOptionId = mobSeatB[0].PcuOfferOptionId;

                                                if (isMFOPEnabled)
                                                {
                                                    seat.Miles = mobSeatB[0].ServicesAndFees[0].MilesFee;
                                                    seat.DisplayMiles = mobSeatB[0].ServicesAndFees[0].DisplayMilesFee;
                                                    seat.MilesAfterTravelerCompanionRules = mobSeatB[0].ServicesAndFees[0].MilesFee;
                                                }
                                            }
                                            if (seatChangeToggle && mobSeatB[0].seatvalue == "X")
                                            {
                                                unavailableSeats.Add(seat);
                                            }
                                        }
                                    }
                                }

                                if (seatChangeToggle && unavailableSeats.Count > 0)
                                {
                                    foreach (var seat in unavailableSeats)
                                    {
                                        state.Seats.Remove(seat);
                                    }
                                }
                            }
                            #endregion
                        }
                        catch (Exception ex)
                        {
                            if (_configuration.GetValue<string>("SeatNotFoundAtCompleteSeatsSelection") != null)
                            {
                                string exMessage = ex.Message != null ? ex.Message : "";
                                throw new Exception(_configuration.GetValue<string>("SeatNotFoundAtCompleteSeatsSelection") + " - " + exMessage);
                            }
                        }
                    }
                }

                bool isCSLSeatMap = IsEnableXmlToCslSeatMapMigration(request.Application.Id, request.Application.Version.Major);

                bool isCSL30SeatMap = _configuration.GetValue<bool>("EnableCSL30ManageResSelectSeatMap");
                if (isCSL30SeatMap)
                {
                    state.Seats = await ApplyTravelerCompanionRulesCSl30(request.SessionId, state.Seats, state.RecordLocator, state.PNRCreationDate, state.BookingTravelerInfo, request.LanguageCode, request.Application.Id, request.Application.Version.Major, state.Segments, request.DeviceId, isMFOPEnabled);
                }
                else
                {
                    state.Seats = ApplyTravelerCompanionRulesCSL(request.SessionId, state.Seats, state.RecordLocator, state.PNRCreationDate, state.BookingTravelerInfo, request.LanguageCode, request.Application.Id, request.Application.Version.Major, state.Segments, request.DeviceId);
                }

                response.Seats = state.Seats;

                foreach (MOBBKTraveler traveler in state.BookingTravelerInfo)
                {
                    IEnumerable<MOBSeat> seatList = from s in state.Seats
                                                    where s.TravelerSharesIndex == traveler.SHARESPosition
                                                    select s;
                    if (seatList.Count() > 0)
                    {
                        List<MOBSeat> travelerSeats = new List<MOBSeat>();
                        travelerSeats = seatList.ToList();
                        travelerSeats = travelerSeats.OrderBy(s => s.Key).ToList();
                        traveler.Seats = travelerSeats;
                    }
                }
                response.bookingTravlerInfo = state.BookingTravelerInfo;

                state.SeatPrices = ComputeSeatPrices(state.Seats, isMFOPEnabled);

                response.SeatPrices = state.SeatPrices;

                bool isEnableTravelOptionsInViewRes = IsEnableTravelOptionsInViewRes(request.Application.Id, request?.Application?.Version?.Major, session?.CatalogItems);

                if ((isEnableTravelOptionsInViewRes && request.Flow == FlowType.VIEWRES_BUNDLES_SEATMAP.ToString()) || _configuration.GetValue<bool>("EnableSeatMapCartIdFix"))
                {
                    MOBShoppingCart persistShoppingCart = new MOBShoppingCart();
                    var shoppingCart = _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, persistShoppingCart.ObjectName, new List<string> { request.SessionId, persistShoppingCart.ObjectName }).Result;
                    request.CartId = shoppingCart.CartId;
                }

                if (ShouldCreateNewCart(request))
                {
                    request.CartId = await CreateCart(request, session);

                    MOBShoppingCart persistShoppingCart = new MOBShoppingCart();
                    await _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, session.SessionId, new List<string> { session.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName);
                }

                response.ShoppingCart = await RegisterSeats(request, request.CartId, session.Token, state, request.Flow, request.IsMiles, isMFOPEnabled);

                if (response.ShoppingCart == null && request.Flow == FlowType.VIEWRES_BUNDLES_SEATMAP.ToString())
                {
                    MOBShoppingCart persistShoppingCart = new MOBShoppingCart();
                    response.ShoppingCart = _sessionHelperService.GetSession<MOBShoppingCart>(session.SessionId, persistShoppingCart.ObjectName, new List<string> { session.SessionId, persistShoppingCart.ObjectName }).Result;
                    // Adding this to pass View res flow when no seat is selected through bundle flow.
                    if (!_configuration.GetValue<bool>("DisablePassViewResFlowWhenNoSeatIsSelectedInBundlesFlow") && response.ShoppingCart != null && response.ShoppingCart.Flow == FlowType.VIEWRES_BUNDLES_SEATMAP.ToString())
                    {
                        response.ShoppingCart.Flow = FlowType.VIEWRES.ToString();
                        await _sessionHelperService.SaveSession<MOBShoppingCart>(response.ShoppingCart, session.SessionId, new List<string> { session.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName);
                    }
                }

                decimal.TryParse(response?.ShoppingCart?.TotalPrice, out decimal totalPrice);

                if (response.SeatPrices == null && totalPrice == 0)
                {
                    if (response.ShoppingCart != null)
                    {
                        MOBCheckOutRequest checkOutRequest = new MOBCheckOutRequest()
                        {
                            SessionId = request.SessionId,
                            CartId = request.CartId,
                            Application = request.Application,
                            AccessCode = request.AccessCode,
                            LanguageCode = request.LanguageCode,
                            FormofPaymentDetails = new MOBFormofPaymentDetails
                            {
                                FormOfPaymentType = null
                            },
                            Flow = request.Flow
                        };

                        MOBCheckOutResponse checkOutResponse = new MOBCheckOutResponse();
                        FlightReservationResponse flightReservationResponse = new FlightReservationResponse();
                        flightReservationResponse = await _checkoutUtiliy.RegisterFormsOfPayments_CFOP(checkOutRequest, session);
                        checkOutResponse = await _checkoutUtiliy.ViewResRFOPResponse(flightReservationResponse, checkOutRequest, response.ShoppingCart, session);
                        if (checkOutResponse.ShoppingCart != null && checkOutResponse.ShoppingCart.AlertMessages != null && checkOutResponse.ShoppingCart.AlertMessages.Any())
                        {
                            response.SeatAssignMessages = new List<string> { _configuration.GetValue<string>("SeatsUnAssignedMessage").Trim() };
                        }
                        else
                        {
                            response.SeatAssignMessages = checkOutResponse.SeatAssignMessages;
                        }
                    }

                    var pnrByRecordLocatorRequest = new MOBPNRByRecordLocatorRequest
                    {
                        Application = request.Application,
                        AccessCode = request.AccessCode,
                        LanguageCode = request.LanguageCode,
                        RecordLocator = state.RecordLocator,
                        LastName = state.LastName,
                        DeviceId = request.DeviceId,
                        TransactionId = request.TransactionId,
                        MileagePlusNumber = request.MileagePlusNumber,
                        Flow = FlowType.VIEWRES.ToString(),
                        CatalogValues = session?.CatalogItems,
                        SessionId = request.SessionId,
                    };
                    var jsonRequest = JsonConvert.SerializeObject(pnrByRecordLocatorRequest);
                    var jsonResponse = await _getPNRByRecordLocatorService.GetPNRByRecordLocator(jsonRequest, request.TransactionId, "/ManageReservation/GetPNRByRecordLocator");
                    var reservationResponse = JsonConvert.DeserializeObject<MOBPNRByRecordLocatorResponse>(jsonResponse.ToString());
                    response.PNR = reservationResponse.PNR;
                    response.TripInsuranceInfo = reservationResponse.TripInsuranceInfo;
                    response.PremierAccess = reservationResponse.PremierAccess;
                    response.ShowPremierAccess = response.PremierAccess != null;
                    response.DotBaggageInformation = reservationResponse.DotBaggageInformation;
                    response.Ancillary = reservationResponse.Ancillary;
                    response.Flow = reservationResponse.Flow;
                    if (!_configuration.GetValue<bool>("DisableRegisterSeatResponseFlowMissingInFreeSeatFlow") && string.IsNullOrEmpty(response.Flow))
                    {
                        response.Flow = request.Flow;
                    }
                    response.PNR.IsEnableEditTraveler = !reservationResponse.PNR.IsGroup;
                    response.ShowSeatChange = ShowSeatChangeButton(response.PNR);
                    response.RewardPrograms = reservationResponse.RewardPrograms;
                    response.SpecialNeeds = reservationResponse.SpecialNeeds;
                    if (isEnableTravelOptionsInViewRes)
                        response.TravelOptions = reservationResponse.TravelOptions;
                    if (await _featureToggles.IsEnableFreeSeatConfirmationMessageOnMyTrips(request.Application.Id, request.Application.Version.Major))
                    {

                        if (response.PNR.AdvisoryInfo == null)
                            response.PNR.AdvisoryInfo = new List<MOBPNRAdvisory>();

                        response.PNR.AdvisoryInfo.Add(await _checkoutUtiliy.PopulateSeatConfirmarionAlertMessage(session, request));
                    }

                }
                else
                {
                    if (response.ShoppingCart != null)
                    {
                        if (_configuration.GetValue<bool>("GetFoPOptionsAlongRegisterOffers"))
                        {
                            bool isDefault = false;
                            if (ConfigUtility.IsManageResETCEnabled(request.Application.Id, request.Application.Version.Major))
                            {
                                bool isMilesFOPEligible = (request.Flow == FlowType.VIEWRES.ToString() || request.Flow == FlowType.VIEWRES_SEATMAP.ToString()) && state?.Seats?.Count() > 0 && !state.Seats.Any(x => x.Miles == 0 && x.Price > 0);
                                await BuildMilesFOP(request, session, response, isMilesFOPEligible).ConfigureAwait(false);
                                var tupleRes = await _mscFormsOfPayment.GetEligibleFormofPayments(request, session, response.ShoppingCart, request.CartId, request.Flow, null, isMilesFOPEligible);
                                response.EligibleFormofPayments = tupleRes.EligibleFormofPayments;
                                isDefault = tupleRes.isDefault;
                            }
                            else
                            {
                                MOBFOPEligibilityRequest eligiblefopRequest = new MOBFOPEligibilityRequest()
                                {
                                    TransactionId = request.TransactionId,
                                    DeviceId = request.DeviceId,
                                    AccessCode = request.AccessCode,
                                    LanguageCode = request.LanguageCode,
                                    Application = request.Application,
                                    CartId = request.CartId,
                                    SessionId = request.SessionId,
                                    Flow = request.Flow,
                                    Products = await GetProductsForEligibleFopRequest(response.ShoppingCart, state)
                                };
                                var tupleResponse = await _mscFormsOfPayment.EligibleFormOfPayments(eligiblefopRequest, session, GetIsMilesFOPEnabled(response.ShoppingCart));
                                response.EligibleFormofPayments = tupleResponse.EligibleFormofPayments;
                                isDefault = tupleResponse.isDefault;
                                var upliftFop = _mscFormsOfPayment.UpliftAsFormOfPayment(null, response.ShoppingCart);
                                if (upliftFop != null && response.EligibleFormofPayments != null)
                                {
                                    response.EligibleFormofPayments.Add(upliftFop);
                                }
                            }
                            response.IsDefaultPaymentOption = isDefault;
                            await _sessionHelperService.SaveSession<List<FormofPaymentOption>>(response.EligibleFormofPayments, request.SessionId, new List<string> { request.SessionId, new FormofPaymentOption().GetType().FullName }, new FormofPaymentOption().GetType().FullName);
                        }


                    }
                    response.Flow = request.Flow == FlowType.VIEWRES_BUNDLES_SEATMAP.ToString() ? FlowType.VIEWRES.ToString() : request.Flow;

                    response.PkDispenserPublicKey = await _mscPkDispenserPublicKey.GetCachedOrNewpkDispenserPublicKey(request.Application.Id, request.Application.Version.Major, request.DeviceId, request.SessionId, session.Token, session.CatalogItems);

                    if (!_configuration.GetValue<bool>("DisableManageResChanges23C"))
                    {
                        response.PkDispenserPublicKey = await _mscPkDispenserPublicKey.GetCachedOrNewpkDispenserPublicKey(request.Application.Id, request.Application.Version.Major, request.DeviceId, session.SessionId, session.Token, request.Flow, session?.CatalogItems).ConfigureAwait(false);
                    }
                }

                await _sessionHelperService.SaveSession<SeatChangeState>(state, state.SessionId, new List<string> { state.SessionId, state.ObjectName }, state.ObjectName);
            }
            else
            {
                throw new MOBUnitedException("The access code you provided is not valid.");
            }

            return response;
        }

            private async System.Threading.Tasks.Task BuildMilesFOP(MOBRegisterUsingMilesRequest request, Session session, MOBRegisterOfferResponse response, bool isMilesFOPEligible)
        {

            if (response.ShoppingCart.FormofPaymentDetails == null)
                response.ShoppingCart.FormofPaymentDetails = new MOBFormofPaymentDetails();

            United.Definition.Common.MOBVerifyMileagePlusHashpinRequest verifyMileagePlusHashpinRequest = new United.Definition.Common.MOBVerifyMileagePlusHashpinRequest();
            verifyMileagePlusHashpinRequest.AppVersion = request.Application.Version.Major;
            verifyMileagePlusHashpinRequest.MpNumber = request.MileagePlusNumber;
            verifyMileagePlusHashpinRequest.ApplicationId = request.Application.Id;
            verifyMileagePlusHashpinRequest.ServiceName = "RegisterUsingMiles.MilesFOP";
            verifyMileagePlusHashpinRequest.DeviceId = request.DeviceId;
            verifyMileagePlusHashpinRequest.SessionID = request.SessionId;
            verifyMileagePlusHashpinRequest.HashValue = request.HashPinCode;
            verifyMileagePlusHashpinRequest.TransactionId = request.TransactionId;
            string verifyMileagePlusHashpinRequestJson = JsonConvert.SerializeObject(verifyMileagePlusHashpinRequest);
            var validateHashPinServiceResponse = await _verifyMileagePlusHashpinService.VerifyMileagePlusHashpin<MOBVerifyMileagePlusHashpinResponse>(session.Token, verifyMileagePlusHashpinRequestJson, session.SessionId);

            if (validateHashPinServiceResponse.mpDetails != null && !request.MileagePlusNumber.IsNullOrEmpty())
            {
                string mpNumber = request.MileagePlusNumber;
                string cslMemberProfileResp = await _memberInformationService.GetMemberInformation(session.Token, mpNumber, true).ConfigureAwait(false);
                CslMemberProfile memberInfoResponse = JsonConvert.DeserializeObject<CslMemberProfile>(cslMemberProfileResp);

                int totalMiles = 0;
                foreach (var product in response.ShoppingCart.Products)
                {
                    if (!product.ProdTotalRequiredMiles.IsNullOrEmpty())
                        totalMiles += Convert.ToInt32(product.ProdTotalRequiredMiles);
                    foreach (var segment in product.Segments)
                    {
                        foreach (var subsegment in segment.SubSegmentDetails)
                        {
                            subsegment.DisplayMilesPrice = UtilityHelper.FormatAwardAmountForDisplay(subsegment.MilesPrice, false);
                        }
                    }
                }
                //response.ShoppingCart.TotalPrice = totalMiles.ToString();
                var balance = memberInfoResponse.Data.Balances?.FirstOrDefault(s => s.Currency == "RDM");
                int accAvailableMiles = balance == null ? 0 : Convert.ToInt32(balance.Amount);
                int remainingMiles = accAvailableMiles - totalMiles;

                MOBMilesFOP milesFOPobj = new MOBMilesFOP
                {
                    RemainingMiles = remainingMiles,
                    AvailableMiles = accAvailableMiles,
                    RequiredMiles = totalMiles,
                    CustomerId = memberInfoResponse.Data.CustomerId,
                    ProfileOwnerMPAccountNumber = request.MileagePlusNumber,
                    DisplayAvailableMiles = UtilityHelper.FormatAwardAmountForDisplay(accAvailableMiles.ToString(), false),
                    DisplayTotalRequiredMiles = UtilityHelper.FormatAwardAmountForDisplay(totalMiles.ToString(), false),
                    DisplayRequiredMiles = '-' + UtilityHelper.FormatAwardAmountForDisplay(totalMiles.ToString(), false),
                    IsMilesFOPEligible = isMilesFOPEligible
                };
                if (remainingMiles > -1)
                {
                    milesFOPobj.DisplayRemainingMiles = UtilityHelper.FormatAwardAmountForDisplay(remainingMiles.ToString(), false);
                    milesFOPobj.HasEnoughMiles = true;
                    response.ShoppingCart.DisplayMessage = null;
                }
                else
                {
                    milesFOPobj.HasEnoughMiles = false;
                    if (response.ShoppingCart.DisplayMessage.IsNullOrEmpty())
                    {
                        response.ShoppingCart.DisplayMessage = new List<MOBSection>();
                    }
                    var InSufficientMilesMessage = _shoppingCartUtility.AddTextForInsufficientMiles();
                    if (!response.ShoppingCart.DisplayMessage.Any(x => x.Text1 == InSufficientMilesMessage.Text1))
                        response.ShoppingCart.DisplayMessage.Add(InSufficientMilesMessage);
                }
                response.ShoppingCart.TermsAndConditions = await _shoppingCartUtility.GetBagsTermsAndConditions(session.SessionId, true);
                response.ShoppingCart.FormofPaymentDetails.FormOfPaymentType = MOBFormofPayment.MilesFOP.ToString();
                response.ShoppingCart.FormofPaymentDetails.MilesFOP = milesFOPobj;
            }
            else
            {
                throw new Exception(_configuration.GetValue<string>("MilesFOPNotLoggedInMessage"));
            }
            //response.ShoppingCart.RequestObjectName = MOBRequestObjectName.RegisterBags;
            response.ShoppingCart.FormofPaymentDetails.MilesFOP.IsMilesFOPEligible = isMilesFOPEligible;

            await _sessionHelperService.SaveSession<MOBShoppingCart>(response.ShoppingCart, session.SessionId, new List<string> { session.SessionId, response.ShoppingCart.ObjectName }, response.ShoppingCart.ObjectName).ConfigureAwait(false);
        }

        private async System.Threading.Tasks.Task BuildMilesFOP(MOBRegisterSeatsRequest request, Session session, MOBRegisterSeatsResponse response, bool isMilesFOPEligible)
        {
            if (ConfigUtility.IsMFOPCatalogEnabled(request.CatalogValues))
            {
                if (response.ShoppingCart.FormofPaymentDetails == null)
                    response.ShoppingCart.FormofPaymentDetails = new MOBFormofPaymentDetails();

                if (request.IsMiles)
                {
                    United.Definition.Common.MOBVerifyMileagePlusHashpinRequest verifyMileagePlusHashpinRequest = new United.Definition.Common.MOBVerifyMileagePlusHashpinRequest();
                    verifyMileagePlusHashpinRequest.AppVersion = request.Application.Version.Major;
                    verifyMileagePlusHashpinRequest.MpNumber = request.MileagePlusNumber;
                    verifyMileagePlusHashpinRequest.ApplicationId = request.Application.Id;
                    verifyMileagePlusHashpinRequest.ServiceName = "RegisterSeats.MilesFOP";
                    verifyMileagePlusHashpinRequest.DeviceId = request.DeviceId;
                    verifyMileagePlusHashpinRequest.SessionID = request.SessionId;
                    verifyMileagePlusHashpinRequest.HashValue = request.HashPinCode;
                    verifyMileagePlusHashpinRequest.TransactionId = request.TransactionId;
                    string verifyMileagePlusHashpinRequestJson = JsonConvert.SerializeObject(verifyMileagePlusHashpinRequest);
                    var validateHashPinServiceResponse = await _verifyMileagePlusHashpinService.VerifyMileagePlusHashpin<MOBVerifyMileagePlusHashpinResponse>(session.Token, verifyMileagePlusHashpinRequestJson, session.SessionId);

                    if (validateHashPinServiceResponse.mpDetails != null && !request.MileagePlusNumber.IsNullOrEmpty())
                    {
                        string mpNumber = request.MileagePlusNumber;
                        string cslMemberProfileResp = await _memberInformationService.GetMemberInformation(session.Token, mpNumber, true).ConfigureAwait(false);
                        CslMemberProfile memberInfoResponse = JsonConvert.DeserializeObject<CslMemberProfile>(cslMemberProfileResp);


                        int totalMiles = 0;
                        foreach (var product in response.ShoppingCart.Products)
                        {
                            if (!product.ProdTotalRequiredMiles.IsNullOrEmpty())
                                totalMiles += Convert.ToInt32(product.ProdTotalRequiredMiles);
                            foreach (var segment in product.Segments)
                            {
                                foreach (var subsegment in segment.SubSegmentDetails)
                                {
                                    subsegment.DisplayMilesPrice = UtilityHelper.FormatAwardAmountForDisplay(subsegment.MilesPrice, false);
                                }
                            }
                        }
                        //response.ShoppingCart.TotalPrice = totalMiles.ToString();
                        var balance = memberInfoResponse.Data.Balances?.FirstOrDefault(s => s.Currency == "RDM");
                        int accAvailableMiles = balance == null ? 0 : Convert.ToInt32(balance.Amount);
                        int remainingMiles = accAvailableMiles - totalMiles;

                        MOBMilesFOP milesFOPobj = new MOBMilesFOP
                        {
                            RemainingMiles = remainingMiles,
                            AvailableMiles = accAvailableMiles,
                            RequiredMiles = totalMiles,
                            CustomerId = memberInfoResponse.Data.CustomerId,
                            ProfileOwnerMPAccountNumber = request.MileagePlusNumber,
                            DisplayAvailableMiles = UtilityHelper.FormatAwardAmountForDisplay(accAvailableMiles.ToString(), false),
                            DisplayTotalRequiredMiles = UtilityHelper.FormatAwardAmountForDisplay(totalMiles.ToString(), false),
                            DisplayRequiredMiles = '-' + UtilityHelper.FormatAwardAmountForDisplay(totalMiles.ToString(), false),
                            IsMilesFOPEligible = isMilesFOPEligible
                        };
                        if (remainingMiles > -1)
                        {
                            milesFOPobj.DisplayRemainingMiles = UtilityHelper.FormatAwardAmountForDisplay(remainingMiles.ToString(), false);
                            milesFOPobj.HasEnoughMiles = true;
                            response.ShoppingCart.DisplayMessage = null;
                        }
                        else
                        {
                            milesFOPobj.HasEnoughMiles = false;
                            if (response.ShoppingCart.DisplayMessage.IsNullOrEmpty())
                            {
                                response.ShoppingCart.DisplayMessage = new List<MOBSection>();
                            }
                            response.ShoppingCart.DisplayMessage.Add(_shoppingCartUtility.AddTextForInsufficientMiles());
                        }
                        response.ShoppingCart.TermsAndConditions.Add(_shoppingCartUtility.AddMilesTnCForSeatChange());
                        response.ShoppingCart.FormofPaymentDetails.FormOfPaymentType = MOBFormofPayment.MilesFOP.ToString();
                        response.ShoppingCart.FormofPaymentDetails.MilesFOP = milesFOPobj;
                    }
                    else
                    {
                        throw new Exception(_configuration.GetValue<string>("MilesFOPNotLoggedInMessage"));
                    }
                }
                else
                {
                    response.ShoppingCart.FormofPaymentDetails.FormOfPaymentType = response.ShoppingCart.FormofPaymentDetails.FormOfPaymentType == MOBFormofPayment.MilesFOP.ToString()
                                                                                   || response.ShoppingCart.FormofPaymentDetails.FormOfPaymentType.IsNullOrEmpty() ? MOBFormofPayment.CreditCard.ToString() :
                                                                                   response.ShoppingCart.FormofPaymentDetails.FormOfPaymentType;
                    if (response.ShoppingCart.FormofPaymentDetails.MilesFOP == null)
                        response.ShoppingCart.FormofPaymentDetails.MilesFOP = new MOBMilesFOP();
                }
                response.ShoppingCart.FormofPaymentDetails.MilesFOP.IsMilesFOPEligible = isMilesFOPEligible;
                response.ShoppingCart.RequestObjectJSON = DataContextJsonSerializer.Serialize<MOBRegisterSeatsRequest>(request);
                response.ShoppingCart.RequestObjectName = MOBRequestObjectName.RegisterSeats;

                await _sessionHelperService.SaveSession<MOBShoppingCart>(response.ShoppingCart, session.SessionId, new List<string> { session.SessionId, response.ShoppingCart.ObjectName }, response.ShoppingCart.ObjectName).ConfigureAwait(false);
            }
        }
        private async System.Threading.Tasks.Task BuildMilesFOP(MOBShoppingCart shoppingCart, MOBRegisterCheckinSeatsRequest request, string token)
        {
            if (shoppingCart.FormofPaymentDetails == null)
                shoppingCart.FormofPaymentDetails = new MOBFormofPaymentDetails();
            bool isMilesFOPEligible = !shoppingCart.Products.Exists(prd => prd != null && prd.Segments != null
            && prd.Segments.Exists(seg => seg.SubSegmentDetails.Exists(subseg => subseg != null && !string.IsNullOrEmpty(subseg.Price) 
            && Convert.ToDouble(subseg.Price) > 0 && (string.IsNullOrEmpty(subseg.MilesPrice) || Convert.ToInt32(subseg.MilesPrice) == 0))));

            if (request.IsMiles)
            {
                if (!request.MileagePlusNumber.IsNullOrEmpty())
                {
                    string mpNumber = request.MileagePlusNumber;
                    string cslMemberProfileResp = await _memberInformationService.GetMemberInformation(token, mpNumber, true).ConfigureAwait(false);
                    CslMemberProfile memberInfoResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<CslMemberProfile>(cslMemberProfileResp);


                    int totalMiles = 0;
                    foreach (var product in shoppingCart.Products)
                    {
                        if (!product.ProdTotalRequiredMiles.IsNullOrEmpty())
                            totalMiles += Convert.ToInt32(product.ProdTotalRequiredMiles);
                    }
                    //shoppingCart.TotalPrice = totalMiles.ToString();
                    var balance = memberInfoResponse.Data.Balances?.FirstOrDefault(s => s.Currency == "RDM");
                    int accAvailableMiles = balance == null ? 0 : Convert.ToInt32(balance.Amount);
                    int remainingMiles = accAvailableMiles - totalMiles;

                    MOBMilesFOP milesFOPobj = new MOBMilesFOP
                    {
                        RemainingMiles = remainingMiles,
                        AvailableMiles = accAvailableMiles,
                        RequiredMiles = totalMiles,
                        CustomerId = memberInfoResponse.Data.CustomerId,
                        ProfileOwnerMPAccountNumber = request.MileagePlusNumber,
                        DisplayAvailableMiles = UtilityHelper.FormatAwardAmountForDisplay(accAvailableMiles.ToString(), false),
                        DisplayTotalRequiredMiles = UtilityHelper.FormatAwardAmountForDisplay(totalMiles.ToString(), false),
                        DisplayRequiredMiles = '-' + UtilityHelper.FormatAwardAmountForDisplay(totalMiles.ToString(), false),
                        IsMilesFOPEligible = isMilesFOPEligible
                    };

                    if (shoppingCart.FormofPaymentDetails == null)
                    {
                        shoppingCart.FormofPaymentDetails = new MOBFormofPaymentDetails();
                    }

                    if (remainingMiles > -1)
                    {
                        milesFOPobj.DisplayRemainingMiles = UtilityHelper.FormatAwardAmountForDisplay(remainingMiles.ToString(), false);
                        milesFOPobj.HasEnoughMiles = true;
                        shoppingCart.DisplayMessage = null;

                    }

                    else
                    {
                        milesFOPobj.HasEnoughMiles = false;
                        if (shoppingCart.DisplayMessage.IsNullOrEmpty())
                        {
                            shoppingCart.DisplayMessage = new List<MOBSection>();
                        }
                        shoppingCart.DisplayMessage.Add(_shoppingCartUtility.AddTextForInsufficientMiles());
                    }
                    shoppingCart.TermsAndConditions.Add(_shoppingCartUtility.AddMilesTnCForSeatChange());

                    shoppingCart.FormofPaymentDetails.FormOfPaymentType = MOBFormofPayment.MilesFOP.ToString();
                    shoppingCart.FormofPaymentDetails.MilesFOP = milesFOPobj;
                }
                else
                {
                    throw new Exception(_configuration.GetValue<string>("MilesFOPNotLoggedInMessage"));
                }
            }
            else
            {
                if (shoppingCart.FormofPaymentDetails != null)
                    shoppingCart.FormofPaymentDetails.FormOfPaymentType = shoppingCart.FormofPaymentDetails.FormOfPaymentType == MOBFormofPayment.MilesFOP.ToString()
                                                                                   || shoppingCart.FormofPaymentDetails.FormOfPaymentType.IsNullOrEmpty() ? MOBFormofPayment.CreditCard.ToString() :
                                                                                   shoppingCart.FormofPaymentDetails.FormOfPaymentType;
                if (shoppingCart.FormofPaymentDetails.MilesFOP == null)
                    shoppingCart.FormofPaymentDetails.MilesFOP = new MOBMilesFOP();

                shoppingCart.FormofPaymentDetails.MilesFOP.IsMilesFOPEligible = isMilesFOPEligible;
            }
            shoppingCart.RequestObjectJSON = JsonConvert.SerializeObject(request, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            shoppingCart.RequestObjectName = MOBRequestObjectName.RegisterCheckInSeats;
        }

        private RegisterSeatsRequest BuildRegisterSeatRequest(string cartId, SeatChangeState state, string flow, bool isMFOPEnabled, bool isMiles = false, bool isEnableCCEFeedBackCallForEplusTile = false, bool isCOGPartialSeatAssignmentFixEnabled = false)
        {
            RegisterSeatsRequest registerSeatsRequest = new RegisterSeatsRequest();

            var reservationDetail = new United.Service.Presentation.ReservationResponseModel.ReservationDetail();
            reservationDetail = _sessionHelperService.GetSession<United.Service.Presentation.ReservationResponseModel.ReservationDetail>(state.SessionId, new Service.Presentation.ReservationResponseModel.ReservationDetail().GetType().FullName, new List<string> { state.SessionId, new Service.Presentation.ReservationResponseModel.ReservationDetail().GetType().FullName }).Result;

            registerSeatsRequest.CartId = cartId;
            registerSeatsRequest.Characteristics = new Collection<Characteristic>() {
                    new Characteristic() { Code = "ManageRes", Value = "true" }};

            // Adding as part of stand alone offers migration to CCE and shopping cart to make feed back call.
            if (isEnableCCEFeedBackCallForEplusTile && flow == FlowType.VIEWRES.ToString() && registerSeatsRequest.Characteristics?.Count > 0)
            {
                var guid = _sessionHelperService.GetSession<Collection<UniqueIdentifier>>(state.SessionId, ObjectNames.CCEDOResponseGuidSession, new List<string> { state.SessionId, ObjectNames.CCEDOResponseGuidSession }).Result;

                if (guid != null && guid.Count > 0)
                {
                    registerSeatsRequest.Characteristics.Add(new Characteristic() { Code = "E01_TILE_SEAT", Value = "True" });
                    foreach (var uniqueIdentifier in guid)
                    {
                        if (!string.IsNullOrEmpty(uniqueIdentifier.ID) && !string.IsNullOrEmpty(uniqueIdentifier.Name))
                            registerSeatsRequest.Characteristics.Add(new Characteristic() { Code = uniqueIdentifier.Name.ToUpper(), Value = uniqueIdentifier.ID });
                    }
                }
            }
            registerSeatsRequest.Reservation = reservationDetail.Detail;
            if (registerSeatsRequest.Reservation.FlightSegments != null && registerSeatsRequest.Reservation.FlightSegments.Count > 0)
            {
                registerSeatsRequest.Reservation.FlightSegments.ForEach(s => s.FlightSegment.OperatingAirlineCode =
                _configuration.GetValue<string>("SeatMapForACSubsidiary").Contains(s.FlightSegment.OperatingAirlineCode) ? "AC" : s.FlightSegment.OperatingAirlineCode);
            }
            registerSeatsRequest.IsPNRFirst = false;
            registerSeatsRequest.RecordLocator = reservationDetail.Detail.ConfirmationID;
            registerSeatsRequest.OfferRequest = BuildRegisterPcuSeatOffer(state, cartId);

            List<SeatAssignment> seatAssignments = new List<SeatAssignment>();
            registerSeatsRequest.SeatAssignments = state.BookingTravelerInfo.SelectMany(x => x.Seats).Where(x => (!string.IsNullOrEmpty(x.SeatAssignment) && x.SeatAssignment != x.OldSeatAssignment)
            || (isCOGPartialSeatAssignmentFixEnabled && IsCOGSeatAssignment(x, state))).Select(x => new SeatAssignment
            {
                Adjustments = x.IsCouponApplied ? null : (!x.Adjustments.IsNullOrEmpty() ? x.Adjustments.ToCollection() : null),
                Currency = isMFOPEnabled && isMiles ? "ML1" : x.Currency,
                FlattenedSeatIndex = Convert.ToInt32(state.Segments.Where(y => y.Arrival.Code == x.Destination && y.Departure.Code == x.Origin && y.FlightNumber == x.FlightNumber).Select(y => y.SegmentIndex).FirstOrDefault()) - 1,
                OptOut = false,
                OriginalPrice = isMFOPEnabled && (!string.IsNullOrEmpty(x.OldSeatCurrency) && string.Equals(x.OldSeatCurrency, "ML1", StringComparison.OrdinalIgnoreCase)) ?
                                x.OldSeatMiles : x.OldSeatPrice,
                ProductCode = _configuration.GetValue<bool>("DisableFixForIncorrectEdocSeats_MRM_1759") ? x.OldSeatProgramCode : null,
                PCUSeat = !string.IsNullOrEmpty(x.PcuOfferOptionId),
                Seat = isCOGPartialSeatAssignmentFixEnabled && IsCOGSeatAssignment(x, state) && string.IsNullOrEmpty(x.SeatAssignment) ? x.OldSeatAssignment : x.SeatAssignment,
                SeatPrice = isMFOPEnabled && (isMiles || string.Equals(x.Currency, "ML1", StringComparison.OrdinalIgnoreCase)) ? x.MilesAfterTravelerCompanionRules : x.IsCouponApplied ? x.PriceAfterTravelerCompanionRules : (!x.Adjustments.IsNullOrEmpty() ? x.PriceAfterCouponApplied : x.PriceAfterTravelerCompanionRules),
                SeatPromotionCode = x.ProgramCode,
                SeatType = x.SeatType,
                TravelerIndex = Convert.ToInt32(x.TravelerSharesIndex.Split('.')[0]) - 1,
                OriginalSegmentIndex = Convert.ToInt32(state.Segments.Where(y => y.Arrival.Code == x.Destination && y.Departure.Code == x.Origin && y.FlightNumber == x.FlightNumber).Select(y => y.OriginalSegmentNumber).FirstOrDefault()),
                LegIndex = Convert.ToInt32(state.Segments.Where(y => y.Arrival.Code == x.Destination && y.Departure.Code == x.Origin && y.FlightNumber == x.FlightNumber).Select(y => y.LegIndex).FirstOrDefault()),
                PersonIndex = x.TravelerSharesIndex.ToString(),
                ArrivalAirportCode = x.Destination.ToString(),
                DepartureAirportCode = x.Origin.ToString(),
                FlightNumber = x.FlightNumber.ToString(),
                IsBundleSeat = IsBundleFlow(flow),
                MilesAmount = isMFOPEnabled ? x.MilesAfterTravelerCompanionRules : 0,
                MoneyAmount = x.IsCouponApplied ? x.PriceAfterTravelerCompanionRules : (!x.Adjustments.IsNullOrEmpty() ? x.PriceAfterCouponApplied : x.PriceAfterTravelerCompanionRules)
            }).ToList();

            if (_configuration.GetValue<bool>("EnableCSLCloudMigrationToggle"))
            {
                registerSeatsRequest.WorkFlowType = ConfigUtility.GetWorkFlowType(flow);
            }

            return registerSeatsRequest;
        }

        private bool IsCOGSeatAssignment(MOBSeat x, SeatChangeState state)
        {
            return state.Segments.Any(y => y.Arrival.Code == x.Destination && y.Departure.Code == x.Origin && y.FlightNumber == x.FlightNumber && y.COGStop);
        }

        public bool IsBundleFlow(string flow)
        {
            bool isBundleFlow = false;
            if (_configuration.GetValue<bool>("EnableTravelOptionsInViewRes"))
            {
                if (flow?.ToUpper() == FlowType.VIEWRES_BUNDLES_SEATMAP.ToString())
                {
                    isBundleFlow = true;
                }
            }
            return isBundleFlow;
        }
        public RegisterOfferRequest GetRegisterOffersRequest(string cartId, string cartKey, string languageCode, string pointOfSale, string productCode, string productId, List<string> productIds, string subProductCode, bool delete, ProductOffer Offer, ReservationDetail reservation)
        {
            RegisterOfferRequest registerOfferRequest = new RegisterOfferRequest();

            registerOfferRequest.Offer = (Offer == null ? null : Offer);
            registerOfferRequest.AutoTicket = false;
            registerOfferRequest.CartId = cartId;
            registerOfferRequest.Characteristics = new System.Collections.ObjectModel.Collection<United.Service.Presentation.CommonModel.Characteristic>() {
                    new Characteristic() { Code = "ManageRes", Value = "true" }};
            registerOfferRequest.CartKey = cartKey;
            registerOfferRequest.CountryCode = pointOfSale;
            registerOfferRequest.Delete = delete;
            registerOfferRequest.LangCode = languageCode;
            registerOfferRequest.ProductCode = productCode;
            registerOfferRequest.ProductId = productId;
            registerOfferRequest.ProductIds = productIds;
            registerOfferRequest.SubProductCode = subProductCode;
            registerOfferRequest.Reservation = (reservation == null ? null : reservation.Detail);

            return registerOfferRequest;
        }

        private RegisterOfferRequest BuildRegisterPcuSeatOffer(SeatChangeState seatState, string cartId)
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
            pcuState = _sessionHelperService.GetSession<PcuState>(seatState.SessionId, pcuState.ObjectName, new List<string> { seatState.SessionId, pcuState.ObjectName }).Result;


            var productIds = pcuState.EligibleSegments.SelectMany(e => e?.UpgradeOptions.Where(s => optionIds.Contains(s.OptionId))).SelectMany(u => u.ProductIds).ToList();

            ProductOffer cceDODOfferResponse = null;
            bool isEnableTravelOptionsInViewRes = _configuration.GetValue<bool>("EnableTravelOptionsInViewRes");
            if (isEnableTravelOptionsInViewRes)
            {
                var DODOfferResponse = new GetOffersCce();
                DODOfferResponse = _sessionHelperService.GetSession<GetOffersCce>(seatState.SessionId, ObjectNames.CCEDynamicOfferDetailResponse, new List<string> { seatState.SessionId, ObjectNames.CCEDynamicOfferDetailResponse }).Result;

                cceDODOfferResponse = new ProductOffer();
                cceDODOfferResponse = string.IsNullOrEmpty(DODOfferResponse?.OfferResponseJson)
                                             ? null
                                             : JsonConvert.DeserializeObject<United.Service.Presentation.ProductResponseModel.ProductOffer>(DODOfferResponse.OfferResponseJson);
            }
            var productOffer = new GetOffers();
            if (cceDODOfferResponse == null || cceDODOfferResponse?.Offers == null)
            {
                productOffer = _sessionHelperService.GetSession<GetOffers>(seatState.SessionId, productOffer.ObjectName, new List<string> { seatState.SessionId, productOffer.ObjectName }).Result;
                productOffer.Offers[0].ProductInformation.ProductDetails.RemoveWhere(p => p.Product.Code != "PCU");
            }

            return GetRegisterOffersRequest(cartId, null, null, null, "PCU", null, productIds, null, false, isEnableTravelOptionsInViewRes && cceDODOfferResponse != null && cceDODOfferResponse?.Offers?.Count > 0 ? cceDODOfferResponse : productOffer, null);
        }

        private async Task<MOBShoppingCart> RegisterSeats(MOBRequest request, string cartId, string token, SeatChangeState state, string flow = "", bool isMiles = false, bool isMFOPEnabled = false)
        {
            var isCOGPartialSeatAssignmentFixEnabled = await _featureSettings.GetFeatureSettingValue("EnableCOGPartialSeatAssignmentFix").ConfigureAwait(false);

            bool IsEnableCCEFeedBackCallForEplusTile = await _shoppingCartUtility.IsEnableCCEFeedBackCallForEplusTile();
            RegisterSeatsRequest registerSeatsRequest = BuildRegisterSeatRequest(cartId, state, flow, isMFOPEnabled, isMiles, IsEnableCCEFeedBackCallForEplusTile, isCOGPartialSeatAssignmentFixEnabled);

            if (registerSeatsRequest.SeatAssignments == null || registerSeatsRequest.SeatAssignments.Count == 0)
                return null;

            string jsonRequest = DataContextJsonSerializer.Serialize<RegisterSeatsRequest>(registerSeatsRequest);
            string actionName = "/registerseats";
            var jsonResponse = _shoppingCartService.GetRegisterSeats<FlightReservationResponse>(token, actionName, state.SessionId, jsonRequest).Result;

            if (jsonResponse != null)
            {
                if (jsonResponse != null && jsonResponse.Errors.Count() == 0)
                {
                    MOBShoppingCart persistShoppingCart = new MOBShoppingCart();
                    persistShoppingCart = _sessionHelperService.GetSession<MOBShoppingCart>(state.SessionId, persistShoppingCart.ObjectName, new List<string> { state.SessionId, persistShoppingCart.ObjectName }).Result;

                    var reservationDetail = new United.Service.Presentation.ReservationResponseModel.ReservationDetail();
                    reservationDetail = _sessionHelperService.GetSession<United.Service.Presentation.ReservationResponseModel.ReservationDetail>(state.SessionId, new Service.Presentation.ReservationResponseModel.ReservationDetail().GetType().FullName, new List<string> { state.SessionId, new Service.Presentation.ReservationResponseModel.ReservationDetail().GetType().FullName }).Result;

                    if (persistShoppingCart == null)
                        persistShoppingCart = new MOBShoppingCart();
                    persistShoppingCart.Flow = GetFlow(persistShoppingCart, flow);
                    string productFlow = flow == FlowType.VIEWRES_BUNDLES_SEATMAP.ToString() ? FlowType.VIEWRES_BUNDLES_SEATMAP.ToString() : FlowType.VIEWRES.ToString();
                    persistShoppingCart.Products = await _shoppingCartUtility.ConfirmationPageProductInfo(jsonResponse, false, false, productFlow, request.Application, state, false, false, null, null, false, flow == FlowType.VIEWRES_BUNDLES_SEATMAP.ToString() ? state.SessionId : string.Empty, isMiles);
                    persistShoppingCart.CartId = cartId;
                    var grandtotal = IsEnableTravelOptionsInViewRes(request.Application.Id, request?.Application?.Version?.Major)
                         ? persistShoppingCart?.Products?.Sum(p => string.IsNullOrEmpty(p.ProdTotalPrice) ? 0 : Convert.ToDecimal(p.ProdTotalPrice)) ?? 0
                         : jsonResponse.DisplayCart.DisplaySeats.Select(x => x.SeatPrice).ToList().Sum();

                    persistShoppingCart.TotalPrice = String.Format("{0:0.00}", grandtotal);
                    persistShoppingCart.DisplayTotalPrice = Decimal.Parse(grandtotal.ToString()).ToString("c");

                    persistShoppingCart.TotalMiles = IsEnableTravelOptionsInViewRes(request.Application.Id, request?.Application?.Version?.Major) ?
                        persistShoppingCart?.Products?.Sum(p => p.ProdTotalMiles).ToString()
                         : jsonResponse.DisplayCart.DisplaySeats.Select(x => x.MilesAmount).ToList().Sum().ToString();

                    if (ConfigUtility.IsManageResETCEnabled(request.Application.Id, request.Application.Version.Major))
                    {
                        persistShoppingCart.Prices = AddGrandTotalIfNotExist(persistShoppingCart.Prices, Convert.ToDouble(persistShoppingCart.TotalPrice), persistShoppingCart.Flow);
                    }
                    persistShoppingCart.TermsAndConditions = await _shoppingCartUtility.GetProductBasedTermAndConditions(null, jsonResponse, false, state.SessionId, flow);
                    persistShoppingCart.PaymentTarget = GetPaymentTargetForRegisterFop(jsonResponse);
                    persistShoppingCart.DisplayMessage = reservationDetail != null && reservationDetail.Detail != null ? await GetPaymentMessagesForWLPNRViewRes(jsonResponse, reservationDetail.Detail.FlightSegments, flow) : null;
                    persistShoppingCart.TripInfoForUplift = await _mscFormsOfPayment.GetUpliftTripInfo(reservationDetail.Detail, persistShoppingCart.TotalPrice, persistShoppingCart.Products);

                    if (persistShoppingCart.Captions.IsNullOrEmpty())
                    {
                        persistShoppingCart.Captions = await _etcUtility.GetCaptions("PaymentPage_ViewRes_Captions");
                        // This code is added as part of bundle implementation in MR path to let client know the product custometer selected is bundle
                        MOBItem isBundleProduct = AddBundleCaptionForQMEvent(jsonResponse, persistShoppingCart.Products);

                        if (isBundleProduct != null && !string.IsNullOrEmpty(isBundleProduct.CurrentValue))
                        {
                            if (persistShoppingCart.Captions != null && persistShoppingCart.Captions.Count > 0)
                                persistShoppingCart.Captions.Add(isBundleProduct);
                            else
                            {
                                persistShoppingCart.Captions = new List<MOBItem>();
                                persistShoppingCart.Captions.Add(isBundleProduct);
                            }
                        }
                    }
                    if (!persistShoppingCart.Products.IsNullOrEmpty())
                    {
                        persistShoppingCart.IsCouponEligibleProduct = !_configuration.GetValue<bool>("IsEnableManageResCoupon") && IsCouponEligibleProduct(persistShoppingCart.Products);
                        var androidAndroidIsEnableManageResMerchCouponAppVersion = _configuration.GetValue<string>("Android_IsEnableManageResMerchCoupon_AppVersion");
                        var iphoneIsEnableManageResMerchCouponAppVersion = _configuration.GetValue<string>("iPhone_IsEnableManageResMerchCoupon_AppVersion");
                        persistShoppingCart.PromoCodeDetails = _configuration.GetValue<bool>("IsEnableManageResCoupon") && GeneralHelper.IsApplicationVersionGreaterorEqual(request.Application.Id, request.Application.Version.Major, androidAndroidIsEnableManageResMerchCouponAppVersion, iphoneIsEnableManageResMerchCouponAppVersion) && flow != FlowType.VIEWRES_BUNDLES_SEATMAP.ToString() ? new MOBPromoCodeDetails() : null;
                    }

                    if (_configuration.GetValue<bool>("EnableMilesAsPayment"))
                    {
                        if (persistShoppingCart.Products.SelectMany(p => p.Segments).ToList().SelectMany(s => s.SubSegmentDetails).Where(m => m.Miles == 0).ToList().Count > 0)
                        {
                            foreach (MOBProductSubSegmentDetail sSegment in persistShoppingCart.Products.SelectMany(p => p.Segments).ToList().SelectMany(s => s.SubSegmentDetails))
                            {
                                sSegment.Miles = 0;
                                sSegment.DisplayMiles = string.Empty;
                            }
                            persistShoppingCart.TotalMiles = "0";
                            persistShoppingCart.DisplayTotalMiles = string.Empty;
                        }
                        else
                        {
                            persistShoppingCart.TotalMiles = (_configuration.GetValue<int>("milesFOP") * persistShoppingCart.Products.SelectMany(p => p.Segments).ToList().SelectMany(s => s.SubSegmentDetails).Count()).ToString();
                            persistShoppingCart.DisplayTotalMiles = UtilityHelper.FormatAwardAmountForDisplay(persistShoppingCart.TotalMiles, false);
                        }
                    }
                    //Billing International address
                    if (_checkoutUtiliy.IsInternationalBillingAddress_CheckinFlowEnabled(request.Application)
                        && persistShoppingCart.FormofPaymentDetails?.InternationalBilling?.BillingAddressProperties == null && flow?.ToLower() == FlowType.CHECKIN.ToString().ToLower())
                    {
                        persistShoppingCart.FormofPaymentDetails = await GetBillingAddressProperties(persistShoppingCart.FormofPaymentDetails);
                    }


                    await _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, state.SessionId, new List<string> { state.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName);
                    return persistShoppingCart;
                }
            }
            return null;
        }
        private async Task<MOBFormofPaymentDetails> GetBillingAddressProperties(MOBFormofPaymentDetails formofPaymentDetails)
        {
            if (formofPaymentDetails == null)
            {
                formofPaymentDetails = new MOBFormofPaymentDetails();
            }
            formofPaymentDetails.InternationalBilling = new MOBInternationalBilling();
            var billingCountries = await _mscFormsOfPayment.GetCachedBillingAddressCountries();

            if (billingCountries == null || !billingCountries.Any())
            {
                billingCountries = new List<MOBCPBillingCountry>();

                billingCountries.Add(new MOBCPBillingCountry
                {
                    CountryCode = "US",
                    CountryName = "United States",
                    Id = "1",
                    IsStateRequired = true,
                    IsZipRequired = true
                });
            }
            formofPaymentDetails.InternationalBilling.BillingAddressProperties = (billingCountries == null || !billingCountries.Any()) ? null : billingCountries;
            return formofPaymentDetails;
        }
        private bool IsCouponEligibleProduct(List<MOBProdDetail> proddetail)
        {
            string couponProductCode = _configuration.GetValue<string>("IsCouponEligibleProduct") ?? string.Empty;

            if (!couponProductCode.IsNullOrEmpty())
            {
                var couponProductCodeList = couponProductCode.Split('|');
                foreach (var c in proddetail)
                {

                    if (couponProductCodeList.Any(t => t.Equals(c.Code)))
                    {
                        return true;
                    }

                }
            }

            return false;
        }
        public bool IsEnableTravelOptionsInViewRes(int applicationId, string appVersion, List<Mobile.Model.Common.MOBItem> catalogItems)
        {
            return _configuration.GetValue<bool>("EnableTravelOptionsInViewRes") && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("EnableTravelOptionsInViewRes_AppVersion_Android"), _configuration.GetValue<string>("EnableTravelOptionsInViewRes_AppVersion_Iphone")) && catalogItems != null &&
                              catalogItems.FirstOrDefault(a => a.Id == ((int)IOSCatalogEnum.EnableBundlesInManageRes).ToString() || a.Id == ((int)AndroidCatalogEnum.EnableBundlesInManageRes).ToString())?.CurrentValue == "1";
        }
        public bool IsEnableTravelOptionsInViewRes(int applicationId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableTravelOptionsInViewRes") && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("EnableTravelOptionsInViewRes_AppVersion_Android"), _configuration.GetValue<string>("EnableTravelOptionsInViewRes_AppVersion_Iphone"));
        }

        private void AddSeats(ref SeatChangeState state, string origin, string destination, string flightNumber, string flightDate, string paxIndex, string seatAssignment)
        {
            string[] paxIndexArray = paxIndex.Split(',');
            string[] seatAssignmentArray = seatAssignment.Split(',');

            if (state != null && state.Seats != null)
            {
                for (int i = 0; i < seatAssignmentArray.Length; i++)
                {
                    IEnumerable<MOBSeat> seatList = from s in state.Seats
                                                    where s.Origin == origin.Trim().ToUpper()
                                                    && s.Destination == destination.Trim().ToUpper()
                                                    && s.TravelerSharesIndex == paxIndexArray[i]
                                                    && Convert.ToDateTime(s.DepartureDate).ToString("MMddyyy") == Convert.ToDateTime(flightDate).ToString("MMddyyy")
                                                    && s.FlightNumber == flightNumber
                                                    select s;

                    if (seatList != null && seatList.Count() > 0)
                    {
                        List<MOBSeat> seats = new List<MOBSeat>();
                        seats = seatList.ToList();
                        if (seats.Count() == 1)
                        {
                            MOBSeat aSeat = new MOBSeat();
                            aSeat.Destination = destination;
                            aSeat.Origin = origin;
                            aSeat.FlightNumber = flightNumber;
                            aSeat.DepartureDate = flightDate;
                            aSeat.TravelerSharesIndex = seats[0].TravelerSharesIndex;
                            aSeat.Key = seats[0].Key;
                            aSeat.OldSeatAssignment = seats[0].OldSeatAssignment;
                            aSeat.OldSeatCurrency = seats[0].OldSeatCurrency;
                            aSeat.OldSeatPrice = seats[0].OldSeatPrice;
                            aSeat.OldSeatType = seats[0].OldSeatType;
                            aSeat.OldSeatProgramCode = seats[0].OldSeatProgramCode;

                            aSeat.OldSeatMiles = seats[0].OldSeatMiles;

                            string[] assignments = seatAssignmentArray[i].Split('|');

                            if (assignments != null && assignments.Length == 5)
                            {
                                aSeat.SeatAssignment = assignments[0];
                                aSeat.Price = Convert.ToDecimal(assignments[1]);
                                aSeat.PriceAfterTravelerCompanionRules = aSeat.Price;
                                aSeat.Currency = assignments[2];
                                aSeat.ProgramCode = assignments[3];
                                aSeat.SeatType = assignments[4];
                                aSeat.LimitedRecline = (aSeat.ProgramCode == "PSL");
                            }
                            else
                            {
                                aSeat.SeatAssignment = seatAssignmentArray[i];
                            }

                            if (_configuration.GetValue<bool>("FixArgumentOutOfRangeExceptionInRegisterSeatsAction"))
                            {
                                state.Seats[state.Seats.IndexOf(seats[0])] = aSeat;
                            }
                            else
                            {
                                state.Seats[seats[0].Key] = aSeat;
                            }
                        }
                    }
                    else
                    {
                        MOBSeat aSeat = new MOBSeat();
                        aSeat.Destination = destination;
                        aSeat.Origin = origin;
                        aSeat.FlightNumber = flightNumber;
                        aSeat.DepartureDate = flightDate;

                        string[] assignments = seatAssignmentArray[i].Split('|');

                        if (assignments != null && assignments.Length == 5)
                        {
                            aSeat.SeatAssignment = assignments[0];
                            aSeat.Price = Convert.ToDecimal(assignments[1]);
                            aSeat.PriceAfterTravelerCompanionRules = aSeat.Price;
                            aSeat.Currency = assignments[2];
                            aSeat.ProgramCode = assignments[3];
                            aSeat.SeatType = assignments[4];
                            aSeat.LimitedRecline = (aSeat.ProgramCode == "PSL");
                        }
                        else
                        {
                            aSeat.SeatAssignment = seatAssignmentArray[i];
                        }

                        aSeat.TravelerSharesIndex = paxIndexArray[i];
                        aSeat.Key = state.Seats.Count;
                        state.Seats.Add(aSeat);
                    }
                }
            }
            else
            {
                for (int i = 0; i < seatAssignment.Split(',').Length; i++)
                {
                    MOBSeat aSeat = new MOBSeat();
                    aSeat.Destination = destination;
                    aSeat.Origin = origin;
                    aSeat.FlightNumber = flightNumber;
                    aSeat.DepartureDate = flightDate;

                    string[] assignments = seatAssignmentArray[i].Split('|');

                    if (assignments != null && assignments.Length == 5)
                    {
                        aSeat.SeatAssignment = assignments[0];
                        aSeat.Price = Convert.ToDecimal(assignments[1]);
                        aSeat.PriceAfterTravelerCompanionRules = aSeat.Price;
                        aSeat.Currency = assignments[2];
                        aSeat.ProgramCode = assignments[3];
                        aSeat.SeatType = assignments[4];
                        aSeat.LimitedRecline = (aSeat.ProgramCode == "PSL");
                    }
                    else
                    {
                        aSeat.SeatAssignment = seatAssignmentArray[i];
                    }

                    aSeat.TravelerSharesIndex = paxIndexArray[i];
                    aSeat.Key = state.Seats.Count;
                    state.Seats.Add(aSeat);
                }
            }

            if (state != null && state.BookingTravelerInfo != null && state.Seats != null)
            {
                foreach (MOBBKTraveler traveler in state.BookingTravelerInfo)
                {
                    IEnumerable<MOBSeat> seatList = from s in state.Seats
                                                    where s.TravelerSharesIndex == traveler.SHARESPosition
                                                    select s;

                    if (seatList != null && seatList.Count() > 0)
                    {
                        List<MOBSeat> travelerSeats = new List<MOBSeat>();
                        travelerSeats = seatList.ToList();
                        travelerSeats = travelerSeats.OrderBy(s => s.Key).ToList();
                        traveler.Seats = travelerSeats;
                    }
                }
            }
        }

        private void AddSeats(ref SeatChangeState state, MOBRegisterSeatsRequest request, bool isMFOPEnabled)
        {
            List<MOBSeatMap> MOBSeatMapList = _sessionHelperService.GetSession<List<MOBSeatMap>>(request.SessionId, ObjectNames.MOBSeatMapListFullName, new List<string> { request.SessionId, ObjectNames.MOBSeatMapListFullName }).Result;
            List<MOBSeat> unavailableSeats = new List<MOBSeat>();

            try
            {
                if (MOBSeatMapList != null && state != null && state.Seats != null)
                {
                    foreach (MOBSeatDetail selectedSeat in request.SelectedSeats)
                    {
                        if (selectedSeat.Origin == null || selectedSeat.Destination == null ||
                            selectedSeat.PaxIndex == null || selectedSeat.FlightDate == null || selectedSeat.FlightNumber == null)
                            break;

                        MOBSeat seat = state.Seats.Where(s =>
                                                        s.Origin == selectedSeat.Origin.Trim().ToUpper()
                                                        && s.Destination == selectedSeat.Destination.Trim().ToUpper()
                                                        && s.TravelerSharesIndex == selectedSeat.PaxIndex
                                                        && Convert.ToDateTime(s.DepartureDate).ToString("MMddyyy") == Convert.ToDateTime(selectedSeat.FlightDate).ToString("MMddyyy")
                                                        && s.FlightNumber == selectedSeat.FlightNumber).FirstOrDefault();

                        MOBSeatMap mobSeatMap = MOBSeatMapList.Where(s =>
                                                                s.Departure?.Code == selectedSeat.Origin.Trim().ToUpper()
                                                                && s.Arrival?.Code == selectedSeat.Destination.Trim().ToUpper()
                                                                && Convert.ToDateTime(s.FlightDateTime).ToString("MMddyyy") == Convert.ToDateTime(selectedSeat.FlightDate).ToString("MMddyyy")
                                                                && s.FlightNumber.ToString() == selectedSeat.FlightNumber).FirstOrDefault();

                        MOBSeatB mobSeatB = (from cabin in mobSeatMap?.Cabins
                                             from row in cabin.Rows
                                             from se in row.Seats
                                             where se.Number.ToUpper().Trim() == selectedSeat.SeatNumber.ToUpper().Trim()
                                             select se).FirstOrDefault();

                        if (seat != null && mobSeatMap != null && mobSeatB != null)
                        {
                            MOBSeat aSeat = new MOBSeat();

                            aSeat.Destination = selectedSeat.Destination;
                            aSeat.Origin = selectedSeat.Origin;
                            aSeat.FlightNumber = selectedSeat.FlightNumber;
                            aSeat.DepartureDate = selectedSeat.FlightDate;
                            aSeat.SeatAssignment = selectedSeat.SeatNumber;
                            aSeat.Currency = "USD";

                            aSeat.TravelerSharesIndex = seat.TravelerSharesIndex;
                            aSeat.Key = seat.Key;
                            aSeat.OldSeatAssignment = seat.OldSeatAssignment;
                            aSeat.OldSeatCurrency = seat.OldSeatCurrency;
                            aSeat.OldSeatPrice = seat.OldSeatPrice;
                            aSeat.OldSeatType = seat.OldSeatType;
                            aSeat.OldSeatProgramCode = seat.OldSeatProgramCode;
                            aSeat.OldSeatMiles = seat.OldSeatMiles;

                            _logger.LogInformation("RegisterSeats_CFOP {SeatMPCache2ValidatPriceManuplate} {SessionId} {ApplicationId} {ApplicationVersion} and {Transactionid}", MOBSeatMapList, request.SessionId, request.Application.Id, request.Application.Version.DisplayText, request.TransactionId);

                            if (!string.IsNullOrEmpty(aSeat.SeatAssignment))
                            {
                                if (mobSeatB.ServicesAndFees != null && mobSeatB.ServicesAndFees.Count > 0)
                                {
                                    aSeat.Price = Convert.ToDecimal(mobSeatB.ServicesAndFees[0].TotalFee);
                                    aSeat.PcuOfferOptionId = mobSeatB.PcuOfferOptionId;

                                    aSeat.PriceAfterTravelerCompanionRules = aSeat.Price;
                                    aSeat.ProgramCode = mobSeatB.ServicesAndFees[0].Program;
                                    aSeat.SeatType = mobSeatB.ServicesAndFees[0].SeatFeature;
                                    aSeat.LimitedRecline = (aSeat.ProgramCode == "PSL");

                                    if (isMFOPEnabled)
                                    {
                                        aSeat.Miles = mobSeatB.ServicesAndFees[0].MilesFee;
                                        aSeat.DisplayMiles = mobSeatB.ServicesAndFees[0].DisplayMilesFee;
                                        aSeat.MilesAfterTravelerCompanionRules = mobSeatB.ServicesAndFees[0].MilesFee;
                                    }
                                }

                                if (mobSeatB.seatvalue == "X")
                                {
                                    unavailableSeats.Add(aSeat);
                                }
                            }

                            if (_configuration.GetValue<bool>("FixArgumentOutOfRangeExceptionInRegisterSeatsAction"))
                            {
                                state.Seats[state.Seats.IndexOf(seat)] = aSeat;
                            }
                            else
                            {
                                state.Seats[seat.Key] = aSeat;
                            }
                        }
                        else
                        {
                            throw new MOBUnitedException(string.Format("Selected seat {0} not found on {1} to {2} seatmap", selectedSeat.SeatNumber, selectedSeat.Origin, selectedSeat.Destination));
                        }
                    }
                }

                if (state != null && state.Seats != null)
                {
                    foreach (MOBBKTraveler traveler in state.BookingTravelerInfo)
                    {
                        IEnumerable<MOBSeat> seatList = from s in state.Seats
                                                        where s.TravelerSharesIndex == traveler.SHARESPosition
                                                        select s;

                        if (seatList != null && seatList.Count() > 0)
                        {
                            List<MOBSeat> travelerSeats = new List<MOBSeat>();
                            travelerSeats = seatList.ToList();
                            travelerSeats = travelerSeats.OrderBy(s => s.Key).ToList();
                            traveler.Seats = travelerSeats;
                        }
                    }
                }

                if (unavailableSeats != null && unavailableSeats.Count > 0)
                {
                    foreach (var seat in unavailableSeats)
                    {
                        state.Seats.Remove(seat);
                    }
                }
            }
            catch (Exception ex)
            {
                if (_configuration.GetValue<string>("SeatNotFoundAtCompleteSeatsSelection") != null)
                {
                    string exMessage = ex.Message != null ? ex.Message : "";
                    throw new Exception(_configuration.GetValue<string>("SeatNotFoundAtCompleteSeatsSelection") + " - " + exMessage);
                }
            }
        }

        private List<MOBSeatPrice> ComputeSeatPrices(List<MOBSeat> seats, bool isMilesFOPEnabled)
        {
            List<MOBSeatPrice> seatPrices = null;

            bool allSeatsFree = true;
            List<MOBSeatPrice> tempSeatPrices = new List<MOBSeatPrice>();

            if (seats != null)
            {
                foreach (MOBSeat seat in seats)
                {
                    if (!IsExistingSeat(seat.OldSeatAssignment, seat.SeatAssignment) && seat.Price > 0 && seat.Price > seat.OldSeatPrice)
                    {
                        if (isMilesFOPEnabled && string.Equals(seat.OldSeatCurrency, "ML1", StringComparison.OrdinalIgnoreCase)
                            && string.IsNullOrEmpty(seat.PcuOfferOptionId) && seat.Miles <= seat.OldSeatMiles)
                        {
                            continue;
                        }

                        if (seat.PriceAfterTravelerCompanionRules > 0)
                        {
                            allSeatsFree = false;
                        }

                        MOBSeatPrice seatPrice = _checkoutUtiliy.BuildSeatPrice(seat);
                        if (!seatPrice.IsNullOrEmpty())
                            tempSeatPrices.Add(seatPrice);
                    }
                }
            }

            if (!allSeatsFree && tempSeatPrices.Count > 0)
            {
                seatPrices = new List<MOBSeatPrice>();
                string origin = "";
                string destination = "";

                foreach (MOBSeatPrice seatPrice in tempSeatPrices)
                {
                    if (seatPrice.Origin.Equals(origin) && seatPrice.Destination.Equals(destination))
                    {
                        var item = seatPrices.Find(s => s.Origin == seatPrice.Origin
                                                     && s.Destination == seatPrice.Destination
                                                     && s.SeatMessage == seatPrice.SeatMessage);

                        if (item != null)
                        {
                            var ci = TopHelper.GetCultureInfo(seatPrice.CurrencyCode ?? "USD");
                            item.NumberOftravelers = item.NumberOftravelers + 1;
                            item.TotalPrice = item.TotalPrice + seatPrice.TotalPrice;
                            item.TotalPriceDisplayValue = TopHelper.FormatAmountForDisplay(item.TotalPrice, ci, false);
                            item.DiscountedTotalPrice = item.DiscountedTotalPrice + seatPrice.DiscountedTotalPrice;
                            item.DiscountedTotalPriceDisplayValue = TopHelper.FormatAmountForDisplay(item.DiscountedTotalPrice, ci, false);
                            if (item.SeatNumbers == null)
                                item.SeatNumbers = new List<string>();
                            if (!seatPrice.SeatNumbers.IsNullOrEmpty())
                                item.SeatNumbers.AddRange(seatPrice.SeatNumbers);
                        }
                        else
                        {
                            MOBSeatPrice sp = BuildSeatPricesWithDiscountedPrice(seatPrice);
                            if (!sp.IsNullOrEmpty())
                            {
                                seatPrices.Add(sp);
                            }
                        }
                    }
                    else
                    {
                        origin = seatPrice.Origin;
                        destination = seatPrice.Destination;

                        MOBSeatPrice sp = BuildSeatPricesWithDiscountedPrice(seatPrice);
                        if (!sp.IsNullOrEmpty())
                        {
                            seatPrices.Add(sp);
                        }
                    }
                }
                if (seatPrices != null)
                {

                    seatPrices = OrderSeatPriceItemsBySeatType(seatPrices);

                    seatPrices.FindAll(s => s.SeatMessage != null && s.NumberOftravelers > 1)
                                            .ForEach(q =>
                                            {
                                                q.SeatMessage = q.SeatMessage.Replace("Economy Plus Seat", "Economy Plus Seats");
                                                q.SeatMessage = q.SeatMessage.Replace("Advance Seat Assignment", "Advance Seat Assignments");
                                                q.SeatMessage = q.SeatMessage.Replace("Preferred seat", "Preferred seats");
                                            });

                }
            }

            return seatPrices;
        }
        public string GetPaymentTargetForRegisterFop(United.Services.FlightShopping.Common.FlightReservation.FlightReservationResponse flightReservationResponse, bool isCompleteFarelockPurchase = false)
        {
            if (string.IsNullOrEmpty(_configuration.GetValue<string>("EnablePCUSelectedSeatPurchaseViewRes")))
            {
                return string.Empty;
            }

            if (isCompleteFarelockPurchase)
            {
                return "RES";
            }

            if (flightReservationResponse == null || flightReservationResponse.ShoppingCart == null || flightReservationResponse.ShoppingCart.Items == null)
            {
                return string.Empty;
            }

            var productCodes = flightReservationResponse.ShoppingCart.Items.Where(x => x.Product.FirstOrDefault().Code != "RES").Select(x => x.Product.FirstOrDefault().Code).ToList();
            if (productCodes == null || !productCodes.Any())
            {
                return string.Empty;
            }

            return string.Join(",", productCodes.Distinct());
        }

        private List<MOBSeatPrice> OrderSeatPriceItemsBySeatType(List<MOBSeatPrice> seatPrices)
        {
            if (seatPrices == null || !seatPrices.Any())
                return seatPrices;
            return seatPrices.GroupBy(g => g.Origin + " - " + g.Destination)
                             .SelectMany(odSeatPriceGrp => GetOrderredSeatPricesForOriginDestination(odSeatPriceGrp)).ToList();
        }
        private IEnumerable<MOBSeatPrice> GetOrderredSeatPricesForOriginDestination(IGrouping<string, MOBSeatPrice> groupOfSeatPrices)
        {
            if (groupOfSeatPrices == null || !groupOfSeatPrices.Any())
                return groupOfSeatPrices;
            var orderOfSeatPriceItems = UtilityHelper.GetSeatPriceOrder();
            var orderredSeatPrices = groupOfSeatPrices.OrderBy(g => orderOfSeatPriceItems[g.SeatMessage]);
            orderredSeatPrices.FirstOrDefault().SeatPricesHeaderandTotal = new MOBItem { Id = groupOfSeatPrices.Key };
            return orderredSeatPrices;
        }

        private MOBSeatPrice BuildSeatPricesWithDiscountedPrice(MOBSeatPrice seatPrice)
        {
            if (seatPrice.IsNullOrEmpty())
                return null;

            CultureInfo ci = TopHelper.GetCultureInfo(seatPrice.CurrencyCode);
            return new MOBSeatPrice
            {
                Origin = seatPrice.Origin,
                Destination = seatPrice.Destination,
                SeatMessage = seatPrice.SeatMessage,
                NumberOftravelers = 1,
                TotalPrice = seatPrice.TotalPrice,
                TotalPriceDisplayValue = TopHelper.FormatAmountForDisplay(seatPrice.TotalPrice, ci, false),
                DiscountedTotalPrice = seatPrice.DiscountedTotalPrice,
                DiscountedTotalPriceDisplayValue = TopHelper.FormatAmountForDisplay(seatPrice.DiscountedTotalPrice, ci, false),
                CurrencyCode = seatPrice.CurrencyCode,
                SeatNumbers = seatPrice.SeatNumbers
            };
        }
        private bool IsExistingSeat(string oldSeatAssignment, string currentSeatAssignment)
        {
            if (string.IsNullOrEmpty(oldSeatAssignment) || string.IsNullOrEmpty(currentSeatAssignment))
                return false;

            return oldSeatAssignment.Equals(currentSeatAssignment, StringComparison.OrdinalIgnoreCase);
        }
        private bool ShowSeatChangeButton(MOBPNR pnr)
        {
            if (pnr.Passengers != null && pnr.Passengers.Count > 9 || pnr.IsGroup == true)
            {
                return false;
            }

            return pnr.IsEligibleToSeatChange && _configuration.GetValue<bool>("ShowSeatChange");
        }
        public bool IsEnableXmlToCslSeatMapMigration(int appId, string appVersion)
        {
            if (!string.IsNullOrEmpty(appVersion) && appId != -1)
            {
                return _configuration.GetValue<bool>("SwithToCSLSeatMapChangeSeats")
                    && GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidXmlToCslSMapVersion", "iPhoneXmlToCslSMapVersion", "", "", true, _configuration);
            }
            return false;
        }
        private async Task<List<MOBSeat>> ApplyTravelerCompanionRulesCSl30(string sessionId, List<MOBSeat> seats, string recordLocator, string pnrCreationDate, List<MOBBKTraveler> travelers, string languageCode, int applicationId, string appVersion, List<MOBTripSegment> tripSegments, string deviceId, bool isMilesFOPEnabled)
        {
            var persistedCSL30SeatMaps = new List<MOBSeatMapCSL30>();
            persistedCSL30SeatMaps = await _sessionHelperService.GetSession<List<MOBSeatMapCSL30>>(sessionId, new MOBSeatMapCSL30().ObjectName, new List<string> { sessionId, new MOBSeatMapCSL30().ObjectName }).ConfigureAwait(false);

            if (persistedCSL30SeatMaps == null)
            {
                if (!(seats?.Any(s => !string.IsNullOrEmpty(s?.SeatAssignment) && s?.SeatAssignment != s?.OldSeatAssignment) ?? false))
                {
                    return seats;
                }

                throw new System.Exception(_configuration.GetValue<string>("SeatNotFoundAtCompleteSeatsSelection"));
            }

            if (!persistedCSL30SeatMaps.IsNullOrEmpty() && seats != null && seats.Count > 0 && travelers != null && travelers.Count > 0)
            {
                foreach (MOBSeatMapCSL30 pCSL30SeatMap in persistedCSL30SeatMaps)
                {
                    foreach (MOBSeat seat in seats)
                    {
                        if (!seat.IsNullOrEmpty() && seat.FlightNumber == pCSL30SeatMap.FlightNumber.ToString() && seat.Origin == pCSL30SeatMap.DepartureCode && seat.Destination == pCSL30SeatMap.ArrivalCode)
                        {
                            var seatDetail = pCSL30SeatMap.Seat.FirstOrDefault(x => !x.IsNullOrEmpty() && x.Number == seat.SeatAssignment && x.IsAvailable);
                            if (!seatDetail.IsNullOrEmpty() && string.IsNullOrEmpty(seat.PcuOfferOptionId))
                            {
                                seat.PriceAfterTravelerCompanionRules = seat.Price > 0 && seat.Price > seat.OldSeatPrice ? seatDetail.Pricing.FirstOrDefault(x => !x.IsNullOrEmpty() && x.TravelerIndex == seat.TravelerSharesIndex).TotalPrice : 0;

                                if (isMilesFOPEnabled)
                                {
                                    if (seat.PriceAfterTravelerCompanionRules == 0)
                                    {
                                        seat.MilesAfterTravelerCompanionRules = 0;
                                    }
                                    else
                                    {
                                        decimal miles = seat.Miles > 0 && seat.Miles > seat.OldSeatMiles ? seatDetail.Pricing.FirstOrDefault(x => !x.IsNullOrEmpty() && string.Equals(x.CurrencyCode, "MILES", StringComparison.OrdinalIgnoreCase) && x.TravelerIndex == seat.TravelerSharesIndex).TotalPrice : 0;
                                        seat.MilesAfterTravelerCompanionRules = Convert.ToInt32(miles);
                                    }

                                    if (string.Equals(seat.OldSeatCurrency, "ML1", StringComparison.OrdinalIgnoreCase)
                                                             && seat.OldSeatMiles > 0 && seat.MilesAfterTravelerCompanionRules == 0)
                                    {
                                        seat.Currency = "ML1";
                                        seat.PriceAfterTravelerCompanionRules = 0;
                                    }
                                }

                                seat.SeatFeature = seatDetail.DisplaySeatCategory;
                            }
                            seat.SeatType = !seatDetail.IsNullOrEmpty() && !seatDetail.DisplaySeatCategory.IsNullOrEmpty() ? seatDetail.DisplaySeatCategory : seat.SeatType;
                        }
                    }
                }
            }

            _logger.LogInformation("ApplyTravelerCompanionRulesCSL30 {Response} {SessionId} {ApplicationId} {ApplicationVersion} {DeviceId}", seats, sessionId, applicationId, appVersion, deviceId);

            return seats;
        }
        private List<MOBSeat> ApplyTravelerCompanionRulesCSL(string sessionId, List<MOBSeat> seats, string recordLocator, string pnrCreationDate, List<MOBBKTraveler> travelers, string languageCode, int applicationId, string appVersion, List<MOBTripSegment> tripSegments, string deviceId)
        {
            #region request
            United.Service.Presentation.FlightRequestModel.SeatMap request = new Service.Presentation.FlightRequestModel.SeatMap();

            if (travelers != null && seats != null && travelers.Count > 0 && seats.Count > 0)
            {
                request.Requestor = new United.Service.Presentation.CommonModel.Requestor();
                string channelId = _configuration.GetValue<string>("AssignChannelID");
                request.Requestor.ChannelID = channelId.IsNullOrEmpty() ? "MOBILE" : channelId;
                request.LanguageCode = languageCode;
                request.ConfirmationID = recordLocator;
                request.ReservationCreateDate = pnrCreationDate;
                request.Rules = BuildSeatMapRequestWithSegmentsCSL(tripSegments);

                request.Travelers = new Collection<ProductTraveler>();
                foreach (var traveler in travelers)
                {
                    ProductTraveler travelerInformation = new ProductTraveler();
                    travelerInformation.GivenName = traveler.Person.GivenName;
                    travelerInformation.Surname = traveler.Person.Surname;
                    travelerInformation.TravelerNameIndex = traveler.SHARESPosition;

                    List<MOBSeat> paxSeats = new List<MOBSeat>();
                    foreach (MOBSeat seat in seats)
                    {
                        if (seat.TravelerSharesIndex == traveler.SHARESPosition)
                        {
                            paxSeats.Add(seat);
                        }
                    }

                    travelerInformation.Seats = new Collection<SeatDetail>();
                    foreach (var paxSeat in paxSeats)
                    {
                        SeatDetail Seat = new SeatDetail();
                        Seat.DepartureAirport = new Service.Presentation.CommonModel.AirportModel.Airport();
                        Seat.DepartureAirport.IATACode = paxSeat.Origin;
                        Seat.ArrivalAirport = new Service.Presentation.CommonModel.AirportModel.Airport();
                        Seat.ArrivalAirport.IATACode = paxSeat.Destination;
                        Seat.FlightNumber = paxSeat.FlightNumber;
                        Seat.Seat = new United.Service.Presentation.CommonModel.AircraftModel.Seat();

                        if (paxSeat.SeatAssignment != null && paxSeat.SeatAssignment.Trim() != string.Empty)
                        {
                            Seat.Seat.Identifier = paxSeat.SeatAssignment;

                        }
                        else if (paxSeat.OldSeatAssignment != null && paxSeat.OldSeatAssignment.Trim() != string.Empty)
                        {
                            Seat.Seat.Identifier = paxSeat.OldSeatAssignment;
                        }
                        else
                        {
                            Seat.Seat = null;
                        }
                        Seat.DepartureDate = Convert.ToDateTime(paxSeat.DepartureDate).ToShortDateString();
                        travelerInformation.Seats.Add(Seat);
                    }

                    request.Travelers.Add(travelerInformation);
                }


                string jsonRequest = JsonConvert.SerializeObject(request);

                #endregion request
                Session session = new Session();
                session = _sessionHelperService.GetSession<Session>(sessionId, session.ObjectName, new List<string> { sessionId, session.ObjectName }).Result;


                var url = EnableSSA(applicationId, appVersion) ? string.Format("{0}GetFinalTravelerSeatPricesWithAllFares", _configuration.GetValue<string>("CSLService-ViewReservationChangeseats"))
                                                                       : string.Format("{0}GetFinalTravelerSeatPrices", _configuration.GetValue<string>("CSLService-ViewReservationChangeseats"));


                _logger.LogInformation("ApplyTravelerCompanionRulesCSL {SessionId} {ApplicationId} {ApplicationVersion} {DeviceId} and {Url}", sessionId, applicationId, appVersion, deviceId, url);
                _logger.LogInformation("ApplyTravelerCompanionRulesCSL {SessionId} {ApplicationId} {ApplicationVersion} {DeviceId} and {CSL Request}", sessionId, applicationId, appVersion, deviceId, jsonRequest);
                _logger.LogInformation("ApplyTravelerCompanionRulesCSL {SessionId} {ApplicationId} {ApplicationVersion} {DeviceId} and {CSL Token}", sessionId, applicationId, appVersion, deviceId, session.Token);

                var jsonResponse = _seatEnginePostService.SeatEnginePostNew(sessionId, url, "application/xml;", session.Token, jsonRequest).Result;

                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    _logger.LogInformation("ApplyTravelerCompanionRulesCSL {SessionId} {ApplicationId} {ApplicationVersion} {DeviceId} and {CSL Response}", sessionId, applicationId, appVersion, deviceId, jsonResponse);

                    var response = JsonConvert.DeserializeObject<United.Service.Presentation.ProductModel.FlightSeatMapDetail>(jsonResponse);

                    if (response != null && response.Travelers != null && response.Travelers.Any() && seats != null && seats.Any())
                    {
                        foreach (var traveler in response.Travelers)
                        {
                            if (traveler.Seats != null && traveler.Seats.Count > 0)
                            {
                                foreach (var seatInfo in traveler.Seats)
                                {
                                    foreach (MOBSeat seat in seats)
                                    {
                                        if (seat.SeatAssignment != null && seat.SeatAssignment.Equals(seatInfo.Seat.Identifier) && seat.Origin.Equals(seatInfo.DepartureAirport.IATACode) && seat.Destination.Equals(seatInfo.ArrivalAirport.IATACode))
                                        {
                                            if (seatInfo.Seat != null && seatInfo.Seat.Price != null && seatInfo.Seat.Price.Totals != null && seatInfo.Seat.Price.Totals.Count > 0)
                                            {
                                                seat.PriceAfterTravelerCompanionRules = Convert.ToDecimal(seatInfo.Seat.Price.Totals[0].Amount);
                                                seat.SeatFeature = seatInfo.Seat.Price.PromotionCode;
                                            }
                                            seat.SeatType = seatInfo.Seat.SeatType.ToString();
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return seats;
        }
        public bool EnableSSA(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableSSA") && GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidSSAVersion", "iPhoneSSAVersion", "", "", true, _configuration);
        }
        private Collection<SeatRuleParameter> BuildSeatMapRequestWithSegmentsCSL(List<MOBTripSegment> segments)
        {
            if (segments == null)
                return null;

            var rules = new Collection<SeatRuleParameter>();
            foreach (var segment in segments)
            {
                var seatRule = new SeatRuleParameter();
                seatRule.Flight = new FlightProfile();
                seatRule.Flight.FlightNumber = segment.FlightNumber;
                seatRule.Flight.OperatingCarrierCode = segment.OperatingCarrier;
                seatRule.ProductCode = segment.ProductCode;
                seatRule.FareBasisCode = segment.FareBasisCode;
                seatRule.Segment = segment.Departure.Code + segment.Arrival.Code;
                seatRule.Flight.DepartureAirport = segment.Departure.Code;
                seatRule.Flight.ArrivalAirport = segment.Arrival.Code;
                rules.Add(seatRule);
            }

            return rules;
        }
        private bool ShouldCreateNewCart(MOBRegisterSeatsRequest request)
        {
            if (!string.IsNullOrEmpty(request.CartId) && request.Flow == FlowType.VIEWRES_BUNDLES_SEATMAP.ToString())
                return false;

            return string.IsNullOrEmpty(request.CartId);
        }
        #endregion

        private MOBRegisterOfferResponse CreateMOBRegisterOfferResponse(MOBShoppingRequest request)
        {
            return new MOBRegisterOfferResponse()
            {
                Flow = request.Flow,
                SessionId = request.SessionId,
                CheckinSessionId = request.CheckinSessionId,
                TransactionId = request.TransactionId
            };
        }

    }
}
