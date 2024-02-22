using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;

namespace United.Definition.FormofPayment
{
    [Serializable()]
    public class MOBReshopRegisterSeatsResponse : MOBRegisterSeatsResponse
    {
        private MOBSHOPReservation reservation = new MOBSHOPReservation();
        private List<MOBTypeOption> hazMat;
        private List<MOBTypeOption> disclaimer;
        private string contractOfCarriage = string.Empty;
        private string footerMessage = null;
        public MOBSHOPReservation Reservation
        {
            get { return reservation; }
            set { reservation = value; }
        }
        public List<MOBTypeOption> HazMat
        {
            get { return this.hazMat; }
            set { this.hazMat = value; }
        }

        public string FooterMessage
        {
            get { return footerMessage; }
            set { footerMessage = value; }
        }

        public List<MOBTypeOption> Disclaimer
        {
            get
            {
                return this.disclaimer;
            }
            set
            {
                this.disclaimer = value;
            }
        }

        public string ContractOfCarriage
        {
            get
            {
                return this.contractOfCarriage;
            }
            set
            {
                this.contractOfCarriage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
