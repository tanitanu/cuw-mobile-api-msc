using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBMPStatementResponse : MOBResponse
    {
        private MOBMPStatement statement;

        public MOBMPStatementResponse()
            : base()
        {
        }

        public MOBMPStatement Statement
        {
            get
            {
                return this.statement;
            }
            set
            {
                this.statement = value;
            }
        }
    }
}
