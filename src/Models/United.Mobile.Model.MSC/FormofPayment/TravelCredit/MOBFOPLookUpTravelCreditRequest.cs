using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBFOPLookUpTravelCreditRequest : MOBShoppingRequest
    {
        public string ObjectName { get; set; } = "United.Definition.MOBFOPLookUpTravelCreditRequest";

        private string lastName;
        private string pinOrPnr;
        private string hashPin = string.Empty;
        private string mileagePlusNumber = string.Empty;
        private string email = string.Empty;


        public string LastName { get { return lastName; } set { lastName = value; } }
        public string PinOrPnr { get { return pinOrPnr; } set { pinOrPnr = value; } }
        public string MileagePlusNumber { get { return mileagePlusNumber; } set { mileagePlusNumber = value; } }
        public string HashPin { get { return hashPin; } set { hashPin = value; } }
        public string Email { get { return email; } set { email = value; } }
    }
}
