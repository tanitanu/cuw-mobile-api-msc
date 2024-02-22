using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPOfferPrice
    {
        private string id = string.Empty;
        private MOBSHOPOfferPriceAssociation association;
        private List<MOBSHOPOfferPaymentOption> paymentOptions;

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
