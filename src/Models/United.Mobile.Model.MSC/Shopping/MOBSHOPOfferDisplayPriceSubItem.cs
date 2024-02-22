using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPOfferDisplayPriceSubItem
    {
        private string amount = string.Empty;
        private string currencyCode = string.Empty;
        private string type = string.Empty;

        public string Amount
        {
            get
            {
                return this.amount;
            }
            set
            {
                this.amount = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string CurrencyCode
        {
            get
            {
                return this.currencyCode;
            }
            set
            {
                this.currencyCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
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
