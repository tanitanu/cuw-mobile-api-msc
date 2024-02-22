using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBPlacePass
    {
        private string placePassImageSrc;
        private string offerDescription;
        private string placePassUrl;
        private string txtPoweredBy;
        private string txtPlacepass;

        public string TxtPoweredBy
        {
            get { return this.txtPoweredBy; }
            set { this.txtPoweredBy = value; }
        }

        public string TxtPlacepass
        {
            get { return this.txtPlacepass; }
            set { this.txtPlacepass = value; }
        }
        public string PlacePassImageSrc
        {
            get
            {
                return this.placePassImageSrc;
            }
            set
            {
                this.placePassImageSrc = value;
            }
        }

        public string OfferDescription
        {
            get
            {
                return this.offerDescription;
            }
            set
            {
                this.offerDescription = value;
            }
        }
        public string PlacePassUrl
        {
            get
            {
                return this.placePassUrl;
            }
            set
            {
                this.placePassUrl = value;
            }
        }
    }
}
