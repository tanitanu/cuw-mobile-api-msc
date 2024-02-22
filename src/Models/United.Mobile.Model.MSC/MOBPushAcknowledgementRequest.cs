using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    public class MOBPushAcknowledgementRequest : MOBRequest
    {
        private string pushEventType = string.Empty;
        private long pushId;

        public string PushEventType
        {
            get
            {
                return this.pushEventType;
            }
            set
            {
                this.pushEventType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public long PushId
        {
            get
            {
                return this.pushId;
            }
            set
            {
                this.pushId = value;
            }
        }
    }
}
