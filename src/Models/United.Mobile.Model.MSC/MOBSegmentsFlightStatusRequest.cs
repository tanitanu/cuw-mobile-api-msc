using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBSegmentsFlightStatusRequest : MOBRequest
    {
        private string segmentParameters = string.Empty;

        public string SegmentParameters
        {
            get
            {
                return this.segmentParameters;
            }
            set
            {
                this.segmentParameters = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
