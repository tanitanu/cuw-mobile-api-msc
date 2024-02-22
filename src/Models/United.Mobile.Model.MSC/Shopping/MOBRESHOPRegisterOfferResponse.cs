using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;

namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBRESHOPRegisterOfferResponse : MOBRegisterOfferResponse
    {
        private MOBSHOPReservation reservation = new MOBSHOPReservation();
        private List<MOBTypeOption> disclaimer;
        public MOBSHOPReservation Reservation
        {
            get { return reservation; }
            set { reservation = value; }
        }
        public List<MOBTypeOption> Disclaimer
        {
            get { return disclaimer; }
            set { disclaimer = value; }
        }
    }
}
