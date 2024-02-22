using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp
{
    [Serializable()]
    public class MOBEmpStandByListRequest : MOBEmpRequest
    {
        private string flightNumber;
        private string flightDate;
        private string departure;
        private string equipment;
        private bool showPosition;
        private string destination;
        private string employeeId;
        private bool isGetPBT;
        private string hashPinCode;
        private string mileagePlusNumber;
        private string sessionId;
        private string carrierCode;
        private string operatingCarrierCode;


        public string OperatingCarrierCode
        {
            get { return this.operatingCarrierCode; }
            set { this.operatingCarrierCode = value; }
        }
        public string CarrierCode
        {
            get { return this.carrierCode; }
            set { this.carrierCode = value; }
        }
        public string SessionId 
        {
            get { return this.sessionId; }
            set { this.sessionId = value; }
        }

        public string Equipment
        {
            get { return this.equipment; }
            set { this.equipment = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string MileagePlusNumber
        {
            get { return mileagePlusNumber; }
            set { this.mileagePlusNumber = value; }
        }

        public string HashPinCode
        {
            get { return hashPinCode; }
            set { this.hashPinCode = value; }
        }

        public bool IsGetPBT
        {
            get { return isGetPBT; }
            set { this.isGetPBT = value; }
        }

        public string FlightNumber
        {
            get { return this.flightNumber; }
            set { this.flightNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string FlightDate
        {
            get { return this.flightDate; }
            set { this.flightDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string Departure
        {
            get { return this.departure; }
            set { this.departure = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string Destination
        {
            get { return destination; }
            set { this.destination = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public bool ShowPosition
        {
            get { return showPosition; }
            set { showPosition = value; }
        }

        public string EmployeeID
        {
            get { return employeeId; }
            set { employeeId = value; }
        }
    }
}
