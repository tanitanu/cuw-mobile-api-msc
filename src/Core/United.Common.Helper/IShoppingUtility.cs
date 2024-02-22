using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using United.Definition;
using United.Definition.Shopping;
using United.Mobile.Model.Common;
using United.Persist.Definition.Shopping;
using United.Services.FlightShopping.Common.DisplayCart;
using United.Services.FlightShopping.Common.FlightReservation;

namespace United.Common.Helper
{
    public interface IShoppingUtility
    {
        System.Threading.Tasks.Task ValidateAwardMileageBalance(string sessionId, decimal milesNeeded);
        double GetGrandTotalPriceForShoppingCart(bool isCompleteFarelockPurchase, FlightReservationResponse flightReservationResponse, bool isPost, string flow = "VIEWRES");
        Task<string> GetAirportName(string airportCode);
        bool EnableShoppingcartPhase2ChangesWithVersionCheck(int appId, string appVersion);
        string BuildPaxTypeDescription(string paxTypeCode, string paxDescription, int paxCount);
        bool EnableEditForAllCabinPOM(int appId, string appVersion);
        bool EnablePOMPreArrival(int appId, string appVersion);

        double UpdatePromoValueForFSRMoneyMiles(List<DisplayPrice> prices, Session session, double promoValue);
        bool IsEnablePartnerProvision(List<MOBItem> catalogItems, string flow, int applicationId, string appVersion);
        CultureInfo GetCultureInfo(string currencyCode);
        string GetFormatPriceSearch(string searchType);
        string BuildPriceTypeDescription(string searchType, string priceDescription, int price, string desc, bool isFareLockViewRes, bool isCorporateFareLock);
        string GetSearchTypeDesc(string searchType);
        string BuildYAPriceTypeDescription(string searchType);
        string GetFareDescription(DisplayPrice price);
        bool EnableYADesc(bool isReshop = false);
        List<MOBSHOPPrice> AdjustTotal(List<MOBSHOPPrice> prices);
        List<List<MOBSHOPTax>> GetTaxAndFeesAfterPriceChange(List<United.Services.FlightShopping.Common.DisplayCart.DisplayPrice> prices, bool isReshopChange = false, int appId = 0, string appVersion = "", string travelType = null);
        List<List<MOBSHOPTax>> GetTaxAndFeesAfterPriceChange(List<United.Services.FlightShopping.Common.DisplayCart.DisplayPrice> prices, int numPax, bool isReshopChange = false, int appId = 0, string appVersion = "", string travelType = null);
        public List<MOBSHOPTax> AddFeesAfterPriceChange(List<United.Services.FlightShopping.Common.DisplayCart.DisplayPrice> prices);
        string BuildStrikeThroughDescription();
        void UpdatePricesForTravelCredits(List<MOBSHOPPrice> bookingPrices, DisplayPrice price, MOBSHOPPrice bookingPrice, Session session);
        void SetETCTravelCreditsEligible(List<FormofPaymentOption> EligibleFormofPayments, MOBSHOPReservation reservation);
    }
}
