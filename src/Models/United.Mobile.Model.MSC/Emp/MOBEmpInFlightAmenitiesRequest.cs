using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp
{
    [Serializable]
    public class MOBEmpInFlightAmenitiesRequest : MOBEmpRequest
    {
        private List<MOBEmpInFlightAmenitiesFlight> flights;

        public List<MOBEmpInFlightAmenitiesFlight> Flights
        {
            get { return this.flights; }
            set { this.flights = value; }
        }
    }
}
