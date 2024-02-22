using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.CancelReservation
{
    public class MOBModifyReservationRequest: MOBRequest
    {
        private string sessionId = string.Empty;
        private string mileagePlusNumber = string.Empty;
        private string hashPinCode = string.Empty;

        public string SessionId
        {
            get { return sessionId; }
            set { sessionId = value; }
        }

        public string HashPinCode { get { return this.hashPinCode; } set { this.hashPinCode = value; } }
        public string MileagePlusNumber
        {
            get { return mileagePlusNumber; }
            set { mileagePlusNumber = value; }
        }
    }
}
