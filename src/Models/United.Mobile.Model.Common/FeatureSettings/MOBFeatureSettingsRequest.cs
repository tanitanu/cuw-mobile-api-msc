using System;
using System.Collections.Generic;
using System.Text;
using United.Definition;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBFeatureSettingsRequest:MOBRequest
    {
        private string apiName;

        public string ApiName
        {
            get { return apiName; }
            set { apiName = value; }
        }

    }
}
