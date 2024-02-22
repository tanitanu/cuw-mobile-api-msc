using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.FormofPayment
{
    public class MOBApplyPromoCodeRequest : MOBShoppingRequest
    {
        private List<MOBPromoCode> promoCodes;
        public List<MOBPromoCode> PromoCodes
        {
            get { return promoCodes; }
            set { promoCodes = value; }
        }

        private bool continueToResetMoneyMiles;
        public bool ContinueToResetMoneyMiles
        {
            get { return continueToResetMoneyMiles; }
            set { continueToResetMoneyMiles = value; }
        }
    }
}
