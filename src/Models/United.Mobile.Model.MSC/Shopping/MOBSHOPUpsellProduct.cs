using System;
using System.Collections.Generic;


namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPUpsellProduct
    {
        #region Properties
        private string bookingCode = string.Empty;
        private string cabinType = string.Empty;
        private string longCabin = string.Empty;
        private string solutionId = string.Empty;
        private string lastSolutionId = string.Empty;
        private string productSubtype = string.Empty;
        private string productType = string.Empty;
        private List<string> prices = new List<string>();
        private int numberOfPassengers = 0;
        private decimal totalPrice;

        public string BookingCode
        {
            get { return this.bookingCode; }
            set { this.bookingCode = value; }
        }

        public decimal TotalPrice
        {
            get { return totalPrice; }
            set { totalPrice = value; }
        }

        public string CabinType
        {
            get { return this.cabinType; }
            set { this.cabinType = value; }
        }

        
        public string SolutionId
        {
            get { return this.solutionId; }
            set { this.solutionId = value; }
        }
        
        public string ProductSubtype
        {
            get { return this.productSubtype; }
            set { this.productSubtype = value; }
        }

        
        public string ProductType
        {
            get { return this.productType; }
            set { this.productType = value; }
        }

        public string LastSolutionId
        {
            get { return this.lastSolutionId; }
            set { this.lastSolutionId = value; }
        }
        
        public List<string> Prices
        {
            get { return this.prices; }
            set { this.prices = value; }
        }

        
        public int NumberOfPassengers
        {
            get { return this.numberOfPassengers; }
            set { this.numberOfPassengers = value; }
        }
        public string LongCabin
        {
            get { return longCabin; }
            set { longCabin = value; }
        }


        #endregion
    }
}
