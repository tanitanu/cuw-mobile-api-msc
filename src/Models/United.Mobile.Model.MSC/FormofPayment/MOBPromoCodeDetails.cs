using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.FormofPayment
{
    [Serializable()]
    public class MOBPromoCodeDetails
    {
        private List<MOBPromoCode> promoCodes;

        public List<MOBPromoCode> PromoCodes
        {
            get { return promoCodes; }
            set { promoCodes = value ?? new List<MOBPromoCode>(); }
        }
        private bool isDisablePromoOption;

        public bool IsDisablePromoOption
        {
            get { return isDisablePromoOption; }
            set { isDisablePromoOption = value; }
        }
        private bool isHidePromoOption;

        public bool IsHidePromoOption
        {
            get { return isHidePromoOption; }
            set { isHidePromoOption = value; }
        }
     

    }
}
