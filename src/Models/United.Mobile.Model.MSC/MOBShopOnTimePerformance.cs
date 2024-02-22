using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MOBShopOnTimePerformance
    {
        private string effectiveDate = string.Empty;
        private string pctOnTimeCancelled = string.Empty; 
        private string pctOnTimeDelayed = string.Empty;
        private string pctOnTimeMax = string.Empty;
        private string pctOnTimeMin = string.Empty;

        public string EffectiveDate
        {
            get
            {
                return this.effectiveDate;
            }
            set
            {
                this.effectiveDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string PctOnTimeCancelled
        {
            get
            {
                return this.pctOnTimeCancelled;
            }
            set
            {
                this.pctOnTimeCancelled = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string PctOnTimeDelayed
        {
            get
            {
                return this.pctOnTimeDelayed;
            }
            set
            {
                this.pctOnTimeDelayed = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string PctOnTimeMax
        {
            get
            {
                return this.pctOnTimeMax;
            }
            set
            {
                this.pctOnTimeMax = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string PctOnTimeMin
        {
            get
            {
                return this.pctOnTimeMin;
            }
            set
            {
                this.pctOnTimeMin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
