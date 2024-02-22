using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBMPStatementDateRequest : MOBRequest
    {
        private string mileagePlusNumber = string.Empty;

        public MOBMPStatementDateRequest()
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
    }
}
