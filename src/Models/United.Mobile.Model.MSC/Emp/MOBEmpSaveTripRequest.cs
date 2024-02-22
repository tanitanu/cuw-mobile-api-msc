using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp
{
    [Serializable]
    public class MOBEmpSaveTripRequest : MOBEmpRequest
    {
        private int tripIndex;
        private int segmentIndex;
        private string change;
        private string impersonateType;
        private bool refresh;

        public bool Refresh
        {
            get { return refresh; }
            set { refresh = value; }
        }

        public int TripIndex
        {
            get { return tripIndex; }
            set { tripIndex = value; }
        }

        public int SegmentIndex
        {
            get
            { return segmentIndex; }
            set
            { segmentIndex = value; }
        }
        public string Change
        {
            get
            { return change; }
            set
            { change = value; }
        }
        public string ImpersonateType
        {
            get
            { return impersonateType; }
            set
            { impersonateType = value; }
        }
    }
}
