using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.PrePaidBags
{
    [Serializable()]
    public class MOBGetBaggageInformationRequest: MOBRequest
    {
        private string sessionId;
        private string recordLocator;
        private string lastName; // not sure if this should be used. will update after getting sample from Microsite
        private bool isBooking;

        public bool IsBooking { get { return isBooking; } set { isBooking = value; } }
        public string SessionId
        {
            get { return sessionId; }
            set { sessionId = value; }
        }
        public string RecordLocator
        {
            get { return recordLocator; }
            set { recordLocator = value; }
        }
        public string LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }
    }
}
