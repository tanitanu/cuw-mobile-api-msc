using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MOBShopReward
    {
        private bool available;
        private string bbxSolutionSet = string.Empty;
        private List<MOBShopBookingInfo> bookingInfos;
        private string bucket = string.Empty;
        private int bucketCount;
        private string bucketString = string.Empty;
        private string cabinType = string.Empty;
        private decimal changeFee;
        private decimal changeFeeTotal;
        private string crossCabinMessaging = string.Empty;
        private string fareBasis = string.Empty;
        private decimal mileage;
        private decimal mileageCollect;
        private decimal mileageCollectTotal;
        private decimal mileageTotal;
        private bool promotion;
        private string rewardCode = string.Empty;
        private List<MOBShopReward> rewards;
        private string rewardType = string.Empty;
        private int segmentIndex;
        private bool selected;
        private int soultion;
        private string solutionId = string.Empty;
        private string status = string.Empty;
        private bool suppressMileage;
        private List<MOBShopTax> taxes;
        private decimal taxTotal;
        private string travelArea = string.Empty;
        private int tripIndex;

        public bool Available
        {
            get
            {
                return this.available;
            }
            set
            {
                this.available = value;
            }
        }

        public string BBXSolutionSet
        {
            get
            {
                return this.bbxSolutionSet;
            }
            set
            {
                this.bbxSolutionSet = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBShopBookingInfo> BookingInfos
        {
            get
            {
                return this.bookingInfos;
            }
            set
            {
                this.bookingInfos = value;
            }
        }

        public string Bucket
        {
            get
            {
                return this.bucket;
            }
            set
            {
                this.bucket = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int BucketCount
        {
            get
            {
                return this.bucketCount;
            }
            set
            {
                this.bucketCount = value;
            }
        }

        public string BucketString
        {
            get
            {
                return this.bucketString;
            }
            set
            {
                this.bucketString = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string CabinType
        {
            get
            {
                return this.cabinType;
            }
            set
            {
                this.cabinType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public decimal ChangeFee
        {
            get
            {
                return this.changeFee;
            }
            set
            {
                this.changeFee = value;
            }
        }

        public decimal ChangeFeeTotal
        {
            get
            {
                return this.changeFeeTotal;
            }
            set
            {
                this.changeFeeTotal = value;
            }
        }

        public string CrossCabinMessaging
        {
            get
            {
                return this.crossCabinMessaging;
            }
            set
            {
                this.crossCabinMessaging = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FareBasis
        {
            get
            {
                return this.fareBasis;
            }
            set
            {
                this.fareBasis = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public decimal Mileage
        {
            get
            {
                return this.mileage;
            }
            set
            {
                this.mileage = value;
            }
        }

        public decimal MileageCollect
        {
            get
            {
                return this.mileageCollect;
            }
            set
            {
                this.mileageCollect = value;
            }
        }

        public decimal MileageCollectTotal
        {
            get
            {
                return this.mileageCollectTotal;
            }
            set
            {
                this.mileageCollectTotal = value;
            }
        }

        public decimal MileageTotal
        {
            get
            {
                return this.mileageTotal;
            }
            set
            {
                this.mileageTotal = value;
            }
        }

        public string RewardCode
        {
            get
            {
                return this.rewardCode;
            }
            set
            {
                this.rewardCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBShopReward> Rewards
        {
            get
            {
                return this.rewards;
            }
            set
            {
                this.rewards = value;
            }
        }

        public string RewardType
        {
            get
            {
                return this.rewardType;
            }
            set
            {
                this.rewardType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int SegmentIndex
        {
            get
            {
                return this.segmentIndex;
            }
            set
            {
                this.segmentIndex = value;
            }
        }

        public bool Selected
        {
            get
            {
                return this.selected;
            }
            set
            {
                this.selected = value;
            }
        }

        public int Soultion
        {
            get
            {
                return this.soultion;
            }
            set
            {
                this.soultion = value;
            }
        }

        public string SolutionId
        {
            get
            {
                return this.solutionId;
            }
            set
            {
                this.solutionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Status
        {
            get
            {
                return this.status;
            }
            set
            {
                this.status = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool SuppressMileage
        {
            get
            {
                return this.suppressMileage;
            }
            set
            {
                this.suppressMileage = value;
            }
        }

        public List<MOBShopTax> Taxes
        {
            get
            {
                return this.taxes;
            }
            set
            {
                this.taxes = value;
            }
        }

        public decimal TaxTotal
        {
            get
            {
                return this.taxTotal;
            }
            set
            {
                this.taxTotal = value;
            }
        }

        public string TravelArea
        {
            get
            {
                return this.travelArea;
            }
            set
            {
                this.travelArea = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int TripIndex
        {
            get
            {
                return this.tripIndex;
            }
            set
            {
                this.tripIndex = value;
            }
        }
    }
}
