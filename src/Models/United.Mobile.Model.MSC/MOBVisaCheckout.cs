using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable]
    public class MOBVisaCheckout
    {
        private MOBCreditCard visaCheckoutCard;
        private string visaCheckOutCallID;

        public string VisaCheckOutCallID
        {
            get { return visaCheckOutCallID; }
            set { visaCheckOutCallID = value; }
        }

        public MOBCreditCard VisaCheckoutCard
        {
            get { return visaCheckoutCard; }
            set { visaCheckoutCard = value; }
        }
    }
}
