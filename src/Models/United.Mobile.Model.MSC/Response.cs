using System;
using System.Collections.Generic;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class Response
    {
        private string transactionId = string.Empty;
        private string languageCode = string.Empty;
        private string machineName = System.Environment.MachineName;
        private MOBException exception;
        private long callDuration;

        private bool pss = false;

        private string area = string.Empty;

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

        public string MachineName
        {
            get 
            {
                return this.machineName; 
            }
            set 
            {
                this.machineName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBException Exception
        {
            get
            {
                return this.exception;
            }
            set
            {
                this.exception = value;
            }
        }

        public long CallDuration
        {
            get
            {
                return this.callDuration;
            }
            set
            {
                this.callDuration = value;
            }
        }

        public bool PSS
        {
            get
            {
                return this.pss;
            }
            set
            {
                this.pss = value;
            }
        }

        public string Area
        {
            get
            {
                return this.area;
            }
            set
            {
                this.area = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
    }
}
