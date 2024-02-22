using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using United.Definition;
using United.Definition.FormofPayment;
using United.Definition.Shopping;
using United.Persist.Definition.Shopping;
using United.Mobile.Model.Common;
using United.Services.FlightShopping.Common.DisplayCart;

namespace United.Common.Helper.MSCPayment.Interfaces
{
    public interface IETCUtility
    {
        Task<MOBFOPTravelerCertificateResponse> AddCertificatesToFOP(List<MOBFOPCertificate> requestedCertificates, Session session);
        Task<MOBFOPTravelCertificate> GetShoppingCartTravelCertificateFromPersist(Session session, MOBFOPTravelerCertificateResponse response);
        void UpdateSavedCertificate(MOBShoppingCart shoppingcart);
        Task UpdateReservationWithCertificatePrices(Session session, MOBFOPTravelerCertificateResponse response, double totalRedeemAmount, Reservation bookingPathReservation);
        void AssignIsOtherFOPRequired(MOBFormofPaymentDetails formofPaymentDetails, List<MOBSHOPPrice> prices, bool IsSecondaryFOP = false, bool isRemoveAll = false);
        void AssignCertificateRedeemAmount(MOBFOPCertificate requestCertificate, List<MOBSHOPPrice> prices);
        Task AssignLearmoreInformationDetails(MOBFOPTravelCertificate travelCertificate);
        void AssignCertificateRedeemAmount(MOBFOPCertificate requestCertificate, double amount);
        MOBSHOPPrice UpdateCertificatePrice(MOBSHOPPrice certificatePrice, double totalAmount);
        void UpdateCertificateRedeemAmountFromTotalInReserationPrices(MOBSHOPPrice price, double value, bool isRemove = true);

        Task<MOBSHOPReservation> GetReservationFromPersist(MOBSHOPReservation reservation, Session session);
        Task<List<MOBItem>> GetCaptions(string key);
        Task<MOBShoppingCart> InitialiseShoppingCartAndDevfaultValuesForETC(MOBShoppingCart shoppingcart, List<MOBProdDetail> products, string flow);

        double GetAlowedETCAmount(List<MOBProdDetail> products, string flow);

        void AssignTotalAndCertificateItemsToPrices(MOBShoppingCart shoppingcart);

        MOBSHOPReservation MakeReservationFromPersistReservation(MOBSHOPReservation reservation, Reservation bookingPathReservation,
          Session session);

        Task<List<MOBCPProfile>> LoadPersistedProfile(string sessionId, string flow);
        CultureInfo GetCultureInfo(string currencyCode);
        void UpdateCertificateAmountInTotalPrices(List<MOBSHOPPrice> prices, double certificateTotalAmount);
        Task<List<MOBMobileCMSContentMessages>> UpdateReviewETCAlertmessages(MOBShoppingCart shoppingCart);
        void AddRequestedCertificatesToFOPTravelerCertificates(List<MOBFOPCertificate> requestedCertificates, List<MOBFOPCertificate> profileTravelerCertificates, MOBFOPTravelCertificate travelerCertificate);
        void SetEligibilityforETCTravelCredit(MOBSHOPReservation reservation, Session session, Reservation bookingPathReservation);
        Task SetETCTraveCreditsReviewAlertMessage(List<MOBMobileCMSContentMessages> alertMessages);
        void AddGrandTotalIfNotExistInPrices(List<MOBSHOPPrice> prices);
    }
}
