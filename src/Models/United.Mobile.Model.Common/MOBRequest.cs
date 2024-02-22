using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBRequest
    {
        public MOBRequest()
            : base()
        {
            string tmp = string.Empty;
        }
        private string accessCode = string.Empty;
        private string transactionId = string.Empty;
        private string languageCode = "en-US";
        private MOBApplication application;
        private string deviceId = string.Empty;

        public string AccessCode
        {
            get
            {
                return this.accessCode;
            }
            set
            {
                this.accessCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string TransactionId
        {
            get
            {
                return this.transactionId;
            }
            set
            {
                this.transactionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string LanguageCode
        {
            get
            {
                return this.languageCode;
            }
            set
            {
                this.languageCode = string.IsNullOrEmpty(value) ? "en-US" : value.Trim();
            }
        }

        public MOBApplication Application
        {
            get
            {
                return this.application;
            }
            set
            {
                this.application = value;
            }
        }

        public string DeviceId
        {
            get
            {
                return this.deviceId;
            }
            set
            {
                this.deviceId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
