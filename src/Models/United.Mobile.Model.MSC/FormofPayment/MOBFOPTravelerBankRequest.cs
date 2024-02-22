using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;

namespace United.Definition
{
    [Serializable()]
    public class MOBFOPTravelerBankRequest : MOBShoppingRequest
    {

		private string displayAmount ;
		private double appliedAmount;//ewmocvw ti
		private bool isRemove;

		

		public bool IsRemove
		{
			get { return isRemove; }
			set { isRemove = value; }
		}

		public string DisplayAmount
		{
			get { return displayAmount; }
			set { displayAmount = value; }
		}

		public double AppliedAmount
		{
			get { return appliedAmount; }
			set { appliedAmount = value; }
		}
	}
}
