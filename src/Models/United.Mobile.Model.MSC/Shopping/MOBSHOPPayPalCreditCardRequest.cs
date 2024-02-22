using System;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{ 
    [Serializable()]
    public class MOBSHOPPayPalCreditCardRequest : MOBRequest
    {
        private string sessionId;
        private string mPNumber;
        private string customerId;
        private double paymentAmount;
      

        private MOBFormofPayment formOfPaymentType;
        private MOBPayPal payPal;

        public MOBPayPal PayPal
        {
            get { return payPal; }
            set { payPal = value; }
        }


        public MOBFormofPayment FormOfPaymentType
        {
            get { return formOfPaymentType; }
            set { formOfPaymentType = value; }
        }

        public string MPNumber
        {
            get { return mPNumber; }
            set { this.mPNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
           

        public string CustomerId
        {
            get { return customerId; }
            set { this.customerId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string SessionId
        {
            get { return sessionId; }
            set { this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public double PaymentAmount
        {
            get { return paymentAmount; }
            set { paymentAmount = value; }
        }
    }
}
