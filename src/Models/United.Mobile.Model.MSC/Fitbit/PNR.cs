using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Fitbit
{
    [Serializable]
    public class PNR
    {
        private string recordLocator = string.Empty;
        private string dateCreated = string.Empty;
        private string flightDate = string.Empty;
        private Airport origin;
        private Airport destination;
        private string checkInStatus = string.Empty;
        private string numberOfPassengers = string.Empty;
        private string lastSegmentArrivalDate = string.Empty;
        private string expirationDate = string.Empty;
        private string lastUpdated = string.Empty;
        private List<PNRSegment> segments;
        private string farelockExpirationDate = string.Empty;

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

        public string DateCreated
        {
            get { return dateCreated; }
            set { dateCreated = value; }
        }

        public string FlightDate
        {
            get
            {
                return this.flightDate;
            }
            set
            {
                this.flightDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public Airport Origin
        {
            get
            {
                return this.origin;
            }
            set
            {
                this.origin = value;
            }
        }

        public Airport Destination
        {
            get
            {
                return this.destination;
            }
            set
            {
                this.destination = value;
            }
        }

        public string CheckInStatus
        {
            get
            {
                return this.checkInStatus;
            }
            set
            {
                this.checkInStatus = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string NumberOfPassengers
        {
            get
            {
                return this.numberOfPassengers;
            }
            set
            {
                this.numberOfPassengers = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string LastSegmentArrivalDate
        {
            get
            {
                return this.lastSegmentArrivalDate;
            }
            set
            {
                this.lastSegmentArrivalDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ExpirationDate
        {
            get
            {
                return this.expirationDate;
            }
            set
            {
                this.expirationDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        public List<PNRSegment> Segments
        {
            get
            {
                return this.segments;
            }
            set
            {
                this.segments = value;
            }
        }

        public string FarelockExpirationDate
        {
            get
            {
                return this.farelockExpirationDate;
            }
            set
            {
                this.farelockExpirationDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
