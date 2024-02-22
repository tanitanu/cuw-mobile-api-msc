using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable]
    public class MOBPaymentResponse : MOBResponse
    {
        public MOBPaymentResponse()
            : base()
        {
        }

        private MOBPaymentRequest request;
        private bool succeed;

        public MOBPaymentRequest Request
        {
            get { return this.request; }
            set
            {
                this.request = value;
            }
        }

        public bool Succeed
        {
            get { return this.succeed; }
            set
            {
                this.succeed = value;
            }
        }
    }
}
