using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Common.Helper.MSCPayment.Interfaces;
using United.Definition;
using United.Definition.FormofPayment;
using United.Definition.Shopping;
using United.Definition.Shopping.TripInsurance;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.MSCPayment.Interfaces;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Mobile.Model.Common;
using United.Persist.Definition.Merchandizing;
using United.Persist.Definition.Shopping;
using United.Service.Presentation.CommonModel;
using United.Service.Presentation.PersonalizationModel;
using United.Service.Presentation.PersonalizationRequestModel;
using United.Service.Presentation.PersonalizationResponseModel;
using United.Service.Presentation.ProductModel;
using United.Service.Presentation.ReservationResponseModel;
using United.Services.FlightShopping.Common.Cart;
using United.Services.FlightShopping.Common.DisplayCart;
using United.Services.FlightShopping.Common.Extensions;
using United.Services.FlightShopping.Common.FlightReservation;

using United.Utility.Helper;

using Characteristic = United.Service.Presentation.CommonModel.Characteristic;
using SDLContentResponse = United.Definition.SDL.SDLContentResponse;
using FlightReservationResponse = United.Services.FlightShopping.Common.FlightReservation.FlightReservationResponse;
using ProfileResponse = United.Persist.Definition.Shopping.ProfileResponse;
using Task = System.Threading.Tasks.Task;
using System.Text.RegularExpressions;
using System.Web;

namespace United.Mobile.PromoCode.Domain
{
    public class PromoCodeBusiness : IPromoCodeBusiness
    {
        private readonly ICacheLog<PromoCodeBusiness> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHeaders _headers;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly IMSCShoppingSessionHelper _mSCShoppingSessionHelper;
        private readonly IShoppingUtility _shoppingUtility;
        private readonly IMSCFormsOfPayment _mSCFormsOfPayment;
        private readonly IDPService _dPService;
        private readonly IMSCPkDispenserPublicKey _mSCPkDispenserPublicKey;
        private readonly IFlightShoppingProductsService _flightShoppingProductsServices;
        private readonly IETCUtility _eTCUtility;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IShoppingCartUtility _shoppingCartUtility;
        private readonly IGetTermsandCondtionsService _getTermsandCondtionsService;
        private readonly IGetCCEContentService _getCCEContentService;
        private readonly ICMSContentService _cMSContentService;
        private readonly ILegalDocumentsForTitlesService _legalDocumentsForTitlesService;
        private string milesMoneyErrorMessage = string.Empty;
        private readonly IMSCPageProductInfoHelper _mSCPageProductInfoHelper;
        private readonly ICachingService _cachingService;
        private readonly IFeatureSettings _featureSettings;
        private readonly IFeatureToggles _featureToggles;


        public PromoCodeBusiness(ICacheLog<PromoCodeBusiness> logger
            , IConfiguration configuration
            , IHeaders headers
            , ISessionHelperService sessionHelperService
            , IMSCShoppingSessionHelper mSCShoppingSessionHelper
            , IShoppingUtility shoppingUtility
            , IMSCFormsOfPayment mSCFormsOfPayment
            , IDPService dPService
            , IFlightShoppingProductsService flightShoppingProductsServices
            , IETCUtility eTCUtility
            , IShoppingCartService shoppingCartService
            , IShoppingCartUtility shoppingCartUtility
            , IGetTermsandCondtionsService getTermsandCondtionsService
            , IGetCCEContentService getCCEContentService
            , ICMSContentService cMSContentService
            , ILegalDocumentsForTitlesService legalDocumentsForTitlesService
            , IMSCPkDispenserPublicKey mSCPkDispenserPublicKey
            , IMSCPageProductInfoHelper mSCPageProductInfoHelper
            , ICachingService cachingService
            , IFeatureSettings featureSettings
            , IFeatureToggles featureToggles)


        {
            _logger = logger;
            _configuration = configuration;
            _headers = headers;
            _sessionHelperService = sessionHelperService;
            _mSCShoppingSessionHelper = mSCShoppingSessionHelper;
            _shoppingUtility = shoppingUtility;
            _mSCFormsOfPayment = mSCFormsOfPayment;
            _dPService = dPService;
            _mSCPkDispenserPublicKey = mSCPkDispenserPublicKey;
            _flightShoppingProductsServices = flightShoppingProductsServices;
            _eTCUtility = eTCUtility;
            _shoppingCartService = shoppingCartService;
            _shoppingCartUtility = shoppingCartUtility;
            _getTermsandCondtionsService = getTermsandCondtionsService;
            _getCCEContentService = getCCEContentService;
            _cMSContentService = cMSContentService;
            _legalDocumentsForTitlesService = legalDocumentsForTitlesService;
            _mSCPageProductInfoHelper = mSCPageProductInfoHelper;
            _cachingService = cachingService;
            _featureSettings = featureSettings;
            _featureToggles = featureToggles;
        }

        public async Task<MOBApplyPromoCodeResponse> ApplyPromoCode(MOBApplyPromoCodeRequest request)
        {
            MOBApplyPromoCodeResponse response = new MOBApplyPromoCodeResponse();
            MOBShoppingCart shoppingCart = new MOBShoppingCart();

            _logger.LogInformation("ApplyPromoCode Request{request} sessionId{sessionId}", JsonConvert.SerializeObject(request), request.SessionId);

            Session session = request.Flow == FlowType.BOOKING.ToString() ? await _mSCShoppingSessionHelper.GetBookingFlowSession(request.SessionId) : await _mSCShoppingSessionHelper.GetValidateSession(request.SessionId, false, true);

            // If UI is sending this ContinueToResetMoneyMiles true then reset moneymiles
            if (_shoppingCartUtility.IncludeMoneyPlusMiles(request.Application.Id, request.Application.Version.Major) && request.ContinueToResetMoneyMiles)
            {
                ApplyMoneyPlusMilesOptionRequest milesRequest = new ApplyMoneyPlusMilesOptionRequest();
                milesRequest.OptionId = null;  // for undo moneymiles send optionid as null
                milesRequest.Application = request.Application;
                milesRequest.CartId = request.CartId;
                milesRequest.AccessCode = request.AccessCode;
                milesRequest.CartKey = request.CartKey;
                milesRequest.DeviceId = request.DeviceId;
                milesRequest.SessionId = request.SessionId;
                milesRequest.TransactionId = request.TransactionId;
                milesRequest.Flow = request.Flow;
                milesRequest.PointOfSale = request.PointOfSale;
                var milesResponse = await ApplyMilesPlusMoneyOption(session, milesRequest);
            }

            if (request?.PromoCodes != null)
            {
                request.PromoCodes.ForEach(p => { p.PromoCode = p.PromoCode.Trim(); });
            }

            if (IsPostBookingPromoCodeEnabled(request.Application.Id, request.Application.Version.Major) && IsPromoCodeEligibleFlow(request.Flow))
            {
                var tupleApplyPromoCodeResponse = await ApplyPromoCodeForStandaloneProduct(request, session);
                response = tupleApplyPromoCodeResponse.ApplyPromoCodeResponse;
                MOBItem inEligibleReason = tupleApplyPromoCodeResponse.inEligibleReason;
                AddPromoCodeIneligibleReason(inEligibleReason, response, request);
                response.Flow = request.Flow;
                response.LanguageCode = request.LanguageCode;
                response.SessionId = request.SessionId;
                response.TransactionId = request.TransactionId;
            }
            else
            {
                var tupleApplyPromoCodeResponse = await ApplyPromoCode(request, session);
                response = tupleApplyPromoCodeResponse.ApplyPromoCodeResponse;
                MOBItem inEligibleReason = tupleApplyPromoCodeResponse.inEligibleReason;
                shoppingCart = response.ShoppingCart;

                if (await _featureSettings.GetFeatureSettingValue("EnablePromocodeDisplay"))
                {
                    await _sessionHelperService.SaveSession<MOBApplyPromoCodeResponse>(response, session.SessionId, new List<string> { session.SessionId, response.ObjectName }, response.ObjectName);
                }


                shoppingCart = await _shoppingCartUtility.ReservationToShoppingCart_DataMigration(response.Reservation, shoppingCart, request,session: session);
                AddPromoCodeIneligibleReason(inEligibleReason, response, request);
                response.ShoppingCart = shoppingCart;
                await _sessionHelperService.SaveSession<MOBApplyPromoCodeResponse>(response, session.SessionId, new List<string> { session.SessionId, response.ObjectName }, response.ObjectName);
            }
            bool isDefault = false;
           
            var tupleRespEligibleFormofPayments = await _mSCFormsOfPayment.GetEligibleFormofPayments(request, session,
                 response.ShoppingCart, request.CartId, request.Flow, response.Reservation);
            response.EligibleFormofPayments = tupleRespEligibleFormofPayments.EligibleFormofPayments;
            isDefault = tupleRespEligibleFormofPayments.isDefault;
            if (response.Reservation != null)
            {
                await response.Reservation.Initialise(_configuration, _cachingService);
            }
            return response;
        }

        public async Task<MOBPromoCodeTermsandConditionsResponse> GetTermsandConditionsByPromoCode(MOBApplyPromoCodeRequest request)
        {
            MOBPromoCodeTermsandConditionsResponse response = new MOBPromoCodeTermsandConditionsResponse();

            string token = string.Empty;

            if (_configuration.GetValue<bool>("IsEnableCCEPromoTermsConditions"))
            {
                token = await _dPService.GetAnonymousToken(request.Application.Id, request.DeviceId, _configuration);
            }
            else
            {
                Session session = await _mSCShoppingSessionHelper.GetBookingFlowSession(request.SessionId);
                if (session == null)
                {
                    throw new MOBUnitedException("Could not find your booking session.");
                }
                token = session?.Token;
            }

            response = await GetTermsandCondtionsByPromoCode(request, token);

            return await Task.FromResult(response);

        }

        public async Task<MOBApplyPromoCodeResponse> RemovePromoCode(MOBApplyPromoCodeRequest request)
        {
            MOBApplyPromoCodeResponse response = new MOBApplyPromoCodeResponse();
            MOBShoppingCart shoppingCart = new MOBShoppingCart();

            _logger.LogInformation("RemovePromoCode Request{request} sessionId{sessionId}", JsonConvert.SerializeObject(request), request.SessionId);

            Session session = await _mSCShoppingSessionHelper.GetBookingFlowSession(request.SessionId);

            if (session == null)
            {
                throw new MOBUnitedException("Could not find your booking session.");
            }
            await AddOrRemovePromo(request, session, true, request.Flow);

            Reservation reservation = new Reservation();
            reservation = await _sessionHelperService.GetSession<Reservation>(request.SessionId, reservation.ObjectName, new List<string> { request.SessionId, reservation.ObjectName }).ConfigureAwait(false);

            if (reservation != null && _shoppingCartUtility.IsEnableOmniCartMVP2Changes(request.Application.Id, request.Application.Version.Major, reservation.ShopReservationInfo2?.IsDisplayCart == true))
            {
                MOBShoppingCart persistShoppingCart = new MOBShoppingCart();
                persistShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(request.SessionId, persistShoppingCart.ObjectName, new List<string> { request.SessionId, persistShoppingCart.ObjectName }).ConfigureAwait(false);
                response.ShoppingCart = persistShoppingCart;


                var shopReservation = MakeReservationFromPersistReservation(reservation);
                _eTCUtility.SetEligibilityforETCTravelCredit(shopReservation, session, reservation);
                _shoppingCartUtility.BuildOmniCart(response.ShoppingCart, shopReservation, request.Application);
                response.Reservation = shopReservation;
                await response.Reservation.Initialise(_configuration, _cachingService);
                await response.Reservation.UpdateRewards(_configuration, _cachingService);
            }

            return await Task.FromResult(response);

        }

        private MOBSHOPReservation MakeReservationFromPersistReservation(Reservation reservation)
        {
            return new MOBSHOPReservation(_configuration, _cachingService)
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
                lmxtravelers = reservation.LMXTravelers,
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
                ReshopTrips = reservation.ReshopTrips,
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

        private async Task<MOBFOPResponse> ApplyMilesPlusMoneyOption(Session session, ApplyMoneyPlusMilesOptionRequest request)
        {
            MOBFOPResponse milesPlusMoneyCreditResponse = new MOBFOPResponse();

            //MOBILE - 15202 Error Message
            List<CMSContentMessage> lstMessages = await GetSDLContentByGroupName(request, session.SessionId, session.Token, _configuration.GetValue<string>("CMSContentMessages_GroupName_BookingRTI_Messages"), "BookingPathRTI_CMSContentMessagesCached_StaticGUID");
            milesMoneyErrorMessage = GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.ReviewMMCMessage.ErrorMsg").FirstOrDefault()?.ContentFull;
            var response = await ApplyCSLMilesPlusMoneyOptions(session, request, request.OptionId);

            var shoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(session.SessionId, new MOBShoppingCart().ObjectName, new List<string>() { session.SessionId, new MOBShoppingCart().ObjectName }).ConfigureAwait(false);

            await AssignUpdatedPricesToReservation(session.SessionId, response, request.OptionId, shoppingCart.Products);
            await LoadSessionValuesToResponse(session, milesPlusMoneyCreditResponse, null, request.OptionId, shoppingCart, lstMessages);
            if (_shoppingCartUtility.IsEnableMoneyPlusMilesFeature(request.Application.Id, request.Application.Version.Major, session?.CatalogItems)
                 && string.IsNullOrEmpty(request.OptionId ) )
            {
                session.IsMoneyPlusMilesSelected = false;
                await _sessionHelperService.SaveSession<Session>(session, session.SessionId, new List<string> { session.SessionId, session.ObjectName }, session.ObjectName).ConfigureAwait(false);
            }
            if (!_configuration.GetValue<bool>("OmniCartTotalPriceBugFixForMoneyPlusMiles"))
            {
                _shoppingCartUtility.BuildOmniCart(milesPlusMoneyCreditResponse?.ShoppingCart, milesPlusMoneyCreditResponse?.Reservation, request.Application);
            }
            #region OfferCode expansion Changes(This properties are added as part of offercode expansion changes but can be later used in general )
            milesPlusMoneyCreditResponse.ShoppingCart.FormofPaymentDetails.IsFOPRequired = _shoppingCartUtility.IsFOPRequired(milesPlusMoneyCreditResponse.ShoppingCart, milesPlusMoneyCreditResponse.Reservation);
            milesPlusMoneyCreditResponse.ShoppingCart.FormofPaymentDetails.IsEnableAgreeandPurchaseButton = _shoppingCartUtility.IsEnableAgreeandPurchaseButton(milesPlusMoneyCreditResponse.ShoppingCart, milesPlusMoneyCreditResponse.Reservation);
            milesPlusMoneyCreditResponse.ShoppingCart.FormofPaymentDetails.MaskedPaymentMethod =await _shoppingCartUtility.AssignMaskedPaymentMethod(milesPlusMoneyCreditResponse.ShoppingCart,request.Application).ConfigureAwait(false);
            #endregion OfferCode expansion Changes
            milesPlusMoneyCreditResponse.PkDispenserPublicKey = await _mSCPkDispenserPublicKey.GetCachedOrNewpkDispenserPublicKey(request.Application.Id, request.Application.Version.Major, request.DeviceId, session.SessionId, session.Token, session?.CatalogItems).ConfigureAwait(false);

            _logger.LogInformation("ApplyMilesPlusMoneyOption MilesPlusMoneyCreditResponse{milesPlusMoneyCreditResponse} SessionId:{sessionId}", JsonConvert.SerializeObject(milesPlusMoneyCreditResponse), request.SessionId);

            return milesPlusMoneyCreditResponse;
        }

        private async Task<List<CMSContentMessage>> GetSDLContentByGroupName(MOBRequest request, string guid, string token, string groupName, string docNameConfigEntry, bool useCache = false)
        {
            MOBCSLContentMessagesResponse response = null;

            try
            {
                var getSDL = await _cachingService.GetCache<string>(_configuration.GetValue<string>(docNameConfigEntry) + ObjectNames.MOBCSLContentMessagesResponseFullName, request.TransactionId).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(getSDL))
                {
                    response = JsonConvert.DeserializeObject<MOBCSLContentMessagesResponse>(getSDL);
                }

                if (response != null && response.Messages != null) { return response.Messages; }
            }

            catch { }

            MOBCSLContentMessagesRequest sdlReqeust = new MOBCSLContentMessagesRequest
            {
                Lang = "en",
                Pos = "us",
                Channel = "mobileapp",
                Listname = new List<string>(),
                LocationCodes = new List<string>(),
                Groupname = groupName,
                Usecache = useCache
            };

            string jsonRequest = JsonConvert.SerializeObject(sdlReqeust);

            response =await _cMSContentService.GetSDLContentByGroupName<MOBCSLContentMessagesResponse>(token, "message", jsonRequest, guid);

            if (response != null)
            {
                _logger.LogInformation("GetSDLContentByGroupName CSL-CallError{CSL-CallError} SessionId:{sessionId}", "CSL response is empty or null", guid);
            }

            if (response == null)
            {
                _logger.LogInformation("GetSDLContentByGroupName Failed to deserialize CSL response SessionId:{sessionId}", guid);

                return null;
            }

            if (response.Errors?.Count > 0)
            {
                string errorMsg = String.Join(" ", response.Errors.Select(x => x.Message));

                _logger.LogError("GetSDLContentByGroupName CSL-CallError{CSL-CallError} SessionId:{sessionId}", errorMsg, guid);

                return null;
            }

            if (response != null && (Convert.ToBoolean(response.Status) && response.Messages != null))
            {
                if (!_configuration.GetValue<bool>("DisableSDLEmptyTitleFix"))
                {
                    response.Messages = response.Messages.Where(l => l.Title != null)?.ToList();
                }
                await _cachingService.SaveCache<MOBCSLContentMessagesResponse>(_configuration.GetValue<string>(docNameConfigEntry) + ObjectNames.MOBCSLContentMessagesResponseFullName, response, request.TransactionId, new TimeSpan(1, 30, 0)).ConfigureAwait(false);

            }

            _logger.LogInformation("GetSDLContentByGroupName responseMessage {responseMessage} sessionID{sessionId}", response.Messages, guid);

            return response.Messages;
        }

        private async Task LoadSessionValuesToResponse(Session session, MOBFOPResponse response, MOBFOPMoneyPlusMilesCredit moneyPlusMilesCredit = null, string optionId = "", MOBShoppingCart shoppingCart = null, List<CMSContentMessage> lstMessages = null)
        {
            //MOBILE-39218
            if (await _featureSettings.GetFeatureSettingValue("EnableFixedTheAuthorizedAmount").ConfigureAwait(false))
                response.ShoppingCart = shoppingCart ?? await _sessionHelperService.GetSession<MOBShoppingCart>(session.SessionId, new MOBShoppingCart().ObjectName, new List<string>() { session.SessionId, new MOBShoppingCart().ObjectName }).ConfigureAwait(false);
            else
                response.ShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(session.SessionId, new MOBShoppingCart().ObjectName, new List<string>() { session.SessionId, new MOBShoppingCart().ObjectName }).ConfigureAwait(false);

            if (response.ShoppingCart == null)
            {
                response.ShoppingCart = new MOBShoppingCart();
            }
            if (response.ShoppingCart?.FormofPaymentDetails == null)
            {
                response.ShoppingCart.FormofPaymentDetails = new MOBFormofPaymentDetails();
            }

            if (moneyPlusMilesCredit != null)
            {
                response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit = moneyPlusMilesCredit;
            }
            else
            {
                if (!string.IsNullOrEmpty(optionId))
                {
                    response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit.SelectedMoneyPlusMiles = response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit.MilesPlusMoneyOptions.FirstOrDefault(mm => mm.OptionId == optionId);
                    var changeInTravelerMessage = string.Empty;
                    changeInTravelerMessage = GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.RemoveCoupons")[0]?.ContentFull;
                    response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit.PromoCodeMoneyMilesAlertMessage = string.IsNullOrWhiteSpace(changeInTravelerMessage) ? null : new MOBSection
                    {
                        Text1 = changeInTravelerMessage,
                        Text2 = "Cancel",
                        Text3 = "Continue"
                    };
                }
                else
                {
                    if (_shoppingCartUtility.IsEnableMoneyPlusMilesFeature(catalogItems: session?.CatalogItems))
                    {
                        if(response.ShoppingCart?.FormofPaymentDetails?.MoneyPlusMilesCredit != null)
                            response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit.SelectedMoneyPlusMiles = null;
                        if (response.ShoppingCart?.FormofPaymentDetails?.MoneyPlusMilesCredit?.PromoCodeMoneyMilesAlertMessage != null)
                            response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit.PromoCodeMoneyMilesAlertMessage = null;
                    }
                    else
                    {
                        response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit = null;
                    }
                }
            }

            await _sessionHelperService.SaveSession(response.ShoppingCart, session.SessionId, new List<string> { session.SessionId, response.ShoppingCart.ObjectName }, response.ShoppingCart.ObjectName).ConfigureAwait(false);

            response.SessionId = session.SessionId;
            response.Flow = string.IsNullOrEmpty(session.Flow) ? response.ShoppingCart.Flow : session.Flow;
            response.Reservation = new MOBSHOPReservation(_configuration, _cachingService);
            response.Reservation = await GetReservationFromPersist(response.Reservation, session);


            ProfileResponse persistedProfileResponse = new ProfileResponse();

            persistedProfileResponse = await _sessionHelperService.GetSession<ProfileResponse>(session.SessionId, persistedProfileResponse.ObjectName, new List<string> { session.SessionId, persistedProfileResponse.ObjectName }).ConfigureAwait(false);

            response.Profiles = persistedProfileResponse != null ? persistedProfileResponse.Response.Profiles : null;
        }

        private async Task<MOBSHOPReservation> GetReservationFromPersist(MOBSHOPReservation reservation, Session session)
        {
            #region
            Reservation bookingPathReservation = new Reservation();
            bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(session.SessionId, bookingPathReservation.ObjectName, new List<string> { session.SessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);
            await reservation.Initialise(_configuration, _cachingService);

            return _eTCUtility.MakeReservationFromPersistReservation(reservation, bookingPathReservation, session);

            #endregion
        }

        private async Task<United.Services.FlightShopping.Common.FlightReservation.FlightReservationResponse> ApplyCSLMilesPlusMoneyOptions(Session session, MOBRequest mobRequest, string optionId)
        {
            United.Services.FlightShopping.Common.FlightReservation.FlightReservationResponse response = new United.Services.FlightShopping.Common.FlightReservation.FlightReservationResponse();
            string actionName = string.IsNullOrEmpty(optionId) ? "UndoMilesAndMoneyOption" : "ApplyMilesAndMoneyOption";

            ApplyMilesAndMoneyOptionRequest cslRequest = new ApplyMilesAndMoneyOptionRequest();
            cslRequest.CartId = session.CartId;
            var cartInfo = await GetCartInformation(session.SessionId, mobRequest.Application, session.DeviceID, session.CartId, session.Token);
            cslRequest.Reservation = cartInfo.Reservation;
            cslRequest.DisplayCart = cartInfo.DisplayCart;
            cslRequest.OptionId = optionId;

            string jsonRequest = JsonConvert.SerializeObject(cslRequest);
            string cslresponse = await _flightShoppingProductsServices.ApplyCSLMilesPlusMoneyOptions(session.Token, actionName, jsonRequest, session.SessionId);
            response = JsonConvert.DeserializeObject<United.Services.FlightShopping.Common.FlightReservation.FlightReservationResponse>(cslresponse);
            await RegisterFlights(response, session, mobRequest);

            return response;
        }

        private async Task<LoadReservationAndDisplayCartResponse> GetCartInformation(string sessionId, MOBApplication application, string deviceid, string cartId, string token, WorkFlowType workFlowType = WorkFlowType.InitialBooking)
        {
            LoadReservationAndDisplayCartRequest loadReservationAndDisplayCartRequest = new LoadReservationAndDisplayCartRequest();
            LoadReservationAndDisplayCartResponse loadReservationAndDisplayResponse = new LoadReservationAndDisplayCartResponse();
            loadReservationAndDisplayCartRequest.CartId = cartId;
            loadReservationAndDisplayCartRequest.WorkFlowType = workFlowType;
            string jsonRequest = JsonConvert.SerializeObject(loadReservationAndDisplayCartRequest);

            string url = string.Format("{0}/LoadReservationAndDisplayCart", _configuration.GetValue<string>("ServiceEndPointBaseUrl - ShoppingCartService"));

            loadReservationAndDisplayResponse = await _shoppingCartService.GetCartInformation<LoadReservationAndDisplayCartResponse>(token, "LoadReservationAndDisplayCart", jsonRequest, sessionId);

            return loadReservationAndDisplayResponse;
        }

        private async Task AssignUpdatedPricesToReservation(string sessionId, United.Services.FlightShopping.Common.FlightReservation.FlightReservationResponse cslReservation, string optionId, List<MOBProdDetail> products)
        {
            Reservation bookingPathReservation = new Reservation();
            bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(sessionId, bookingPathReservation.ObjectName, new List<string>() { sessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);


            List<DisplayPrice> displayPrices = cslReservation?.DisplayCart?.DisplayPrices;

            if (displayPrices != null)
            {
                foreach (var price in displayPrices)
                {
                    if (string.IsNullOrEmpty(price.Description))
                    {
                        price.Description = price.Type;
                    }
                }
            }

            BuildMoneyPlusMilesPrice(bookingPathReservation.Prices, displayPrices, string.IsNullOrEmpty(optionId), products);
            AddGrandTotalIfNotExistInPrices(bookingPathReservation.Prices);

           await  _sessionHelperService.SaveSession(bookingPathReservation, sessionId, new List<string> { sessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);
        }

        private void AddGrandTotalIfNotExistInPrices(List<MOBSHOPPrice> prices)
        {
            var grandTotalPrice = prices.FirstOrDefault(p => p.DisplayType.ToUpper().Equals("GRAND TOTAL"));
            if (grandTotalPrice == null)
            {
                var totalPrice = prices.FirstOrDefault(p => p.DisplayType.ToUpper().Equals("TOTAL"));
                grandTotalPrice = BuildGrandTotalPriceForReservation(totalPrice.Value);
                prices.Add(grandTotalPrice);
            }
        }

        private MOBSHOPPrice BuildGrandTotalPriceForReservation(double grandtotal)
        {
            grandtotal = Math.Round(grandtotal, 2, MidpointRounding.AwayFromZero);
            MOBSHOPPrice totalPrice = new MOBSHOPPrice();
            totalPrice.CurrencyCode = "USD";
            totalPrice.DisplayType = "Grand Total";
            totalPrice.DisplayValue = grandtotal.ToString("N2", CultureInfo.InvariantCulture);
            totalPrice.FormattedDisplayValue = formatAmountForDisplay(totalPrice.DisplayValue, _eTCUtility.GetCultureInfo(totalPrice.CurrencyCode), false); //string.Format("${0:c}", totalPrice.DisplayValue);
            double tempDouble1 = 0;
            double.TryParse(totalPrice.DisplayValue.ToString(), out tempDouble1);
            totalPrice.Value = Math.Round(tempDouble1, 2, MidpointRounding.AwayFromZero);
            totalPrice.PriceType = "Grand Total";

            //_logger.LogInformation("BuildGrandTotalPriceForReservation TotalPrice {totalPrice} sessionID{sessionId}", totalPrice, Headers.ContextValues.SessionId);

            return totalPrice;
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
                if (_configuration.GetValue<bool>("EnableFSRMoneyPlusMilesFeature"))
                {
                    var mplusmPrice = prices.FirstOrDefault(p => p.DisplayType == "MONEYPLUSMILES");
                    if (mplusmPrice != null)
                    {
                        prices.Remove(mplusmPrice);
                        isDirty = true;
                    }
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
                    CultureInfo ci = _eTCUtility.GetCultureInfo(mmPrice.Currency);
                    bookingPrice.FormattedDisplayValue = "-" + formatAmountForDisplay(mmPrice.Amount.ToString(), ci, false); // Money and Miles have to be displayed in -ve format as per MOBILE-14807
                    prices.Add(bookingPrice);
                    mmValue = bookingPrice.Value;
                    isDirty = true;
                }
            }

            if (isDirty)
            {
                var price = prices.FirstOrDefault(p => p.DisplayType == "RES");
                _eTCUtility.UpdateCertificateRedeemAmountFromTotalInReserationPrices(price, mmValue, !isRemove);
                price = prices.FirstOrDefault(p => p.DisplayType == "TOTAL");
                _eTCUtility.UpdateCertificateRedeemAmountFromTotalInReserationPrices(price, mmValue, !isRemove);
                price = prices.FirstOrDefault(p => p.DisplayType.ToUpper().Equals("GRAND TOTAL"));
                _eTCUtility.UpdateCertificateRedeemAmountFromTotalInReserationPrices(price, mmValue, !isRemove);
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
                scProduct.ProdTotalPrice = (prodValueAfterUpdate).ToString("N2", CultureInfo.CurrentCulture);
                scProduct.ProdDisplayTotalPrice = (prodValueAfterUpdate).ToString("C2", CultureInfo.CurrentCulture);
            }
        }

        private string formatAmountForDisplay(string amt, CultureInfo ci, bool roundup = true, bool isAward = false)
        {
            string newAmt = amt;
            decimal amount = 0;
            decimal.TryParse(amt, out amount);

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

        private async Task<FlightReservationResponse> RegisterFlights(FlightReservationResponse flightReservationResponse, Session session, MOBRequest request)
        {
            string flow = session.Flow;
            var registerFlightRequest = BuildRegisterFlightRequest(flightReservationResponse, flow, request, session.CartId);
            string jsonRequest = JsonConvert.SerializeObject(registerFlightRequest);
            FlightReservationResponse response = new FlightReservationResponse();
            string url = "RegisterFlights";
            
            response = await _shoppingCartService.RegisterFlights<FlightReservationResponse>(session.Token, url, jsonRequest, session.SessionId);
            if (response != null)
            {
                if (!(response != null && response.Status.Equals(United.Services.FlightShopping.Common.StatusType.Success) && response.DisplayCart != null && response.Reservation != null))
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
                throw new MOBUnitedException(milesMoneyErrorMessage != null ? milesMoneyErrorMessage : _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }

            return response;
        }

        private RegisterFlightsRequest BuildRegisterFlightRequest(United.Services.FlightShopping.Common.FlightReservation.FlightReservationResponse flightReservationResponse, string flow, MOBRequest mobRequest, string cartid)
        {
            RegisterFlightsRequest request = new RegisterFlightsRequest();
            request.CartId = cartid;
            request.CartInfo = flightReservationResponse.DisplayCart;
            request.CountryCode = flightReservationResponse.DisplayCart.CountryCode;//TODO:Check this is populated all the time.
            request.Reservation = flightReservationResponse.Reservation;
            request.DeviceID = mobRequest.DeviceId;
            request.Upsells = flightReservationResponse.Upsells;
            request.MerchOffers = flightReservationResponse.MerchOffers;
            request.LoyaltyUpgradeOffers = flightReservationResponse.LoyaltyUpgradeOffers;
            request.WorkFlowType = GetWorkFlowType(flow);
            request.AppliedMilesAndMoney = true;

            _logger.LogInformation("BuildRegisterFlightRequest Request{request} with SessionId{sessoinId}", JsonConvert.SerializeObject(request), _headers.ContextValues.SessionId);

            return request;
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

        private bool IsPostBookingPromoCodeEnabled(int applicationId, string appVersion)
        {
            if (_configuration.GetValue<bool>("EnableCouponsInPostBooking") && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Android_EnablePromoCodePostBooking_AppVersion"), _configuration.GetValue<string>("iPhone_EnablePromoCodePostBooking_AppVersion")))
            {
                return true;
            }

            return false;
        }

        private bool IsPromoCodeEligibleFlow(string flow)
        {
            string[] flows;
            flows = _configuration.GetValue<string>("EligiblePromoCodeFlows").Split('|');
            if (flows.Contains(flow))
            {
                return true;
            }

            return false;
        }

        private async Task<(MOBApplyPromoCodeResponse ApplyPromoCodeResponse, MOBItem inEligibleReason)> ApplyPromoCodeForStandaloneProduct(MOBApplyPromoCodeRequest request, Session session, bool isByPassValidations = false)
        {
            MOBApplyPromoCodeResponse response = new MOBApplyPromoCodeResponse();
            MOBItem inEligibleReason;
            var persistShoppingCart = new MOBShoppingCart();
            persistShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(session.SessionId, persistShoppingCart.ObjectName, new List<string>() { session.SessionId, persistShoppingCart.ObjectName }).ConfigureAwait(false);

            inEligibleReason = new MOBItem();
            United.Services.FlightShopping.Common.FlightReservation.FlightReservationResponse cslFlightReservationResponse = new United.Services.FlightShopping.Common.FlightReservation.FlightReservationResponse();
            //Code to be cleaned after 23C Manage Res Release - temp adding togggle for saftey purpose
            if (!_configuration.GetValue<bool>("DisableManageResChanges23C"))
            {
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
                    }
                    if (request.PromoCodes.Any(p => !p.IsRemove))
                    {
                        if (IsMaxPromoCountReached(persistShoppingCart))
                        {
                            response.MaxCountMessage = _configuration.GetValue<string>("MaxPromoCodeMessage");
                            response.ShoppingCart = persistShoppingCart;
                            return (response, inEligibleReason);
                        }
                        cslFlightReservationResponse = await RegisterOrRemoveCoupon(request, session, false);
                    }
                    #endregion coupon Service Call       
                    if (_configuration.GetValue<bool>("IsEnableManageResCoupon") && cslFlightReservationResponse != null && !string.IsNullOrEmpty(request.Flow) && (request.Flow == FlowType.VIEWRES.ToString() || request.Flow == FlowType.VIEWRES_SEATMAP.ToString()) && cslFlightReservationResponse.Warnings != null && cslFlightReservationResponse.Warnings.Count > 0)
                    {
                        response.ShoppingCart = persistShoppingCart;

                        var warnings = _configuration.GetValue<bool>("EnablePromoCodeForAncillaryOffersManageRes") ? cslFlightReservationResponse.Warnings.FirstOrDefault(error => (error.MajorCode == "2000" || error.MajorCode == "3000")) : null;
                        if (warnings != null && !string.IsNullOrEmpty(warnings.MajorCode) && !string.IsNullOrEmpty(warnings.Message) && warnings.MajorCode == "2000" || warnings.MinorCode == "3000")
                        {
                            inEligibleReason.Id = warnings.MajorCode;
                            inEligibleReason.CurrentValue = warnings.Message;
                        }

                        return (response, inEligibleReason);
                    }
                    persistShoppingCart.Products = await _shoppingCartUtility.ConfirmationPageProductInfo(cslFlightReservationResponse, false, false, request.Flow, request.Application, null, false, false, null, null, sessionId: session.SessionId);
                    double price = 0.0;
                    price = GetGrandTotalPriceForShoppingCart(false, cslFlightReservationResponse, false, request.Flow);
                    #region Save ShoppingCart   
                    persistShoppingCart.TotalPrice = String.Format("{0:0.00}", price);
                    persistShoppingCart.DisplayTotalPrice = Decimal.Parse(price.ToString()).ToString("c");
                    UpdateShoppinCartWithCouponDetails(persistShoppingCart);
                    InitialiseFop(persistShoppingCart, request.PromoCodes.Any(p => p.IsRemove));
                    IsHidePaymentMethod(persistShoppingCart);
                    if (_configuration.GetValue<bool>("IsEnableManageResCoupon") && (request.Flow == FlowType.VIEWRES.ToString() || request.Flow == FlowType.VIEWRES_SEATMAP.ToString()))
                    {
                        UpdateTravelCerticatePrices(persistShoppingCart);
                    }

                    await _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, session.SessionId, new List<string> { session.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName).ConfigureAwait(false);
                    #endregion Save ShoppingCart          
                }
                catch (System.Net.WebException wex)
                {
                    throw wex;
                }
                catch (United.Definition.MOBUnitedException uaex)
                {
                    //If it is a soft failure from service it should be thrown as united exception .
                    //But, no need to rethrow back to contrller since client shows Soft failures from service as inline instead of popup
                    if (!string.IsNullOrEmpty(uaex.Code) && (uaex.Code == "2000" || uaex.Code == "3000"))
                    {
                        inEligibleReason.Id = uaex.Code;
                        inEligibleReason.CurrentValue = uaex.Message;
                        MOBExceptionWrapper uaexWrapper = new MOBExceptionWrapper(uaex);
                        _logger.LogWarning("ApplyPromoCode - invalid PromoCode exception {sessionId} and {UnitedException}", session, uaexWrapper);
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
            }
            else {
                #region coupon Service Call        

                if (request.PromoCodes.Any(p => p.IsRemove))
                {
                    if (IsUpliftFopAdded(persistShoppingCart) && !isByPassValidations)
                    {
                        throw new MOBUnitedException(_configuration.GetValue<string>("RemovePromo_UpliftAddedMessage"));
                    }
                    cslFlightReservationResponse = await RegisterOrRemoveCoupon(request, session, true);
                }
                if (request.PromoCodes.Any(p => !p.IsRemove))
                {
                    if (IsMaxPromoCountReached(persistShoppingCart))
                    {
                        response.MaxCountMessage = _configuration.GetValue<string>("MaxPromoCodeMessage");
                        response.ShoppingCart = persistShoppingCart;
                        return (response, inEligibleReason);
                    }
                    cslFlightReservationResponse = await RegisterOrRemoveCoupon(request, session, false);
                }
                #endregion coupon Service Call       
                if (_configuration.GetValue<bool>("IsEnableManageResCoupon") && cslFlightReservationResponse != null && !string.IsNullOrEmpty(request.Flow) && (request.Flow == FlowType.VIEWRES.ToString() || request.Flow == FlowType.VIEWRES_SEATMAP.ToString()) && cslFlightReservationResponse.Warnings != null && cslFlightReservationResponse.Warnings.Count > 0)
                {
                    response.ShoppingCart = persistShoppingCart;

                    var warnings = _configuration.GetValue<bool>("EnablePromoCodeForAncillaryOffersManageRes") ? cslFlightReservationResponse.Warnings.FirstOrDefault(error => (error.MajorCode == "2000" || error.MajorCode == "3000")) : null;
                    if (warnings != null && !string.IsNullOrEmpty(warnings.MajorCode) && !string.IsNullOrEmpty(warnings.Message) && warnings.MajorCode == "2000" || warnings.MinorCode == "3000")
                    {
                        inEligibleReason.Id = warnings.MajorCode;
                        inEligibleReason.CurrentValue = warnings.Message;
                    }

                    return (response, inEligibleReason);
                }
                persistShoppingCart.Products = await _shoppingCartUtility.ConfirmationPageProductInfo(cslFlightReservationResponse, false, false, request.Flow, request.Application, null, false, false, null, null, sessionId: session.SessionId);
                double price = 0.0;
                price = GetGrandTotalPriceForShoppingCart(false, cslFlightReservationResponse, false, request.Flow);
                #region Save ShoppingCart   
                persistShoppingCart.TotalPrice = String.Format("{0:0.00}", price);
                persistShoppingCart.DisplayTotalPrice = Decimal.Parse(price.ToString()).ToString("c");
                UpdateShoppinCartWithCouponDetails(persistShoppingCart);
                InitialiseFop(persistShoppingCart, request.PromoCodes.Any(p => p.IsRemove));
                IsHidePaymentMethod(persistShoppingCart);
                if (_configuration.GetValue<bool>("IsEnableManageResCoupon") && (request.Flow == FlowType.VIEWRES.ToString() || request.Flow == FlowType.VIEWRES_SEATMAP.ToString()))
                {
                    UpdateTravelCerticatePrices(persistShoppingCart);
                }

                await _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, session.SessionId, new List<string> { session.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName).ConfigureAwait(false);
                #endregion Save ShoppingCart          
            }

            response.ShoppingCart = persistShoppingCart;

            return (response, inEligibleReason);
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

        private void AddPromoCodeIneligibleReason(MOBItem inEligibleReason, MOBApplyPromoCodeResponse response, MOBApplyPromoCodeRequest request)
        {
            if (response.ShoppingCart?.PromoCodeDetails == null)
            {
                response.ShoppingCart.PromoCodeDetails = new MOBPromoCodeDetails();
                response.ShoppingCart.PromoCodeDetails.PromoCodes = new List<MOBPromoCode>();
            }
            if (inEligibleReason != null && !string.IsNullOrEmpty(inEligibleReason.CurrentValue))
            {
                response.ShoppingCart.PromoCodeDetails.PromoCodes
                      .Add(new MOBPromoCode
                      {
                          PromoCode = request.PromoCodes.Where(p => !p.IsRemove).FirstOrDefault().PromoCode,
                          AlertMessage = inEligibleReason.CurrentValue,
                          IsSuccess = false,
                          TermsandConditions = inEligibleReason.Id == "3000" ? null : new MOBMobileCMSContentMessages
                          {
                              Title = _configuration.GetValue<string>("PromoCodeTermsandConditionsTitle"),
                              HeadLine = _configuration.GetValue<string>("PromoCodeTermsandConditionsTitle")
                          }
                      });
            }
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
        private bool IsOfferApplied(MOBShoppingCart persistShoppingCart)
        {
            if (persistShoppingCart?.Offers != null
                && !string.IsNullOrEmpty(persistShoppingCart?.Offers.OfferCode))
            {
                return true;
            }
            return false;
        }

        private async Task<FlightReservationResponse> RegisterOrRemoveCoupon(MOBApplyPromoCodeRequest request, Session session, bool isRemove)
        {
            //string cslEndpoint = _configuration.GetValue<string>("ServiceEndPointBaseUrl - ShoppingCartService");
            var registerCouponRequest = BuildRegisterOrRemoveCouponRequest(request, isRemove);
            string jsonRequest = JsonConvert.SerializeObject(registerCouponRequest);
            FlightReservationResponse response = new FlightReservationResponse();
            string cslActionName = isRemove ? "RemoveCoupon" : "RegisterCoupon";

            response = await _shoppingCartService.RegisterOrRemoveCoupon<FlightReservationResponse>(session.Token, cslActionName, jsonRequest, session.SessionId);

            #region

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

        private async Task<MOBPromoCodeTermsandConditionsResponse> GetTermsandCondtionsByPromoCode(MOBApplyPromoCodeRequest request, string token)
        {
            MOBPromoCodeTermsandConditionsResponse response = new MOBPromoCodeTermsandConditionsResponse();

            if (_configuration.GetValue<bool>("IsEnableCCEPromoTermsConditions"))
            {
                var cceRequest = BuildCCERequest(request);
                response = await GetCCEContent(cceRequest, request, token);
            }
            else
            {
                #region          
                string url = request.PromoCodes.First().PromoCode + ".html";
                SDLContentResponse sdlResponse = new SDLContentResponse();

                string cslresponse = await _getTermsandCondtionsService.GetTermsandCondtionsByPromoCode(url, request.SessionId, token);

                if (!string.IsNullOrEmpty(cslresponse))
                {
                    sdlResponse = JsonConvert.DeserializeObject<SDLContentResponse>(cslresponse);

                    if (sdlResponse != null && sdlResponse.content != null && sdlResponse.content.Any())
                    {
                        response.TermsandConditions = new MOBMobileCMSContentMessages();
                        var contentItem = sdlResponse.content.FirstOrDefault();
                        if (contentItem != null)
                        {
                            response.TermsandConditions.ContentFull = contentItem.content.body;
                            response.TermsandConditions.Title = _configuration.GetValue<string>("PromoCodeTermsandConditionsTitle");
                            response.TermsandConditions.HeadLine = contentItem.content.title;
                        }
                    }
                    else
                    {
                        if (sdlResponse.ErrorList != null && sdlResponse.ErrorList.Any())
                        {
                            string errorMessage = string.Empty;
                            foreach (var error in sdlResponse.ErrorList)
                            {
                                errorMessage = errorMessage + " " + (error.Code + error.Message);
                            }
                            throw new System.Exception(errorMessage);
                        }
                    }
                }
                else
                {
                    throw new Exception(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
                #endregion
            }

            return response;
        }
        #endregion PromoCode Terms and conditons

        private ContextualCommRequest BuildCCERequest(MOBApplyPromoCodeRequest request)
        {
            ContextualCommRequest cceRequest = new ContextualCommRequest();
            cceRequest.ComponentsToLoad = new Collection<string>
             {
                _configuration.GetValue<string>("CCERequestTCComponentToLoad")
             };
            cceRequest.PageToLoad = _configuration.GetValue<string>("CCERequestTCPageToLoad");
            cceRequest.MileagePlusId = string.Empty;
            cceRequest.DeviceId = request?.DeviceId;
            cceRequest.ChannelType = _configuration.GetValue<string>("CCERequestChannelName");
            cceRequest.IPCountry = _configuration.GetValue<string>("CCERequestCountryCode");
            cceRequest.LangCode = _configuration.GetValue<string>("CCERequestLanguageCode");
            cceRequest.Browser = (request?.Application?.Id == 1) ? "iOS" : "Android";
            // If switch is not available then we convert coupon code to upper case when calling CCE service
            cceRequest.CouponCode = !_configuration.GetValue<bool>("IgnoreCouponCodeCaseInSensitive") ? request?.PromoCodes?.First()?.PromoCode.ToUpper() : request.PromoCodes.First().PromoCode;

            _logger.LogInformation("BuildCCERequest CCERequest {cceRequest} sessionID{sessionId}", JsonConvert.SerializeObject(cceRequest), request.SessionId);

            return cceRequest;
        }

        private async Task<MOBPromoCodeTermsandConditionsResponse> GetCCEContent(ContextualCommRequest cceRequest, MOBApplyPromoCodeRequest request, string token)
        {
            MOBPromoCodeTermsandConditionsResponse response = new MOBPromoCodeTermsandConditionsResponse();
            var jsonRequest = JsonConvert.SerializeObject(cceRequest);
            // string _cceBaseUrl = _configuration.GetValue<string>("ServiceEndPointBaseUrl - CCE");
            var url = $"mobile/messages";

            if (!_configuration.GetValue<bool>("AddSDLUrlForGettingPromoCodeInAlert"))
            {
                string cslEndpoint = _configuration.GetValue<string>("ServiceEndPointBaseUrl - SDLPromoCodeTermsandConditions");
                string SDLUrl = string.Format("{0}{1}", cslEndpoint, request?.PromoCodes?.First()?.PromoCode + ".html");

                _logger.LogInformation("GetCCEContent CouponCode{CouponCode} SessionId:{sessionId}", request?.PromoCodes?.First()?.PromoCode, request?.SessionId);
            }

            string cslresponse = await _getCCEContentService.GetCCEContent(token, url, jsonRequest, request?.SessionId);

            if (!string.IsNullOrEmpty(cslresponse))
            {
                var cceResponse = string.IsNullOrEmpty(cslresponse) ? null
                 : JsonConvert.DeserializeObject<ContextualCommResponse>(cslresponse);
                if (cceResponse?.Components != null && cceResponse?.Components?.Count > 0)
                {
                    var cceComponent = cceResponse?.Components?.FirstOrDefault()?.ContextualElements?.FirstOrDefault()?.Value;
                    var valueJson = cceComponent != null ? JsonConvert.SerializeObject(cceComponent) : null;
                    ContextualMessage value = JsonConvert.DeserializeObject<ContextualMessage>(valueJson);
                    if (value != null && value.Content != null)
                    {
                        response.TermsandConditions = new MOBMobileCMSContentMessages();
                        response.TermsandConditions.ContentFull = value?.Content?.BodyText;
                        response.TermsandConditions.Title = _configuration.GetValue<string>("PromoCodeTermsandConditionsTitle");
                        response.TermsandConditions.HeadLine = !_configuration.GetValue<bool>("IsRemoveHeaderForDynamicTC") && !string.IsNullOrEmpty(value?.Content?.Title) && value?.Content?.Title != "~$TITLE$~" ? value?.Content?.Title : string.Empty;
                    }
                }
                else if (cceResponse?.Error != null && cceResponse?.Error?.Count > 0)
                {
                    string errorMessage = string.Empty;
                    foreach (var error in cceResponse.Error)
                    {
                        errorMessage = errorMessage + " " + (error?.Text);

                    }
                    throw new System.Exception(errorMessage);
                }
                else
                {
                    throw new Exception(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
                }
                // sdlResponse = JsonConvert.DeserializeObject<SDLContentResponse>(jsonResponse);

            }
            else
            {
                throw new Exception(_configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }

            return response;
        }

        private void InitialiseFop(MOBShoppingCart shoppingCart, bool isRemove)
        {
            if (!_configuration.GetValue<bool>("DisableFreePBSCouponCheckoutFix"))
            {
                if (Decimal.TryParse(shoppingCart.TotalPrice, out decimal totalPrice)
                     && totalPrice == 0
                     && shoppingCart.FormofPaymentDetails == null)
                {
                    shoppingCart.FormofPaymentDetails = new MOBFormofPaymentDetails();
                }
                if (isRemove && string.IsNullOrEmpty(shoppingCart.FormofPaymentDetails?.FormOfPaymentType))
                {
                    shoppingCart.FormofPaymentDetails = null;
                }
            }

        }

        private void IsHidePaymentMethod(MOBShoppingCart shoppingCart)
        {
            if (!_configuration.GetValue<bool>("DisableFreePBSCouponCheckoutFix") && IsFullAmountCoveredWithCoupon(shoppingCart))
            {
                shoppingCart.IsHidePaymentMethod = true;
            }
            else
            {
                shoppingCart.IsHidePaymentMethod = false;
            }
        }

        private void UpdateTravelCerticatePrices(MOBShoppingCart persistShoppingCart)
        {
            if (persistShoppingCart?.FormofPaymentDetails?.TravelCertificate?.Certificates?.Count > 0)
            {
                persistShoppingCart.FormofPaymentDetails.TravelCertificate.AllowedETCAmount = _eTCUtility.GetAlowedETCAmount(persistShoppingCart.Products, persistShoppingCart.Flow);
                persistShoppingCart?.FormofPaymentDetails?.TravelCertificate?.Certificates?.ForEach(c => c.RedeemAmount = 0);
                _eTCUtility.AddRequestedCertificatesToFOPTravelerCertificates(persistShoppingCart?.FormofPaymentDetails?.TravelCertificate?.Certificates, persistShoppingCart.ProfileTravelerCertificates, persistShoppingCart.FormofPaymentDetails.TravelCertificate);
                var certificatePrice = persistShoppingCart.Prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "CERTIFICATE");
                _eTCUtility.UpdateCertificatePrice(certificatePrice, persistShoppingCart.FormofPaymentDetails.TravelCertificate.TotalRedeemAmount);
                _eTCUtility.AssignTotalAndCertificateItemsToPrices(persistShoppingCart);
                _eTCUtility.AssignIsOtherFOPRequired(persistShoppingCart.FormofPaymentDetails, persistShoppingCart.Prices);
            }
            else
            {
                UpdatePricesGrandTotalWithProductsTotalValue(persistShoppingCart);
            }
        }

        private void UpdatePricesGrandTotalWithProductsTotalValue(MOBShoppingCart shopingCart)
        {
            var grandtotal = shopingCart.Prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "GRAND TOTAL");
            double total = shopingCart.Products.Sum(p => Convert.ToDouble(p.ProdTotalPrice));
            grandtotal.Value = Math.Round(total, 2, MidpointRounding.AwayFromZero);
            grandtotal.FormattedDisplayValue = (grandtotal.Value).ToString("C2", CultureInfo.CurrentCulture);
            grandtotal.DisplayValue = string.Format("{0:#,0.00}", grandtotal.Value);
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


        private bool IsFullAmountCoveredWithCoupon(MOBShoppingCart shoppingCart)
        {
            if (shoppingCart?.PromoCodeDetails?.PromoCodes != null
                && shoppingCart.PromoCodeDetails.PromoCodes.Count > 0
                && shoppingCart?.Products != null)
            {
                Double totalPrice = 0, totalDiscountPrice = 0;
                foreach (var products in shoppingCart.Products)
                {
                    foreach (var segments in products.Segments)
                    {
                        foreach (var subSegments in segments.SubSegmentDetails)
                        {
                            totalPrice += Convert.ToDouble(subSegments.OrginalPrice);
                            totalDiscountPrice += subSegments.PromoDetails != null ? subSegments.PromoDetails.PromoValue : 0;
                        }
                    }
                }
                if (totalPrice == totalDiscountPrice)
                    return true;
            }

            return false;
        }

        private List<MOBMobileCMSContentMessages> GetSDLMessageFromList(List<CMSContentMessage> list, string title)
        {
            List<MOBMobileCMSContentMessages> listOfMessages = new List<MOBMobileCMSContentMessages>();
            list?.Where(l => l.Title.ToUpper().Equals(title.ToUpper()))?.ForEach(i => listOfMessages.Add(new MOBMobileCMSContentMessages()
            {
                Title = i.Title,
                ContentFull = i.ContentFull,
                HeadLine = i.Headline,
                ContentShort = i.ContentShort,
                LocationCode = i.LocationCode
            }));

            return listOfMessages;
        }

        private void UpdateShoppinCartWithCouponDetails(MOBShoppingCart persistShoppingCart)
        {
            if (persistShoppingCart != null && persistShoppingCart.Products.Any())
            {
                persistShoppingCart.PromoCodeDetails = new MOBPromoCodeDetails();
                persistShoppingCart.PromoCodeDetails.PromoCodes = new List<MOBPromoCode>();
                persistShoppingCart.Products.ForEach(product =>
                {
                    if (product.CouponDetails != null && product.CouponDetails.Any())
                    {
                        product.CouponDetails.ForEach(CouponDetail =>
                        {
                            if (_configuration.GetValue<bool>("EnableFareandAncillaryPromoCodeChanges") ? !IsDuplicatePromoCode(persistShoppingCart.PromoCodeDetails.PromoCodes, CouponDetail.PromoCode) : true)
                            {
                                persistShoppingCart.PromoCodeDetails.PromoCodes
                                .Add(new MOBPromoCode
                                {
                                    PromoCode = CouponDetail.PromoCode,
                                    AlertMessage = CouponDetail.Description,
                                    IsSuccess = true,
                                    TermsandConditions = new MOBMobileCMSContentMessages
                                    {
                                        Title = _configuration.GetValue<string>("PromoCodeTermsandConditionsTitle"),
                                        HeadLine = _configuration.GetValue<string>("PromoCodeTermsandConditionsTitle")
                                    }
                                });
                            }
                        });
                    }
                });
            }
        }

        private bool IsDuplicatePromoCode(List<MOBPromoCode> promoCodes, string promoCode)
        {

            if (promoCodes != null && promoCodes.Any() && promoCodes.Count > 0)
            {
                if (promoCodes.Exists(c => c.PromoCode.Equals(promoCode)))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<bool> AddOrRemovePromo(MOBRequest request, Session session, bool isRemove, string flow)
        {

            var persistedShoppingCart = new MOBShoppingCart();

            persistedShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(session.SessionId, persistedShoppingCart.ObjectName, new List<string>() { session.SessionId, persistedShoppingCart.ObjectName }).ConfigureAwait(false);

            List<MOBPromoCode> promoCodes = new List<MOBPromoCode>();
            bool isAdvanceSearchCoupon = EnableAdvanceSearchCouponBooking(request.Application.Id, request.Application.Version.Major);

            var persistedApplyPromoCodeResponse = new MOBApplyPromoCodeResponse();

            persistedApplyPromoCodeResponse = await _sessionHelperService.GetSession<MOBApplyPromoCodeResponse>(session.SessionId, persistedApplyPromoCodeResponse.ObjectName, new List<string>() { session.SessionId, persistedApplyPromoCodeResponse.ObjectName }).ConfigureAwait(false);

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
            var tupleApplyPromoCodeResponse = await ApplyPromoCode(promoCodeRequest, session);
            MOBItem inEligibleReason = tupleApplyPromoCodeResponse.inEligibleReason;
            if (inEligibleReason != null && !string.IsNullOrEmpty(inEligibleReason.CurrentValue))
                return false;

            return true;
        }

        private bool EnableAdvanceSearchCouponBooking(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableAdvanceSearchCouponBooking") && GeneralHelper.IsApplicationVersionGreaterorEqual(appId, appVersion, _configuration.GetValue<string>("AndroidAdvanceSearchCouponBookingVersion"), _configuration.GetValue<string>("iPhoneAdvanceSearchCouponBookingVersion"));
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

        private async Task<(MOBApplyPromoCodeResponse ApplyPromoCodeResponse, MOBItem inEligibleReason)> ApplyPromoCode(MOBApplyPromoCodeRequest request, Session session, bool isByPassValidations = false)
        {
            MOBApplyPromoCodeResponse response = new MOBApplyPromoCodeResponse();
            MOBItem inEligibleReason;
            var persistShoppingCart = new MOBShoppingCart();

            var bookingPathReservationTask =_sessionHelperService.GetSession<Reservation>(session.SessionId, new Reservation().ObjectName, new List<string> { session.SessionId, new Reservation().ObjectName });
            var persistShoppingCartTask = _sessionHelperService.GetSession<MOBShoppingCart>(session.SessionId, persistShoppingCart.ObjectName, new List<string>() { session.SessionId, persistShoppingCart.ObjectName });

            await Task.WhenAll(bookingPathReservationTask, persistShoppingCartTask);

            var bookingPathReservation =  bookingPathReservationTask.Result;

            persistShoppingCart = persistShoppingCartTask.Result;

            inEligibleReason = new MOBItem();
            United.Services.FlightShopping.Common.FlightReservation.FlightReservationResponse cslFlightReservationResponse = new United.Services.FlightShopping.Common.FlightReservation.FlightReservationResponse();
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
                    bool isOfferApplied = IsOfferApplied(persistShoppingCart);
                    if (IsMaxPromoCountReached(persistShoppingCart)|| isOfferApplied)
                    {
                        response.MaxCountMessage = isOfferApplied? _configuration.GetValue<string>("PromoAlreadyappliedMessage") : _configuration.GetValue<string>("MaxPromoCodeMessage");
                        response.ShoppingCart = persistShoppingCart;
                        response.Reservation = new MOBSHOPReservation(_configuration, _cachingService);
                        response.Reservation = await GetReservationFromPersist(response.Reservation, session);
                        return (response, inEligibleReason);
                    }
                    cslFlightReservationResponse = await RegisterOrRemoveCoupon(request, session, false);
                }
                #endregion coupon Service Call
                #region Update Prices with CouponDetails    
                var persistedSCproducts = persistShoppingCart.Products.Clone();
                persistShoppingCart.Products = await _shoppingCartUtility.ConfirmationPageProductInfo(cslFlightReservationResponse, false, false, request.Flow, request.Application, null, false, false, null, null, sessionId: session?.SessionId);
                UpdatePricesWithPromo(cslFlightReservationResponse, bookingPathReservation.Prices.Clone(), persistedSCproducts, persistShoppingCart.Products, bookingPathReservation, session);
                AddFreeBagDetails(persistShoppingCart, bookingPathReservation);
                #endregion Update Prices with CouponDetails  
                #region TPI in booking path
                if (EnableTPI(request.Application.Id, request.Application.Version.Major, 3) && !bookingPathReservation.IsReshopChange)
                {
                    // call TPI 
                    string token = session.Token;
                    MOBTPIInfoInBookingPath tPIInfo = await GetBookingPathTPIInfo(request.SessionId, request.LanguageCode, request.Application, request.DeviceId, request.CartId, token, true, false, false);
                    bookingPathReservation.TripInsuranceFile = new MOBTripInsuranceFile();
                    bookingPathReservation.TripInsuranceFile.TripInsuranceBookingInfo = tPIInfo;

                }
                #endregion
                #region Save Reservation & ShoppingCart              
                double price = _shoppingUtility.GetGrandTotalPriceForShoppingCart(false, cslFlightReservationResponse, false, request.Flow);
                persistShoppingCart.TotalPrice = String.Format("{0:0.00}", price);
                persistShoppingCart.DisplayTotalPrice = Decimal.Parse(price.ToString()).ToString("c", new CultureInfo("en-us"));
                AssignWarningMessage(bookingPathReservation.ShopReservationInfo2, cslFlightReservationResponse.Warnings);
                #region Update the chase banner price total
                // Code is moved to utility so that it can be reused across different actions
                if (_shoppingCartUtility.EnableChaseOfferRTI(request.Application.Id, request.Application.Version.Major))
                    _shoppingCartUtility.UpdateChaseCreditStatement(bookingPathReservation);
                #endregion Update the chase banner price Total

                await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, session.SessionId, new List<string>() { session.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);

                #endregion Save Reservation & ShoppingCart   

               await  _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, session.SessionId, new List<string>() { session.SessionId, persistShoppingCart.ObjectName}, persistShoppingCart.ObjectName).ConfigureAwait(false);
            }
            catch (System.Net.WebException wex)
            {
                throw wex;
            }
            catch (United.Definition.MOBUnitedException uaex)
             {
                //If it is a soft failure from service it should be thrown as united exception .
                //But, no need to rethrow back to contrller since client shows Soft failures from service as inline instead of popup
                if (!string.IsNullOrEmpty(uaex.Code) && (uaex.Code == "2000" || uaex.Code == "3000"))
                {
                    inEligibleReason.Id = uaex.Code;
                    inEligibleReason.CurrentValue = uaex.Message;
                    MOBExceptionWrapper uaexWrapper = new MOBExceptionWrapper(uaex);
                    if (_configuration.GetValue<bool>("DisableInvalidPromoFix") == false &&  persistShoppingCart != null && bookingPathReservation?.Prices?.Count > 0)
                    {
                        // MOBILE-35024 : M+M Applied scenario, Try to apply Invalid Promocode, M+M gets removed and but promo fails in this scenario , shopping cart TotalPrice still has the M+M price
                        // This should be same as TOTAL price , fixing this so that checkout passes
                        persistShoppingCart.TotalPrice = bookingPathReservation?.Prices?.FirstOrDefault(a => a.DisplayType == "TOTAL")?.Value.ToString();
                        await _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, session.SessionId, new List<string>() { session.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName).ConfigureAwait(false);
                    }
                    //LogEntries.Add(United.Logger.LogEntry.GetLogEntry<MOBExceptionWrapper>(request.SessionId, "ApplyPromoCode", "UnitedException", request.Application.Id, request.Application.Version.Major, request.DeviceId, uaexWrapper, true, false));
                    _logger.LogWarning("ApplyPromoCode - invalid PromoCode exception {sessionId} and {exception}",session, uaexWrapper);
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

                    await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, session.SessionId, new List<string>() { session.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);

                   await _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, session.SessionId, new List<string>() { session.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName).ConfigureAwait(false);
                }
            }
            await UpdateTravelBankPrice(request, persistShoppingCart, session, bookingPathReservation);
            response.ShoppingCart = persistShoppingCart;
            response.Reservation = new MOBSHOPReservation(_configuration, _cachingService);
            response.Reservation = await _eTCUtility.GetReservationFromPersist(response.Reservation, session);
            #region Update individual amount in traveler
            await UpdateTravelerIndividualAmount(response.Reservation, response.ShoppingCart, cslFlightReservationResponse, bookingPathReservation, request);

            #endregion Update individual amount in traveler
            #region OfferCode expansion Changes(This properties are added as part of offercode expansion changes but can be later used in general )
            response.ShoppingCart.FormofPaymentDetails.IsFOPRequired = _shoppingCartUtility.IsFOPRequired(response.ShoppingCart, response.Reservation);
            response.ShoppingCart.FormofPaymentDetails.IsEnableAgreeandPurchaseButton = _shoppingCartUtility.IsEnableAgreeandPurchaseButton(response.ShoppingCart, response.Reservation);
            response.ShoppingCart.FormofPaymentDetails.MaskedPaymentMethod = await _shoppingCartUtility.AssignMaskedPaymentMethod(response.ShoppingCart,request.Application).ConfigureAwait(false);
            #endregion OfferCode expansion Changes
            return (response, inEligibleReason);
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
                session = await _sessionHelperService.GetSession<Session>(sessionId, session.ObjectName, new List<string>() { sessionId, session.ObjectName }).ConfigureAwait(false);
            }

            Reservation bookingPathReservation = new Reservation();

            bookingPathReservation= await _sessionHelperService.GetSession<Reservation>(sessionId, bookingPathReservation.ObjectName, new List<string>() { sessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);

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

                bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(sessionId, bookingPathReservation.ObjectName, new List<string>() { sessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);

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
                        //productId = persistregisteredTPIInfo.QuoteId;
                        //delete = true;
                        registerOffersRequest.MerchandizingOfferDetails[0].IsOfferRegistered = true;
                        registerOffersRequest.MerchandizingOfferDetails[0].ProductIds.Add(persistregisteredTPIInfo.QuoteId);
                        var flightReservationResponse = await RegisterOffers(registerOffersRequest, null, null, null, session);
                        var travelOptions = GetTravelBundleOptions(flightReservationResponse.DisplayCart, application.Id, application.Version.Major, isReshop);
                        //bookingPathReservation.Prices = UpdatePriceForUnRegisterTPI(bookingPathReservation.Prices);
                        if (flightReservationResponse != null && flightReservationResponse.Errors.Count() == 0)
                        {
                            MOBShoppingCart persistShoppingCart = new MOBShoppingCart();

                            await _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, session.SessionId, new List<string>() { session.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName).ConfigureAwait(false);

                            if (persistShoppingCart == null)
                                persistShoppingCart = new MOBShoppingCart();

                            persistShoppingCart.Products = await _shoppingCartUtility.ConfirmationPageProductInfo(flightReservationResponse, false, false, null, application, sessionId: session.SessionId);
                            persistShoppingCart.CartId = registerOffersRequest.CartId;
                            double price = _shoppingUtility.GetGrandTotalPriceForShoppingCart(false, flightReservationResponse, false, registerOffersRequest.Flow);
                            persistShoppingCart.TotalPrice = String.Format("{0:0.00}", price);
                            persistShoppingCart.DisplayTotalPrice = Decimal.Parse(price.ToString()).ToString("c");
                            persistShoppingCart.TermsAndConditions = await _shoppingCartUtility.GetProductBasedTermAndConditions(null, flightReservationResponse, false, sessionId, registerOffersRequest.Flow);
                            persistShoppingCart.PaymentTarget = (registerOffersRequest.Flow == FlowType.BOOKING.ToString()) ? _shoppingCartUtility.GetBookingPaymentTargetForRegisterFop(flightReservationResponse) : _shoppingCartUtility.GetPaymentTargetForRegisterFop(flightReservationResponse.DisplayCart.TravelOptions);
                            await _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, session.SessionId, new List<string>() { session.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName).ConfigureAwait(false);
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
                                //productId = persistregisteredTPIInfo.QuoteId;
                                //delete = true;
                                registerOffersRequest.MerchandizingOfferDetails[0].IsOfferRegistered = true;
                                registerOffersRequest.MerchandizingOfferDetails[0].ProductIds.Add(persistregisteredTPIInfo.QuoteId);
                                var flightReservationResponse = await RegisterOffers(registerOffersRequest, null, null, null, session, null);
                                var travelOptions = GetTravelBundleOptions(flightReservationResponse.DisplayCart, application.Id, application.Version.Major, isReshop);
                                //productId = tPIInfo.QuoteId;
                                //delete = false;

                                registerOffersRequest.MerchandizingOfferDetails[0].IsOfferRegistered = false;
                                registerOffersRequest.MerchandizingOfferDetails[0].ProductIds = new List<string>();
                                registerOffersRequest.MerchandizingOfferDetails[0].ProductIds.Add(tPIInfo.QuoteId);
                                flightReservationResponse = await RegisterOffers(registerOffersRequest, null, null, null, session, null);
                                travelOptions = GetTravelBundleOptions(flightReservationResponse.DisplayCart, application.Id, application.Version.Major, isReshop);
                                tPIInfo.ButtonTextInProdPage = _configuration.GetValue<string>("TPIinfo_BookingPath_PRODBtnText_AfterRegister");
                                tPIInfo.CoverCostStatus = _configuration.GetValue<string>("TPIinfo_BookingPath_CoverCostStatus");
                                tPIInfo.IsRegistered = true;
                                if (flightReservationResponse != null && flightReservationResponse.Errors.Count() == 0)
                                {
                                    MOBShoppingCart persistShoppingCart = new MOBShoppingCart();
                                    await _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, session.SessionId, new List<string>() { session.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName).ConfigureAwait(false);

                                    if (persistShoppingCart == null)
                                        persistShoppingCart = new MOBShoppingCart();

                                    persistShoppingCart.Products = await _shoppingCartUtility.ConfirmationPageProductInfo(flightReservationResponse, false, false, FlowType.BOOKING.ToString(), application, sessionId: session.SessionId);
                                    persistShoppingCart.CartId = registerOffersRequest.CartId;
                                    double price = _shoppingUtility.GetGrandTotalPriceForShoppingCart(false, flightReservationResponse, false, registerOffersRequest.Flow);
                                    persistShoppingCart.TotalPrice = String.Format("{0:0.00}", price);
                                    persistShoppingCart.DisplayTotalPrice = Decimal.Parse(price.ToString()).ToString("c");
                                    persistShoppingCart.TermsAndConditions = await _shoppingCartUtility.GetProductBasedTermAndConditions(null, flightReservationResponse, false, sessionId, registerOffersRequest.Flow);
                                    persistShoppingCart.PaymentTarget = (registerOffersRequest.Flow == FlowType.BOOKING.ToString()) ? _shoppingCartUtility.GetBookingPaymentTargetForRegisterFop(flightReservationResponse) : _shoppingCartUtility.GetPaymentTargetForRegisterFop(flightReservationResponse.DisplayCart.TravelOptions);

                                    await _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, session.SessionId, new List<string>() { session.SessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName).ConfigureAwait(false);

                                }
                            }
                            // send pop up message
                            else
                            {
                                tPIInfo.OldAmount = persistregisteredTPIInfo.Amount;
                                tPIInfo.OldQuoteId = persistregisteredTPIInfo.QuoteId;
                                CultureInfo ci = _eTCUtility.GetCultureInfo("en-US");
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

            await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, session.SessionId, new List<string>() { session.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);

            return tPIInfo;
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

        private async Task<MOBTPIInfo> GetTripInsuranceInfo(MOBSHOPProductSearchRequest request, string token, bool isBookingPath = false, Session session = null)
        {
            MOBTPIInfo tripInsuranceInfo = new MOBTPIInfo();
            bool isPostBookingCall = Convert.ToBoolean(_configuration.GetValue<string>("ShowTripInsuranceSwitch") ?? "false");
            // ==>> Venkat and Elise change only one below line of code to offer TPI postbooking when inline TPI is off for new clients 2.1.36 and above
            isPostBookingCall = EnableTPI(request.Application.Id, request.Application.Version.Major, 1);
            if (isPostBookingCall ||
                (Convert.ToBoolean(_configuration.GetValue<string>("ShowTripInsuranceBookingSwitch") ?? "false")
                && isBookingPath)
                )
            {
                string logAction = isBookingPath ? "GetTripInsuranceInfoInBookingPath" : "GetTripInsuranceInfo";

                DisplayCartRequest displayCartRequest = await GetDisplayCartRequest(request, session).ConfigureAwait(false);
                string jsonRequest = JsonConvert.SerializeObject(displayCartRequest);

                //todo
                string url = "GetProducts";

                string cslresponse = await _flightShoppingProductsServices.GetTripInsuranceInfo(token, url, jsonRequest, request.SessionId);

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

                        await _sessionHelperService.SaveSession(productVendorOffer, request.SessionId, new List<string>() { request.SessionId, productVendorOffer.ObjectName }, productVendorOffer.ObjectName).ConfigureAwait(false);
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

                    await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation,request.SessionId, new List<string>() { request.SessionId, bookingPathReservation.ObjectName}, bookingPathReservation.ObjectName).ConfigureAwait(false);

                    if (bookingPathReservation.TripInsuranceFile == null)
                    {
                        bookingPathReservation.TripInsuranceFile = new MOBTripInsuranceFile() { };
                    }
                    bookingPathReservation.TripInsuranceFile.TripInsuranceInfo = tripInsuranceInfo;

                    await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, request.SessionId, new List<string>() { request.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);

                }

            }

            _logger.LogInformation("GetTripInsuranceInfo TripInsuranceInfo {tripInsuranceInfo} sessionID{sessionId}", JsonConvert.SerializeObject(tripInsuranceInfo), request.SessionId);

            return tripInsuranceInfo;
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

        private bool EnableTPI(int appId, string appVersion, int path)
        {
            // path 1 means confirmation flow, path 2 means view reservation flow, path 3 means booking flow 
            if (path == 1)
            {
                // ==>> Venkat and Elise chagne code to offer TPI postbooking when inline TPI is off for new clients 2.1.36 and above
                // App Version 2.1.36 && ShowTripInsuranceSwitch = true
                bool offerTPIAtPostBooking = _configuration.GetValue<bool>("ShowTripInsuranceSwitch") &&
                    GeneralHelper.isApplicationVersionGreater(appId, appVersion, "AndroidTPIConfirmationVersion", "iPhoneTPIConfirmationVersion", "", "", true, _configuration);
                if (offerTPIAtPostBooking)
                {
                    offerTPIAtPostBooking = !GeneralHelper.isApplicationVersionGreater(appId, appVersion, "AndroidTPIBookingVersion", "iPhoneTPIBookingVersion", "", "", true, _configuration);                    // When the Flag is true, we offer for old versions, when the flag is off, we offer for all versions. 
                    if (!offerTPIAtPostBooking && _configuration.GetValue<bool>("ShowTPIatPostBooking_ForAppVer_2.1.36_UpperVersions"))
                    {
                        //"ShowTripInsuranceBookingSwitch" == false
                        //ShowTPIatPostBooking_ForAppVer_2.1.36_LowerVersions = true
                        //
                        offerTPIAtPostBooking = true;
                    }
                }
                return offerTPIAtPostBooking;
            }
            else if (path == 2)
            {
                return _configuration.GetValue<bool>("ShowTripInsuranceViewResSwitch") &&
                    GeneralHelper.isApplicationVersionGreater(appId, appVersion, "AndroidTPIViewResVersion", "iPhoneTPIViewResVersion", "", "", true, _configuration);
            }
            else if (path == 3)
            {
                return _configuration.GetValue<bool>("ShowTripInsuranceBookingSwitch") &&
                    GeneralHelper.isApplicationVersionGreater(appId, appVersion, "AndroidTPIBookingVersion", "iPhoneTPIBookingVersion", "", "", true, _configuration);
            }
            else
            {
                return false;
            }
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

            }
            if (tripInsuranceInfo == null && Convert.ToBoolean(_configuration.GetValue<string>("Log_TI_Offer_If_AIG_NotOffered_At_BookingPath") ?? "false"))
            {
                _logger.LogError("GetTripInsuranceDetails UnitedException{UnitedException} SessionId:{sessionId}",
                    "AIG Not Offered Trip Insuracne in Booking Path", sessionId);

            }

            _logger.LogInformation("GetTripInsuranceDetails TripInsuranceInfo {tripInsuranceInfo} sessionID{sessionId}", JsonConvert.SerializeObject(tripInsuranceInfo), sessionId);

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
            if (product.SubProducts[0].Prices == null || product.Presentation == null|| product.Presentation.Contents == null)
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

            _logger.LogInformation("GetTPIDetails TripInsuranceInfo {tripInsuranceInfo} sessionID{sessionId}", JsonConvert.SerializeObject(tripInsuranceInfo), sessionId);

            return tripInsuranceInfo;
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
                        tripInsuranceInfo.TileImage = tripInsuranceInfo.Image;
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

                bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(sessionId, bookingPathReservation.ObjectName, new List<string> { sessionId, bookingPathReservation.ObjectName });

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
            CultureInfo ci = _eTCUtility.GetCultureInfo(currencyCode);
            formattedDisplayAmount = formatAmountForDisplay(string.Format("{0:#,0.00}", amount), ci, isRoundUp);

            return formattedDisplayAmount;
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



        private List<MOBSHOPTravelOption> GetTravelBundleOptions(DisplayCart displayCart, int appID, string appVersion, bool isReshop)
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

                    if (ci == null)
                    {
                        ci = _eTCUtility.GetCultureInfo(anOption.Currency);
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

        private void GetTravelOptionAncillaryDescription(SubitemsCollection subitemsCollection, MOBSHOPTravelOption travelOption, DisplayCart displayCart)
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
                                ci = _eTCUtility.GetCultureInfo(item.Currency);
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

        private string formatAmountForDisplay(decimal amt, CultureInfo ci, bool roundup = true, bool isAward = false)
        {
            return formatAmountForDisplay(amt.ToString(), ci, roundup, isAward);
        }

        private bool EnableEPlusAncillary(int appID, string appVersion, bool isReshop = false)
        {
            return _configuration.GetValue<bool>("EnableEPlusAncillaryChanges") && !isReshop && GeneralHelper.IsApplicationVersionGreaterorEqual(appID, appVersion, "EplusAncillaryAndroidversion", "EplusAncillaryiOSversion");
        }

        private List<MOBSHOPTravelOptionSubItem> GetTravelOptionSubItems(SubitemsCollection subitemsCollection)
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
                        ci = _eTCUtility.GetCultureInfo(item.Currency);
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

        private List<MobShopBundleEplus> GetTravelOptionEplus(SubitemsCollection subitemsCollection)
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

                }

            }

            return bundlecode;
        }

        private List<MobShopBundleEplus> GetTravelOptionEplusAncillary(SubitemsCollection subitemsCollection, List<MobShopBundleEplus> bundlecode)
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

        private bool IncludeTravelBankFOP(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableTravelBankFOP")
                && GeneralHelper.isApplicationVersionGreater
                (appId, appVersion, "AndroidTravelBankFOPVersion", "iPhoneTravelBankFOPVersion", "", "", true, _configuration);
        }

        private async Task<MOBFOPResponse> TravelBankCredit(Session session, MOBFOPTravelerBankRequest request, bool isCustomerCall)
        {
            Reservation bookingPathReservation= new Reservation ();
            var promoCodeBusinessResult= await LoadBasicFOPResponse(session, bookingPathReservation);
            MOBFOPResponse response = promoCodeBusinessResult.MobFopResponse;
            bookingPathReservation = promoCodeBusinessResult.BookingPathReservation;
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
            List<CMSContentMessage> lstMessages = await GetSDLContentByGroupName(request, session.SessionId, session.Token, _configuration.GetValue<string>("CMSContentMessages_GroupName_BookingRTI_Messages"), "BookingPathRTI_CMSContentMessagesCached_StaticGUID");
            travelBankDetails.ApplyTBContentMessage = GetSDLContentMessages(lstMessages, "RTI.TravelBank.Apply");
            //travelBankDetails.ReviewTBContentMessage = GetSDLContentMessages(lstMessages, "RTI.TravelBank.Review");
            travelBankDetails.LearnmoreTermsandConditions = GetSDLContentMessages(lstMessages, "RTI.TravelBank.Learnmore");
            SwapTitleAndLocation(travelBankDetails.ApplyTBContentMessage);
            //SwapTitleAndLocation(travelBankDetails.ReviewTBContentMessage);
            SwapTitleAndLocation(travelBankDetails.LearnmoreTermsandConditions);
            UpdateCertificateAmountInTotalPrices(bookingPathReservation.Prices, (request.IsRemove ? 0 : travelBankDetails.TBApplied), "TB", "TravelBank cash");
            response.Reservation.Prices = bookingPathReservation.Prices;

            AssignIsOtherFOPRequired(response.ShoppingCart.FormofPaymentDetails, response.Reservation.Prices);
            AssignFormOfPaymentType(response.ShoppingCart.FormofPaymentDetails, response.Reservation.Prices, response.ShoppingCart.FormofPaymentDetails.IsOtherFOPRequired, travelBankDetails.TBApplied, MOBFormofPayment.TB);
            response.ShoppingCart.FormofPaymentDetails.TravelBankDetails = travelBankDetails;

            if (_shoppingCartUtility.IsEnableBundleLiveUpdateChanges(request.Application.Id, request.Application.Version.Major, response.Reservation?.ShopReservationInfo2?.IsDisplayCart == true))
            {
                _shoppingCartUtility.BuildOmniCart(response.ShoppingCart, response.Reservation, request.Application);
            }
            #region OfferCode expansion Changes(This properties are added as part of offercode expansion changes but can be later used in general )
            response.ShoppingCart.FormofPaymentDetails.IsFOPRequired = _shoppingCartUtility.IsFOPRequired(response.ShoppingCart, response.Reservation);
            response.ShoppingCart.FormofPaymentDetails.IsEnableAgreeandPurchaseButton = _shoppingCartUtility.IsEnableAgreeandPurchaseButton(response.ShoppingCart, response.Reservation);
            response.ShoppingCart.FormofPaymentDetails.MaskedPaymentMethod =await  _shoppingCartUtility.AssignMaskedPaymentMethod(response.ShoppingCart,request.Application).ConfigureAwait(false);
            #endregion OfferCode expansion Changes
            response.PkDispenserPublicKey = await _mSCPkDispenserPublicKey.GetCachedOrNewpkDispenserPublicKey(request.Application.Id, request.Application.Version.Major, request.DeviceId, session.SessionId, session.Token, session?.CatalogItems).ConfigureAwait(false);

            await _sessionHelperService.SaveSession(bookingPathReservation, session.SessionId, new List<string> { session.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);
           await _sessionHelperService.SaveSession(response.ShoppingCart, session.SessionId, new List<string> { session.SessionId, response.ShoppingCart.ObjectName }, response.ShoppingCart.ObjectName).ConfigureAwait(false);

            _logger.LogInformation("TravelBankCredit Response {response} sessionID{sessionId}", JsonConvert.SerializeObject(response), session.SessionId);

            return response;
        }

        private async Task<double> GetTravelBankBalance(string sessionId)
        {
            MOBCPTraveler mobCPTraveler = await GetProfileOwnerTravelerCSL(sessionId);
            return mobCPTraveler?.MileagePlus?.TravelBankBalance > 0.00 ? mobCPTraveler.MileagePlus.TravelBankBalance : 0.00;

        }

        private async Task<MOBCPTraveler> GetProfileOwnerTravelerCSL(string sessionID)
        {
            ProfileResponse profilePersist = new ProfileResponse();
            profilePersist = await GetCSLProfileResponseInSession(sessionID);

            if (profilePersist != null && profilePersist.Response != null && profilePersist.Response.Profiles != null)
            {
                foreach (MOBCPTraveler mobCPTraveler in profilePersist.Response.Profiles[0].Travelers)
                {
                    if (mobCPTraveler.IsProfileOwner)
                    {
                        return mobCPTraveler;
                    }
                }
            }

            return null;
        }

        private async Task<ProfileResponse> GetCSLProfileResponseInSession(string sessionId)
        {
            ProfileResponse profile = new ProfileResponse();
            try
            {
                profile = await _sessionHelperService.GetSession<ProfileResponse>(sessionId, profile.ObjectName, new List<string> { sessionId, profile.ObjectName }).ConfigureAwait(false);
            }
            catch (System.Exception)
            {

            }
            return profile;
        }

        private void AssignIsOtherFOPRequired(MOBFormofPaymentDetails formofPaymentDetails, List<MOBSHOPPrice> prices)
        {
            var grandTotalPrice = prices.FirstOrDefault(p => p.DisplayType.ToUpper().Equals("GRAND TOTAL"));
            if (grandTotalPrice != null)
                formofPaymentDetails.IsOtherFOPRequired = (grandTotalPrice.Value > 0);
        }

        private async Task<(MOBFOPResponse MobFopResponse, Reservation BookingPathReservation)> LoadBasicFOPResponse(Session session, Reservation bookingPathReservation)
        {
            var response = new MOBFOPResponse();
            bookingPathReservation = new Reservation();

            bookingPathReservation =await _sessionHelperService.GetSession<Reservation>(session.SessionId, bookingPathReservation.ObjectName, new List<string> { session.SessionId, bookingPathReservation.ObjectName }).ConfigureAwait(false);

            response.Reservation = new MOBSHOPReservation(_configuration, _cachingService);
            response.Reservation = _eTCUtility.MakeReservationFromPersistReservation(response.Reservation, bookingPathReservation, session);

            var persistShoppingCart = new MOBShoppingCart();

            persistShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(session.SessionId, persistShoppingCart.ObjectName, new List<string> { session.SessionId, persistShoppingCart.ObjectName }).ConfigureAwait(false);

            response.ShoppingCart = persistShoppingCart;
            response.Profiles = await _eTCUtility.LoadPersistedProfile(session.SessionId, response.ShoppingCart.Flow);
            if (response?.ShoppingCart?.FormofPaymentDetails == null)
            {
                response.ShoppingCart.FormofPaymentDetails = new MOBFormofPaymentDetails();
            }

            return (response,bookingPathReservation);
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

        private List<MOBMobileCMSContentMessages> GetSDLContentMessages(List<CMSContentMessage> lstMessages, string title)
        {
            List<MOBMobileCMSContentMessages> messages = new List<MOBMobileCMSContentMessages>();
            messages.AddRange(GetSDLMessageFromList(lstMessages, title));

            return messages;
        }

        private List<MOBMobileCMSContentMessages> SwapTitleAndLocation(List<MOBMobileCMSContentMessages> cmsList)
        {
            foreach (var item in cmsList)
            {
                string location = item.LocationCode;
                item.LocationCode = item.Title;
                item.Title = location;
            }

            return cmsList;
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
                _eTCUtility.UpdateCertificateRedeemAmountFromTotalInReserationPrices(grandtotal, ffcPrice.Value, false);
            }

            if (certificateTotalAmount > 0)
            {
                UpdateCertificatePrice(ffcPrice, certificateTotalAmount, fopType, fopDescription, isAddNegative: true);
                _eTCUtility.UpdateCertificateRedeemAmountFromTotalInReserationPrices(grandtotal, certificateTotalAmount);
            }
            else
            {
                prices.Remove(ffcPrice);
            }
        }

        private bool IsUpliftFopAdded(MOBShoppingCart shoppingCart)
        {
            if (shoppingCart?.FormofPaymentDetails != null && shoppingCart.FormofPaymentDetails.FormOfPaymentType.ToUpper() == MOBFormofPayment.Uplift.ToString().ToUpper())
            {
                return true;
            }

            return false;
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
            ffc.FormattedDisplayValue = (isAddNegative ? "-" : "") + (ffc.Value).ToString("C2", CultureInfo.CurrentCulture);
            ffc.DisplayValue = string.Format("{0:#,0.00}", ffc.Value);

            return ffc;
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

        private double GetCloseBookingFee(bool isPost, United.Service.Presentation.InteractionModel.ShoppingCart flightReservationResponseShoppingCart, string flow)
        {
            return isPost ? flightReservationResponseShoppingCart.Items.SelectMany(d => d.Product).Where(d => d.Code == "RBF").FirstOrDefault().Price.Totals.FirstOrDefault().Amount :
                                  flightReservationResponseShoppingCart.Items.SelectMany(d => d.Product).Where(d => d.Code == "RBF").SelectMany(x => x.Price.Totals).FirstOrDefault().Amount;
        }

        private bool IsCheckinFlow(string flowName)
        {
            FlowType flowType;
            if (!Enum.TryParse(flowName, out flowType))
            {
                return false;
            }

            return flowType == FlowType.CHECKIN;
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

        private void UpdatePricesWithPromo(United.Services.FlightShopping.Common.FlightReservation.FlightReservationResponse cslFlightReservationResponse, List<MOBSHOPPrice> peristedPrices, List<MOBProdDetail> persistedSCproducts, List<MOBProdDetail> updatedSCproducts, Reservation bookingPathReservation, Session session)
        {
            UpdateFlightPriceWithPromo(cslFlightReservationResponse, bookingPathReservation.Prices, persistedSCproducts, updatedSCproducts, bookingPathReservation, session);
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
                            price.PromoDetails = promoValue > 0 ? new MOBPromoCode
                            {
                                PriceTypeDescription = _configuration.GetValue<string>("PromoCodeAppliedText"),
                                PromoValue = Math.Round(promoValue, 2, MidpointRounding.AwayFromZero),
                                FormattedPromoDisplayValue = "-" + promoValue.ToString("C2", new CultureInfo("en-us"))
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

        public List<List<MOBSHOPTax>> GetTaxAndFeesAfterPriceChange(List<United.Services.FlightShopping.Common.DisplayCart.DisplayPrice> prices, bool isReshopChange = false, int appId = 0, string appVersion = "", string travelType = null)
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

                if (price.SubItems != null && price.SubItems.Count > 0 && (!isReshopChange || (isReshopChange && price.Type.ToUpper() == "TRAVELERPRICE" && !isTravelerPriceDirty)) && (_configuration.GetValue<bool>("EnableCouponsforBooking") && price.Type.ToUpper() != "NONDISCOUNTPRICE-TRAVELERPRICE")) // Added by Hasnan - # 167553 - 10/04/2017
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

        private void UpdateSeatsPriceWithPromo(United.Services.FlightShopping.Common.FlightReservation.FlightReservationResponse cslFlightReservationResponse, List<MOBSHOPPrice> prices, List<MOBSeatPrice> seatPrices, List<MOBProdDetail> persistedSCproducts, List<MOBProdDetail> updatedSCproducts)
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
                            price.PromoDetails = totalEplusPromoValue > 0 ? new MOBPromoCode
                            {
                                PromoValue = totalEplusPromoValue,
                                FormattedPromoDisplayValue = "-" + totalEplusPromoValue.ToString("C2", new CultureInfo("en-us")),
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
                            price.PromoDetails = totalPreferredSeatPromoValue > 0 ? new MOBPromoCode
                            {
                                PromoValue = totalPreferredSeatPromoValue,
                                FormattedPromoDisplayValue = "-" + totalPreferredSeatPromoValue.ToString("C2", new CultureInfo("en-us")),
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
                            price.PromoDetails = totalAdvanceSeatpromoValue > 0 ? new MOBPromoCode
                            {
                                PromoValue = totalAdvanceSeatpromoValue,
                                FormattedPromoDisplayValue = "-" + totalAdvanceSeatpromoValue.ToString("C2", new CultureInfo("en-us")),
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
            grandTotalPrice.PromoDetails = totalPromoVlaue > 0 ? new MOBPromoCode
            {
                PriceTypeDescription = _configuration.GetValue<string>("PromoSavedText"),
                PromoValue = totalPromoVlaue,
                FormattedPromoDisplayValue = totalPromoVlaue.ToString("C2", new CultureInfo("en-us"))
            } : null;
            #endregion update GrandTotal with new promo value
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

        private bool IncludeFFCResidual(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableFFCResidual") && GeneralHelper.isApplicationVersionGreater(appId, appVersion, "AndroidFFCResidualVersion", "iPhoneFFCResidualVersion", "", "", true, _configuration);
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

        private MOBTPIInfoInBookingPath AssignBookingPathTPIInfo(MOBTPIInfo tripInsuranceInfo, MOBTPIInfoInBookingPath tripInsuranceBookingInfo)
        {
            MOBTPIInfoInBookingPath tPIInfo = new MOBTPIInfoInBookingPath() { };
            tPIInfo.Amount = tripInsuranceInfo.Amount;
            tPIInfo.ButtonTextInProdPage = Convert.ToString(_configuration.GetValue<string>("TPIinfo_BookingPath_PRODBtnText_BeforeRegister") ?? "") + tripInsuranceInfo.DisplayAmount;
            //tPIInfo.ButtonTextInRTIPage = ConfigurationManager.AppSettings["TPIinfo_BookingPath_RTIBtnText_BeforeRegister"] ??  "";
            tPIInfo.Title = tripInsuranceInfo.PageTitle;
            tPIInfo.CoverCostText = _configuration.GetValue<string>("TPIinfo_BookingPath_CoverCostTest") ?? "";
            tPIInfo.CoverCost = Convert.ToString(_configuration.GetValue<string>("TPIinfo_BookingPath_CoverCost") ?? "") + "<b>" + tripInsuranceInfo.CoverCost + "</b>";
            tPIInfo.Img = tripInsuranceInfo.Image;
            tPIInfo.IsRegistered = false;
            tPIInfo.QuoteId = tripInsuranceInfo.ProductId;
            tPIInfo.Tnc = tripInsuranceInfo.Body3;
            tPIInfo.Content = tripInsuranceBookingInfo.Content;
            tPIInfo.Header = tripInsuranceBookingInfo.Header;
            tPIInfo.LegalInformation = tripInsuranceBookingInfo.LegalInformation;
            tPIInfo.LegalInformationText = tripInsuranceInfo.TNC;
            tPIInfo.TncSecondaryFOPPage = tripInsuranceBookingInfo.TncSecondaryFOPPage;
            tPIInfo.DisplayAmount = tripInsuranceInfo.DisplayAmount;
            tPIInfo.ConfirmationMsg = tripInsuranceBookingInfo.ConfirmationMsg;
            // Feature TravelInsuranceOptimization : MOBILE-21191, MOBILE-21193, MOBILE-21195, MOBILE-21197
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

        private async  Task<FlightReservationResponse> RegisterOffers(MOBRegisterOfferRequest request, United.Service.Presentation.ProductResponseModel.ProductOffer productOffer, United.Service.Presentation.ProductResponseModel.ProductOffer productVendorOffer, United.Service.Presentation.ReservationResponseModel.ReservationDetail reservationDetail, Session session)
        {
            return await RegisterOffers(request, productOffer, productVendorOffer, reservationDetail, session, null); ;
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

                //string url = string.Format("{0}{1}", _configuration.GetValue<string>("ServiceEndPointBaseUrl - ShoppingCartService"), "/RegisterOffers");
                string url = "RegisterOffers";

                if (session == null)
                {
                    throw new MOBUnitedException("Could not find your session.");
                }

                response = await _shoppingCartService.RegisterOffers<FlightReservationResponse>(session.Token, url, jsonRequest, request.SessionId);


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

        private bool IsEnableOmniCartReleaseCandidateTwoChanges_Bundles(int applicationId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableOmniCartReleaseCandidateTwoChanges_Bundles") && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, "Android_EnableOmniCartReleaseCandidateTwoChanges_Bundles_AppVersion", "iPhone_EnableOmniCartReleaseCandidateTwoChanges_Bundles_AppVersion");
        }

        public United.Services.FlightShopping.Common.FlightReservation.RegisterOfferRequest GetRegisterOffersRequest
          (MOBRegisterOfferRequest request, MerchandizingOfferDetails merchandizingOfferDetail,
          United.Service.Presentation.ProductResponseModel.ProductOffer Offer,
             United.Service.Presentation.ReservationResponseModel.ReservationDetail reservation,
           System.Collections.ObjectModel.Collection<Characteristic> characteristics = null)
        {
            United.Services.FlightShopping.Common.FlightReservation.RegisterOfferRequest registerOfferRequest = new United.Services.FlightShopping.Common.FlightReservation.RegisterOfferRequest();


            registerOfferRequest.AutoTicket = false;
            registerOfferRequest.CartId = request.CartId;
            registerOfferRequest.CartKey = request.CartKey;
            registerOfferRequest.CountryCode = (request.PointOfSale != null) ? request.PointOfSale : "US";
            registerOfferRequest.Delete = merchandizingOfferDetail.IsOfferRegistered;
            registerOfferRequest.LangCode = (request.LanguageCode != null) ? request.LanguageCode : "en-US";
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

    }
}



