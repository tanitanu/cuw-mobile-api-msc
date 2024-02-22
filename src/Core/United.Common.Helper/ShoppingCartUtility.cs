using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using United.Definition;
using United.Definition.Booking;
using United.Definition.CCE;
using United.Definition.FormofPayment;
using United.Definition.FormofPayment.TravelCredit;
using United.Definition.PreOrderMeals;
using United.Definition.Shopping;
using United.Definition.Shopping.Bundles;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Mobile.Model.Common;
using United.Mobile.Model.MSC;
using United.Persist.Definition.SeatChange;
using United.Persist.Definition.Shopping;
using United.Service.Presentation.CommonModel;
using United.Service.Presentation.CustomerResponseModel;
using United.Service.Presentation.PersonalizationResponseModel;
using United.Service.Presentation.SegmentModel;
using United.Services.FlightShopping.Common.DisplayCart;
using United.Services.FlightShopping.Common.Extensions;
using United.Services.FlightShopping.Common.FlightReservation;
using United.Utility.Helper;
using United.Mobile.Model.MSC.Corporate;
using CacheCountry = United.Mobile.Model.Common.CacheCountry;
using Task = System.Threading.Tasks.Task;
using System.Text.RegularExpressions;
using United.Definition.Shopping.TripInsurance;
using TravelCreditsResponse = United.Services.FlightShopping.Common.TravelCreditsResponse;

namespace United.Common.Helper
{
    public class ShoppingCartUtility : IShoppingCartUtility
    {
        private readonly IConfiguration _configuration;
        private readonly IHeaders _headers;
        private readonly ICacheLog<ShoppingCartUtility> _logger;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly IMSCPageProductInfoHelper _mSCPageProductInfoHelper;
        private readonly ICMSContentService _cMSContentService;
        private readonly ILegalDocumentsForTitlesService _legalDocumentsForTitlesService;
        private readonly ICachingService _cachingService;
        private readonly DynamicOfferDetailResponse _offerResponse;
        private readonly IFeatureSettings _featureSettings;
        private readonly IFeatureToggles _featureToggles;
        private const string BE_BUYOUT_PRODUCT_CODE = "BEB";

        public ShoppingCartUtility(IConfiguration configuration
            , IHeaders headers
            , ICacheLog<ShoppingCartUtility> logger
            , ISessionHelperService sessionHelperService
            , ICMSContentService cMSContentService
            , ILegalDocumentsForTitlesService legalDocumentsForTitlesService
            , IMSCPageProductInfoHelper mSCPageProductInfoHelper
            , ICachingService cachingService
            ,IFeatureSettings featureSettings
            , IFeatureToggles featureToggles)
        {
            _configuration = configuration;
            _headers = headers;
            _logger = logger;
            _sessionHelperService = sessionHelperService;
            _cMSContentService = cMSContentService;
            _legalDocumentsForTitlesService = legalDocumentsForTitlesService;
            _mSCPageProductInfoHelper = mSCPageProductInfoHelper;
            _cachingService = cachingService;
            _featureSettings = featureSettings;
            _featureToggles = featureToggles;
        }


        public async Task<MOBShoppingCart> ReservationToShoppingCart_DataMigration(MOBSHOPReservation reservation, MOBShoppingCart shoppingCart, MOBRequest request, bool isCheckout = false, Session session = null)
        {
            try
            {
                bool isETCCertificatesExistInShoppingCartPersist = (_configuration.GetValue<bool>("MTETCToggle") &&
                                                                    shoppingCart?.FormofPaymentDetails?.TravelCertificate?.Certificates != null &&
                                                                    shoppingCart?.FormofPaymentDetails?.TravelCertificate?.Certificates.Count > 0);
                if (shoppingCart == null)
                    shoppingCart = new MOBShoppingCart();
                var formOfPaymentDetails = new MOBFormofPaymentDetails();
                shoppingCart.CartId = reservation.CartId;
                shoppingCart.PointofSale = reservation.PointOfSale;
                if (_configuration.GetValue<bool>("MTETCToggle"))
                    shoppingCart.IsMultipleTravelerEtcFeatureClientToggleEnabled = reservation.ShopReservationInfo2 != null ? reservation.ShopReservationInfo2.IsMultipleTravelerEtcFeatureClientToggleEnabled : false;
                formOfPaymentDetails.FormOfPaymentType = reservation.FormOfPaymentType.ToString();
                formOfPaymentDetails.PayPal = reservation.PayPal;
                formOfPaymentDetails.PayPalPayor = reservation.PayPalPayor;
                //TODO
                formOfPaymentDetails.MasterPassSessionDetails = reservation.MasterpassSessionDetails;
                formOfPaymentDetails.Masterpass = reservation.Masterpass;
                formOfPaymentDetails.Uplift = reservation.ShopReservationInfo2?.Uplift;
                if (_configuration.GetValue<bool>("EnableExtraSeatsFeature") && reservation?.TravelersCSL != null)
                {
                    foreach (var extraSeatSCTraveler in reservation?.TravelersCSL?.Where(a => a.IsExtraSeat == true))
                    {
                        extraSeatSCTraveler.Message = ""; // Remove warning messages "More info  needed" if it is extra seat
                    }
                }
                shoppingCart.SCTravelers = (reservation.TravelersCSL != null && reservation.TravelersCSL?.Count() > 0) ? reservation.TravelersCSL : null;
                if (shoppingCart.SCTravelers != null && shoppingCart.SCTravelers.Any())
                {
                    shoppingCart.SCTravelers[0].SelectedSpecialNeeds = (reservation.TravelersCSL != null && reservation.TravelersCSL?.Count() > 0) ? reservation.TravelersCSL[0].SelectedSpecialNeeds : null;
                    shoppingCart.SCTravelers[0].SelectedSpecialNeedMessages = (reservation.TravelersCSL != null && reservation.TravelersCSL?.Count() > 0) ? reservation.TravelersCSL[0].SelectedSpecialNeedMessages : null;
                }
                if (shoppingCart.FormofPaymentDetails != null && shoppingCart.FormofPaymentDetails.SecondaryCreditCard != null)
                {
                    formOfPaymentDetails.CreditCard = shoppingCart.FormofPaymentDetails.CreditCard;
                    formOfPaymentDetails.SecondaryCreditCard = shoppingCart.FormofPaymentDetails.SecondaryCreditCard;
                }
                else
                {
                    formOfPaymentDetails.CreditCard = reservation.CreditCards?.Count() > 0 ? reservation.CreditCards[0] : null;
                }

                // Added as part of Money + Miles changes: MOBILE-14736 // MM is only for Booking
                if (IncludeMoneyPlusMiles(request.Application.Id, request.Application.Version.Major) && shoppingCart.FormofPaymentDetails?.MoneyPlusMilesCredit != null)
                {
                    formOfPaymentDetails.MoneyPlusMilesCredit = shoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit;
                }
               
                bool isTravelCredit = IncludeTravelCredit(request.Application.Id, request.Application.Version.Major);
                if (isTravelCredit)
                {
                    formOfPaymentDetails.TravelCreditDetails = shoppingCart.FormofPaymentDetails?.TravelCreditDetails;
                }

                if (IncludeTravelBankFOP(request.Application.Id, request.Application.Version.Major))
                {
                    formOfPaymentDetails.TravelBankDetails = shoppingCart.FormofPaymentDetails?.TravelBankDetails;
                    if (formOfPaymentDetails.TravelBankDetails?.TBApplied > 0)
                    {
                        AssignIsOtherFOPRequired(formOfPaymentDetails, reservation.Prices);
                        AssignFormOfPaymentType(formOfPaymentDetails, reservation.Prices, formOfPaymentDetails.IsOtherFOPRequired, formOfPaymentDetails.TravelBankDetails.TBApplied, MOBFormofPayment.TB);
                        shoppingCart.FormofPaymentDetails.IsOtherFOPRequired = formOfPaymentDetails.IsOtherFOPRequired;
                        shoppingCart.FormofPaymentDetails.FormOfPaymentType = formOfPaymentDetails.FormOfPaymentType;
                    }
                }
                if (ConfigUtility.EnableMFOP(request.Application.Id, request.Application.Version.Major) && shoppingCart.Flow == FlowType.BOOKING.ToString())
                {
                    formOfPaymentDetails.TravelCertificate = shoppingCart?.FormofPaymentDetails?.TravelCertificate;
                }

                await AssignFFCValues(reservation.SessionId, shoppingCart, request, formOfPaymentDetails, reservation, isCheckout);
                if (shoppingCart.FormofPaymentDetails?.MilesFOP != null)
                {
                    formOfPaymentDetails.MilesFOP = shoppingCart.FormofPaymentDetails.MilesFOP;
                    if (shoppingCart.FormofPaymentDetails.FormOfPaymentType != MOBFormofPayment.MilesFOP.ToString() && shoppingCart.DisplayMessage != null)
                    {
                        shoppingCart.DisplayMessage = shoppingCart.DisplayMessage.Where(x => x.Text1 != "Not enough miles").ToList();
                    }
                }
                shoppingCart.FormofPaymentDetails = formOfPaymentDetails;
                shoppingCart.FormofPaymentDetails.Phone = reservation.ReservationPhone;
                shoppingCart.FormofPaymentDetails.Email = reservation.ReservationEmail;
                shoppingCart.FormofPaymentDetails.EmailAddress = reservation.ReservationEmail != null ? reservation.ReservationEmail.EmailAddress : null;
                shoppingCart.FormofPaymentDetails.BillingAddress = reservation.CreditCardsAddress?.Count() > 0 ? reservation.CreditCardsAddress[0] : null;

                if (reservation.IsReshopChange)
                {

                    double changeFee = 0.0;
                    double grandTotal = 0.0;
                    if (reservation.Prices.Exists(price => price.DisplayType.ToUpper().Trim() == "CHANGEFEE"))
                        changeFee = reservation.Prices.First(price => price.DisplayType.ToUpper().Trim() == "CHANGEFEE").Value;

                    if (reservation.Prices.Exists(price => price.DisplayType.ToUpper().Trim() == "GRAND TOTAL"))
                        grandTotal = reservation.Prices.First(price => price.DisplayType.ToUpper().Trim() == "GRAND TOTAL").Value;

                    if (!reservation.AwardTravel)
                    {
                        if (grandTotal == 0.0)
                        {
                            grandTotal = reservation.Prices.First(price => price.DisplayType.ToUpper().Trim() == "TOTAL").Value;
                        }
                    }
                    string totalDue = (grandTotal > changeFee ? (grandTotal - changeFee) : 0).ToString();
                    shoppingCart.TotalPrice = String.Format("{0:0.00}", totalDue);
                    shoppingCart.DisplayTotalPrice = string.Format("{0:c}", totalDue);
                }

                //Check and add TravelBank FOP TODO : Condition to populate.
                if (IsETCCombinabilityEnabled(request.Application.Id, request.Application.Version.Major) && shoppingCart.Flow == FlowType.BOOKING.ToString())
                {
                    await LoadandAddTravelCertificate(shoppingCart, reservation, isETCCertificatesExistInShoppingCartPersist, request.Application, isCheckout);
                }
                else if (_configuration.GetValue<bool>("ETCToggle") && shoppingCart.Flow == FlowType.BOOKING.ToString())
                {
                    await LoadandAddTravelCertificate(shoppingCart, reservation.SessionId, reservation.Prices, isETCCertificatesExistInShoppingCartPersist, request.Application, isCheckout);
                }

                //Update Email messages 
                if (ConfigUtility.EnableMFOP(request.Application.Id, request.Application.Version.Major) && shoppingCart.Flow == FlowType.BOOKING.ToString())
                {
                    await AssignETCFFCvalues(reservation.SessionId, shoppingCart, request, formOfPaymentDetails, reservation, isCheckout).ConfigureAwait(false);
                }

                if (_configuration.GetValue<bool>("EnableETCBalanceAttentionMessageOnRTI") && !IsETCCombinabilityEnabled(request.Application.Id, request.Application.Version.Major))
                {
                    await AssignBalanceAttentionInfoWarningMessage(reservation.ShopReservationInfo2, shoppingCart.FormofPaymentDetails?.TravelCertificate);
                }
                //Updating FFC/ETC value twice for MFOP ignoring below Update
                if (isTravelCredit && !ConfigUtility.EnableMFOP(request.Application.Id, request.Application.Version.Major))
                {
                    await UpdateTCPriceAndFOPType(reservation.Prices, shoppingCart.FormofPaymentDetails, request.Application, shoppingCart.Products, shoppingCart.SCTravelers, isCheckout);
                }
                if (_configuration.GetValue<bool>("EnableCouponsforBooking") && shoppingCart.Flow == FlowType.BOOKING.ToString())
                {
                    await LoadandAddPromoCode(shoppingCart, reservation.SessionId, request.Application);
                }
                //reservation.CartId = null;
                reservation.PointOfSale = null;
                reservation.PayPal = null;
                reservation.PayPalPayor = null;
                reservation.MasterpassSessionDetails = null;
                reservation.Masterpass = null;
                reservation.TravelersCSL = null;
                reservation.CreditCards2 = null;
                reservation.ReservationPhone2 = null;
                reservation.ReservationEmail2 = null;
                reservation.CreditCardsAddress = null;
                reservation.FOPOptions = null;

                if (_configuration.GetValue<bool>("EnableSelectDifferentFOPAtRTI"))
                {
                    if (!reservation.IsReshopChange)
                    {
                        //If ETC, ghost card, no saved cc presents and no due in reshop disable this button.
                        if (reservation.ShopReservationInfo2 != null && shoppingCart.FormofPaymentDetails != null)
                        {
                            if (((shoppingCart.FormofPaymentDetails.CreditCard != null && (reservation.ShopReservationInfo == null || !reservation.ShopReservationInfo.CanHideSelectFOPOptionsAndAddCreditCard)) ||
                                                        shoppingCart.FormofPaymentDetails.Masterpass != null || shoppingCart.FormofPaymentDetails.PayPal != null || shoppingCart.FormofPaymentDetails.Uplift != null ||
                                                      (!string.IsNullOrEmpty(shoppingCart.FormofPaymentDetails.FormOfPaymentType) && shoppingCart.FormofPaymentDetails.FormOfPaymentType.ToUpper().Equals("APPLEPAY"))) && (shoppingCart.FormofPaymentDetails.TravelCertificate == null
                                                      || (shoppingCart.FormofPaymentDetails?.TravelCertificate?.Certificates == null || shoppingCart.FormofPaymentDetails?.TravelCertificate?.Certificates?.Count == 0)
                                                      ))
                            {
                                reservation.ShopReservationInfo2.ShowSelectDifferentFOPAtRTI = true;
                            }
                            else
                            {
                                reservation.ShopReservationInfo2.ShowSelectDifferentFOPAtRTI = false;
                            }

                            //if (reservation.IsReshopChange)
                            //{
                            //    if (Double.TryParse(shoppingCart.TotalPrice, out double p))
                            //    {
                            //        if (p > 0 && (shoppingCart.FormofPaymentDetails.CreditCard != null ||
                            //                           (!string.IsNullOrEmpty(shoppingCart.FormofPaymentDetails.FormOfPaymentType) && shoppingCart.FormofPaymentDetails.FormOfPaymentType.ToUpper().Equals("APPLEPAY"))))
                            //        {
                            //            reservation.ShopReservationInfo2.ShowSelectDifferentFOPAtRTI = true;
                            //        }
                            //        else
                            //            reservation.ShopReservationInfo2.ShowSelectDifferentFOPAtRTI = false;
                            //    }
                            //}
                        }
                    }
                }
                AssignNullToETCAndFFCCertificates(shoppingCart.FormofPaymentDetails, request);
                if (IsEnableOmniCartMVP2Changes(request.Application.Id, request.Application.Version.Major, reservation?.ShopReservationInfo2?.IsDisplayCart == true)
                    && shoppingCart.Flow == FlowType.BOOKING.ToString())
                {
                    BuildOmniCart(shoppingCart, reservation, request.Application);
                }
                //if (IsBuyMilesFeatureEnabled(request.Application.Id, request.Application.Version.Major))
                //    UpdateGrandTotal(reservation);

                if (IsExtraSeatFeatureEnabled(request.Application.Id, request.Application.Version.Major, session?.CatalogItems))
                {
                    reservation.ShopReservationInfo2.AllowExtraSeatSelection = IsExtraSeatExcluded(session, reservation?.Trips, reservation?.ShopReservationInfo2?.DisplayTravelTypes, shoppingCart, reservation?.ShopReservationInfo2?.IsUnfinihedBookingPath);
                }
                else
                {
                    if (reservation?.ShopReservationInfo2 != null)
                        reservation.ShopReservationInfo2.AllowExtraSeatSelection = false;
                }
                shoppingCart.FormofPaymentDetails.IsFOPRequired = IsFOPRequired(shoppingCart, reservation);
                shoppingCart.FormofPaymentDetails.IsEnableAgreeandPurchaseButton = IsEnableAgreeandPurchaseButton(shoppingCart, reservation);
                shoppingCart.FormofPaymentDetails.MaskedPaymentMethod = await AssignMaskedPaymentMethod(shoppingCart, request.Application).ConfigureAwait(false); ;

                if (await _featureSettings.GetFeatureSettingValue("EnableChaseCreditStatementValueFix").ConfigureAwait(false) && EnableChaseOfferRTI(request.Application.Id, request.Application.Version.Major))
                {
                    Reservation bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(reservation.SessionId, new Reservation().ObjectName, new List<string> { reservation.SessionId, new Reservation().ObjectName }).ConfigureAwait(false);
                    UpdateChaseCreditStatement(bookingPathReservation);
                    if (reservation?.ShopReservationInfo2 != null)
                    {
                        reservation.ShopReservationInfo2.ChaseCreditStatement = bookingPathReservation?.ShopReservationInfo2?.ChaseCreditStatement;
                    }
                    await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, reservation.SessionId, new List<string> { reservation.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);
                }

                shoppingCart.IsCorporateBusinessNamePersonalized = reservation.ShopReservationInfo2.IsCorporateBusinessNamePersonalized;
            }
            catch (Exception ex)
            {
                throw new MOBUnitedException(ex.Message.ToString());
            }
            return shoppingCart;

        }


        public bool IsExtraSeatExcluded(Session session, List<MOBSHOPTrip> trips, List<MOBDisplayTravelType> displayTravelTypes, MOBShoppingCart shoppingCart = null, bool? isUnfinishedBooking = false)
        {
            return (displayTravelTypes?.Count() > 1) && !session.IsCorporateBooking
                && (isUnfinishedBooking == true || session.TravelType == TravelType.RA.ToString() || session.TravelType == TravelType.TPBooking.ToString())
                && IsUAFlight(trips)
                && IsExcludedOperatingCarrier(trips);
                
        }

        private bool IsUAFlight(List<MOBSHOPTrip> trips)
        {
            var unitedCarriers = _configuration.GetValue<string>("UnitedCarriers");
            return (trips?.SelectMany(a => a?.FlattenedFlights)?.SelectMany(b => b?.Flights)?
             .Any(c => !unitedCarriers.Contains(c?.OperatingCarrier)) == true) ? false : true;
        }

        private bool IsExcludedOperatingCarrier(List<MOBSHOPTrip> trips)
        {
            var execludedUnitedCarriers = _configuration.GetValue<string>("ExcludedOperatingCarriersForExtraSeat");
            return (trips?.SelectMany(a => a?.FlattenedFlights)?.SelectMany(b => b?.Flights)?
             .Any(c => execludedUnitedCarriers.Contains(c?.OperatingCarrier)) == true) ? false : true;
        }

        public bool IsExtraSeatFeatureEnabled(int appId, string appVersion, List<MOBItem> catalogItems)
        {
            if (catalogItems != null && catalogItems.Count > 0 &&
                           catalogItems.FirstOrDefault(a => a.Id == ((int)IOSCatalogEnum.EnableExtraSeatFeature).ToString() || a.Id == ((int)AndroidCatalogEnum.EnableExtraSeatFeature).ToString())?.CurrentValue == "1")
                return IsExtraSeatFeatureEnabled(appId, appVersion);
            else return false;
        }

        public bool IsExtraSeatFeatureEnabled(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableExtraSeatsFeature")
                        && GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "Android_EnableExtraSeatsFeature_AppVersion", "IPhone_EnableExtraSeatsFeature_AppVersion", "", "", true, _configuration);
        }

        private void UpdateGrandTotal(MOBSHOPReservation reservation)
        {
            var grandTotalIndex = reservation.Prices.FindIndex(a => a.PriceType == "GRAND TOTAL");
            if (grandTotalIndex >= 0)
            {
                double extraMilePurchaseAmount = (reservation?.Prices?.Where(a => a.DisplayType == "MPF")?.FirstOrDefault()?.Value != null) ?
                                         Convert.ToDouble(reservation?.Prices?.Where(a => a.DisplayType == "MPF")?.FirstOrDefault()?.Value) : 0;
                if (extraMilePurchaseAmount > 0)
                {
                    reservation.Prices[grandTotalIndex].Value += extraMilePurchaseAmount;
                    CultureInfo ci = null;
                    ci = TopHelper.GetCultureInfo(reservation?.Prices[grandTotalIndex].CurrencyCode);
                    reservation.Prices[grandTotalIndex].DisplayValue = reservation.Prices[grandTotalIndex].Value.ToString();
                    reservation.Prices[grandTotalIndex].FormattedDisplayValue = formatAmountForDisplay(reservation?.Prices[grandTotalIndex].Value.ToString(), ci, false);
                }
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

                    newAmt = TopHelper.GetCurrencySymbol(ci, amount, roundup);
                    break;
            }

            return isAward ? "+ " + newAmt : newAmt;
        }

        private bool IsBuyMilesFeatureEnabled(int appId, string version)
        {
            if (!_configuration.GetValue<bool>("EnableBuyMilesFeature")) return false;
            return GeneralHelper.IsApplicationVersionGreaterorEqual(appId, version, _configuration.GetValue<string>("Android_BuyMilesFeatureSupported_AppVersion"), _configuration.GetValue<string>("IPhone_BuyMilesFeatureSupported_AppVersion"));
        }

        private int MinimumPriceForUplift
        {
            get
            {
                var minimumAmountForUplift = _configuration.GetValue<string>("MinimumPriceForUplift");
                if (string.IsNullOrEmpty(minimumAmountForUplift))
                    return 300;

                int.TryParse(minimumAmountForUplift, out int upliftMinAmount);
                return upliftMinAmount;
            }
        }

        public MOBSection GetAdditionalMileDetail(MOBSHOPReservation reservation)
        {
            var additionalMilesPrice = reservation?.Prices?.FirstOrDefault(price => string.Equals("MPF", price?.DisplayType, StringComparison.OrdinalIgnoreCase));
            if (additionalMilesPrice != null)
            {
                var returnObject = new MOBSection();
                returnObject.Text1 = !string.IsNullOrEmpty(_configuration.GetValue<string>("AdditionalMilesLabelText")) ? _configuration.GetValue<string>("AdditionalMilesLabelText") : "Additional Miles";
                returnObject.Text2 = additionalMilesPrice.PriceTypeDescription?.Replace("Additional", String.Empty).Trim();
                returnObject.Text3 = additionalMilesPrice.FormattedDisplayValue;

                return returnObject;
            }
            return null;

        }

        public void BuildOmniCart(MOBShoppingCart shoppingCart, MOBSHOPReservation reservation, MOBApplication application)
        {
            if (shoppingCart.OmniCart == null)
            {
                shoppingCart.OmniCart = new MOBCart();
            }
            shoppingCart.OmniCart.CartItemsCount = GetCartItemsCount(shoppingCart);
            shoppingCart.OmniCart.TotalPrice = GetTotalPrice(shoppingCart?.Products, reservation);
            shoppingCart.OmniCart.PayLaterPrice = GetPayLaterAmount(shoppingCart?.Products, reservation);
            if (_configuration.GetValue<bool>("EnableShoppingCartPhase2Changes"))
            {
                shoppingCart.OmniCart.CostBreakdownFareHeader = GetCostBreakdownFareHeader(reservation?.ShopReservationInfo2?.TravelType, shoppingCart);
            }
            if (_configuration.GetValue<bool>("EnableLivecartForAwardTravel") && reservation.AwardTravel)
            {
                shoppingCart.OmniCart.AdditionalMileDetail = GetAdditionalMileDetail(reservation);
            }
            shoppingCart.OmniCart.FOPDetails = GetFOPDetails(reservation, application);

            if (reservation != null && reservation.ShopReservationInfo2 != null && !string.IsNullOrEmpty(reservation.ShopReservationInfo2.CorporateDisclaimerText))
            {
                shoppingCart.OmniCart.CorporateDisclaimerText = reservation.ShopReservationInfo2.CorporateDisclaimerText;
            }
            AssignUpliftText(shoppingCart, reservation);                //Assign message text and link text to the Uplift
            if (_configuration.GetValue<bool>("EnableFSRMoneyPlusMilesFeature"))
            {
                if (reservation.Prices.FirstOrDefault(p => p.DisplayType.Equals("MONEYPLUSMILES", StringComparison.OrdinalIgnoreCase)) != null && shoppingCart?.OmniCart?.TotalPrice?.CurrentValue != null)
                    shoppingCart.OmniCart.TotalPrice.CurrentValue += " " + reservation.Prices.FirstOrDefault(p => p.DisplayType.Equals("MONEYPLUSMILES", StringComparison.OrdinalIgnoreCase)).FormattedDisplayValue;
            }
            if (_configuration.GetValue<bool>("EnableFSRETCCreditsFeature") && reservation.Prices.Any(p => p.DisplayType.Equals("TravelCredits", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(p.DisplayValue)) == true && shoppingCart?.OmniCart?.TotalPrice?.CurrentValue != null)
                shoppingCart.OmniCart.TotalPrice.CurrentValue += " " + reservation.Prices.FirstOrDefault(p => p.DisplayType.Equals("TRAVELCREDITS", StringComparison.OrdinalIgnoreCase)).FormattedDisplayValue;
        }

        public async Task<MOBShoppingCart> BuildShoppingCart(MOBRequest request, FlightReservationResponse flightReservationResponse, string flow, string cartId, string sessionId, Session session)
        {
            MOBShoppingCart persistShoppingCart = new MOBShoppingCart();
            persistShoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(sessionId, persistShoppingCart.ObjectName, new List<string> { sessionId, persistShoppingCart.ObjectName }).ConfigureAwait(false);
            persistShoppingCart.Products = await ConfirmationPageProductInfo(flightReservationResponse, false, false, flow, request.Application, null, false, false, null, null, sessionId: sessionId);
            persistShoppingCart.CartId = cartId;
            double price = GetGrandTotalPriceForShoppingCart(false, flightReservationResponse, false, flow);
            persistShoppingCart.TotalPrice = String.Format("{0:0.00}", price);
            persistShoppingCart.DisplayTotalPrice = Decimal.Parse(price.ToString()).ToString("c", new CultureInfo("en-us"));
            persistShoppingCart.TermsAndConditions = await GetProductBasedTermAndConditions(null, flightReservationResponse, false, sessionId, flow);
            persistShoppingCart.PaymentTarget = (flow == FlowType.BOOKING.ToString()) ? GetBookingPaymentTargetForRegisterFop(flightReservationResponse) : GetPaymentTargetForRegisterFop(flightReservationResponse.DisplayCart.TravelOptions);
            persistShoppingCart.TravelPolicyWarningAlert = await BuildCorporateTravelPolicyWarningAlert(request, session, flightReservationResponse, persistShoppingCart.IsCorporateBusinessNamePersonalized);
            if(await IsEnableETCCreditsInBookingFeature(session?.CatalogItems))
            {
                ApplyETCCreditsOnRTIAction(persistShoppingCart, flightReservationResponse.DisplayCart);
            }
            await _sessionHelperService.SaveSession<MOBShoppingCart>(persistShoppingCart, sessionId, new List<string> { sessionId, persistShoppingCart.ObjectName }, persistShoppingCart.ObjectName).ConfigureAwait(false);

            return persistShoppingCart;
        }
        public async Task<FlightReservationResponse> BuildShoppingCartDisplayCart(TravelCreditsResponse travelCreditsResponse, Session session)
        {
            FlightReservationResponse loadFlightReservationResponse = await _sessionHelperService.GetSession<FlightReservationResponse>(session.SessionId, new FlightReservationResponse().GetType().FullName, new List<string> { session.SessionId, new FlightReservationResponse().GetType().FullName }).ConfigureAwait(false);
            if (loadFlightReservationResponse != null)
            {
                loadFlightReservationResponse.DisplayCart = travelCreditsResponse.DisplayCart;
            }                  
            return loadFlightReservationResponse;
        }
        public string GetPaymentTargetForRegisterFop(TravelOptionsCollection travelOptions, bool isCompleteFarelockPurchase = false)
        {
            if (string.IsNullOrEmpty(_configuration.GetValue<string>("EnablePCUSelectedSeatPurchaseViewRes")))
                return string.Empty;

            if (isCompleteFarelockPurchase)
                return "RES";

            if (travelOptions == null || !travelOptions.Any())
                return string.Empty;

            return string.Join(",", travelOptions.Select(x => x.Type == "SEATASSIGNMENTS" ? x.Type : x.Key).Distinct());
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

        public async Task<List<MOBMobileCMSContentMessages>> GetProductBasedTermAndConditions(United.Service.Presentation.ProductResponseModel.ProductOffer productVendorOffer, FlightReservationResponse flightReservationResponse, bool isPost, string sessionId, string flow, bool isGetCartInfo = false)
        {
            List<MOBMobileCMSContentMessages> tNClist = new List<MOBMobileCMSContentMessages>();
            MOBMobileCMSContentMessages tNC = null;
            List<MOBTypeOption> typeOption = null;

            var productCodes = isPost ? flightReservationResponse.CheckoutResponse.ShoppingCart.Items.Where(x => x.Product.FirstOrDefault().Code != "RES").Select(x => x.Product.FirstOrDefault().Code).ToList() :
                                       flightReservationResponse.ShoppingCart.Items.Where(x => x.Product.FirstOrDefault().Code != "RES").Select(x => x.Product.FirstOrDefault().Code).ToList();

            if (productCodes == null || !productCodes.Any())
                return null;

            if (ConfigUtility.IsViewResFlowCheckOut(flow))
            {
                productCodes = OrderPCUTnC(productCodes);
            }
            else if (isPost == true)
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
                            tNC.ContentShort = HttpUtility.HtmlDecode(_configuration.GetValue<string>("PaymentTnCMessage").ToString());
                            tNC.ContentFull = HttpUtility.HtmlDecode(tncPCU[0].ContentFull);
                            tNC.HeadLine = tncPCU[0].Title;
                            tNClist.Add(tNC);
                            break;

                        case "BAG":
                            var tNCs = await GetBagsTermsAndConditions(sessionId, false);
                            if (tNCs != null && tNCs.Any())
                                tNClist.AddRange(tNCs);
                            break;

                        case "PAS":
                            tNC = new MOBMobileCMSContentMessages();
                            //  MerchandizingServices merchandizingServices = new MerchandizingServices();
                            typeOption = new List<MOBTypeOption>();
                            typeOption = GetPATermsAndConditionsList();
                            tNC.Title = "Terms and conditions";
                            tNC.ContentShort = HttpUtility.HtmlDecode(_configuration.GetValue<string>("PaymentTnCMessage"));
                            tNC.ContentFull = HttpUtility.HtmlDecode(string.Join("<br><br>", typeOption.Select(x => x.Value)));
                            tNC.HeadLine = "Premier Access";
                            tNClist.Add(tNC);
                            break;

                        case "PBS":
                            tNC = new MOBMobileCMSContentMessages();
                            typeOption = new List<MOBTypeOption>();
                            typeOption = GetPBContentList("PriorityBoardingTermsAndConditionsList");

                            tNC.Title = "Terms and conditions";
                            tNC.ContentShort = HttpUtility.HtmlDecode(_configuration.GetValue<string>("PaymentTnCMessage"));
                            tNC.ContentFull = HttpUtility.HtmlDecode("<ul><li>" + string.Join("<br></li><li>", typeOption.Select(x => x.Value)) + "</li></ul>");
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
                            tNC.ContentShort = HttpUtility.HtmlDecode(_configuration.GetValue<string>("PaymentTnCMessage"));
                            tNC.ContentFull = HttpUtility.HtmlDecode(tncTPI);
                            tNC.HeadLine = "Terms and conditions";
                            tNClist.Add(tNC);
                            break;
                        case "AAC":
                            var acceleratorTnCs = await GetTermsAndConditions(flightReservationResponse.DisplayCart.TravelOptions.Any(d => d.Key == "PAC"), flow);
                            if (acceleratorTnCs != null && acceleratorTnCs.Any())
                            {
                                tNClist.AddRange(acceleratorTnCs);
                            }
                            break;
                        case "POM":
                            break;
                        case "SEATASSIGNMENTS":
                            if (!ConfigUtility.IsViewResFlowCheckOut(flow) && !isGetCartInfo)
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
                                    ContentShort = HttpUtility.HtmlDecode(_configuration.GetValue<string>("PaymentTnCMessage")),
                                    ContentFull = HttpUtility.HtmlDecode(string.Join("<br>", seatsTnCs.Select(a => a.CurrentValue))),
                                    HeadLine = seatsTnCs[0].Id
                                };
                                tNClist.Add(tNC);
                            }
                            break;
                        case "BEB":
                            if (ConfigUtility.IsViewResFlowCheckOut(flow))
                            {
                                if (!_configuration.GetValue<bool>("DisableBEBTnCFixACM2741"))
                                {
                                    bool isEnableIBEBuyOut = await IsEnableIBEBuyOutViewRes().ConfigureAwait(false);
                                    tNC = new BasicEconomyBuyOut(sessionId, _configuration, _sessionHelperService, isEnableIBEBuyOut).GetTermsAndConditionsForBEB();
                                }
                                else
                                    tNC = GetTermsAndConditionsForBEB();
                                
                                if (tNC != null)
                                {
                                    tNClist.Add(tNC);
                                }
                            }
                            break;
                        case "PET":
                            if (!isGetCartInfo)
                                break;
                            tNC = new MOBMobileCMSContentMessages();
                            List<MOBMobileCMSContentMessages> tncPET = await GetTermsAndConditions("PETINCABIN_TNC");
                            tNC.Title = "Terms and conditions";
                            tNC.ContentShort = HttpUtility.HtmlDecode(_configuration.GetValue<string>("PaymentTnCMessage"));
                            tNC.ContentFull = HttpUtility.HtmlDecode(tncPET[0].ContentFull);
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
                            tNC.ContentShort = HttpUtility.HtmlDecode(_configuration.GetValue<string>("PaymentTnCMessage"));
                            tNC.ContentFull = HttpUtility.HtmlDecode("<ul><li>" + string.Join("<br></li><li>", typeOption.Select(x => x.Value)) + "</li></ul>");
                            tNC.HeadLine = "United Club Pass";
                            tNClist.Add(tNC);
                            break;
                        case "SFC":
                            if (ConfigUtility.IsViewResFlowCheckOut(flow))
                            {
                                List<CMSContentMessage> lstMessages = await GetSDLContentByGroupName(new MOBRequest { TransactionId = sessionId }, sessionId, "", _configuration.GetValue<string>("CMSContentMessages_GroupName_MANAGERESOffers_Messages"), "ManageReservation_Offers_CMSContentMessagesCached_StaticGUID");
                                var safTC = GetSDLMessageFromList(lstMessages, "MOB_SAF_TermsandConditions");
                                if (safTC != null && safTC.Count > 0)
                                {
                                    tNC = new MOBMobileCMSContentMessages();
                                    tNC.Title = "Terms and conditions";
                                    tNC.ContentShort = safTC.First().ContentShort;
                                    tNC.ContentFull = safTC.First().ContentFull;
                                    tNC.HeadLine = safTC.First().HeadLine;
                                    tNClist.Add(tNC);
                                }
                            }                                                    
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
                            tNC.ContentShort = HttpUtility.HtmlDecode(_configuration.GetValue<string>("TPIPurchaseResposne-ConfirmationResponseEmailMessage")); // + ((flightReservationResponse.Reservation.EmailAddress.Count() > 0) ? flightReservationResponse.Reservation.EmailAddress.Where(x => x.Address != null).Select(x => x.Address).FirstOrDefault().ToString() : null) ?? "";
                            tNC.ContentFull = HttpUtility.HtmlDecode(tncTPI);
                            tNClist.Add(tNC);
                            break;
                    }
                }
            }

            if (ConfigUtility.IsViewResFlowPaymentSvcEnabled(flow) && IsBundleProductSelected(flightReservationResponse, isPost))
            {
                BundleDetailsPersist bundleDetailsPersist = new BundleDetailsPersist();
                bundleDetailsPersist = await _sessionHelperService.GetSession<BundleDetailsPersist>(sessionId, ObjectNames.BundleDetailsPersist, new List<string> { sessionId, ObjectNames.BundleDetailsPersist }).ConfigureAwait(false);
                if (bundleDetailsPersist != null)
                {
                    tNC = new MOBMobileCMSContentMessages
                    {
                        Title = bundleDetailsPersist.TermsAndCondition?.Title,
                        ContentShort = _configuration.GetValue<string>("PaymentTnCMessage"),
                        ContentFull = bundleDetailsPersist.TermsAndCondition?.ContentFull
                    };
                    tNClist.Add(tNC);
                }
            }

            return tNClist;
        }

        public async Task<bool> IsEnableIBEBuyOutViewRes(int applicationId, string appVersion)
        {

            return await IsEnableIBEBuyOutViewRes().ConfigureAwait(false) && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("IsEnableIBEBuyOutViewRes_AppVersion_Android"), _configuration.GetValue<string>("IsEnableIBEBuyOutViewRes_AppVersion_Iphone"));

        }
        public async Task<bool> IsEnableIBEBuyOutViewRes()
        {
            return await _featureSettings.GetFeatureSettingValue("IsEnableIBEBuyOutViewRes").ConfigureAwait(false);
        }

        public async Task<bool> IsExtraSeatCheckEnableForUpliftMR()
        {
            return await _featureSettings.GetFeatureSettingValue("IsExtraSeatCheckEnableForUpliftMR").ConfigureAwait(false);
        }

        public bool IsBundleProductSelected(United.Services.FlightShopping.Common.FlightReservation.FlightReservationResponse flightReservationResponse, bool isPost)
        {
            if (isPost)
                return flightReservationResponse?.CheckoutResponse?.ShoppingCart?.Items?.Where(x => x.Product?.FirstOrDefault()?.Code != "RES")?.Any(x => x.Product?.Any(p => p?.SubProducts?.Any(sp => sp?.GroupCode == "BE") ?? false) ?? false) ?? false;
            else
                return flightReservationResponse?.ShoppingCart?.Items?.Where(x => x.Product?.FirstOrDefault()?.Code != "RES")?.Any(x => x.Product?.Any(p => p?.SubProducts?.Any(sp => sp?.GroupCode == "BE") ?? false) ?? false) ?? false;
            //return false;
        }
        public MOBMobileCMSContentMessages GetTermsAndConditionsForBEB()
        {
            if (_offerResponse?.ResponseData == null)
                return null;

            SDLContentResponseData sdlData = _offerResponse.ResponseData.ToObject<SDLContentResponseData>();
            var termsAndConditions = sdlData?.Body
                                            ?.FirstOrDefault(b => b?.name?.Equals(BE_BUYOUT_PRODUCT_CODE, StringComparison.OrdinalIgnoreCase) ?? false)
                                            ?.content
                                            ?.FirstOrDefault(c => c?.name?.Equals("beb-Terms-And-Conditions", StringComparison.OrdinalIgnoreCase) ?? false)
                                            ?.content;

            if (termsAndConditions == null || string.IsNullOrEmpty(termsAndConditions?.body) || string.IsNullOrEmpty(termsAndConditions?.title) || string.IsNullOrEmpty(termsAndConditions?.subtitle))
                return null;

            return new MOBMobileCMSContentMessages
            {
                Title = $"{termsAndConditions?.title}",
                ContentShort = _configuration.GetValue<string>("PaymentTnCMessage"),
                HeadLine = $"{termsAndConditions?.subtitle}",
                ContentFull = $"{termsAndConditions?.body}"
            };
        }

        public List<string> OrderPCUTnC(List<string> productCodes)
        {
            if (productCodes == null || !productCodes.Any())
                return productCodes;

            return productCodes.OrderBy(p => GetProductOrderTnC()[GetProductTnCtoOrder(p)]).ToList();
        }

        private string GetProductTnCtoOrder(string productCode)
        {
            productCode = string.IsNullOrEmpty(productCode) ? string.Empty : productCode.ToUpper().Trim();

            if (productCode == "SEATASSIGNMENTS" || productCode == "PCU")
                return productCode;

            return string.Empty;
        }

        /// <summary>
        /// specify order of products
        /// </summary>
        private Dictionary<string, int> GetProductOrderTnC()
        {
            return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) {
                { "SEATASSIGNMENTS", 0 },
                { "PCU", 1 },
                { string.Empty, 2 } };
        }
        public List<MOBTypeOption> GetPBContentList(string configValue)
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

        private async Task<List<MOBMobileCMSContentMessages>> GetTermsAndConditions(bool hasPremierAccelerator, string flow)
        {
            var dbKey = hasPremierAccelerator ? "AAPA_TERMS_AND_CONDITIONS_AA_PA_MP"
                                              : "AAPA_TERMS_AND_CONDITIONS_AA_MP";

            if (ConfigUtility.IsViewResFlowCheckOut(flow))
            {
                dbKey = _configuration.GetValue<bool>("EnablePPRChangesForAAPA") ? hasPremierAccelerator ? "PPR_AAPA_TERMS_AND_CONDITIONS_AA_PA_MP"
                                                : "PPR_AAPA_TERMS_AND_CONDITIONS_AA_MP" : hasPremierAccelerator ? "AAPA_TERMS_AND_CONDITIONS_AA_PA_MP"
                                                : "AAPA_TERMS_AND_CONDITIONS_AA_MP";
            }

            var docs = await _legalDocumentsForTitlesService.GetNewLegalDocumentsForTitles(dbKey, _headers.ContextValues.TransactionId, true);
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

        public async Task<List<MOBMobileCMSContentMessages>> GetBagsTermsAndConditions(string sessionId, bool isMFOPSelected)
        {
            var session = new Session();
            session = await _sessionHelperService.GetSession<Session>(sessionId, session.ObjectName, new List<string> { sessionId, session.ObjectName }).ConfigureAwait(false);
            MOBRequest request = new MOBRequest();
            request.TransactionId = session.SessionId;
            string key = "CMSContentMessages_GroupName_Baggage_Messages";
            string groupName = _configuration.GetValue<string>(key);
            var messages = await GetSDLContentByGroupName(request, session.SessionId, session.Token, groupName, key);
            var bagTnC = messages?.FirstOrDefault(x => x.Title == "BAGGAGE.PrepayForCheckedBags.TermsAndConditions.Bag");
            var milesFopTnC = isMFOPSelected ? messages?.FirstOrDefault(x => x.Title == "BAGGAGE.TermsAndConditions.MilesFOP") : null;
            
            var tncs = new List<MOBMobileCMSContentMessages>();
            if (bagTnC != null) tncs.Add(new MOBMobileCMSContentMessages
            {
                Title = bagTnC.ContentShort,
                ContentShort = HttpUtility.HtmlDecode(_configuration.GetValue<string>("PaymentTnCMessage").ToString()),
                ContentFull = bagTnC.ContentFull
            });
            if (milesFopTnC != null) tncs.Add(new MOBMobileCMSContentMessages
            {
                ContentShort = HttpUtility.HtmlDecode(_configuration.GetValue<string>("PaymentTnCMessage").ToString()),
                ContentFull = milesFopTnC.ContentFull,
                ContentKey = "MilesFOP_TandC"
            });

            return tncs;
        }
        private async Task<List<MOBMobileCMSContentMessages>> GetTermsAndConditions()
        {
            var cmsContentMessages = new List<MOBMobileCMSContentMessages>();
            var docKeys = "PCU_TnC"; ;
            var docs = await _legalDocumentsForTitlesService.GetNewLegalDocumentsForTitles(docKeys, _headers.ContextValues.TransactionId, true);
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

        private async Task<List<MOBMobileCMSContentMessages>> GetTermsAndConditions(string docKeys)
        {
            var cmsContentMessages = new List<MOBMobileCMSContentMessages>();
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

        private double GetCloseBookingFee(bool isPost, United.Service.Presentation.InteractionModel.ShoppingCart flightReservationResponseShoppingCart, string flow)
        {
            return isPost ? flightReservationResponseShoppingCart.Items.SelectMany(d => d.Product).Where(d => d.Code == "RBF").FirstOrDefault().Price.Totals.FirstOrDefault().Amount :
                                  flightReservationResponseShoppingCart.Items.SelectMany(d => d.Product).Where(d => d.Code == "RBF").SelectMany(x => x.Price.Totals).FirstOrDefault().Amount;
        }

        private int MaxmimumPriceForUplift
        {
            get
            {
                var maximumAmountForUplift = _configuration.GetValue<string>("MaximumPriceForUplift");
                if (string.IsNullOrEmpty(maximumAmountForUplift))
                    return 150000;

                int.TryParse(maximumAmountForUplift, out int upliftMaxAmount);
                return upliftMaxAmount;
            }
        }

        private bool HasEligibleProductsForUplift(string totalPrice, List<MOBProdDetail> products)
        {
            decimal.TryParse(totalPrice, out decimal price);
            if (price >= MinimumPriceForUplift && price <= MaxmimumPriceForUplift)
            {
                var eligibleProductsForUplift = _configuration.GetValue<string>("EligibleProductsForUpliftInViewRes").Split(',');
                if (eligibleProductsForUplift.Any())
                {
                    return products.Any(p => eligibleProductsForUplift.Contains(p.Code));
                }
            }

            return false;
        }

        private bool IsEligibileForUplift(MOBSHOPReservation reservation, MOBShoppingCart shoppingCart)
        {
            if (shoppingCart?.Flow?.ToUpper() == FlowType.VIEWRES.ToString().ToUpper())
            {
                return HasEligibleProductsForUplift(shoppingCart.TotalPrice, shoppingCart.Products);
            }

            if (!_configuration.GetValue<bool>("EnableUpliftPayment"))
                return false;
            if (shoppingCart.Offers != null 
                && !IsUpliftEligbleOffer(shoppingCart.Offers))
            {
                return false;
            }
            if (reservation == null || reservation.Prices == null || shoppingCart == null || shoppingCart?.Flow != FlowType.BOOKING.ToString())
                return false;

            if (reservation.ShopReservationInfo?.IsCorporateBooking ?? false)
                return false;

            if (shoppingCart.Products?.Any(p => p?.Code == "FLK") ?? false)
                return false;

            if (!_configuration.GetValue<bool>("DisableFixForUpliftFareLockDefect"))
            {
                if (shoppingCart.Products?.Any(p => p?.Code?.ToUpper() == "FARELOCK") ?? false)
                    return false;
            }

            if (reservation.Prices.Any(p => "TOTALPRICEFORUPLIFT".Equals(p.DisplayType, StringComparison.CurrentCultureIgnoreCase) && p.Value >= MinimumPriceForUplift && p.Value <= MaxmimumPriceForUplift) &&
               (shoppingCart?.SCTravelers?.Any(t => t?.TravelerTypeCode == "ADT" || t?.TravelerTypeCode == "SNR") ?? false))
            {
                return true;
            }
            return false;
        }

        public bool IsUpliftEligbleOffer(MOBOffer offer)
        {
            if (!string.IsNullOrEmpty(offer.OfferCode)
                && (offer.IsPassPlussOffer || offer.OfferType == OfferType.ECD))
            {
                return false;
            }
            return true;
        }
        private void AssignUpliftText(MOBShoppingCart shoppingCart, MOBSHOPReservation reservation)
        {

            if (IsEligibileForUplift(reservation, shoppingCart) && shoppingCart?.FormofPaymentDetails?.MoneyPlusMilesCredit?.SelectedMoneyPlusMiles == null)                //Check if eligible for Uplift
            {
                shoppingCart.OmniCart.IsUpliftEligible = true;      //Set property to true, if Uplift is eligible
            }
            else //Set Uplift properties to false / empty as Uplift isn't eligible
            {
                shoppingCart.OmniCart.IsUpliftEligible = false;
            }
        }

        public List<MOBSection> GetFOPDetails(MOBSHOPReservation reservation, MOBApplication application)
        {
            var mobSection = default(MOBSection);
            if (ConfigUtility.EnableMFOP(application.Id, application.Version.Major))
            {
                return GetMFOPDetails(reservation);
            }
            if (reservation?.Prices?.Count > 0)
            {
                var travelCredit = reservation.Prices.FirstOrDefault(price => new[] { "TB", "CERTIFICATE", "FFC", "TRAVELCREDITS" }.Any(credit => string.Equals(price.PriceType, credit, StringComparison.OrdinalIgnoreCase)));
                if (travelCredit != null)
                {
                    if (string.Equals(travelCredit.PriceType, "TB", StringComparison.OrdinalIgnoreCase))
                    {
                        mobSection = new MOBSection();
                        mobSection.Text1 = !string.IsNullOrEmpty(_configuration.GetValue<string>("UnitedTravelBankCashLabelText")) ? _configuration.GetValue<string>("UnitedTravelBankCashLabelText") : "United TravelBank cash";
                        mobSection.Text2 = !string.IsNullOrEmpty(_configuration.GetValue<string>("TravelBankCashAppliedLabelText")) ? _configuration.GetValue<string>("TravelBankCashAppliedLabelText") : "TravelBank cash applied";
                        mobSection.Text3 = travelCredit.FormattedDisplayValue;
                    }
                    else
                    {
                        mobSection = new MOBSection();
                        mobSection.Text1 = !string.IsNullOrEmpty(_configuration.GetValue<string>("TravelCreditsLabelText")) ? _configuration.GetValue<string>("TravelCreditsLabelText") : "Travel credits";
                        mobSection.Text2 = !string.IsNullOrEmpty(_configuration.GetValue<string>("CreditKeyLabelText")) ? _configuration.GetValue<string>("CreditKeyLabelText") : "Credit";
                        mobSection.Text3 = travelCredit.FormattedDisplayValue;

                    }
                }
            }
            return mobSection != null ? new List<MOBSection> { mobSection } : null;
        }
        private List<MOBSection> GetMFOPDetails(MOBSHOPReservation reservation)
        {
            var mobSectionList = new List<MOBSection>();
            var mobSection = default(MOBSection);

            var travelCredit = reservation.Prices.Where(price => new[] { "TB", "CERTIFICATE", "FFC", "TRAVELCREDITS" }.Any(credit => string.Equals(price.PriceType, credit, StringComparison.OrdinalIgnoreCase)));
            mobSection = new MOBSection();
            foreach (var tc in travelCredit)
            {
                if (string.Equals(tc.PriceType, "TB", StringComparison.OrdinalIgnoreCase))
                {
                    mobSection = new MOBSection();
                    mobSection.Text1 = !string.IsNullOrEmpty(_configuration.GetValue<string>("UnitedTravelBankCashLabelText")) ? _configuration.GetValue<string>("UnitedTravelBankCashLabelText") : "United TravelBank cash";
                    mobSection.Text2 = !string.IsNullOrEmpty(_configuration.GetValue<string>("TravelBankCashAppliedLabelText")) ? _configuration.GetValue<string>("TravelBankCashAppliedLabelText") : "TravelBank cash applied";
                    mobSection.Text3 = tc.FormattedDisplayValue;
                }
                else
                {
                    mobSection = new MOBSection();
                    mobSection.Text1 = string.Equals(tc.PriceType, "CERTIFICATE", StringComparison.OrdinalIgnoreCase) || string.Equals(tc.PriceType, "TRAVELCREDITS", StringComparison.OrdinalIgnoreCase) ? "Travel certificate" : "Future flight credit";
                    mobSection.Text2 = "Travel credit";
                    mobSection.Text3 = tc.FormattedDisplayValue;
                }
               
                mobSectionList.Add(mobSection);
            }

            return mobSectionList;
        }
        private MOBItem GetPayLaterAmount(List<MOBProdDetail> products, MOBSHOPReservation reservation)
        {
            if (products != null && reservation != null)
            {
                if (IsFarelock(products))
                {
                    return new MOBItem { Id = _configuration.GetValue<string>("PayDueLaterLabelText"), CurrentValue = GetGrandTotalPrice(reservation) };
                }
            }
            return null;
        }

        public string GetGrandTotalPrice(MOBSHOPReservation reservation)
        {
            if (reservation?.Prices != null)
            {
                var grandTotalPrice = reservation.Prices.Exists(p => p.DisplayType.ToUpper().Equals("GRAND TOTAL"))
                                ? reservation.Prices.FirstOrDefault(p => p.DisplayType.ToUpper().Equals("GRAND TOTAL"))
                                : reservation.Prices.FirstOrDefault(p => p.DisplayType.ToUpper().Equals("TOTAL"));
                if (_configuration.GetValue<bool>("EnableLivecartForAwardTravel") && reservation.AwardTravel)
                {
                    var totalDue = string.Empty;
                    var awardPrice = reservation.Prices.FirstOrDefault(p => string.Equals("miles", p.DisplayType, StringComparison.OrdinalIgnoreCase));
                    if (awardPrice != null)
                    {
                        totalDue = FormatedMilesValueAndText(awardPrice.Value);
                    }
                    if (grandTotalPrice != null)
                    {
                        totalDue = string.IsNullOrWhiteSpace(totalDue)
                                    ? grandTotalPrice.FormattedDisplayValue
                                    : $"{totalDue} + {grandTotalPrice.FormattedDisplayValue}";
                    }
                    return totalDue;
                }
                else
                {
                    if (grandTotalPrice != null)
                    {
                        return grandTotalPrice.FormattedDisplayValue;
                    }
                }
            }
            return string.Empty;
        }

        private static string FormatedMilesValueAndText(double milesValue)
        {
            if (milesValue >= 1000)
                return (milesValue / 1000D).ToString("0.#" + "K miles");
            else if (milesValue > 0)
                return milesValue.ToString("0,# miles");
            else
                return string.Empty;
        }

        private MOBItem GetTotalPrice(List<MOBProdDetail> products, MOBSHOPReservation reservation)
        {
            if (products != null && reservation != null)
            {
                return new MOBItem
                {
                    Id = IsFarelock(products) ? _configuration.GetValue<string>("FarelockTotalPriceLabelText") : _configuration.GetValue<string>("TotalPriceLabelText")
                ,
                    CurrentValue = IsFarelock(products) ? GetFareLockPrice(products) : GetGrandTotalPrice(reservation)
                };
            }
            return null;
        }


        private int GetCartItemsCount(MOBShoppingCart shoppingcart)
        {
            int itemsCount = 0;
            if (shoppingcart?.Products != null && shoppingcart.Products.Count > 0)
            {
                shoppingcart.Products.ForEach(product =>
                {
                    if (!string.IsNullOrEmpty(product.ProdTotalPrice) && Decimal.TryParse(product.ProdTotalPrice, out decimal totalprice) && (totalprice > 0
                    || product.Code == "RES"&& totalprice == 0))
                    {
                        if (product?.Segments != null && product.Segments.Count > 0)
                        {
                            product.Segments.ForEach(segment =>
                            {
                                segment.SubSegmentDetails.ForEach(subSegment =>
                                {
                                    if (subSegment != null)
                                    {
                                        if (product.Code == "SEATASSIGNMENTS")
                                        {
                                            itemsCount += subSegment.PaxDetails.Count();

                                        }
                                        else
                                        {
                                            itemsCount += 1;
                                        }
                                    }
                                });

                            });
                            return;
                        }
                        itemsCount += 1;
                    }
                });
            }
            return itemsCount;
        }

        public bool IsEnableBundleLiveUpdateChanges(int applicationId, string appVersion, bool isDisplayCart)
        {
            if (_configuration.GetValue<bool>("EnableBundleLiveUpdateChanges")
                && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Android_EnableBundleLiveUpdateChanges_AppVersion"), _configuration.GetValue<string>("iPhone_EnableBundleLiveUpdateChanges_AppVersion"))
                && isDisplayCart)
            {
                return true;
            }
            return false;
        }

        public bool IsEnableOmniCartMVP2Changes(int applicationId, string appVersion, bool isDisplayCart)
        {
            if (_configuration.GetValue<bool>("EnableOmniCartMVP2Changes") && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Android_EnableOmniCartMVP2Changes_AppVersion"), _configuration.GetValue<string>("iPhone_EnableOmniCartMVP2Changes_AppVersion")) && isDisplayCart)
            {
                return true;
            }
            return false;
        }

        private async Task AssignBalanceAttentionInfoWarningMessage(MOBSHOPReservationInfo2 shopReservationInfo2, MOBFOPTravelCertificate travelCertificate)
        {
            if (shopReservationInfo2 == null)
            {
                shopReservationInfo2 = new MOBSHOPReservationInfo2();
            }
            //To show balance attention message on RTI when Combinability is ON from Shoppingcart service  and OFF from MRest
            if (shopReservationInfo2.InfoWarningMessages == null)
            {
                shopReservationInfo2.InfoWarningMessages = new List<MOBInfoWarningMessages>();
            }
            MOBInfoWarningMessages balanceAttentionMessage = new MOBInfoWarningMessages();
            balanceAttentionMessage = await GetETCBalanceAttentionInfoWarningMessage(travelCertificate);
            if (shopReservationInfo2.InfoWarningMessages.Exists(x => x.Order == MOBINFOWARNINGMESSAGEORDER.RTIETCBALANCEATTENTION.ToString()))
            {
                shopReservationInfo2.InfoWarningMessages.Remove(shopReservationInfo2.InfoWarningMessages.Find(x => x.Order == MOBINFOWARNINGMESSAGEORDER.RTIETCBALANCEATTENTION.ToString()));
            }
            if (balanceAttentionMessage != null)
            {
                shopReservationInfo2.InfoWarningMessages.Add(balanceAttentionMessage);
                shopReservationInfo2.InfoWarningMessages = shopReservationInfo2.InfoWarningMessages.OrderBy(c => (int)((MOBINFOWARNINGMESSAGEORDER)System.Enum.Parse(typeof(MOBINFOWARNINGMESSAGEORDER), c.Order))).ToList();
            }
        }
        private async Task<MOBInfoWarningMessages> GetETCBalanceAttentionInfoWarningMessage(MOBFOPTravelCertificate travelCertificate)
        {
            MOBInfoWarningMessages infoMessage = null;
            double? etcBalanceAttentionAmount = travelCertificate?.Certificates?.Sum(c => c.NewValueAfterRedeem);
            if (etcBalanceAttentionAmount > 0 && travelCertificate?.Certificates?.Count > 1)
            {
                List<MOBMobileCMSContentMessages> alertMessages = new List<MOBMobileCMSContentMessages>();
                alertMessages = await AssignAlertMessages("TravelCertificate_Combinability_ReviewETCAlertMsg");
                infoMessage = new MOBInfoWarningMessages();
                infoMessage.Order = MOBINFOWARNINGMESSAGEORDER.RTIETCBALANCEATTENTION.ToString();
                infoMessage.IconType = MOBINFOWARNINGMESSAGEICON.INFORMATION.ToString();
                infoMessage.Messages = new List<string>();
                infoMessage.Messages.Add(string.Format(alertMessages.Find(x => x.HeadLine == "TravelCertificate_Combinability_ReviewETCAlertMsgs_ETCBalanceAttentionmessage").ContentFull, String.Format("{0:0.00}", etcBalanceAttentionAmount)));
            }
            return infoMessage;
        }
        private async Task<List<MOBMobileCMSContentMessages>> AssignAlertMessages(string captionKey)
        {
            List<MOBMobileCMSContentMessages> tncs = null;
            var docs = await GetCaptions(captionKey);
            if (docs != null && docs.Any())
            {
                tncs = new List<MOBMobileCMSContentMessages>();
                foreach (var doc in docs)
                {
                    var tnc = new MOBMobileCMSContentMessages
                    {
                        ContentFull = doc.CurrentValue,
                        HeadLine = doc.Id
                    };
                    tncs.Add(tnc);
                }
            }
            return tncs;
        }

        public async Task<List<MOBItem>> GetCaptions(string key)
        {
            return !string.IsNullOrEmpty(key) ? await GetCaptions(key, true) : null;
        }
        private async Task<List<MOBItem>> GetCaptions(string keyList, bool isTnC)
        {
            //confirm
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

        public bool IncludeMoneyPlusMiles(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableMilesPlusMoney")
                && GeneralHelper.IsApplicationVersionGreater
                (appId, appVersion, "AndroidMilesPlusMoneyVersion", "iPhoneMilesPlusMoneyVersion", "", "", true, _configuration);
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

        private async System.Threading.Tasks.Task AssignFFCValues(string sessionid, MOBShoppingCart shoppingCart, MOBRequest request, MOBFormofPaymentDetails formOfPaymentDetails, MOBSHOPReservation reservation, bool isCheckout = false)
        {
            if (IncludeFFCResidual(request.Application.Id, request.Application.Version.Major))
            {
                if (shoppingCart.FormofPaymentDetails != null)
                {
                    formOfPaymentDetails.TravelFutureFlightCredit = shoppingCart.FormofPaymentDetails.TravelFutureFlightCredit;
                    formOfPaymentDetails.FormOfPaymentType = shoppingCart.FormofPaymentDetails.FormOfPaymentType;
                }
                if (formOfPaymentDetails.TravelFutureFlightCredit == null)
                    formOfPaymentDetails.TravelFutureFlightCredit = new MOBFOPTravelFutureFlightCredit();

                var session = await _sessionHelperService.GetSession<Session>(sessionid, new Session().ObjectName, new List<string> { sessionid, new Session().ObjectName }).ConfigureAwait(false);

                List<CMSContentMessage> lstMessages = await GetSDLContentByGroupName(request, sessionid, session?.Token, _configuration.GetValue<string>("CMSContentMessages_GroupName_BookingRTI_Messages"), "BookingPathRTI_CMSContentMessagesCached_StaticGUID");
                formOfPaymentDetails.TravelFutureFlightCredit.LookUpFFCMessages = GetSDLMessageFromList(lstMessages, "RTI.FutureFlightCredits.LookupFFC");
                formOfPaymentDetails.TravelFutureFlightCredit.LookUpFFCMessages.AddRange(GetSDLMessageFromList(lstMessages, "RTI.FutureFlightCredits.LookupFFC.BtnText"));
                formOfPaymentDetails.TravelFutureFlightCredit.FindFFCMessages = GetSDLMessageFromList(lstMessages, "RTI.FutureFlightCredits.FindFFC");
                formOfPaymentDetails.TravelFutureFlightCredit.FindFFCMessages.AddRange(GetSDLMessageFromList(lstMessages, "RTI.FutureFlightCredits.FindFFC.BtnText"));

                if (shoppingCart.FormofPaymentDetails?.TravelFutureFlightCredit?.FutureFlightCredits?.Count > 0)
                {
                    string email = (string.IsNullOrEmpty(formOfPaymentDetails.EmailAddress) ? shoppingCart.FormofPaymentDetails?.EmailAddress : formOfPaymentDetails.EmailAddress);
                    formOfPaymentDetails.TravelFutureFlightCredit.EmailConfirmationFFCMessages = AssignEmailMessageForFFCRefund(lstMessages, reservation.Prices, email, formOfPaymentDetails.TravelFutureFlightCredit, request.Application);
                    AddGrandTotalIfNotExistInPricesAndUpdateCertificateValue(reservation.Prices, formOfPaymentDetails);
                    bool isAncillaryFFCEnable = IsInclueWithThisToggle(request.Application.Id, request.Application.Version.Major, "EnableTravelCreditAncillary", "AndroidTravelCreditVersion", "iPhoneTravelCreditVersion");
                    formOfPaymentDetails.TravelFutureFlightCredit.AllowedFFCAmount = GetAllowedFFCAmount(shoppingCart.Products, isAncillaryFFCEnable);
                    bool isDirty = UpdateFFCAmountAsPerChangedPrice(formOfPaymentDetails.TravelFutureFlightCredit, reservation.TravelersCSL, sessionid, request.Application);
                    UpdatePricesInReservation(formOfPaymentDetails.TravelFutureFlightCredit, reservation.Prices,request.Application, request.Application.Id, request.Application.Version.Major, isCheckout : isCheckout);
                    if (isDirty)
                    {
                        Reservation bookingPathReservation = new Reservation();
                        bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(sessionid, bookingPathReservation.ObjectName, new List<string> { sessionid, bookingPathReservation.ObjectName }).ConfigureAwait(false);

                        if (bookingPathReservation.TravelersCSL != null && bookingPathReservation.TravelersCSL.Count > 0)
                        {
                            bookingPathReservation.TravelersCSL = new SerializableDictionary<string, MOBCPTraveler>();
                            foreach (var travelerCSL in reservation.TravelersCSL)
                            {
                                bookingPathReservation.TravelersCSL.Add(travelerCSL.PaxIndex.ToString(), travelerCSL);
                            }
                        }
                        bookingPathReservation.Prices = reservation.Prices;
                        await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, sessionid, new List<string> { sessionid, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);

                    }

                    AssignIsOtherFOPRequired(formOfPaymentDetails, reservation.Prices);
                    AssignFormOfPaymentType(formOfPaymentDetails, reservation.Prices, formOfPaymentDetails?.SecondaryCreditCard != null);
                }
            }
        }
        private List<MOBMobileCMSContentMessages> GetSDLMessageFromList(List<CMSContentMessage> list, string title)
        {
            List<MOBMobileCMSContentMessages> listOfMessages = new List<MOBMobileCMSContentMessages>();
            if(!_configuration.GetValue<bool>("enableNullCheckForSDL"))
                list?.Where(l => (l.Title ?? "").ToUpper().Equals(title.ToUpper()))?.ForEach(i => listOfMessages.Add(new MOBMobileCMSContentMessages()
                {
                    Title = i.Title,
                    ContentFull = i.ContentFull,
                    HeadLine = i.Headline,
                    ContentShort = i.ContentShort,
                    LocationCode = i.LocationCode
                }));
            else
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

        public async Task<List<CMSContentMessage>> GetSDLContentByGroupName(MOBRequest request, string guid, string token, string groupName, string docNameConfigEntry, bool useCache = false)
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
            if (string.IsNullOrEmpty(token))
            {
                Session session = new Session();
                session = await _sessionHelperService.GetSession<Session>(guid, session.ObjectName, new List<string>() { guid, session.ObjectName }).ConfigureAwait(false);
                token = session?.Token;
            }
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

            response = await _cMSContentService.GetSDLContentByGroupName<MOBCSLContentMessagesResponse>(token, "message", jsonRequest, guid).ConfigureAwait(false);

            if (response == null)
            {
                return null;
            }

            if (response.Errors.Count > 0)
            {
                string errorMsg = String.Join(" ", response.Errors.Select(x => x.Message));
                _logger.LogWarning("GetSDLContentByGroupName {CSL-CallError}", errorMsg);
                return null;
            }

            if (response != null && (Convert.ToBoolean(response.Status) && response.Messages != null))
            {
                if (!_configuration.GetValue<bool>("DisableSDLEmptyTitleFix"))
                {
                    response.Messages = response.Messages.Where(l => l.Title != null)?.ToList();
                }
                var saveSDL = await _cachingService.SaveCache<MOBCSLContentMessagesResponse>(_configuration.GetValue<string>(docNameConfigEntry) + ObjectNames.MOBCSLContentMessagesResponseFullName, response, request.TransactionId, new TimeSpan(1, 30, 0)).ConfigureAwait(false);
            }

            return response.Messages;
        }

        private bool IncludeFFCResidual(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableFFCResidual")
                && GeneralHelper.IsApplicationVersionGreater
                (appId, appVersion, "AndroidFFCResidualVersion", "iPhoneFFCResidualVersion", "", "", true, _configuration);
        }

        public async Task LoadandAddTravelCertificate(MOBShoppingCart shoppingCart, MOBSHOPReservation reservation, bool isETCCertificatesExistInShoppingCartPersist, MOBApplication application, bool isCheckout = false)
        {
            var persistedTravelCertifcateResponse = new MOBFOPTravelerCertificateResponse();

            if (_configuration.GetValue<bool>("CombinebilityETCToggle") && (reservation.Prices.Exists(price => price.DisplayType.ToUpper().Trim() == "CERTIFICATE") || isETCCertificatesExistInShoppingCartPersist))
            {
                persistedTravelCertifcateResponse = await _sessionHelperService.GetSession<MOBFOPTravelerCertificateResponse>(reservation.SessionId, persistedTravelCertifcateResponse.ObjectName, new List<string> { reservation.SessionId, persistedTravelCertifcateResponse.ObjectName }).ConfigureAwait(false);
            }

            if (persistedTravelCertifcateResponse?.ShoppingCart?.CertificateTravelers?.Count > 0)
            {
                shoppingCart.CertificateTravelers = persistedTravelCertifcateResponse.ShoppingCart.CertificateTravelers;
            }
            else if (shoppingCart.CertificateTravelers != null)
            {
                AssignCertificateTravelers(shoppingCart);
            }

            await AssignTravelerCertificateToFOP(persistedTravelCertifcateResponse, shoppingCart.Products, shoppingCart.Flow);
            var formOfPayment = shoppingCart.FormofPaymentDetails;

            MOBFormofPaymentDetails persistedFOPDetail = persistedTravelCertifcateResponse.ShoppingCart.FormofPaymentDetails;

            //validate this code - for MFOP 
            formOfPayment.TravelCertificate = (ConfigUtility.EnableMFOP(application.Id, application.Version.Major) && persistedFOPDetail?.TravelCertificate?.Certificates == null && formOfPayment?.TravelCertificate?.Certificates !=null) ? formOfPayment?.TravelCertificate : persistedFOPDetail.TravelCertificate;
            formOfPayment.BillingAddress = formOfPayment.BillingAddress == null ? persistedFOPDetail.BillingAddress : formOfPayment.BillingAddress;
            formOfPayment.Email = formOfPayment.Email == null ? persistedFOPDetail.Email : formOfPayment.Email;
            formOfPayment.Phone = formOfPayment.Phone == null ? persistedFOPDetail.Phone : formOfPayment.Phone;
            formOfPayment.EmailAddress = formOfPayment.EmailAddress == null ? persistedFOPDetail.EmailAddress : formOfPayment.EmailAddress;


            //Add requested certificaates to TravelerCertificate object in FOP
            formOfPayment.TravelCertificate.AllowedETCAmount = ConfigUtility.GetAlowedETCAmount(shoppingCart.Products, shoppingCart.Flow);
            formOfPayment?.TravelCertificate?.Certificates?.ForEach(c => c.RedeemAmount = 0);

            //added to match onprem response
            //formOfPayment.TravelCertificate.Certificates = formOfPayment.TravelCertificate.Certificates.Count == 0 ? formOfPayment.TravelCertificate.Certificates = null : formOfPayment.TravelCertificate.Certificates;
            //formOfPayment.TravelCertificate.LearnmoreTermsandConditions = formOfPayment.TravelCertificate.LearnmoreTermsandConditions.Count == 0 ? formOfPayment.TravelCertificate.LearnmoreTermsandConditions = null : formOfPayment.TravelCertificate.LearnmoreTermsandConditions;

            UtilityHelper.AddRequestedCertificatesToFOPTravelerCertificates(formOfPayment.TravelCertificate.Certificates, shoppingCart.ProfileTravelerCertificates, formOfPayment.TravelCertificate);
            Reservation bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(reservation.SessionId, new Reservation().ObjectName, new List<string> { reservation.SessionId, new Reservation().ObjectName }).ConfigureAwait(false);
            AddGrandTotalIfNotExistInPricesAndUpdateCertificateValue(bookingPathReservation.Prices, formOfPayment);

            if (ConfigUtility.EnableMFOP(application.Id, application.Version.Major))
                UpdateETCPricesInReservation(formOfPayment.TravelCertificate, reservation.Prices, application.Id, application.Version.Major, isCheckout: isCheckout);
            else
                ConfigUtility.UpdateCertificateAmountInTotalPrices(bookingPathReservation.Prices, formOfPayment.TravelCertificate.TotalRedeemAmount);

            ConfigUtility.AssignIsOtherFOPRequired(formOfPayment, bookingPathReservation.Prices, shoppingCart.FormofPaymentDetails?.SecondaryCreditCard != null);
                     
            await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, reservation.SessionId, new List<string> { reservation.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);

            reservation.Prices = bookingPathReservation.Prices;
            persistedTravelCertifcateResponse.ShoppingCart.FormofPaymentDetails = formOfPayment;
            persistedTravelCertifcateResponse.ShoppingCart.CertificateTravelers = shoppingCart.CertificateTravelers;
            persistedTravelCertifcateResponse.ShoppingCart.FormofPaymentDetails.TravelCertificate.ReviewETCMessages = await UpdateReviewETCAlertmessages(shoppingCart);
            ConfigUtility.UpdateSavedCertificate(shoppingCart);
            persistedTravelCertifcateResponse.ShoppingCart.ProfileTravelerCertificates = shoppingCart.ProfileTravelerCertificates;

            await _sessionHelperService.SaveSession<MOBFOPTravelerCertificateResponse>(persistedTravelCertifcateResponse, reservation.SessionId, new List<string> { reservation.SessionId, persistedTravelCertifcateResponse.ObjectName }, persistedTravelCertifcateResponse.ObjectName).ConfigureAwait(false);
            await _sessionHelperService.SaveSession<MOBShoppingCart>(shoppingCart, reservation.SessionId, new List<string> { reservation.SessionId, shoppingCart.ObjectName }, shoppingCart.ObjectName).ConfigureAwait(false);
        }

        private async Task<List<MOBMobileCMSContentMessages>> UpdateReviewETCAlertmessages(MOBShoppingCart shoppingCart)
        {
            List<MOBMobileCMSContentMessages> alertMessages = new List<MOBMobileCMSContentMessages>();
            alertMessages = await AssignAlertMessages("TravelCertificate_Combinability_ReviewETCAlertMsg");
            //Show other fop required message only when isOtherFop is required
            if (shoppingCart?.FormofPaymentDetails?.IsOtherFOPRequired == false)
            {
                alertMessages.Remove(alertMessages.Find(x => x.HeadLine == "TravelCertificate_Combinability_ReviewETCAlertMsgs_OtherFopRequiredMessage"));
            }
            //Update the total price
            if (shoppingCart?.FormofPaymentDetails?.TravelCertificate?.Certificates != null &&
                shoppingCart?.FormofPaymentDetails?.TravelCertificate?.Certificates.Count > 0
                )
            {
                double balanceETCAmount = Math.Round(shoppingCart.FormofPaymentDetails.TravelCertificate.Certificates.Sum(x => x.NewValueAfterRedeem), 2);
                if (balanceETCAmount > 0 && shoppingCart.FormofPaymentDetails?.TravelCertificate?.Certificates?.Count > 1)
                {
                    alertMessages.Find(x => x.HeadLine == "TravelCertificate_Combinability_ReviewETCAlertMsgs_ETCBalanceAttentionmessage").ContentFull = string.Format(alertMessages.Find(x => x.HeadLine == "TravelCertificate_Combinability_ReviewETCAlertMsgs_ETCBalanceAttentionmessage").ContentFull, String.Format("{0:0.00}", balanceETCAmount));
                }
                else
                {
                    alertMessages.Remove(alertMessages.Find(x => x.HeadLine == "TravelCertificate_Combinability_ReviewETCAlertMsgs_ETCBalanceAttentionmessage"));
                }
            }
            return alertMessages;
        }

        private async Task LoadandAddTravelCertificate(MOBShoppingCart shoppingCart, string sessionId, List<MOBSHOPPrice> prices, bool isETCCertificatesExistInShoppingCartPersist, MOBApplication application, bool isCheckout = false)
        {
            var persistedTravelCertifcateResponse = new MOBFOPTravelerCertificateResponse();
            if (_configuration.GetValue<bool>("MTETCToggle") && (prices.Exists(price => price.DisplayType.ToUpper().Trim() == "CERTIFICATE") || isETCCertificatesExistInShoppingCartPersist))
            {
                persistedTravelCertifcateResponse = await _sessionHelperService.GetSession<MOBFOPTravelerCertificateResponse>(sessionId, persistedTravelCertifcateResponse.ObjectName, new List<string> { sessionId, persistedTravelCertifcateResponse.ObjectName }).ConfigureAwait(false);
            }
            else
            {
                persistedTravelCertifcateResponse = await _sessionHelperService.GetSession<MOBFOPTravelerCertificateResponse>(sessionId, persistedTravelCertifcateResponse.ObjectName, new List<string> { sessionId, persistedTravelCertifcateResponse.ObjectName }).ConfigureAwait(false);
            }
            if (_configuration.GetValue<bool>("MTETCToggle") && shoppingCart.IsMultipleTravelerEtcFeatureClientToggleEnabled && shoppingCart?.SCTravelers != null && shoppingCart.SCTravelers.Exists(st => !string.IsNullOrEmpty(st.TravelerNameIndex)))
            {
                if (persistedTravelCertifcateResponse?.ShoppingCart?.CertificateTravelers?.Count > 0)
                {
                    shoppingCart.CertificateTravelers = persistedTravelCertifcateResponse.ShoppingCart.CertificateTravelers;
                }
                else if (shoppingCart.CertificateTravelers != null)
                {
                    AssignCertificateTravelers(shoppingCart, persistedTravelCertifcateResponse, prices, application);
                }
            }
            if (persistedTravelCertifcateResponse?.ShoppingCart?.FormofPaymentDetails?.TravelCertificate != null)
            {
                var formOfPayment = shoppingCart.FormofPaymentDetails;

                MOBFormofPaymentDetails persistedFOPDetail = persistedTravelCertifcateResponse.ShoppingCart.FormofPaymentDetails;
                formOfPayment.TravelCertificate = (ConfigUtility.EnableMFOP(application.Id, application.Version.Major) && persistedFOPDetail?.TravelCertificate?.Certificates == null) ? formOfPayment.TravelCertificate : persistedFOPDetail.TravelCertificate;
                formOfPayment.BillingAddress = formOfPayment.BillingAddress == null ? persistedFOPDetail.BillingAddress : formOfPayment.BillingAddress;
                formOfPayment.Email = formOfPayment.Email == null ? persistedFOPDetail.Email : formOfPayment.Email;
                formOfPayment.Phone = formOfPayment.Phone == null ? persistedFOPDetail.Phone : formOfPayment.Phone;
                formOfPayment.EmailAddress = formOfPayment.EmailAddress == null ? persistedFOPDetail.EmailAddress : formOfPayment.EmailAddress;
                Reservation bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(sessionId, new Reservation().ObjectName, new List<string> { sessionId, new Reservation().ObjectName }).ConfigureAwait(false);
                var requestSCRES = shoppingCart.Products.Find(p => p.Code == "RES");
                var persistSCRES = persistedTravelCertifcateResponse.ShoppingCart.Products.Find(p => p.Code == "RES");
                bool isSCRESProductGotRefreshed = true;
                if (requestSCRES != null && persistSCRES != null)
                {
                    isSCRESProductGotRefreshed = (requestSCRES.ProdTotalPrice != persistSCRES.ProdTotalPrice);
                }
                AddGrandTotalIfNotExistInPricesAndUpdateCertificateValue(bookingPathReservation.Prices, formOfPayment);

                if (ConfigUtility.EnableMFOP(application.Id, application.Version.Major))
                    UpdateETCPricesInReservation(formOfPayment.TravelCertificate, bookingPathReservation.Prices, application.Id, application.Version.Major, false, isCheckout);
                else
                UpdateCertificateAmountInTotalPrices(bookingPathReservation.Prices, shoppingCart.Products, formOfPayment.TravelCertificate.TotalRedeemAmount, isSCRESProductGotRefreshed);

                ConfigUtility.AssignIsOtherFOPRequired(formOfPayment, bookingPathReservation.Prices, shoppingCart.FormofPaymentDetails?.SecondaryCreditCard != null);

                await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, sessionId, new List<string> { sessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);

                persistedTravelCertifcateResponse.ShoppingCart.FormofPaymentDetails = formOfPayment;
                persistedTravelCertifcateResponse.ShoppingCart.CertificateTravelers = shoppingCart.CertificateTravelers;
                if (_configuration.GetValue<bool>("SavedETCToggle"))
                {
                    ConfigUtility.UpdateSavedCertificate(shoppingCart);
                    persistedTravelCertifcateResponse.ShoppingCart.ProfileTravelerCertificates = shoppingCart.ProfileTravelerCertificates;
                }
                await _sessionHelperService.SaveSession<MOBFOPTravelerCertificateResponse>(persistedTravelCertifcateResponse, sessionId, new List<string> { sessionId, persistedTravelCertifcateResponse.ObjectName }, persistedTravelCertifcateResponse.ObjectName).ConfigureAwait(false);//sessionId, typeof(FOPTravelerCertificateResponse).FullName,
                await _sessionHelperService.SaveSession<MOBShoppingCart>(shoppingCart, sessionId, new List<string> { sessionId, shoppingCart.ObjectName }, shoppingCart.ObjectName).ConfigureAwait(false);
            }
        }

        private async Task UpdateTCPriceAndFOPType(List<MOBSHOPPrice> prices, MOBFormofPaymentDetails formofPaymentDetails, MOBApplication application, List<MOBProdDetail> products, List<MOBCPTraveler> travelers, bool isManageTravelCreditApi = false, bool isCheckout = false)
        {
            if (IncludeTravelCredit(application.Id, application.Version.Major))
            {
                ApplyFFCToAncillary(products, application, formofPaymentDetails, prices, isManageTravelCreditApi: isManageTravelCreditApi, isCheckout: isCheckout);
                var price = prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "CERTIFICATE" || p.DisplayType.ToUpper() == "FFC");
                if (price != null)
                {
                    formofPaymentDetails.TravelCreditDetails.AlertMessages = (formofPaymentDetails.TravelFutureFlightCredit?.FutureFlightCredits?.Count > 0 ?
                                                                              formofPaymentDetails.TravelFutureFlightCredit.ReviewFFCMessages :
                                                                              formofPaymentDetails.TravelCertificate.ReviewETCMessages.Where(m => m.HeadLine != "TravelCertificate_Combinability_ReviewETCAlertMsgs_OtherFopRequiredMessage").ToList());
                }
                else if (formofPaymentDetails.TravelCreditDetails != null)
                {
                    formofPaymentDetails.TravelCreditDetails.AlertMessages = null;
                }

                UpdateTravelCreditAmountWithSelectedETCOrFFC(formofPaymentDetails, prices, travelers, application, isManageTravelCreditApi);
                try
                {
                    MOBCSLContentMessagesResponse lstMessages = null;

                    var s = await _cachingService.GetCache<string>(_configuration.GetValue<string>("BookingPathRTI_CMSContentMessagesCached_StaticGUID") + ObjectNames.MOBCSLContentMessagesResponseFullName, _headers.ContextValues.TransactionId).ConfigureAwait(false);

                    if (!string.IsNullOrEmpty(s))
                    {
                        lstMessages = JsonConvert.DeserializeObject<MOBCSLContentMessagesResponse>(s);
                        formofPaymentDetails.TravelCreditDetails = formofPaymentDetails.TravelCreditDetails ?? new MOBFOPTravelCreditDetails();
                        formofPaymentDetails.TravelCreditDetails.AlertMessages = BuildReviewFFCHeaderMessage(formofPaymentDetails?.TravelFutureFlightCredit, travelers, lstMessages.Messages);
                        if (_configuration.GetValue<bool>("EnableEmailConfirmationBasedOnNEWPINCREATED") == false && ConfigUtility.IsFFCSummaryUpdated(application.Id, application.Version.Major) && isManageTravelCreditApi == false && formofPaymentDetails.TravelCreditDetails.AlertMessages != null)
                        {
                            formofPaymentDetails.TravelFutureFlightCredit.EmailConfirmationFFCMessages = GetSDLMessageFromList(lstMessages.Messages, "RTI.FutureFlightCredits.EmailConfirmation");
                            formofPaymentDetails.TravelFutureFlightCredit.EmailConfirmationFFCMessages[0].ContentFull = string.Format(formofPaymentDetails.TravelFutureFlightCredit.EmailConfirmationFFCMessages[0].ContentFull, formofPaymentDetails.EmailAddress);
                            formofPaymentDetails.TravelFutureFlightCredit.EmailConfirmationFFCMessages[0].ContentShort = formofPaymentDetails.TravelFutureFlightCredit.EmailConfirmationFFCMessages[0].ContentFull;
                        }

                    }
                }
                catch { }

                if (formofPaymentDetails?.FormOfPaymentType == "ETC" ||
                   formofPaymentDetails?.FormOfPaymentType == "FFC")
                    formofPaymentDetails.FormOfPaymentType = "TC";

            }
        }

        private async Task LoadandAddPromoCode(MOBShoppingCart shoppingCart, string sessionId, MOBApplication application)
        {
            var persistedApplyPromoCodeResponse = new MOBApplyPromoCodeResponse();
            persistedApplyPromoCodeResponse = await _sessionHelperService.GetSession<MOBApplyPromoCodeResponse>(sessionId, persistedApplyPromoCodeResponse.ObjectName, new List<string> { sessionId, persistedApplyPromoCodeResponse.ObjectName }).ConfigureAwait(false);
            if (shoppingCart.PromoCodeDetails == null)
            {
                shoppingCart.PromoCodeDetails = new MOBPromoCodeDetails();
            }
            if (persistedApplyPromoCodeResponse != null)
            {
                UpdateShoppinCartWithCouponDetails(shoppingCart);
                persistedApplyPromoCodeResponse.ShoppingCart.PromoCodeDetails = shoppingCart.PromoCodeDetails;
                await _sessionHelperService.SaveSession<MOBShoppingCart>(shoppingCart, sessionId, new List<string> { sessionId, shoppingCart.ObjectName }, shoppingCart.ObjectName).ConfigureAwait(false);
                await _sessionHelperService.SaveSession<MOBApplyPromoCodeResponse>(persistedApplyPromoCodeResponse, sessionId, new List<string> { sessionId, persistedApplyPromoCodeResponse.ObjectName }, persistedApplyPromoCodeResponse.ObjectName).ConfigureAwait(false);
            }
            // DisablePromoOption(shoppingCart);
            IsHidePromoOption(shoppingCart);
        }

        private void AssignNullToETCAndFFCCertificates(MOBFormofPaymentDetails fopDetails, MOBRequest request)
        {
            if (!_configuration.GetValue<bool>("disableSFOPClearFFCAndETCCertificatesToggle") && fopDetails.SecondaryCreditCard != null)
            {
                if (fopDetails?.TravelCertificate?.Certificates?.Count() > 0)
                {
                    fopDetails.TravelCertificate.Certificates = null;
                }
                if (fopDetails?.TravelFutureFlightCredit?.FutureFlightCredits?.Count() > 0)
                {
                    fopDetails.TravelFutureFlightCredit.FutureFlightCredits = null;
                }

                //After checkout empty moneymiles data from the shoppingcart.
                if (IncludeMoneyPlusMiles(request.Application.Id, request.Application.Version.Major))
                {
                    fopDetails.MoneyPlusMilesCredit = null;
                }
                if (IncludeTravelBankFOP(request.Application.Id, request.Application.Version.Major))
                {
                    fopDetails.TravelBankDetails = null;
                }
            }
        }

        private void BuildCartTotalPrice(MOBShoppingCart shoppingCart, MOBSHOPReservation reservation)
        {
            if (shoppingCart.OmniCart == null)
            {
                //todo
                //shoppingCart.OmniCart = new MOBCart();
            }
            shoppingCart.OmniCart.TotalPrice = GetTotalPrice(shoppingCart?.Products, reservation);
        }

        private bool IsFarelock(List<MOBProdDetail> products)
        {
            if (products != null)
            {
                if (products.Any(p => p.Code.ToUpper() == "FARELOCK" || p.Code.ToUpper() == "FLK"))
                {
                    return true;
                }
            }
            return false;
        }

        private string GetFareLockPrice(List<MOBProdDetail> products)
        {
            return products.Where(p => p.Code.ToUpper() == "FARELOCK" || p.Code.ToUpper() == "FLK").First().ProdDisplayTotalPrice;
        }


        private List<MOBMobileCMSContentMessages> AssignEmailMessageForFFCRefund(List<CMSContentMessage> lstMessages, List<MOBSHOPPrice> prices, string email, MOBFOPTravelFutureFlightCredit futureFlightCredit, MOBApplication application)
        {
            List<MOBMobileCMSContentMessages> ffcHeaderMessage = null;
            if (ConfigUtility.EnableMFOP(application.Id, application.Version.Major))
            {
                if (prices.Exists(p => p.DisplayType.ToUpper() == "REFUNDFFCPRICE"))
                {
                    ffcHeaderMessage = GetSDLMessageFromList(lstMessages, "RTI.FutureFlightCredits.EmailConfirmation");
                    ffcHeaderMessage[0].ContentFull = string.Format(ffcHeaderMessage[0].ContentFull, email);
                    ffcHeaderMessage[0].ContentShort = ffcHeaderMessage[0].ContentFull;
                }
            }
            else if (prices.Exists(p => p.DisplayType.ToUpper() == "REFUNDPRICE"))
            {
                ffcHeaderMessage = GetSDLMessageFromList(lstMessages, "RTI.FutureFlightCredits.EmailConfirmation");
                if (IncludeMOBILE12570ResidualFix(application.Id, application.Version.Major))
                {
                    ffcHeaderMessage[0].ContentFull = "";
                    ffcHeaderMessage[0].ContentShort = "";
                }
                else
                {
                    ffcHeaderMessage[0].ContentFull = string.Format(ffcHeaderMessage[0].ContentFull, email);
                    ffcHeaderMessage[0].ContentShort = ffcHeaderMessage[0].ContentFull;
                }
            }
            return ffcHeaderMessage;
        }

        private bool IncludeMOBILE12570ResidualFix(int appId, string appVersion)
        {
            bool isApplicationGreater = GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidMOBILE12570ResidualVersion", "iPhoneMOBILE12570ResidualVersion", "", "", true, _configuration);
            return (_configuration.GetValue<bool>("eableMOBILE12570Toggle") && isApplicationGreater);
        }

        private void AddGrandTotalIfNotExistInPricesAndUpdateCertificateValue(List<MOBSHOPPrice> prices, MOBFormofPaymentDetails formofPaymentDetails)
        {
            var grandTotalPrice = prices.FirstOrDefault(p => p.DisplayType.ToUpper().Equals("GRAND TOTAL"));
            if (grandTotalPrice == null)
            {
                var totalPrice = prices.FirstOrDefault(p => p.DisplayType.ToUpper().Equals("TOTAL"));
                grandTotalPrice = BuildGrandTotalPriceForReservation(totalPrice.Value);
                if (_configuration.GetValue<bool>("EnableAdvanceSearchCouponBooking") && (grandTotalPrice.PromoDetails == null || string.IsNullOrEmpty(grandTotalPrice.PromoDetails.PriceTypeDescription)) && totalPrice.PromoDetails != null && !string.IsNullOrEmpty(totalPrice.PromoDetails.PriceTypeDescription))
                {
                    grandTotalPrice.PromoDetails = totalPrice.PromoDetails;
                }
                prices.Add(grandTotalPrice);
            }
        }

        public static MOBSHOPPrice BuildGrandTotalPriceForReservation(double grandtotal)
        {
            grandtotal = Math.Round(grandtotal, 2, MidpointRounding.AwayFromZero);
            MOBSHOPPrice totalPrice = new MOBSHOPPrice
            {
                CurrencyCode = "USD",
                DisplayType = "Grand Total",
                DisplayValue = grandtotal.ToString("N2", CultureInfo.InvariantCulture)
            };
            totalPrice.FormattedDisplayValue = TopHelper.FormatAmountForDisplay(totalPrice.DisplayValue, TopHelper.GetCultureInfo(totalPrice.CurrencyCode), false); //string.Format("${0:c}", totalPrice.DisplayValue);
            double tempDouble1 = 0;
            double.TryParse(totalPrice.DisplayValue.ToString(), out tempDouble1);
            totalPrice.Value = Math.Round(tempDouble1, 2, MidpointRounding.AwayFromZero);
            totalPrice.PriceType = "Grand Total";
            return totalPrice;
        }


        private double GetAllowedFFCAmount(List<MOBProdDetail> products, bool isAncillaryFFCEnable = false)
        {
            string allowedFFCProducts = string.Empty;
            allowedFFCProducts = _configuration.GetValue<string>("FFCEligibleProductCodes");
            double allowedFFCAmount = products == null ? 0 : products.Where(p => (allowedFFCProducts.IndexOf(p.Code) > -1) && !string.IsNullOrEmpty(p.ProdTotalPrice)).Sum(a => Convert.ToDouble(a.ProdTotalPrice));
            allowedFFCAmount += GetAncillaryAmount(products, isAncillaryFFCEnable);
            allowedFFCAmount = Math.Round(allowedFFCAmount, 2, MidpointRounding.AwayFromZero);
            return allowedFFCAmount;
        }

        private double GetAncillaryAmount(List<MOBProdDetail> products, bool isAncillaryFFCEnable = false)
        {
            double allowedFFCAmount = 0;
            if (isAncillaryFFCEnable)
            {
                string allowedFFCProducts = _configuration.GetValue<string>("TravelCreditEligibleProducts");
                allowedFFCAmount += products == null ? 0 : products.Where(p => (allowedFFCProducts.IndexOf(p.Code) > -1) && !string.IsNullOrEmpty(p.ProdTotalPrice)).Sum(a => Convert.ToDouble(a.ProdTotalPrice));
                allowedFFCAmount += GetBundlesAmount(products);
                allowedFFCAmount = Math.Round(allowedFFCAmount, 2, MidpointRounding.AwayFromZero);
            }
            return allowedFFCAmount;
        }

        private double GetBundlesAmount(List<MOBProdDetail> products)
        {
            string nonBundleProductCode = _configuration.GetValue<string>("NonBundleProductCode");
            double bundleAmount = products == null ? 0 : products.Where(p => (nonBundleProductCode.IndexOf(p.Code) == -1) && !string.IsNullOrEmpty(p.ProdTotalPrice)).Sum(a => Convert.ToDouble(a.ProdTotalPrice));
            return bundleAmount;
        }

        private bool UpdateFFCAmountAsPerChangedPrice(MOBFOPTravelFutureFlightCredit travelFutureFlightCredit, List<MOBCPTraveler> travelersCSL, string sessionid, MOBApplication application)
        {
            bool isDirty = false;
            foreach (var traveler in travelersCSL)
            {
                var travelerFFCs = traveler.FutureFlightCredits;
                traveler.IndividualTotalAmount = Math.Round(traveler.IndividualTotalAmount, 2, MidpointRounding.AwayFromZero);
                if (travelerFFCs != null &&
                    travelerFFCs.Count > 0 &&
                    ((travelerFFCs.Sum(ffc => ffc.NewValueAfterRedeem) > 0)
                        ||
                        (!_configuration.GetValue<bool>("disable21GFFCToggle") && travelerFFCs.Sum(ffc => ffc.CurrentValue) > traveler.IndividualTotalAmount)
                    ) &&
                    travelerFFCs.Sum(ffc => ffc.RedeemAmount) != traveler.IndividualTotalAmount)
                {
                    isDirty = true;

                    double travelerIndividualTotalAmount = traveler.IndividualTotalAmount;
                    double ffcAppliedToTraveler = 0;
                    foreach (var travelerFFC in travelerFFCs)
                    {
                        if (travelerIndividualTotalAmount > 0)
                        {
                            travelerFFC.RedeemAmount = travelerIndividualTotalAmount > travelerFFC.CurrentValue ? travelerFFC.CurrentValue : travelerIndividualTotalAmount;
                            travelerFFC.RedeemAmount = Math.Round(travelerFFC.RedeemAmount, 2, MidpointRounding.AwayFromZero);
                            travelerFFC.DisplayRedeemAmount = (travelerFFC.RedeemAmount).ToString("C2", CultureInfo.CurrentCulture);
                            travelerFFC.NewValueAfterRedeem = travelerFFC.CurrentValue - travelerFFC.RedeemAmount;
                            travelerFFC.NewValueAfterRedeem = Math.Round(travelerFFC.NewValueAfterRedeem, 2, MidpointRounding.AwayFromZero);
                            travelerFFC.DisplayNewValueAfterRedeem = (travelerFFC.NewValueAfterRedeem).ToString("C2", CultureInfo.CurrentCulture);

                            travelerIndividualTotalAmount -= travelerFFC.RedeemAmount;
                            ffcAppliedToTraveler += travelerFFC.RedeemAmount;
                        }
                    }
                    travelerFFCs.RemoveAll(ffc => ffc.RedeemAmount == 0);
                    travelFutureFlightCredit.FutureFlightCredits.RemoveAll(ffc => (ffc.TravelerNameIndex == traveler.TravelerNameIndex && ffc.PaxId == traveler.PaxID));
                    if (travelerFFCs != null)
                    {
                        travelFutureFlightCredit.FutureFlightCredits.AddRange(travelerFFCs);
                        AssignTravelerTotalFFCNewValueAfterReDeem(traveler, application); ;                       
                    }
                }
                else if (travelerFFCs != null &&
                   travelerFFCs.Count > 0 && application != null && ConfigUtility.EnableMFOP(application.Id, application?.Version?.Major)
                   && traveler.FutureFlightCredits.Sum(ffc => ffc.RedeemAmount) > 0)
                    AssignTravelerTotalFFCReDeemAmount(traveler);
            }
            return isDirty;
        }

        private void AssignTravelerTotalFFCNewValueAfterReDeem(MOBCPTraveler traveler, MOBApplication application)
        {
            if (traveler.FutureFlightCredits?.Count > 0)
            {
                var sumOfNewValueAfterRedeem = traveler.FutureFlightCredits.Sum(ffc => ffc.NewValueAfterRedeem);
                if (sumOfNewValueAfterRedeem > 0)
                {
                    sumOfNewValueAfterRedeem = Math.Round(sumOfNewValueAfterRedeem, 2, MidpointRounding.AwayFromZero);
                    traveler.TotalFFCNewValueAfterRedeem = (sumOfNewValueAfterRedeem).ToString("C2", CultureInfo.CurrentCulture);
                }
                else
                {
                    traveler.TotalFFCNewValueAfterRedeem = "";
                }
            }
            else
            {
                traveler.TotalFFCNewValueAfterRedeem = "";
            }
            try
            {
                if (application != null && ConfigUtility.EnableMFOP(application.Id, application?.Version?.Major))
                    AssignTravelerTotalFFCReDeemAmount(traveler);
            }
            catch (Exception ex)
            {
                _logger.LogError("AssignTravelerTotalFFCReDeemAmount Failed {@message}", ex.Message);
                throw new MOBUnitedException(ex.Message);
            }
        }
        private void AssignTravelerTotalFFCReDeemAmount(MOBCPTraveler traveler)
        {
            if (traveler.FutureFlightCredits?.Count > 0)
            {
                var sumOfRedeemAmount = traveler.FutureFlightCredits.Sum(ffc => ffc.RedeemAmount);
                if (sumOfRedeemAmount > 0)
                {
                    sumOfRedeemAmount = Math.Round(sumOfRedeemAmount, 2, MidpointRounding.AwayFromZero);
                    traveler.TotalFFCRedeemAmount = sumOfRedeemAmount;
                    traveler.DisplayTotalFFCRedeemAmount = "-" + (sumOfRedeemAmount).ToString("C2", CultureInfo.CurrentCulture);
                }
                else
                {
                    traveler.TotalFFCRedeemAmount = null;
                    traveler.DisplayTotalFFCRedeemAmount = "";
                }
            }
            else
            {
                traveler.TotalFFCRedeemAmount = null;
                traveler.DisplayTotalFFCRedeemAmount = "";
            }
        }
        private void UpdatePricesInReservation(MOBFOPTravelFutureFlightCredit travelFutureFlightCredit, List<MOBSHOPPrice> prices, MOBApplication application, int appId = 0, string appVersion = "", bool isManageTravelCreditAPI = false, bool isCheckout = false)
        {
            if (ConfigUtility.EnableMFOP(application.Id, application.Version.Major))
            {
                UpdateFFCPricesInReservation(travelFutureFlightCredit, prices, application.Id, application.Version.Major, isManageTravelCreditAPI, isCheckout);
            }
            else
            {
                var ffcPrice = prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "FFC");
                var totalCreditFFC = prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "REFUNDPRICE");
                //var scRESProduct = response.ShoppingCart.Products.Find(p => p.Code == "RES");
                var grandtotal = prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "GRAND TOTAL");
                string maskedConfirmation = string.Empty;

                if (ffcPrice == null && travelFutureFlightCredit.TotalRedeemAmount > 0)
                {
                    ffcPrice = new MOBSHOPPrice();
                    prices.Add(ffcPrice);
                }
                else if (ffcPrice != null)
                {
                    UtilityHelper.UpdateCertificateRedeemAmountFromTotalInReserationPrices(grandtotal, ffcPrice.Value, false);
                }

                if (totalCreditFFC != null)
                    prices.Remove(totalCreditFFC);

                if (travelFutureFlightCredit.TotalRedeemAmount > 0)
                {
                    string priceTypeDescription = (ConfigUtility.IsFFCSummaryUpdated(appId, appVersion) && isManageTravelCreditAPI == false) ? _configuration.GetValue<string>("FFC_Applied") : "Future Flight Credit";
                    UpdateCertificatePrice(ffcPrice, travelFutureFlightCredit.TotalRedeemAmount, "FFC", priceTypeDescription, isAddNegative: true);
                    //Build Total Credit item
                    double totalCreditValue = travelFutureFlightCredit.FutureFlightCredits.Sum(ffc => ffc.NewValueAfterRedeem);
                    if (totalCreditValue > 0)
                    {
                        priceTypeDescription = UpdateRefundPricePriceTypeDescription(travelFutureFlightCredit, appId, appVersion, ref maskedConfirmation, isManageTravelCreditAPI);
                        totalCreditFFC = new MOBSHOPPrice();
                        prices.Add(totalCreditFFC);
                        UpdateCertificatePrice(totalCreditFFC, totalCreditValue, "REFUNDPRICE", "priceTypeDescription", "RESIDUALCREDIT");
                    }
                    UtilityHelper.UpdateCertificateRedeemAmountFromTotalInReserationPrices(grandtotal, travelFutureFlightCredit.TotalRedeemAmount);
                }
                else
                {
                    prices.Remove(ffcPrice);
                }
            }
        }

        private string UpdateRefundPricePriceTypeDescription(MOBFOPTravelFutureFlightCredit travelFutureFlightCredit, int appId, string appVersion, ref string maskedConfirmation, bool isManageTravelCreditAPI = false)
        {
            string priceTypeDescription = "Total credit";
            if (ConfigUtility.IsFFCSummaryUpdated(appId, appVersion) && isManageTravelCreditAPI == false)
            {
                priceTypeDescription = _configuration.GetValue<string>("FFC_RemainingText");
            }

            return priceTypeDescription;
        }
        private async System.Threading.Tasks.Task AssignETCFFCvalues(string sessionid, MOBShoppingCart shoppingCart, MOBRequest request, MOBFormofPaymentDetails formOfPaymentDetails, MOBSHOPReservation reservation, bool isCheckout)
        {
            try
            {
                if (shoppingCart.FormofPaymentDetails != null)
                {
                    formOfPaymentDetails.TravelCertificate = shoppingCart?.FormofPaymentDetails?.TravelCertificate;
                    formOfPaymentDetails.FormOfPaymentType = shoppingCart?.FormofPaymentDetails?.FormOfPaymentType;
                    formOfPaymentDetails.TravelFutureFlightCredit = shoppingCart?.FormofPaymentDetails?.TravelFutureFlightCredit;
                }
                if (formOfPaymentDetails.TravelCertificate == null)
                    formOfPaymentDetails.TravelCertificate = new MOBFOPTravelCertificate();
                if (formOfPaymentDetails.TravelFutureFlightCredit == null)
                    formOfPaymentDetails.TravelFutureFlightCredit = new MOBFOPTravelFutureFlightCredit();          

                if ((reservation.Prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "REFUNDFFCPRICE")?.Value > 0 && shoppingCart.FormofPaymentDetails?.TravelFutureFlightCredit?.FutureFlightCredits?.Count > 1)
                    || (reservation.Prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "REFUNDCERTIFICATEPRICE")?.Value > 0) && shoppingCart.FormofPaymentDetails?.TravelCertificate?.Certificates?.Count > 1)
                {
                    MOBCSLContentMessagesResponse lstMessages = null;
                    lstMessages = null;
                    var s = await _cachingService.GetCache<string>(_configuration.GetValue<string>("BookingPathRTI_CMSContentMessagesCached_StaticGUID") + ObjectNames.MOBCSLContentMessagesResponseFullName, _headers.ContextValues.TransactionId).ConfigureAwait(false);
                    lstMessages = JsonConvert.DeserializeObject<MOBCSLContentMessagesResponse>(s);
                    List<MOBMobileCMSContentMessages> msg = GetSDLMessageFromList(lstMessages.Messages, "RTI.TravelCredits.EmailMSG");
                    msg[0].Title = reservation.Prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "REFUNDFFCPRICE").Value > 0 ? "RTI.FutureFlightCredits.EmailConfirmation" : "RTI.TravelCredits.EmailConfirmation";
                    msg[0].ContentFull = string.Format(msg[0].ContentFull, formOfPaymentDetails.EmailAddress);
                    msg[0].ContentShort = formOfPaymentDetails.TravelFutureFlightCredit.EmailConfirmationFFCMessages[0].ContentFull;
                    formOfPaymentDetails.TravelFutureFlightCredit.EmailConfirmationFFCMessages = reservation.Prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "REFUNDFFCPRICE")?.Value > 0 ? msg : null;
                    formOfPaymentDetails.TravelCertificate.EmailConfirmationTCMessages = reservation.Prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "REFUNDCERTIFICATEPRICE")?.Value > 0 ? msg : null;
                }
                else
                {
                    formOfPaymentDetails.TravelFutureFlightCredit.EmailConfirmationFFCMessages = null;
                    formOfPaymentDetails.TravelCertificate.EmailConfirmationTCMessages = null;
                }
                if ((shoppingCart.FormofPaymentDetails?.TravelCertificate?.Certificates?.Count > 0) || (shoppingCart.FormofPaymentDetails?.TravelFutureFlightCredit?.FutureFlightCredits?.Count > 0))
                {
                    Reservation bookingPathReservation = new Reservation();
                    bookingPathReservation = await _sessionHelperService.GetSession<Reservation>(sessionid, bookingPathReservation.ObjectName, new List<string> { sessionid, bookingPathReservation.ObjectName }).ConfigureAwait(false);
                    bookingPathReservation.Prices = reservation.Prices;
                    await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, sessionid, new List<string> { sessionid, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);
                }
            }
            catch (Exception ex) { }
        }
        private void UpdateETCPricesInReservation(MOBFOPTravelCertificate travelCertificate, List<MOBSHOPPrice> prices, int appId = 0, string appVersion = "", bool isManageTravelCreditAPI = false, bool isCheckout = false)
        {
            if (travelCertificate != null && travelCertificate?.Certificates?.Count > 0)
            {
                var etcPrice = prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "CERTIFICATE");
                var totalCreditValue = prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "REFUNDCERTIFICATEPRICE");
                string maskedConfirmation = string.Empty;
                string priceTypeDescription = isCheckout ? _configuration.GetValue<string>("ETC_Applied") : "Travel certificate";

                if (totalCreditValue != null)
                    prices.Remove(totalCreditValue);

                if (etcPrice != null && etcPrice?.Value > 0)
                    UpdateCertificatePrice(etcPrice, etcPrice.Value, "CERTIFICATE", priceTypeDescription, "");
                else
                    prices.Remove(etcPrice);

                if (totalCreditValue != null && totalCreditValue.Value > 0)
                {
                    priceTypeDescription = UpdateETCRefundPricePriceTypeDescription(travelCertificate, appId, appVersion, ref maskedConfirmation, isManageTravelCreditAPI);
                    var totalCreditETC = new MOBSHOPPrice();
                    prices.Add(totalCreditETC);
                    UpdateCertificatePrice(totalCreditETC, totalCreditValue.Value, "REFUNDCERTIFICATEPRICE", priceTypeDescription, "RESIDUALCREDIT");
                }
            }
            else
            {
                prices.Remove(prices.FirstOrDefault(x => x.DisplayType.ToUpper() == "REFUNDCERTIFICATEPRICE"));
            }
        }
        private void UpdateFFCPricesInReservation(MOBFOPTravelFutureFlightCredit travelFutureFlightCredit, List<MOBSHOPPrice> prices, int appId = 0, string appVersion = "", bool isManageTravelCreditAPI = false, bool isCheckout = false)
        {
            if (travelFutureFlightCredit != null && travelFutureFlightCredit?.FutureFlightCredits?.Count > 0)
            {
                var FFCPrice = prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "FFC");
                var totalCreditValue = prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "REFUNDFFCPRICE");
                //var grandtotal = prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "GRAND TOTAL");
                string maskedConfirmation = string.Empty;
                string priceTypeDescription = isCheckout ? _configuration.GetValue<string>("FFC_Applied") : "Future flight credit";

                if (totalCreditValue != null)
                    prices.Remove(totalCreditValue);

                if (FFCPrice != null && FFCPrice?.Value > 0)
                    UpdateCertificatePrice(FFCPrice, FFCPrice.Value, "FFC", priceTypeDescription, "");
                else
                    prices.Remove(FFCPrice);

                if (totalCreditValue != null && totalCreditValue?.Value > 0)
                {
                    priceTypeDescription = UpdateRefundPricePriceTypeDescription(travelFutureFlightCredit, appId, appVersion, ref maskedConfirmation, isManageTravelCreditAPI);
                    var totalCreditETC = new MOBSHOPPrice();
                    prices.Add(totalCreditETC);
                    UpdateCertificatePrice(totalCreditETC, totalCreditValue.Value, "REFUNDFFCPRICE", priceTypeDescription, "RESIDUALCREDIT");

                }
            }
            else
            {
                prices.Remove(prices.FirstOrDefault(x => x.DisplayType.ToUpper() == "REFUNDFFCPRICE"));
            }

        }

        private string UpdateETCRefundPricePriceTypeDescription(MOBFOPTravelCertificate travelCertificate, int appId, string appVersion, ref string maskedConfirmation, bool isManageTravelCreditAPI = false)
        {
            string priceTypeDescription = "Total credit";
            if (ConfigUtility.IsFFCSummaryUpdated(appId, appVersion) && isManageTravelCreditAPI == false)
            {
                priceTypeDescription = _configuration.GetValue<string>("ETC_RemainingText");
            }

            return priceTypeDescription;
        }

        public void AssignIsOtherFOPRequired(MOBFormofPaymentDetails formofPaymentDetails, List<MOBSHOPPrice> prices)
        {
            var grandTotalPrice = prices.FirstOrDefault(p => p.DisplayType.ToUpper().Equals("GRAND TOTAL"));
            if (grandTotalPrice != null)
                formofPaymentDetails.IsOtherFOPRequired = (grandTotalPrice.Value > 0);
        }

        private void AssignCertificateTravelers(MOBShoppingCart shoppingCart)
        {
            if (shoppingCart?.SCTravelers != null)
            {
                shoppingCart.CertificateTravelers = new List<MOBFOPCertificateTraveler>();

                foreach (var traveler in shoppingCart.SCTravelers)
                {
                    if (traveler.IndividualTotalAmount > 0)
                    {
                        MOBFOPCertificateTraveler certificateTraveler = new MOBFOPCertificateTraveler();
                        certificateTraveler.Name = traveler.FirstName + " " + traveler.LastName;
                        certificateTraveler.TravelerNameIndex = traveler.TravelerNameIndex;
                        certificateTraveler.PaxId = traveler.PaxID;
                        certificateTraveler.IndividualTotalAmount = traveler.IndividualTotalAmount;
                        shoppingCart.CertificateTravelers.Add(certificateTraveler);
                    }
                }
            }
        }

        private async Task<MOBShoppingCart> InitialiseShoppingCartAndDevfaultValuesForETC(MOBShoppingCart shoppingcart, List<MOBProdDetail> products, string flow)
        {
            if (shoppingcart == null)
            {
                shoppingcart = new MOBShoppingCart();
            }
            if (shoppingcart.FormofPaymentDetails == null)
            {
                shoppingcart.FormofPaymentDetails = new MOBFormofPaymentDetails();
            }
            if (shoppingcart.FormofPaymentDetails.TravelCertificate == null)
            {
                shoppingcart.FormofPaymentDetails.TravelCertificate = new MOBFOPTravelCertificate();
                shoppingcart.FormofPaymentDetails.TravelCertificate.AllowedETCAmount = ConfigUtility.GetAlowedETCAmount(shoppingcart.Products ?? products, (string.IsNullOrEmpty(shoppingcart.Flow) ? flow : shoppingcart.Flow));
                shoppingcart.FormofPaymentDetails.TravelCertificate.NotAllowedETCAmount = GetNotAlowedETCAmount(products, (string.IsNullOrEmpty(shoppingcart.Flow) ? flow : shoppingcart.Flow));
                shoppingcart.FormofPaymentDetails.TravelCertificate.MaxAmountOfETCsAllowed = Convert.ToDouble(_configuration.GetValue<string>("CombinebilityMaxAmountOfETCsAllowed"));
                shoppingcart.FormofPaymentDetails.TravelCertificate.MaxNumberOfETCsAllowed = _configuration.GetValue<int>("CombinebilityMaxNumberOfETCsAllowed");
                shoppingcart.FormofPaymentDetails.TravelCertificate.ReviewETCMessages = await AssignAlertMessages("TravelCertificate_Combinability_ReviewETCAlertMsg");
                shoppingcart.FormofPaymentDetails.TravelCertificate.SavedETCMessages = await AssignAlertMessages("TravelCertificate_Combinability_SavedETCListAlertMsg");
                string removeAllCertificatesAlertMessage = _configuration.GetValue<string>("RemoveAllTravelCertificatesAlertMessage");
                shoppingcart.FormofPaymentDetails.TravelCertificate.RemoveAllCertificateAlertMessage = new MOBSection { Text1 = removeAllCertificatesAlertMessage, Text2 = "Cancel", Text3 = "Continue" };
                await SetETCTraveCreditsReviewAlertMessage(shoppingcart.FormofPaymentDetails.TravelCertificate.ReviewETCMessages);
            }
            return shoppingcart;
        }

        private double GetNotAlowedETCAmount(List<MOBProdDetail> products, string flow)
        {
            return products.Sum(a => Convert.ToDouble(a.ProdTotalPrice)) - ConfigUtility.GetAlowedETCAmount(products, flow);
        }

        private async Task AssignTravelerCertificateToFOP(MOBFOPTravelerCertificateResponse persistedTravelCertifcateResponse, List<MOBProdDetail> products, string flow)
        {
            if (persistedTravelCertifcateResponse == null)
            {
                persistedTravelCertifcateResponse = new MOBFOPTravelerCertificateResponse();
            }
            persistedTravelCertifcateResponse.ShoppingCart = await InitialiseShoppingCartAndDevfaultValuesForETC(persistedTravelCertifcateResponse.ShoppingCart, products, flow);
        }

        private void AssignCertificateTravelers(MOBShoppingCart shoppingCart, MOBFOPTravelerCertificateResponse persistedTravelCertifcateResponse, List<MOBSHOPPrice> prices, MOBApplication application)
        {
            List<MOBFOPCertificateTraveler> certTravelersCopy = null;
            if (persistedTravelCertifcateResponse?.ShoppingCart?.CertificateTravelers != null)
            {
                certTravelersCopy = persistedTravelCertifcateResponse.ShoppingCart.CertificateTravelers;
            }

            if (shoppingCart?.SCTravelers != null)
            {
                shoppingCart.CertificateTravelers = new List<MOBFOPCertificateTraveler>();
                if (shoppingCart.SCTravelers.Count > 1)
                {
                    AddAllTravelersOptionInCertificateTravelerList(shoppingCart);
                }
                foreach (var traveler in shoppingCart.SCTravelers)
                {
                    if (traveler.IndividualTotalAmount > 0)
                    {
                        MOBFOPCertificateTraveler certificateTraveler = new MOBFOPCertificateTraveler();
                        certificateTraveler.Name = traveler.FirstName + " " + traveler.LastName;
                        certificateTraveler.TravelerNameIndex = traveler.TravelerNameIndex;
                        certificateTraveler.PaxId = traveler.PaxID;
                        MOBFOPCertificateTraveler persistTraveler = certTravelersCopy?.Find(ct => ct.Name == traveler.FirstName + " " + traveler.LastName && traveler.PaxID == ct.PaxId);
                        if (persistTraveler != null)
                        {
                            certificateTraveler.IsCertificateApplied = persistTraveler.IsCertificateApplied;
                        }
                        else
                        {
                            certificateTraveler.IsCertificateApplied = false;
                        }
                        certificateTraveler.IndividualTotalAmount = traveler.IndividualTotalAmount;
                        shoppingCart.CertificateTravelers.Add(certificateTraveler);
                    }
                }

                if (!IsETCCombinabilityEnabled(application.Id, application.Version.Major) &&
                    persistedTravelCertifcateResponse?.ShoppingCart?.FormofPaymentDetails?.TravelCertificate?.Certificates != null && shoppingCart.SCTravelers.Count > 1 &&
                    persistedTravelCertifcateResponse.ShoppingCart.FormofPaymentDetails.TravelCertificate.Certificates.Exists(c => c.CertificateTraveler != null &&
                                                                                                      !string.IsNullOrEmpty(c.CertificateTraveler.TravelerNameIndex))
                    )
                {
                    ClearUnmatchedCertificatesAfterEditTravelers(shoppingCart, persistedTravelCertifcateResponse, prices);
                }
            }
        }

        private void UpdateCertificateAmountInTotalPrices(List<MOBSHOPPrice> prices, List<MOBProdDetail> scProducts, double certificateTotalAmount, bool isShoppingCartProductsGotRefresh = false)
        {
            var certificatePrice = prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "CERTIFICATE");
            var scRESProduct = scProducts.Find(p => p.Code == "RES");
            //var total = prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "TOTAL");
            var grandtotal = prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "GRAND TOTAL");
            if (certificatePrice == null && certificateTotalAmount > 0)
            {
                certificatePrice = new MOBSHOPPrice();
                ConfigUtility.UpdateCertificatePrice(certificatePrice, certificateTotalAmount);
                prices.Add(certificatePrice);
            }
            else if (certificatePrice != null)
            {
                //this two lines adding certificate price back to total for removing latest certificate amount in next lines
                if (!isShoppingCartProductsGotRefresh)
                {
                    UtilityHelper.UpdateCertificateRedeemAmountInSCProductPrices(scRESProduct, certificatePrice.Value, false);
                }
                UtilityHelper.UpdateCertificateRedeemAmountFromTotalInReserationPrices(grandtotal, certificatePrice.Value, false);
                if (_configuration.GetValue<bool>("MTETCToggle"))
                {
                    ConfigUtility.UpdateCertificatePrice(certificatePrice, certificateTotalAmount);
                }
            }

            if (certificateTotalAmount == 0 && certificatePrice != null)
            {
                prices.Remove(certificatePrice);
            }

            //UpdateCertificateRedeemAmountFromTotal(total, certificateTotalAmount);
            UtilityHelper.UpdateCertificateRedeemAmountInSCProductPrices(scRESProduct, certificateTotalAmount);
            UtilityHelper.UpdateCertificateRedeemAmountFromTotalInReserationPrices(grandtotal, certificateTotalAmount);
        }

        private bool IsInclueWithThisToggle(int appId, string appVersion, string configToggleKey, string androidVersion, string iosVersion)
        {
            return _configuration.GetValue<bool>(configToggleKey) &&
                   GeneralHelper.isApplicationVersionGreater(appId, appVersion, androidVersion, iosVersion, "", "", true, _configuration);
        }

        private void ApplyFFCToAncillary(List<MOBProdDetail> products, MOBApplication application, MOBFormofPaymentDetails mobFormofPaymentDetails, List<MOBSHOPPrice> prices, bool isAncillaryON = false, bool isManageTravelCreditApi = false, bool isCheckout = false)
        {
            bool isAncillaryFFCEnable = (application == null ? isAncillaryON : IsInclueWithThisToggle(application.Id, application.Version.Major, "EnableTravelCreditAncillary", "AndroidTravelCreditVersionAncillary", "iPhoneTravelCreditVersionAncillary"));
            var futureFlightCredits = mobFormofPaymentDetails.TravelFutureFlightCredit?.FutureFlightCredits;

            if (isAncillaryFFCEnable && futureFlightCredits?.Count > 0)
            {
                mobFormofPaymentDetails.TravelFutureFlightCredit.AllowedFFCAmount = GetAllowedFFCAmount(products, isAncillaryFFCEnable);
                mobFormofPaymentDetails.TravelFutureFlightCredit.AllowedAncillaryAmount = GetAncillaryAmount(products, isAncillaryFFCEnable);

                var travelCredits = mobFormofPaymentDetails.TravelCreditDetails.TravelCredits.Where(tc => futureFlightCredits.Exists(ffc => ffc.PinCode == tc.PinCode)).ToList();
                int index = 0;

                foreach (var travelCredit in travelCredits)
                {
                    double ffcAppliedToAncillary = 0;
                    ffcAppliedToAncillary = futureFlightCredits.Where(ffc => ffc.TravelerNameIndex == "ANCILLARY").Sum(t => t.RedeemAmount);
                    ffcAppliedToAncillary = Math.Round(ffcAppliedToAncillary, 2, MidpointRounding.AwayFromZero);
                    var existedFFC = futureFlightCredits.FirstOrDefault(f => f.TravelerNameIndex == "ANCILLARY" && f.PinCode == travelCredit.PinCode);
                    double alreadyAppliedAmount = futureFlightCredits.Where(f => f.PinCode == travelCredit.PinCode).Sum(p => p.RedeemAmount);
                    var balanceAfterAppliedToRESAndAncillary = travelCredit.CurrentValue - alreadyAppliedAmount;

                    if (balanceAfterAppliedToRESAndAncillary > 0 &&
                        ffcAppliedToAncillary < mobFormofPaymentDetails.TravelFutureFlightCredit?.AllowedAncillaryAmount &&
                        existedFFC == null)
                    {
                        index++;
                        var mobFFC = new MOBFOPFutureFlightCredit();
                        mobFFC.CreditAmount = travelCredit.CreditAmount;
                        mobFFC.ExpiryDate = Convert.ToDateTime(travelCredit.ExpiryDate).ToString("MMMMM dd, yyyy");
                        mobFFC.IsCertificateApplied = true;
                        mobFFC.InitialValue = travelCredit.InitialValue;
                        mobFFC.Index = index;
                        mobFFC.PinCode = travelCredit.PinCode;
                        mobFFC.PromoCode = travelCredit.PromoCode;
                        mobFFC.RecordLocator = travelCredit.RecordLocator;
                        mobFFC.TravelerNameIndex = "ANCILLARY";
                        double remainingBalanceAfterAppliedFFC = mobFormofPaymentDetails.TravelFutureFlightCredit.AllowedAncillaryAmount - ffcAppliedToAncillary;
                        mobFFC.RedeemAmount = remainingBalanceAfterAppliedFFC > balanceAfterAppliedToRESAndAncillary ? balanceAfterAppliedToRESAndAncillary : remainingBalanceAfterAppliedFFC;
                        mobFFC.RedeemAmount = Math.Round(mobFFC.RedeemAmount, 2, MidpointRounding.AwayFromZero);
                        mobFFC.DisplayRedeemAmount = (mobFFC.RedeemAmount).ToString("C2", CultureInfo.CurrentCulture);
                        mobFFC.NewValueAfterRedeem = travelCredit.CurrentValue - (mobFFC.RedeemAmount + alreadyAppliedAmount);
                        mobFFC.NewValueAfterRedeem = Math.Round(mobFFC.NewValueAfterRedeem, 2, MidpointRounding.AwayFromZero);
                        mobFFC.DisplayNewValueAfterRedeem = (mobFFC.NewValueAfterRedeem).ToString("C2", CultureInfo.CurrentCulture);
                        mobFFC.IsCertificateApplied = true;
                        mobFFC.CurrentValue = travelCredit.CurrentValue;
                        futureFlightCredits.Add(mobFFC);
                    }
                    else if (existedFFC != null)
                    {
                        double remainingBalanceAfterAppliedFFC = (mobFormofPaymentDetails.TravelFutureFlightCredit.AllowedAncillaryAmount - ffcAppliedToAncillary) + existedFFC.RedeemAmount;
                        existedFFC.NewValueAfterRedeem += existedFFC.RedeemAmount;
                        existedFFC.RedeemAmount = 0;
                        existedFFC.RedeemAmount = remainingBalanceAfterAppliedFFC > existedFFC.NewValueAfterRedeem ? existedFFC.NewValueAfterRedeem : remainingBalanceAfterAppliedFFC;
                        existedFFC.RedeemAmount = Math.Round(existedFFC.RedeemAmount, 2, MidpointRounding.AwayFromZero);
                        existedFFC.DisplayRedeemAmount = (existedFFC.RedeemAmount).ToString("C2", CultureInfo.CurrentCulture);
                        existedFFC.NewValueAfterRedeem -= existedFFC.RedeemAmount;
                        existedFFC.NewValueAfterRedeem = Math.Round(existedFFC.NewValueAfterRedeem, 2, MidpointRounding.AwayFromZero);
                        existedFFC.DisplayNewValueAfterRedeem = (existedFFC.NewValueAfterRedeem).ToString("C2", CultureInfo.CurrentCulture);
                    }

                    futureFlightCredits.RemoveAll(f => f.RedeemAmount <= 0);
                    UpdatePricesInReservation(mobFormofPaymentDetails.TravelFutureFlightCredit, prices, application, application.Id, application.Version.Major, isManageTravelCreditApi, isCheckout);
                    AssignIsOtherFOPRequired(mobFormofPaymentDetails, prices);
                    //TODO
                    AssignFormOfPaymentType(mobFormofPaymentDetails, prices, false);
                }
            }
        }

        private void UpdateTravelCreditAmountWithSelectedETCOrFFC(MOBFormofPaymentDetails formofPaymentDetails, List<MOBSHOPPrice> prices, List<MOBCPTraveler> travelers, MOBApplication application, bool isManageTravelCreditApi = false, bool isCheckout = false)
        {
            if (formofPaymentDetails?.TravelCreditDetails?.TravelCredits?.Count > 0)
            {
                bool isETC = (formofPaymentDetails?.TravelCertificate?.Certificates?.Count > 0);
                bool isFFC = (formofPaymentDetails?.TravelFutureFlightCredit?.FutureFlightCredits?.Count > 0);
                bool isTravelerFFC_Check = !_configuration.GetValue<bool>("DisableMPSignedInInsertUpdateTraveler");
                foreach (var travelCredit in formofPaymentDetails.TravelCreditDetails.TravelCredits)
                {
                    double redeemAmount = 0;
                    if (isETC)
                    {
                        var cert = formofPaymentDetails.TravelCertificate.Certificates.Where(c => c.PinCode == travelCredit.PinCode).ToList();
                        if (cert != null)
                        {
                            redeemAmount = cert.Sum(c => c.RedeemAmount);
                        }
                    }

                    if (isFFC)
                    {
                        var ffcs = formofPaymentDetails.TravelFutureFlightCredit.FutureFlightCredits.Where(c => c.PinCode == travelCredit.PinCode).ToList();
                        if (ffcs != null)
                        {
                            redeemAmount = ffcs.Sum(c => c.RedeemAmount);
                        }
                    }
                    UpdateTravelCreditRedeemAmount(travelCredit, redeemAmount);
                }
                if (isFFC)
                {
                    IEnumerable<MOBFOPTravelCredit> ancillaryTCs = formofPaymentDetails.TravelCreditDetails.TravelCredits.Where(tc => tc.IsApplied);
                    foreach (var ancillaryTC in ancillaryTCs)
                    {
                        foreach (var scTraveler in isTravelerFFC_Check ? travelers.Where(trav => trav.FutureFlightCredits != null).Where(tc => tc.FutureFlightCredits.Exists(f => f.PinCode == ancillaryTC.PinCode)) : travelers.Where(trav => trav.FutureFlightCredits.Exists(f => f.PinCode == ancillaryTC.PinCode)))
                        {
                            var travelerFFC = scTraveler.FutureFlightCredits.FirstOrDefault(ffc => ffc.PinCode == ancillaryTC.PinCode);
                            if (travelerFFC != null)
                            {
                                travelerFFC.NewValueAfterRedeem = 0;
                                travelerFFC.NewValueAfterRedeem = Math.Round(travelerFFC.NewValueAfterRedeem, 2, MidpointRounding.AwayFromZero);
                                travelerFFC.DisplayNewValueAfterRedeem = (travelerFFC.NewValueAfterRedeem).ToString("C2", CultureInfo.CurrentCulture);

                                AssignTravelerTotalFFCNewValueAfterReDeem(scTraveler, application);
                            }
                        }
                        foreach (var tnIndex in ancillaryTC.EligibleTravelers)
                        {
                            MOBCPTraveler traveler = isTravelerFFC_Check ? travelers.Where(tc => tc.FutureFlightCredits != null).FirstOrDefault(trav => trav.TravelerNameIndex == tnIndex) : travelers.FirstOrDefault(trav => trav.TravelerNameIndex == tnIndex);
                            var travelerFFC = traveler.FutureFlightCredits.FirstOrDefault(ffc => ffc.PinCode == ancillaryTC.PinCode);
                            if (travelerFFC != null)
                            {
                                travelerFFC.NewValueAfterRedeem = ancillaryTC.NewValueAfterRedeem / ancillaryTC.EligibleTravelers.Count;
                                travelerFFC.NewValueAfterRedeem = Math.Round(travelerFFC.NewValueAfterRedeem, 2, MidpointRounding.AwayFromZero);
                                travelerFFC.DisplayNewValueAfterRedeem = (travelerFFC.NewValueAfterRedeem).ToString("C2", CultureInfo.CurrentCulture);

                                AssignTravelerTotalFFCNewValueAfterReDeem(traveler, application);
                            }
                        }                        
                    }

                    var newRefundValueAfterReeedeem = ancillaryTCs.Sum(tcAmount => tcAmount.NewValueAfterRedeem);
                    var refundPrice = prices.FirstOrDefault(p => p.PriceType == "REFUNDPRICE");
                    if (refundPrice != null)
                    {
                        if (newRefundValueAfterReeedeem > 0)
                        {
                            string priceTypeDescription = UpdateRefundPricePriceDescription(application.Id, application.Version.Major, ancillaryTCs, isManageTravelCreditApi);
                            UpdateCertificatePrice(refundPrice, newRefundValueAfterReeedeem, "REFUNDPRICE", priceTypeDescription, "RESIDUALCREDIT");
                        }
                        else
                        {
                            prices.RemoveAll(p => p.PriceType == "REFUNDPRICE");
                        }
                    }
                }
            }
        }

        private string UpdateRefundPricePriceDescription(int appId, string appVersion, IEnumerable<MOBFOPTravelCredit> ancillaryTCs, bool isManageTravelCreditApi = false)
        {
            string priceTypeDescription = "Total credit";
            if (ConfigUtility.IsFFCSummaryUpdated(appId, appVersion) && isManageTravelCreditApi == false)
            {
                priceTypeDescription = _configuration.GetValue<string>("FFC_RemainingText");
            }

            return priceTypeDescription;
        }

        private List<MOBMobileCMSContentMessages> BuildReviewFFCHeaderMessage(MOBFOPTravelFutureFlightCredit travelFutureFlightCredit, List<MOBCPTraveler> travelers, List<CMSContentMessage> lstMessages)
        {
            List<MOBMobileCMSContentMessages> ffcHeaderMessage = null;
            if (travelFutureFlightCredit?.FutureFlightCredits?.Count() > 1 && travelFutureFlightCredit.FutureFlightCredits.Sum(ffc => ffc.NewValueAfterRedeem) > 0)
            {
                ffcHeaderMessage = GetSDLMessageFromList(lstMessages, "RTI.FutureFlightCredits.ReviewFFC");
                string contentFullMessage = ffcHeaderMessage[0].ContentFull;
                string travlerBalanceFFCMessageTemplete = "{0}:{1}, travel-by date {2}";
                string travlerBalanceFFCMessage = "";
                foreach (var traveler in travelers)
                {
                    if (traveler.FutureFlightCredits != null && traveler.FutureFlightCredits.Count > 1 && traveler.FutureFlightCredits.Sum(ffc => ffc.NewValueAfterRedeem) > 0)
                    {
                        travlerBalanceFFCMessage += (travlerBalanceFFCMessage != "" ? " and " : "") + string.Format(travlerBalanceFFCMessageTemplete,
                                                                                traveler.FirstName + " " + traveler.LastName,
                                                                                traveler.FutureFlightCredits.Sum(ffc => ffc.NewValueAfterRedeem).ToString("C2", CultureInfo.CurrentCulture),
                                                                                traveler.FutureFlightCredits.Min(ffc => Convert.ToDateTime(ffc.ExpiryDate).ToString("MM/dd/yyyy")));
                    }
                }
                if (!string.IsNullOrEmpty(travlerBalanceFFCMessage))
                {
                    ffcHeaderMessage[0].ContentFull = string.Format(ffcHeaderMessage[0].ContentFull, travlerBalanceFFCMessage);
                }
                else
                {
                    ffcHeaderMessage = null;
                }
            }

            return ffcHeaderMessage;
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

        private void IsHidePromoOption(MOBShoppingCart shoppingCart)
        {
            bool isTravelCertificateAdded = shoppingCart?.FormofPaymentDetails?.TravelCertificate != null && shoppingCart.FormofPaymentDetails.TravelCertificate.Certificates != null && shoppingCart.FormofPaymentDetails.TravelCertificate.Certificates.Count > 0;
            bool isCouponAdded = (shoppingCart?.PromoCodeDetails?.PromoCodes != null && shoppingCart.PromoCodeDetails.PromoCodes.Count > 0);
            if (shoppingCart?.Products != null && shoppingCart.Products.Any(p => p?.Code?.ToUpper() == "FARELOCK" || p?.Code?.ToUpper() == "FLK"))
            {
                shoppingCart.PromoCodeDetails.IsHidePromoOption = true;
                return;
            }

            if (!isCouponAdded && shoppingCart?.FormofPaymentDetails?.FormOfPaymentType != null && (_configuration.GetValue<string>("Fops_HidePromoOption").Contains(shoppingCart?.FormofPaymentDetails?.FormOfPaymentType)
                || (_configuration.GetValue<string>("Fops_HidePromoOption").Contains("ETC") && isTravelCertificateAdded)))
            {
                shoppingCart.PromoCodeDetails.IsHidePromoOption = true;
            }
            else
            {
                shoppingCart.PromoCodeDetails.IsHidePromoOption = false;
            }
        }

        private void AddAllTravelersOptionInCertificateTravelerList(MOBShoppingCart shoppingCart)
        {
            MOBFOPCertificateTraveler certificateTraveler = new MOBFOPCertificateTraveler
            {
                Name = "Apply to all travelers",
                TravelerNameIndex = "0"
            };

            if (shoppingCart?.FormofPaymentDetails?.TravelCertificate?.Certificates != null)
            {
                certificateTraveler.IsCertificateApplied = shoppingCart.FormofPaymentDetails.TravelCertificate.Certificates.Count() > 0;
            }
            else
            {
                certificateTraveler.IsCertificateApplied = false;
            }

            if (shoppingCart.Products.Exists(p => p.Code == "RES"))
            {
                var scRESProduct = shoppingCart.Products.Find(p => p.Code == "RES");
                certificateTraveler.IndividualTotalAmount = Math.Round(Convert.ToDouble(scRESProduct.ProdTotalPrice), 2, MidpointRounding.AwayFromZero);
            }
            shoppingCart.CertificateTravelers.Add(certificateTraveler);
        }

        private bool IsETCCombinabilityEnabled(int applicationId, string appVersion)
        {
            if (_configuration.GetValue<bool>("CombinebilityETCToggle") && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Android_EnableETCCombinability_AppVersion"), _configuration.GetValue<string>("iPhone_EnableETCCombinability_AppVersion")))
            {
                return true;
            }

            return false;
        }

        private void ClearUnmatchedCertificatesAfterEditTravelers(MOBShoppingCart shoppingCart, MOBFOPTravelerCertificateResponse persistedTravelCertifcateResponse, List<MOBSHOPPrice> prices)
        {
            List<MOBFOPCertificate> removeCertificates = new List<MOBFOPCertificate>();
            foreach (var certificate in persistedTravelCertifcateResponse.ShoppingCart.FormofPaymentDetails.TravelCertificate.Certificates)
            {
                if (certificate.CertificateTraveler?.TravelerNameIndex == "0")
                {
                    foreach (var sctraveler in shoppingCart.SCTravelers)
                    {
                        var persistResponseTraveler = persistedTravelCertifcateResponse.ShoppingCart.SCTravelers.Find(st => st.PaxID == sctraveler.PaxID);
                        if (persistResponseTraveler == null || IsValuesChangedForSameTraveler(sctraveler, persistResponseTraveler))
                        {
                            removeCertificates.Add(certificate);
                        }
                    }
                }
                else if (shoppingCart.CertificateTravelers != null && !shoppingCart.CertificateTravelers.Exists(ct => ct.PaxId == certificate.CertificateTraveler.PaxId && ct.IsCertificateApplied))
                {
                    removeCertificates.Add(certificate);
                }
                else
                {
                    var scTraveler = shoppingCart.SCTravelers.Find(st => st.PaxID == certificate.CertificateTraveler.PaxId);
                    var persistResponseTraveler = persistedTravelCertifcateResponse.ShoppingCart.SCTravelers.Find(st => st.PaxID == certificate.CertificateTraveler.PaxId);
                    if (scTraveler != null && persistResponseTraveler != null && IsValuesChangedForSameTraveler(scTraveler, persistResponseTraveler))
                    {
                        removeCertificates.Add(certificate);
                    }
                }
            }
            if (removeCertificates.Count > 0)
            {
                foreach (var removeCertificate in removeCertificates)
                {
                    var scRemovedOrEditTraveler = shoppingCart.CertificateTravelers?.Find(ct => ct.PaxId == removeCertificate.CertificateTraveler.PaxId);
                    if (scRemovedOrEditTraveler != null)
                    {
                        scRemovedOrEditTraveler.IsCertificateApplied = false;
                    }
                }
                persistedTravelCertifcateResponse.ShoppingCart.FormofPaymentDetails.TravelCertificate.Certificates.RemoveAll(c => removeCertificates.Contains(c));
                SelectAllTravelersAndAssignIsCertificateApplied(shoppingCart.CertificateTravelers, persistedTravelCertifcateResponse.ShoppingCart.FormofPaymentDetails.TravelCertificate.Certificates);
            }
            persistedTravelCertifcateResponse.ShoppingCart.CertificateTravelers = shoppingCart.CertificateTravelers;
        }

        private void SelectAllTravelersAndAssignIsCertificateApplied(List<MOBFOPCertificateTraveler> certTravelersCopy, List<MOBFOPCertificate> certificates)
        {
            var allTraveler = certTravelersCopy.Find(ct => ct.TravelerNameIndex == "0");
            if (allTraveler != null)
            {
                allTraveler.IsCertificateApplied = certificates.Count > 0;
            }
        }

        private void UpdateTravelCreditRedeemAmount(MOBFOPTravelCredit travelCredit, double redeemAmount)
        {
            //mahesh
            travelCredit.RedeemAmount = redeemAmount;
            travelCredit.RedeemAmount = Math.Round(travelCredit.RedeemAmount, 2, MidpointRounding.AwayFromZero);
            travelCredit.DisplayRedeemAmount = (redeemAmount).ToString("C2", CultureInfo.CurrentCulture);
            travelCredit.NewValueAfterRedeem = travelCredit.CurrentValue - (!_configuration.GetValue<bool>("FFC_NegativeValue") ? travelCredit.RedeemAmount : redeemAmount);
            travelCredit.NewValueAfterRedeem = Math.Round(travelCredit.NewValueAfterRedeem, 2, MidpointRounding.AwayFromZero);
            travelCredit.DisplayNewValueAfterRedeem = (travelCredit.NewValueAfterRedeem).ToString("C2", CultureInfo.CurrentCulture);
            travelCredit.IsApplied = redeemAmount > 0;
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

        private bool IsValuesChangedForSameTraveler(MOBCPTraveler requestTraveler, MOBCPTraveler travelerCSLBeforeRegister)
        {
            return requestTraveler.FirstName != travelerCSLBeforeRegister.FirstName ||
                   requestTraveler.LastName != travelerCSLBeforeRegister.LastName ||
                   requestTraveler.BirthDate != travelerCSLBeforeRegister.BirthDate ||
                   requestTraveler.GenderCode != travelerCSLBeforeRegister.GenderCode ||
                   (requestTraveler.Nationality ?? "") != (travelerCSLBeforeRegister.Nationality ?? "") ||
                   (requestTraveler.CountryOfResidence ?? "") != (travelerCSLBeforeRegister.CountryOfResidence ?? "");
        }


        public string GetBookingPaymentTargetForRegisterFop(FlightReservationResponse flightReservationResponse)
        {
            if (flightReservationResponse.ShoppingCart == null || !flightReservationResponse.ShoppingCart.Items.Any())
                return string.Empty;

            return string.Join(",", flightReservationResponse.ShoppingCart.Items.SelectMany(x => x.Product).Select(x => x.Code).Distinct());
        }

        private void AssignFormOfPaymentType(MOBFormofPaymentDetails formofPaymentDetails, List<MOBSHOPPrice> prices, bool IsSecondaryFOP = false, bool isRemoveAll = false)
        {
            //need to update only when TravelFutureFlightCredit is added as formofpayment.          
            if (formofPaymentDetails?.TravelFutureFlightCredit?.FutureFlightCredits?.Count() > 0 || isRemoveAll)
            {
                if (!formofPaymentDetails.IsOtherFOPRequired && !IsSecondaryFOP)
                {
                    formofPaymentDetails.FormOfPaymentType = MOBFormofPayment.FFC.ToString();
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
        }

        private bool IncludeTravelCredit(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableTravelCredit") &&
                   GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidTravelCreditVersion", "iPhoneTravelCreditVersion", "", "", true, _configuration);
        }

        private bool IncludeTravelBankFOP(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableTravelBankFOP")
                && GeneralHelper.IsApplicationVersionGreater
                (appId, appVersion, "AndroidTravelBankFOPVersion", "iPhoneTravelBankFOPVersion", "", "", true, _configuration);
        }

        public string GetCostBreakdownFareHeader(string travelType, MOBShoppingCart shoppingCart)
        {
            string fareHeader = "Fare";
            if (!string.IsNullOrEmpty(travelType))
            {
                travelType = travelType.ToUpper();
                if (travelType == TravelType.CB.ToString())
                {
                    fareHeader = "Corporate fare";
                }
                else if (travelType == TravelType.CLB.ToString())
                {
                    fareHeader = "Break from Business fare";
                }
                else if (shoppingCart?.Offers != null && !string.IsNullOrEmpty(shoppingCart?.Offers.OfferCode))
                {
                    fareHeader = _configuration.GetValue<string>("OfferCodeFareText");
                }
            }
            return fareHeader;
        }
        public Dictionary<string, string> GetAdditionalHeadersForMosaic(string flow)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            if (_configuration.GetValue<bool>("EnableAdditionalHeadersForMosaicInRFOP"))
            {
                if (flow?.ToUpper() == FlowType.BOOKING.ToString() || flow?.ToUpper() == FlowType.RESHOP.ToString())
                {
                    headers.Add("ChannelID", _configuration.GetValue<string>("MerchandizeOffersCSLServiceChannelID"));
                    headers.Add("ChannelName", _configuration.GetValue<string>("MerchandizeOffersCSLServiceChannelName"));
                }
                else if (flow?.ToUpper() == FlowType.CHECKIN.ToString() || flow?.ToUpper() == FlowType.CHECKINSDC.ToString())
                {
                    headers.Add("ChannelID", _configuration.GetValue<string>("MerchandizeOffersMCEServiceChannelID"));
                    headers.Add("ChannelName", _configuration.GetValue<string>("MerchandizeOffersMCEServiceChannelName"));
                }
                else
                {
                    headers.Add("ChannelID", _configuration.GetValue<string>("MerchandizeOffersServiceChannelID"));
                    headers.Add("ChannelName", _configuration.GetValue<string>("MerchandizeOffersServiceChannelName"));
                }
            }
            return headers;
        }

        public string ExcludeCountryCodeFrmPhoneNumber(string phonenumber, string countrycode)
        {
            try
            {
                Int64 _phonenumber;
                if (!string.IsNullOrEmpty(phonenumber)) phonenumber = phonenumber.Replace(" ", "");
                if (Int64.TryParse(phonenumber, out _phonenumber))
                {
                    if (!string.IsNullOrEmpty(countrycode))
                    {
                        var phonenumbercountrycode = phonenumber.Substring(0, countrycode.Length);
                        if (string.Equals(countrycode, phonenumbercountrycode, StringComparison.OrdinalIgnoreCase))
                        {
                            return phonenumber.Remove(0, countrycode.Length);
                        }
                    }
                }
            }
            catch
            { return string.Empty; }
            return phonenumber;
        }
        public async Task<List<CacheCountry>> LoadCountries()
        {
            List<CacheCountry> Countries = new List<CacheCountry>();
            try
            {
                var CountriesList = await _cachingService.GetCache<string>("CountriesContent", "CountriesContent01").ConfigureAwait(false);

                if (!string.IsNullOrEmpty(CountriesList))
                {
                    return JsonConvert.DeserializeObject<List<CacheCountry>>(CountriesList);
                }
            }
            catch { return Countries; }
            return default;
        }

        public async Task<string> GetCountryCode(string countryaccesscode)
        {
            string countrycode = string.Empty;
            try
            {
                var _countries = LoadCountries().Result;
                countrycode = _countries.FirstOrDefault<CacheCountry>(_ => _.CODE == countryaccesscode)?.ACCESSCODE;
                //if (_countries != null && _countries.Any())
                //{
                //    countrycode = _countries.FirstOrDefault(x => (x.Length == 3 && string.Equals
                //    (countryaccesscode, x[0], StringComparison.OrdinalIgnoreCase)))[2];
                //    countrycode = countrycode.Replace(" ", "");
                //}

            }
            catch { return countrycode; }

            return countrycode;
        }
        public string GetPaymentTargetForRegisterFop(FlightReservationResponse flightReservationResponse, string flow, bool isCompleteFarelockPurchase = false)
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
                case "MANAGERES":
                case "VIEWRES_BUNDLES_SEATMAP":
                case "VIEWRES_SEATMAP":
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

        public List<string> GetProductCodes(FlightReservationResponse flightReservationResponse, string flow, bool isPost, bool isGetCartInfo)
        {
            List<string> productCodes = new List<string>();

            if (isPost ? flightReservationResponse.CheckoutResponse.ShoppingCartResponse.Items.Select(x => x.Item).Any(x => x.Product.FirstOrDefault().Code == "FLK")
                        : flightReservationResponse.ShoppingCart.Items.Any(x => x.Product.FirstOrDefault().Code == "FLK"))
                flow = FlowType.FARELOCK.ToString();

            switch (flow)
            {
                case "BOOKING":
                case "RESHOP":
                    productCodes = isPost ? flightReservationResponse.CheckoutResponse.ShoppingCart.Items.Where(x => !UtilityHelper.CheckFailedShoppingCartItem(flightReservationResponse, x)).Select(x => x.Product.FirstOrDefault().Code).Distinct().ToList()
                        : flightReservationResponse.ShoppingCart.Items.Select(x => x.Product.FirstOrDefault().Code).ToList();
                    break;

                case "POSTBOOKING":
                    productCodes = isPost ? flightReservationResponse.CheckoutResponse.ShoppingCart.Items.Where(x => !UtilityHelper.CheckFailedShoppingCartItem(flightReservationResponse, x)).SelectMany(x => x.Product).Where(x => x.Characteristics != null && (x.Characteristics.Any(y => y.Description == "PostPurchase" && Convert.ToBoolean(y.Value) == true))).Select(x => x.Code).Distinct().ToList()
                        : flightReservationResponse.ShoppingCart.Items.SelectMany(x => x.Product).Where(x => x.Characteristics != null && (x.Characteristics.Any(y => y.Description == "PostPurchase" && Convert.ToBoolean(y.Value) == true))).Select(x => x.Code).Distinct().ToList();
                    break;

                case "FARELOCK":
                case "VIEWRES":
                case "MANAGERES":
                case "VIEWRES_SEATMAP":
                case "VIEWRES_BUNDLES_SEATMAP":
                case "CHECKIN":
                case "MOBILECHECKOUT":
                case "BAGGAGECALCULATOR":
                    productCodes = isPost ? flightReservationResponse.CheckoutResponse.ShoppingCart.Items.Where(x => !UtilityHelper.CheckFailedShoppingCartItem(flightReservationResponse, x)).Where(x => x.Product.FirstOrDefault().Code != "RES").Select(x => x.Product.FirstOrDefault().Code).Distinct().ToList()
                        : flightReservationResponse.ShoppingCart.Items.Where(x => x.Product.FirstOrDefault().Code != "RES").Select(x => x.Product.FirstOrDefault().Code).Distinct().ToList();
                    break;

            }

            return productCodes;
        }
        public string BuildSegmentInfo(string productCode, Collection<ReservationFlightSegment> flightSegments, IGrouping<string, SubItem> x, string flow, bool isBundleProduct = false, bool isMRBundle = false)
        {
            if (productCode == "AAC" || productCode == "PAC" || productCode == "SDC")
                return string.Empty;

            if (isBundleProduct || productCode == "BAG")
            {
                var subItem = x.FirstOrDefault();
                var segmentInfo = string.Join(" - ", flightSegments?.FirstOrDefault(f => string.Equals(f.TripNumber, subItem?.TripIndex))?.FlightSegment?.DepartureAirport?.IATACode
                                                    , flightSegments?.LastOrDefault(l => string.Equals(l.TripNumber, subItem?.TripIndex))?.FlightSegment?.ArrivalAirport?.IATACode);
                return segmentInfo;
            }
            if ((ConfigUtility.IsViewResFlowCheckOut(flow) && _configuration.GetValue<bool>("EnableBasicEconomyBuyOutInViewRes") && productCode == "BEB") || isMRBundle)
            {
                var tripNumber = flightSegments?.Where(y => y.FlightSegment.SegmentNumber == Convert.ToInt32(x.Select(u => u.SegmentNumber).FirstOrDefault())).FirstOrDefault().TripNumber;
                var tripFlightSegments = flightSegments?.Where(c => c != null && !string.IsNullOrEmpty(c.TripNumber) && c.TripNumber.Equals(tripNumber)).ToCollection();
                if (tripFlightSegments != null && tripFlightSegments.Count > 1)
                {
                    return tripFlightSegments?.FirstOrDefault()?.FlightSegment?.DepartureAirport?.IATACode + " - " + tripFlightSegments?.LastOrDefault()?.FlightSegment?.ArrivalAirport?.IATACode;
                }
                else
                {
                    return flightSegments.Where(y => y.FlightSegment.SegmentNumber == Convert.ToInt32(x.Select(u => u.SegmentNumber).FirstOrDefault())).Select(y => y.FlightSegment.DepartureAirport.IATACode + " - " + y.FlightSegment.ArrivalAirport.IATACode).FirstOrDefault().ToString();
                }
            }

            return flightSegments?.Where(y => y.FlightSegment.SegmentNumber == Convert.ToInt32(x.Select(u => u.SegmentNumber).FirstOrDefault())).Select(y => y.FlightSegment.DepartureAirport.IATACode + " - " + y.FlightSegment.ArrivalAirport.IATACode).FirstOrDefault().ToString();
        }

        public async Task<List<MOBProdDetail>> ConfirmationPageProductInfo(FlightReservationResponse flightReservationResponse, bool isPost, bool isError, string flow, 
                                MOBApplication application, SeatChangeState state = null, bool isRefundSuccess = false, bool isPartialSuccess = false, 
                                List<SCProductContext> unSuccessfulSegmentDetails = null,
                                List<string> refundedSegmentNums = null, bool isGetCartInformation = false, string sessionId = "",bool isMiles=false)
        {
            List<MOBProdDetail> prodDetails = new List<MOBProdDetail>();
            List<string> productCodes = new List<string>();
            bool isFareLockCompletePurchase = await IsFareLockCompletePurchase(flightReservationResponse, isPost).ConfigureAwait(false);
            var isEnableMFOPCheckinBags = await _featureToggles.IsEnableMFOPCheckinBags(application.Id, application.Version.Major).ConfigureAwait(false);

            if (isFareLockCompletePurchase)
            {
                var displayTotalPrice = flightReservationResponse.DisplayCart.DisplayPrices.FirstOrDefault(o => (o.Description != null && o.Description.Equals("Total", StringComparison.OrdinalIgnoreCase))).Amount;
                var grandTotal = UtilityHelper.GetGrandTotalPriceFareLockShoppingCart(flightReservationResponse);
                var prodDetail = new MOBProdDetail()
                {
                    Code = "FLK_VIEWRES",
                    ProdDescription = string.Empty,
                    ProdTotalPrice = string.Format("{0:0.00}", grandTotal),
                    ProdDisplayTotalPrice = grandTotal.ToString("c"),
                    Segments = new List<MOBProductSegmentDetail> {
                                     new MOBProductSegmentDetail {
                                                        SegmentInfo = "",
                                                        SubSegmentDetails = new List<MOBProductSubSegmentDetail>
                                                                            {
                                                                                new MOBProductSubSegmentDetail
                                                                                {
                                                                                    Price = String.Format("{0:0.00}", displayTotalPrice),
                                                                                    DisplayPrice = displayTotalPrice.ToString("c"),
                                                                                    Passenger = UtilityHelper.GetFareLockPassengerDescription(flightReservationResponse.Reservation),
                                                                                    SegmentDescription = UtilityHelper.GetFareLockSegmentDescription(flightReservationResponse.Reservation)
                                                                                }
                                                                            }
                                                                    }
                    },
                };
                prodDetails.Add(prodDetail);
            }
            else
            {

                productCodes = GetProductCodes(flightReservationResponse, flow, isPost, isGetCartInformation);
                productCodes = UtilityHelper.OrderProducts(productCodes);

                //Added this line to replace the ProductCode for FareLock
                int index = productCodes.FindIndex(ind => ind.Equals("FLK"));
                if (index != -1)
                    productCodes[index] = "FareLock";

                bool isMRBundle = IsManageResBundleSelected(flightReservationResponse, isPost, flow);
                if (isMRBundle)
                {
                    productCodes.Remove("SEATASSIGNMENTS");
                }

                var enableMFOPBags = await _featureSettings.GetFeatureSettingValue("EnableMfopForBags").ConfigureAwait(false)
                                        && GeneralHelper.IsApplicationVersionGreaterorEqual(application.Id, application.Version.Major, _configuration.GetValue<string>("AndroidMilesFopBagsVersion"), _configuration.GetValue<string>("iPhoneMilesFopBagsVersion"));

                foreach (string productCode in productCodes)
                {
                    MOBProdDetail prodDetail;
                    if (productCode == "SEATASSIGNMENTS")
                    {
                        if (ConfigUtility.IsViewResFlowCheckOut(flow))
                        {
                            prodDetail = BuildProdDetailsForSeats(flightReservationResponse, state, isPost, flow, isMiles);
                            if (prodDetail != null && ((!string.IsNullOrEmpty(prodDetail.ProdDisplayTotalPrice) || !string.IsNullOrEmpty(prodDetail.ProdDisplayOtherPrice)) || IsFreeSeatCouponApplied(prodDetail, flightReservationResponse)))
                            {
                                prodDetails.Add(prodDetail);
                            }
                        }
                        else
                        {
                            prodDetail = await BuildProdDetailsForSeats(flightReservationResponse, state, flow, application).ConfigureAwait(false);
                            if (prodDetail != null && (!string.IsNullOrEmpty(prodDetail.ProdDisplayTotalPrice) || !string.IsNullOrEmpty(prodDetail.ProdDisplayOtherPrice)))
                            {
                                prodDetails.Add(prodDetail);
                            }
                        }

                    }
                    else if (productCode == "RES")
                    {
                        United.Service.Presentation.InteractionModel.ShoppingCart flightReservationResponseShoppingCart = new United.Service.Presentation.InteractionModel.ShoppingCart();
                        flightReservationResponseShoppingCart = isPost ? flightReservationResponse.CheckoutResponse.ShoppingCart : flightReservationResponse.ShoppingCart;

                        prodDetail = new MOBProdDetail()
                        {
                            Code = flightReservationResponseShoppingCart.Items.SelectMany(d => d.Product).Where(d => d.Code == productCode).Select(d => d.Code).FirstOrDefault().ToString(),
                            ProdDescription = flightReservationResponseShoppingCart.Items.SelectMany(d => d.Product).Where(d => d.Code == productCode).Select(d => d.Description).FirstOrDefault().ToString(),
                            ProdTotalPrice = string.Format("{0:0.00}", UtilityHelper.GetTotalPriceForRESProduct(isPost, flightReservationResponseShoppingCart, flow)),
                            ProdDisplayTotalPrice = Decimal.Parse(UtilityHelper.GetTotalPriceForRESProduct(isPost, flightReservationResponseShoppingCart, flow).ToString()).ToString("c", new CultureInfo("en-us"))
                        };
                        prodDetails.Add(prodDetail);
                    }
                    else if (_configuration.GetValue<bool>("CFOP19HBugFixToggle") && productCode == "RBF")
                    {
                        United.Service.Presentation.InteractionModel.ShoppingCart flightReservationResponseShoppingCart = new United.Service.Presentation.InteractionModel.ShoppingCart();
                        flightReservationResponseShoppingCart = isPost ? flightReservationResponse.CheckoutResponse.ShoppingCart : flightReservationResponse.ShoppingCart;

                        prodDetail = new MOBProdDetail()
                        {
                            Code = flightReservationResponseShoppingCart.Items.SelectMany(d => d.Product).Where(d => d.Code == productCode).Select(d => d.Code).FirstOrDefault().ToString(),
                            ProdDescription = flightReservationResponseShoppingCart.Items.SelectMany(d => d.Product).Where(d => d.Code == productCode).Select(d => d.Description).FirstOrDefault().ToString(),
                            ProdTotalPrice = string.Format("{0:0.00}", UtilityHelper.GetCloseBookingFee(isPost, flightReservationResponseShoppingCart, flow)),
                            ProdDisplayTotalPrice = Decimal.Parse(UtilityHelper.GetCloseBookingFee(isPost, flightReservationResponseShoppingCart, flow).ToString()).ToString("c")
                        };
                        prodDetails.Add(prodDetail);
                    }
                    else if (_configuration.GetValue<bool>("EnablePassCHGProductInReshopFlowToggle") && productCode == "CHG")
                    {
                        United.Service.Presentation.InteractionModel.ShoppingCart flightReservationResponseShoppingCart = new United.Service.Presentation.InteractionModel.ShoppingCart();
                        flightReservationResponseShoppingCart = isPost ? flightReservationResponse.CheckoutResponse.ShoppingCart : flightReservationResponse.ShoppingCart;

                        prodDetail = new MOBProdDetail()
                        {
                            Code = flightReservationResponseShoppingCart.Items.SelectMany(d => d.Product).Where(d => d.Code == productCode).Select(d => d.Code).FirstOrDefault().ToString(),
                            ProdDescription = flightReservationResponseShoppingCart.Items.SelectMany(d => d.Product).Where(d => d.Code == productCode).Select(d => d.Description).FirstOrDefault().ToString(),
                            ProdTotalPrice = string.Format("{0:0.00}", GetProductFee(isPost, flightReservationResponseShoppingCart, flow, productCode)),
                            ProdDisplayTotalPrice = Decimal.Parse(GetProductFee(isPost, flightReservationResponseShoppingCart, flow, productCode).ToString()).ToString("c")
                        };
                        prodDetails.Add(prodDetail);
                    }
                    else if (_configuration.GetValue<bool>("EnableCouponMVP2Changes") && flow == FlowType.BOOKING.ToString() && productCode == "BAG")//SC is adding a bag product if free bag coupon is applies..Adding default values
                    {
                        prodDetail = new MOBProdDetail()
                        {
                            Code = "BAG",
                            ProdDescription = string.Empty,
                            ProdTotalPrice = "0",
                            ProdDisplayTotalPrice = "0"
                        };
                        prodDetails.Add(prodDetail);
                    }

                    else if (productCode == "PCU" && isError == true)
                    {
                        var chargedSegmentNums = flightReservationResponse.DisplayCart.TravelOptions.SelectMany(x => x.SubItems).Where(x => x.Amount != 0).Select(x => x.SegmentNumber).ToList().Except(refundedSegmentNums).ToList();

                        prodDetail = new MOBProdDetail()
                        {
                            Code = flightReservationResponse.DisplayCart.TravelOptions.Where(d => d.Key == productCode).Select(d => d.Key).FirstOrDefault().ToString(),
                            ProdDescription = flightReservationResponse.DisplayCart.TravelOptions.Where(d => d.Key == productCode).Select(d => d.Type).FirstOrDefault().ToString(),
                            ProdTotalPrice = string.Format("{0:0.00}", flightReservationResponse.DisplayCart.TravelOptions.Where(d => d.Key == productCode).Select(d => d.Amount).Sum()),
                            ProdDisplayTotalPrice = Decimal.Parse(flightReservationResponse.DisplayCart.TravelOptions.Where(d => d.Key == productCode).Select(d => d.Amount).Sum().ToString()).ToString("c"),
                            Segments = (isPartialSuccess == true) ? flightReservationResponse.DisplayCart.TravelOptions.Where(d => d.Key == productCode).SelectMany(x => x.SubItems).Where(x => x.Amount != 0).OrderBy(x => x.SegmentNumber).GroupBy(x => x.SegmentNumber).ToList().Select(x => new MOBProductSegmentDetail
                            {
                                SegmentInfo = BuildSegmentInfo(productCode, flightReservationResponse.Reservation.FlightSegments, x, flow),
                                ProductId = string.Join(",", x.Select(u => u.Key).ToList()),
                                TripId = string.Join(",", x.Select(u => u.TripIndex).ToList()),
                                SubSegmentDetails = x.GroupBy(f => f.SegmentNumber).Select(t => new MOBProductSubSegmentDetail
                                {
                                    Price = String.Format("{0:0.00}", t.Select(i => i.Amount).Sum()),
                                    DisplayPrice = Decimal.Parse(t.Select(i => i.Amount).Sum().ToString()).ToString("c"),
                                    Passenger = x.Count().ToString() + (x.Count() > 1 ? " Travelers" : " Traveler"),
                                    SegmentDescription = ConfigUtility.GetFormattedCabinName(t.Select(u => u.Description).FirstOrDefault().ToString()),
                                    IsPurchaseFailure = unSuccessfulSegmentDetails.Select(q => q.Value).ToList().Select(q => q.Split(' ')).Any(q => q[1] == t.Select(u => u.SegmentNumber).FirstOrDefault())
                                }).ToList()
                            }).ToList() : null,
                            ProdOtherPrice = string.Format("{0:0.00}", (isPartialSuccess == true) ? (flightReservationResponse.DisplayCart.TravelOptions.Where(d => d.Key == productCode).SelectMany(x => x.SubItems).Where(x => x.Amount != 0 && chargedSegmentNums.Contains(x.SegmentNumber)).Select(r => r.Amount).Sum()) : flightReservationResponse.DisplayCart.TravelOptions.Where(d => d.Key == productCode).SelectMany(x => x.SubItems).Where(x => x.Amount != 0).Select(r => r.Amount).Sum()),
                            ProdDisplayOtherPrice = (isPartialSuccess == true) ? (flightReservationResponse.DisplayCart.TravelOptions.Where(d => d.Key == productCode).SelectMany(x => x.SubItems).Where(x => x.Amount != 0 && chargedSegmentNums.Contains(x.SegmentNumber)).Select(r => r.Amount).Sum()).ToString("c") : flightReservationResponse.DisplayCart.TravelOptions.Where(d => d.Key == productCode).SelectMany(x => x.SubItems).Where(x => x.Amount != 0).Select(r => r.Amount).Sum().ToString("c"),
                            ProdTotalMiles = 0,
                            ProdDisplayTotalMiles = string.Empty
                        };
                        prodDetails.Add(prodDetail);
                    }
                    else if ((productCode == "POM") && ConfigUtility.IsViewResFlowPaymentSvcEnabled(flow))
                    {
                        prodDetails = await BuildProductDetailsForInflightMeals(flightReservationResponse, productCode, sessionId, isPost);
                    }
                    else
                    {
                        var travelOptions = flightReservationResponse.DisplayCart.TravelOptions?.Where(d => d.Key == productCode).ToCollection().Clone();
                        if (ConfigUtility.IsViewResFlowCheckOut(flow))
                        {
                            refundedSegmentNums = null;
                            travelOptions = GetTravelOptionItems(flightReservationResponse, productCode);
                        }
                        bool isBundleProduct = string.Equals(travelOptions?.FirstOrDefault(t => t.Key == productCode)?.Type, "BE", StringComparison.OrdinalIgnoreCase); //ensuring bundle product
                        BundleDetailsPersist bundleDetailsPersist = new BundleDetailsPersist();
                        if (isMRBundle)
                        {
                            bundleDetailsPersist = await _sessionHelperService.GetSession<BundleDetailsPersist>(sessionId, ObjectNames.BundleDetailsPersist, new List<string> { sessionId, ObjectNames.BundleDetailsPersist }).ConfigureAwait(false);
                        }
                        var safProductName = "";
                        
                        if (await _featureSettings.GetFeatureSettingValue("GetSAFProductNameFromSDL").ConfigureAwait(false) && (travelOptions?.Any(t => t.Key == "SFC") ?? false))
                        {
                            List<CMSContentMessage> lstMessages = await GetSDLContentByGroupName(new MOBRequest { TransactionId = sessionId, Application = application }, sessionId, "", _configuration.GetValue<string>("CMSContentMessages_GroupName_MANAGERESOffers_Messages"), "ManageReservation_Offers_CMSContentMessagesCached_StaticGUID");
                            safProductName = lstMessages?.FirstOrDefault(msg => msg?.Title == "MOB_SAF_ProductName_PaymentPage")?.ContentFull;
                        }

                        if (!ConfigUtility.IsViewResFlowCheckOut(flow) && !string.IsNullOrEmpty(_configuration.GetValue<string>("EnablePCUSelectedSeatPurchaseViewRes")))
                        {
                            if (flightReservationResponse.DisplayCart.DisplaySeats != null && flightReservationResponse.DisplayCart.DisplaySeats.Any(s => s != null && s.PCUSeat))
                            {
                                travelOptions.Where(t => t.Key == "PCU")
                                             .ForEach(t => t.SubItems.RemoveWhere(sb => sb.Amount == 0 || flightReservationResponse.DisplayCart.DisplaySeats.Any(s => s.PCUSeat && s.OriginalSegmentIndex.ToString() == sb.SegmentNumber && (s.TravelerIndex + 1).ToString() == sb.TravelerRefID)));
                                travelOptions.RemoveWhere(t => t.SubItems == null || !t.SubItems.Any());
                            }
                        }
                        if (travelOptions == null || !travelOptions.Any())
                            continue;

                        if (flightReservationResponse.Errors != null && flightReservationResponse.Errors.Any(e => e != null && e.MinorCode == "90506"))
                        {
                            if (!IsRefundSuccess(flightReservationResponse.CheckoutResponse.ShoppingCartResponse.Items, out refundedSegmentNums, flow))
                            {
                                continue;
                            }
                        }
                        bool isCheckinFlow = await IsCheckInFlow(flow).ConfigureAwait(false);
                        var populateMiles = enableMFOPBags && productCode == "BAG";
                        var populateCheckinBagMiles = isEnableMFOPCheckinBags && isCheckinFlow && productCode == "BAG";
                        prodDetail = new MOBProdDetail()
                        {
                            Code = travelOptions.Where(d => d.Key == productCode).Select(d => d.Key).FirstOrDefault().ToString(),
                            ProdDescription = (productCode == "TPI" && _configuration.GetValue<bool>("GetTPIProductName_HardCode")) ? "Travel insurance" : isMRBundle ? bundleDetailsPersist?.Title : travelOptions.Where(d => d.Key == productCode).Select(d => d.Type).FirstOrDefault().ToString(),
                            ProdTotalPrice = string.Format("{0:0.00}", travelOptions.Select(d => d.Amount).Sum()),
                            ProdTotalRequiredMiles = (populateMiles || populateCheckinBagMiles) ? string.Format("{0:0}", travelOptions.Select(d => d.MileageAmount).Sum()) : "0",
                            ProdDisplayTotalRequiredMiles = (populateMiles || populateCheckinBagMiles) ? UtilityHelper.FormatAwardAmountForDisplay(Decimal.Parse(travelOptions.Select(d => d.MileageAmount).Sum().ToString()).ToString(), false) : string.Empty,
                            ProdDisplayTotalPrice = Decimal.Parse(travelOptions.Select(d => d.Amount).Sum().ToString()).ToString("c"),
                            ProdOriginalPrice = string.Format("{0:0.00}", travelOptions.Select(d => d.OriginalPrice).Sum()),
                            Segments = travelOptions
                                           .Where(d => d.Key == productCode)
                                           .SelectMany(x => x.SubItems)
                                           .Where(x => (ShouldIgnoreAmount(x, flightReservationResponse, flow)) ? true : x.Amount != 0 || (!_configuration.GetValue<bool>("DisableFreeCouponFix") && x.OriginalPrice != 0))
                                           .OrderBy(x => x.SegmentNumber).GroupBy(x => x.SegmentNumber)
                                           .ToList()
                                           .Select(x => new MOBProductSegmentDetail
                                           {
                                               SegmentInfo = BuildSegmentInfo(productCode, flightReservationResponse.Reservation.FlightSegments, x, flow, isMRBundle),
                                               ProductId = isCheckinFlow ? string.Join(",", x.Select(u => u.Key).ToList()) : string.Join(",", x.Select(u => u.Value).ToList()),
                                               TripId = string.Join(",", x.Select(u => u.TripIndex).ToList()),
                                               //ONLY FOR VIEWRES TODO
                                               SegmentId = string.Join(",", x.Select(u => u.SegmentNumber).Distinct().ToList()),
                                               ProductIds = isCheckinFlow ? x.Select(u => u.Key).ToList() : productCode == "BAG" ? x.Select(u => u.Value?.ToString()).ToList() : x.Select(u => u.Key).ToList(),
                                               SubSegmentDetails = x.GroupBy(f => f.SegmentNumber).Select(t => new MOBProductSubSegmentDetail
                                               {
                                                   SegmentInfo = IsEnableOmniCartMVP2Changes(application.Id, application.Version.Major, true) && productCode != "TPI" ? BuildSegmentInfo(productCode, flightReservationResponse.Reservation.FlightSegments, x, flow, isBundleProduct, isMRBundle) : string.Empty,
                                                   Price = string.Format("{0:0.00}", t.Select(i => i.Amount).Sum()),
                                                   MilesPrice = (populateMiles || populateCheckinBagMiles) ? string.Format("{0:0.00}", t.Select(i => i.MileageAmount).Sum()) : "0",
                                                   DisplayMiles = (populateMiles || populateCheckinBagMiles) ? UtilityHelper.FormatAwardAmountForDisplay(String.Format("{0:0}", t.Select(s => s.MileageAmount).ToList().Sum()), false) : string.Empty,
                                                   DisplayMilesPrice = (populateMiles || populateCheckinBagMiles) ? UtilityHelper.FormatAwardAmountForDisplay(String.Format("{0:0}", t.Select(s => s.MileageAmount).ToList().Sum()), false) : string.Empty,
                                                   OrginalPrice = _configuration.GetValue<bool>("EnablePromoCodeForAncillaryOffersManageRes") ? string.Format("{0:0.00}", t.Select(i => i.OriginalPrice).Sum()) : string.Empty,
                                                   DisplayOriginalPrice = IsEnableOmniCartMVP2Changes(application.Id, application.Version.Major, true) ? UtilityHelper.GetDisplayOriginalPrice(t.Select(i => i.Amount).Sum(), t.Select(i => i.OriginalPrice).Sum()) : string.Empty,
                                                   DisplayPrice = Decimal.Parse(t.Select(i => i.Amount).Sum().ToString()).ToString("c"),
                                                   Passenger = GetPassengerText(flow, productCode, x.GroupBy(i => i.TravelerRefID).Count(), flightReservationResponse.Reservation.Travelers?.Count),
                                                   SegmentDescription = BuildProductDescription(travelOptions, t, productCode, flow, isMRBundle ? bundleDetailsPersist?.Title : string.Empty, safProductName),
                                                   IsPurchaseFailure = UtilityHelper.IsPurchaseFailed(productCode == "PCU", t.Select(sb => sb.SegmentNumber).FirstOrDefault(), refundedSegmentNums),
                                                   ProdDetailDescription = IsEnableOmniCartMVP2Changes(application.Id, application.Version.Major, true) ? GetProductDetailDescrption(t, productCode, sessionId, isBundleProduct, isMRBundle, bundleDetailsPersist?.BundleDescriptions).ConfigureAwait(false).GetAwaiter().GetResult() : null,
                                                   ProductDescription = IsEnableOmniCartMVP2Changes(application.Id, application.Version.Major, true) ? GetProductDescription(travelOptions, productCode, flow) : string.Empty
                                               }).ToList()
                                           }).ToList(),
                            ProdTotalMiles = 0,
                            ProdDisplayTotalMiles = string.Empty
                        };

                        // MOBILE-25395: SAF
                        // Fix the product descriptions and tripID for SAF bundle
                        var safCode = _configuration.GetValue<string>("SAFCode");
                        if (string.Equals(productCode, safCode, StringComparison.OrdinalIgnoreCase))
                        {
                            prodDetail?.Segments?.ForEach(segment =>
                            {
                                segment.SubSegmentDetails?.ForEach(subSegment => subSegment.ProductDescription = subSegment.ProdDetailDescription?.FirstOrDefault());
                                var safProdDetail = flightReservationResponse.ShoppingCart?.Items?.FirstOrDefault(item => item.Product?.Any(product => string.Equals(product.Code, safCode, StringComparison.OrdinalIgnoreCase)) ?? false);
                                if (safProdDetail != null)
                                {
                                    segment.TripId = string.Join(",", safProdDetail.Product?.FirstOrDefault(product => string.Equals(product.Code, safCode, StringComparison.OrdinalIgnoreCase))?.SubProducts?.Select(sp => sp.ID)?.ToList());
                                }
                            });
                        }
                        if (!ConfigUtility.IsViewResFlowCheckOut(flow) && isGetCartInformation)
                        {
                            List<MOBTypeOption> lineItems = new List<MOBTypeOption>();
                            MOBBag bagCounts = null;
                            TravelOption travelOption = flightReservationResponse.DisplayCart?.TravelOptions?.FirstOrDefault(x => x.Type == "Bags");

                            var product = travelOption?.SubItems?.Select(p => p.Product).Where(p => p != null);
                            if (travelOption != null && product != null)
                            {
                                bagCounts = new MOBBag()
                                {
                                    TotalBags = travelOption.ItemCount,
                                    OverWeightBags = product.SelectMany(subproducts => subproducts.SubProducts.Select(b => b.EddCode == "OWB")).Where(exist => exist == true).Count(),
                                    OverWeight2Bags = product.SelectMany(subproducts => subproducts.SubProducts.Select(b => b.EddCode == "OWH")).Where(exist => exist == true).Count(),
                                    OverSizeBags = product.SelectMany(subproducts => subproducts.SubProducts.Select(b => b.EddCode == "OSB")).Where(exist => exist == true).Count(),
                                    ExceptionItemInfantSeat = product.SelectMany(subproducts => subproducts.SubProducts.Select(b => b.EddCode.Contains("EX"))).Where(exist => exist == true).Count()
                                };
                                prodDetail.LineItems = GetBagsLineItems(bagCounts);
                            }
                        }
                        if (prodDetail == null) { continue; }
                        if (!IsCheckinFlow(flow))
                        {
                            if (productCode != "FareLock")
                                UpdateRefundTotal(prodDetail);
                            else
                                prodDetail.Code = "FLK";

                        }
                        if (!string.IsNullOrEmpty(prodDetail.ProdDisplayTotalPrice) || !string.IsNullOrEmpty(prodDetail.ProdDisplayOtherPrice) || IsOriginalPriceExists(prodDetail))
                            prodDetails.Add(prodDetail);
                    }
                }
                if ((_configuration.GetValue<bool>("EnableCouponsforBooking") && flow == FlowType.BOOKING.ToString() && prodDetails != null)
                    || (_configuration.GetValue<bool>("EnableCouponsInPostBooking") && flow == FlowType.POSTBOOKING.ToString()) || (_configuration.GetValue<bool>("IsEnableManageResCoupon") && (flow == FlowType.VIEWRES.ToString() || flow == FlowType.VIEWRES_SEATMAP.ToString())))
                {
                    ConfigUtility.AddCouponDetails(prodDetails, flightReservationResponse, isPost, flow, application);
                }
            }
            return await Task.FromResult(prodDetails);
        }

        private async Task<bool> IsFareLockCompletePurchase(FlightReservationResponse flightReservationResponse, bool isPost)
        {
            return flightReservationResponse.Reservation?.Characteristic != null && flightReservationResponse.Reservation.Characteristic.Any(o => (o.Code != null && o.Value != null && o.Code.Equals("FARELOCK") && o.Value.Equals("TRUE"))) &&
                flightReservationResponse.Reservation.Characteristic.Any(o => (o.Code != null && o.Code.Equals("FARELOCK_DATE")))
                && !(await _featureSettings.GetFeatureSettingValue("EnableSAFInViewRes").ConfigureAwait(false) && isSFCProductRegistered(flightReservationResponse, isPost));
        }

        private bool IsManageResBundleSelected(FlightReservationResponse flightReservationResponse, bool isPost, string flow)
        {
            if(_configuration.GetValue<bool>("EnableTravelOptionsInViewRes"))
            {
              return  isPost? ConfigUtility.IsViewResFlowCheckOut(flow) && IsBundleProductSelected(flightReservationResponse, isPost) : ConfigUtility.IsViewResFlowPaymentSvcEnabled(flow) && IsBundleProductSelected(flightReservationResponse, isPost);
            }
            return false;
        }
  
        public MOBProdDetail BuildProdDetailsForSeats(FlightReservationResponse flightReservationResponse, SeatChangeState state, bool isPost, string flow,bool isMiles=false)
        {
            if (flightReservationResponse.DisplayCart.DisplaySeats == null || !flightReservationResponse.DisplayCart.DisplaySeats.Any())
            {
                return null;
            }
            //check here.
            var fliterSeats = flightReservationResponse.DisplayCart.DisplaySeats.Where(d => d.PCUSeat || (CheckSeatAssignMessage(d.SeatAssignMessage, isPost) && d.Seat != "---")).ToList();
            if (_configuration.GetValue<bool>("EnablePCUFromSeatMapErrorCheckViewRes"))
            {
                fliterSeats = HandleCSLDefect(flightReservationResponse, fliterSeats, isPost);
            }
            if (!fliterSeats.Any())
            {
                return null;
            }
            
            var totalPrice = fliterSeats.Select(s => s.SeatPrice).ToList().Sum();
            double totalMilesPrice = 0;
            if (isMiles)
            {
                totalPrice = fliterSeats.Select(s => s.MoneyAmount).ToList().Sum();
                totalMilesPrice = fliterSeats.Select(s => s.MilesAmount).ToList().Sum();
            }
            
            var prod = new MOBProdDetail
            {
                Code = "SEATASSIGNMENTS",
                ProdDescription = string.Empty,
                ProdTotalPrice = String.Format("{0:0.00}", totalPrice),
                ProdDisplayTotalPrice = totalPrice > 0 ? Decimal.Parse(totalPrice.ToString()).ToString("c") : string.Empty,
                Segments = BuildProductSegmentsForSeats(flightReservationResponse, state?.Seats, state?.BookingTravelerInfo, isPost, flow, isMiles)
            };
            
            if (isMiles) {
                prod.ProdTotalRequiredMiles = String.Format("{0:0}", totalMilesPrice);
            }

            if (prod.Segments != null && prod.Segments.Any())
            {
                if (ConfigUtility.IsMilesFOPEnabled())
                {
                    if (prod.Segments.SelectMany(s => s.SubSegmentDetails).ToList().Select(ss => ss.Miles == 0).ToList().Count == 0 && ConfigUtility.IsMilesFOPEnabled())
                    {
                        prod.ProdTotalMiles = _configuration.GetValue<int>("milesFOP");
                        prod.ProdDisplayTotalMiles = UtilityHelper.FormatAwardAmountForDisplay(_configuration.GetValue<string>("milesFOP"), false);
                    }
                    else
                    {
                        prod.ProdTotalMiles = 0;
                        prod.ProdDisplayTotalMiles = string.Empty;
                    }
                }
                if (_configuration.GetValue<bool>("IsEnableManageResCoupon") && _mSCPageProductInfoHelper.isAFSCouponApplied(flightReservationResponse.DisplayCart))
                    prod.Segments.Select(x => x.SubSegmentDetails).ToList().ForEach(item => item.RemoveAll(k => Decimal.Parse(k.Price) == 0 && (string.IsNullOrEmpty(k.OrginalPrice) || Decimal.Parse(k.OrginalPrice) == 0) && k.Currency == "USD"));
                else
                    prod.Segments.Select(x => x.SubSegmentDetails).ToList().ForEach(item => item.RemoveAll(k => Decimal.Parse(k.Price) == 0 && (k.StrikeOffPrice == string.Empty || Decimal.Parse(k.StrikeOffPrice) == 0) && k.Currency == "USD"));

                prod.Segments.Select(x => x.SubSegmentDetails).ToList().ForEach(item => item.RemoveAll(k => Decimal.Parse(k.Price) == 0 && (k.MilesPrice == string.Empty || Decimal.Parse(k.MilesPrice) == 0) && k.Currency == "ML1"));

                prod.Segments.RemoveAll(k => k.SubSegmentDetails.Count == 0);
            }
            UpdateRefundTotal(prod,isMiles);
            return prod;
        }
        private List<SeatAssignment> HandleCSLDefect(FlightReservationResponse flightReservationResponse, List<SeatAssignment> fliterSeats, bool isPost)
        {
            if (fliterSeats == null || !fliterSeats.Any())
                return fliterSeats;

            fliterSeats = fliterSeats.Where(s => s != null && s.OriginalSegmentIndex != 0 && !string.IsNullOrEmpty(s.DepartureAirportCode) && !string.IsNullOrEmpty(s.ArrivalAirportCode)).ToList();

            if (fliterSeats == null || !fliterSeats.Any())
                return fliterSeats;

            if (flightReservationResponse.Errors != null &&
                flightReservationResponse.Errors.Any(e => e != null && e.MinorCode == "90584") &&
                flightReservationResponse.DisplayCart.DisplaySeats != null &&
                flightReservationResponse.DisplayCart.DisplaySeats.Any(s => s != null && s.PCUSeat) &&
                flightReservationResponse.DisplayCart.DisplaySeats.Any(s => s != null && !s.PCUSeat &&
                 CheckSeatAssignMessage(s.SeatAssignMessage, isPost)))
            {
                //take this from errors
                var item = flightReservationResponse.CheckoutResponse.ShoppingCartResponse.Items.Where(t => t.Item.Category == "Reservation.Reservation.SEATASSIGNMENTS").FirstOrDefault();
                if (item != null && item.Item != null && item.Item.Product != null && item.Item.Product.Any())
                {
                    var description = DataContextJsonSerializer.DeserializeJsonDataContract<Service.Presentation.FlightResponseModel.AssignTravelerSeat>(item.Item.Product.FirstOrDefault().Status.Description);
                    var unAssignedSeats = description.Travelers.SelectMany(t => t.Seats.Where(s => !string.IsNullOrEmpty(s.AssignMessage))).ToList();
                    if (unAssignedSeats != null && unAssignedSeats.Any())
                    {
                        return fliterSeats.Where(s => !IsFailedSeat(s, unAssignedSeats)).ToList();
                    }
                }
            }
            return fliterSeats;
        }
        private bool IsFailedSeat(SeatAssignment displaySeat, List<SeatDetail> unAssignedSeats)
        {
            if (unAssignedSeats == null || !unAssignedSeats.Any() || displaySeat == null || displaySeat.PCUSeat)
                return false;

            return unAssignedSeats.Any(s => s.DepartureAirport != null && s.ArrivalAirport != null &&
                                            s.DepartureAirport.IATACode == displaySeat.DepartureAirportCode &&
                                            s.ArrivalAirport.IATACode == displaySeat.ArrivalAirportCode &&
                                            s.FlightNumber == displaySeat.FlightNumber &&
                                            s.Seat != null && !string.IsNullOrEmpty(s.Seat.Identifier) &&
                                            s.Seat.Identifier == displaySeat.Seat &&
                                            s.Seat.Price != null && s.Seat.Price.Totals != null && s.Seat.Price.Totals.Any() &&
                                            s.Seat.Price.Totals.FirstOrDefault().Amount == Convert.ToDouble(displaySeat.SeatPrice));

        }
        private List<MOBProductSegmentDetail> BuildProductSegmentsForSeats(FlightReservationResponse flightReservationResponse, List<MOBSeat> seats, MOBApplication application, string flow)
        {
            return flightReservationResponse.DisplayCart.DisplaySeats.OrderBy(d => d.OriginalSegmentIndex)
                                                        .GroupBy(d => new { d.OriginalSegmentIndex, d.LegIndex })
                                                        .Select(d => new MOBProductSegmentDetail
                                                        {
                                                            SegmentInfo = UtilityHelper.GetSegmentInfo(flightReservationResponse, d.Key.OriginalSegmentIndex, Convert.ToInt32(d.Key.LegIndex)),
                                                            SubSegmentDetails = d.GroupBy(s => UtilityHelper.GetSeatTypeForDisplay(s, flightReservationResponse.DisplayCart.TravelOptions))
                                                                                .Select(seatGroup => new MOBProductSubSegmentDetail
                                                                                {
                                                                                    SegmentInfo = IsEnableOmniCartMVP2Changes(application.Id, application.Version.Major, true) ? UtilityHelper.GetSegmentInfo(flightReservationResponse, d.Key.OriginalSegmentIndex, Convert.ToInt32(d.Key.LegIndex)) : string.Empty,
                                                                                    Price = String.Format("{0:0.00}", seatGroup.Select(s => s.SeatPrice).ToList().Sum()),
                                                                                    OrginalPrice = IsEnableOmniCartMVP2Changes(application.Id, application.Version.Major, true) || _configuration.GetValue<bool>("IsEnableManageResCoupon") ? String.Format("{0:0.00}", seatGroup.Select(s => s.OriginalPrice).ToList().Sum()) : string.Empty,
                                                                                    DisplayPrice = Decimal.Parse(seatGroup.Select(s => s.SeatPrice).ToList().Sum().ToString()).ToString("c", new CultureInfo("en-us")),
                                                                                    DisplayOriginalPrice = IsEnableOmniCartMVP2Changes(application.Id, application.Version.Major, true) || _configuration.GetValue<bool>("IsEnableManageResCoupon") ? Decimal.Parse(seatGroup.Select(s => s.OriginalPrice).ToList().Sum().ToString()).ToString("c", new CultureInfo("en-us")) : string.Empty,
                                                                                    StrikeOffPrice = GetOriginalTotalSeatPriceForStrikeOff(seatGroup.ToList(), seats),
                                                                                    DisplayStrikeOffPrice = UtilityHelper.GetFormatedDisplayPriceForSeats(GetOriginalTotalSeatPriceForStrikeOff(seatGroup.ToList(), seats)),
                                                                                    Passenger = seatGroup.Count().ToString() + (seatGroup.Count() > 1 ? " Travelers" : " Traveler"),
                                                                                    SeatCode = seatGroup.Key,
                                                                                    FlightNumber = seatGroup.Select(x => x.FlightNumber).FirstOrDefault(),
                                                                                    SegmentDescription = GetSeatTypeBasedonCode(seatGroup.Key, seatGroup.Count()),
                                                                                    //Passengers = IsEnableOmniCartMVP2Changes(application.Id, application.Version.Major, true) ? GetPassengerNamesRemove(seatGroup, flightReservationResponse) : null,
                                                                                    PaxDetails = IsEnableOmniCartMVP2Changes(application.Id, application.Version.Major, true) ? GetPaxDetails(seatGroup, flightReservationResponse, flow, application) : null,
                                                                                    ProductDescription = IsEnableOmniCartMVP2Changes(application.Id, application.Version.Major, true) ? UtilityHelper.GetSeatDescription(seatGroup.Key) : string.Empty
                                                                                }).ToList().OrderBy(p => GetSeatPriceOrder()[p.SegmentDescription]).ToList()
                                                        }).ToList();
        }
        private List<MOBProductSegmentDetail> BuildProductSegmentsForSeats(FlightReservationResponse flightReservationResponse, List<MOBSeat> seats, List<MOBBKTraveler> BookingTravelerInfo, bool isPost, string flow,bool isMiles=false)
        {
            if (flightReservationResponse.DisplayCart.DisplaySeats == null || !flightReservationResponse.DisplayCart.DisplaySeats.Any())
                return null;

            var displaySeats = flightReservationResponse.DisplayCart.DisplaySeats.Clone();
            List<string> refundedSegmentNums = null;
            if (flightReservationResponse.Errors != null && flightReservationResponse.Errors.Any(e => e != null && e.MinorCode == "90506"))
            {
                var isRefundSuccess = IsRefundSuccess(flightReservationResponse.CheckoutResponse.ShoppingCartResponse.Items, out refundedSegmentNums, flow);
                //Remove pcu seats if refund Failed
                if (!isRefundSuccess)
                {
                    displaySeats.RemoveAll(ds => ds.PCUSeat);
                }
                if (!displaySeats.Any())
                    return null;
            }

            //Remove all failed seats other than pcu seats.
            displaySeats.RemoveAll(ds => !ds.PCUSeat && !CheckSeatAssignMessage(ds.SeatAssignMessage, isPost)); // string.IsNullOrEmpty(ds.SeatAssignMessage)
            if (_configuration.GetValue<bool>("EnablePCUFromSeatMapErrorCheckViewRes"))
            {
                displaySeats = HandleCSLDefect(flightReservationResponse, displaySeats, isPost);
            }
            if (!displaySeats.Any())
                return null;
            if (isMiles)
            {
                return displaySeats.OrderBy(d => d.OriginalSegmentIndex)
                                    .GroupBy(d => new { d.OriginalSegmentIndex, d.LegIndex })
                                    .Select(d => new MOBProductSegmentDetail
                                    {
                                        SegmentInfo = GetSegmentInfo(flightReservationResponse, d.Key.OriginalSegmentIndex, Convert.ToInt32(d.Key.LegIndex)),
                                        SubSegmentDetails = d.GroupBy(s => GetSeatTypeForDisplay(s, flightReservationResponse.DisplayCart.TravelOptions))
                                                            .Select(seatGroup => new MOBProductSubSegmentDetail
                                                            {
                                                                Price = String.Format("{0:0.00}", seatGroup.Select(s => s.MoneyAmount).ToList().Sum()),
                                                                MoneyAmount = String.Format("{0:0.00}", seatGroup.Select(s => s.MoneyAmount).ToList().Sum()),
                                                                MilesPrice = String.Format("{0:0}", seatGroup.Select(s => s.MilesAmount).ToList().Sum()),
                                                                DisplayMilesPrice = UtilityHelper.FormatAwardAmountForDisplay(String.Format("{0:0}", seatGroup.Select(s => s.MilesAmount).ToList().Sum()), false),
                                                                OrginalPrice = _configuration.GetValue<bool>("IsEnableManageResCoupon") ? String.Format("{0:0.00}", seatGroup.Select(s => s.OriginalPrice).ToList().Sum()) : string.Empty,
                                                                DisplayPrice = Decimal.Parse(seatGroup.Select(s => s.MoneyAmount).ToList().Sum().ToString()).ToString("c"),
                                                                DisplayOriginalPrice = _configuration.GetValue<bool>("IsEnableManageResCoupon") ? Decimal.Parse(seatGroup.Select(s => s.OriginalPrice).ToList().Sum().ToString()).ToString("c") : string.Empty,
                                                                StrikeOffPrice = GetOriginalTotalSeatPriceForStrikeOff(seatGroup.ToList(), seats, BookingTravelerInfo),
                                                                DisplayStrikeOffPrice = GetFormatedDisplayPriceForSeats(GetOriginalTotalSeatPriceForStrikeOff(seatGroup.ToList(), seats, BookingTravelerInfo)),
                                                                Passenger = seatGroup.Count().ToString() + (seatGroup.Count() > 1 ? " Travelers" : " Traveler"),
                                                                SeatCode = seatGroup.Key,
                                                                FlightNumber = seatGroup.Select(x => x.FlightNumber).FirstOrDefault(),
                                                                SegmentDescription = GetSeatTypeBasedonCode(seatGroup.Key, seatGroup.Count()),
                                                                IsPurchaseFailure = IsPurchaseFailed(seatGroup.Any(s => s.PCUSeat), d.Key.OriginalSegmentIndex.ToString(), refundedSegmentNums),
                                                                Miles = ConfigUtility.IsMilesFOPEnabled() ? seatGroup.Any(s => s.PCUSeat == true) ? 0 : _configuration.GetValue<int>("milesFOP") : 0,
                                                                DisplayMiles = ConfigUtility.IsMilesFOPEnabled() ? seatGroup.Any(s => s.PCUSeat == true) ? string.Empty : UtilityHelper.FormatAwardAmountForDisplay(_configuration.GetValue<string>("milesFOP"), false) : string.Empty,
                                                                StrikeOffMiles = ConfigUtility.IsMilesFOPEnabled() ? seatGroup.Any(s => s.PCUSeat == true) ? 0 : _configuration.GetValue<int>("milesFOP") : 0,
                                                                DisplayStrikeOffMiles = GetOriginalTotalSeatMilesPriceForStrikeOff(seatGroup.ToList(), seats, BookingTravelerInfo),
                                                                Currency = seatGroup.Where(s => s.SeatPrice > 0).Count() > 0 ? seatGroup.Where(s => s.SeatPrice > 0).Select(s => s.Currency).FirstOrDefault() : "USD"
                                                            }).ToList().OrderBy(p => GetSeatPriceOrder()[p.SegmentDescription]).ToList()
                                    }).ToList();
            }
            else
            {
                return displaySeats.OrderBy(d => d.OriginalSegmentIndex)
                                .GroupBy(d => new { d.OriginalSegmentIndex, d.LegIndex })
                                .Select(d => new MOBProductSegmentDetail
                                {
                                    SegmentInfo = GetSegmentInfo(flightReservationResponse, d.Key.OriginalSegmentIndex, Convert.ToInt32(d.Key.LegIndex)),
                                    SubSegmentDetails = d.GroupBy(s => GetSeatTypeForDisplay(s, flightReservationResponse.DisplayCart.TravelOptions))
                                                        .Select(seatGroup => new MOBProductSubSegmentDetail
                                                        {
                                                            Price = String.Format("{0:0.00}", seatGroup.Select(s => s.SeatPrice).ToList().Sum()),
                                                            OrginalPrice = _configuration.GetValue<bool>("IsEnableManageResCoupon") ? String.Format("{0:0.00}", seatGroup.Select(s => s.OriginalPrice).ToList().Sum()) : string.Empty,
                                                            DisplayPrice = Decimal.Parse(seatGroup.Select(s => s.SeatPrice).ToList().Sum().ToString()).ToString("c"),
                                                            DisplayOriginalPrice = _configuration.GetValue<bool>("IsEnableManageResCoupon") ? Decimal.Parse(seatGroup.Select(s => s.OriginalPrice).ToList().Sum().ToString()).ToString("c") : string.Empty,
                                                            StrikeOffPrice = GetOriginalTotalSeatPriceForStrikeOff(seatGroup.ToList(), seats, BookingTravelerInfo),
                                                            DisplayStrikeOffPrice = GetFormatedDisplayPriceForSeats(GetOriginalTotalSeatPriceForStrikeOff(seatGroup.ToList(), seats, BookingTravelerInfo)),
                                                            Passenger = seatGroup.Count().ToString() + (seatGroup.Count() > 1 ? " Travelers" : " Traveler"),
                                                            SeatCode = seatGroup.Key,
                                                            FlightNumber = seatGroup.Select(x => x.FlightNumber).FirstOrDefault(),
                                                            SegmentDescription = GetSeatTypeBasedonCode(seatGroup.Key, seatGroup.Count()),
                                                            IsPurchaseFailure = IsPurchaseFailed(seatGroup.Any(s => s.PCUSeat), d.Key.OriginalSegmentIndex.ToString(), refundedSegmentNums),
                                                            Miles = ConfigUtility.IsMilesFOPEnabled() ? seatGroup.Any(s => s.PCUSeat == true) ? 0 : _configuration.GetValue<int>("milesFOP") : 0,
                                                            DisplayMiles = ConfigUtility.IsMilesFOPEnabled() ? seatGroup.Any(s => s.PCUSeat == true) ? string.Empty : UtilityHelper.FormatAwardAmountForDisplay(_configuration.GetValue<string>("milesFOP"), false) : string.Empty,
                                                            StrikeOffMiles = ConfigUtility.IsMilesFOPEnabled() ? seatGroup.Any(s => s.PCUSeat == true) ? 0 : _configuration.GetValue<int>("milesFOP") : 0,
                                                            DisplayStrikeOffMiles = ConfigUtility.IsMilesFOPEnabled() ? seatGroup.Any(s => s.PCUSeat == true) ? string.Empty : UtilityHelper.FormatAwardAmountForDisplay(_configuration.GetValue<string>("milesFOP"), false) : string.Empty,
                                                            Currency = seatGroup.Where(s => s.SeatPrice > 0).Count() > 0 ? seatGroup.Where(s => s.SeatPrice > 0).Select(s => s.Currency).FirstOrDefault() : "USD"
                                                        }).ToList().OrderBy(p => GetSeatPriceOrder()[p.SegmentDescription]).ToList()
                                }).ToList();
            }
        }
        private string GetSeatTypeForDisplay(SeatAssignment s, TravelOptionsCollection travelOptions)
        {
            if (s == null)
                return string.Empty;

            if (s.PCUSeat)
                return GetCabinNameForPcuSeat(s.TravelerIndex, s.OriginalSegmentIndex, travelOptions);

            return GetCommonSeatCode(s.SeatPromotionCode);
        }
        private string GetOriginalTotalSeatPriceForStrikeOff(List<SeatAssignment> seatAssignments, List<MOBSeat> seats)
        {
            if (seatAssignments.Any(s => s.PCUSeat) || seats == null)
                return string.Empty;

            var seatsBySeatType = seats.Where(x => x.Origin == seatAssignments[0].DepartureAirportCode && x.Destination == seatAssignments[0].ArrivalAirportCode && x.FlightNumber == seatAssignments[0].FlightNumber && (UtilityHelper.GetCommonSeatCode(x.ProgramCode) == seatAssignments[0].SeatPromotionCode)).ToList();

            var originalPrice = seatsBySeatType.Sum(s => s.Price);
            var priceAfterCompanionRules = seatsBySeatType.Sum(s => s.PriceAfterTravelerCompanionRules);
            if (originalPrice > priceAfterCompanionRules)
            {
                return string.Format("{0:0.00}", originalPrice);
            }

            return string.Empty;
        }

        private Dictionary<string, int> GetSeatPriceOrder()
        {
            return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) {
                { "United First® Seats",1 },
                { "United First® Seat",1 },
                { "United Business® Seats",1 },
                { "United Business® Seat",1 },
                { "United Polaris℠ first Seats",1 },
                { "United Polaris℠ first Seat",1 },
                { "United Polaris℠ business Seats",1 },
                { "United Polaris℠ business Seat",1 },
                { "United® Premium Plus Seats",1 },
                { "United® Premium Plus Seat",1 },
                { "Economy Plus Seats", 2 },
                { "Economy Plus Seat", 3 },
                { "Economy Plus Seats (limited recline)", 4 },
                { "Economy Plus Seat (limited recline)", 5 },
                { "Preferred seats", 6 },
                { "Preferred seat", 7 },
                { "Advance Seat Assignments", 8 },
                { "Advance Seat Assignment", 9 },
                { "Seat assignments", 8 },
                { "Seat assignment", 9 },
                { string.Empty, 9 } };
        }
        public bool CheckSeatAssignMessage(FlightReservationResponse flightReservationResponse, string flow)
        {
            if (!flightReservationResponse.IsNullOrEmpty())
            {
                if (_configuration.GetValue<bool>("EnableCSL30BookingReshopSelectSeatMap")
                    && (flow == FlowType.BOOKING.ToString() || flow == FlowType.RESHOP.ToString()))
                {
                    return flightReservationResponse.DisplayCart.DisplaySeats.Any
                        (x => !x.IsNullOrEmpty() && !x.SeatAssignMessage.IsNullOrEmpty() && !x.SeatAssignMessage.Equals
                        ("SEATS ASSIGNED", StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    return flightReservationResponse.DisplayCart.DisplaySeats.Any(x => x.SeatAssignMessage != null);
                }
            }
            return false;
        }

        private bool IsFreeSeatCouponApplied(MOBProdDetail prodDetail, FlightReservationResponse flightReservationResponse)
        {
            return _configuration.GetValue<bool>("IsEnableManageResCoupon") && _mSCPageProductInfoHelper.isAFSCouponApplied(flightReservationResponse?.DisplayCart) && prodDetail?.Segments != null && prodDetail.Segments.Any(x => x != null && IsCouponApplied(x));
        }
        public async Task<MOBProdDetail> BuildProdDetailsForSeats(FlightReservationResponse flightReservationResponse, Persist.Definition.SeatChange.SeatChangeState state, string flow, MOBApplication application)
        {
            if (IsCheckinFlow(flow) && flightReservationResponse.DisplayCart.TravelOptions != null && flightReservationResponse.DisplayCart.TravelOptions.Any(x => x.Type == "SEATASSIGNMENTS"))
            {
                MOBProdDetail prod = new MOBProdDetail();
                prod.Code = "SEATASSIGNMENTS";
                prod.ProdDescription = string.Empty;
                decimal totalPrice = flightReservationResponse.DisplayCart.TravelOptions.Sum(x => x.Type == "SEATASSIGNMENTS" ? x.Amount : 0);
                if (flightReservationResponse.DisplayCart.DisplaySeats.Exists(ds => ds.Currency == "ML1") && await _featureSettings.GetFeatureSettingValue("EnableCheckInCloudMigrationMSC_23X"))
                {
                    totalPrice = flightReservationResponse.DisplayCart.DisplaySeats.Select(s => s.MoneyAmount).ToList().Sum();
                }
                prod.ProdTotalPrice = String.Format("{0:0.00}", totalPrice);
                prod.ProdDisplayTotalPrice = $"${prod.ProdTotalPrice}";
                if (totalPrice > 0)
                {
                    var displaySeats = flightReservationResponse.DisplayCart.DisplaySeats.Where(x => x.OriginalPrice > 0).Select(x => { x.SeatType = GetCommonSeatCode(x.SeatType); return x; }).GroupBy(x => $"{x.DepartureAirportCode} - {x.ArrivalAirportCode}");
                    prod.Segments = BuildCheckinSegmentDetail(displaySeats);
                }
                var seatMilesTotal = prod.Segments?.SelectMany(s => s?.SubSegmentDetails).ToList().Sum(s => s != null && !string.IsNullOrEmpty(s.MilesPrice) ? Convert.ToDecimal(s.MilesPrice) : 0);
                prod.ProdTotalRequiredMiles = String.Format("{0:0}", seatMilesTotal);
                return prod;
            }

            if (!string.IsNullOrEmpty(_configuration.GetValue<string>("EnablePCUSelectedSeatPurchaseViewRes")) && !IsCheckinFlow(flow))
            {
                if (!_configuration.GetValue<bool>("DisableMinusPriceSeatsegmentPopulatingIssueFix"))
                {
                    var seatTotalPrice = flightReservationResponse.DisplayCart.DisplaySeats.Select(d => d.SeatPrice).ToList().Sum();
                    var prod = new MOBProdDetail
                    {
                        Code = "SEATASSIGNMENTS",
                        ProdDescription = string.Empty,
                        ProdTotalPrice = seatTotalPrice > 0 ? String.Format("{0:0.00}", seatTotalPrice) : string.Format("{0:0.00}", 0),
                        ProdDisplayTotalPrice = seatTotalPrice > 0 ? seatTotalPrice.ToString("c") : 0.ToString("c"),
                        Segments = seatTotalPrice > 0 ? BuildProductSegmentsForSeats(flightReservationResponse, state?.Seats, application, flow) : null
                    };
                    if (_configuration.GetValue<bool>("IsEnableManageResCoupon") && (flow.Equals(FlowType.VIEWRES.ToString()) || flow.Equals(FlowType.VIEWRES_SEATMAP.ToString())) && _mSCPageProductInfoHelper.isAFSCouponApplied(flightReservationResponse.DisplayCart))
                        prod.Segments?.Select(x => x.SubSegmentDetails).ToList().ForEach(item => item.RemoveAll(k => Decimal.Parse(k.Price) == 0 && (string.IsNullOrEmpty(k.OrginalPrice) || Decimal.Parse(k.OrginalPrice) == 0)));
                    else
                        prod.Segments?.Select(x => x.SubSegmentDetails).ToList().ForEach(item => item.RemoveAll(k => Decimal.Parse(k.Price) == 0 && (k.StrikeOffPrice == string.Empty || Decimal.Parse(k.StrikeOffPrice) == 0)));

                    prod.Segments?.RemoveAll(k => k.SubSegmentDetails.Count == 0);
                    return prod;
                }
                else
                {
                    var prod = new MOBProdDetail
                    {
                        Code = "SEATASSIGNMENTS",
                        ProdDescription = string.Empty,
                        ProdTotalPrice = String.Format("{0:0.00}", flightReservationResponse.DisplayCart.DisplaySeats.Select(d => d.SeatPrice).ToList().Sum()),
                        ProdDisplayTotalPrice = Decimal.Parse(flightReservationResponse.DisplayCart.DisplaySeats.Select(d => d.SeatPrice).ToList().Sum().ToString()).ToString("c"),
                        Segments = BuildProductSegmentsForSeats(flightReservationResponse, state?.Seats, application, flow)
                    };
                    if (_configuration.GetValue<bool>("IsEnableManageResCoupon") && (flow.Equals(FlowType.VIEWRES.ToString()) || flow.Equals(FlowType.VIEWRES_SEATMAP.ToString())) && _mSCPageProductInfoHelper.isAFSCouponApplied(flightReservationResponse.DisplayCart))
                        prod.Segments.Select(x => x.SubSegmentDetails).ToList().ForEach(item => item.RemoveAll(k => Decimal.Parse(k.Price) == 0 && (string.IsNullOrEmpty(k.OrginalPrice) || Decimal.Parse(k.OrginalPrice) == 0)));
                    else
                        prod.Segments.Select(x => x.SubSegmentDetails).ToList().ForEach(item => item.RemoveAll(k => Decimal.Parse(k.Price) == 0 && (k.StrikeOffPrice == string.Empty || Decimal.Parse(k.StrikeOffPrice) == 0)));

                    prod.Segments.RemoveAll(k => k.SubSegmentDetails.Count == 0);
                    return prod;
                }
            }

            var prodDetail = new MOBProdDetail()
            {
                Code = "SEATASSIGNMENTS",
                ProdDescription = string.Empty,
                ProdTotalPrice = String.Format("{0:0.00}", flightReservationResponse.DisplayCart.DisplaySeats.Select(d => d.SeatPrice).ToList().Sum()),
                ProdDisplayTotalPrice = Decimal.Parse(flightReservationResponse.DisplayCart.DisplaySeats.Select(d => d.SeatPrice).ToList().Sum().ToString()).ToString("c"),
                //Mobile-1524: Include all the seats even if seat price is null when user has E+/Preferred subscriptions
                //                                                           |----Ignore the DisplaySeats ---------|------Updating the SeatPromotionCode to a common code for easy grouping ---------------|                                                                                                                            |--Ordering the DisplaySeats based---|--Group the resulted DisplaySeats objects based on OriginalSegmentIndex and LegIndex value and return the List of DisplaySeats -|   
                //       LEVEL 1                                             |----if SeatPromotionCode = null------|------and return the object using GetCommonCode method---------------------------------|----------Ignoring DisplaySeats child object if either SeatPrice = 0  ------------------------------------------------      |--on OriginalSegmentIndex-----------|--This is been done for COG, THRU flights. These flights even though are one segment at high level but have multiple segments---|
                Segments = flightReservationResponse.DisplayCart.DisplaySeats.Where(x => x.SeatPromotionCode != null).Select(x => { x.SeatPromotionCode = GetCommonSeatCode(x.SeatPromotionCode); return x; }).Where(d => (state != null ? (state.TotalEplusEligible > 0 ? true : d.SeatPrice != 0) : d.SeatPrice != 0)).OrderBy(d => d.OriginalSegmentIndex).GroupBy(d => new { d.OriginalSegmentIndex, d.LegIndex }).Select(d => new MOBProductSegmentDetail
                //Segments = flightReservationResponse.DisplayCart.DisplaySeats.Where(x => x.SeatPromotionCode != null).Select(x => { x.SeatPromotionCode = GetCommonSeatCode(x.SeatPromotionCode); return x; }).Where(d => (true? d.SeatPrice != 0 : d.SeatPrice >= 0)).OrderBy(d =>d.Orih]]]]]]]]]]]ginalSegmentIndex).GroupBy(d => new { d.OriginalSegmentIndex, d.LegIndex }).Select(d => new MOBProductSegmentDetail
                {
                    //                |--------------Get the individual Segment Origin and Destination detail based on OriginalSegmentIndex and LegIndex for the list of DisplaySeats from LEVEL 1---| 
                    SegmentInfo = GetSegmentInfo(flightReservationResponse, d.Select(u => u.OriginalSegmentIndex).FirstOrDefault(), Convert.ToInt32(d.Select(u => u.LegIndex).FirstOrDefault())),
                    ProductId = null,
                    //     LEVEL 2         |---- Further group the LEVEL 1 list of DisplaySeats based on OriginalSegmentIndex and SeatPromotionCode to get SubSegmentDetails--|
                    SubSegmentDetails = d.GroupBy(t => new { t.OriginalSegmentIndex, t.SeatPromotionCode }).Select(t => new MOBProductSubSegmentDetail
                    {
                        //              |--Getting the sum of SeatPrice from the list of DisplaySeats at LEVEL 2 --|
                        Price = String.Format("{0:0.00}", t.Select(s => s.SeatPrice).ToList().Sum()),
                        DisplayPrice = Decimal.Parse(t.Select(s => s.SeatPrice).ToList().Sum().ToString()).ToString("c"),
                        //                  | --Getting the count of list of DisplaySeats at LEVEL 2-- |
                        Passenger = t.Count().ToString() + (t.Count() > 1 ? " Travelers" : " Traveler"), // t.GroupBy(u => u.TravelerIndex).Count().ToString() + (t.GroupBy(u => u.TravelerIndex).Count() > 1 ? " Travelers" : " Traveler"),
                        SeatCode = t.Select(u => u.SeatPromotionCode).FirstOrDefault(),
                        FlightNumber = t.Select(x => x.FlightNumber).FirstOrDefault(),
                        // DepartureTime = flightReservationResponse.Reservation.FlightSegments.Where(s => s.FlightSegment == ),
                        //                          | --Getting the SeatDescription based on SeatPromotionCode from the list of DisplaySeats at LEVEL 2, TravelerIndex count for pluralizing the text. -- |
                        SegmentDescription = GetSeatTypeBasedonCode(t.Select(u => u.SeatPromotionCode).FirstOrDefault(), t.GroupBy(u => u.TravelerIndex).Count())
                        //             | -- Once Get the final list ordering them based on the order defined in GetSEatPriceOrder method comparing with list SegmentDescription -- |
                    }).ToList().OrderBy(p => GetSeatPriceOrder()[p.SegmentDescription]).ToList()
                }).ToList(),
            };

            if (state == null ? false : (state.TotalEplusEligible > 0))
            {
                foreach (var segmnt in prodDetail.Segments)
                {
                    var subSegments = segmnt.SubSegmentDetails;
                    foreach (var subSegment in subSegments)
                    {
                        if (state.Seats != null)
                        {
                            var sbSegments = state.Seats.Where(x => x.Origin == segmnt.SegmentInfo.Substring(0, 3) && x.Destination == segmnt.SegmentInfo.Substring(6, 3) && x.FlightNumber == subSegment.FlightNumber && (GetCommonSeatCode(x.ProgramCode) == subSegment.SeatCode)).ToList();
                            decimal totalPrice = sbSegments.Select(u => u.Price).ToList().Sum();
                            decimal discountPrice = sbSegments.Select(u => u.PriceAfterTravelerCompanionRules).ToList().Sum();
                            if (discountPrice < totalPrice)
                            {
                                subSegment.StrikeOffPrice = String.Format("{0:0.00}", sbSegments.Select(u => u.Price).ToList().Sum().ToString());
                                subSegment.DisplayStrikeOffPrice = Decimal.Parse(sbSegments.Select(u => u.Price).ToList().Sum().ToString()).ToString("c");
                            }
                            subSegment.Passenger = sbSegments.Count() + (sbSegments.Count() > 1 ? " Travelers" : " Traveler");
                            subSegment.Price = String.Format("{0:0.00}", sbSegments.Select(u => u.PriceAfterTravelerCompanionRules).ToList().Sum().ToString());
                            subSegment.DisplayPrice = Decimal.Parse(sbSegments.Select(u => u.PriceAfterTravelerCompanionRules).ToList().Sum().ToString()).ToString("c");
                        }
                    }
                }
            }
            //Mobile-1855: Remove segments with no seats to purchase
            prodDetail.Segments.Select(x => x.SubSegmentDetails).ToList().ForEach(item => item.RemoveAll(k => k.Price == "0" && (k.StrikeOffPrice == "0" || k.StrikeOffPrice == string.Empty)));
            prodDetail.Segments.RemoveAll(k => k.SubSegmentDetails.Count == 0);
            return prodDetail;
        }
        private double GetProductFee(bool isPost, United.Service.Presentation.InteractionModel.ShoppingCart flightReservationResponseShoppingCart, string flow, string productCode)
        {
            return isPost ? flightReservationResponseShoppingCart.Items.SelectMany(d => d.Product).Where(d => d.Code == productCode).FirstOrDefault().Price.Totals.FirstOrDefault().Amount :
                                  flightReservationResponseShoppingCart.Items.SelectMany(d => d.Product).Where(d => d.Code == productCode).SelectMany(x => x.Price.Totals).FirstOrDefault().Amount;
        }
        private async Task<List<MOBProdDetail>> BuildProductDetailsForInflightMeals(FlightReservationResponse flightReservationResponse, string productCode, string sessionId, bool isPost)
        {
            List<MOBInFlightMealsRefreshmentsResponse> savedResponse =
          await _sessionHelperService.GetSession<List<MOBInFlightMealsRefreshmentsResponse>>(sessionId, (new MOBInFlightMealsRefreshmentsResponse()).GetType().FullName, new List<string> { sessionId, (new MOBInFlightMealsRefreshmentsResponse()).GetType().FullName }).ConfigureAwait(false);
            United.Service.Presentation.InteractionModel.ShoppingCart flightReservationResponseShoppingCart;
            if (isPost)
                flightReservationResponseShoppingCart = flightReservationResponse.CheckoutResponse.ShoppingCart;
            else
                flightReservationResponseShoppingCart = flightReservationResponse.ShoppingCart;


            var displayTotalPrice = flightReservationResponse.DisplayCart.DisplayPrices.FirstOrDefault(o => (o.Description != null && o.Description.Equals("Total", StringComparison.OrdinalIgnoreCase))).Amount;
            var grandTotal = flightReservationResponseShoppingCart?.Items.SelectMany(p => p.Product).Where(d => d.Code == "POM")?.Select(p => p.Price?.Totals?.FirstOrDefault().Amount).FirstOrDefault();

            var travelOptions = GetTravelOptionItems(flightReservationResponse, productCode);
            // For RegisterOffer uppercabin when there is no price no need to build the product
            List<MOBProdDetail> response = new List<MOBProdDetail>();
            if (grandTotal > 0 && productCode == _configuration.GetValue<string>("InflightMealProductCode"))
            {
                var productDetail = new MOBProdDetail()
                {
                    Code = travelOptions.Where(d => d.Key == productCode).Select(d => d.Key).FirstOrDefault().ToString(),
                    ProdDescription = travelOptions.Where(d => d.Key == productCode).Select(d => d.Type).FirstOrDefault().ToString(),
                    ProdTotalPrice = String.Format("{0:0.00}", grandTotal),
                    ProdDisplayTotalPrice = grandTotal?.ToString("c"),
                    Segments = GetProductSegmentForInFlightMeals(flightReservationResponse, savedResponse, travelOptions, flightReservationResponseShoppingCart),
                };
                response.Add(productDetail);
                return response;
            }
            else return response;

        }
        public List<MOBProductSegmentDetail> GetProductSegmentForInFlightMeals(FlightReservationResponse flightReservationResponse,
 List<MOBInFlightMealsRefreshmentsResponse> savedResponse, Collection<TravelOption> travelOptions, United.Service.Presentation.InteractionModel.ShoppingCart flightReservationResponseShoppingCart)
        {
            List<MOBProductSegmentDetail> response = new List<MOBProductSegmentDetail>();
            MOBProductSegmentDetail segmentDetail = new MOBProductSegmentDetail();
            List<MOBProductSubSegmentDetail> subSegmentDetails = new List<MOBProductSubSegmentDetail>();
            var traveler = flightReservationResponse?.Reservation?.Travelers;
            string productCode = _configuration.GetValue<string>("InflightMealProductCode");

            var subProducts = flightReservationResponseShoppingCart.Items
           ?.Where(a => a.Product != null)
           ?.SelectMany(b => b.Product)
           ?.Where(c => c.SubProducts != null && c.SubProducts.Any(d => d.Code == _configuration.GetValue<string>("InflightMealProductCode")))
           ?.SelectMany(d => d.SubProducts);

            var characterStics = flightReservationResponseShoppingCart.Items
           ?.Where(a => a.Product != null)
           ?.SelectMany(b => b.Product)
           ?.Where(c => c.Code == productCode)
           ?.SelectMany(d => d.Characteristics)
           ?.Where(e => e.Code == "SegTravProdSubGroupIDQtyPrice")
           ?.FirstOrDefault();

            string[] items = characterStics.Value.Split(',');
            List<Tuple<string, string, int, string>> tupleList = new List<Tuple<string, string, int, string>>();

            if (items != null && items.Length > 0)
            {
                string[] selectedItems = null;
                foreach (var item in items)
                {
                    //segmentID - TravelerID - ProductID - SubGroupID - Quantity - Price
                    if (item != "")
                        selectedItems = item.Split('|');
                    if (selectedItems != null && selectedItems.Length > 0)
                    {
                        //TravelerID - ProductID - SubGroupID - Quantity - Price
                        tupleList.Add(Tuple.Create(selectedItems[2], selectedItems[3], Convert.ToInt32(selectedItems[4]), selectedItems[5]));
                    }
                }
            }
            for (int i = 0; i < flightReservationResponse.Reservation.Travelers.Count; i++)
            {
                if (response.Count == 0)
                    segmentDetail.SegmentInfo = GetSegmentDescription(travelOptions);
                List<MOBProductSubSegmentDetail> snackDetails = new List<MOBProductSubSegmentDetail>();
                int travelerCouter = 0;
                int prodCounter = 0;
                foreach (var subProduct in subProducts)
                {
                    MOBProductSubSegmentDetail segDetail = new MOBProductSubSegmentDetail();
                    if (subProduct.Prices.Where(a => a.Association.TravelerRefIDs[0] == (i + 1).ToString()).Any())
                    {
                        if (subProduct != null && subProduct.Extension != null)
                        {
                            var priceInfo = subProduct.Prices.Where(a => a.Association.TravelerRefIDs[0] == (i + 1).ToString()).FirstOrDefault();
                            double price = priceInfo.PaymentOptions.FirstOrDefault().PriceComponents.FirstOrDefault().Price.Totals.FirstOrDefault().Amount;
                            var tupleSelectedItem = tupleList.Where(a => a.Item2 == subProduct.SubGroupCode && a.Item1 == priceInfo.ID).FirstOrDefault();

                            if (tupleSelectedItem != null)
                            {
                                if (_configuration.GetValue<bool>("EnableisEditablePOMFeature"))
                                {
                                    if (price > 0 && subProduct.Extension.MealCatalog?.MealShortDescription != null)
                                    {
                                        if (prodCounter == 0 && travelerCouter == 0)
                                        {
                                            segDetail.Passenger = traveler[i].Person.GivenName.ToLower().ToPascalCase() + " " + traveler[i].Person.Surname.ToLower().ToPascalCase();
                                            segDetail.Price = "0";
                                            snackDetails.Add(segDetail);
                                            segDetail = new MOBProductSubSegmentDetail();

                                            segDetail.SegmentDescription = subProduct.Extension.MealCatalog?.MealShortDescription + " x " + tupleSelectedItem.Item3;
                                            segDetail.DisplayPrice = "$" + String.Format("{0:0.00}", price * tupleSelectedItem.Item3);
                                            segDetail.Price = price.ToString();
                                        }
                                        else
                                        {
                                            segDetail.SegmentDescription = subProduct.Extension.MealCatalog?.MealShortDescription + " x " + tupleSelectedItem.Item3;
                                            segDetail.DisplayPrice = "$" + String.Format("{0:0.00}", price * tupleSelectedItem.Item3);
                                            segDetail.Price = price.ToString();
                                        }
                                        prodCounter++;
                                        snackDetails.Add(segDetail);
                                    }
                                }
                                else
                                {
                                    //  int quantity = GetQuantity(travelOptions, subProduct.SubGroupCode, subProduct.Prices.Where(a=>a.ID == (i+1).ToString()).Select(b=>b.ID).ToString());
                                    if (prodCounter == 0 && travelerCouter == 0)
                                    {
                                        segDetail.Passenger = traveler[i].Person.GivenName.ToLower().ToPascalCase() + " " + traveler[i].Person.Surname.ToLower().ToPascalCase();
                                        segDetail.Price = "0";
                                        snackDetails.Add(segDetail);
                                        segDetail = new MOBProductSubSegmentDetail();

                                        segDetail.SegmentDescription = subProduct.Extension.MealCatalog?.MealShortDescription + " x " + tupleSelectedItem.Item3;
                                        segDetail.DisplayPrice = "$" + String.Format("{0:0.00}", price * tupleSelectedItem.Item3);
                                        segDetail.Price = price.ToString();
                                    }
                                    else
                                    {
                                        segDetail.SegmentDescription = subProduct.Extension.MealCatalog?.MealShortDescription + " x " + tupleSelectedItem.Item3;
                                        segDetail.DisplayPrice = "$" + String.Format("{0:0.00}", price * tupleSelectedItem.Item3);
                                        segDetail.Price = price.ToString();
                                    }
                                    prodCounter++;
                                    snackDetails.Add(segDetail);
                                }
                            }
                        }
                    }
                }
                if (segmentDetail.SubSegmentDetails == null) segmentDetail.SubSegmentDetails = new List<MOBProductSubSegmentDetail>();
                if (snackDetails != null)
                    segmentDetail.SubSegmentDetails.AddRange(snackDetails);
                travelerCouter++;

            }
            if (segmentDetail != null && segmentDetail.SubSegmentDetails != null && !response.Contains(segmentDetail))
                response.Add(segmentDetail);
            return response;
        }
        private string GetSegmentDescription(Collection<TravelOption> travelOptions)
        {
            if (travelOptions == null) return string.Empty;
            var refSegments = travelOptions.SelectMany(a => a.SubItems)?.Select(b => b.Product)?.Select(c => c.SubProducts?.FirstOrDefault().ReferencedSegments.FirstOrDefault());
            return refSegments?.FirstOrDefault().Origin + " - " + refSegments?.FirstOrDefault().Destination;
        }
        private Collection<TravelOption> GetTravelOptionItems(FlightReservationResponse flightReservationResponse, string productCode)
        {
            var travelOptions = flightReservationResponse.DisplayCart.TravelOptions.Where(d => d.Key == productCode).ToCollection().Clone();

            //when pcu seat is select, we will have duplicate items in travelOptions we need ignore those items
            if (flightReservationResponse?.DisplayCart?.DisplaySeats?.Any(s => s?.PCUSeat ?? false) ?? false)
            {
                travelOptions.Where(t => t.Key == "PCU")
                                .ForEach(t => t.SubItems.RemoveWhere(sb => sb.Amount == 0 || flightReservationResponse.DisplayCart.DisplaySeats.Any(s => s.PCUSeat && s.OriginalSegmentIndex.ToString() == sb.SegmentNumber && (s.TravelerIndex + 1).ToString() == sb.TravelerRefID)));
                travelOptions.RemoveWhere(t => t.SubItems == null || !t.SubItems.Any());
            }

            return travelOptions;
        }
        public bool IsRefundSuccess(Collection<ShoppingCartItemResponse> items, out List<string> refundedSegments, string flow)
        {
            refundedSegments = null;
            var item = items.FirstOrDefault(i => i != null && i.Item != null && !string.IsNullOrEmpty(i.Item.Category) && i.Item.Category.Equals("Reservation.Merchandise.PCU"));
            if (item == null) return false;

            var productContext = string.Empty;
            if (ConfigUtility.IsViewResFlowCheckOut(flow) && !_configuration.GetValue<bool>("DisableFixForPCUPurchaseFailMsg_MOBILE15837"))
            {
                //productContext = item.Item.ProductContext.FirstOrDefault(p => p.StartsWith("["));
                foreach (var p in item.Item.ProductContext)
                    if (!p.IsNullOrEmpty())
                    {
                        //var refundedProductContext = JsonConvert.DeserializeObject<Collection<Genre>>(p);
                        var refundedProductContext = DataContextJsonSerializer.NewtonSoftDeserializeIgnoreErrorAndReturnNull<Collection<Genre>>(p);
                        if (!refundedProductContext.IsNullOrEmpty())
                        {
                            productContext = p;
                            break;
                        }
                    }
                if (productContext.IsNullOrEmpty()) return false;
            }
            else
            {
                productContext = item.Item.ProductContext.FirstOrDefault(p => !p.StartsWith("<"));
                if (productContext == null) return false;
            }
            var refundInfo = DataContextJsonSerializer.NewtonSoftDeserializeIgnoreErrorAndReturnNull<Collection<Genre>>(productContext);
            //var refundInfo = JsonConvert.DeserializeObject<Collection<Genre>>(productContext);
            if (refundInfo != null && refundInfo.Any() && !refundInfo.Any(g => containsKey(g, "REFUNDFAILED")) &&
                refundInfo.Any(g => containsKey(g, "REFUNDED")))
            {
                refundedSegments = refundInfo.Where(g => containsKey(g, "REFUNDED")).Select(s => s.Value.Split(' ')[1]).Distinct().ToList();
                return true;
            }

            return false;
        }
        private bool containsKey(Genre g, string key)
        {
            return g != null && !string.IsNullOrEmpty(g.Description) && g.Description.ToUpper().Contains(key);
        }
        public bool ShouldIgnoreAmount(SubItem subItem)
        {
            if (string.IsNullOrEmpty(subItem.Reason))
                return false;

            return subItem.Reason.ToUpper().Equals("INF");
        }
        public bool ShouldIgnoreAmount(SubItem subItem, FlightReservationResponse flightReservationResponse, string flow)
        {
            if (string.IsNullOrEmpty(subItem.Reason))
                return false;

            return subItem.Reason.ToUpper().Equals("INF");
        }
        private String GetPassengerText(string flow, string productCode, int travelerCount, int? reservationTravelerCount)
        {
            //required for SDC since eservice and shopping cart do not return price per passenger so existing logic doesnt work for getting the traveler count
            int numTravelers = IsCheckinFlow(flow) && productCode == "SDC"
                ? reservationTravelerCount.HasValue ? reservationTravelerCount.Value : 0
                : travelerCount;
            return $"{numTravelers} Traveler{(numTravelers > 1 ? "s" : String.Empty)}";
        }
        private string BuildProductDescription(Collection<TravelOption> travelOptions, IGrouping<string, SubItem> t, string productCode, string flow, string bundleTitle, string safProductName)
        {
            if (string.IsNullOrEmpty(productCode))
                return string.Empty;

            productCode = productCode.ToUpper().Trim();

            if (productCode == "AAC")
                return "Award Accelerator®";

            if (productCode == "PAC")
                return "Premier Accelerator℠";

            if (ConfigUtility.IsViewResFlowCheckOut(flow) && productCode == "TPI" && _configuration.GetValue<bool>("GetTPIProductName_HardCode"))
                return "Trip insurance";
            else if (productCode == "TPI" && _configuration.GetValue<bool>("GetTPIProductName_HardCode"))
                return "Travel insurance";

            if (productCode == "FARELOCK")
                return "FareLock";

            if (productCode == "SDC")
                return "Flight change";

            if (productCode == "SFC")
                return !string.IsNullOrEmpty(safProductName) ? safProductName : "Sustainable Flight Fund";

            if (ConfigUtility.IsViewResFlowCheckOut(flow) && _configuration.GetValue<bool>("EnableBasicEconomyBuyOutInViewRes") && productCode == "BEB")
                return !_configuration.GetValue<bool>("EnableNewBEBContentChange") ? "Switch to Economy" : _configuration.GetValue<string>("BEBuyOutPaymentInformationMessage");

            if (productCode == "PCU")
                return ConfigUtility.GetFormattedCabinName(t.Select(u => u.Description).FirstOrDefault().ToString());
            if (productCode == "BAG")
            {
                var bagCount = t.Select(x => x.Amount > 0 ? x.Count : 0).Sum();
                return $"{bagCount} bag{(bagCount > 1 ? "s" : string.Empty)} for purchase";
            };

            if (!string.IsNullOrEmpty(bundleTitle))
                return bundleTitle;

            return travelOptions.Where(d => d.Key == productCode).Select(d => d.Type).FirstOrDefault().ToString();
        }



        private bool IsCheckinFlow(string flowName)
        {
            FlowType flowType;
            if (!System.Enum.TryParse(flowName, out flowType))
                return false;
            return flowType == FlowType.CHECKIN;
        }

        private string GetProductDescription(Collection<TravelOption> travelOptions, string productCode, string flow)
        {
            if (string.IsNullOrEmpty(productCode))
                return string.Empty;

            productCode = productCode.ToUpper().Trim();
            var key = travelOptions != null && travelOptions.Any(d => d.Key == productCode) ? travelOptions.Where(d => d.Key == productCode).Select(d => d.Type).FirstOrDefault().ToString() : string.Empty;
            if (ConfigUtility.IsViewResFlowCheckOut(flow) && productCode == "TPI")
                return "Travel insurance";
            else if (productCode == "TPI")
                return "Trip insurance";
            if (productCode == "FARELOCK")
                return "FareLock";
            if (key == "BE")
                return "Travel Options Bundle";
            if (new[] { "EFS", "ECONOMY-MERCH-EPLUS" }.Any(x => x == key || x == productCode))
                return $"Economy Plus{(char)174} ";
            return string.Empty;
        }

        private async Task<List<string>> GetProductDetailDescrption(IGrouping<String, SubItem> subItem, string productCode, string sessionId, bool isBundleProduct, bool isMRBundle, List<BundleDescriptionPersist> bundleDescriptionPersist)
        {
            List<string> prodDetailDescription = new List<string>();
            // MOBILE-25395: SAF
            var safCode = _configuration.GetValue<string>("SAFCode");

            if (string.Equals(productCode, "EFS", StringComparison.OrdinalIgnoreCase))
            {
                prodDetailDescription.Add("Included with your fare");
            }

            // MOBILE-25395: SAF
            if ((isBundleProduct || string.Equals(productCode, safCode, StringComparison.OrdinalIgnoreCase)) && !string.IsNullOrEmpty(sessionId))
            {
                MOBBookingBundlesResponse bundleResponse = new MOBBookingBundlesResponse(_configuration);
                bundleResponse = await _sessionHelperService.GetSession<MOBBookingBundlesResponse>(sessionId, bundleResponse.ObjectName, new List<string> { sessionId, bundleResponse.ObjectName }).ConfigureAwait(false);

                if (bundleResponse != null)
                {
                    var selectedBundleResponse = bundleResponse.Products?.FirstOrDefault(p => string.Equals(p.ProductCode, productCode, StringComparison.OrdinalIgnoreCase));
                    if (selectedBundleResponse != null)
                    {
                        // MOBILE-25395: SAF
                        if (string.Equals(productCode, safCode, StringComparison.OrdinalIgnoreCase))
                        {
                            prodDetailDescription.Add(selectedBundleResponse.ProductName);
                        }
                        else
                        {
                            prodDetailDescription.AddRange(selectedBundleResponse.Tile.OfferDescription);
                        }
                    }
                }
            }
            if (isMRBundle && bundleDescriptionPersist != null && bundleDescriptionPersist.Count > 0)
            {
                if (!_configuration.GetValue<bool>("FixBundleTitleWhenProductDescriptionIsEmpty"))
                    bundleDescriptionPersist?.ForEach(x => AddBundleTitleInProductDescriptions(x?.Title, prodDetailDescription));
                else
                    bundleDescriptionPersist?.ForEach(x => prodDetailDescription.Add(x?.Title));
            }

            return prodDetailDescription;
        }

        private void AddBundleTitleInProductDescriptions(string title, List<string> prodDetailDescription)
        {
            if (!string.IsNullOrEmpty(title))
                prodDetailDescription.Add(title);
        }

        public string GetSeatTypeBasedonCode(string seatCode, int travelerCount, bool isCheckinPath = false)
        {
            string seatType = string.Empty;

            switch (seatCode.ToUpper().Trim())
            {
                case "SXZ": //StandardPreferredExitPlus
                case "SZX": //StandardPreferredExit
                case "SBZ": //StandardPreferredBlukheadPlus
                case "SZB": //StandardPreferredBlukhead
                case "SPZ": //StandardPreferredZone
                case "PZA":
                    seatType = (travelerCount > 1) ? "Preferred seats" : "Preferred seat";
                    break;
                case "SXP": //StandardPrimeExitPlus
                case "SPX": //StandardPrimeExit
                case "SBP": //StandardPrimeBlukheadPlus
                case "SPB": //StandardPrimeBlukhead
                case "SPP": //StandardPrimePlus
                case "PPE": //StandardPrime
                case "BSA":
                case "ASA":
                    if (isCheckinPath)
                        seatType = (travelerCount > 1) ? "Seat assignments" : "Seat assignment";
                    else
                        seatType = (travelerCount > 1) ? "Advance seat assignments" : "Advance seat assignment";
                    break;
                case "EPL": //EplusPrime
                case "EPU": //EplusPrimePlus
                case "BHS": //BulkheadPrime
                case "BHP": //BulkheadPrimePlus  
                case "PSF": //PrimePlus  
                    seatType = (travelerCount > 1) ? "Economy Plus Seats" : "Economy Plus Seat";
                    break;
                case "PSL": //Prime                            
                    seatType = (travelerCount > 1) ? "Economy Plus Seats (limited recline)" : "Economy Plus Seat (limited recline)";
                    break;
                default:
                    var pcuCabinName = ConfigUtility.GetFormattedCabinName(seatCode);
                    if (!string.IsNullOrEmpty(pcuCabinName))
                    {
                        return pcuCabinName + ((travelerCount > 1) ? " Seats" : " Seat");
                    }
                    return string.Empty;
            }
            return seatType;
        }
        private List<MOBTypeOption> GetBagsLineItems(MOBBag bagCount)
        {
            List<MOBTypeOption> lineItems = new List<MOBTypeOption>();

            lineItems.Add(new MOBTypeOption() { Key = "Total bags:", Value = $"{bagCount.TotalBags} bag{(bagCount.TotalBags > 1 ? "s" : String.Empty)}" });

            if (bagCount.OverWeightBags > 0)
                lineItems.Add(new MOBTypeOption() { Key = "Bags over 50 lbs:", Value = $"{bagCount.OverWeightBags} bag{(bagCount.OverWeightBags > 1 ? "s" : String.Empty)}" });

            if (bagCount.OverWeight2Bags > 0)
                lineItems.Add(new MOBTypeOption() { Key = "Bags over 70 lbs:", Value = $"{bagCount.OverWeight2Bags} bag{(bagCount.OverWeight2Bags > 1 ? "s" : String.Empty)}" });

            if (bagCount.OverSizeBags > 0)
                lineItems.Add(new MOBTypeOption() { Key = "Bags over 60 in:", Value = $"{bagCount.OverSizeBags} bag{(bagCount.OverSizeBags > 1 ? "s" : String.Empty)}" });

            int totalExceptionBags = bagCount.ExceptionItemHockeySticks + bagCount.ExceptionItemInfantSeat + bagCount.ExceptionItemInfantStroller + bagCount.ExceptionItemSkiBoots + bagCount.ExceptionItemWheelChair;
            if (totalExceptionBags > 0)
                lineItems.Add(new MOBTypeOption() { Key = "Exception items:", Value = $"{totalExceptionBags} bag{(totalExceptionBags > 1 ? "s" : String.Empty)}" });

            return lineItems;
        }

        private void UpdateRefundTotal(MOBProdDetail prod, bool isMiles = false)
        {
            decimal otherPrice = RefundedPriceOrTotalAfterRefund(prod.Segments);
            var chargedTotalPrice = GetChargedAmountFromSegments(prod.Segments, isMiles);
            if (isMiles)
            {
                prod.ProdTotalRequiredMiles = String.Format("{0:0}", chargedTotalPrice);
                prod.ProdDisplayTotalRequiredMiles = chargedTotalPrice > 0 ? UtilityHelper.FormatAwardAmountForDisplay(Decimal.Parse(chargedTotalPrice.ToString()).ToString(), false) : string.Empty;
            }
            else
            {
                prod.ProdTotalPrice = String.Format("{0:0.00}", chargedTotalPrice);
                prod.ProdDisplayTotalPrice = chargedTotalPrice > 0 ? Decimal.Parse(chargedTotalPrice.ToString()).ToString("c") : string.Empty;
            }

            if (prod.Segments != null && prod.Segments.Any() && prod.Segments.TrueForAll(s => s.SubSegmentDetails.Any(sb => sb.IsPurchaseFailure)))
            {
                prod.Segments = null;
                prod.ProdTotalPrice = string.Empty;
                prod.ProdDisplayTotalPrice = string.Empty;
            }

            if (otherPrice > 0)
            {
                prod.ProdOtherPrice = String.Format("{0:0.00}", otherPrice);
                prod.ProdDisplayOtherPrice = Decimal.Parse(otherPrice.ToString()).ToString("c");
            }
        }
        private decimal RefundedPriceOrTotalAfterRefund(List<MOBProductSegmentDetail> segments)
        {
            if (segments == null || !segments.Any())
                return 0;

            var chargedTotalPrice = GetChargedAmountFromSegments(segments);
            var refundedTotalPrice = segments.SelectMany(s => s.SubSegmentDetails).ToList().Sum(s => s != null && s.IsPurchaseFailure && !string.IsNullOrEmpty(s.Price) ? Convert.ToDecimal(s.Price) : 0);
            if (refundedTotalPrice > 0 && chargedTotalPrice > refundedTotalPrice)
            {
                return chargedTotalPrice - refundedTotalPrice;
            }

            return refundedTotalPrice;
        }

        private decimal GetChargedAmountFromSegments(List<MOBProductSegmentDetail> segments, bool isMiles=false)
        {
            if (segments == null || !segments.Any())
                return 0;
            if (isMiles) { 
                return segments.SelectMany(s => s.SubSegmentDetails).ToList().Sum(s => s != null && !string.IsNullOrEmpty(s.MilesPrice) ? Convert.ToDecimal(s.MilesPrice) : 0);
            }
            else return segments.SelectMany(s => s.SubSegmentDetails).ToList().Sum(s => s != null && !string.IsNullOrEmpty(s.Price) ? Convert.ToDecimal(s.Price) : 0);
        }
        private bool IsOriginalPriceExists(MOBProdDetail prodDetail)
        {
            return !_configuration.GetValue<bool>("DisableFreeCouponFix")
                   && !string.IsNullOrEmpty(prodDetail.ProdOriginalPrice)
                   && Decimal.TryParse(prodDetail.ProdOriginalPrice, out decimal originalPrice)
                   && originalPrice > 0;
        }
        private List<MOBProductSegmentDetail> BuildCheckinSegmentDetail(IEnumerable<IGrouping<string, SeatAssignment>> seatAssignmentGroup)
        {
            List<MOBProductSegmentDetail> segmentDetails = new List<MOBProductSegmentDetail>();
            seatAssignmentGroup.ForEach(seatSegment => segmentDetails.Add(new MOBProductSegmentDetail()
            {
                SegmentInfo = seatSegment.Key,
                SubSegmentDetails = BuildSubsegmentDetails(seatSegment.ToList()).OrderBy(p => GetSeatPriceOrder()[p.SegmentDescription]).ToList()
            }));
            return segmentDetails;
        }

        private List<MOBProductSubSegmentDetail> BuildSubsegmentDetails(List<SeatAssignment> seatAssignments)
        {
            List<MOBProductSubSegmentDetail> subSegmentDetails = new List<MOBProductSubSegmentDetail>();
            var groupedByTypeAndPrice = seatAssignments.GroupBy(s => s.SeatType, (key, grpSeats) => new { SeatType = key, OriginalPrice = grpSeats.Sum(x => x.OriginalPrice), 
                SeatPrice = grpSeats.Sum(x => x.SeatPrice), Count = grpSeats.Count(), MilesPrice = grpSeats.Sum(x => x.MilesAmount),
                MoneyAmount = grpSeats.Where(x => x.SeatPrice != 0).Sum(x => x.MoneyAmount) });
            var forCurrency = seatAssignments.Where(s => s.SeatPrice > 0).FirstOrDefault();
            string currency = "USD";
            if (forCurrency != null)
                currency = forCurrency.Currency;

            groupedByTypeAndPrice.ForEach(grpSeats =>
            {
                subSegmentDetails.Add(PopulateSubsegmentDetails(grpSeats.SeatType, grpSeats.OriginalPrice, grpSeats.SeatPrice, grpSeats.Count, grpSeats.MilesPrice, grpSeats.MoneyAmount, currency));
            });
            return subSegmentDetails;
        }

        private MOBProductSubSegmentDetail PopulateSubsegmentDetails(string seatType, decimal originalPrice, decimal seatPrice, int count, double milesPrice, decimal moneyAmount, string Currency)
        {
            MOBProductSubSegmentDetail subsegmentDetail = new MOBProductSubSegmentDetail();
            subsegmentDetail.MoneyAmount = String.Format("{0:0}", moneyAmount);
           subsegmentDetail.Price = Currency == "USD" ? String.Format("{0:0.00}", seatPrice) : String.Format("{0:0.00}", moneyAmount); ;
            subsegmentDetail.DisplayPrice = $"${subsegmentDetail.Price}";           

            if ((Currency == "USD" && originalPrice > seatPrice) || (Currency == "ML1" && originalPrice > moneyAmount))
            {
                subsegmentDetail.StrikeOffPrice = String.Format("{0:0.00}", originalPrice);
                subsegmentDetail.DisplayStrikeOffPrice = $"${subsegmentDetail.StrikeOffPrice}";
            }
            subsegmentDetail.Passenger = $"{count} Traveler{(count > 1 ? "s" : String.Empty)}";
            subsegmentDetail.SegmentDescription = GetSeatTypeBasedonCode(seatType, count, true);
            if (Currency == "ML1" && Convert.ToDouble(seatPrice) < milesPrice)
            {
                subsegmentDetail.StrikeOffMiles = (int)milesPrice;
                subsegmentDetail.DisplayStrikeOffMiles = UtilityHelper.FormatAwardAmountForDisplay(String.Format("{0:0}", milesPrice), false);
                milesPrice = Convert.ToDouble(seatPrice);
            }
            subsegmentDetail.MilesPrice = String.Format("{0:0}", milesPrice);
            subsegmentDetail.DisplayMilesPrice = UtilityHelper.FormatAwardAmountForDisplay(subsegmentDetail.MilesPrice, false);

            return subsegmentDetail;
        }
        private bool IsCouponApplied(MOBProductSegmentDetail segmentDetail)
        {
            return segmentDetail?.SubSegmentDetails != null ? segmentDetail.SubSegmentDetails.Any(x => x != null && !string.IsNullOrEmpty(x.OrginalPrice) && Decimal.Parse(x.OrginalPrice) > 0) : false;
        }
        private void AddPromoDetailsInSegments(MOBProdDetail prodDetail)
        {
            if (prodDetail?.Segments != null)
            {
                double promoValue;
                prodDetail?.Segments.ForEach(p =>
                {
                    p.SubSegmentDetails.ForEach(subSegment =>
                    {
                        if (!string.IsNullOrEmpty(subSegment.OrginalPrice) && !string.IsNullOrEmpty(subSegment.Price))
                        {
                            promoValue = Convert.ToDouble(subSegment.OrginalPrice) - Convert.ToDouble(subSegment.Price);
                            subSegment.Price = subSegment.OrginalPrice;
                            subSegment.DisplayPrice = Decimal.Parse(subSegment.Price).ToString("c");
                            if (promoValue > 0)
                            {
                                subSegment.PromoDetails = new MOBPromoCode
                                {
                                    PriceTypeDescription = _configuration.GetValue<string>("PromoCodeAppliedText"),
                                    PromoValue = Math.Round(promoValue, 2, MidpointRounding.AwayFromZero),
                                    FormattedPromoDisplayValue = "-" + promoValue.ToString("C2", CultureInfo.CurrentCulture)
                                };
                            }
                        }
                    });
                });
            }
        }
        private string GetSegmentInfo(FlightReservationResponse flightReservationResponse, int SegmentNumber, int LegIndex)
        {
            if (LegIndex == 0)
                return flightReservationResponse.Reservation.FlightSegments.Where(k => k.SegmentNumber == Convert.ToInt32(SegmentNumber)).Select(y => y.FlightSegment.DepartureAirport.IATACode + " - " + y.FlightSegment.ArrivalAirport.IATACode).FirstOrDefault().ToString();
            else
                return flightReservationResponse.Reservation.FlightSegments.Where(k => k.SegmentNumber == Convert.ToInt32(SegmentNumber)).Select(x => x.Legs).Select(x => x[LegIndex - 1]).Select(y => y.DepartureAirport.IATACode + " - " + y.ArrivalAirport.IATACode).FirstOrDefault().ToString();
            //return flightReservationResponse.Reservation.FlightSegments.Where(k => k.SegmentNumber == Convert.ToInt32(SegmentNumber)).Select(k => k.Legs).FirstOrDefault().Select(y => y.DepartureAirport.IATACode + " - " + y.ArrivalAirport.IATACode).FirstOrDefault().ToString();

        }
        public bool CheckSeatAssignMessage(string seatAssignMessage, bool isPost)
        {
            if (_configuration.GetValue<bool>("EnableCSL30ManageResSelectSeatMap") && isPost)
            {
                return !string.IsNullOrEmpty(seatAssignMessage) && seatAssignMessage.Equals("SEATS ASSIGNED", StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return string.IsNullOrEmpty(seatAssignMessage);
            }
        }
        private List<MOBPaxDetails> GetPaxDetails(IGrouping<string, SeatAssignment> t, FlightReservationResponse response, string flow, MOBApplication application)
        {
            List<MOBPaxDetails> paxDetails = new List<MOBPaxDetails>();
            if (response?.Reservation?.Travelers != null)
            {
                bool isExtraSeatEnabled = application != null && IsExtraSeatFeatureEnabled(application.Id, application?.Version?.Major) && flow.ToString() == FlowType.BOOKING.ToString();
                var extraSeatPassengerIndex = GetTravelerNameIndexForExtraSeat(isExtraSeatEnabled, response.Reservation.Services);

                t.ForEach(seat =>
                {
                    var traveler = response.Reservation.Travelers.Where(passenger => passenger.Person != null && passenger.Person.Key == seat.PersonIndex).FirstOrDefault();
                    if (traveler != null && (seat.SeatPrice > 0 || seat.OriginalPrice > 0)) // Added OriginalPrice check as well to handle coupon applied sceanrios where seat price can be 0 but we have original price
                    {
                        paxDetails.Add(new MOBPaxDetails
                        {
                            FullName = PaxName(traveler, isExtraSeatEnabled, extraSeatPassengerIndex),
                            Key = seat.PersonIndex,
                            Seat = seat.Seat

                        });
                    }

                });
            }
            return paxDetails;
        }

        public List<string> GetTravelerNameIndexForExtraSeat(bool isExtraSeatEnabled, Collection<Service.Presentation.CommonModel.Service> services)
        {
            var extraSeatPassengerIndex = new List<string>();
            if (isExtraSeatEnabled)
            {
                var extraSeatCodes = _configuration.GetValue<string>("EligibleSSRCodesForExtraSeat")?.Split("|");
                services?.ForEach(x =>
                {
                    if (!string.IsNullOrEmpty(x?.Code) && extraSeatCodes.Contains(x.Code) && !string.IsNullOrEmpty(x.TravelerNameIndex) && !extraSeatPassengerIndex.Contains(x.TravelerNameIndex))
                    {
                        extraSeatPassengerIndex.Add(x.TravelerNameIndex);
                    }
                });
            }

            return extraSeatPassengerIndex;
        }

        private string PaxName(United.Service.Presentation.ReservationModel.Traveler traveler, bool isExtraSeatEnabled, List<string> extraSeatPassengerIndex)
        {
            if (isExtraSeatEnabled && extraSeatPassengerIndex?.Count > 0 && !string.IsNullOrEmpty(traveler?.Person?.Key) && extraSeatPassengerIndex.Contains(traveler.Person.Key))
            {
                string travelerMiddleInitial = !string.IsNullOrEmpty(traveler.Person?.MiddleName) ? " " + traveler.Person.MiddleName.Substring(0, 1) : string.Empty;
                string travelerSuffix = !string.IsNullOrEmpty(traveler.Person?.Suffix) ? " " + traveler.Person.Suffix : string.Empty;

                return _configuration.GetValue<string>("ExtraSeatName") + " (" + GetGivenNameForExtraSeat(traveler.Person?.GivenName) + travelerMiddleInitial + " " + traveler.Person?.Surname + travelerSuffix + ")";
            }
            else
                return traveler.Person.GivenName + " " + traveler.Person.Surname;
        }

        private string GetGivenNameForExtraSeat(string givenName)
        {           
            if (!string.IsNullOrEmpty(givenName))
            {
                if (givenName.Contains(MOBExtraSeat.EXTRASEATCOUNTFORSSRREMARKS_PERSONAL_COMFORT.EXSTTWO.ToString()))
                    return givenName.Remove(0, MOBExtraSeat.EXTRASEATCOUNTFORSSRREMARKS_PERSONAL_COMFORT.EXSTTWO.ToString().Length);
                else if (givenName.Contains(MOBExtraSeat.EXTRASEATCOUNTFORSSRREMARKS_CABIN_BAGGAGE.CBBGTWO.ToString()))
                    return givenName.Remove(0, MOBExtraSeat.EXTRASEATCOUNTFORSSRREMARKS_CABIN_BAGGAGE.CBBGTWO.ToString().Length);
                else if(givenName.Contains(MOBExtraSeat.EXTRASEATCOUNTFORSSRREMARKS_PERSONAL_COMFORT.EXST.ToString()))
                    return givenName.Remove(0, MOBExtraSeat.EXTRASEATCOUNTFORSSRREMARKS_PERSONAL_COMFORT.EXST.ToString().Length);
                else if (givenName.Contains(MOBExtraSeat.EXTRASEATCOUNTFORSSRREMARKS_CABIN_BAGGAGE.CBBG.ToString()))
                    return givenName.Remove(0, MOBExtraSeat.EXTRASEATCOUNTFORSSRREMARKS_CABIN_BAGGAGE.CBBG.ToString().Length);
            }

            return givenName;
        }

        private string GetFormatedDisplayPriceForSeats(string price)
        {
            if (string.IsNullOrEmpty(price))
                return string.Empty;

            return Decimal.Parse(price).ToString("c");
        }

        private string GetCabinNameForPcuSeat(int travelerIndex, int originalSegmentIndex, TravelOptionsCollection travelOptions)
        {
            if (travelOptions == null || !travelOptions.Any())
                return string.Empty;

            var pcutravelOption = travelOptions.Where(t => t.Key == "PCU").FirstOrDefault();
            if (pcutravelOption == null)
                return string.Empty;


            var subItem = pcutravelOption.SubItems.Where(s => s != null && s.Amount > 0 && s.SegmentNumber == originalSegmentIndex.ToString() && s.TravelerRefID == (travelerIndex + 1).ToString()).FirstOrDefault();
            if (subItem == null)
                return string.Empty;

            return subItem.Description;
        }
        private string GetOriginalTotalSeatPriceForStrikeOff(List<SeatAssignment> seatAssignments, List<MOBSeat> seats, List<MOBBKTraveler> BookingTravelerInfo)
        {
            if (seatAssignments.Any(s => s.PCUSeat) || seats == null)
                return string.Empty;

            var seatsBySeatType = seats.Where(x => x.Origin == seatAssignments[0].DepartureAirportCode && x.Destination == seatAssignments[0].ArrivalAirportCode && x.FlightNumber == seatAssignments[0].FlightNumber && (GetCommonSeatCode(x.ProgramCode) == GetCommonSeatCode(seatAssignments[0].SeatPromotionCode))).ToList().
                                   Where(s => s.SeatAssignment != s.OldSeatAssignment && ((s.OldSeatPrice < s.Price && s.Currency == "USD") || (s.OldSeatMiles < s.Miles && s.Currency == "ML1")));

            var originalPrice = seatsBySeatType.Sum(s => s.Price);
            var priceAfterCompanionRules = seatsBySeatType.Sum(s => s.PriceAfterTravelerCompanionRules);
            var bookingTravelerInfoSeats = BookingTravelerInfo.Where(t => !t.Seats.IsNullOrEmpty()).SelectMany(t => t.Seats).ToCollection();
            var seatsBySeatTypeCoupon = bookingTravelerInfoSeats.Where(x => x.Origin == seatAssignments[0].DepartureAirportCode && x.Destination == seatAssignments[0].ArrivalAirportCode && x.FlightNumber == seatAssignments[0].FlightNumber && (GetCommonSeatCode(x.ProgramCode) == GetCommonSeatCode(seatAssignments[0].SeatPromotionCode))).ToList();

            if (!seatsBySeatTypeCoupon.IsNullOrEmpty() && seatsBySeatTypeCoupon.Count > 0)
            {
                foreach (var item in seatsBySeatTypeCoupon)
                {
                    if (!item.Adjustments.IsNullOrEmpty() && !item.IsCouponApplied)
                    {
                        var priceAfterCouponApplied = seatsBySeatTypeCoupon.Sum(s => s.PriceAfterCouponApplied);
                        var PriceBeforeCouponApplied = seatsBySeatTypeCoupon.Sum(s => s.PriceBeforeCouponApplied);
                        if (PriceBeforeCouponApplied > priceAfterCouponApplied)
                        {
                            return String.Format("{0:0.00}", PriceBeforeCouponApplied);
                        }
                    }
                    else
                    {
                        if (originalPrice > priceAfterCompanionRules)
                        {
                            return String.Format("{0:0.00}", originalPrice);
                        }

                    }
                }
            }
            else
            {
                if (originalPrice > priceAfterCompanionRules)
                {
                    return String.Format("{0:0.00}", originalPrice);
                }
            }
            return string.Empty;
        }
        private string GetOriginalTotalSeatMilesPriceForStrikeOff(List<SeatAssignment> seatAssignments, List<MOBSeat> seats, List<MOBBKTraveler> BookingTravelerInfo)
        {
            if (seatAssignments.Any(s => s.PCUSeat) || seats == null)
                return string.Empty;

            var seatsBySeatType = seats.Where(x => x.Origin == seatAssignments[0].DepartureAirportCode && x.Destination == seatAssignments[0].ArrivalAirportCode && x.FlightNumber == seatAssignments[0].FlightNumber && (GetCommonSeatCode(x.ProgramCode) == GetCommonSeatCode(seatAssignments[0].SeatPromotionCode))).ToList().
                                   Where(s => s.SeatAssignment != s.OldSeatAssignment && ((s.OldSeatPrice < s.Price && s.Currency == "USD") || (s.OldSeatMiles < s.Miles && s.Currency == "ML1")));

            var originalMiles = seatsBySeatType.Sum(s => s.Miles);
            var milesAfterCompanionRules = seatAssignments.Sum(s => s.MilesAmount);
            
                if (originalMiles > milesAfterCompanionRules)
                {
                    return UtilityHelper.FormatAwardAmountForDisplay(String.Format("{0:0}", originalMiles), false);
                }
            
            return string.Empty;
        }
        private bool IsPurchaseFailed(bool isPCUProduct, string originalSegmentIndex, List<string> refundedSegmentNums)
        {
            if (!isPCUProduct)
                return false;

            if (refundedSegmentNums == null || !refundedSegmentNums.Any() || string.IsNullOrEmpty(originalSegmentIndex))
                return false;

            return refundedSegmentNums.Any(s => s == originalSegmentIndex);
        }

        public async Task<bool> IsEnableU4BTravelAddONPolicy(int applicationId, string appVersion)
        {    
            if(_configuration.GetValue<bool>("EnableFeatureSettingsChanges"))
            {
                return await _featureSettings.GetFeatureSettingValue("EnableU4BTravelAddOnPolicy").ConfigureAwait(false) && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Android_EnableU4BTravelAddOnPolicy_AppVersion"), _configuration.GetValue<string>("IPhone_EnableU4BTravelAddOnPolicy_AppVersion"));
            }
            else
            {
                return _configuration.GetValue<bool>("EnableU4BTravelAddOnPolicy") && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Android_EnableU4BTravelAddOnPolicy_AppVersion"), _configuration.GetValue<string>("IPhone_EnableU4BTravelAddOnPolicy_AppVersion"));
            }
           
        }

        public async Task<TravelPolicy> GetTravelPolicy(United.CorporateDirect.Models.CustomerProfile.CorpPolicyResponse response, Session session, MOBRequest request, string corporateCompanyName, bool isCorporateNamePersonalized)
        {
            TravelPolicy corpTravelPolicy = null;
            if (response.TravelPolicies != null && response.TravelPolicies.Count > 0 && request != null && !string.IsNullOrEmpty(corporateCompanyName))
            {
                corpTravelPolicy = new TravelPolicy();
                List<CMSContentMessage> lstMessages = await GetCorporateSDLMessages(session, request);

                List<MOBMobileCMSContentMessages> travelPolicyTitle = null;
                List<MOBMobileCMSContentMessages> travelPolicyBudget = null;
                List<MOBMobileCMSContentMessages> travelPolicySeat = null;
                List<MOBMobileCMSContentMessages> travelPolicyFSRPageTitle = null;
                List<MOBMobileCMSContentMessages> travelPolicyCorporateBusinessNamePersonalizedTitle = null;

                CorporateDirect.Models.CustomerProfile.CorporateTravelPolicy travelPolicy = response?.TravelPolicies?.FirstOrDefault();

                CorporateDirect.Models.CustomerProfile.CorporateTravelCabinRestriction travelCabinRestrictions = null;
                travelCabinRestrictions = travelPolicy?.TravelCabinRestrictions?.FirstOrDefault(x => x != null && !string.IsNullOrEmpty(x.TripTypeCode) && x.TripTypeCode == "DE");
                if (travelCabinRestrictions == null)
                    travelCabinRestrictions = travelPolicy?.TravelCabinRestrictions?.FirstOrDefault();

                string cabinNameAllowedForLongTripDuration = string.Empty;
                int duration = 0;
                string cabinNameAllowed = GetCabinNameFromCorpTravelPolicy(travelCabinRestrictions);
                if (travelPolicy?.TravelCabinRestrictions != null && travelPolicy?.TravelCabinRestrictions.Count > 1) //If short trip/long trip duration is available
                {
                    CorporateDirect.Models.CustomerProfile.CorporateTravelCabinRestriction travelCabinRestrictionsForLongTripDuration = travelPolicy?.TravelCabinRestrictions?.FirstOrDefault(x => x != null && !string.IsNullOrEmpty(x.TripTypeCode) && x.TripTypeCode == "LT");
                    duration = travelCabinRestrictionsForLongTripDuration?.Duration != null ? Convert.ToInt32(travelCabinRestrictionsForLongTripDuration?.Duration) : 0;
                    if (duration > 0)
                        cabinNameAllowedForLongTripDuration = GetCabinNameFromCorpTravelPolicy(travelCabinRestrictionsForLongTripDuration);
                }

                bool isFarePlusTravelAddOnEnabled = response.TravelPolicies.FirstOrDefault().IsAirfarePlusTravelAddOn;
                bool hasDurationFromTravelPolicy = duration > 0 && !string.IsNullOrEmpty(cabinNameAllowedForLongTripDuration);
                if (lstMessages != null && lstMessages.Count > 0)
                {
                    travelPolicyTitle = GetSDLMessageFromList(lstMessages, "TravelPolicy.Title");
                    string travelBudgetKey = isFarePlusTravelAddOnEnabled ? "TravelPolicyAddOn.Budget" : "TravelPolicy.Budget";
                    string travelPolicySeatKey = hasDurationFromTravelPolicy ? "TravelPolicyDuration.Seat" : "TravelPolicy.Seat";
                    travelPolicyBudget = GetSDLMessageFromList(lstMessages, travelBudgetKey);
                    travelPolicySeat = GetSDLMessageFromList(lstMessages, travelPolicySeatKey);
                    travelPolicyFSRPageTitle = GetSDLMessageFromList(lstMessages, "TravelPolicyAlert.OutOfPolicy.PolicyLink");
                    travelPolicyCorporateBusinessNamePersonalizedTitle = GetSDLMessageFromList(lstMessages, "TravelPolicy.CorporateBusinessNamePersonalizedTitle");
                }
                
                corpTravelPolicy.TravelPolicyTitleForFSRLink = travelPolicyFSRPageTitle?.FirstOrDefault()?.ContentFull;
                corpTravelPolicy.TravelPolicyTitle = travelPolicyTitle?.FirstOrDefault()?.ContentShort;
                
                if (await IsEnableCorporateNameChange().ConfigureAwait(false))
                {
                    bool isEnableSuppressingCompanyNameForBusiness = await IsEnableSuppressingCompanyNameForBusiness(isCorporateNamePersonalized).ConfigureAwait(false);
                    corpTravelPolicy.TravelPolicyHeader = isEnableSuppressingCompanyNameForBusiness ? string.Format(travelPolicyTitle?.FirstOrDefault()?.ContentFull, corporateCompanyName) : travelPolicyCorporateBusinessNamePersonalizedTitle?.FirstOrDefault()?.ContentFull;
                }
                else
                    corpTravelPolicy.TravelPolicyHeader = string.Format(travelPolicyTitle?.FirstOrDefault()?.ContentFull, corporateCompanyName);

                corpTravelPolicy.TravelPolicyContent = new List<MOBSection>();
                if (travelPolicy?.MaximumBudget > 0 && (travelPolicy.IsAirfare || travelPolicy.IsAirfarePlusTravelAddOn))
                {
                    corpTravelPolicy.TravelPolicyContent.Add(new MOBSection
                    {
                        Text1 = travelPolicyBudget?.FirstOrDefault()?.ContentShort?.Split('|')?.FirstOrDefault(),
                        Text2 = string.Format(travelPolicyBudget?.FirstOrDefault()?.ContentFull, GetTravelPolicyBudgetAmount(travelPolicy?.MaximumBudget)),
                        Text3 = travelPolicyBudget?.FirstOrDefault()?.ContentShort?.Split('|')?.LastOrDefault()
                    });
                }

                if (!string.IsNullOrEmpty(cabinNameAllowed))
                {
                    corpTravelPolicy.TravelPolicyContent.Add(new MOBSection
                    {
                        Text1 = travelPolicySeat?.FirstOrDefault()?.ContentShort?.Split('|')?.FirstOrDefault(),
                        Text2 = hasDurationFromTravelPolicy ? string.Format(travelPolicySeat?.FirstOrDefault()?.ContentFull, duration, cabinNameAllowed, cabinNameAllowedForLongTripDuration) : string.Format(travelPolicySeat?.FirstOrDefault()?.ContentFull, cabinNameAllowed),
                        Text3 = travelPolicySeat?.FirstOrDefault()?.ContentShort?.Split('|')?.LastOrDefault()
                    });
                }
            }
            return corpTravelPolicy;
        }

        public static string GetTravelPolicyBudgetAmount(int? maximumBudget)
        {
            return string.Format("{0:n0}", maximumBudget);
        }

        public async Task<List<CMSContentMessage>> GetCorporateSDLMessages(Session session, MOBRequest request)
        {
            return await GetSDLContentByGroupName(request, session.SessionId, session.Token, _configuration.GetValue<string>("U4BCorporateContentMessageGroupName"), "U4BCorporateContentMessageCache");
        }

        public async Task<List<MOBInfoWarningMessages>> BuildTravelPolicyAlert(United.CorporateDirect.Models.CustomerProfile.CorpPolicyResponse travelPolicies, MOBRequest request, FlightReservationResponse response, Session session, bool isCorporateNamePersonalized)
        {
            List<MOBInfoWarningMessages> outOfPolicyMessage = null;
            try
            {
                if (request != null && session != null && response != null && response.Warnings != null)
                {
                    //Build Cabin descriptions allowed based on travel policy
                    CorporateDirect.Models.CustomerProfile.CorporateTravelPolicy travelPolicy = travelPolicies?.TravelPolicies?.FirstOrDefault();
                    CorporateDirect.Models.CustomerProfile.CorporateTravelCabinRestriction travelCabinRestrictions = null;
                    travelCabinRestrictions = travelPolicy?.TravelCabinRestrictions?.FirstOrDefault(x => x != null && !string.IsNullOrEmpty(x.TripTypeCode) && x.TripTypeCode == "DE");
                    if (travelCabinRestrictions == null)
                        travelCabinRestrictions = travelPolicy?.TravelCabinRestrictions?.FirstOrDefault();

                    string cabinNameAllowedForLongTripDuration = string.Empty;
                    int duration = 0;
                    string cabinNameAllowed = GetCabinNameFromCorpTravelPolicy(travelCabinRestrictions);
                    if (travelPolicy?.TravelCabinRestrictions != null && travelPolicy?.TravelCabinRestrictions.Count > 1) //If short trip/long trip duration is available
                    {
                        CorporateDirect.Models.CustomerProfile.CorporateTravelCabinRestriction travelCabinRestrictionsForLongTripDuration = travelPolicy?.TravelCabinRestrictions?.FirstOrDefault(x => x != null && !string.IsNullOrEmpty(x.TripTypeCode) && x.TripTypeCode == "LT");
                        duration = travelCabinRestrictionsForLongTripDuration?.Duration != null ? Convert.ToInt32(travelCabinRestrictionsForLongTripDuration?.Duration) : 0;
                        if (duration > 0)
                            cabinNameAllowedForLongTripDuration = GetCabinNameFromCorpTravelPolicy(travelCabinRestrictionsForLongTripDuration);
                    }

                    List<CMSContentMessage> lstMessages = await GetCorporateSDLMessages(session, request);

                    if (lstMessages != null && lstMessages.Count > 0)
                    {
                        MOBInfoWarningMessages travelPolicyWarningMessage = new MOBInfoWarningMessages();
                        List<MOBMobileCMSContentMessages> travelPolicyTitle = null;
                        List<MOBMobileCMSContentMessages> travelPolicyMessage = null;
                        List<MOBMobileCMSContentMessages> travelPolicyBudget = null;
                        List<MOBMobileCMSContentMessages> travelPolicySeat = null;
                        List<MOBMobileCMSContentMessages> travelPolicyCorporateBusinessPolicyGuidelines = null;

                        travelPolicyTitle = GetSDLMessageFromList(lstMessages, "TravelPolicyAlert.OutOfPolicy.PolicyLink");
                        travelPolicyWarningMessage.HeaderMessage = travelPolicyTitle?.FirstOrDefault()?.ContentShort;
                        travelPolicyWarningMessage.ButtonLabel = travelPolicyTitle?.FirstOrDefault()?.ContentFull;

                        travelPolicyWarningMessage.Messages = new List<string>();
                        travelPolicyMessage = GetSDLMessageFromList(lstMessages, "TravelPolicyAlert.OutOfPolicy.Message");
                        travelPolicyCorporateBusinessPolicyGuidelines = GetSDLMessageFromList(lstMessages, "TravelPolicyAlert.OutOfPolicy.CorporateBusinessPolicyGuidelines");

                        bool isFarePlusTravelAddOnEnabled = travelPolicy.IsAirfarePlusTravelAddOn;
                        bool hasDurationFromTravelPolicy = duration > 0 && !string.IsNullOrEmpty(cabinNameAllowedForLongTripDuration);

                        travelPolicyBudget = isFarePlusTravelAddOnEnabled ? GetSDLMessageFromList(lstMessages, "TravelPolicyAlertAddOn.OutOfPolicy.Budget") : GetSDLMessageFromList(lstMessages, "TravelPolicyAlert.OutOfPolicy.Budget");
                        bool hasDurationWarningMesssage = response.Warnings.Any(x => x.MinorCode == "21900") && response.Warnings.Any(x => x.MinorCode == "21800");
                        travelPolicySeat = hasDurationFromTravelPolicy && hasDurationWarningMesssage ? GetSDLMessageFromList(lstMessages, "TravelPolicyAlertDurationLT.OutOfPolicy.Seat") : hasDurationFromTravelPolicy ? GetSDLMessageFromList(lstMessages, "TravelPolicyAlertDurationST.OutOfPolicy.Seat") : GetSDLMessageFromList(lstMessages, "TravelPolicyAlert.OutOfPolicy.Seat");

                        var corporateData = response.Reservation?.Travelers?.FirstOrDefault()?.CorporateData;
                        string corporateCompanyName = corporateData?.CompanyName;

                        string headerMessage = string.Empty;
                        if (await IsEnableCorporateNameChange().ConfigureAwait(false))
                        {
                            headerMessage = await IsEnableSuppressingCompanyNameForBusiness(isCorporateNamePersonalized).ConfigureAwait(false) ? string.Format(travelPolicyMessage?.FirstOrDefault()?.ContentFull, corporateCompanyName) : travelPolicyCorporateBusinessPolicyGuidelines?.FirstOrDefault()?.ContentFull;
                        }
                        else
                            headerMessage = string.Format(travelPolicyMessage?.FirstOrDefault()?.ContentFull, corporateCompanyName);

                        string budgetMessage = string.Empty;
                        if (response.Warnings.Any(x => !string.IsNullOrEmpty(x.MinorCode) && x.MinorCode == "21700") && travelPolicy?.MaximumBudget > 0)
                        {
                            budgetMessage = string.Format(travelPolicyBudget?.FirstOrDefault()?.ContentFull, GetTravelPolicyBudgetAmount(travelPolicy?.MaximumBudget));
                        }
                        string seatMessage = string.Empty;
                        if (response.Warnings.Any(x => !string.IsNullOrEmpty(x.MinorCode) && (x.MinorCode == "21800")))
                        {
                            if (hasDurationFromTravelPolicy && hasDurationWarningMesssage)
                                seatMessage = string.Format(travelPolicySeat?.FirstOrDefault()?.ContentFull, duration, cabinNameAllowedForLongTripDuration);
                            else if (hasDurationFromTravelPolicy)
                                seatMessage = string.Format(travelPolicySeat?.FirstOrDefault()?.ContentFull, duration, cabinNameAllowed);
                            else
                                seatMessage = string.Format(travelPolicySeat?.FirstOrDefault()?.ContentFull, cabinNameAllowed);
                        }

                        string alertMessage = headerMessage + budgetMessage + seatMessage;
                        travelPolicyWarningMessage.Messages.Add(alertMessage);
                        travelPolicyWarningMessage.IconType = MOBINFOWARNINGMESSAGEICON.CAUTION.ToString();
                        travelPolicyWarningMessage.IsCollapsable = true;
                        travelPolicyWarningMessage.Order = MOBINFOWARNINGMESSAGEORDER.CORPORATETRAVELOUTOFPOLICY.ToString();


                        if (!string.IsNullOrEmpty(travelPolicyWarningMessage.HeaderMessage) && travelPolicyMessage != null && travelPolicyMessage.Count > 0)
                        {
                            outOfPolicyMessage = new List<MOBInfoWarningMessages>();
                            outOfPolicyMessage.Add(travelPolicyWarningMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return outOfPolicyMessage;
        }

        public async Task<CorporateDirect.Models.CustomerProfile.CorpPolicyResponse> GetCorporateTravelPolicyResponse(string deviceId, string mileagePlusNumber, string sessionId)
        {
            United.CorporateDirect.Models.CustomerProfile.CorpPolicyResponse _corpPolicyResponse = null;
            _corpPolicyResponse = await _sessionHelperService.GetSession<United.CorporateDirect.Models.CustomerProfile.CorpPolicyResponse>(deviceId + mileagePlusNumber, ObjectNames.CSLCorporatePolicyResponse, new List<string> { deviceId + mileagePlusNumber, ObjectNames.CSLCorporatePolicyResponse }).ConfigureAwait(false);
            return _corpPolicyResponse;
        }

        public bool HasPolicyWarningMessage(List<Services.FlightShopping.Common.ErrorInfo> response)
        {
            return response != null && response.Count > 0 && response.Any(x => !string.IsNullOrEmpty(x.MinorCode) && (x.MinorCode == "21700" || x.MinorCode == "21800" || x.MinorCode == "21900"));
        }

        public string GetCabinNameFromCorpTravelPolicy(CorporateDirect.Models.CustomerProfile.CorporateTravelCabinRestriction travelCabinRestrictions)
        {
            string cabinNameAllowed = string.Empty;
            var U4BCorporateCabinTypes = _configuration.GetValue<string>("U4BCorporateCabinTypes").Split('|');

            //if (travelPolicy.IsBasicEconomyAllowed.Value)
            //    cabinNameAllowed = cabinNameAllowed + U4BCorporateCabinTypes[0];
            if (travelCabinRestrictions.IsEconomyAllowed.Value)
                cabinNameAllowed = !string.IsNullOrEmpty(cabinNameAllowed) ? cabinNameAllowed + ", " + U4BCorporateCabinTypes[1] : cabinNameAllowed + U4BCorporateCabinTypes[1];
            if (travelCabinRestrictions.IsPremiumEconomyAllowed.Value)
                cabinNameAllowed = !string.IsNullOrEmpty(cabinNameAllowed) ? cabinNameAllowed + ", " + U4BCorporateCabinTypes[2] : cabinNameAllowed + U4BCorporateCabinTypes[2];
            if (travelCabinRestrictions.IsBusinessFirstAllowed.Value)
                cabinNameAllowed = !string.IsNullOrEmpty(cabinNameAllowed) ? cabinNameAllowed + ", " + U4BCorporateCabinTypes[3] : cabinNameAllowed + U4BCorporateCabinTypes[3];
            return cabinNameAllowed;
        }

        public async Task<TravelPolicyWarningAlert> BuildCorporateTravelPolicyWarningAlert(MOBRequest request, Session session, FlightReservationResponse flightReservationResponse, bool isCorporateNamePersonalized)
        {
            TravelPolicyWarningAlert travelPolicyWarningAlert = null;
            try
            {
                if (session.IsCorporateBooking && (await IsEnableU4BTravelAddONPolicy(request.Application.Id, request.Application.Version.Major)) && HasPolicyWarningMessage(flightReservationResponse?.Warnings))
                {
                    travelPolicyWarningAlert = new TravelPolicyWarningAlert();
                    CorporateDirect.Models.CustomerProfile.CorpPolicyResponse _corpPolicyResponse = await GetCorporateTravelPolicyResponse(request.DeviceId, session.MileagPlusNumber, session.SessionId);
                    if (_corpPolicyResponse != null && _corpPolicyResponse.TravelPolicies != null && _corpPolicyResponse.TravelPolicies.Count > 0)
                    {
                        var corporateData = flightReservationResponse?.Reservation?.Travelers?.FirstOrDefault()?.CorporateData;
                        string corporateCompanyName = corporateData != null ? corporateData.CompanyName : string.Empty;
                        travelPolicyWarningAlert.TravelPolicy = await GetTravelPolicy(_corpPolicyResponse, session, request, corporateCompanyName, isCorporateNamePersonalized);
                        travelPolicyWarningAlert.InfoWarningMessages = new List<MOBInfoWarningMessages>();
                        travelPolicyWarningAlert.InfoWarningMessages = await BuildTravelPolicyAlert(_corpPolicyResponse, request, flightReservationResponse, session, isCorporateNamePersonalized);
                    }
                }
            }
            catch(Exception ex)
            {
            }
            return travelPolicyWarningAlert;
        }

        public async Task<bool> IsEnableCCEFeedBackCallForEplusTile()
        {
            return await _featureSettings.GetFeatureSettingValue("IsEnableCCEFeedBackCallForEplusTileMR").ConfigureAwait(false);

        }
        public MOBMobileCMSContentMessages AddMilesTnCForSeatChange() {
            MOBMobileCMSContentMessages tNC = new MOBMobileCMSContentMessages();
            List<MOBTypeOption> tAndCList = new List<MOBTypeOption>();
            if (_configuration.GetValue<string>("MileagePlusSeatTnC") != null)
            {
                string mpTermsAndConditionsList = _configuration.GetValue<string>("MileagePlusSeatTnC");
                foreach (string eachItem in mpTermsAndConditionsList.Split('~'))
                {
                    tAndCList.Add(new MOBTypeOption(eachItem.Split('|')[0].ToString(), eachItem.Split('|')[1].ToString()));
                }
                tNC.ContentFull = HttpUtility.HtmlDecode("<ul><li>" + string.Join("</li><li>", tAndCList.Select(x => x.Value)) + "</li></ul>");
                tNC.HeadLine = "MileagePlus®";
                tNC.Title = "Terms and conditions";
                tNC.ContentShort = HttpUtility.HtmlDecode(_configuration.GetValue<string>("PaymentTnCMessage").ToString());
            }
            return tNC;

        }
        public MOBSection AddTextForInsufficientMiles()
        {
            MOBSection section = new MOBSection();
            List<string> textList = new List<string>();
            if (_configuration.GetValue<string>("NotEnoughMilesAdvisory") != null)
            {
                string notEnoughMilesWarning = _configuration.GetValue<string>("NotEnoughMilesAdvisory");
                foreach (string eachItem in notEnoughMilesWarning.Split("||"))
                {
                    textList.Add((eachItem.Split('|')[1]));
                }
                section.IsDefaultOpen = true;
                section.Text1 = textList[0];
                section.Text2 = textList[1];
                section.MessageType = textList[2];
            }
            return section;

        }

        public bool IsEnableMoneyPlusMilesFeature(int applicationId = 0, string appVersion = "", List<MOBItem> catalogItems = null)
        {
            return (_configuration.GetValue<bool>("EnableFSRMoneyPlusMilesFeature") && (catalogItems != null && catalogItems.Count > 0 &&
            catalogItems.FirstOrDefault(a => a.Id == ((int)IOSCatalogEnum.EnableMoneyMilesBooking).ToString() || a.Id == ((int)AndroidCatalogEnum.EnableMoneyMilesBooking).ToString())?.CurrentValue == "1")
                );
        }


        public bool IsIncludeMoneyMilesInRTI(List<FormofPaymentOption> eligibleFormsOfPayment)
        {
            return (eligibleFormsOfPayment != null && eligibleFormsOfPayment.Exists(x => x.Category == "MILES"));
        }
        #region Booking Advance Search Offer Code changes

        public bool IsFOPRequired(MOBShoppingCart shoppingCart, MOBSHOPReservation reservation)
        {
            if (shoppingCart.Offers != null && !string.IsNullOrEmpty(shoppingCart.Offers.OfferCode))
            {
                var grandTotalPrice = reservation?.Prices?.FirstOrDefault(p => !string.IsNullOrEmpty(p.DisplayType)
                                                                           && p.DisplayType.ToUpper().Equals("GRAND TOTAL"));
                if (grandTotalPrice != null && grandTotalPrice.Value == 0)
                {
                    return shoppingCart.Products.Any(prod => !string.IsNullOrEmpty(prod.ProdTotalPrice) && Convert.ToDecimal(prod.ProdTotalPrice) > 0);//return true only when none of the product has price(Ex:grandtotal can be zero when entire amount is covered with TravelCredit but we cannot assume no fop is required)
                }
            }
            return true;
        }
        public bool IsEnableAgreeandPurchaseButton(MOBShoppingCart shoppingCart, MOBSHOPReservation reservation)
        {
            if (shoppingCart.Offers != null
                && !string.IsNullOrEmpty(shoppingCart.Offers.OfferCode)
                && shoppingCart.FormofPaymentDetails?.IsFOPRequired == false
                && shoppingCart.FormofPaymentDetails?.BillingAddress != null)
            {
                return true;
            }
            return false;
        }
        public async Task<string> AssignMaskedPaymentMethod(MOBShoppingCart shoppingCart, MOBApplication application)
        {
            if (shoppingCart.Offers != null
                && !string.IsNullOrEmpty(shoppingCart.Offers.OfferCode)
                && shoppingCart.FormofPaymentDetails?.IsFOPRequired == false
                && shoppingCart.FormofPaymentDetails?.BillingAddress != null)
            {
                if (await IsEnableAdvanceSearchOfferCodeFastFollower(application.Id, application.Version.Major).ConfigureAwait(false))
                {
                    return _configuration.GetValue<string>("ZeroDollarPaymentmethodText");
                }
                else
                {
                    return $"****{shoppingCart.Offers.OfferCode.Substring(shoppingCart.Offers.OfferCode.Length - 4)}";
                }
            }
            return string.Empty;
        }
        public async Task<bool> IsEnableAdvanceSearchOfferCodeFastFollower(int applicationId, string appVersion)
        {
            return (await _featureSettings.GetFeatureSettingValue("EnableOfferCodeFastFollowerChanges").ConfigureAwait(false)
                && GeneralHelper.IsApplicationVersionGreaterorEqual(applicationId, appVersion, _configuration.GetValue<string>("Andriod_EnableAdvanceSearchOfferCodefastfollower_AppVersion"), _configuration.GetValue<string>("Iphone_EnableAdvanceSearchOfferCodefastfollower_AppVersion")));
        }
        #endregion
      
        public bool UpdateChaseCreditStatement(Reservation reservation)
        {
            try
            {
                if (reservation.ShopReservationInfo2 != null && reservation.ShopReservationInfo2.ChaseCreditStatement != null)
                {
                    var objPrice = reservation.Prices.FirstOrDefault(p => p.PriceType.ToUpper().Equals("GRAND TOTAL"));
                    if (objPrice != null)
                    {
                        decimal price = Convert.ToDecimal(objPrice.Value);
                        if (!_configuration.GetValue<bool>("TurnOffChaseBugMOBILE-11134"))
                        {
                            reservation.ShopReservationInfo2.ChaseCreditStatement.finalAfterStatementDisplayPrice = GetPriceAfterChaseCredit(price, reservation.ShopReservationInfo2.ChaseCreditStatement.statementCreditDisplayPrice);
                        }
                        else
                        {
                            reservation.ShopReservationInfo2.ChaseCreditStatement.finalAfterStatementDisplayPrice = GetPriceAfterChaseCredit(price);
                        }

                        reservation.ShopReservationInfo2.ChaseCreditStatement.initialDisplayPrice = price.ToString("C2", new CultureInfo("en-US"));
                        FormatChaseCreditStatemnet(reservation.ShopReservationInfo2.ChaseCreditStatement);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.ILoggerError("UpdateChaseCreditStatement Error {@Exception}", JsonConvert.SerializeObject(ex));
            }
            return true;

        }

        public bool EnableChaseOfferRTI(int appID, string appVersion)
        {
            return _configuration.GetValue<bool>("EnableChaseOfferRTI") && (!_configuration.GetValue<bool>("EnableChaseOfferRTIVersionCheck") ||
                (_configuration.GetValue<bool>("EnableChaseOfferRTIVersionCheck") && GeneralHelper.IsApplicationVersionGreaterorEqual(appID, appVersion, _configuration.GetValue<string>("AndroidEnableChaseOfferRTIVersion"), _configuration.GetValue<string>("iPhoneEnableChaseOfferRTIVersion"))));
        }

        private void FormatChaseCreditStatemnet(MOBCCAdStatement chaseCreditStatement)
        {
            if (_configuration.GetValue<bool>("UpdateChaseColor16788"))
            {
                chaseCreditStatement.styledInitialDisplayPrice = string.IsNullOrWhiteSpace(chaseCreditStatement.initialDisplayPrice) ? "" : HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextBeginning")) + chaseCreditStatement.initialDisplayPrice + HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextEnding"));
                chaseCreditStatement.styledInitialDisplayText = HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextBeginning")) + _configuration.GetValue<string>("InitialDisplayText") + HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextEnding"));
                chaseCreditStatement.styledStatementCreditDisplayPrice = string.IsNullOrWhiteSpace(chaseCreditStatement.statementCreditDisplayPrice) ? "" : HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextBeginningWithColor")) + HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextStrongBeginning")) + GetPriceAfterChaseCredit(0, chaseCreditStatement.statementCreditDisplayPrice) + HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextStrongEnding")) + HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextEnding"));
                chaseCreditStatement.styledStatementCreditDisplayText = HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextBeginningWithColor")) + HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextStrongBeginning")) + _configuration.GetValue<string>("StatementCreditDisplayText") + HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextStrongEnding")) + HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextEnding"));
                chaseCreditStatement.styledFinalAfterStatementDisplayPrice = string.IsNullOrWhiteSpace(chaseCreditStatement.finalAfterStatementDisplayPrice) ? "" : HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextBeginning")) + chaseCreditStatement.finalAfterStatementDisplayPrice + HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextEnding"));
                chaseCreditStatement.styledFinalAfterStatementDisplayText = HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextBeginning")) + _configuration.GetValue<string>("FinalAfterStatementDisplayText") + HttpUtility.HtmlDecode(_configuration.GetValue<string>("StyledTextEnding"));
            }
        }

        private string GetPriceAfterChaseCredit(decimal price)
        {
            int creditAmt = _configuration.GetValue<int>("ChaseStatementCredit");

            CultureInfo culture = new System.Globalization.CultureInfo("en-US");
            culture.NumberFormat.CurrencyNegativePattern = 1;
            return String.Format(culture, "{0:C}", price - creditAmt);

        }

        private string GetPriceAfterChaseCredit(decimal price, string chaseCrediAmount)
        {
            int creditAmt = 0;

            if (_configuration.GetValue<string>("IsEnableTryWithRegEX").ToUpper() == "TRUE")
            {
                int.TryParse(Regex.Match(chaseCrediAmount, @"\d+").Value, out creditAmt);
            }
            else
            {
                int.TryParse(chaseCrediAmount, System.Globalization.NumberStyles.AllowCurrencySymbol | System.Globalization.NumberStyles.AllowDecimalPoint, null, out creditAmt);

            }

            CultureInfo culture = new System.Globalization.CultureInfo("en-US");
            culture.NumberFormat.CurrencyNegativePattern = 1;
            string diff = String.Format(culture, "{0:C}", price - creditAmt);
            return diff;
            // return String.Format(culture, "{0:C}", price - creditAmt);

        }

        private bool isSFCProductRegistered(FlightReservationResponse flightReservationResponse, bool isPost)
        {
            United.Service.Presentation.InteractionModel.ShoppingCart flightReservationResponseShoppingCart = new United.Service.Presentation.InteractionModel.ShoppingCart();
            flightReservationResponseShoppingCart = isPost ? flightReservationResponse.CheckoutResponse.ShoppingCart : flightReservationResponse.ShoppingCart;
            if (flightReservationResponseShoppingCart != null
                && flightReservationResponseShoppingCart.Items.Any())
            {
                return flightReservationResponseShoppingCart.Items.Any(item => item.Product?.FirstOrDefault()?.Code == "SFC");
            }
            return false;
        }

        public async Task<MOBTPIInfo> GetTPIInfoFromContentV2(United.Service.Presentation.ProductModel.Presentation presentation, MOBTPIInfo tripInsuranceInfo, string sessionId, bool isShoppingCall, bool isBookingPath = false)
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
            string legalInfoText = string.Empty;
            string bookingTncContentMsg = string.Empty;
            string bookingTncLinkMsg = string.Empty;
            string bookingLegalInfoContentMsg = string.Empty;
            string mobTgiLimitationMessage = string.Empty;
            string mobTgiReadMessage = string.Empty;
            string mobTgiAndMessage = string.Empty;
            // Covid-19 Emergency WHO TPI content
            string mobTIMBemergencyMessage = string.Empty;
            string mobTIMBemergencyMessageUrltext = string.Empty;
            string mobTIMBemergencyMessagelinkUrl = string.Empty;

            foreach (var content in presentation.Contents)
            {
                switch (content.Header.ToUpper().Trim())
                {
                    case "PREBOOKINGMOBOFFERTITLEMESSAGE":
                        tripInsuranceInfo.QuoteTitle = content.Body.Trim();
                        tripInsuranceInfo.TileQuoteTitle = content.Body.Trim();
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
                    // used in payment confirmation page 
                    case "MOBTICONFIRMATIONBODY1MESSAGE":
                        confirmationResponseDetailMessage1 = content.Body.Trim();
                        break;
                    case "MOBTICONFIRMATIONBODY2MESSAGE":
                        confirmationResponseDetailMessage2 = content.Body.Trim();
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
                    case "MOBTGILIMITATIONMESSAGE":
                        mobTgiLimitationMessage = content.Body.Trim();
                        break;
                    case "MOBTGIREADMESSAGE":
                        mobTgiReadMessage = content.Body.Trim();
                        break;
                    case "MOBTGIANDMESSAGE":
                        mobTgiAndMessage = content.Body.Trim();
                        break;
                    case "MOBTIMBEMERGENCYMESSAGETEXT":
                        mobTIMBemergencyMessage = content != null ? content.Body.Trim() : string.Empty;
                        break;
                    case "MOBTIMBEMERGENCYMESSAGELINKTEXT":
                        mobTIMBemergencyMessageUrltext = content != null ? content.Body.Trim() : string.Empty;
                        break;
                    case "MOBTIMBEMERGENCYMESSAGELINKURL":
                        mobTIMBemergencyMessagelinkUrl = content != null ? content.Body.Trim() : string.Empty;
                        break;
                    default:
                        break;
                }
            }

            // Get the html content
            Regex remScript = new Regex(@"<script[^>]*>[\s\S]*?</script>");
            string output = remScript.Replace(presentation.HTML, "");
            tripInsuranceInfo.HtmlContentV2 = output;

            //Covid-19 Emergency WHO TPI content
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
            }

            string specialCharacter = _configuration.GetValue<string>("TPIinfo-SpecialCharacter") ?? "";
            if (isShoppingCall && isBookingPath && !string.IsNullOrEmpty(tncProductPageText1) && !string.IsNullOrEmpty(tripInsuranceInfo.HtmlContentV2)
                        && !string.IsNullOrEmpty(bookingTncLinkMsg) && !string.IsNullOrEmpty(bookingTncContentMsg) && !string.IsNullOrEmpty(bookingLegalInfoContentMsg)
                        && !string.IsNullOrEmpty(tncPaymentLinkMessage) && !string.IsNullOrEmpty(tncPaymentText2) && !string.IsNullOrEmpty(tncPaymentText3)
                        && !string.IsNullOrEmpty(tncPaymentText1)
                        && !string.IsNullOrEmpty(confirmationResponseDetailMessage1)
                        && !string.IsNullOrEmpty(confirmationResponseDetailMessage2))
            {
                tripInsuranceInfo.TNC = (mobTgiLimitationMessage.IsNullOrEmpty() && mobTgiReadMessage.IsNullOrEmpty()) ? bookingLegalInfoContentMsg + " <a href =\"" + tncPaymentLinkMessage + "\" target=\"_blank\">" + tncPaymentText2 + "</a> " + tncPaymentText3
                                        : bookingLegalInfoContentMsg + " " + tncPaymentText3 + " <a href =\"" + tncPaymentLinkMessage + "\" target=\"_blank\">" + tncPaymentText2 + "</a> " + mobTgiAndMessage + " <a href =\"" + bookingTncLinkMsg + "\" target=\"_blank\">" + bookingTncContentMsg + "</a>";

                tripInsuranceInfo.PageTitle = _configuration.GetValue<string>("TPIinfo-PageTitle") ?? "";
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

                bookingPathReservation.TripInsuranceFile.TripInsuranceBookingInfo.TncSecondaryFOPPage = (mobTgiLimitationMessage.IsNullOrEmpty() && mobTgiReadMessage.IsNullOrEmpty()) ? tncPaymentText1 + " <a href =\"" + tncPaymentLinkMessage + "\" target=\"_blank\">" + tncPaymentText2 + "</a> " + tncPaymentText3
                       : tncPaymentText1 + " " + tncPaymentText3 + " <a href =\"" + tncPaymentLinkMessage + "\" target=\"_blank\">" + tncPaymentText2 + "</a> " + mobTgiAndMessage + " <a href =\"" + tncProductPageLinkMessage + "\" target=\"_blank\">" + tncProductPageText2 + "</a> ";
                bookingPathReservation.TripInsuranceFile.TripInsuranceBookingInfo.ConfirmationMsg = @confirmationResponseDetailMessage1.Replace("(R)", specialCharacter) + "\n\n" + confirmationResponseDetailMessage2;
                bookingPathReservation.TripInsuranceFile.TripInsuranceBookingInfo.HtmlContentV2 = tripInsuranceInfo.HtmlContentV2;
                await _sessionHelperService.SaveSession<Reservation>(bookingPathReservation, _headers.ContextValues.SessionId, new List<string> { _headers.ContextValues.SessionId, bookingPathReservation.ObjectName }, bookingPathReservation.ObjectName).ConfigureAwait(false);
            }
            else
            {
                tripInsuranceInfo = null;
            }
            return tripInsuranceInfo;
        }
        public async Task< bool> IsCheckInFlow(string flow)
        {
            return !string.IsNullOrEmpty(flow) && flow == FlowType.CHECKIN.ToString() && await _featureSettings.GetFeatureSettingValue("EnableCheckInCloudMigrationMSC");
        }

        public async Task<bool> IsEnableSuppressingCompanyNameForBusiness(bool isCorporateNamePersonalized)
        {
            return await _featureSettings.GetFeatureSettingValue("EnableSuppressingCompanyNameForBusiness").ConfigureAwait(false) && isCorporateNamePersonalized;
        }

        public async Task<bool> IsEnableCorporateNameChange()
        {
            return await _featureSettings.GetFeatureSettingValue("IsEnableCorporateNameChange").ConfigureAwait(false);
        }
        public async Task<bool> IsEnableETCCreditsInBookingFeature(List<MOBItem> catalogItems = null)
        {
            return await _featureSettings.GetFeatureSettingValue("EnableFSRETCTravelCreditsFeature").ConfigureAwait(false) && (catalogItems != null && catalogItems.Count > 0 &&
                catalogItems.FirstOrDefault(a => a.Id == ((int)IOSCatalogEnum.EnableETCCreditsInBooking).ToString() || a.Id == ((int)AndroidCatalogEnum.EnableETCCreditsInBooking).ToString())?.CurrentValue == "1");
        }
        public bool IsEnableETCCreditsInBookingFeature(int applicationId = 0, string appVersion = "", List<MOBItem> catalogItems = null)
        {
            return _configuration.GetValue<bool>("EnableFSRETCCreditsFeature") && (catalogItems != null && catalogItems.Count > 0 &&
                catalogItems.FirstOrDefault(a => a.Id == ((int)IOSCatalogEnum.EnableETCCreditsInBooking).ToString() || a.Id == ((int)AndroidCatalogEnum.EnableETCCreditsInBooking).ToString())?.CurrentValue == "1");
        }

        public async Task SetETCTraveCreditsReviewAlertMessage(List<MOBMobileCMSContentMessages> alertMessages)
        {
            if (await _featureSettings.GetFeatureSettingValue("EnableFSRETCTravelCreditsFeature"))
            {
                if (!alertMessages.Any(c => c.LocationCode == _configuration.GetValue<string>("RTI.TravelCredits.ReviewETCAlertMsgs.AboutTravelCredits.LocationCode")))
                {
                    alertMessages.Add(
                        new MOBMobileCMSContentMessages
                        {
                            ContentFull = _configuration.GetValue<string>("RTI.TravelCredits.ReviewETCAlertMsgs.AboutTravelCredits.ContentFull"),
                            ContentShort = _configuration.GetValue<string>("RTI.TravelCredits.ReviewETCAlertMsgs.AboutTravelCredits.ContentShort"),
                            HeadLine = _configuration.GetValue<string>("RTI.TravelCredits.ReviewETCAlertMsgs.AboutTravelCredits.HeadLine"),
                            LocationCode = _configuration.GetValue<string>("RTI.TravelCredits.ReviewETCAlertMsgs.AboutTravelCredits.LocationCode"),
                            Title = _configuration.GetValue<string>("RTI.TravelCredits.ReviewETCAlertMsgs.AboutTravelCredits.Title"),
                            ContentKey = ""
                        });
                }
                if (!alertMessages.Any(c => c.LocationCode == _configuration.GetValue<string>("RTI.TravelCredits.ReviewETCAlertMsgs.TravelCreditsMsg.LocationCode")))
                {
                    alertMessages.Add(
                      new MOBMobileCMSContentMessages
                      {
                          ContentFull = _configuration.GetValue<string>("RTI.TravelCredits.ReviewETCAlertMsgs.TravelCreditsMsg.ContentFull"),
                          ContentShort = _configuration.GetValue<string>("RTI.TravelCredits.ReviewETCAlertMsgs.TravelCreditsMsg.ContentShort"),
                          HeadLine = _configuration.GetValue<string>("RTI.TravelCredits.ReviewETCAlertMsgs.TravelCreditsMsg.HeadLine"),
                          LocationCode = _configuration.GetValue<string>("RTI.TravelCredits.ReviewETCAlertMsgs.TravelCreditsMsg.LocationCode"),
                          Title = _configuration.GetValue<string>("ETCLearnmoreTooltRTI.TravelCredits.ReviewETCAlertMsgs.TravelCreditsMsg.Title"),
                          ContentKey = ""
                      });
                }
            }
        }

        public void ApplyETCCreditsOnRTIAction(MOBShoppingCart shoppingCart, DisplayCart displayCart)
        {
            if (displayCart?.SpecialPricingInfo != null && displayCart.SpecialPricingInfo.TravelCreditDetails?.TravelCertificates?.Count() > 0 && shoppingCart?.FormofPaymentDetails != null)
            {

                foreach (var displayCartAppliedTravelCredits in displayCart.SpecialPricingInfo.TravelCreditDetails?.TravelCertificates)
                {
                    foreach (var scTravelCredits in shoppingCart?.FormofPaymentDetails?.TravelCreditDetails?.TravelCredits)
                    {
                        if (displayCartAppliedTravelCredits.CertPin.Equals(scTravelCredits.PinCode))
                            scTravelCredits.IsApplied = true;
                    }
                }
            }
        }
    }
}
