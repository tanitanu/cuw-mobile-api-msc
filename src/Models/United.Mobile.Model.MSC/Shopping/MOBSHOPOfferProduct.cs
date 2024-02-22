using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPOfferProduct
    {
        private string code = string.Empty;
        private string description = string.Empty;
        private string displayName = string.Empty;
        private List<MOBSHOPOfferSubProduct> subProducts;

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

        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string DisplayName
        {
            get
            {
                return this.displayName;
            }
            set
            {
                this.displayName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBSHOPOfferSubProduct> SubProducts
        {
            get
            {
                return this.subProducts;
            }
            set
            {
                this.subProducts = value;
            }
        }
    }
}
