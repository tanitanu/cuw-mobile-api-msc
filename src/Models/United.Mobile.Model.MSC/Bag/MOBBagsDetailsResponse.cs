using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;

namespace United.Definition.Bag
{
    [Serializable]
    public class MOBBagsDetailsResponse : MOBResponse
    {
        private List<MOBBagsDetails> bagsDetails;
        public List<MOBBagsDetails> BagsDetails
        {
            get
            {
                return this.bagsDetails;
            }
            set
            {
                this.bagsDetails = value;
            }
        }

        private string lastUpdatedTimeGMT = string.Empty;
        public string LastUpdatedTimeGMT
        {
            get
            {
                return lastUpdatedTimeGMT;
            }
            set
            {
                this.lastUpdatedTimeGMT = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }

    [Serializable]
    public class MOBBagTrackingDetailsResponse : MOBResponse
    {
        private MOBBagsDetailsRequest bagTrackingDetailsRequest;

        public MOBBagsDetailsRequest BagTrackingDetailsRequest
        {
            get { return bagTrackingDetailsRequest; }
            set { bagTrackingDetailsRequest = value; }
        }

        private List<MOBBagsDetails> bagsDetails;
        public List<MOBBagsDetails> BagsDetails
        {
            get
            {
                return this.bagsDetails;
            }
            set
            {
                this.bagsDetails = value;
            }
        }

        private string lastUpdatedTimeGMT = string.Empty;
        public string LastUpdatedTimeGMT
        {
            get
            {
                return lastUpdatedTimeGMT;
            }
            set
            {
                this.lastUpdatedTimeGMT = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }

    [Serializable]
    public class MOBGetBagForPNRsResponse : MOBResponse
    {
        private MOBGetBagsForPNRsRequest getBagsForPNRsRequest;

        public MOBGetBagsForPNRsRequest GetBagsForPNRsRequest
        {
            get { return getBagsForPNRsRequest; }
            set { getBagsForPNRsRequest = value; }
        }

        private List<MOBBagsDetails> bagsDetails;
        public List<MOBBagsDetails> BagsDetails
        {
            get
            {
                return this.bagsDetails;
            }
            set
            {
                this.bagsDetails = value;
            }
        }

        private string lastUpdatedTimeGMT = string.Empty;
        public string LastUpdatedTimeGMT
        {
            get
            {
                return lastUpdatedTimeGMT;
            }
            set
            {
                this.lastUpdatedTimeGMT = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
