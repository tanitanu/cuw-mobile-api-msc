using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using United.Definition.FormofPayment;
using United.Definition.Shopping;
using United.Definition.Uplift;
using United.Mobile.Model.Common;
using United.Mobile.Model.MSC;
using United.Mobile.Model.MSC.Corporate;
using United.Mobile.Model.MSC.Shopping;
using United.Service.Presentation.CommonEnumModel;

namespace United.Definition
{
    [Serializable()]
    public class MOBShoppingCart
    {
        public MOBShoppingCart()
        { }

        [System.Text.Json.Serialization.JsonIgnore()]
        public string ObjectName { get; set; } = "United.Definition.MOBShoppingCart";

        private string cartId = string.Empty;
        private string flow = string.Empty;
        private List<MOBProdDetail> products;
        private string totalPrice = string.Empty;
        private string displayTotalPrice = string.Empty;
        private string displaySubTotalPrice = string.Empty;
        private string displayTaxesAndFees = string.Empty;
        private string totalMiles;
        private MOBFormofPaymentDetails formofPaymentDetails;
        private List<MOBCPTraveler> scTravelers;
        private string pointofSale = string.Empty;
        private string currencyCode = string.Empty;
        private List<MOBSection> alertMessages;
        private List<MOBMobileCMSContentMessages> termsAndConditions;
        private string paymentTarget;
        private string couponOfferDescription;
        private string couponCode;
        private bool isCouponApplied;
        private bool disableCouponEditOption;
        private MOBCPTraveler travelers;
        private List<MOBSHOPTrip> trips;
        private List<MOBSHOPPrice> prices;
        private List<List<MOBSHOPTax>> taxes;
        private List<MOBItem> captions;
        private bool isCouponEligibleProduct;
        private List<MOBItem> elfLimitations;
        private string flightShareMessage = string.Empty;
        private string displayTotalMiles;
        private string partialCouponEligibleMessage;
        private string partialCouponEligibleUrl;
        private List<MOBSection> paymentAlerts;
        private string bundleCartId;
        private List<MOBFOPCertificateTraveler> certificateTravelers;
        private bool isMultipleTravelerEtcFeatureClientToggleEnabled;
        private List<MOBFOPCertificate> profileTravelerCertificates;
        private MOBULTripInfo tripInfoForUplift;
        private List<MOBMobileCMSContentMessages> confirmationPageAlertMessages;
        private MOBPromoCodeDetails promoCodeDetails;
        private MOBSHOPInflightContactlessPaymentEligibility inFlightContactlessPaymentEligibility;
        private int cslWorkFlowType;
        private string corporateDisclaimerText;
        private List<MOBSection> displayMessage;
        private List<MOBUpgradeOption> upgradeCabinProducts;
        private string totalPoints = string.Empty;
        private string displayTotalPoints = string.Empty;
        private MOBSHOPTripShare tripShare;
        private string requestObjectJSON;
        private MOBRequestObjectName requestObjectName;

        public MOBRequestObjectName RequestObjectName
        {
            get { return requestObjectName; }
            set { requestObjectName = value; }
        }

        public string RequestObjectJSON
        {
            get { return requestObjectJSON; }
            set { requestObjectJSON = value; }
        }


        public List<MOBUpgradeOption> UpgradeCabinProducts
        { get { return upgradeCabinProducts; } set { upgradeCabinProducts = value; } }

        public int CslWorkFlowType
        {
            get { return cslWorkFlowType; }
            set { cslWorkFlowType = value; }
        }

        public MOBSHOPInflightContactlessPaymentEligibility InFlightContactlessPaymentEligibility
        {
            get { return inFlightContactlessPaymentEligibility; }
            set { inFlightContactlessPaymentEligibility = value; }
        }

        public MOBPromoCodeDetails PromoCodeDetails
        {
            get { return promoCodeDetails; }
            set { promoCodeDetails = value; }
        }

        public List<MOBMobileCMSContentMessages> ConfirmationPageAlertMessages
        {
            get
            {
                return this.confirmationPageAlertMessages;
            }
            set
            {
                this.confirmationPageAlertMessages = value;
            }
        }
        public List<MOBFOPCertificate> ProfileTravelerCertificates
        {
            get { return profileTravelerCertificates; }
            set { profileTravelerCertificates = value; }
        }
        public List<MOBFOPCertificateTraveler> CertificateTravelers
        {
            get { return certificateTravelers; }
            set { certificateTravelers = value; }
        }

        public string BundleCartId
        {
            get { return bundleCartId; }
            set
            {
                bundleCartId = value;
            }
        }

        public string PartialCouponEligibleUrl
        {
            get { return partialCouponEligibleUrl; }
            set { partialCouponEligibleUrl = value; }
        }

        public string PartialCouponEligibleMessage
        {
            get { return partialCouponEligibleMessage; }
            set { partialCouponEligibleMessage = value; }
        }
        public bool IsCouponEligibleProduct
        {
            get { return isCouponEligibleProduct; }
            set { isCouponEligibleProduct = value; }
        }

        public MOBCPTraveler Travelers
        {
            get { return travelers; }
            set { travelers = value; }
        }


        public bool IsCouponApplied
        {
            get { return isCouponApplied; }
            set { isCouponApplied = value; }
        }

        public bool DisableCouponEditOption
        {
            get { return disableCouponEditOption; }
            set { disableCouponEditOption = value; }
        }

        public string CouponCode
        {
            get { return couponCode; }
            set { couponCode = value; }
        }

        public string CouponOfferDescription
        {
            get { return couponOfferDescription; }
            set { couponOfferDescription = value; }
        }


        public string CartId
        {
            get { return cartId; }
            set { cartId = value; }
        }
        public string Flow
        {
            get { return flow; }
            set { flow = value; }
        }
        public List<MOBProdDetail> Products
        {
            get { return products; }
            set { products = value; }
        }
        public string TotalPrice
        {
            get { return totalPrice; }
            set { totalPrice = value; }
        }
        public string DisplayTotalPrice
        {
            get { return displayTotalPrice; }
            set { displayTotalPrice = value; }
        }
        public string DisplaySubTotalPrice
        {
            get { return displaySubTotalPrice; }
            set { displaySubTotalPrice = value; }
        }
        public string DisplayTaxesAndFees
        {
            get { return displayTaxesAndFees; }
            set { displayTaxesAndFees = value; }
        }
        public string TotalMiles
        {
            get { return totalMiles; }
            set { totalMiles = value; }
        }
        public MOBFormofPaymentDetails FormofPaymentDetails
        {
            get { return formofPaymentDetails; }
            set { formofPaymentDetails = value; }
        }
        public List<MOBCPTraveler> SCTravelers
        {
            get { return scTravelers; }
            set { scTravelers = value == null ? new List<MOBCPTraveler>() : value; }
        }
        public string PointofSale
        {
            get { return pointofSale; }
            set { pointofSale = value; }
        }
        public string CurrencyCode
        {
            get { return currencyCode; }
            set { currencyCode = value; }
        }
        public List<MOBSection> AlertMessages
        {
            get { return alertMessages; }
            set { alertMessages = value; }
        }

        public List<MOBMobileCMSContentMessages> TermsAndConditions
        {
            get { return termsAndConditions; }
            set
            {
                termsAndConditions = value;
                PopulateTermsAndConditionsForOldClient(termsAndConditions);
            }
        }

        public string PaymentTarget
        {
            get { return paymentTarget; }
            set { paymentTarget = value; }
        }

        private void PopulateTermsAndConditionsForOldClient(List<MOBMobileCMSContentMessages> termsAndConditions)
        {
            if (termsAndConditions != null && termsAndConditions.Any() &&
                products != null && products.Any() &&
                products.FirstOrDefault() != null)
            {
                products.FirstOrDefault().TermsAndCondition = termsAndConditions.FirstOrDefault();
            }
        }

        public List<MOBSHOPTrip> Trips
        {
            get
            {
                return this.trips;
            }
            set
            {
                this.trips = value;
            }
        }
        public List<MOBSHOPPrice> Prices
        {
            get { return prices; }
            set { prices = value; }
        }

        public List<List<MOBSHOPTax>> Taxes
        {
            get { return taxes; }
            set { taxes = value; }
        }


        public List<MOBItem> Captions
        {
            get { return captions; }
            set { captions = value; }
        }
        public List<MOBItem> ELFLimitations
        {
            get { return elfLimitations; }
            set { elfLimitations = value; }
        }
        public string DisplayTotalMiles
        {
            get { return displayTotalMiles; }
            set { displayTotalMiles = value; }
        }
        public string FlightShareMessage
        {
            get
            {
                return this.flightShareMessage;
            }
            set
            {
                this.flightShareMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public List<MOBSection> PaymentAlerts
        {
            get
            {
                return this.paymentAlerts;
            }
            set
            {
                this.paymentAlerts = value;
            }
        }
        public bool IsMultipleTravelerEtcFeatureClientToggleEnabled
        {
            get
            {
                return this.isMultipleTravelerEtcFeatureClientToggleEnabled;
            }
            set
            {
                this.isMultipleTravelerEtcFeatureClientToggleEnabled = value;
            }
        }

        public MOBULTripInfo TripInfoForUplift
        {
            get { return tripInfoForUplift; }
            set { tripInfoForUplift = value; }
        }

        private bool isHidePaymentMethod;

        public bool IsHidePaymentMethod
        {
            get { return isHidePaymentMethod; }
            set { isHidePaymentMethod = value; }
        }
        private MOBCart omniCart;

        public MOBCart OmniCart
        {
            get { return omniCart; }
            set { omniCart = value; }
        }

        public string CorporateDisclaimerText
        {
            get { return corporateDisclaimerText; }
            set { corporateDisclaimerText = value; }
        }
        public List<MOBSection> DisplayMessage
        {
            get
            {
                return this.displayMessage;
            }
            set
            {
                this.displayMessage = value;
            }
        }

        public string TotalPoints
        { get { return totalPoints; } set { totalPoints = value; } }

        public string DisplayTotalPoints
        { get { return displayTotalPoints; } set { displayTotalPoints = value; } }

        public MOBSHOPTripShare TripShare
        {
            get { return tripShare; }
            set { tripShare = value; }
        }
        private TravelPolicyWarningAlert travelPolicyWarningAlert;
        public TravelPolicyWarningAlert TravelPolicyWarningAlert
        {
            get { return travelPolicyWarningAlert; }
            set { travelPolicyWarningAlert = value; }
        }
        private MOBOffer offers;

        public MOBOffer Offers
        {
            get { return offers; }
            set { offers = value; }
        }
        private PartnerProvisionDetails partnerProvisionDetails;
        public PartnerProvisionDetails PartnerProvisionDetails {
            get { return partnerProvisionDetails; }
            set { partnerProvisionDetails = value; }
        }

        private bool isCorporateBusinessNamePersonalized;
        public bool IsCorporateBusinessNamePersonalized
        {
            get { return isCorporateBusinessNamePersonalized; }
            set { isCorporateBusinessNamePersonalized = value; }
        }
    }
    [Serializable()]
    public class PartnerProvisionDetails
    {
        private string objectName = "United.Mobile.Model.Shopping.PartnerProvisionDetails";
        public string ObjectName
        {
            get
            {
                return this.objectName;
            }
            set
            {
                this.objectName = value;
            }
        }
        private bool isEnableProvisionLogin = false;
        public bool IsEnableProvisionLogin
        {
            get { return isEnableProvisionLogin; }
            set { isEnableProvisionLogin = value; }
        }
        private string provisionLoginMessage = string.Empty;
        public string ProvisionLoginMessage
        {
            get { return provisionLoginMessage; }
            set { provisionLoginMessage = value; }
        }
        private string provisionLinkedCardMessage;
        public string ProvisionLinkedCardMessage
        {
            get { return this.provisionLinkedCardMessage; }
            set { this.provisionLinkedCardMessage = value; }
        }
        private bool isItChaseProvisionCard = false;
        public bool IsItChaseProvisionCard
        {
            get { return isItChaseProvisionCard; }
            set { isItChaseProvisionCard = value; }
        }
    }
    [Serializable()]
    public class MOBProdDetail
    {
        private string code = string.Empty;
        private string prodDescription = string.Empty;
        private string prodTotalPrice = string.Empty;
        private string prodDisplayTotalPrice = string.Empty;
        private string prodOtherPrice = string.Empty;
        private string prodDisplayOtherPrice = string.Empty;
        private string prodDisplaySubTotal = string.Empty;
        private string prodDisplayTaxesAndFees = string.Empty;
        private MOBMobileCMSContentMessages termAndCondition;
        private List<MOBProductSegmentDetail> segments;
        private Int32 prodTotalMiles;
        private string prodDisplayTotalMiles;
        private Int32 prodTotalPoints;
        private string prodDisplayTotalPoints;
        private List<MOBTypeOption> lineItems;
        private List<CouponDetails> couponDetails;
        private string prodTotalRequiredMiles = string.Empty;
        private string prodDisplayTotalRequiredMiles = string.Empty;

        public List<CouponDetails> CouponDetails
        {
            get { return couponDetails; }
            set { couponDetails = value; }
        }


        public string ProdDescription
        {
            get { return prodDescription; }
            set { prodDescription = value; }
        }
        public string Code
        {
            get { return code; }
            set { code = value; }
        }
        public string ProdTotalPrice
        {
            get { return prodTotalPrice; }
            set { prodTotalPrice = value; }
        }
        public string ProdTotalRequiredMiles
        {
            get { return prodTotalRequiredMiles; }
            set { prodTotalRequiredMiles = value; }
        }
        public string ProdDisplayTotalRequiredMiles
        {
            get { return prodDisplayTotalRequiredMiles; }
            set { prodDisplayTotalRequiredMiles = value; }
        }
        public string ProdDisplayTotalPrice
        {
            get { return prodDisplayTotalPrice; }
            set { prodDisplayTotalPrice = string.IsNullOrEmpty(value) ? string.Empty : value.ToString().Trim().ToString(new CultureInfo("en-us")); }
        }
        public string ProdOtherPrice
        {
            get { return prodOtherPrice; }
            set { prodOtherPrice = value; }
        }
        public string ProdDisplayOtherPrice
        {
            get { return prodDisplayOtherPrice; }
            set { prodDisplayOtherPrice = value; }
        }
        public string ProdDisplaySubTotal
        {
            get { return prodDisplaySubTotal; }
            set { prodDisplaySubTotal = value; }
        }
        public string ProdDisplayTaxesAndFees
        {
            get { return prodDisplayTaxesAndFees; }
            set { prodDisplayTaxesAndFees = value; }
        }

        [JsonProperty(PropertyName = "termAndCondition")]
        [JsonPropertyName("termAndCondition")]

        public MOBMobileCMSContentMessages TermsAndCondition
        {
            get { return termAndCondition; }
            set { termAndCondition = value; }
        }

        //   [JsonProperty("segments")]
        public List<MOBProductSegmentDetail> Segments
        {
            get { return segments; }
            set { segments = value; }
        }
        public Int32 ProdTotalMiles
        {
            get { return prodTotalMiles; }
            set { prodTotalMiles = value; }
        }
        public string ProdDisplayTotalMiles
        {
            get { return prodDisplayTotalMiles; }
            set { prodDisplayTotalMiles = value; }
        }
        public Int32 ProdTotalPoints { get { return prodTotalPoints; } set { prodTotalPoints = value; } }
        public string ProdDisplayTotalPoints { get { return prodDisplayTotalPoints; } set { prodDisplayTotalPoints = value; } }

        public List<MOBTypeOption> LineItems
        {
            get { return lineItems; }
            set { lineItems = value; }
        }
        private string prodOriginalPrice;

        public string ProdOriginalPrice
        {
            get { return prodOriginalPrice; }
            set { prodOriginalPrice = value; }
        }

    }

    [Serializable()]
    [XmlRoot("MOBProductSegmentDetail")]
    public class MOBProductSegmentDetail
    {
        private string segmentInfo;
        private string productId = string.Empty;
        private string tripId = string.Empty;
        private List<string> productIds;
        private List<MOBProductSubSegmentDetail> subSegmentDetails;
        private string segmentId = string.Empty;
        public string SegmentInfo
        {
            get { return segmentInfo; }
            set { segmentInfo = value; }
        }
        public string ProductId
        {
            get { return productId; }
            set { productId = value; }
        }
        public string TripId
        {
            get { return tripId; }
            set { tripId = value; }
        }
        public List<string> ProductIds
        {
            get { return productIds; }
            set { productIds = value; }
        }
        public string SegmentId { get { return segmentId; } set { segmentId = value; } }
        public List<MOBProductSubSegmentDetail> SubSegmentDetails
        {
            get { return subSegmentDetails; }
            set { subSegmentDetails = value; }
        }
    }
    [Serializable()]
    public class MOBProductSubSegmentDetail
    {
        private string passenger = string.Empty;
        private string price = string.Empty;
        private string moneyAmount = string.Empty;
        private string milesPrice = string.Empty;
        private string displayPrice = string.Empty;
        private string segmentDescription;
        private bool isPurchaseFailure = false;
        private string strikeOffPrice = string.Empty;
        private string displayStrikeOffPrice = string.Empty;
        //Required for strike off price to identify the flight segments uniquely
        private string seatCode = string.Empty;
        private string flightNumber = string.Empty;
        private DateTime departureTime;
        private DateTime arrivalTime;
        private Int32 miles;
        private string displayMiles;
        private int strikeOffMiles;
        private string displayStrikeOffMiles = string.Empty;
        private string displayMilesPrice;
        private string currency;

        public string Currency
        {
            get { return currency; }
            set { currency = value; }
        }

        public string Passenger
        {
            get { return passenger; }
            set { passenger = value; }
        }
        public string Price
        {
            get { return price; }
            set { price = value; }
        }
        public string MoneyAmount
        {
            get { return moneyAmount; }
            set { moneyAmount = value; }
        }
        public string MilesPrice
        {
            get { return milesPrice; }
            set { milesPrice = value; }
        }
        public string DisplayPrice
        {
            get { return displayPrice; }
            set { displayPrice = value; }
        }

        public string SegmentDescription
        {
            get { return segmentDescription; }
            set { segmentDescription = value; }
        }
        public bool IsPurchaseFailure
        {
            get { return isPurchaseFailure; }
            set { isPurchaseFailure = value; }
        }
        public string StrikeOffPrice
        {
            get { return strikeOffPrice; }
            set { strikeOffPrice = value; }
        }
        public string DisplayStrikeOffPrice
        {
            get { return displayStrikeOffPrice; }
            set { displayStrikeOffPrice = value; }
        }
        public string SeatCode
        {
            get { return seatCode; }
            set { seatCode = value; }
        }
        public string FlightNumber
        {
            get { return flightNumber; }
            set { flightNumber = value; }
        }
        public DateTime DepartureTime
        {
            get { return departureTime; }
            set { departureTime = value; }
        }
        public DateTime ArrivalTime
        {
            get { return arrivalTime; }
            set { arrivalTime = value; }
        }
        public Int32 Miles
        {
            get { return miles; }
            set { miles = value; }
        }
        public string DisplayMiles
        {
            get { return displayMiles; }
            set { displayMiles = value; }
        }
        public string DisplayMilesPrice
        {
            get { return displayMilesPrice; }
            set { displayMilesPrice = value; }
        }

        public Int32 StrikeOffMiles
        {
            get { return strikeOffMiles; }
            set { strikeOffMiles = value; }
        }
        public string DisplayStrikeOffMiles
        {
            get { return displayStrikeOffMiles; }
            set { displayStrikeOffMiles = value; }
        }
        private string orginalPrice;

        public string OrginalPrice
        {
            get { return orginalPrice; }
            set { orginalPrice = value; }
        }
        private MOBPromoCode promoDetails;

        public MOBPromoCode PromoDetails
        {
            get { return promoDetails; }
            set { promoDetails = value; }
        }

        private string productDescription;

        public string ProductDescription
        {
            get { return productDescription; }
            set { productDescription = string.IsNullOrEmpty(value) ? string.Empty : value; }
        }
        private List<string> prodDetailDescription;

        public List<string> ProdDetailDescription
        {
            get { return prodDetailDescription; }
            set { prodDetailDescription = value; }
        }

        private List<MOBPaxDetails> paxDetails;

        public List<MOBPaxDetails> PaxDetails
        {
            get { return paxDetails; }
            set { paxDetails = value; }
        }

        private string displayOriginalPrice;

        public string DisplayOriginalPrice
        {
            get { return displayOriginalPrice; }
            set { displayOriginalPrice = value; }
        }
        private string segmentInfo;

        public string SegmentInfo
        {
            get { return segmentInfo; }
            set { segmentInfo = string.IsNullOrEmpty(value) ? string.Empty : value; }
        }


    }
    [Serializable()]
    public class SCProductContext
    {
        public string Description { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
    [Serializable()]
    public class CouponDetails
    {
        private string promoCode;
        private string product;
        private string description;
        private United.Service.Presentation.CommonEnumModel.CouponDiscountType discountType;

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public string Product
        {
            get { return product; }
            set { product = value; }
        }

        public string PromoCode
        {
            get { return promoCode; }
            set { promoCode = value; }
        }
        private string isCouponEligible;
        public string IsCouponEligible
        {
            get { return isCouponEligible; }
            set { isCouponEligible = value; }
        }

        public CouponDiscountType DiscountType
        {
            get { return discountType; }
            set { discountType = value; }
        }
    }
    [Serializable()]
    public class MOBCart
    {
        private int cartItemsCount;

        public int CartItemsCount
        {
            get { return cartItemsCount; }
            set { cartItemsCount = value; }
        }
        private MOBItem payLaterPrice;

        public MOBItem PayLaterPrice
        {
            get { return payLaterPrice; }
            set { payLaterPrice = value; }
        }
        private string costBreakdownFareHeader = "Fare";

        public string CostBreakdownFareHeader
        {
            get { return costBreakdownFareHeader; }
            set { costBreakdownFareHeader = value; }
        }

        private MOBItem totalPrice;

        public MOBItem TotalPrice
        {
            get { return totalPrice; }
            set { totalPrice = value; }
        }
        private string navigateToScreen;

        public string NavigateToScreen
        {
            get { return navigateToScreen; }
            set { navigateToScreen = value; }
        }
        private bool isCallRegisterOffers;
        public bool IsCallRegisterOffers
        {
            get { return isCallRegisterOffers; }
            set { isCallRegisterOffers = value; }
        }
        private List<MOBOmniCartRepricingInfo> omniCartPricingInfos;

        public List<MOBOmniCartRepricingInfo> OmniCartPricingInfos
        {
            get { return omniCartPricingInfos; }
            set { omniCartPricingInfos = value; }
        }
        private int navigateToSeatmapSegmentNumber;

        public int NavigateToSeatmapSegmentNumber
        {
            get { return navigateToSeatmapSegmentNumber; }
            set { navigateToSeatmapSegmentNumber = value; }
        }

        private bool isUpliftEligible;
        public bool IsUpliftEligible
        {
            get { return isUpliftEligible; }
            set { isUpliftEligible = value; }
        }

        private List<MOBSection> fopDetails;
        public List<MOBSection> FOPDetails
        {
            get { return fopDetails; }
            set { fopDetails = value; }
        }

        private MOBSection additionalMileDetail;
        public MOBSection AdditionalMileDetail
        {
            get { return additionalMileDetail; }
            set { additionalMileDetail = value; }
        }

        private string corporateDisclaimerText;

        public string CorporateDisclaimerText
        {
            get { return corporateDisclaimerText; }
            set { corporateDisclaimerText = value; }
        }
    }
    [Serializable()]
    public class MOBOmniCartRepricingInfo
    {
        private string product;
        public string Product
        {
            get { return product; }
            set { product = value; }
        }
        private MOBSection repriceAlertMessage;

        public MOBSection RepriceAlertMessage
        {
            get { return repriceAlertMessage; }
            set { repriceAlertMessage = value; }
        }
        private List<MOBOmniCartRepricingSegmentInfo> segments;
        public List<MOBOmniCartRepricingSegmentInfo> Segments
        {
            get { return segments; }
            set { segments = value; }
        }
    }
    [Serializable()]
    public class MOBOmniCartRepricingSegmentInfo
    {
        private int segmentNumber;

        public int SegmentNumber
        {
            get { return segmentNumber; }
            set { segmentNumber = value; }
        }

    }
    [Serializable()]
    public class MOBPaxDetails
    {
        private string fullName;

        public string FullName
        {
            get { return fullName; }
            set { fullName = value; }
        }
        private string seat;

        public string Seat
        {
            get { return seat; }
            set { seat = value; }
        }
        private string key;

        public string Key
        {
            get { return key; }
            set { key = value; }
        }
    }

    [Serializable]
    public enum MOBRequestObjectName
    {
        [EnumMember(Value = "RegisterSeats")]
        RegisterSeats,
        [EnumMember(Value = "RegisterCheckInSeats")]
        RegisterCheckInSeats,
        [EnumMember(Value = "RegisterBags")]
        RegisterBags
    }
    [Serializable]
    public class MOBOffer
    {
        private string offerCode;

        public string OfferCode
        {
            get { return offerCode; }
            set { offerCode = value; }
        }
        private bool isPassPlussOffer;

        public bool IsPassPlussOffer
        {
            get { return isPassPlussOffer; }
            set { isPassPlussOffer = value; }
        }
        private OfferType offerType;

        public OfferType OfferType
        {
            get { return offerType; }
            set { offerType = value; }
        }
    }
}
