using System;
using System.Collections.Generic;
using System.Text;

namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBCarbonEmissionData
    {
        private string flightHash;

        public string FlightHash
        {
            get
            {
                return this.flightHash;
            }
            set
            {
                this.flightHash = value;
            }
        }

        private MOBItemWithIconName carbonDetails;

        public MOBItemWithIconName CarbonDetails
        {
            get { return carbonDetails; }
            set { carbonDetails = value; }
        }

        private MOBContentScreen contentScreen;

        public MOBContentScreen ContentScreen
        {
            get { return contentScreen; }
            set { contentScreen = value; }
        }
    }
}
