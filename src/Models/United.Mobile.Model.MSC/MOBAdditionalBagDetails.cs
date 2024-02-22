using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable]
    public class MOBAdditionalBagDetails
    {
        private string title = string.Empty;
        private List<MOBOverSizeWeightBagFeeDetails> bagFeeDetails;

        public string Title
        {
            get { return this.title; }
            set { this.title = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public List<MOBOverSizeWeightBagFeeDetails> BagFeeDetails
        {
            get { return this.bagFeeDetails; }
            set { this.bagFeeDetails = value; }
        }
    }
}
