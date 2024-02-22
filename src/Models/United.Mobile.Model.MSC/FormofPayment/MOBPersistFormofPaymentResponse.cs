using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;

namespace United.Definition
{
    [Serializable()]
    public class MOBPersistFormofPaymentResponse : MOBShoppingResponse
    {
        [JsonIgnore()]
        public string ObjectName { get; set; } = "United.Definition.MOBPersistFormofPaymentResponse";
        private MOBShoppingCart shoppingCart;
        private List<FormofPaymentOption> eligibleFormofPayments;
        private string pkDispenserPublicKey;
        private MOBSHOPReservation reservation;
        private List<MOBCPProfile> profiles;

        public MOBShoppingCart ShoppingCart
        {
            get { return shoppingCart; }
            set { shoppingCart = value; }
        }
        public List<FormofPaymentOption> EligibleFormofPayments
        {
            get { return eligibleFormofPayments; }
            set { eligibleFormofPayments = value; }
        }
        public string PkDispenserPublicKey
        {
            get { return pkDispenserPublicKey; }
            set { pkDispenserPublicKey = value; }
        }
        public MOBSHOPReservation Reservation
        {
            get { return reservation; }
            set { this.reservation = value; }
        }
        public List<MOBCPProfile> Profiles
        {
            get
            {
                return profiles;
            }
            set
            {
                this.profiles = value;
            }
        }
    }
}
