using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPOfferDisplayPrice
    {
        private List<MOBSHOPOfferDisplayPriceSubItem> adjustments;
        private MOBSHOPOfferDisplayPriceSubItem basePrice;
        private List<MOBSHOPOfferDisplayPriceSubItem> fees;
        private List<MOBSHOPOfferDisplayPriceSubItem> totals;
        private string fareType = string.Empty;


        public List<MOBSHOPOfferDisplayPriceSubItem> Adjustments
        {
            get
            {
                return this.adjustments;
            }
            set
            {
                this.adjustments = value;
            }
        }

        public MOBSHOPOfferDisplayPriceSubItem BasePrice
        {
            get
            {
                return this.basePrice;
            }
            set
            {
                this.basePrice = value;
            }
        }

        public List<MOBSHOPOfferDisplayPriceSubItem> Fees
        {
            get
            {
                return this.fees;
            }
            set
            {
                this.fees = value;
            }
        }

        public List<MOBSHOPOfferDisplayPriceSubItem> Totals
        {
            get
            {
                return this.totals;
            }
            set
            {
                this.totals = value;
            }
        }

        public string FareType
        {
            get
            {
                return this.fareType;
            }
            set
            {
                this.fareType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
