using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBLogRequest : MOBRequest
    {
        private string action = string.Empty;
        private string messageType = string.Empty;
        private string message = string.Empty;

        public string Action
        {
            get { return action; }
            set { action = value; }
        }
        public string MessageType
        {
            get { return messageType; }
            set { messageType = value; }
        }
        public string Message
        {
            get { return message; }
            set { message = value; }
        }
    }
}
