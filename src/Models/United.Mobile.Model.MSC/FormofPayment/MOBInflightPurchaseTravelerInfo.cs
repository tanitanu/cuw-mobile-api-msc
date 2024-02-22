using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.FormofPayment
{
    [Serializable()]
    public  class MOBInflightPurchaseTravelerInfo
    {
        private string firstName;
        private string lastName;
        private bool hasSavedCreditCard;
        private string fqtvNumber;
        private string airline;
        private string employeeId;
        private string savedCardInfo;

        public string FirstName
        {
            get
            {
                return this.firstName;
            }
            set
            {
                this.firstName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string LastName
        {
            get
            {
                return this.lastName;
            }
            set
            {
                this.lastName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public bool HasSavedCreditCard
        {
            get
            {
                return this.hasSavedCreditCard;
            }
            set
            {
                this.hasSavedCreditCard = value;
            }
        }
        public string FqtvNumber
        {
            get
            {
                return this.fqtvNumber;
            }
            set
            {
                this.fqtvNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
        public string Airline
        {
            get
            {
                return this.airline;
            }
            set
            {
                this.airline = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
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
                this.employeeId = value;
            }
        }

        public string SavedCardInfo
        {
            get
            {
                return this.savedCardInfo;
            }
            set
            {
                this.savedCardInfo = value;
            }
        }
    }
}
