using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp
{
    [Serializable]
    public class MOBEmpChangeSearchDateRequest : MOBEmpRequest
    {
        private string changeDays;
        private string tripIndex;

        public string ChangeDays
        {
            get
            { return changeDays; }
            set
            {
                this.changeDays = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string TripIndex
        {
            get
            { return tripIndex; }
            set
            {
                this.tripIndex = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
