using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBProductOffersFlight : MOBSHOPFlight
    {
        //private string segID = string.Empty;
        //private int segNumber;
        private List<MOBSHOPTraveler> travelers;
        private List<MOBSHOPProductOffersPrice> offers;

        //public string SegID
        //{
        //    get
        //    {
        //        return this.segID;
        //    }
        //    set
        //    {
        //        this.segID = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        //    }
        //}

        //public int SegmentNumber
        //{
        //    get
        //    {
        //        return this.segNumber;
        //    }
        //    set
        //    {
        //        this.segNumber = value;
        //    }
        //}

        public List<MOBSHOPTraveler> Travelers
        {
            get
            {
                return this.travelers;
            }
            set
            {
                this.travelers = value;
            }
        }

        public List<MOBSHOPProductOffersPrice> Offers
        {
            get
            {
                return this.offers;
            }
            set
            {
                this.offers = value;
            }
        }
    }
}
