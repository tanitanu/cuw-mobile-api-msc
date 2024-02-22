using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPProductOffer
    {
        private List<MOBProductOffersFlight> productOffersPerSegment;
        private List<MOBSHOPTnC> tnCs;
        private List<MOBOfferBenefit> benefits;

        public List<MOBProductOffersFlight> ProductOffersPerSegment
        {
            get
            {
                return this.productOffersPerSegment;
            }
            set
            {
                this.productOffersPerSegment = value;
            }
        }

        public List<MOBSHOPTnC> TnCs
        {
            get
            {
                return this.tnCs;
            }
            set
            {
                this.tnCs = value;
            }
        }

        public List<MOBOfferBenefit> Benefits
        {
            get
            {
                return this.benefits;
            }
            set
            {
                this.benefits = value;
            }
        }
    }
}
