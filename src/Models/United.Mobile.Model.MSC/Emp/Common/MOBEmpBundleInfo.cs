using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp.Common
{
    [Serializable()]
    public class MOBEmpBundleInfo
    {
        private List<MOBEmpBundleFlightSegment> flightSegments;
        private string errorMessage = string.Empty;

        public List<MOBEmpBundleFlightSegment> FlightSegments
        {
            get { return this.flightSegments; }
            set { this.flightSegments = value; }
        }

        public string ErrorMessage
        {
            get { return this.errorMessage; }
            set { this.errorMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
    }
}
