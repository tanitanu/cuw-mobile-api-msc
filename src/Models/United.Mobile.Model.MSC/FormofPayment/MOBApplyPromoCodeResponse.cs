using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;

namespace United.Definition.FormofPayment
{
    [Serializable()]
    public class MOBApplyPromoCodeResponse : MOBShoppingResponse
    {
        [JsonIgnore()]
        public string ObjectName { get; set; } = "United.Definition.FormofPayment.MOBApplyPromoCodeResponse";
        private MOBShoppingCart shoppingCart = new MOBShoppingCart();
        private MOBSHOPReservation reservation;
        private List<FormofPaymentOption> eligibleFormofPayments;
        private string maxCountMessage;
        private MOBSection promoCodeRemovalAlertMessage;

        public MOBSection PromoCodeRemovalAlertMessage
        {
            get { return promoCodeRemovalAlertMessage; }
            set { promoCodeRemovalAlertMessage = value; }
        }

        public string MaxCountMessage
        {
            get { return maxCountMessage; }
            set { maxCountMessage = value; }
        }

        public MOBShoppingCart ShoppingCart
        {
            get { return shoppingCart; }
            set { shoppingCart = value; }
        }
        public MOBSHOPReservation Reservation
        {
            get { return reservation; }
            set { this.reservation = value; }
        }
        public List<FormofPaymentOption> EligibleFormofPayments
        {
            get { return eligibleFormofPayments; }
            set { eligibleFormofPayments = value; }
        }
    }
}
