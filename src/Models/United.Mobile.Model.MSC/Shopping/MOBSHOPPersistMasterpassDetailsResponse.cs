using System;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBSHOPPersistMasterpassDetailsResponse : MOBResponse
    {
        private MOBSHOPPersistMasterpassDetailsRequest request;
        public MOBSHOPPersistMasterpassDetailsRequest Request
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
