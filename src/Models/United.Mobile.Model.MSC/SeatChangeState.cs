using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition;
using United.Definition.Booking;

namespace United.Persist.Definition.SeatChange
{
    public class SeatChangeState 
    {
        public SeatChangeState() { }
        

        #region IPersist Members
        private string objectName = "United.Persist.Definition.SeatChange.SeatChangeState";

        public string ObjectName
        {

            get
            {
                return this.objectName;
            }
            set
            {
                this.objectName = value;
            }
        }

        #endregion

        private string recordLocator = string.Empty;
        private string lastName = string.Empty;
        private string pnrCreationDate = string.Empty;
        private string sessionId = string.Empty;
        private List<MOBBKTraveler> bookingTravlerInfo;
        private List<MOBBKTrip> trips;
        private List<MOBSeat> seats;
        private List<MOBSeatPrice> seatPrices;
        private List<MOBCreditCard> creditCards;
        private List<MOBAddress> profileOwnerAddresses;
        private List<MOBEmail> emails;
        private int totalEplusEligible;
        private List<MOBTripSegment> segments;
      
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

        public string LastName
        {
            get
            {
                return this.lastName;
            }
            set
            {
                this.lastName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string PNRCreationDate
        {
            get
            {
                return this.pnrCreationDate;
            }
            set
            {
                this.pnrCreationDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        public List<MOBBKTraveler> BookingTravelerInfo
        {
            get { return this.bookingTravlerInfo; }
            set { this.bookingTravlerInfo = value; }
        }

        public List<MOBBKTrip> Trips
        {
            get { return this.trips; }
            set { this.trips = value; }
        }

        public List<MOBSeat> Seats
        {
            get { return this.seats; }
            set { this.seats = value; }
        }

        public List<MOBSeatPrice> SeatPrices
        {
            get { return this.seatPrices; }
            set { this.seatPrices = value; }
        }

        public List<MOBCreditCard> CreditCards
        {
            get { return this.creditCards; }
            set { this.creditCards = value; }
        }

        public List<MOBAddress> ProfileOwnerAddresses
        {
            get { return this.profileOwnerAddresses; }
            set { this.profileOwnerAddresses = value; }
        }

        public List<MOBEmail> Emails
        {
            get { return this.emails; }
            set { this.emails = value; }
        }

        public int TotalEplusEligible
        {
            get { return totalEplusEligible; }
            set { totalEplusEligible = value; }
        }

        public List<MOBTripSegment> Segments
        {
            get { return segments; }
            set { segments = value; }
        }
    }
}
