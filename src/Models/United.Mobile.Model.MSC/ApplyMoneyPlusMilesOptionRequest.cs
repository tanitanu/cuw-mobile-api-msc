using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
	[Serializable]
	public class ApplyMoneyPlusMilesOptionRequest : MOBShoppingRequest
    {
		private string optionId;
		public string OptionId
		{
			get { return optionId; }
			set { optionId = value; }
		}

		private string mileagePlusNumber;

		public string MileagePlusNumber
		{
			get { return mileagePlusNumber; }
			set { mileagePlusNumber = value; }
		}

		private bool isRTI;

		public bool IsRTI
		{
			get { return isRTI; }
			set { isRTI = value; }
		}
	}
}
