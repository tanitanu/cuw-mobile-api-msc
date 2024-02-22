using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBUberProductTimePriceEstimates
    {
        private int id;
        private string localized_display_name;
        private string estimateTime;
        private string display_name;
        private string estimatePriceRange;
        private MOBUberProductDetails product;
        private int estimatedPriceToSort;
        private bool surgeIndicator;

        public int ID
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

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

        public string EstimateTime
        {
            get
            {
                return this.estimateTime;
            }
            set
            {
                this.estimateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        public string EstimatePriceRange
        {
            get
            {
                return this.estimatePriceRange;
            }
            set
            {
                this.estimatePriceRange = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public MOBUberProductDetails Product
        {
            get { return this.product; }
            set
            {
                this.product = value;
            }
        }

        public int EstimatedPriceToSort
        {
            get
            {
                return this.estimatedPriceToSort;
            }
            set
            {
                this.estimatedPriceToSort = value;
            }
        }

        public bool SurgeIndicator
        {
            get
            {
                return this.surgeIndicator;
            }
            set
            {
                this.surgeIndicator = value;
            }
        }
    }
}


