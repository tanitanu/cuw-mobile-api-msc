using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBInternationalBilling
    {
        private List<MOBCPBillingCountry> billingAddressProperties;
        public List<MOBCPBillingCountry> BillingAddressProperties
        {
            get
            {
                return this.billingAddressProperties;
            }
            set
            {
                this.billingAddressProperties = value;
            }
        }
    }

}
