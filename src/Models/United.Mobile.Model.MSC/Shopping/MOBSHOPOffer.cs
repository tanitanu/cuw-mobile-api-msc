using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPOffer
    {
        private string id = string.Empty;
        private string offerDescriptionHeader = string.Empty;
        private string offerDescription = string.Empty;
        private List<MOBSHOPOfferProduct> products;

        public string Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

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

        public List<MOBSHOPOfferProduct> Products
        {
            get
            {
                return this.products;
            }
            set
            {
                this.products = value;
            }
        }
    }
}
