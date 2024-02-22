using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using United.Definition.Shopping;
using United.Mobile.Model.Common;

namespace United.Definition
{
    [Serializable()]
    public class MOBRegisterUsingMilesRequest: MOBShoppingRequest
    {
        private string mileagePlusNumber = string.Empty;
        public string MileagePlusNumber
        {
            get
            {
                return this.mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        private string hashPinCode;
        public string HashPinCode
        {
            get { return hashPinCode; }
            set { hashPinCode = value; }
        }

        private string partnerRPCIds;
        public string PartnerRPCIds
        {
            get { return partnerRPCIds; }
            set { partnerRPCIds = value; }
        }

        private List<FOPProduct> productCodes;

        public List<FOPProduct> ProductCodes
        {
            get { return productCodes; }
            set { productCodes = value; }
        }

    }
    
}
