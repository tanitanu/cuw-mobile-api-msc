using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Fitbit
{
    [Serializable]
    public class FlightStatusSegment : Segment
    {
        private string scheduledFlightTime = string.Empty;
        private string actualFlightTime = string.Empty;
        private string estimatedDepartureDateTime = string.Empty;
        private string estimatedArrivalDateTime = string.Empty;
        private string actualDepartureDateTime = string.Empty;
        private string actualArrivalDateTime = string.Empty;
        private string departureTerminal = string.Empty;
        private string arrivalTerminal = string.Empty;
        private string departureGate = string.Empty;
        private string arrivalGate = string.Empty;
        private Equipment ship;
        private Airline operatingCarrier;
        private Airline codeShareCarrier;
        private string status = string.Empty;
        private string codeShareflightNumber = string.Empty;
        private bool isSegmentCancelled;
        private bool isWiFiAvailable;
        private string lastUpdatedGMT = string.Empty;
        private string statusShort = string.Empty;

        public FlightStatusSegment()
            : base()
        {
        }

        public string ScheduledFlightTime
        {
            get
            {
                return this.scheduledFlightTime;
            }
            set
            {
                this.scheduledFlightTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ActualFlightTime
        {
            get
            {
                return this.actualFlightTime;
            }
            set
            {
                this.actualFlightTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string EstimatedDepartureDateTime
        {
            get
            {
                return this.estimatedDepartureDateTime;
            }
            set
            {
                this.estimatedDepartureDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string EstimatedArrivalDateTime
        {
            get
            {
                return this.estimatedArrivalDateTime;
            }
            set
            {
                this.estimatedArrivalDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ActualDepartureDateTime
        {
            get
            {
                return this.actualDepartureDateTime;
            }
            set
            {
                this.actualDepartureDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ActualArrivalDateTime
        {
            get
            {
                return this.actualArrivalDateTime;
            }
            set
            {
                this.actualArrivalDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string DepartureTerminal
        {
            get
            {
                return this.departureTerminal;
            }
            set
            {
                this.departureTerminal = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ArrivalTerminal
        {
            get
            {
                return this.arrivalTerminal;
            }
            set
            {
                this.arrivalTerminal = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string DepartureGate
        {
            get
            {
                return this.departureGate;
            }
            set
            {
                this.departureGate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string ArrivalGate
        {
            get
            {
                return this.arrivalGate;
            }
            set
            {
                this.arrivalGate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public Equipment Ship
        {
            get
            {
                return this.ship;
            }
            set
            {
                this.ship = value;
            }
        }

        public Airline OperatingCarrier
        {
            get
            {
                return this.operatingCarrier;
            }
            set
            {
                this.operatingCarrier = value;
            }
        }

        public Airline CodeShareCarrier
        {
            get
            {
                return this.codeShareCarrier;
            }
            set
            {
                this.codeShareCarrier = value;
            }
        }

        public string Status
        {
            get
            {
                return this.status;
            }
            set
            {
                this.status = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string CodeShareflightNumber
        {
            get
            {
                return this.codeShareflightNumber;
            }
            set
            {
                this.codeShareflightNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool IsSegmentCancelled
        {
            get
            {
                return isSegmentCancelled;
            }
            set
            {
                this.isSegmentCancelled = value;
            }
        }

        public bool ISWiFiAvailable
        {
            get
            {
                return isWiFiAvailable;
            }
            set
            {
                this.isWiFiAvailable = value;
            }
        }

        public string LastUpdatedGMT
        {
            get
            {
                return this.lastUpdatedGMT;
            }
            set
            {
                this.lastUpdatedGMT = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string StatusShort
        {
            get
            {
                return this.statusShort;
            }
            set
            {
                this.statusShort = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
