using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable]
    public class MOBCustomerProfileRequest:MOBRequest
    {
        
       // private bool profileOwnerOnly;
        private string sessionId = string.Empty;

        public string SessionId
        {
            get { return sessionId; }
            set { sessionId = value; }
        }

        //private string token = string.Empty;
        private string mileagePlusNumber = string.Empty;
        private int customerId;
        private string hashPinCode = string.Empty;
        private bool includeAllTravelerData;
        private bool includeAddresses;
        private bool includeEmailAddresses;
        private bool includePhones;
        private bool includeCreditCards;
        private bool includeSubscriptions;
        private bool includeTravelMarkets;
        private bool includeCustomerProfitScore;
        private bool includePets;
        private bool includeCarPreferences;
        private bool includeDisplayPreferences;
        private bool includeHotelPreferences;
        private bool includeAirPreferences;
        private bool includeContacts;
        private bool includePassports;
        private bool includeSecureTravelers;
        private bool includeFlexEQM;
        private bool includeServiceAnimals;
        private bool includeSpecialRequests;
        private bool includePosCountyCode;
        private bool profileOwnerOnly;
        private bool returnAllSavedTravelers;
        private string transactionPath = string.Empty;
        private string redirectViewName = string.Empty;
        private bool isRequirePKDispenserPublicKey;

        public bool IsRequirePKDispenserPublicKey
        {
            get { return isRequirePKDispenserPublicKey; }
            set { isRequirePKDispenserPublicKey = value; }
        }
        public string RedirectViewName
        {
            get { return redirectViewName; }
            set { redirectViewName = value; }
        }
        public string TransactionPath
        {
            get { return transactionPath; }
            set { transactionPath = value; }
        }

        public bool IncludeAllTravelerData
        {
            get
            {
                return includeAllTravelerData;
            }
            set
            {
                this.includeAllTravelerData = value;
            }
        }
        public bool IncludeAddresses
        {
            get
            {
                return includeAddresses;
            }
            set
            {
                this.includeAddresses = value;
            }
        }
        public bool IncludeEmailAddresses
        {
            get
            {
                return includeEmailAddresses;
            }
            set
            {
                this.includeEmailAddresses = value;
            }
        }
        public bool IncludePhones
        {
            get
            {
                return includePhones;
            }
            set
            {
                this.includePhones = value;
            }
        }
        public bool IncludeCreditCards
        {
            get
            {
                return includeCreditCards;
            }
            set
            {
                this.includeCreditCards = value;
            }
        }
        public bool IncludeSubscriptions
        {
            get
            {
                return includeSubscriptions;
            }
            set
            {
                this.includeSubscriptions = value;
            }
        }
        public bool IncludeTravelMarkets
        {
            get
            {
                return includeTravelMarkets;
            }
            set
            {
                this.includeTravelMarkets = value;
            }
        }
        public bool IncludeCustomerProfitScore
        {
            get
            {
                return includeCustomerProfitScore;
            }
            set
            {
                this.includeCustomerProfitScore = value;
            }
        }
        public bool IncludePets
        {
            get
            {
                return includePets;
            }
            set
            {
                this.includePets = value;
            }
        }
        public bool IncludeCarPreferences
        {
            get
            {
                return includeCarPreferences;
            }
            set
            {
                this.includeCarPreferences = value;
            }
        }
        public bool IncludeDisplayPreferences
        {
            get
            {
                return includeDisplayPreferences;
            }
            set
            {
                this.includeDisplayPreferences = value;
            }
        }
        public bool IncludeHotelPreferences
        {
            get
            {
                return includeHotelPreferences;
            }
            set
            {
                this.includeHotelPreferences = value;
            }
        }
        public bool IncludeAirPreferences
        {
            get
            {
                return includeAirPreferences;
            }
            set
            {
                this.includeAirPreferences = value;
            }
        }
        public bool IncludeContacts
        {
            get
            {
                return includeContacts;
            }
            set
            {
                this.includeContacts = value;
            }
        }
        public bool IncludePassports
        {
            get
            {
                return includePassports;
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
                return includeSecureTravelers;
            }
            set
            {
                this.includeSecureTravelers = value;
            }
        }
        public bool IncludeFlexEQM
        {
            get
            {
                return includeFlexEQM;
            }
            set
            {
                this.includeFlexEQM = value;
            }
        }
        public bool IncludeServiceAnimals
        {
            get
            {
                return includeServiceAnimals;
            }
            set
            {
                this.includeServiceAnimals = value;
            }
        }
        public bool IncludeSpecialRequests
        {
            get
            {
                return includeSpecialRequests;
            }
            set
            {
                this.includeSpecialRequests = value;
            }
        }
        public bool IncludePosCountyCode
        {
            get
            {
                return includePosCountyCode;
            }
            set
            {
                this.includePosCountyCode = value;
            }
        }

        public bool ProfileOwnerOnly
        {
            get
            {
                return profileOwnerOnly;
            }
            set
            {
                this.profileOwnerOnly = value;
            }
        }
        public bool ReturnAllSavedTravelers
        {
            get
            {
                return returnAllSavedTravelers;
            }
            set
            {
                this.returnAllSavedTravelers = value;
            }
        }


        public string MileagePlusNumber
        {
            get
            {
                return mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public int CustomerId
        {
            get
            {
                return customerId;
            }
            set
            {
                this.customerId = value;
            }
        }

        public string HashPinCode
        {
            get
            {
                return hashPinCode;
            }
            set
            {
                this.hashPinCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
