using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.CouchBase
{
    [Serializable]
    public class WebHookResponse : MOBResponse
    {
        private bool succeed;
        private MOBWalletRequest request;

        public bool Succeed
        {
            get
            {
                return succeed;
            }
            set
            {
                this.succeed = value;
            }
        }

        public MOBWalletRequest Request
        {
            get
            {
                return request;
            }
            set
            {
                this.request = value;
            }
        }
    }
}
