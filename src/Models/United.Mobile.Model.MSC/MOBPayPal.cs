using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBPayPal
    {
        private string currencyCode;
        private string payerID;
        private string payPalTokenID;
        private string billingAddressCountryCode;

        public string CurrencyCode
        {
            get { return currencyCode; }
            set { currencyCode = value; }
        }
        public string PayPalTokenID
        {
            get { return payPalTokenID; }
            set { payPalTokenID = value; }
        }
        public string PayerID
        {
            get { return payerID; }
            set { payerID = value; }
        }
        public string BillingAddressCountryCode
        {
            get { return billingAddressCountryCode; }
            set { billingAddressCountryCode = value; }
        }
    }

}
