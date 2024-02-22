using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBAirportGeoInfoResponse : MOBResponse
    {
        public MOBAirportGeoInfoResponse()
            : base()
        {
        }

        private MOBAirportGeoInfo airportGeoInfo;

        public MOBAirportGeoInfo AirportGeoInfo
        {
            get
            {
                return this.airportGeoInfo;
            }
            set
            {
                this.airportGeoInfo = value;
            }
        }
    }
}
