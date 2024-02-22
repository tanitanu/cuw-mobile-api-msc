using United.Definition.Shopping;
using United.Persist.Definition.Shopping;
using United.Services.FlightShopping.Common.FlightReservation;


namespace United.Common.Helper.Product
{
    public interface IShoppingBuyMilesHelper
    {
        void ApplyPriceChangesForBuyMiles(FlightReservationResponse flightReservationResponse, MOBSHOPReservation reservation = null,
            Reservation bookingPathReservation = null);
        void ApplyPriceChangesForBuyMiles(MOBSHOPReservation reservation = null, Reservation bookingPathReservation = null);

    }
}
