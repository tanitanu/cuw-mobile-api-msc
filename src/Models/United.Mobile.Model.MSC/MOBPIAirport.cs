using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBPIAirport
    {
        private string airportCode = string.Empty;
        private string cityName = string.Empty;
        private string venueId = string.Empty;

        public string AirportCode
        {
            get
            {
                return this.airportCode;
            }
            set
            {
                this.airportCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string CityName
        {
            get
            {
                return this.cityName;
            }
            set
            {
                this.cityName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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
    }
}
