using System.Collections.Generic;
using System.Threading.Tasks;
using United.Definition;
using United.Definition.FormofPayment;
using United.Definition.Shopping;
using United.Persist.Definition.Shopping;
using United.Mobile.Model.Common;
namespace United.Common.Helper.Product
{
    public interface IProductUtility
    {
        bool IsBuyMilesFeatureEnabled(int appId, string version);
        System.Threading.Tasks.Task<MOBSHOPReservation> GetReservationFromPersist(MOBSHOPReservation reservation, Session sessionID);
        MOBSHOPTripPriceBreakDown GetPriceBreakDown(Reservation reservation);
        System.Threading.Tasks.Task PreLoadTravelCredit(string sessionId, MOBShoppingCart shoppingCart, MOBRequest request, bool isLoadFromCSL = false);
        List<MOBMobileCMSContentMessages> SwapTitleAndLocation(List<MOBMobileCMSContentMessages> cmsList);
        List<MOBMobileCMSContentMessages> GetSDLContentMessages(List<CMSContentMessage> lstMessages, string title);
        System.Threading.Tasks.Task<List<CMSContentMessage>> GetSDLContentByGroupName(MOBRequest request, string guid, string token, string groupName, string docNameConfigEntry, bool useCache = false);
        Task<(MOBFOPResponse MobFopResponse, Reservation BookingPathReservation)> LoadBasicFOPResponse(Session session, Reservation bookingPathReservation);
        System.Threading.Tasks.Task<MOBFOPTravelBankDetails> PopulateTravelBankData(Session session, MOBSHOPReservation reservation, MOBRequest request);
        System.Threading.Tasks.Task<MOBCPTraveler> GetProfileOwnerTravelerCSL(string sessionID);
    }
}
