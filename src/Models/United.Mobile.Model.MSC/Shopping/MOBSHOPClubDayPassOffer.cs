using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPClubDayPassOffer
    {
        private string offerDescriptionHeader = string.Empty;
        private string offerDescription = string.Empty;
        private List<MOBSHOPPrice> prices;
        private List<string> descriptions;
        private List<string> termsAndConditions;

        public string OfferDescriptionHeader
        {
            get
            {
                return this.offerDescriptionHeader;
            }
            set
            {
                this.offerDescriptionHeader = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string OfferDescription
        {
            get
            {
                return this.offerDescription;
            }
            set
            {
                this.offerDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBSHOPPrice> Prices
        {
            get
            {
                return this.prices;
            }
            set
            {
                this.prices = value;
            }
        }

        public List<string> Descriptions
        {
            get
            {
                return this.descriptions;
            }
            set
            {
                this.descriptions = value;
            }
        }

        public List<string> TermsAndConditions
        {
            get
            {
                return this.termsAndConditions;
            }
            set
            {
                this.termsAndConditions = value;
            }
        }
    }
}
