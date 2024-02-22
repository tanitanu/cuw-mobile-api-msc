using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPOfferPriceComponent
    {
        private Dictionary<string, List<String>> descriptions;
        private Dictionary<string, List<String>> disclaimers;
        private MOBSHOPOfferDisplayPrice price;

        public Dictionary<string, List<String>> Descriptions
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

        public Dictionary<string, List<String>> Disclaimers
        {
            get
            {
                return this.disclaimers;
            }
            set
            {
                this.disclaimers = value;
            }
        }

        public MOBSHOPOfferDisplayPrice Price
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
    }
}
