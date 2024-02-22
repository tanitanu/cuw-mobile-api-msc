using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Shopping
{
    [Serializable]
   public class MOBSHOPFareMatrixItem
    {
       
        private string displayValue = string.Empty;
        private string departureDate = string.Empty;
        private string arrivalDate = string.Empty;
        private bool isSelectable = false;

        public string DepartureDate
        {
            set
            {
                this.departureDate = value;
            }
            get
            {
                return this.departureDate;
            }
        }
        public string ArrivalDate
        {
            set
            {
                this.arrivalDate = value;
            }
            get
            {
                return this.arrivalDate;
            }
        }

        public bool IsSelectable
        {
            get
            {
                return this.isSelectable;
            }
            set
            {
                this.isSelectable = value;
            }
        }

        //public string Key
        //{
        //    get
        //    {
        //        return this.key;
        //    }
        //    set
        //    {
        //        this.key = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        //    }
        //}

        //public string TripId
        //{
        //    get
        //    {
        //        return this.tripId;
        //    }
        //    set
        //    {
        //        this.tripId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        //    }
        //}

        //public string ProductId
        //{
        //    get
        //    {
        //        return this.productId;
        //    }
        //    set
        //    {
        //        this.productId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        //    }
        //}

        public string DisplayValue
        {
            get
            {
                return this.displayValue;
            }
            set
            {
                this.displayValue = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        //public string Value
        //{
        //    get
        //    {
        //        return this.value;
        //    }
        //    set
        //    {
        //        this.value = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        //    }
        //}
    }
}
