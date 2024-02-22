using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBUberProductEstimatesResponse : MOBResponse
    {
        public MOBUberProductEstimatesResponse()
            : base()
        {
        }
        private List<MOBUberProductTimePriceEstimates> uberProductTimePriceEstimates;

        public List<MOBUberProductTimePriceEstimates> UberProductTimePriceEstimates
        {
            get { return this.uberProductTimePriceEstimates; }
            set
            {
                this.uberProductTimePriceEstimates = value;
            }
        }
    }
}
