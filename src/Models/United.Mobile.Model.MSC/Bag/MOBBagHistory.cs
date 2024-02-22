using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition.Bag
{
    [Serializable]
    public class MOBBagHistory
    {
        private MOBBagFlightSegment bagFlightSegment;
        private string statusCode = string.Empty;

        public MOBBagFlightSegment BagFlightSegment
        {
            get
            {
                return bagFlightSegment;
            }
            set
            {
                this.bagFlightSegment = value;
            }
        }

        public string StatusCode
        {
            get
            {
                return statusCode;
            }
            set
            {
                this.statusCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
