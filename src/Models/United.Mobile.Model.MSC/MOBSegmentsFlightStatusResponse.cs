using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBSegmentsFlightStatusResponse : MOBResponse
    {
        private List<MOBFlightStatusInfo> flightStatusInfoList;

        public List<MOBFlightStatusInfo> FlightStatusInfoList
        {
            get
            {
                return this.flightStatusInfoList;
            }
            set
            {
                this.flightStatusInfoList = value;
            }
        }
    }
}
