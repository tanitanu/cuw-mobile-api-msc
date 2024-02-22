using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBSHOPPersistMasterpassDetailsRequest : MOBRequest
    {
        private string sessionId;
        private string mPNumber;
        private string customerId;
        private double paymentAmount;

        private MOBMasterpass masterpass; 
        private MOBFormofPayment formOfPaymentType;
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

        public MOBFormofPayment FormOfPaymentType
        {
            get { return formOfPaymentType; }
            set { formOfPaymentType = value; }
        }

        public MOBMasterpass Masterpass
        {
            get { return masterpass; }
            set { masterpass = value; }
        }


    }
}
