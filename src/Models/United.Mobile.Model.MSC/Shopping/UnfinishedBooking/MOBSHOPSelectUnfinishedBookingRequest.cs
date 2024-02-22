using System;
using United.Persist;

namespace United.Definition.Shopping.UnfinishedBooking
{
    [Serializable]
    public class MOBSHOPSelectUnfinishedBookingRequest : MOBSHOPUnfinishedBookingRequestBase
    {
        private MOBSHOPUnfinishedBooking selectedUnfinishBooking;
        private bool isOmniCartSavedTrip;
        private string cartId;
        private string sessionId;
        public MOBSHOPUnfinishedBooking SelectedUnfinishBooking
        {
            get { return selectedUnfinishBooking; }
            set { selectedUnfinishBooking = value; }
        }
        public bool IsOmniCartSavedTrip
        {
            get { return isOmniCartSavedTrip; }
            set { isOmniCartSavedTrip = value; }
        }

        public string CartId
        {
            get { return cartId; }
            set { cartId = value; }
        }
        public string SessionId
        {
            get { return sessionId; }
            set { sessionId = value; }
        }
    }
}
