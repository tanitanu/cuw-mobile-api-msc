using System;
using System.Runtime.Serialization;

namespace United.Definition
{
    [Serializable()]
    public enum MOBTripType
    {
        RoundTrip = 1,
        OneWay = 2
    }

    [Serializable()]
    public enum MOBReservationType
    {
        All = 0,
        Current = 1,
        Past = 2,
        Cancelled = 3,
        Inactive = 4
    }

    [Serializable()]
    public enum MOBPASegmentType
    {
        [EnumMember(Value = "0")]
        Regular = 0,
        [EnumMember(Value = "1")]
        SoldOut = 1,
        [EnumMember(Value = "2")]
        NotOffered = 2,
        [EnumMember(Value = "3")]
        AlreadyPurchased = 3
    }
    [Serializable()]
    public enum MOBPACustomerType
    {
        [EnumMember(Value = "0")]
        Regular = 0,
        [EnumMember(Value = "1")]
        AlreadyPurchased = 1,
        [EnumMember(Value = "2")]
        AlreadyPremier = 2
    }

    [Serializable()]
    public enum MOBPBSegmentType
    {
        [EnumMember(Value = "0")]
        Regular = 0,
        [EnumMember(Value = "1")]
        AlreadyPurchased = 1,
        [EnumMember(Value = "2")]
        InEligible = 2,
        [EnumMember(Value = "3")]
        Included = 3
    }

    [Serializable()]
    public enum MOBFSREnhancementType
    {
        [EnumMember(Value = "0")]
        NoResultsSuggestNearByAirports = 0,
        [EnumMember(Value = "1")]
        WithResultsSuggestNearByOrigins = 1,
        [EnumMember(Value = "2")]
        WithResultsSuggestNearByDestinations = 2,
        [EnumMember(Value = "3")]
        WithResultsSuggestNearByOrigsAndDests = 3,
        [EnumMember(Value = "4")]
        NoResultsDestinationSeasonal = 4,
        [EnumMember(Value = "5")]
        NoResultsOriginSeasonal = 5,
        [EnumMember(Value = "6")]
        NoResultsOrigAndDestSeasonal = 6,
        [EnumMember(Value = "7")]
        SuggestNonStopFutureDate = 7,
        [EnumMember(Value = "8")]
        WithResultsForceNearByOrigin = 8,
        [EnumMember(Value = "9")]
        WithResultsForceNearByDestination = 9,
        [EnumMember(Value = "10")]
        WithResultsForceNearByOrigAndDest = 10,
        [EnumMember(Value = "11")]
        WithResultsForceGSTByOrigin = 11,
    }

    public enum MOBTravelSpecialNeedType
    {
        Unknown,
        SpecialMeal,
        SpecialRequest,
        ServiceAnimal,
        ServiceAnimalType
    };

    public enum MOBIConType
    {
        None,
        Warning,
        Info,
        Error,
        Question,
    };

    public enum PostPurchasePage
    {
        None,
        PNRRetrival,
        Confirmation,
        SecondaryFormOfPayment
    };

    public enum MOBErrorCodes
    {
        [EnumMember(Value = "900111")]
        ViewResCFOPSessionExpire = 900111,      //Error code for session expire in View/Manage reservation flow implemented during Common FOP
        [EnumMember(Value = "900112")]
        ViewResCFOP_NullSession_AfterAppUpgradation = 900112,      //Error code for null session if the app get update during the process. Actual Message: "We're sorry, we are currently updating the mobile app. Please reload your reservation."
    };

    public enum MOBSeatType
    {
        BLUE,
        PREF,
        STANDARD,
        PURPLE,
        FBLEFT,
        FBRIGHT,
        FBBACK,
        FBFRONT,
        DAAFRONTL,
        DAAFRONTR,
        DAAFRONTRM,
        DAALEFT,
        DAARIGHT,
        FRONT
    };

    public enum MOBSeatValue
    {
        X,
        P,
        PZ,
        O
    };

    public enum MOBSeatPosition
    {
        FBB,
        FBF,
        FBL,
        FBR,
        DAFL,
        DAFR,
        DAL,
        DAR,
        DAFRM
    }
    public enum MOBNavigationToScreen
    {
        TRAVELOPTIONS,
        SEATS,
        FINALRTI
    }
    public enum ServiceNames
    {
        SHOPPING,
        SHOPAWARD,
        SHOPFAREWHEEL,
        SHOPFLIGHTDETAILS,
        SHOPTRIPS,
        SHOPSEATS,
        SHOPBUNDLES,
        BAGCALCULATOR,
        POSTBOOKING,
        SEATENGINE,
        TRAVELERS,
        TRIPPLANNER,
        TRIPPLANNERGETSERVICE,
        MEMBERPROFILE,
        UPDATEMEMBERPROFILE,
        UNFINISHEDBOOKING,
        SEATMAP,
        UNITEDCLUBPASSES,
        RESHOP,
        PRODUCT,
        ETC,
        MONEYPLUSMILES,
        MSCCHECKOUT,
        MSCREGISTER,
        PAYMENT,
        PROMOCODE,
        TRAVELCREDIT
    }
    public enum OfferType
    {
        NONE,
        ECD,
        UNITEDPASSPLUSSECURE,
        UNITEDPASSPLUSFLEX,
        VETERAN,
        UNITEDMEETINGS
    }
    public enum ComponentType
    {
        NONE,
        NUMERIC,
        ALPHANUMERIC,
        TEXTONLY,
        DATE,
        EMAIL
    }

    public enum TaxIdCountryType
    {
        TRAVELER,
        PURCHASER
    }
}
