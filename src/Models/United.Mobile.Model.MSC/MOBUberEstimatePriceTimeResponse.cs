using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBUberEstimatePriceTimeResponse : MOBResponse
    {
        public MOBUberEstimatePriceTimeResponse()
            : base()
        {
        }

        private List<MOBUberPriceDetails> prices;

        public List<MOBUberPriceDetails> Prices
        {
            get { return this.prices; }
            set
            {
                this.prices = value;
            }
        }
    }

    [Serializable()]
    public class MOBUberPrice
    {
        public List<MOBUberPriceDetails> Prices { get; set; }
    }

}

