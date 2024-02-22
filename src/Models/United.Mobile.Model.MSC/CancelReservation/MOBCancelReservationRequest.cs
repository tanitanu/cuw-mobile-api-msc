using United.Mobile.Model.Common;
namespace United.Definition.CancelReservation
{
    public class MOBCancelReservationRequest : MOBRequest
    {
        public string RecordLocator { get; set; }
        public string EmailAddress { get; set; }
        public string LastName { get; set; }
        public string PointOfSale { get; set; }
        public string MileagePlusNumber { get; set; }
    }
}
