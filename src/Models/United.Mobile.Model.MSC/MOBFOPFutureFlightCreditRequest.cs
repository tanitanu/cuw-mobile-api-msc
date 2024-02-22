using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.FormofPayment;

namespace United.Definition
{
    [Serializable]
    public class MOBFOPFutureFlightCreditRequest : MOBShoppingRequest
	{
        private MOBFOPFutureFlightCredit futureFlightCredit;

        public MOBFOPFutureFlightCredit FutureFlightCredit
        {
            get { return futureFlightCredit; }
            set { futureFlightCredit = value; }
        }
    

    }



}
