using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBFlightStatusPushNotificationDetailRequest : MOBRequest
    {
        private string notificationId = string.Empty;

        public MOBFlightStatusPushNotificationDetailRequest()
            : base()
        {
        }

        public string NotificationId
        {
            get
            {
                return this.notificationId;
            }
            set
            {
                this.notificationId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

    }
}
