using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBMPStatementDateResponse : MOBResponse
    {
        private string mileagePlusNumber = string.Empty;
        private List<MOBMPStatementDate> statementDates;

        public MOBMPStatementDateResponse()
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

        public List<MOBMPStatementDate> StatementDates
        {
            get
            {
                return this.statementDates;
            }
            set
            {
                this.statementDates = value;
            }
        }
    }
}
