using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Emp
{
    [Serializable()]
    public class MOBEmpStandByListPassengers : MOBRequest
    {
        private string firstName;
        private string middleName;
        private string lastName;
        private bool hasCheckedIn;
        private string passengerNumber;
        private short numberOfPassengers;
        private string passClass;
        private string cabin;
        private string position;
        private string employeeId;
        private string isSpecial;

        private string displayName;

        public string DisplayName
        {
            get { return displayName; }
            set { this.displayName = value; }
        }

        public string IsSpecial
        {
            get { return isSpecial; }
            set { this.isSpecial = value; }
        }

        public string FirstName
        {
            get { return firstName; }
            set { this.firstName = value; }
        }
        public string MiddleName
        {
            get { return middleName; }
            set { this.middleName = value; }
        }

        public string LastName
        {
            get { return lastName; }
            set { this.lastName = value; }
        }

        
        public bool HasCheckedIn
        {
            get { return this.hasCheckedIn; }
            set { this.hasCheckedIn = value; }
        }

        public string PassengerNumber
        {
            get { return this.passengerNumber; }
            set { this.passengerNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public short NumberOfPassengers
        {
            get { return this.numberOfPassengers; }
            set { this.numberOfPassengers = value; }
        }

        public string PassClass
        {
            get { return this.passClass; }
            set { this.passClass = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string Cabin
        {
            get { return this.cabin; }
            set { this.cabin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string Position
        {
            get { return this.position; }
            set { this.position = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string EmployeeID
        {
            get { return this.employeeId; }
            set { this.employeeId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
     }


}
