using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Location
{
    [Serializable]
    public class MOBSITAWalkInfo
    {
        private int walkTimeMins;
        private bool passSecurity;
        private bool passImmigration;
        private string destinationGate = string.Empty;
        private string beaconName = string.Empty;

        public int WalkTimeMins
        {
            get
            {
                return this.walkTimeMins;
            }
            set
            {
                this.walkTimeMins = value;
            }
        }

        public bool PassSecurity
        {
            get
            {
                return this.passSecurity;
            }
            set
            {
                this.passSecurity = value;
            }
        }

        public bool PassImmigration
        {
            get
            {
                return this.passImmigration;
            }
            set
            {
                this.passImmigration = value;
            }
        }

        public string DestinationGate
        {
            get
            {
                return this.destinationGate;
            }
            set
            {
                this.destinationGate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string BeaconName
        {
            get
            {
                return this.beaconName;
            }
            set
            {
                this.beaconName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
    }
}
