using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp
{
    [Serializable]
    public class MOBEmployeeSession
    {
        private string sessionId = string.Empty;
        private string mpNumber = string.Empty;
        private string employeeId = string.Empty;
        private string eResTransactionId = string.Empty;
        private string eResSessionId = string.Empty;
        private bool isPayrollDeduct;


        public bool IsPayrollDeduct
        {
            get
            {
                return this.isPayrollDeduct;
            }
            set
            {
                this.isPayrollDeduct = value;
            }
        }

        public string SessionId
        {
            get
            {
                return this.sessionId;
            }
            set
            {
                this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string MPNumber
        {
            get
            {
                return this.mpNumber;
            }
            set
            {
                this.mpNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string EmployeeId
        {
            get
            {
                return this.employeeId;
            }
            set
            {
                this.employeeId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string EResTransactionId
        {
            get
            {
                return this.eResTransactionId;
            }
            set
            {
                this.eResTransactionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string EResSessionId
        {
            get
            {
                return this.eResSessionId;
            }
            set
            {
                this.eResSessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
