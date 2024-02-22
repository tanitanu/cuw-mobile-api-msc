using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.FormofPayment;
using United.Definition.Shopping;

namespace United.Definition
{
    [Serializable]
    public class GetMoneyPlusMilesRequest : MOBShoppingRequest
    {
		private string mileagePlusNumber;

		public string MileagePlusNumber
		{
			get { return mileagePlusNumber; }
			set { mileagePlusNumber = value; }
		}

	}

}
