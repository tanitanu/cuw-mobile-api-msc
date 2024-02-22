using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Messaging
{
    [Serializable]
    public class Message
    {
        private string id = string.Empty;
        private string source = string.Empty;
        private string messagetype = string.Empty;
        private string deviceType = string.Empty;
        private string deviceId = string.Empty;
        private Trigger trigger;
        private List<KeyValue> details;
        private string timestamp  = string.Empty;

        public string Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string Source
        {
            get
            {
                return this.source;
            }
            set
            {
                this.source = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string Messagetype
        {
            get
            {
                return this.messagetype;
            }
            set
            {
                this.messagetype = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string DeviceType
        {
            get
            {
                return this.deviceType;
            }
            set
            {
                this.deviceType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string DeviceId
        {
            get
            {
                return this.deviceId;
            }
            set
            {
                this.deviceId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public Trigger Trigger
        {
            get
            {
                return this.trigger;
            }
            set
            {
                this.trigger = value;
            }
        }

        public List<KeyValue> Details
        {
            get
            {
                return this.details;
            }
            set
            {
                this.details = value;
            }
        }

        public string Timestamp
        {
            get
            {
                return this.timestamp;
            }
            set
            {
                this.timestamp = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
