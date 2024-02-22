using System;
using System.Collections.Generic;
using System.Text;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBCarbonEmissionsResponse : MOBResponse
    {
        private List<MOBCarbonEmissionData> carbonEmissionData;

        public List<MOBCarbonEmissionData> CarbonEmissionData
        {
            get { return carbonEmissionData; }
            set { carbonEmissionData = value; }
        }
    }
}
