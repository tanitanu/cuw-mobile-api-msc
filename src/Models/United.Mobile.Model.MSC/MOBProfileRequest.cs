using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBProfileRequest : MOBRequest
    {
        private string mileagePlusNumber = string.Empty;
        private string pinCode = string.Empty;
        private string sessionID = string.Empty;
        private bool includeAddresses;
        private bool includeEmails;
        private bool includePhones;
        private bool includePaymentInfos;
        private bool includePartnerCards;
        private bool includeClubs;
        private bool includeAirPreferences;
        private bool includeCarPreferences;
        private bool includeHotelPreferences;
        private bool includeDisplayPreferences;
        private bool includeSubscriptions;
        private bool includePassports;
        private bool includeSecureTravelers;
        private bool includePets;

        public MOBProfileRequest()
            : base()
        {
        }

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

        public string PinCode
        {
            get
            {
                return this.pinCode;
            }
            set
            {
                this.pinCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string SessionID
        {
            get
            {
                return this.sessionID;
            }
            set
            {
                this.sessionID = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public bool IncludeAddresses
        {
            get
            {
                return this.includeAddresses;
            }
            set
            {
                this.includeAddresses = value;
            }
        }

        public bool IncludeEmails
        {
            get
            {
                return this.includeEmails;
            }
            set
            {
                this.includeEmails = value;
            }
        }

        public bool IncludePhones
        {
            get
            {
                return this.includePhones;
            }
            set
            {
                this.includePhones = value;
            }
        }

        public bool IncludePaymentInfos
        {
            get
            {
                return this.includePaymentInfos;
            }
            set
            {
                this.includePaymentInfos = value;
            }
        }

        public bool IncludePartnerCards
        {
            get
            {
                return this.includePartnerCards;
            }
            set
            {
                this.includePartnerCards = value;
            }
        }

        public bool IncludeClubs
        {
            get
            {
                return this.includeClubs;
            }
            set
            {
                this.includeClubs = value;
            }
        }

        public bool IncludeAirPreferences
        {
            get
            {
                return this.includeAirPreferences;
            }
            set
            {
                this.includeAirPreferences = value;
            }
        }

        public bool IncludeCarPreferences
        {
            get
            {
                return this.includeCarPreferences;
            }
            set
            {
                this.includeCarPreferences = value;
            }
        }

        public bool IncludeHotelPreferences
        {
            get
            {
                return this.includeHotelPreferences;
            }
            set
            {
                this.includeHotelPreferences = value;
            }
        }

        public bool IncludeDisplayPreferences
        {
            get
            {
                return this.includeDisplayPreferences;
            }
            set
            {
                this.includeDisplayPreferences = value;
            }
        }

        public bool IncludeSubscriptions
        {
            get
            {
                return this.includeSubscriptions;
            }
            set
            {
                this.includeSubscriptions = value;
            }
        }

        public bool IncludePassports
        {
            get
            {
                return this.includePassports;
            }
            set
            {
                this.includePassports = value;
            }
        }

        public bool IncludeSecureTravelers
        {
            get
            {
                return this.includeSecureTravelers;
            }
            set
            {
                this.includeSecureTravelers = value;
            }
        }

        public bool IncludePets
        {
            get
            {
                return this.includePets;
            }
            set
            {
                this.includePets = value;
            }
        }
    }
}
