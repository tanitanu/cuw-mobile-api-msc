using System;
using System.Collections.Generic;
using System.Text;
using United.Definition;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBFeatureSettingsResponse:MOBResponse
    {
        private List<MOBFeatureSetting> featureSettings;

        public List<MOBFeatureSetting> FeatureSettings
        {
            get { return featureSettings; }
            set { featureSettings = value; }
        }
      
    }
}
