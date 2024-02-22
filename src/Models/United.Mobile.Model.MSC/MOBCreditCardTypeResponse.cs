using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBCreditCardTypeResponse: MOBResponse 
    {
        private string sessionId;
        private string cardCode;

        public string CardCode
        {
            get
            {
                return this.cardCode;
            }
            set
            {
                this.cardCode = value;
            }
        }
        public string SessionId
        {
            get { return sessionId; }
            set { sessionId = value; }
        }
    }
}
