using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class AirportAdvisoryMessage
    {
        public AirportAdvisoryMessage()
        {
        }
        private string buttonTitle;
        private string headerTitle;
        private List<MessageTypeOption> advisoryMessages;

        public string ButtonTitle
        {
            get
            {
                return this.buttonTitle;
            }
            set
            {
                this.buttonTitle = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string HeaderTitle
        {
            get
            {
                return this.headerTitle;
            }
            set
            {
                this.headerTitle = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MessageTypeOption> AdvisoryMessages
        {
            get
            {
                return this.advisoryMessages;
            }
            set
            {
                this.advisoryMessages = value;
            }
        }
    }
}
