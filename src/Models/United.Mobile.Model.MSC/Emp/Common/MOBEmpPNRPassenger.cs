using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp.Common
{
    [Serializable()]
    public class MOBEmpPNRPassenger
    {
        private MOBEmpName passengerName;
        private string sharesPosition = string.Empty;
        private MOBEmpCPMileagePlus mileagePlus;
        private List<MOBEmpRewardProgram> oaRewardPrograms;

        public MOBEmpName PassengerName
        {
            get
            {
                return this.passengerName;
            }
            set
            {
                this.passengerName = value;
            }
        }

        public string SHARESPosition
        {
            get
            {
                return this.sharesPosition;
            }
            set
            {
                this.sharesPosition = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBEmpCPMileagePlus MileagePlus
        {
            get
            {
                return this.mileagePlus;
            }
            set
            {
                this.mileagePlus = value;
            }
        }

        public List<MOBEmpRewardProgram> OaRewardPrograms
        {
            get
            {
                return this.oaRewardPrograms;
            }
            set
            {
                this.oaRewardPrograms = value;
            }
        }
    }
}
