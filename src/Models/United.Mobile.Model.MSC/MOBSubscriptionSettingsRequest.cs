using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBSubscriptionSettingsRequest : MOBRequest
    {
        private string pushToken = string.Empty;
        private bool pushEnabled;
        private List<MOBSubscriptionSetting> subscriptionSettings;


        public string PushToken
        {
            get
            {
                return this.pushToken;
            }
            set
            {
                this.pushToken = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool PushEnabled
        {
            get
            {
                return this.pushEnabled;
            }
            set
            {
                this.pushEnabled = value;
            }
        }

        public List<MOBSubscriptionSetting> SubscriptionSettings
        {
            get
            {
                return this.subscriptionSettings;
            }
            set
            {
                this.subscriptionSettings = value;
            }
        }

    }
}
