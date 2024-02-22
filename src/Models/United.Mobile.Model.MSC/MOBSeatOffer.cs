using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable]
    public class MOBSeatOffer
    {
        private string offerTitle = string.Empty;
        private decimal price;
        private string currencyCode = string.Empty;
        private string offerText1 = string.Empty;
        private string offerText2 = string.Empty;
        private string offerText3 = string.Empty;
        private bool isAdvanceSeatOffer;

        public Int32 Miles { get; set; }
        public string DisplayMiles { get; set; }
        public string OfferTitle
        {
            get
            {
                return this.offerTitle;
            }
            set
            {
                this.offerTitle = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public decimal Price
        {
            get
            {
                return this.price;
            }
            set
            {
                this.price = value;
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

        public string OfferText1
        {
            get
            {
                return this.offerText1;
            }
            set
            {
                this.offerText1 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string OfferText2
        {
            get
            {
                return this.offerText2;
            }
            set
            {
                this.offerText2 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string OfferText3
        {
            get
            {
                return this.offerText3;
            }
            set
            {
                this.offerText3 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool IsAdvanceSeatOffer
        {
            get { return isAdvanceSeatOffer; }
            set { isAdvanceSeatOffer = value; }
        }


    }
}
