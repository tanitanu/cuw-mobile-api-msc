using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MOBShopPricingItem
    {
        private decimal amount;
        private string currency = string.Empty;
        private List<MOBShopPriceDetail> pricingDetails;
        private string pricingType = string.Empty;

        public decimal Amount
        {
            get
            {
                return this.amount;
            }
            set
            {
                this.amount = value;
            }
        }

        public string Currency
        {
            get
            {
                return this.currency;
            }
            set
            {
                this.currency = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public List<MOBShopPriceDetail> PricingDetails
        {
            get
            {
                return this.pricingDetails;
            }
            set
            {
                this.pricingDetails = value;
            }
        }

        public string PricingType
        {
            get
            {
                return this.pricingType;
            }
            set
            {
                this.pricingType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
