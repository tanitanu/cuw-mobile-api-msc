using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable]
    public class MOBCurrencyConverterResponse : MOBResponse
    {
        private MOBCurrencyConverter currencyConverter;

        public MOBCurrencyConverter CurrencyConverter
        {
            get
            {
                return this.currencyConverter;
            }
            set
            {
                this.currencyConverter = value;
            }
        }
    }
}
