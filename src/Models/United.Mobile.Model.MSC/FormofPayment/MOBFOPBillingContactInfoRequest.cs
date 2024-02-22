using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;

namespace United.Definition
{
    [Serializable()]
    public class MOBFOPBillingContactInfoRequest : MOBShoppingRequest
    {
		private MOBEmail email;
		private MOBCPPhone phone;
		private MOBAddress billingAddress;
		
		public MOBEmail Email
		{
			get { return email; }
			set { email = value; }
		}

		public MOBCPPhone Phone
		{
			get { return phone; }
			set { phone = value; }
		}

		public MOBAddress BillingAddress
		{
			get { return billingAddress; }
			set { billingAddress = value; }
		}

	}
}
