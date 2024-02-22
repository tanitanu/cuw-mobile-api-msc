using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPOfferPaymentOption
    {
        private string eddCode = string.Empty;
        private List<MOBSHOPOfferPriceComponent> priceComponents;
        private string type = string.Empty;


        public string EddCode
        {
            get
            {
                return this.eddCode;
            }
            set
            {
                this.eddCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBSHOPOfferPriceComponent> PriceComponents
        {
            get
            {
                return this.priceComponents;
            }
            set
            {
                this.priceComponents = value;
            }
        }

        public string Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
