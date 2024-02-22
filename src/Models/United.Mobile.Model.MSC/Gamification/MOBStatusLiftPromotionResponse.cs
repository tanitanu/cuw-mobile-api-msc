using System;
using United.Mobile.Model.Common;

namespace United.Definition.Gamification
{
    [Serializable]
    public class MOBStatusLiftPromotionResponse : MOBResponse
    {
        private int currentLevel = 0;
        private string currentLevelName = string.Empty;
        private string premierStatusDescription = string.Empty;
        private int eligibleLevel = 0;
        private string eligibleLevelCode = string.Empty;
        private string eligibleLevelName = string.Empty;
        private string mileagePlusNumber = string.Empty;
        private string custID = string.Empty;
        private string promotionId = string.Empty;
        private string memberPromotionID = string.Empty;
        private string altRefID1 = string.Empty;
        private bool? isRegistered = false;
        private string state = string.Empty;
        private string expirationDate;
        private string registrationTimestamp;       
        private string premierStatusImageurl = string.Empty;
        private string premierStatusTabletImageurl = string.Empty;
        private string premierStatusHomeScreenLandingImageurl = string.Empty;
        private string vanityUrl = string.Empty;

        public string MileagePlusNumber
        {
            get { return mileagePlusNumber; }
            set { this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper(); }
        }

        public string CustID
        {
            get
            {
                return this.custID;
            }
            set
            {
                this.custID = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string PromotionId
        {
            get { return promotionId; }
            set { promotionId = value; }
        }

        public string MemberPromotionID
        {
            get
            {
                return this.memberPromotionID;
            }
            set
            {
                this.memberPromotionID = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string AltRefID1
        {
            get
            {
                return this.altRefID1;
            }
            set
            {
                this.altRefID1 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public bool? IsRegistered
        {
            get
            {
                return this.isRegistered;
            }
            set
            {
                this.isRegistered = value;
            }
        }

        public string State
        {
            get
            {
                return this.state;
            }
            set
            {
                this.state = value;
            }
        }

        public string ExpirationDate
        {
            get
            {
                return this.expirationDate;
            }
            set
            {
                this.expirationDate = value;
            }
        }

        public string RegistrationTimestamp
        {
            get
            {
                return this.registrationTimestamp;
            }
            set
            {
                this.registrationTimestamp = value;
            }
        }
        public int CurrentLevel
        {
            get { return currentLevel; }
            set { currentLevel = value; }
        }
        public string CurrentLevelName
        {
            get { return currentLevelName; }
            set { currentLevelName = value; }
        }
        public string PremierStatusDescription
        {
            get { return premierStatusDescription; }
            set { premierStatusDescription = value; }
        }

        public int EligibleLevel
        {
            get { return eligibleLevel; }
            set { eligibleLevel = value; }
        }
        public string EligibleLevelCode
        {
            get { return eligibleLevelCode; }
            set { eligibleLevelCode = value; }
        }
        public string EligibleLevelName
        {
            get { return eligibleLevelName; }
            set { eligibleLevelName = value; }
        }

        public string PremierStatusImageurl
        {
            get { return premierStatusImageurl; }
            set { premierStatusImageurl = value; }
        }

        public string PremierStatusTabletImageurl
        {
            get { return premierStatusTabletImageurl; }
            set { premierStatusTabletImageurl = value; }
        }

        public string PremierStatusHomeScreenLandingImageurl
        {
            get { return premierStatusHomeScreenLandingImageurl; }
            set { premierStatusHomeScreenLandingImageurl = value; }
        }
        public string VanityUrl
        {
            get { return vanityUrl; }
            set { vanityUrl = value; }
        }

    }
}





