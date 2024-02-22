using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MOBShopBookingInfo
    {
        private string bookingCode = string.Empty;
        private decimal cocTotal;
        private string extendedFareCode = string.Empty;
        private string fareBasisCode = string.Empty;
        private string fareIndex = string.Empty;
        private string fareInfoHash = string.Empty;
        private string paxIds = string.Empty;
        private string paxPricingIndex = string.Empty;
        private decimal saleFareTotal;
        private decimal saleTaxTotal;
        private int segmentindex;
        private string sliceIndex = string.Empty;
        private string ticketDesignator = string.Empty;

        public string BookingCode
        {
            get
            {
                return this.bookingCode;
            }
            set
            {
                this.bookingCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public decimal COCTotal
        {
            get
            {
                return this.cocTotal;
            }
            set
            {
                this.cocTotal = value;
            }
        }

        public string ExtendedFareCode
        {
            get
            {
                return this.extendedFareCode;
            }
            set
            {
                this.extendedFareCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FareBasisCode
        {
            get
            {
                return this.fareBasisCode;
            }
            set
            {
                this.fareBasisCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FareIndex
        {
            get
            {
                return this.fareIndex;
            }
            set
            {
                this.fareIndex = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FareInfoHash
        {
            get
            {
                return this.fareInfoHash;
            }
            set
            {
                this.fareInfoHash = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string PaxIds
        {
            get
            {
                return this.paxIds;
            }
            set
            {
                this.paxIds = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string PaxPricingIndex
        {
            get
            {
                return this.paxPricingIndex;
            }
            set
            {
                this.paxPricingIndex = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public decimal SaleFareTotal
        {
            get
            {
                return this.saleFareTotal;
            }
            set
            {
                this.saleFareTotal = value;
            }
        }

        public decimal SaleTaxTotal
        {
            get
            {
                return this.saleTaxTotal;
            }
            set
            {
                this.saleTaxTotal = value;
            }
        }

        public int Segmentindex
        {
            get
            {
                return this.segmentindex;
            }
            set
            {
                this.segmentindex = value;
            }
        }

        public string SliceIndex
        {
            get
            {
                return this.sliceIndex;
            }
            set
            {
                this.sliceIndex = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string TicketDesignator
        {
            get
            {
                return this.ticketDesignator;
            }
            set
            {
                this.ticketDesignator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
