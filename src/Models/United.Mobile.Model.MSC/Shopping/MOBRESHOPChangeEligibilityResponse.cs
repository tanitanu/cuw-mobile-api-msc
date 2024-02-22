using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBRESHOPChangeEligibilityResponse : MOBResponse
    {
        private bool pathEligible;
        public bool PathEligible
        {
            get { return pathEligible; }
            set { pathEligible = value; }
        }

        private string failedRule;

        public string FailedRule
        {
            get { return failedRule; }
            set { failedRule = value; }
        }

        private string sessionId;
        public string SessionId
        {
            get { return sessionId; }
            set { sessionId = value; }
        }

        private string redirectURL = string.Empty;
        public string RedirectURL
        {
            get { return redirectURL; }
            set { redirectURL = value; }
        }

        private bool awardTravel = false;
        public bool AwardTravel
        {
            get { return awardTravel; }
            set { awardTravel = value; }
        }

        private string sponsorMileagePlus = string.Empty;
        public string SponsorMileagePlus
        {
            get { return sponsorMileagePlus; }
            set { sponsorMileagePlus = value; }
        }

        private string webShareToken = string.Empty;
        private string webSessionShareUrl = string.Empty;

        public string WebShareToken { get { return this.webShareToken; } set { this.webShareToken = value; } }
        public string WebSessionShareUrl { get { return this.webSessionShareUrl; } set { this.webSessionShareUrl = value; } }

        private MOBSHOPReservation reservation;
        public MOBSHOPReservation Reservation
        {
            get
            {
                return this.reservation;
            }
            set
            {
                this.reservation = value;
            }
        }

        private MOBSHOPResponseStatusItem responseStatusItem;

        public MOBSHOPResponseStatusItem ResponseStatusItem
        {
            get { return responseStatusItem; }
            set { responseStatusItem = value; }
        }

        private List<MOBPNRPassenger> pnrTravelers = null;
        
        public List<MOBPNRPassenger> PnrTravelers
        {
            get { return pnrTravelers; }
            set { pnrTravelers = value; }
        }

        private List<MOBPNRSegment> pnrSegment = null;

        public List<MOBPNRSegment> PnrSegments
        {
            get { return pnrSegment; }
            set { pnrSegment = value; }
        }

        private bool exceptionPolicyEligible;
        public bool ExceptionPolicyEligible
        {
            get { return exceptionPolicyEligible; }
            set { exceptionPolicyEligible = value; }
        }

        private bool sameDayChangeEligible;
        public bool SameDayChangeEligible
        {
            get { return sameDayChangeEligible; } 
            set { sameDayChangeEligible = value; }
        }

        //Book With Travel Credit Session
        private string bwcsessionId = string.Empty;
        public string BWCSessionId
        {
            get
            {
                return this.bwcsessionId;
            }
            set
            {
                this.bwcsessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
