using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    public class MOBWalletFlightStatusResponse
    {
        private List<MOBWalletFlightStatus> flightStatusList;
        private List<MOBFlightStatusSegment> flightStatusSegments;

        public List<MOBWalletFlightStatus> FlightStatusList
        {
            get
            {
                return this.flightStatusList;
            }
            set
            {
                this.flightStatusList = value;
            }
        }

        public List<MOBFlightStatusSegment> FlightStatusSegments
        {
            get
            {
                return this.flightStatusSegments;
            }
            set
            {
                this.flightStatusSegments = value;
            }
        }
    }
}
