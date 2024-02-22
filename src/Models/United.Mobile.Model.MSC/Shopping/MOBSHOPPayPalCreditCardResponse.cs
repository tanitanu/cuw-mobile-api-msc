using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBSHOPPayPalCreditCardResponse : MOBResponse
    {
        private MOBSHOPReservation reservation;
        private MOBSHOPPayPalCreditCardRequest request;

        public MOBSHOPPayPalCreditCardRequest Request
        {
            get { return request; }
            set { this.request = value; }
        }

      
        public MOBSHOPReservation Reservation
        {
            get { return reservation; }
            set { this.reservation = value; }
        }

    }
}
