using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBPIAirportResponse : MOBResponse
    {
        public MOBPIAirportResponse()
            : base()
        {
        }

        private List<MOBPIAirport> piAirports;

        public List<MOBPIAirport> PIAirports
        {
            get { return this.piAirports; }
            set
            {
                this.piAirports = value;
            }
        }
    }
}
