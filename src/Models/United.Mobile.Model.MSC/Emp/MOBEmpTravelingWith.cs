using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp
{
    [Serializable]
    public class MOBEmpTravelingWith
    {
        private string type;
        private string travelPlan;
        public string Type 
        {
            get
            {
                return this.type;
            }

            set
            {
                this.type = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
        public string TravelPlan 
        {
            get
            {
                return this.travelPlan;
            }

            set
            {
                this.travelPlan = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
