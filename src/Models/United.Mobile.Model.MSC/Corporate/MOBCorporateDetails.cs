using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace United.Definition.Corporate
{
    [Serializable]
    public class MOBCorporateDetails
    {

        private string discountCode;
        public string DiscountCode
        {
            get { return discountCode; }
            set { discountCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string corporateCompanyName;
        public string CorporateCompanyName
        {
            get { return corporateCompanyName; }
            set { corporateCompanyName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string corporateTravelProvider;
        public string CorporateTravelProvider
        {
            get { return corporateTravelProvider; }
            set { corporateTravelProvider = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string fareGroupId;

        public string FareGroupId
        {
            get { return fareGroupId;}
            set { fareGroupId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private bool isPersonalized;
        public bool IsPersonalized
        {
            get { return isPersonalized; }
            set { isPersonalized = value; }
        }

    }
}
