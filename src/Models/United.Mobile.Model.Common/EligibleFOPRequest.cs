using System;
using System.Collections.Generic;
using System.Text;
using United.Service.Presentation.InteractionModel;

namespace United.Mobile.Model.Common
{
    public class EligibleFOPRequest
    {

        public ShoppingCart ShoppingCart { get; set; }

        public double GrandTotalFareAmount { get; set; }

        public string PointOfSale { get; set; }

        public string Path { get; set; }


        public string PromotionCode { get; set; }


        public string LoyaltyID { get; set; }


        public string CartID { get; set; }


        public string IsCorporateBooking { get; set; }


        public string GrandTotalFareAmountCurrency { get; set; }
    }
}
