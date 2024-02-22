using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBFlightStatusPushNotificationUnsubscribeRequest : MOBRequest
    {
        private string id = string.Empty;

        public MOBFlightStatusPushNotificationUnsubscribeRequest()
            : base()
        { }

        public string Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
