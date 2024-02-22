using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition.Bag
{
    [Serializable]
    public class MOBBagTag
    {
        private string bagTagNumber = string.Empty;
        private string departureAirport = string.Empty;
        private string finalArrivalAirport = string.Empty;
        private string issueDateTime = string.Empty;
        private string issueStationCode = string.Empty;
        private string typeCode = string.Empty;
        private string uniqueKeyNumber = string.Empty;

        public string BagTagNumber
        {
            get
            {
                return bagTagNumber;
            }
            set
            {
                this.bagTagNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string DepartureAirport
        {
            get
            {
                return departureAirport;
            }
            set
            {
                this.departureAirport = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string FinalArrivalAirport
        {
            get
            {
                return finalArrivalAirport;
            }
            set
            {
                this.finalArrivalAirport = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string IssueDateTime
        {
            get
            {
                return issueDateTime;
            }
            set
            {
                this.issueDateTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string IssueStationCode
        {
            get
            {
                return issueStationCode;
            }
            set
            {
                this.issueStationCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string TypeCode
        {
            get
            {
                return typeCode;
            }
            set
            {
                this.typeCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string UniqueKeyNumber
        {
            get
            {
                return uniqueKeyNumber;
            }
            set
            {
                this.uniqueKeyNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
    }
}
