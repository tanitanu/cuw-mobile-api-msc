using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPOfferPriceAssociation
    {
        private List<string> travelerRefIDs;
        private List<string> segmentRefIDs;
        private List<MOBSHOPOfferODMapping> oDMappings;

        public List<string> TravelerRefIDs
        {
            get
            {
                return this.travelerRefIDs;
            }
            set
            {
                this.travelerRefIDs = value;
            }
        }

        public List<string> SegmentRefIDs
        {
            get
            {
                return this.segmentRefIDs;
            }
            set
            {
                this.segmentRefIDs = value;
            }
        }

        public List<MOBSHOPOfferODMapping> ODMappings
        {
            get
            {
                return this.oDMappings;
            }
            set
            {
                this.oDMappings = value;
            }
        }
    }
}
