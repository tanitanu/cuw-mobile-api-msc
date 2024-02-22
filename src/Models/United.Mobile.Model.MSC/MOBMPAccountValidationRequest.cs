using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBMPAccountValidationRequest : MOBRequest
    {
        private string mileagePlusNumber = string.Empty;
        private string pinCode = string.Empty;
        private string sessionID = string.Empty;

        public MOBMPAccountValidationRequest()
            : base()
        {
        }

        public string MileagePlusNumber
        {
            get
            {
                return this.mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string PinCode
        {
            get
            {
                return this.pinCode;
            }
            set
            {
                this.pinCode = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        public string SessionID
        {
            get
            {
                return this.sessionID;
            }
            set
            {
                this.sessionID = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }
    }
}
