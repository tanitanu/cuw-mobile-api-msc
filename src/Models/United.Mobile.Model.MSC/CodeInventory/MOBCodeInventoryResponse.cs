using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Flifo;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable]
    public class MOBCodeInventoryResponse : MOBResponse
    {
        public MOBCodeInventoryResponse()
        { }

        private MOBCarriersDetailsResponse carriersDetailsResponse;

        public MOBCarriersDetailsResponse CarriersDetailsResponse
        {
            get { return carriersDetailsResponse; }
            set { carriersDetailsResponse = value; }
        }
    }

    
}
