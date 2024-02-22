using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition.Bag
{
    [Serializable]
    public class MOBBagItinerary
    {
        private string currentItineraryRoute = string.Empty;
        private string originalItineraryRoute = string.Empty;
        private List<MOBBagFlightSegment> currentItinerary;
        private string currentPaxItineraryRoute = string.Empty;

        public string CurrentItineraryRoute
        {
            get
            {
                return currentItineraryRoute;
            }
            set
            {
                this.currentItineraryRoute = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string OriginalItineraryRoute
        {
            get
            {
                return originalItineraryRoute;
            }
            set
            {
                this.originalItineraryRoute = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBBagFlightSegment> CurrentItinerary
        {
            get
            {
                return currentItinerary;
            }
            set
            {
                this.currentItinerary = value;
            }
        }

        public string CurrentPaxItineraryRoute
        {
            get
            {
                return currentPaxItineraryRoute;
            }
            set
            {
                this.currentPaxItineraryRoute = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
