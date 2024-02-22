using System;
using System.Collections.Generic;
using System.Text;

namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBContentScreen
    {
        private string pageTitle;
        private string header;
        private string body;
        private List<MOBContentDetails> contentDetails;
        private string footerMessage;
        private List<MOBActionButton> buttons;
        
        public string PageTitle
        {
            get
            {
                return pageTitle;
            }
            set
            {
                pageTitle = value;
            }
        }

        public string Header
        {
            get
            {
                return header;
            }
            set
            {
                header = value;
            }
        }

        public string Body
        {
            get
            {
                return body;
            }
            set
            {
                body = value;
            }
        }

        public List<MOBContentDetails> ContentDetails
        {
            get
            {
                return contentDetails;
            }
            set
            {
                contentDetails = value;
            }
        }

        public string FooterMessage
        {
            get
            {
                return footerMessage;
            }
            set
            {
                footerMessage = value;
            }
        }

        public List<MOBActionButton> Buttons
        {
            get
            {
                return buttons;
            }
            set
            {
                buttons = value;
            }
        }
    }

    [Serializable()]
    public class MOBActionButton
    {
        private string actionURL;

        private string actionText;
        private int rank;
        private bool isPrimary;
        private bool isEnabled;

        public bool IsEnabled
        {
            get { return isEnabled; }
            set { isEnabled = value; }
        }

        public bool IsPrimary
        {
            get { return isPrimary; }
            set { isPrimary = value; }
        }

        public int Rank
        {
            get { return rank; }
            set { rank = value; }
        }

        public string ActionText
        {
            get { return actionText; }
            set { actionText = value; }
        }

        public string ActionURL
        {
            get { return actionURL; }
            set { actionURL = value; }
        }
    }
}
