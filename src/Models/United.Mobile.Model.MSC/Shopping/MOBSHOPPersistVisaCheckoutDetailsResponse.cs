using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable]
  public  class MOBSHOPPersistVisaCheckoutDetailsResponse : MOBResponse
    {
        private MOBSHOPPersistVisaCheckoutDetailsRequest request;
        public MOBSHOPPersistVisaCheckoutDetailsRequest Request
        {
            get { return request; }
            set { request = value; }
        }
        private MOBSHOPReservation reservation;
        public MOBSHOPReservation Reservation
        {
            get { return reservation; }
            set { this.reservation = value; }
        }
    }
}
