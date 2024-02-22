using System;
using System.Collections.Generic;

namespace United.Definition.Shopping
{

    public enum FSRAlertMessageType
    {
        None,
        SuggestNearbyAirports,
        NoResults,
        NonstopsSuggestion,
        RevenueLowestPriceForAwardSearch,
        CorporateLeisureOptOut,
        NoChangeFee,
        NoSeatsAvailable,
        PartnerResults
    }

    [Serializable()]
    public class MOBFSRAlertMessage
    {
        private string headerMsg;
        private string bodyMsg;
        private List<MOBFSRAlertMessageButton> buttons;       
        private FSRAlertMessageType messageTypeDescription = FSRAlertMessageType.None; // default to none
        private int messageType = 0; //default to 0
        private string restHandlerType;

        /// <summary>
        /// Header message
        /// </summary>
        public string HeaderMessage
        {
            get { return headerMsg; }
            set { headerMsg = value; }
        }

        /// <summary>
        /// Body message
        /// </summary>
        public string BodyMessage
        {
            get { return bodyMsg; }
            set { bodyMsg = value; }
        }

        /// <summary>
        /// List of buttons that need to be shown
        /// </summary>
        public List<MOBFSRAlertMessageButton> Buttons
        {
            get { return buttons; }
            set { buttons = value; }
        }

        /// <summary>
        /// The type on message (info/alert/etc)
        /// 0 for info per UI proposal
        /// </summary>
        public FSRAlertMessageType MessageTypeDescription
        {
            get { return messageTypeDescription; }
            set { messageTypeDescription = value; }
        }

        /// <summary>
        /// This is for REST debug purpose only
        /// </summary>
        public string RestHandlerType
        {
            get { return restHandlerType; }
            set { restHandlerType = value; }
        }       

        public int MessageType
        {
            get { return messageType; }
            set { messageType = value; }
        }

    }
}
