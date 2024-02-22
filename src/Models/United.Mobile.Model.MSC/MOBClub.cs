using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBClub
    {
        private string id = string.Empty;
        private string name = string.Empty;
        private string location = string.Empty;
        private string locationName = string.Empty;
        private string web = string.Empty;
        private string notes = string.Empty;
        private bool bfc;
        private List<string> hours;
        private List<string> phoneNumbers;
        private List<MOBClubAmenity> amenities;
        private List<MOBClubEligibility> eligibilities;
        private MOBGeoLocationInfo geoLocation;
        private MOBClubConferenceRoom conferenceRoom;
        private string clubType = string.Empty;
        private bool pClub = false;
        private bool redCarpetClub = false;
        private string sortField = string.Empty;

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

        public List<MOBClubAmenity> Amenities
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

        public List<MOBClubEligibility> Eligibilities
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

        public MOBGeoLocationInfo GeoLocation
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

        public MOBClubConferenceRoom ConferenceRoom
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
    }
}
