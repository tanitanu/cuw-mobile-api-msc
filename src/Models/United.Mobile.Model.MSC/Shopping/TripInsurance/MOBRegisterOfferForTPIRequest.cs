using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping.TripInsurance
{
    [Serializable]
    public class MOBRegisterOfferForTPIRequest : MOBRequest
    {
        private string sessionId = string.Empty;
        private bool isRegisterOffer;
        private bool isReQuote = false;
        private bool isAcceptChanges = false;

        public string SessionId
        {
            get
            {
                return this.sessionId;
            }
            set
            {
                this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool IsRegisterOffer
        {
            get
            {
                return this.isRegisterOffer;
            }
            set
            {
                this.isRegisterOffer = value;
            }
        }

        public bool IsReQuote
        {
            get
            {
                return this.isReQuote;
            }
            set
            {
                this.isReQuote = value;
            }
        }
        public bool IsAcceptChanges
        {
            get
            {
                return this.isAcceptChanges;
            }
            set
            {
                this.isAcceptChanges = value;
            }
        }
    }
}
