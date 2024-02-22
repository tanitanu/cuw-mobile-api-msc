using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBClubDayPassPurchaseResponse : MOBResponse
    {
        public MOBClubDayPassPurchaseResponse()
            : base()
        {
        }

        private MOBClubDayPassPurchaseRequest request;
        private List<MOBClubDayPass> passes;

        public MOBClubDayPassPurchaseRequest Request
        {
            get
            {
                return this.request;
            }
            set
            {
                this.request = value;
            }
        }

        public List<MOBClubDayPass> Passes
        {
            get
            {
                return this.passes;
            }
            set
            {
                this.passes = value;
            }
        }
    }

    [Serializable()]
    public class MOBOTPPurchaseResponse : MOBResponse
    {
        public MOBOTPPurchaseResponse()
            : base()
        {
        }

        private MOBOTPPurchaseRequest request;
        private List<MOBClubDayPass> passes;
        private string sessionId;

        public MOBOTPPurchaseRequest Request
        {
            get
            {
                return this.request;
            }
            set
            {
                this.request = value;
            }
        }

        public List<MOBClubDayPass> Passes
        {
            get
            {
                return this.passes;
            }
            set
            {
                this.passes = value;
            }
        }

        public string SessionId
        {
            get { return sessionId; }
            set { sessionId = value; }
        }
    }
}
