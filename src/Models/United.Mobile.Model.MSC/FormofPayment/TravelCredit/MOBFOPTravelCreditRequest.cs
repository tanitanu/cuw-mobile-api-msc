using System;


namespace United.Definition
{
	[Serializable()]
	public class MOBFOPTravelCreditRequest : MOBShoppingRequest
	{        
        private string lastName;
        private string pinOrPnr;             
        private bool isRemove = false;
        private string mileagePlusNumber = string.Empty;
        

        public string LastName { get { return lastName; } set { lastName = value; } }
        public string PinOrPnr { get { return pinOrPnr; } set { pinOrPnr = value; } }        
        public string MileagePlusNumber { get { return mileagePlusNumber; } set { mileagePlusNumber = value; } }
        public bool IsRemove { get { return isRemove; } set { isRemove = value; } }
	}
}
