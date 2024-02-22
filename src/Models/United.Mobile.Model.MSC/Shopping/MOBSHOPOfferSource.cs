using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPOfferSource
    {
        private string offerHeaderDescription = string.Empty;
        private MOBSHOPClubDayPassOffer clubDayPassOffer;
        private MOBSHOPProductOffer productOffer;

        public string OfferHeaderDescription
        {
            get
            {
                return this.offerHeaderDescription;
            }
            set
            {
                this.offerHeaderDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBSHOPClubDayPassOffer ClubDayPassOffer
        {
            get
            {
                return this.clubDayPassOffer;
            }
            set
            {
                this.clubDayPassOffer = value;
            }
        }

        public MOBSHOPProductOffer ProductOffer
        {
            get
            {
                return this.productOffer;
            }
            set
            {
                this.productOffer = value;
            }
        }
    }
}
