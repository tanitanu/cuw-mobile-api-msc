using United.Definition;
using United.Definition.Shopping;

namespace United.Common.Helper.MSCPayment.Interfaces
{
    public interface IMSCBaggageInfo
    {
        System.Threading.Tasks.Task<MOBDOTBaggageInfo> GetBaggageInfo(MOBSHOPReservation reservation);

        System.Threading.Tasks.Task<MOBDOTBaggageInfo> GetBaggageInfo(MOBPNR pnr);

        //MOBDOTBaggageInfo GetBaggageInfo(United.Service.Presentation.ReservationModel.Reservation reservation);
    }
}
