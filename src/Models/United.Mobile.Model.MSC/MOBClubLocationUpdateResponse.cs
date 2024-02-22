using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBClubLocationUpdateResponse : MOBResponse
    {
        private List<MOBAirportClubLocation> airportClubLocations;

        public MOBClubLocationUpdateResponse()
            : base()
        {
        }

        public List<MOBAirportClubLocation> AirportClubLocations
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
