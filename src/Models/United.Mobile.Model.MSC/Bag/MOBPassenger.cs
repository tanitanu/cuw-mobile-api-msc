using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition.Bag
{
    [Serializable]
    public class MOBPassenger
    {
        private string id = string.Empty;
        private string givenName = string.Empty;
        private string sirName = string.Empty;
        private List<MOBLoyaltyProgramProfile> loyaltyProgramProfiles;
        private List<string> bagTags;
        private MOBPassengerItinerary itinerary; 

        public string Id
        {
            get
            {
                return id;
            }
            set
            {
                this.id = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string GivenName
        {
            get
            {
                return givenName;
            }
            set
            {
                this.givenName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string SirName
        {
            get
            {
                return sirName;
            }
            set
            {
                this.sirName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBLoyaltyProgramProfile> LoyaltyProgramProfiles
        {
            get
            {
                return loyaltyProgramProfiles;
            }
            set
            {
                this.loyaltyProgramProfiles = value;
            }
        }

        public List<string> BagTags
        {
            get
            {
                return bagTags;
            }
            set
            {
                this.bagTags = value;
            }
        }

        public MOBPassengerItinerary Itinerary
        {
            get
            {
                return itinerary;
            }
            set
            {
                this.itinerary = value;
            }
        }
    }
}
