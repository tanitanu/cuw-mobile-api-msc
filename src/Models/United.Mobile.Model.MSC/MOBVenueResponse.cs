using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable]
    public class MOBVenueResponse : MOBResponse
    {
        public MOBVenueResponse()
            : base()
        {
        }

        private List<MOBVenue> venues;

        public List<MOBVenue> Venues
        {
            get { return this.venues; }
            set
            {
                this.venues = value;
            }
        }
    }
}
