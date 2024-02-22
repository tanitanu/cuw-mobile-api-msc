using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBUberEstimateTimeResponse : MOBResponse
    {
        public MOBUberEstimateTimeResponse()
            : base()
        {
        }

        private List<MOBUberTimeDetails> times;

        public List<MOBUberTimeDetails> Times
        {
            get { return this.times; }
            set
            {
                this.times = value;
            }
        }
    }

    [Serializable()]
    public class MOBUberTime
    {
        public List<MOBUberTimeDetails> Times { get; set; }
    }

}

