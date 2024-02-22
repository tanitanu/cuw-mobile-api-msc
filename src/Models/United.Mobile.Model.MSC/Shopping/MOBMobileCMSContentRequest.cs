using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBMobileCMSContentRequest : MOBRequest
    {        
        public MOBMobileCMSContentRequest()
            : base()
        {
        }
        private string sessionId = string.Empty;
        private string cartId = string.Empty;
        private string token = string.Empty;
        private string mileagePlusNumber = string.Empty;        
        private string groupName = string.Empty;
        private List<string> listNames = new List<string>();
        private bool getShopTnC = false;
        private string flow = string.Empty;

        public string SessionId
        {
            get
            {
                return sessionId;
            }
            set
            {
                this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string CartId
        {
            get
            {
                return this.cartId;
            }
            set
            {
                this.cartId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Token
        {
            get
            {
                return token;
            }
            set
            {
                this.token = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string MileagePlusNumber
        {
            get
            {
                return mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string GroupName
        {
            get
            {
                return groupName;
            }
            set
            {
                this.groupName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<string> ListNames
        {
            get
            {
                return listNames;
            }
            set
            {
                this.listNames =  value;
            }
        }

        public bool GetShopTnC
        {
            get
            {
                return getShopTnC;
            }
            set
            {
                this.getShopTnC = value;
            }
        }
        public string Flow
        {
            get { return flow; }
            set { flow = value; }
        }
    }
}
