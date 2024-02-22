using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBReceiptByEmailResponse : MOBResponse
    {
        private string recordLocator = string.Empty;
        private string emailAdress = string.Empty;
        private string creationDate = string.Empty;
        private string message = string.Empty;

        public MOBReceiptByEmailResponse()
            : base()
        {
        }

        public string RecordLocator
        {
            get
            {
                return this.recordLocator;
            }
            set
            {
                this.recordLocator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string CreationDate
        {
            get
            {
                return this.creationDate;
            }
            set
            {
                this.creationDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string EMailAdress
        {
            get
            {
                return this.emailAdress;
            }
            set
            {
                this.emailAdress = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Message
        {
            get
            {
                return this.message;
            }
            set
            {
                this.message = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
