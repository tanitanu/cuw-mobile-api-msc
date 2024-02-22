using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable]
    public class MOBFlightStatusResponse : MOBResponse
    {
        private MOBFlightStatusInfo flightStatusInfo;


        public MOBFlightStatusResponse()
            : base()
        {
        }


        public MOBFlightStatusInfo FlightStatusInfo
        {
            get
            {
                return this.flightStatusInfo;
            }
            set
            {
                this.flightStatusInfo = value;
            }
        }
    }
}
