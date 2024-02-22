using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.ClubMatchOLDREST
{
    public class MOBClubLocationUpdateResponseToMatchOLDREST: ResponseToMatchOLDREST
    {
        private List<AirportClubLocation> airportClubLocations;

        public MOBClubLocationUpdateResponseToMatchOLDREST()
            : base()
        {
        }

        public List<AirportClubLocation> AirportClubLocations
        {
            get
            {
                return this.airportClubLocations;
            }
            set
            {
                this.airportClubLocations = value;
            }
        }
    }
}
