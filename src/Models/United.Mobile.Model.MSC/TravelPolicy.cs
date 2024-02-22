using System;
using System.Collections.Generic;
using System.Text;
using United.Definition;

namespace United.Mobile.Model.MSC
{
    [Serializable]
    public class TravelPolicy
    {
        private string travelPolicyTitle;
        private string travelPolicyHeader;
        private string travelPolicyBody;
        private List<MOBSection> travelPolicyContent;
        private string travelPolicyFooterMessage;
        private List<string> travelPolicyButton;
        private string travelPolicyTitleForFSRLink;

        public string TravelPolicyTitle
        {
            get
            {
                return travelPolicyTitle;
            }
            set
            {
                travelPolicyTitle = value;
            }
        }

        public string TravelPolicyHeader
        {
            get
            {
                return travelPolicyHeader;
            }
            set
            {
                travelPolicyHeader = value;
            }
        }

        public string TravelPolicyBody
        {
            get
            {
                return travelPolicyBody;
            }
            set
            {
                travelPolicyBody = value;
            }
        }

        public List<MOBSection> TravelPolicyContent
        {
            get
            {
                return travelPolicyContent;
            }
            set
            {
                travelPolicyContent = value;
            }
        }

        public string TravelPolicyFooterMessage
        {
            get
            {
                return travelPolicyFooterMessage;
            }
            set
            {
                travelPolicyFooterMessage = value;
            }
        }

        public List<string> TravelPolicyButton
        {
            get
            {
                return travelPolicyButton;
            }
            set
            {
                travelPolicyButton = value;
            }
        }
        public string TravelPolicyTitleForFSRLink
        {
            get
            {
                return travelPolicyTitleForFSRLink;
            }
            set
            {
                travelPolicyTitleForFSRLink = value;
            }
        }
    }
}
