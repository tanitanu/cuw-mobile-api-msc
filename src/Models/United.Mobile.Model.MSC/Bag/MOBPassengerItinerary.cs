using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Bag
{
    [Serializable]
    public class MOBPassengerItinerary
    {
        private string confirmationNumber = string.Empty;
        private List<MOBSegment> segments;

        public string ConfirmationNumber
        {
            get
            {
                return confirmationNumber;
            }
            set
            {
                this.confirmationNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public List<MOBSegment> Segments
        {
            get
            {
                return this.segments;
            }
            set
            {
                this.segments = value;
            }
        }
    }
}
