using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBPaxBag
    {
        private MOBAirport finalArrArpt;
        private string bagTagNbr = string.Empty;
        private string bagTagIssDt = string.Empty;
        private List<MOBBagTagCheckInDetail> bagCheckInDetail;

        public MOBAirport FinalArrArpt
        {
            get
            {
                return this.finalArrArpt;
            }
            set
            {
                this.finalArrArpt = value;
            }
        }

        public string BagTagNum
        {
            get
            {
                return this.bagTagNbr;
            }
            set
            {
                this.bagTagNbr = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        public List<MOBBagTagCheckInDetail> BagCheckInDetail
        {
            get
            {
                return this.bagCheckInDetail;
            }
            set
            {
                this.bagCheckInDetail = value;
            }
        }
    }
}
