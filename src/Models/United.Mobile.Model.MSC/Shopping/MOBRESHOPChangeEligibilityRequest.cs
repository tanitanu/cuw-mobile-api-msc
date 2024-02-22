using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.CancelReservation;

namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBRESHOPChangeEligibilityRequest : MOBModifyReservationRequest
    {
        private string sessionId = string.Empty;
        private string recordLocator;
        private string lastName;

        public string LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }


        public string RecordLocator
        {
            get { return recordLocator; }
            set { recordLocator = value; }
        }

        public string SessionId
        {
            get
            {
                return this.sessionId;
            }
            set
            {
                this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
