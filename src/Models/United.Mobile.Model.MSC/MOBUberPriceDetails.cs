using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBUberPriceDetails
    {
        private string localized_display_name;
        private string estimate;
        private string display_name;
        private string id;
        private string product_id;
        private string surge_multiplier;

        public string Localized_display_name
        {
            get
            {
                return this.localized_display_name;
            }
            set
            {
                this.localized_display_name = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Estimate
        {
            get
            {
                return this.estimate;
            }
            set
            {
                this.estimate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Display_name
        {
            get
            {
                return this.display_name;
            }
            set
            {
                this.display_name = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string ID
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string Product_ID
        {
            get
            {
                return this.product_id;
            }
            set
            {
                this.product_id = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string Surge_multiplier
        {
            get
            {
                return this.surge_multiplier;
            }
            set
            {
                this.surge_multiplier = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

    }
}
