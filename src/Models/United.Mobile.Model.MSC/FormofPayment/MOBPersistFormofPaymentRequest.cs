using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBPersistFormofPaymentRequest : MOBShoppingRequest
    {
        private string amount;
        private MOBFormofPaymentDetails formofPaymentDetails;
        private bool isCCSelectedForContactless;

        public bool IsCCSelectedForContactless
        {
            get { return isCCSelectedForContactless; }
            set { isCCSelectedForContactless = value; }
        }


        public string Amount
        {
            get { return amount; }
            set { amount = value; }
        }
        public MOBFormofPaymentDetails FormofPaymentDetails
        {
            get { return formofPaymentDetails; }
            set { formofPaymentDetails = value; }
        }
    }
}
