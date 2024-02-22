using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBFlightStatusPushNotificationUnsubscribeResponse : MOBResponse
    {
        private string id = string.Empty;
        private string succeed = string.Empty;

        public MOBFlightStatusPushNotificationUnsubscribeResponse()
            : base()
        {
        }

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



        public string Succeed
        {
            get
            {
                return this.succeed;
            }
            set
            {
                this.succeed = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
    }
}
