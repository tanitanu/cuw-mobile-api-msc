using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBBagHistory
    {
        private string bagTagIssStn = string.Empty;
        private string bagTagIssDt = string.Empty;

        private MOBBagFlight bagFlight;
        private MOBBagStatus bagStatus;

        public MOBBagHistory()
        {
        }

        public string BagTagIssStn
        {
            get
            {
                return this.bagTagIssStn;
            }
            set
            {
                this.bagTagIssStn = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string BagTagIssDt
        {
            get
            {
                return this.bagTagIssDt;
            }
            set
            {
                this.bagTagIssDt = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBBagFlight BagFlight
        {
            get
            {
                return this.bagFlight;
            }
            set
            {
                this.bagFlight = value;
            }
        }

        public MOBBagStatus BagStatus
        {
            get
            {
                return this.bagStatus;
            }
            set
            {
                this.bagStatus = value;
            }
        }
    }
}
