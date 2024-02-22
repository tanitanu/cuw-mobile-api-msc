using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;

namespace United.Definition.Location
{
    [Serializable]
    public class MOBSITABeaconResponse : MOBResponse
    {
        private List<MOBSITABeacon> beacons;

        public List<MOBSITABeacon> Beacons
        {
            get
            {
                return this.beacons;
            }
            set
            {
                this.beacons = value;
            }
        }
    }
}
