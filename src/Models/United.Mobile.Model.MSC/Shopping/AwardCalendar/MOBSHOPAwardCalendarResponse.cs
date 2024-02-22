using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping.AwardCalendar
{
    [Serializable()]
    public class MOBSHOPAwardCalendarResponse : MOBResponse
    {
        private MOBSHOPSelectTripRequest awardCalendarRequest;
        private string cartId = string.Empty;
        private string sessionID = string.Empty;
        private MOBSHOPAwardDynamicCalendar awardDynamicCalendar;
        private string callDurationText;

        public MOBSHOPSelectTripRequest AwardCalendarRequest
        {
            get
            {
                return this.awardCalendarRequest;
            }
            set
            {
                this.awardCalendarRequest = value;
            }
        }
        public string CartId
        {
            get { return cartId; }
            set { cartId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        public string SessionID
        {
            get
            {
                return this.sessionID;
            }
            set
            {
                this.sessionID = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }
        public MOBSHOPAwardDynamicCalendar AwardDynamicCalendar
        {
            get
            {
                return awardDynamicCalendar;
            }
            set
            {
                awardDynamicCalendar = value;
            }
        }
        public string CallDurationText
        {
            get { return callDurationText; }
            set { callDurationText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
     }
}
