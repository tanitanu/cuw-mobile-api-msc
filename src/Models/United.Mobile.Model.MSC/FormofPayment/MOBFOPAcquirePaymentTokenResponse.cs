using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;

namespace United.Definition
{
    [Serializable()]
    public class MOBFOPAcquirePaymentTokenResponse : MOBShoppingResponse
    {
        [JsonIgnore()]
        public string ObjectName { get; set; } = "United.Definition.MOBFOPAcquirePaymentTokenResponse";
        private string cartId = string.Empty;
        private string token = string.Empty;
        private string cslSessionId = string.Empty;
       
        public string CartId
        {
            get { return cartId; }
            set { cartId = value; }
        }
        public string Token {
            get { return token; }
            set { token = value; }
        }
        public string CslSessionId
        {
            get { return cslSessionId; }
            set { cslSessionId = value; }
        }
    }
}
