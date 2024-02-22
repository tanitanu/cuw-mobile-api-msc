using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;
using United.Definition.Shopping.Bundles;

namespace United.Definition
{
    [Serializable()]
    public class MOBBookingRegisterOfferResponse : MOBRegisterOfferResponse
    {
        private MOBSHOPReservation reservation = new MOBSHOPReservation();
        private List<MOBTypeOption> disclaimer;
        private MOBSection promoCodeRemoveAlertForProducts;

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

        public MOBSection PromoCodeRemoveAlertForProducts
        {
            get { return promoCodeRemoveAlertForProducts; }
            set { promoCodeRemoveAlertForProducts = value; }
        }
        private MOBBookingBundlesResponse bundleResponse;

        public MOBBookingBundlesResponse BundleResponse
        {
            get { return bundleResponse; }
            set { bundleResponse = value; }
        }

    }
}
