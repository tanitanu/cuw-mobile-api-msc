using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class CPBillingCountry
    {
        private string id;
        private string countryCode;
        private string countryName;
        private bool isStateRequired;
        private bool isZipRequired;

        public string Id { get { return id; } set { id = value; } }
        public string CountryCode { get { return countryCode; } set { countryCode = value; } }
        public string CountryName { get { return countryName; } set { countryName = value; } }
        public bool IsStateRequired { get { return isStateRequired; } set { isStateRequired = value; } }
        public bool IsZipRequired { get { return isZipRequired; } set { isZipRequired = value; } }
    }
}
