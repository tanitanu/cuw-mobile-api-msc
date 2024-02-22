using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBSHOPFareWheelResponse : MOBResponse
    {
        private MOBSHOPSelectTripRequest fareWheelRequest;
        private string cartId = string.Empty;
        List<MOBSHOPFareWheelItem> fareWheel;
        private string callDurationText;
        private bool disablePricingBySlice;
        private List<MOBFSRAlertMessage> fsrAlertMessages;
        private int numberPeopleViewingFlights;
        public MOBSHOPSelectTripRequest FareWheelRequest
        {
            get
            {
                return this.fareWheelRequest;
            }
            set
            {
                this.fareWheelRequest = value;
            }
        }
        public string CartId
        {
            get { return cartId; }
            set { cartId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        public List<MOBSHOPFareWheelItem> FareWheel
        {
            get
            {
                return this.fareWheel;
            }
            set
            {
                this.fareWheel = value;
            }
        }
        public string CallDurationText
        {
            get { return callDurationText; }
            set { callDurationText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        public bool DisablePricingBySlice
        {
            get
            {
                return this.disablePricingBySlice;
            }
            set
            {
                this.disablePricingBySlice = value;
            }
        }
        public List<MOBFSRAlertMessage> FSRAlertMessages
        {
            get { return fsrAlertMessages; }
            set { fsrAlertMessages = value; }
        }

        public int NumberPeopleViewingFlights
        {
            get { return numberPeopleViewingFlights; }
            set { numberPeopleViewingFlights = value; }
        }
    }
}
