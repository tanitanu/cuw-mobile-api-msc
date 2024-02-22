using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBFlightConfirmTravelerRequest : MOBRequest
    {
        public MOBFlightConfirmTravelerRequest()
            : base()
        {
        }

        private string sessionId = string.Empty;
        private MOBBookingTravelerInfo bookingTravelerInfo;
        private string profileOption = string.Empty;
        private string secureTravelerOption = string.Empty;
        private string skipSeats = string.Empty;

        public string SessionId
        {
            get { return this.sessionId; }
            set { this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public MOBBookingTravelerInfo BookingTravlerInfo
        {
            get { return this.bookingTravelerInfo; }
            set { this.bookingTravelerInfo = value; }
        }

        public string ProfileOption
        {
            get { return this.profileOption; }
            set { this.profileOption = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string SecureTravelerOption
        {
            get { return this.secureTravelerOption; }
            set { this.secureTravelerOption = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string SkipSeats
        {
            get { return this.skipSeats; }
            set { this.skipSeats = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
    }
}
