using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBFlightStatusPushNotificationDetailResponse : MOBResponse
    {
        private string notificationId = string.Empty;
        private MOBNotification notification;

        public MOBFlightStatusPushNotificationDetailResponse()
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

        public MOBNotification Notification
        {
            get
            {
                return this.notification;
            }
            set
            {
                this.notification = value;
            }
        }
    }
}
