using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MOBShopPrice
    {
        private string cellId = string.Empty;
        private string description = string.Empty;
        private string priceSubtype = string.Empty;
        private string priceType = string.Empty;
        private List<MOBShopPricingItem> pricingItems;
        private string solutionUrl = string.Empty;

        public string CellId
        {
            get
            {
                return this.cellId;
            }
            set
            {
                this.cellId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string PriceSubtype
        {
            get
            {
                return this.priceSubtype;
            }
            set
            {
                this.priceSubtype = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string PriceType
        {
            get
            {
                return this.priceType;
            }
            set
            {
                this.priceType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBShopPricingItem> PricingItems
        {
            get
            {
                return this.pricingItems;
            }
            set
            {
                this.pricingItems = value;
            }
        }

        public string SolutionUrl
        {
            get
            {
                return this.solutionUrl;
            }
            set
            {
                this.solutionUrl = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
