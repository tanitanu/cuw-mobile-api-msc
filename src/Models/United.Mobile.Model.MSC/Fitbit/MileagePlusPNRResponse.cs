using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;

namespace United.Definition.Fitbit
{
    [Serializable]
    public class MileagePlusPNRResponse : MOBResponse
    {
        private string mileagePlusNumber = string.Empty;
        private List<PNR> pnrs;

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

        public List<PNR> PNRs
        {
            get
            {
                return this.pnrs;
            }
            set
            {
                this.pnrs = value;
            }
        }
    }
}
