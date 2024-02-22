using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.ClubMatchOLDREST
{
    [Serializable()]
    public class MOBUnitedClubRequestToMatchOLDREST : RequestToMatchOLDREST
    {
        private string airportCode = string.Empty;
        private string clubType = string.Empty;
        private bool bfcOnly;
        private string mileagePlusNumber = string.Empty;

        public string MileagePlusNumber
        {
            get { return mileagePlusNumber; }
            set { this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper(); }
        }

        public MOBUnitedClubRequestToMatchOLDREST()
            : base()
        {
        }

        public string AirportCode
        {
            get
            {
                return this.airportCode;
            }
            set
            {
                this.airportCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ClubType
        {
            get
            {
                return this.clubType;
            }
            set
            {
                this.clubType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool BFCOnly
        {
            get
            {
                return this.bfcOnly;
            }
            set
            {
                this.bfcOnly = value;
            }
        }
    }
    [Serializable()]
    public class RequestToMatchOLDREST
    {
        private string accessCode = string.Empty;
        private string transactionId = string.Empty;
        private string languageCode = string.Empty;
        private int applicationId;
        private string applicationVersion = string.Empty;
        private string deviceId = string.Empty;
        public string AccessCode
        {
            get
            {
                return this.accessCode;
            }
            set
            {
                this.accessCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string TransactionId
        {
            get
            {
                return this.transactionId;
            }
            set
            {
                this.transactionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string LanguageCode
        {
            get
            {
                return this.languageCode;
            }
            set
            {
                this.languageCode = string.IsNullOrEmpty(value) ? "en-US" : value.Trim();
            }
        }

        public int ApplicationId
        {
            get
            {
                return this.applicationId;
            }
            set
            {
                this.applicationId = value;
            }
        }

        public string ApplicationVersion
        {
            get
            {
                return this.applicationVersion;
            }
            set
            {
                this.applicationVersion = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string DeviceId
        {
            get
            {
                return this.deviceId;
            }
            set
            {
                this.deviceId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
    [Serializable]
    public class MOBAllClubRequestToMatchOLDREST : RequestToMatchOLDREST
    {
        private string airportCode = string.Empty;
        private string clubType = string.Empty;
        private bool bfcOnly;
        private string mileagePlusNumber = string.Empty;

        public string MileagePlusNumber
        {
            get { return mileagePlusNumber; }
            set { this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper(); }
        }
        public MOBAllClubRequestToMatchOLDREST()
            : base()
        {
        }

        public string AirportCode
        {
            get
            {
                return this.airportCode;
            }
            set
            {
                this.airportCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ClubType
        {
            get
            {
                return this.clubType;
            }
            set
            {
                this.clubType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool BFCOnly
        {
            get
            {
                return this.bfcOnly;
            }
            set
            {
                this.bfcOnly = value;
            }
        }
    }
}
