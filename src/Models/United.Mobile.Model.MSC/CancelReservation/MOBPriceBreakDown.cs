using System;
using System.Collections.Generic;

namespace United.Definition.CancelReservation
{
    [Serializable]
    public class MOBPriceBreakDown
    {
        private MOBBasePrice basePrice;
        private List<MOBBasePrice> taxes;
        private List<string> passengerTypeCode;
        private int travelerCount;
        private MOBBasePrice totalPrice;

        public int TravelerCount
        {
            get { return travelerCount; }
            set { travelerCount = value; }
        }

        public List<string> PassengerTypeCode
        {
            get { return passengerTypeCode; }
            set { passengerTypeCode = value; }
        }

        public MOBBasePrice BasePrice
        {
            get { return basePrice; }
            set { basePrice = value; }
        }

        public MOBBasePrice TotalPrice
        {
            get { return totalPrice; }
            set { totalPrice = value; }
        }

        public List<MOBBasePrice> Taxes
        {
            get { return taxes; }
            set { taxes = value; }
        }

        private List<MOBBasePrice> fees;
        public List<MOBBasePrice> Fees
        {
            get { return fees; }
            set { fees = value; }
        }
    }
}
