using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class CorporateUnenrolledTravelerMsg
    {
        private string title;
        public string Title
        {
            get { return title; }
            set { title = value; }
        }
        private string header;
        public string Header
        {
            get { return header; }
            set { header = value; }
        }
        private string body;
        public string Body
        {
            get { return body; }
            set { body = value; }
        }
        private List<MOBButton> buttons;
        public List<MOBButton> Buttons
        {
            get { return buttons; }
            set { buttons = value; }
        }
    }
    [Serializable()]
    public class MOBButton
    {
        private string actionText;
        private string buttonText;
        private bool isPrimary;
        private bool isEnabled;
        private int rank;

        public string ActionText
        {
            get { return actionText; }
            set { actionText = value; }
        }
        public string ButtonText
        {
            get { return buttonText; }
            set { buttonText = value; }
        }
        public bool IsPrimary
        {
            get { return isPrimary; }
            set { isPrimary = value; }
        }
        public bool IsEnabled
        {
            get { return isEnabled; }
            set { isEnabled = value; }
        }

        public int Rank
        {
            get { return rank; }
            set { rank = value; }
        }

    }
}
