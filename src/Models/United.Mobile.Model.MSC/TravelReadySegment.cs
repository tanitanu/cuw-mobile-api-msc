using System;

namespace United.Definition
{
    //[Serializable]
    public class TravelReadySegment
   {         
        public Int64 Id { get; set; }

        public string DeviceId { get; set; }

        public int ApplicationId { get; set; }

        public string ApplicationVersion { get; set; }

        public string MPNumber { get; set; }

        public string PushToken { get; set; }

        public string RecordLocator { get; set; }

        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string FlightNumber { get; set; }

        public DateTime FlightDate { get; set; }

        public string Origin { get; set; }

        public string Destination { get; set; }

        public string ScheduledDepartureTimeUtc { get; set; }

        public string ScheduleArrivalTimeUtc { get; set; }

        public bool CheckinEligible { get; set; }

        public bool Checkedin { get; set; }

        public bool Onboard { get; set; }

        public DateTime? LandedDateTimeUtc { get; set; }

        public string TripNumber { get; set; }

        public string SegmentNumber { get; set; }
    }
}
