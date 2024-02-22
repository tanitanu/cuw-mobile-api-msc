using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using United.CorporateDirect.Models.CustomerProfile;
using United.Definition;
using United.Definition.Shopping;
using United.Mobile.Model.MSC;
using United.Persist.Definition.SeatChange;
using United.Persist.Definition.Shopping;
using United.Service.Presentation.CustomerResponseModel;
using United.Service.Presentation.SegmentModel;
using United.Services.FlightShopping.Common.DisplayCart;
using United.Services.FlightShopping.Common.FlightReservation;
using United.Mobile.Model.Common;
using United.Definition.Shopping.TripInsurance;
using TravelCreditsResponse = United.Services.FlightShopping.Common.TravelCreditsResponse;

namespace United.Common.Helper
{
    public interface IShoppingCartUtility
    {
        Task<MOBShoppingCart> ReservationToShoppingCart_DataMigration(MOBSHOPReservation reservation, MOBShoppingCart shoppingCart, MOBRequest request, bool isCheckout = false, Session session = null);
        Task<List<CMSContentMessage>> GetSDLContentByGroupName(MOBRequest request, string guid, string token, string groupName, string docNameConfigEntry, bool useCache = false);
        bool IsEnableBundleLiveUpdateChanges(int applicationId, string appVersion, bool isDisplayCart);
        void BuildOmniCart(MOBShoppingCart shoppingCart, MOBSHOPReservation reservation, MOBApplication application);
        string GetBookingPaymentTargetForRegisterFop(FlightReservationResponse flightReservationResponse);
        Task<MOBShoppingCart> BuildShoppingCart(MOBRequest request, FlightReservationResponse flightReservationResponse, string flow, string cartId, string sessionId, Session session);
        Task<FlightReservationResponse> BuildShoppingCartDisplayCart(TravelCreditsResponse travelCreditsResponse, Session session);
        Task<List<MOBMobileCMSContentMessages>> GetProductBasedTermAndConditions(United.Service.Presentation.ProductResponseModel.ProductOffer productVendorOffer, FlightReservationResponse flightReservationResponse, bool isPost, string sessionId, string flow, bool isGetCartInfo = false);
        string GetPaymentTargetForRegisterFop(TravelOptionsCollection travelOptions, bool isCompleteFarelockPurchase = false);
        bool IsEnableOmniCartMVP2Changes(int applicationId, string appVersion, bool isDisplayCart);
        bool IncludeMoneyPlusMiles(int appId, string appVersion);
        string GetCostBreakdownFareHeader(string travelType, MOBShoppingCart shoppingCart);        
        MOBSection GetAdditionalMileDetail(MOBSHOPReservation reservation);
        List<MOBSection> GetFOPDetails(MOBSHOPReservation reservation, MOBApplication application);
        void AssignIsOtherFOPRequired(MOBFormofPaymentDetails formofPaymentDetails, List<MOBSHOPPrice> prices);
        Dictionary<string, string> GetAdditionalHeadersForMosaic(string flow);
        List<MOBTypeOption> GetPBContentList(string configValue);
        bool IsBundleProductSelected(United.Services.FlightShopping.Common.FlightReservation.FlightReservationResponse flightReservationResponse, bool isPost);
        Task<List<MOBItem>> GetCaptions(string key);
        string ExcludeCountryCodeFrmPhoneNumber(string phonenumber, string countrycode);
        Task<List<United.Mobile.Model.Common.CacheCountry>> LoadCountries();
        Task<string> GetCountryCode(string countryaccesscode);
        string GetPaymentTargetForRegisterFop(FlightReservationResponse flightReservationResponse, string flow, bool isCompleteFarelockPurchase = false);
        List<string> GetProductCodes(FlightReservationResponse flightReservationResponse, string flow, bool isPost, bool isGetCartInfo);
        string BuildSegmentInfo(string productCode, Collection<ReservationFlightSegment> flightSegments, IGrouping<string, SubItem> x, string flow, bool isBundleProduct = false, bool isMRBundle = false);
        Task<List<MOBProdDetail>> ConfirmationPageProductInfo(FlightReservationResponse flightReservationResponse, bool isPost, bool isError, string flow, MOBApplication application, SeatChangeState state = null, bool isRefundSuccess = false, bool isPartialSuccess = false, List<SCProductContext> unSuccessfulSegmentDetails = null, List<string> refundedSegmentNums = null, bool isGetCartInformation = false, string sessionId = "",bool isMiles=false);
        MOBProdDetail BuildProdDetailsForSeats(FlightReservationResponse flightReservationResponse, SeatChangeState state, bool isPost, string flow, bool isMiles = false);
        Task<MOBProdDetail> BuildProdDetailsForSeats(FlightReservationResponse flightReservationResponse, Persist.Definition.SeatChange.SeatChangeState state, string flow, MOBApplication application);
        bool ShouldIgnoreAmount(SubItem subItem);
        bool CheckSeatAssignMessage(string seatAssignMessage, bool isPost);
        bool CheckSeatAssignMessage(FlightReservationResponse flightReservationResponse, string flow);
        bool ShouldIgnoreAmount(SubItem subItem, FlightReservationResponse flightReservationResponse, string flow);
        bool IsRefundSuccess(Collection<ShoppingCartItemResponse> items, out List<string> refundedSegments, string flow);
        Task<bool> IsEnableU4BTravelAddONPolicy(int applicationId, string appVersion);
        Task<TravelPolicy> GetTravelPolicy(CorpPolicyResponse response, Session session, MOBRequest request, string corporateCompanyName, bool isCorporateNamePersonalized);
        Task<List<CMSContentMessage>> GetCorporateSDLMessages(Session session, MOBRequest request);
        Task<List<MOBInfoWarningMessages>> BuildTravelPolicyAlert(CorpPolicyResponse travelPolicies, MOBRequest request, FlightReservationResponse response, Session session, bool isCorporateNamePersonalized);
        Task<CorpPolicyResponse> GetCorporateTravelPolicyResponse(string deviceId, string mileagePlusNumber, string sessionId);
        bool HasPolicyWarningMessage(List<Services.FlightShopping.Common.ErrorInfo> response);
        string GetCabinNameFromCorpTravelPolicy(CorporateTravelCabinRestriction travelCabinRestrictions);
        Task<Mobile.Model.MSC.Corporate.TravelPolicyWarningAlert> BuildCorporateTravelPolicyWarningAlert(MOBRequest request, Session session, FlightReservationResponse flightReservationResponse, bool isCorporateNamePersonalized);
        MOBMobileCMSContentMessages AddMilesTnCForSeatChange();
        MOBSection AddTextForInsufficientMiles();
        Task<bool> IsEnableCCEFeedBackCallForEplusTile();
        bool IsExtraSeatExcluded(Session session, List<MOBSHOPTrip> trips, List<MOBDisplayTravelType> displayTravelTypes, MOBShoppingCart response = null, bool? isUnfinishedBooking = false);
        bool IsEnableMoneyPlusMilesFeature(int applicationId = 0, string appVersion = "", List<MOBItem> catalogItems = null);
        List<string> GetTravelerNameIndexForExtraSeat(bool isExtraSeatEnabled, Collection<Service.Presentation.CommonModel.Service> services);
        Task<bool> IsExtraSeatCheckEnableForUpliftMR();
        string GetGrandTotalPrice(MOBSHOPReservation reservation);

        bool IsIncludeMoneyMilesInRTI(List<FormofPaymentOption> eligibleFormsOfPayment);


        bool IsFOPRequired(MOBShoppingCart shoppingCart, MOBSHOPReservation reservation);
        bool IsEnableAgreeandPurchaseButton(MOBShoppingCart shoppingCart, MOBSHOPReservation reservation);
        Task<string> AssignMaskedPaymentMethod(MOBShoppingCart shoppingCart, MOBApplication application);
        bool IsUpliftEligbleOffer(MOBOffer offer);      
        bool UpdateChaseCreditStatement(United.Persist.Definition.Shopping.Reservation reservation);
        Task<List<MOBMobileCMSContentMessages>> GetBagsTermsAndConditions(string sessionId, bool isMFOPSelected);

        bool EnableChaseOfferRTI(int appID, string appVersion);

        Task<MOBTPIInfo> GetTPIInfoFromContentV2(United.Service.Presentation.ProductModel.Presentation presentation, MOBTPIInfo tripInsuranceInfo, string sessionId, bool isShoppingCall, bool isBookingPath = false);
        Task<bool> IsCheckInFlow(string flow);
        Task<bool> IsEnableSuppressingCompanyNameForBusiness(bool isCorporateNamePersonalized);
        Task<bool> IsEnableCorporateNameChange();
        Task<bool> IsEnableETCCreditsInBookingFeature(List<MOBItem> catalogItems = null);
        bool IsEnableETCCreditsInBookingFeature(int applicationId = 0, string appVersion = "", List<MOBItem> catalogItems = null);
        void ApplyETCCreditsOnRTIAction(MOBShoppingCart shoppingCart, DisplayCart displayCart);

    }
}
