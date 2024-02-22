using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Emp.Shopping;

namespace United.Definition.Emp
{
    [Serializable()]
    public class MOBEmpFlightSearchResponse : MOBEmpSHOPShopResponse
    {
        private int tripIndex;
       // private MOBEmpReservation mobEmpReservation;
        private MOBEmpFlightSearchRequest empFlightSearchRequest;
/*Need to delete.  Currently in use*/
        private MOBEmpAvailableRoutes availableRoutes;
        private List<MOBEmpBookingInfo> lstbookingInfo;
/*STOP Deleting.  Currently in use*/
        private List<MOBEmpBookingPassengerExtended> availableTravelers;
        private bool lastTrip;
        private string flightMessage;
        private string eResTransactionId;
        private string eResSessionId;
        private string sessionId;

        private MOBEmpSaveTripRequest mobEmpSaveTripRequest;

        public MOBEmpSaveTripRequest MobEmpSaveTripRequest
        {
            get
            {
                return this.mobEmpSaveTripRequest;
            }
            set
            {
                if (value != null)
                {
                    this.mobEmpSaveTripRequest = value;
                }
            }
        }

        public int TripIndex 
        {
            get { return this.tripIndex; }
            set { this.tripIndex = value; }
        }

        public MOBEmpFlightSearchRequest EmpFlightSearchRequest
        {
            get { return this.empFlightSearchRequest; }
            set { this.empFlightSearchRequest = value; }
        }
/*Need to delete.  Currently in use*/
        public MOBEmpAvailableRoutes AvailableRoutes
        {
            get { return this.availableRoutes; }
            set { this.availableRoutes = value; }
        }
        public List<MOBEmpBookingInfo> BookingInfo
        {
            get { return this.lstbookingInfo; }
            set { this.lstbookingInfo = value; }
        }
/*STOP Deleting*/
        public List<MOBEmpBookingPassengerExtended> AvailableTravelers
        {
            get { return this.availableTravelers; }
            set { this.availableTravelers = value; }
        }
        public bool LastTrip
        {
            get { return this.lastTrip; }
            set { this.lastTrip = value; }
        }

        public string FlightMessage
        {
            get { return this.flightMessage; }
            set { this.flightMessage = value; }
        }
        public string EResTransactionId
        {
            get
            {
                return this.eResTransactionId;
            }
            set
            {
                this.eResTransactionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string EResSessionId
        {
            get
            {
                return this.eResSessionId;
            }
            set
            {
                this.eResSessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string SessionId
        {
            get
            {
                return this.sessionId;
            }
            set
            {
                this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

    }
}
