using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MOBMPStatement
    {
        private string mileagePlusNumber = string.Empty;
        private string startDate = string.Empty;
        private string endDate = string.Empty;
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

        public string StartDate
        {
            get
            {
                return this.startDate;
            }
            set
            {
                this.startDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string EndDate
        {
            get
            {
                return this.endDate;
            }
            set
            {
                this.endDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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
