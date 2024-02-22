using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp
{
    
    public class MOBEmpAddPassengerCompleteRequest : MOBEmpRequest
    {
        private List<MOBEmpBookingPassengerExtended> selectedPassengers = new List<MOBEmpBookingPassengerExtended>();
        private MOBEmpTravelingWith travelingWith;
        private string impersonateType;
        private string clientIp;

        public string ClientIP
        {
            get
            {
                return clientIp;
            }
            set
            {
                clientIp = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
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

        public List<MOBEmpBookingPassengerExtended> SelectedPassengers
        {
            get
            {
                return selectedPassengers;
            }
            set
            {
                selectedPassengers = value;
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
