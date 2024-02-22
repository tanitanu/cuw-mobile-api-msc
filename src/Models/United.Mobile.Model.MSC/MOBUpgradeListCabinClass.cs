using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable]
    public class MOBUpgradeListCabinClass
    {
        private string cabinTypeDescription = string.Empty;
        private List<MOBTypeOption> cabinBookingStatus;
        private List<MOBLDPassenger> upgraded;
        private List<MOBLDPassenger> standby;

        public List<MOBTypeOption> CabinBookingStatus
        {
            get
            {
                return this.cabinBookingStatus;
            }
            set
            {
                this.cabinBookingStatus = value;
            }
        }

        public List<MOBLDPassenger> Upgraded
        {
            get
            {
                return this.upgraded;
            }
            set
            {
                this.upgraded = value;
            }
        }

        public List<MOBLDPassenger> Standby
        {
            get
            {
                return this.standby;
            }
            set
            {
                this.standby = value;
            }
        }

        public string CabinTypeDescription
        {
            get { return this.cabinTypeDescription; }
            set { this.cabinTypeDescription = value; }
        }
    }
}
