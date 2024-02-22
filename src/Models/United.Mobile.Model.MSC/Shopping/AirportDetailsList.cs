using System;
using System.Collections.Generic;
using United.Definition.Bag;

namespace United.Persist.Definition.Shopping
{
    [Serializable]
    public class AirportDetailsList
    {
        private List<MOBDisplayBagTrackAirportDetails> airportsList;
        public List<MOBDisplayBagTrackAirportDetails> AirportsList
        {
            get { return airportsList; }
            set { airportsList = value; }
        }
    }
}
