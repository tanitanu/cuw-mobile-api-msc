using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBReceiptByEmailRequest : MOBRequest
    {
        private string recordLocator = string.Empty;
        private string creationDate = string.Empty;
        private string emailAdress = string.Empty;

        public MOBReceiptByEmailRequest()
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
    }
}
