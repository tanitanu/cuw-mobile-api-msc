using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable]
   public class MOBSHOPPersistVisaCheckoutDetailsRequest:MOBRequest
    {
        private string sessionId;
        private string mPNumber;
        private string customerId;
        private MOBVisaCheckout visaCheckOutDetails;
        private MOBFormofPayment formOfPaymentType;


        public MOBVisaCheckout VisaCheckOutDetails
        {
            get { return visaCheckOutDetails; }
            set
            {
                visaCheckOutDetails = value;
            }
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

       
    }
}
