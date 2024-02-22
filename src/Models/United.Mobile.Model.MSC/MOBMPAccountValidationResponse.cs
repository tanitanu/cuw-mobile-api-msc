using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBMPAccountValidationResponse : MOBResponse
    {
        private MOBMPAccountValidation accountValidation;

        public MOBMPAccountValidationResponse()
            : base()
        {
        }

        public MOBMPAccountValidation AccountValidation 
        { 
            get
            {
                return this.accountValidation;
            }
            set
            {
                this.accountValidation = value;
            }
        }
    }
}
