using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPProductOffersPrice
    {
        private string offerID = string.Empty;
        private string offerDescriptionHeader = string.Empty;
        private string offerDescription = string.Empty;

        private string productCode = string.Empty;
        private string productDescription = string.Empty;
        private string productDisplayName = string.Empty;

        private string subproductID = string.Empty;
        private string subproductName = string.Empty;
        private string subproductCode = string.Empty;
        private List<string> subproductDescriptions;
        private string subproductGroupCode = string.Empty;
        private string subproductSubGroupCode = string.Empty;

        private string priceID = string.Empty;
        private List<string> priceIds;
        private MOBSHOPOfferPriceAssociation association;
        private List<MOBSHOPOfferPaymentOption> paymentOptions;

        public string OfferID
        {
            get
            {
                return this.offerID;
            }
            set
            {
                this.offerID = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        public string ProductCode
        {
            get
            {
                return this.productCode;
            }
            set
            {
                this.productCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string ProductDescription
        {
            get
            {
                return this.productDescription;
            }
            set
            {
                this.productDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string ProductDisplayName
        {
            get
            {
                return this.productDisplayName;
            }
            set
            {
                this.productDisplayName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string SubproductID
        {
            get
            {
                return this.subproductID;
            }
            set
            {
                this.subproductID = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string SubproductName
        {
            get
            {
                return this.subproductName;
            }
            set
            {
                this.subproductName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string SubproductCode
        {
            get
            {
                return this.subproductCode;
            }
            set
            {
                this.subproductCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<string> SubproductDescriptions
        {
            get
            {
                return this.subproductDescriptions;
            }
            set
            {
                this.subproductDescriptions = value;
            }
        }

        public string SubproductGroupCode
        {
            get
            {
                return this.subproductGroupCode;
            }
            set
            {
                this.subproductGroupCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string SubproductSubGroupCode
        {
            get
            {
                return this.subproductSubGroupCode;
            }
            set
            {
                this.subproductSubGroupCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string PriceID
        {
            get
            {
                return this.priceID;
            }
            set
            {
                this.priceID = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<string> PriceIds
        {
            get
            {
                return this.priceIds;
            }
            set
            {
                this.priceIds = value;
            }
        }

        public MOBSHOPOfferPriceAssociation Association
        {
            get
            {
                return this.association;
            }
            set
            {
                this.association = value;
            }
        }
        public List<MOBSHOPOfferPaymentOption> PaymentOptions
        {
            get
            {
                return this.paymentOptions;
            }
            set
            {
                this.paymentOptions = value;
            }
        }

    }
}
