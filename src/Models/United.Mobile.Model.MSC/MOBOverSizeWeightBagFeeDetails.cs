using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable]
    public class MOBOverSizeWeightBagFeeDetails
    {
        private string bagInfo = string.Empty;
        private string priceInfo = string.Empty;

        public string BagInfo
        {
            get { return this.bagInfo; }
            set { this.bagInfo = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string PriceInfo
        {
            get { return this.priceInfo; }
            set { this.priceInfo = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
    }
}
