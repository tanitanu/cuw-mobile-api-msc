using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.FormofPayment
{
    [Serializable()]
    public class MOBFutureFlightCreditRequest : MOBShoppingRequest
    {
        private string recordLocator;
        private string lastName;        
        private string email;
        private string mileagePlusNumber = string.Empty;
        private string hashPinCode = string.Empty;

        public string Email{ get { return email; } set { email = value; } }       
        public string RecordLocator { get { return recordLocator; } set { recordLocator = value; } }
        public string LastName { get { return lastName; } set { lastName = value; }}
        public string HashPinCode { get { return this.hashPinCode; } set { this.hashPinCode = value; } }
        public string MileagePlusNumber { get { return mileagePlusNumber; } set { mileagePlusNumber = value; } }

    }

}
