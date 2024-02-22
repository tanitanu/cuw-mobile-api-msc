using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Fitbit
{
    [Serializable]
    public class Segment
    {
        private Airline marketingCarrier;
        private string flightNumber = string.Empty;
        private Airport departure;
        private Airport arrival;
        private string scheduledDepartureDateTime = string.Empty;
        private string scheduledArrivalDateTime = string.Empty;
        private string scheduledDepartureTimeGMT = string.Empty;
        private string scheduledArrivalTimeGMT = string.Empty;

        public Segment()
        {
        }

        public Airline MarketingCarrier
        {
            get
            {
                return this.marketingCarrier;
            }
            set
            {
                this.marketingCarrier = value;
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

        public Airport Departure
        {
            get
            {
                return this.departure;
            }
            set
            {
                this.departure = value;
            }
        }

        public Airport Arrival
        {
            get
            {
                return this.arrival;
            }
            set
            {
                this.arrival = value;
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

        public string ScheduledDepartureTimeGMT
        {
            get
            {
                return this.scheduledDepartureTimeGMT;
            }
            set
            {
                this.scheduledDepartureTimeGMT = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ScheduledArrivalTimeGMT
        {
            get
            {
                return this.scheduledArrivalTimeGMT;
            }
            set
            {
                this.scheduledArrivalTimeGMT = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
