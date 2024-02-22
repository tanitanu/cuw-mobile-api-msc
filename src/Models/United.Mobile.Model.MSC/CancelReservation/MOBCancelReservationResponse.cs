using System;
using United.Mobile.Model.Common;
namespace United.Definition.CancelReservation
{
    [Serializable]
    public class MOBCancelReservationResponse : MOBResponse
    {
        public String pnr;
        public String email;
    }
}
