using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Flifo;
using United.Mobile.Model.Common;
namespace United.Definition
{
    public class MOBCodeInventoryRequest : MOBRequest
    {
        private string codeRequiredFor = string.Empty;

        public string CodeRequiredFor
        {
            get { return codeRequiredFor; }
            set { codeRequiredFor = value; }
        }

        private MOBCarriersDetailsRequest carriersDetailsRequest;

        public MOBCarriersDetailsRequest CarriersDetailsRequest
        {
            get { return carriersDetailsRequest; }
            set { carriersDetailsRequest = value; }
        }
    }
}
