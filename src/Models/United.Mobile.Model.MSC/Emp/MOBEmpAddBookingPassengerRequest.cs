using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp
{
    [Serializable]
    public class MOBEmpAddBookingPassengerRequest : MOBEmpRequest
    {
        private MOBEmpBookingPassengerExtended selectedPassenger = new MOBEmpBookingPassengerExtended();
        private MOBEmpTravelingWith travelingWith;
        private string impersonateType;

        public string ImpersonateType
        {
            get
            {
                return impersonateType;
            }
            set
            {
                impersonateType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBEmpBookingPassengerExtended SelectedPassenger
        {
            get
            {
                return selectedPassenger;
            }
            set
            {
                selectedPassenger = value;
            }
        }

        public MOBEmpTravelingWith TravelingWith
        {
            get
            {
                return travelingWith;
            }
            set
            {
                travelingWith = value;
            }
        }


    }
}
