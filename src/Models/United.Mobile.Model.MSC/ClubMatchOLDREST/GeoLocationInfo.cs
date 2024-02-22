﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition.ClubMatchOLDREST
{
    [Serializable()]
    public class GeoLocationInfo
    {
        private string locationId = string.Empty;
        private string venueId = string.Empty;
        private string latitude = string.Empty;
        private string longitude = string.Empty;
        private string llPoiId = string.Empty;

        public string LocationId
        {
            get
            {
                return this.locationId;
            }
            set
            {
                this.locationId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string VenueId
        {
            get
            {
                return this.venueId;
            }
            set
            {
                this.venueId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Latitude
        {
            get
            {
                return this.latitude;
            }
            set
            {
                this.latitude = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Longitude
        {
            get
            {
                return this.longitude;
            }
            set
            {
                this.longitude = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string LLPoiId
        {
            get
            {
                return this.llPoiId;
            }
            set
            {
                this.llPoiId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
