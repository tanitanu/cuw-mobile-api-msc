using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBStationResponse : MOBResponse
    {
        private string availableFlag = string.Empty;
        private List<MOBStation> stations;

        public MOBStationResponse()
            : base()
        {
        }

        public string AvailableFlag
        {
            get
            {
                return this.availableFlag;
            }
            set
            {
                this.availableFlag = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBStation> Stations
        {
            get
            {
                return this.stations;
            }
            set
            {
                this.stations = value;
            }
        }
    }
}
