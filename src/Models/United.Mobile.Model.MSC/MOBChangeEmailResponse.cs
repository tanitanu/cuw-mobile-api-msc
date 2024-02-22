using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable]
    public class MOBChangeEmailResponse : MOBResponse
    {
        private string sessionId = string.Empty;
        private MOBSHOPReservation reservation;
        private string flow = string.Empty;
        private MOBShoppingCart shoppingCart = new MOBShoppingCart();

        public MOBSHOPReservation Reservation
        {
            get
            {
                return this.reservation;
            }
            set
            {
                this.reservation = value;
            }
        }
        public string SessionId
        {
            get
            {
                return this.sessionId;
            }
            set
            {
                this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string Flow
        {
            get { return flow; }
            set { flow = value; }
        }

        public MOBShoppingCart ShoppingCart
        {
            get { return shoppingCart; }
            set { shoppingCart = value; }
        }
    }
}
