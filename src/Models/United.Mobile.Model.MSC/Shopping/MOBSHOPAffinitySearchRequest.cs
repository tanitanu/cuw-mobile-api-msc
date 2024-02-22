using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBSHOPAffinitySearchRequest : MOBRequest
    {
        private string origin = string.Empty;
        private string destination = string.Empty;
        private string pointOfSale = string.Empty;
        private string minDepartDate = string.Empty;
        private string maxDepartDate = string.Empty;
        private string minReturnDate = string.Empty;
        private string maxReturnDate = string.Empty;
        private int tripDurationDays = -1;
        private bool isOneWay = false;

        public string Origin
        {
            get
            {
                return this.origin;
            }
            set
            {
                this.origin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string Destination
        {
            get
            {
                return this.destination;
            }
            set
            {
                this.destination = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string PointOfSale
        {
            get
            {
                return this.pointOfSale;
            }
            set
            {
                this.pointOfSale = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string MinDepartDate
        {
            get
            {
                return this.minDepartDate;
            }
            set
            {
                this.minDepartDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string MaxDepartDate
        {
            get
            {
                return this.maxDepartDate;
            }
            set
            {
                this.maxDepartDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string MinReturnDate
        {
            get
            {
                return this.minReturnDate;
            }
            set
            {
                this.minReturnDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string MaxReturnDate
        {
            get
            {
                return this.maxReturnDate;
            }
            set
            {
                this.maxReturnDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public int TripDurationDays
        {
            get
            {
                return this.tripDurationDays;
            }
            set
            {
                this.tripDurationDays = value < 0 ? -1 : value;
            }
        }

        public bool IsOneWay
        {
            get
            {
                return this.isOneWay;
            }
            set
            {
                this.isOneWay = value;
            }
        }
    }
}
