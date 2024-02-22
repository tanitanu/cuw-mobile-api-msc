using System;

namespace United.Mobile.DAL.Models
{
    public class CancelReservationRequest
    {
        public string Channel { get; set; }
        public string RecordLocator { get; set; }
        public string EmailAddress { get; set; }
        public string MileagePlusNumber { get; set; }
        public string PointOfSale { get; set; }
    }
}
