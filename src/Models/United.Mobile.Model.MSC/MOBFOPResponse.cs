using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using United.Definition.Shopping;

namespace United.Definition
{
    [Serializable()]
    public class MOBFOPResponse : MOBShoppingResponse
    {
        [JsonIgnore()]
        public string ObjectName { get; set; } = "United.Definition.MOBFOPResponse";

        private List<MOBTypeOption> captions;
        private List<FormofPaymentOption> eligibleFormofPayments;
        private MOBShoppingCart shoppingCart = new MOBShoppingCart();
        private MOBSHOPReservation reservation;
        private List<MOBCPProfile> profiles;
        private string pkDispenserPublicKey;
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

        public List<MOBTypeOption> Captions
        {
            get { return this.captions; }
            set { this.captions = value; }
        }

        public List<FormofPaymentOption> EligibleFormofPayments
        {
            get { return eligibleFormofPayments; }
            set { eligibleFormofPayments = value; }
        }
        public MOBShoppingCart ShoppingCart
        {
            get { return shoppingCart; }
            set { shoppingCart = value; }
        }
        public List<MOBCPProfile> Profiles
        {
            get { return profiles; }
            set { this.profiles = value; }
        }

    }

}
