using System;
using United.Definition.FormofPayment.TravelCredit;

namespace United.Definition
{
    [Serializable()]
    public class MOBFOPManageTravelCreditRequest : MOBShoppingRequest
    {
        private MOBFOPTravelCredit travelCredit;
        private bool isRemove = false;

        public MOBFOPTravelCredit TravelCredit { get { return travelCredit; } set { travelCredit = value; } }
        public bool IsRemove { get { return isRemove; } set { isRemove = value; } }
    }
}
