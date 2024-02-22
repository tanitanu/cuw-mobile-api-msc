using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using United.Definition;
using United.Definition.FormofPayment;
using United.Definition.Shopping;
using United.Definition.Uplift;
using United.Mobile.Model.Common;
using United.Persist.Definition.SeatChange;
using United.Persist.Definition.Shopping;
using United.Service.Presentation.InteractionModel;
using United.Service.Presentation.SegmentModel;
using United.Services.FlightShopping.Common.FlightReservation;

namespace United.Common.Helper
{
    public interface IMSCFormsOfPayment
    {
        Task<(List<FormofPaymentOption> EligibleFormofPayments, bool isDefault)> GetEligibleFormofPayments(MOBRequest request, Session session, MOBShoppingCart shoppingCart, string cartId, string flow, MOBSHOPReservation reservation = null, bool IsMilesFOPEnabled = false, SeatChangeState persistedState = null);
        Task<(List<FormofPaymentOption> EligibleFormofPayments, bool isDefault)> EligibleFormOfPayments(MOBFOPEligibilityRequest request, Session session, bool IsMilesFOPEnabled = false);
        FormofPaymentOption UpliftAsFormOfPayment(MOBSHOPReservation reservation, MOBShoppingCart shoppingCart);
        List<FormofPaymentOption> BuildEligibleFormofPaymentsResponse(List<FormofPaymentOption> response, MOBShoppingCart shoppingCart, Session session, MOBRequest request, bool isMetaSearch = false, bool isPoolMiles = false);
        List<FormofPaymentOption> BuildEligibleFormofPaymentsResponse(List<FormofPaymentOption> response, MOBShoppingCart shoppingCart, MOBRequest request);
        List<FormofPaymentOption> TravelBankEFOPFilter(MOBRequest request, MOBFOPTravelBankDetails travelBankDet, string flow, List<FormofPaymentOption> response);
        Collection<FOPProduct> GetProductsForEligibleFopRequest(MOBShoppingCart shoppingCart, SeatChangeState state = null);
        Task<United.Service.Presentation.InteractionModel.ShoppingCart> BuildEligibleFormOfPaymentsRequest(MOBFOPEligibilityRequest request, Session session);
        bool HasEligibleProductsForUplift(string totalPrice, List<MOBProdDetail> products);
        List<MOBCPTraveler> GetTravelerCSLDetails(United.Service.Presentation.ReservationModel.Reservation reservation, List<MOBSHOPTrip> shoptrips, string sessionId, string flow);
        Task<double> GetTravelBankBalance(string sessionId);
        bool IsEligibileForUplift(MOBSHOPReservation reservation, MOBShoppingCart shoppingCart);
        MOBCPTraveler MapCslPersonToMOBCPTravel(Service.Presentation.ReservationModel.Traveler cslTraveler, int paxIndex, Collection<ReservationFlightSegment> flightSegments, List<MOBSHOPTripBase> trips, string flow);
        string FirstLetterToUpperCase(string value);
        List<MOBCPPhone> GetMobCpPhones(United.Service.Presentation.PersonModel.Contact contact);
        Task<List<MOBCPBillingCountry>> GetCachedBillingAddressCountries();
        Task<List<MOBSHOPTrip>> GetTrips(Collection<United.Service.Presentation.SegmentModel.ReservationFlightSegment> FlightSegments, string flow);
        Task<MOBULTripInfo> GetUpliftTripInfo(United.Service.Presentation.ReservationModel.Reservation reservation, string totalPrice, List<MOBProdDetail> products, bool isGetCartInfo = false);
        Task<MOBULTripInfo> GetUpliftTripInfo(FlightReservationResponse flightReservationResponse, string totalPrice, List<MOBProdDetail> products, bool isGetCartInfo = false);
    }
}
