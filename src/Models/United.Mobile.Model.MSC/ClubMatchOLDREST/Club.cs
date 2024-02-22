using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition.ClubMatchOLDREST
{
    [Serializable()]
    public class Club
    {
        private string id = string.Empty;
        private string name = string.Empty;
        private string location = string.Empty;
        private string locationName = string.Empty;
        private string web = string.Empty;
        private string notes = string.Empty;
        private bool bfc;
        private List<string> hours = new List<string>();
        private List<string> phoneNumbers = new List<string>();
        private List<ClubAmenity> amenities = new List<ClubAmenity>();
        private List<ClubEligibility> eligibilities = new List<ClubEligibility>();
        private GeoLocationInfo geoLocation = new GeoLocationInfo();
        private ClubConferenceRoom conferenceRoom = new ClubConferenceRoom();
        private string clubType = string.Empty;
        private bool pClub = false;
        private bool redCarpetClub = false;
        private string sortField = string.Empty;
        private string clubTitle = string.Empty; // "Lounge Details" / "United Polaris Lounge" 
        private bool allowOTPPurchase = false;
        private bool showAirportMap = false;
        private List<KeyValue> dayAndHours = new List<KeyValue>();
        private string eligibilityDescription = string.Empty;
        public string Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Location
        {
            get
            {
                return this.location;
            }
            set
            {
                this.location = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string LocationName
        {
            get
            {
                return this.locationName;
            }
            set
            {
                this.locationName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Web
        {
            get
            {
                return this.web;
            }
            set
            {
                this.web = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Notes
        {
            get
            {
                return this.notes;
            }
            set
            {
                this.notes = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool BFC
        {
            get
            {
                return this.bfc;
            }
            set
            {
                this.bfc = value;
            }
        }

        public List<string> Hours
        {
            get
            {
                return this.hours;
            }
            set
            {
                this.hours = value;
            }
        }

        public List<string> PhoneNumbers
        {
            get
            {
                return this.phoneNumbers;
            }
            set
            {
                this.phoneNumbers = value;
            }
        }

        public List<ClubAmenity> Amenities
        {
            get
            {
                return this.amenities;
            }
            set
            {
                this.amenities = value;
            }
        }

        public List<ClubEligibility> Eligibilities
        {
            get
            {
                return this.eligibilities;
            }
            set
            {
                this.eligibilities = value;
            }
        }

        public GeoLocationInfo GeoLocation
        {
            get
            {
                return this.geoLocation;
            }
            set
            {
                this.geoLocation = value;
            }
        }

        public ClubConferenceRoom ConferenceRoom
        {
            get
            {
                return this.conferenceRoom;
            }
            set
            {
                this.conferenceRoom = value;
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

        public bool PClub
        {
            get
            {
                return this.pClub;
            }
            set
            {
                this.pClub = value;
            }
        }

        public bool RedCarpetClub
        {
            get
            {
                return this.redCarpetClub;
            }
            set
            {
                this.redCarpetClub = value;
            }
        }

        public string SortField
        {
            get
            {
                return this.sortField;
            }
            set
            {
                this.sortField = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string ClubTitle
        {
            get
            {
                return this.clubTitle;
            }
            set
            {
                this.clubTitle = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public bool AllowOTPPurchase
        {
            get { return allowOTPPurchase; } 
            set { allowOTPPurchase = value; }
        }
        public bool ShowAirportMap
        {
            get { return showAirportMap; }
            set { showAirportMap = value; }
        }
        public List<KeyValue> DayAndHours
        {
            get
            {
                return this.dayAndHours;
            }
            set
            {
                this.dayAndHours = value;
            }
        }
        public string EligibilityDescription
        {
            get
            {
                return this.eligibilityDescription;
            }
            set
            {
                this.eligibilityDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
