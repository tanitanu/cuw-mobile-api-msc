using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Pcu
{
    [Serializable()]
    public class MOBPcuOptions
    {
        private List<string> eligibleTravelers;
        private List<MOBPcuSegment> eligibleSegments;
        private List<MOBPcuUpgradeOptionInfo> compareOptions;
        private string currencyCode;

        public List<string> EligibleTravelers
        {
            get { return eligibleTravelers; }
            set { eligibleTravelers = value; }
        }

        public List<MOBPcuSegment> EligibleSegments
        {
            get { return eligibleSegments; }
            set { eligibleSegments = value; }
        }

        public List<MOBPcuUpgradeOptionInfo> CompareOptions
        {
            get { return compareOptions; }
            set { compareOptions = value; }
        }

        public string CurrencyCode
        {
            get { return currencyCode; }
            set { currencyCode = value; }
        }
    }
}
