using System.Collections.Generic;
using System.Threading.Tasks;
using United.Definition;
using United.Definition.CFOP;
using United.Definition.FormofPayment;
using United.Definition.Shopping;
using United.Definition.Shopping.UnfinishedBooking;
using United.Mobile.Model.Common;

namespace United.Mobile.Product.Domain
{
    public interface IProductBusiness
    {
        Task<MOBBookingRegisterOfferResponse> RegisterOffersForBooking(MOBRegisterOfferRequest request);
        Task<MOBRESHOPRegisterOfferResponse> RegisterOffersForReshop(MOBRegisterOfferRequest request);
        Task<MOBBookingRegisterSeatsResponse> RegisterSeatsForBooking(MOBRegisterSeatsRequest request);
        Task<MOBReshopRegisterSeatsResponse> RegisterSeatsForReshop(MOBRegisterSeatsRequest request);
        Task<MOBBookingRegisterOfferResponse> ClearCartOnBackNavigation(MOBClearCartOnBackNavigationRequest request);
        Task<MOBBookingRegisterOfferResponse> RemoveAncillaryOffer(MOBRemoveAncillaryOfferRequest request);
        Task<MOBSHOPSelectTripResponse> RegisterOffersForOmniCartSavedTrip(MOBSHOPUnfinishedBookingRequestBase request);
        MOBSection GetPromoCodeAlertMessage(bool isContinueToRegisterAncillary);
        Task<MOBBookingRegisterOfferResponse> UnRegisterAncillaryOffersForBooking(MOBRegisterOfferRequest request);
        //void AddMissingSeatAssignment(List<MOBSeatPrice> tempSeatPrices, United.Persist.Definition.Shopping.Reservation persistedReservation);
        string BuildStrikeThroughDescription();
        Task<MOBCreateShoppingSessionResponse> CreateShoppingSession(MOBCreateShoppingSessionRequest request);
        Task<MOBResponse> PopulateMerchOffers(MOBPopulateMerchOffersRequest request);

        Task<MOBResponse> UpdateReservation(MOBUpdateReservationRequest request);
    }
}
