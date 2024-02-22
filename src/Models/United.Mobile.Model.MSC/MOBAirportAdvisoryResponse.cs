using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable]
    public class MOBAirportAdvisoryResponse : MOBResponse
    {
        private MOBAirportAdvisoryMessage airportAdvisoryMessage;


        public MOBAirportAdvisoryResponse()
            : base()
        {
        }


        public MOBAirportAdvisoryMessage AirportAdvisoryMessage
        {
            get
            {
                return this.airportAdvisoryMessage;
            }
            set
            {
                this.airportAdvisoryMessage = value;
            }
        }
    }
}
