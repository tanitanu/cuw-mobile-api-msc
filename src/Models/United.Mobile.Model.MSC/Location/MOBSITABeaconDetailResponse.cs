using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Location
{
    [Serializable]
    public class MOBSITABeaconDetailResponse : MOBResponse
    {
        private MOBSITABeaconDetail beaconDetail;

        public MOBSITABeaconDetail BeaconDetail
        {
            get
            {
                return this.beaconDetail;
            }
            set
            {
                this.beaconDetail = value;
            }
        }
    }
}
