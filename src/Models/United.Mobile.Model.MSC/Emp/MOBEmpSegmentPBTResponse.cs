using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Emp;
using United.Mobile.Model.Common;
namespace United.Definition.Emp
{
    [Serializable()]
    public class MOBEmpSegmentPBTResponse : MOBResponse
    {
        private List<MOBEmpSegmentPBT> mobEmpSegmentPBTs;
        private MOBEmpFlightPBTRequest mobEmpFlightPBTRequest;

        public List<MOBEmpSegmentPBT> MOBEmpSegmentPBTs
        {
            get { return this.mobEmpSegmentPBTs; }
            set { this.mobEmpSegmentPBTs = value; }
        }
        public MOBEmpFlightPBTRequest MOBEmpFlightPBTRequest
        {
            get { return this.mobEmpFlightPBTRequest; }
            set { this.mobEmpFlightPBTRequest = value; }
        }
    }
}
