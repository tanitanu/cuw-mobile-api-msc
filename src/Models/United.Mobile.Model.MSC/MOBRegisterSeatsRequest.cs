using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using United.Mobile.Model.Common;

namespace United.Definition
{
    [Serializable()]
    public class MOBRegisterSeatsRequest : MOBRequest
    {
        private string sessionId;
        private string cartId = string.Empty;
        private string flow = string.Empty;
        private string origin = string.Empty;
        private string destination = string.Empty;
        private string flightNumber = string.Empty;
        private string flightDate = string.Empty;
        private string paxIndex = string.Empty;
        private string seatAssignment = string.Empty;
        private string mileagePlusNumber = string.Empty;
        private string sponsorMPAccountId = string.Empty;
        private string sponsorEliteLevel = string.Empty;
        private bool continueToRegisterAncillary;
        private bool isMiles = false;
        private int accountTotalMilesBalance;
        private List<MOBItem> catalogValues;
        private string hashPinCode;
        private List<MOBSeatDetail> selectedSeats;

        public string HashPinCode
        {
            get { return hashPinCode; }
            set { hashPinCode = value; }
        }


        public List<MOBItem> CatalogValues
        {
            get { return catalogValues; }
            set { catalogValues = value; }
        }

        public bool ContinueToRegisterAncillary
        {
            get { return continueToRegisterAncillary; }
            set { continueToRegisterAncillary = value; }
        }

        public string SessionId
        {
            get { return sessionId; }
            set { sessionId = value; }
        }

        public string CartId
        {
            get { return cartId; }
            set { cartId = value; }
        }
        public string Flow
        {
            get { return flow; }
            set { flow = value; }
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
                this.destination = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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
                this.flightNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
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

        public string PaxIndex
        {
            get
            {
                return this.paxIndex;
            }
            set
            {
                this.paxIndex = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string SeatAssignment
        {
            get
            {
                return this.seatAssignment;
            }
            set
            {
                this.seatAssignment = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string SponsorMPAccountId
        {
            get
            {
                return this.sponsorMPAccountId;
            }
            set
            {
                this.sponsorMPAccountId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string SponsorEliteLevel
        {
            get
            {
                return this.sponsorEliteLevel;
            }
            set
            {
                this.sponsorEliteLevel = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        private bool isOmniCartSavedTripFlow;
        public bool IsOmniCartSavedTripFlow
        {
            get { return isOmniCartSavedTripFlow; }
            set { isOmniCartSavedTripFlow = value; }
        }
        private bool isRemove;

        public bool IsRemove
        {
            get { return isRemove; }
            set { isRemove = value; }
        }

        private bool isDone;
        public bool IsDone
        {
            get { return isDone; }
            set { isDone = value; }
        }

        public bool IsMiles
        {
            get { return isMiles; }
            set { isMiles = value; }
        }

        public int AccountTotalMilesBalance
        {
            get { return accountTotalMilesBalance; }
            set { accountTotalMilesBalance = value; }
        }

        public List<MOBSeatDetail> SelectedSeats
        {
            get { return selectedSeats; }
            set { selectedSeats = value; }
        }
    }

    [Serializable()]
    public class MOBSeatDetail
    {
        private string origin;
        private string destination;
        private string flightNumber;
        private string paxIndex;
        private string seatNumber;
        private string flightDate;

        public string Origin
        {
            get { return origin; }
            set { origin = value; }
        }
        public string Destination
        {
            get { return destination; }
            set { destination = value; }
        }
        public string PaxIndex
        {
            get
            {
                if (paxIndex.Contains(".")) { return paxIndex; }
                else { return paxIndex.Aggregate("", (acc, x) => x == '0' ? acc : acc + x).Insert(1, "."); }
            }
            set
            {
                paxIndex = value;
            }
        }
        public string FlightNumber
        {
            get { return flightNumber; }
            set { flightNumber = value; }
        }
    
        public string SeatNumber
        {
            get { return seatNumber; }
            set { seatNumber = value; }
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
    }
}
