using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    public class MOBTravelReadyViewFlightSegment : MOBTravelReadyFlightSegment
    {
        public string TravelReadyView { get; set; }

        public int FlightCountInWindow { get; set; }
    }
}
