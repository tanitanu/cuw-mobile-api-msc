using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp
{
    [Serializable]
    public class MOBEmpBookingPassengerRequest : MOBEmpRequest
    {
        private string employeeId;
       // private string sessionId;
        private bool isCustomized;
        private string impersonateType;

        public string EmployeeId
        {
            get { return employeeId; }
            set { this.employeeId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        //public string SessionId
        //{
        //    get { return sessionId; }
        //    set { this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        //}

        public bool IsCustomized
        {
            get { return isCustomized; }
            set { this.isCustomized = value; }
        }

        public string ImpersonateType
        {
            get { return impersonateType; }
            set { this.impersonateType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
    
    }
}
