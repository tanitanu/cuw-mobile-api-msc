using System;
using System.Collections.Generic;
using System.Text;
using United.Definition.Shopping;

namespace United.Mobile.Model.MSC.Corporate
{
    [Serializable()]
    public class TravelPolicyWarningAlert
    {
        private TravelPolicy travelPolicy;
        private List<MOBInfoWarningMessages> infoWarningMessages;
        public List<MOBInfoWarningMessages> InfoWarningMessages
        {
            get { return infoWarningMessages; }
            set { infoWarningMessages = value; }
        }

        public TravelPolicy TravelPolicy
        {
            get
            {
                return travelPolicy;
            }
            set
            {
                travelPolicy = value;
            }
        }
    }
}
