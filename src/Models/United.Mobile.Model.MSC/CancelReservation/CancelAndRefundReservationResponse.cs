using System;
using System.Collections.ObjectModel;

namespace United.Definition.CancelReservation
{
    [Serializable]
    public class CancelAndRefundReservationResponse
    {
        public string Pnr { get; set; }
        public string Email { get; set; }
        public virtual Collection<Error> Error { get; set; }
        public virtual Collection<Status> StatusDetails { get; set; }
    }

    public class Status
    {
        public string StatusId { get; set; } //Succeed / Failed / NotApplicable
        public string OperationName { get; set; } //FlightCancellation / Refund / Email
        public Error StatusDetail { get; set; }
    }
}
