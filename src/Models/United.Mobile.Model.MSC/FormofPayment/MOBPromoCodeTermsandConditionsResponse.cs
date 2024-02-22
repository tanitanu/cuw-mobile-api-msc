using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;

namespace United.Definition.FormofPayment
{
    [Serializable()]
    public class MOBPromoCodeTermsandConditionsResponse: MOBShoppingResponse
    {
        private MOBMobileCMSContentMessages termsandConditions;

        public MOBMobileCMSContentMessages TermsandConditions
        {
            get { return termsandConditions; }
            set { termsandConditions = value; }
        }

    }
}
