using System;
using System.Collections.Generic;
using United.Services.FlightShopping.Common;

namespace United.Definition
{
    [Serializable()]
    public class MOBCPCustomerMetrics
    {

        private string ptcCode = string.Empty;

        public string PTCCode
        {
            get { return ptcCode; }
            set { ptcCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
    }

}
