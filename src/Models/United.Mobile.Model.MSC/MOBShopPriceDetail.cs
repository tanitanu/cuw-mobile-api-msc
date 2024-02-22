using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MOBShopPriceDetail
    {
        private double amount;
        private string currency = string.Empty;
        private string detailDescription = string.Empty;
        private string detailType = string.Empty;
        private string priceSubtype = string.Empty;
        private string priceType = string.Empty;

        public double Amount
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

        public string DetailDescription
        {
            get
            {
                return this.detailDescription;
            }
            set
            {
                this.detailDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string DetailType
        {
            get
            {
                return this.detailType;
            }
            set
            {
                this.detailType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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
    }
}
