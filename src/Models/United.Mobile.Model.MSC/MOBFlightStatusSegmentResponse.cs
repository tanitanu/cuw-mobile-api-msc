using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBFlightStatusSegmentResponse : MOBResponse
    {
        private MOBFlightStatusSegment flightStatusSegment;



        public MOBFlightStatusSegmentResponse()
            : base()
        {
        }


        public MOBFlightStatusSegment FlightStatusSegment
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
