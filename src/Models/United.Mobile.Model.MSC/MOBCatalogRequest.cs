using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;

namespace United.Definition
{
    [Serializable()]
    public class MOBCatalogRequest : MOBRequest
    {
        public MOBCatalogRequest()
            : base()
        {
        }

        private string mileagePlusNumber;
        private string hashPinCode;

        public string MileagePlusNumber
        {
            get { return mileagePlusNumber;}
            set { mileagePlusNumber = value; }

        }
        public string HashPinCode
        {
            get { return hashPinCode; }
            set { hashPinCode = value; }
        }
    }
}
