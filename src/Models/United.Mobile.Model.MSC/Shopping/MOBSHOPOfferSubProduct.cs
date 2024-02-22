using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPOfferSubProduct
    {
        private string id = string.Empty;
        private string name = string.Empty;
        private string code = string.Empty;
        private List<string> descriptions;
        private string groupCode = string.Empty;
        private string subGroupCode = string.Empty;
        private List<MOBSHOPOfferFeature> features;
        private List<MOBSHOPOfferPrice> prices;

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

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Code
        {
            get
            {
                return this.code;
            }
            set
            {
                this.code = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        public string GroupCode
        {
            get
            {
                return this.groupCode;
            }
            set
            {
                this.groupCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string SubGroupCode
        {
            get
            {
                return this.subGroupCode;
            }
            set
            {
                this.subGroupCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBSHOPOfferPrice> Prices
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
    }
}
