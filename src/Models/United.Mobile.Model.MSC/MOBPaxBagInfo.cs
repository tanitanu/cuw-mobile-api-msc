using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBPaxBagInfo
    {
        private string paxFirstName = string.Empty;
        private string paxLastName = string.Empty;
        private string pnr = string.Empty;
        private string paxID = string.Empty;
        private List<MOBPaxFlight> paxFlights;
        private List<MOBPaxBag> paxBags;

        public string PaxFirstName
        {
            get
            {
                return this.paxFirstName;
            }
            set
            {
                this.paxFirstName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string PaxLastName
        {
            get
            {
                return this.paxLastName;
            }
            set
            {
                this.paxLastName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string PNR
        {
            get
            {
                return this.pnr;
            }
            set
            {
                this.pnr = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string PaxID
        {
            get
            {
                return this.paxID;
            }
            set
            {
                this.paxID = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBPaxFlight> PaxFlights
        {
            get
            {
                return this.paxFlights;
            }
            set
            {
                this.paxFlights = value;
            }
        }

        public List<MOBPaxBag> PaxBags
        {
            get
            {
                return this.paxBags;
            }
            set
            {
                this.paxBags = value;
            }
        }
    }
}
