using System;

namespace United.Definition
{
    [Serializable()]
    public class MOBWalletPNRSegment
    {
        private string recordLocator = string.Empty;
        private string carrierCode = string.Empty;
        private string flightNumber = string.Empty;
        private string origin = string.Empty;
        private string destination = string.Empty;
        private string scheduledDepartureDateTime = string.Empty;
        private string scheduledArrivalDateTime = string.Empty;
        private string scheduledDepartureDateTimeGMT = string.Empty;
        private string scheduledArrivalDateTimeGMT = string.Empty;
        private string seats = string.Empty;
        private string activationDateTimeGMT = string.Empty;
        private MOBFlightStatusSegment flightStatusSegment;
        private MOBWalletFlightStatus flightStatus;
        private MOBWalletWeather destinationWeather;
        private string lastUpdated = string.Empty;
        private bool enableUberLinkButton;
        private string cabin = string.Empty;
        private string cabinType = string.Empty;
        private string classOfService = string.Empty;
        private string tripNumber = string.Empty;
        private string segmentNumber = string.Empty;
        private string ticketCouponStatus;

        public string TicketCouponStatus
        {
            get { return ticketCouponStatus; }
            set { ticketCouponStatus = value; }
        }


        public string RecordLocator
        {
            get
            {
                return this.recordLocator;
            }
            set
            {
                this.recordLocator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string CarrierCode
        {
            get
            {
                return this.carrierCode;
            }
            set
            {
                this.carrierCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string FlightNumber
        {
            get
            {
                return this.flightNumber;
            }
            set
            {
                this.flightNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Origin
        {
            get
            {
                return this.origin;
            }
            set
            {
                this.origin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string Destination
        {
            get
            {
                return this.destination;
            }
            set
            {
                this.destination = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string ScheduledDepartureDateTime
        {
            get
            {
                return this.scheduledDepartureDateTime;
            }
            set
            {
                this.scheduledDepartureDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ScheduledArrivalDateTime
        {
            get
            {
                return this.scheduledArrivalDateTime;
            }
            set
            {
                this.scheduledArrivalDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ScheduledArrivalDateTimeGMT
        {
            get
            {
                return this.scheduledArrivalDateTimeGMT;
            }
            set
            {
                this.scheduledArrivalDateTimeGMT = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ScheduledDepartureDateTimeGMT
        {
            get
            {
                return this.scheduledDepartureDateTimeGMT;
            }
            set
            {
                this.scheduledDepartureDateTimeGMT = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Seats
        {
            get
            {
                return this.seats;
            }
            set
            {
                this.seats = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ActivationDateTimeGMT
        {
            get
            {
                return this.activationDateTimeGMT;
            }
            set
            {
                this.activationDateTimeGMT = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBWalletFlightStatus FlightStatus
        {
            get
            {
                return this.flightStatus;
            }
            set
            {
                this.flightStatus = value;
            }
        }

        public MOBFlightStatusSegment FlightStatusSegment
        {
            get
            {
                return this.flightStatusSegment;
            }
            set
            {
                this.flightStatusSegment = value;
            }
        }

        public MOBWalletWeather DestinationWeather
        {
            get
            {
                return this.destinationWeather;
            }
            set
            {
                this.destinationWeather = value;
            }
        }

        public string LastUpdated
        {
            get
            {
                return this.lastUpdated;
            }
            set
            {
                this.lastUpdated = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool EnableUberLinkButton
        {
            get
            {
                return this.enableUberLinkButton;
            }
            set
            {
                this.enableUberLinkButton = value;
            }
        }

        public string Cabin
        {
            get
            {
                return this.cabin;
            }
            set
            {
                this.cabin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string CabinType
        {
            get
            {
                return this.cabinType;
            }
            set
            {
                this.cabinType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
                this.Cabin = GetCabin(this.cabinType);
            }
        }

        public string ClassOfService
        {
            get
            {
                return this.classOfService;
            }
            set
            {
                this.classOfService = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string TripNumber
        {
            get { return tripNumber; }
            set { tripNumber = value; }
        }

        public string SegmentNumber
        {
            get { return segmentNumber; }
            set { segmentNumber = value; }
        }


        private string GetCabin(string cabinType)
        {
            string cabin = string.Empty;

            if (!string.IsNullOrEmpty(cabinType))
            {
                switch (cabinType.ToLower())
                {
                    case "united economy":
                        cabin = "Coach";
                        break;
                    case "economy":
                        cabin = "Coach";
                        break;
                    case "united business":
                        cabin = "Business";
                        break;
                    case "business":
                        cabin = "Business";
                        break;
                    case "united businessfirst":
                        cabin = "BusinessFirst";
                        break;
                    case "businessfirst":
                        cabin = "BusinessFirst";
                        break;
                    case "united global first":
                        cabin = "First";
                        break;
                    case "united first":
                        cabin = "First";
                        break;
                    case "first":
                        cabin = "First";
                        break;
                }
            }
            return cabin;
        }

    }
}
