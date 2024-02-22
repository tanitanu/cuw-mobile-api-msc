using System.Collections.Generic;
using System.Threading.Tasks;
using United.Definition;
using United.Mobile.Model.Common;
using United.Persist.Definition.Shopping;
using United.Services.FlightShopping.Common.DisplayCart;
using United.Services.FlightShopping.Common.FlightReservation;

namespace United.Common.Helper.MSCPayment.Interfaces
{
    public interface ICheckoutUtiliy
    {
        Task<MOBCheckOutResponse> CheckOut(MOBCheckOutRequest request);
        Task<Session> GetValidateSession(string sessionId, bool isBookingFlow, bool isViewRes_CFOPFlow);
        bool IsInclueWithThisToggle(int appId, string appVersion, string configToggleKey, string androidVersion, string iosVersion);
        Task<MOBVormetricKeys> GetPersistentTokenUsingAccountNumberToken(string accountNumberToke, string sessionId, string token);
        Task<(bool GeneratedCCTokenWithDataVault, string CcDataVaultToken)> GenerateCCTokenWithDataVault(MOBCreditCard creditCardDetails, string sessionID, string token, MOBApplication applicationDetails, string deviceID, string ccDataVaultToken);
        Task<MOBVormetricKeys> AssignPersistentTokenToCC(string accountNumberToken, string persistentToken, string securityCodeToken, string cardType, string sessionId, string action, int appId, string deviceID);
        List<MOBSection> AssignRefundMessage(List<MOBItem> refundMessages);
        Task<FlightReservationResponse> RegisterFormsOfPayments_CFOP(MOBCheckOutRequest checkOutRequest, Session session);
        bool IsInternationalBillingAddress_CheckinFlowEnabled(MOBApplication application);
        MOBSeatPrice BuildSeatPrice(MOBSeat seat);
        Task<MOBCheckOutResponse> ViewResRFOPResponse(FlightReservationResponse flightReservationResponse, MOBCheckOutRequest request, MOBShoppingCart persistShoppingCart, Session session);
        void NotifyTPIOfferDeclined(string sessionId);
        Task<MOBPNRAdvisory> PopulateSeatConfirmarionAlertMessage(Session session, MOBRequest request);
    }
}
