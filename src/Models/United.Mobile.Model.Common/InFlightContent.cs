using System;
using System.Collections.Generic;
using System.Text;
using United.Definition;

namespace United.Mobile.Model.Common
{
    public class InFlightContent<T>
    {
        public string id { get; set; }
        public string contentKey { get; set; }
        public string featureKey { get; set; }
        public string expirationDateTime { get; set; }
        public string updatedDateTime { get; set; }
        public string updatedBy { get; set; }

        public string content { get; set; }
        public SDLKeyValuePairContentResponse sdlcontent { get; set; }
        public CPBillingCountry billingCountry { get; set; }


    }
}
