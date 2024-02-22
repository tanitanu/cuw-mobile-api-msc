using System;
using System.Collections.Generic;
using System.Linq;
using United.Mobile.Model.Common;

namespace United.Definition
{
    [Serializable()]
    public class MOBRegisterCheckinSeatsRequest : MOBShoppingRequest
    {

        private string hashPinCode;

        public string HashPinCode
        {
            get { return hashPinCode; }
            set { hashPinCode = value; }
        }

        private List<MOBCheckinSeatDetail> lastSeatSelected;

        public List<MOBCheckinSeatDetail> LastSeatSelected
        {
            get { return lastSeatSelected; }
            set { lastSeatSelected = value; }
        }

        private List<MOBCheckinSeatDetail> selectedSeats;
        public List<MOBCheckinSeatDetail> SelectedSeats
        {
            get { return selectedSeats; }
            set { selectedSeats = value; }
        }
        private bool isMiles = false;
        public bool IsMiles
        {
            get { return isMiles; }
            set { isMiles = value; }
        }

        private string mileagePlusNumber = string.Empty;
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


        private List<MOBItem> catalogValues;
        public List<MOBItem> CatalogValues
        {
            get { return catalogValues; }
            set { catalogValues = value; }
        }
    }

    [Serializable()]
    public class MOBCheckinSeatDetail
    {
        private string legId;
        private string origin;
        private string destination;
        private string flightNumber;
        private string customerId;
        private decimal seatPrice;
        private string seatNumber;
        private string seatType;
        private string seatPromotionCode;
        private int milesFee;
        public string LegId
        {
            get { return legId; }
            set { legId = value; }
        }

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
        public string CustomerId
        {
            get {
                if (customerId.Contains(".")) { return customerId; }
                else { return customerId.Aggregate("", (acc, x) => x == '0' ? acc : acc + x).Insert(1, "."); }
            }
            set
            {
                customerId = value;
            }
        }
        public string FlightNumber
        {
            get { return flightNumber; }
            set { flightNumber = value; }
        }
        public decimal SeatPrice
        {
            get { return seatPrice; }
            set { seatPrice = value; }
        }
        public string SeatNumber
        {
            get { return seatNumber; }
            set { seatNumber = value; }
        }

        public string SeatType
        {
            get { return seatType; }
            set { seatType = value; }
        }
        public string SeatPromotionCode
        {
            get { return seatPromotionCode; }
            set { seatPromotionCode = value; }
        }

        public int MilesFee
        {
            get { return milesFee; }
            set { milesFee = value; }
        }
    }    
}
