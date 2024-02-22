using System;

namespace United.Definition.Fitbit
{
    [Serializable]
    public class EBP
    {
        private string flightNumber = string.Empty;
        private string flightDate = string.Empty;
        private Airport departure;
        private Airport arrival;
        private string departureDate = string.Empty;
        private string arrivalDate = string.Empty;
        private string departureTime = string.Empty;
        private string arrivalTime = string.Empty;
        private string boardGate = string.Empty;
        private string boardStartTime = string.Empty;
        private string boardEndTime = string.Empty;
        private string boardGroup = string.Empty;
        private string passengerName = string.Empty;
        private string seat = string.Empty;
        private string sequenceNumber = string.Empty;
        private string cos = string.Empty;
        private string mileagePlusNumber = string.Empty;
        private string mileagePlusAccountStatus = string.Empty;
        private Barcode barcode;

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
        public string DepartureDate
        {
            get
            {
                return this.departureDate;
            }
            set
            {
                this.departureDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string ArrivalDate
        {
            get
            {
                return this.arrivalDate;
            }
            set
            {
                this.arrivalDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string DepartureTime
        {
            get
            {
                return this.departureTime;
            }
            set
            {
                this.departureTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string ArrivalTime
        {
            get
            {
                return this.arrivalTime;
            }
            set
            {
                this.arrivalTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string BoardGate
        {
            get
            {
                return this.boardGate;
            }
            set
            {
                this.boardGate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string BoardStartTime
        {
            get
            {
                return this.boardStartTime;
            }
            set
            {
                this.boardStartTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string BoardEndTime
        {
            get
            {
                return this.boardEndTime;
            }
            set
            {
                this.boardEndTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string BoardGroup
        {
            get
            {
                return this.boardGroup;
            }
            set
            {
                this.boardGroup = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string PassengerName
        {
            get
            {
                return this.passengerName;
            }
            set
            {
                this.passengerName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string Seat
        {
            get
            {
                return this.seat;
            }
            set
            {
                this.seat = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string SequenceNumber
        {
            get
            {
                return this.sequenceNumber;
            }
            set
            {
                this.sequenceNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string COS
        {
            get
            {
                return this.cos;
            }
            set
            {
                this.cos = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string MileagePlusNumber
        {
            get
            {
                return this.mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
        public string MileagePlusAccountStatus
        {
            get
            {
                return this.mileagePlusAccountStatus;
            }
            set
            {
                this.mileagePlusAccountStatus = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public Barcode Barcode
        {
            get
            {
                return this.barcode;
            }
            set
            {
                this.barcode = value;
            }
        }

    }
}