using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition.ClubMatchOLDREST
{
    [Serializable()]
    public class ClubAmenity
    {
        private string amenity = string.Empty;
        private string guests = string.Empty;

        public string Amenity
        {
            get
            {
                return this.amenity;
            }
            set
            {
                this.amenity = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Guests
        {
            get
            {
                return this.guests;
            }
            set
            {
                this.guests = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
