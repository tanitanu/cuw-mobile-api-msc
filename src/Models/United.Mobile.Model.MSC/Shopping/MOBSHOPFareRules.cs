using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPFareRules
    {
        private string origin;
        private string destination;
        private string fareBasisCode;
        private string serviceClass;
        private List<MOBSHOPFareRuleList> fareRuleTextList;

        public string Origin
        {
            get
            {
                return this.origin;
            }
            set
            {
                this.origin = value;
            }
        }

        public string Destination
        {
            get
            {
                return this.destination;
            }
            set
            {
                this.destination = value;
            }
        }

        public string FareBasisCode
        {
            get
            {
                return this.fareBasisCode;
            }
            set
            {
                this.fareBasisCode = value;
            }
        }

        public string ServiceClass
        {
            get
            {
                return this.serviceClass;
            }
            set
            {
                this.serviceClass = value;
            }
        }

        public List<MOBSHOPFareRuleList> FareRuleTextList
        {
            get
            {
                return this.fareRuleTextList;
            }
            set
            {
                this.fareRuleTextList = value;
            }
        }
    }
}
