using System;
using System.Collections.Generic;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping.UnfinishedBooking
{
    [Serializable]
    public class MOBSHOPGetUnfinishedBookingsResponse : MOBResponse
    {
        private List<MOBSHOPUnfinishedBooking> pricedUnfinishedBookingList;

        public List<MOBSHOPUnfinishedBooking> PricedUnfinishedBookingList
        {
            get { return pricedUnfinishedBookingList; }
            set { pricedUnfinishedBookingList = value; }
        }
    }
}
