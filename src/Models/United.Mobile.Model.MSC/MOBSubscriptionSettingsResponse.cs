using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBSubscriptionSettingsResponse : MOBResponse
    {
        private List<MOBSubscriptionSetting> settings;

        public List<MOBSubscriptionSetting> Settings
        {
            get { return this.settings; }
            set { this.settings = value; }
        }
    }
}
