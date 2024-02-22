using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Emp.Common;

namespace United.Definition.Emp
{
    [Serializable()]
    public class MOBEmpCreditCardRequest : MOBEmpRequest
    {
        private MOBEmpCreditCard mobEmpCreditCard;

        public MOBEmpCreditCard MOBEmpCreditCard
        {
            get { return this.mobEmpCreditCard; }
            set { this.mobEmpCreditCard = value; }
        }
    }
}
