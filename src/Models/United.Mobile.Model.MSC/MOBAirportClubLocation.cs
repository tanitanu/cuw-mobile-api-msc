using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBAirportClubLocation
    {
        private string airportCode = string.Empty;
        private string clubLocations = string.Empty;

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

        public string ClubLocations
        {
            get
            {
                return this.clubLocations;
            }
            set
            {
                this.clubLocations = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
