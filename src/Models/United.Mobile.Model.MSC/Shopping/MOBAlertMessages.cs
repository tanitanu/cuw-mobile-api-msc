using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBAlertMessages
    {
        private string headerMessage;

        public string HeaderMessage
        {
            get { return headerMessage; }
            set { headerMessage = value; }
        }
        private List<MOBSection> alertMessages;

        public List<MOBSection> AlertMessages
        {
            get
            {
                return alertMessages;
            }
            set
            {
                alertMessages = value;
            }
        }

        private bool isDefaultOption;

        public bool IsDefaultOption
        {
            get { return isDefaultOption; }
            set { isDefaultOption = value; }
        }

        private string messageType;

        public string MessageType
        {
            get { return messageType; }
            set { messageType = value; }
        }

    }
}
