using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBMPPNRs
    {
        private string mileagePlusNumber = string.Empty;
        private List<MOBPNR> current;
        private List<MOBPNR> past;
        private List<MOBPNR> cancelled;
        private List<MOBPNR> inactive;
        private bool gotException4GetPNRSbyMPCSLcallDoNotDropExistingPnrsInWallet = false;

        public string MileagePlusNumber
        {
            get
            {
                return this.mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public List<MOBPNR> Current
        {
            get
            {
                return this.current;

            }
            set
            {
                this.current = value;
            }
        }

        public List<MOBPNR> Past
        {
            get
            {
                return this.past;
            }
            set
            {
                this.past = value;
            }
        }

        public List<MOBPNR> Cancelled
        {
            get
            {
                return this.cancelled;
            }
            set
            {
                this.cancelled = value;
            }
        }

        public List<MOBPNR> Inactive
        {
            get
            {
                return this.inactive;
            }
            set
            {
                this.inactive = value;
            }
        }
        public bool GotException4GetPNRSbyMPCSLcallDoNotDropExistingPnrsInWallet
        {
            get { return gotException4GetPNRSbyMPCSLcallDoNotDropExistingPnrsInWallet; }
            set { gotException4GetPNRSbyMPCSLcallDoNotDropExistingPnrsInWallet = value; }
        }
    }
}
