using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MOBMPRecentActivity
    {
        private string mileagePlusNumber = string.Empty;
        private List<MOBMPActivity> airlineActivities;
        private List<MOBMPActivity> nonAirlineActivities;
        private List<MOBMPActivity> rewardAirlineActivities;
        private List<MOBMPActivity> feqMilesActivities;

        public string MileagePlusNumber
        {
            get
            {
                return this.mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public List<MOBMPActivity> AirlineActivities
        {
            get
            {
                return this.airlineActivities;
            }
            set
            {
                this.airlineActivities = value;
            }
        }

        public List<MOBMPActivity> NonAirlineActivities
        {
            get
            {
                return this.nonAirlineActivities;
            }
            set
            {
                this.nonAirlineActivities = value;
            }
        }

        public List<MOBMPActivity> RewardAirlineActivities
        {
            get
            {
                return this.rewardAirlineActivities;
            }
            set
            {
                this.rewardAirlineActivities = value;
            }
        }

        public List<MOBMPActivity> FEQMilesActivities
        {
            get
            {
                return this.feqMilesActivities;
            }
            set
            {
                this.feqMilesActivities = value;
            }
        }
    }
}
