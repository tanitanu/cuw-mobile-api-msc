using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Fitbit
{
    [Serializable]
    public class FlightStatusResponse : MOBResponse
    {
        private FlightStatusSegment flightStatusSegment;

        public FlightStatusSegment FlightStatusSegment
        {
            get
            {
                return this.flightStatusSegment;
            }
            set
            {
                this.flightStatusSegment = value;
            }
        }
    }
}
