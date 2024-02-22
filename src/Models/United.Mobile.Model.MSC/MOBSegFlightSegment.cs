using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBSegFlightSegment
    {
        private MOBTMAAirport arrivalAirport;
        private string arrivalDateTime = string.Empty;
        private List<MOBComBookingClass> bookingClasses;
        private MOBTMAAirport departureAirport;
        private string departureDateTime = string.Empty;
        private int distance;
        private MOBComAircraft equipment;
        private string flightNumber = string.Empty;
        private int groundTime;
        private string isChangeOfGauge = string.Empty;
        private string isInternational = string.Empty;
        private int journeyDuration;
        private List<MOBSegFlightLeg> legs;
        private List<MOBComMessage> messages;
        private int numberOfStops;
        private double onTimeRate;
        private string operatingAirlineCode = string.Empty;
        private string operatingAirlineName = string.Empty;
        private int scheduledFlightDuration;
        private int segmentNumber;
        private List<MOBTMAAirport> stopLocations;

        public MOBTMAAirport ArrivalAirport
        {
            get
            {
                return this.arrivalAirport;
            }
            set
            {
                this.arrivalAirport = value;
            }
        }

        public string ArrivalDateTime
        {
            get
            {
                return this.arrivalDateTime;
            }
            set
            {
                this.arrivalDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBComBookingClass> BookingClasses
        {
            get
            {
                return this.bookingClasses;
            }
            set
            {
                this.bookingClasses = value;
            }
        }

        public MOBTMAAirport DepartureAirport
        {
            get
            {
                return this.departureAirport;
            }
            set
            {
                this.departureAirport = value;
            }
        }

        public string DepartureDateTime
        {
            get
            {
                return this.departureDateTime;
            }
            set
            {
                this.departureDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int Distance
        {
            get
            {
                return this.distance;
            }
            set
            {
                this.distance = value;
            }
        }

        public MOBComAircraft Equipment
        {
            get
            {
                return this.equipment;
            }
            set
            {
                this.equipment = value;
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

        public int GroundTime
        {
            get
            {
                return this.groundTime;
            }
            set
            {
                this.groundTime = value;
            }
        }

        public string IsChangeOfGauge
        {
            get
            {
                return this.isChangeOfGauge;
            }
            set
            {
                this.isChangeOfGauge = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string IsInternational
        {
            get
            {
                return this.isInternational;
            }
            set
            {
                this.isInternational = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int JourneyDuration
        {
            get
            {
                return this.journeyDuration;
            }
            set
            {
                this.journeyDuration = value;
            }
        }

        public List<MOBSegFlightLeg> Legs
        {
            get
            {
                return this.legs;
            }
            set
            {
                this.legs = value;
            }
        }

        public List<MOBComMessage> Messages
        {
            get
            {
                return this.messages;
            }
            set
            {
                this.messages = value;
            }
        }

        public int NumberOfStops
        {
            get
            {
                return this.numberOfStops;
            }
            set
            {
                this.numberOfStops = value;
            }
        }

        public double OnTimeRate
        {
            get
            {
                return this.onTimeRate;
            }
            set
            {
                this.onTimeRate = value;
            }
        }

        public string OperatingAirlineCode
        {
            get
            {
                return this.operatingAirlineCode;
            }
            set
            {
                this.operatingAirlineCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string OperatingAirlineName
        {
            get
            {
                return this.operatingAirlineName;
            }
            set
            {
                this.operatingAirlineName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int ScheduledFlightDuration
        {
            get
            {
                return this.scheduledFlightDuration;
            }
            set
            {
                this.scheduledFlightDuration = value;
            }
        }

        public int SegmentNumber
        {
            get
            {
                return this.segmentNumber;
            }
            set
            {
                this.segmentNumber = value;
            }
        }

        public List<MOBTMAAirport> StopLocations
        {
            get
            {
                return this.stopLocations;
            }
            set
            {
                this.stopLocations = value;
            }
        }
    }
}
