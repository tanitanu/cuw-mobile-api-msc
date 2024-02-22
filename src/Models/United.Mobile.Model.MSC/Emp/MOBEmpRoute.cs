using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp
{
    [Serializable]
    public class MOBEmpRoute
    {
        private List<MOBEmpSegment> segments;
        private string tripNumber;

        public List<MOBEmpSegment> Segments
        {
            get { return this.segments; }
            set { this.segments = value; }
        }

        public string TripNumber
        {
            get { return this.tripNumber; }
            set { this.tripNumber = string.IsNullOrEmpty(value) ? Guid.NewGuid().ToString() : value.Trim(); }
        }
    }
}
