using System;

namespace United.Definition.CancelReservation
{
    [Serializable]
    public class MOBCancelAndRefundReservationResponse : MOBModifyReservationResponse
    {
        private string pnr;
        private string email;
        private string customerServicePhoneNumber;

        public string Pnr
        {
            get { return pnr; }
            set { pnr = value; }
        }

        public string Email
        {
            get { return email; }
            set { email = value; }
        }

        public string CustomerServicePhoneNumber
        {
            get { return customerServicePhoneNumber; }
            set { customerServicePhoneNumber = value; }
        }
    }
}
