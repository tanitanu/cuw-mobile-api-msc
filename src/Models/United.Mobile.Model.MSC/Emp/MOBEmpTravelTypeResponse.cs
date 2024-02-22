using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Emp
{
    [Serializable]
    public class MOBEmpTravelTypeResponse : MOBResponse
    {

        private string eResTransactionId = string.Empty;
        private string sessionId = string.Empty;
        private MOBEmpTravelType empTravelType;
        private bool isPayrollDeduct;
        private int advanceBookingDays;
        private string displayEmployeeId = string.Empty;

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
        public MOBEmpTravelType EmpTravelType
        {
            get
            {
                return empTravelType;
            }
            set
            {
                empTravelType = value;
            }
        }
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

        public int AdvanceBookingDays
        {
            get
            {
                return this.advanceBookingDays;
            }
            set
            {
                this.advanceBookingDays = value;
            }
        }

        public string DisplayEmployeeId
        {
            get
            {
                return this.displayEmployeeId;
            }
            set
            {
                this.displayEmployeeId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
    }
}
