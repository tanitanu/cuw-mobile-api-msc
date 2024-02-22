using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;

namespace United.Definition
{
    [Serializable()]
    //v788383: 11/02/2018 - Generic Method for Token generation
    public class MOBFOPAcquirePaymentTokenRequest : MOBShoppingRequest
    {
        private string countryCode = string.Empty;
        private string amount;
        private string formofPaymentCode = string.Empty;
        
        public string Amount {
            get { return amount; }
            set { amount = value; }
        }
        public string FormofPaymentCode {
            get { return formofPaymentCode; }
            set { formofPaymentCode = value; }
        }

        public string CountryCode
        {
            get { return countryCode; }
            set { countryCode = value; }
        }
    }
}
